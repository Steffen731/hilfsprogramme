Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text

Namespace Chemistry
    Public Enum AlkaneFormulaKind
        Halbstruktur
        Ausfuehrlich
    End Enum

    Public NotInheritable Class ChemfigRenderOptions
        Public Property UseSubmolecules As Boolean = True
        Public Property IncludeComments As Boolean = True
        Public Property MainBondLength As Decimal = 2.2D
        Public Property BranchBondLength As Decimal = 1.6D
        Public Property HydrogenBondLength As Decimal = 0.7D

        Public Sub Validate()
            For Each value As Decimal In {MainBondLength, BranchBondLength, HydrogenBondLength}
                If value <= 0D OrElse value > 10D Then
                    Throw New ArgumentOutOfRangeException("Bindungslaengen muessen zwischen 0 und 10 liegen.")
                End If
            Next
        End Sub
    End Class

    ''' <summary>
    ''' Deterministischer chemfig-Renderer auf Basis des C-Atomgraphen.
    ''' Ausfuehrlich = jedes C und H sowie jede C-C-/C-H-Bindung explizit.
    ''' IUPAC-Benennung und Parsing von Stoffnamen sind bewusst NICHT Teil dieses Renderers.
    ''' </summary>
    Public NotInheritable Class ChemfigRenderer
        Private Sub New()
        End Sub

        Public Shared Function Render(molecule As AlkaneMolecule,
                                      kind As AlkaneFormulaKind,
                                      Optional options As ChemfigRenderOptions = Nothing) As String
            If molecule Is Nothing Then Throw New ArgumentNullException(NameOf(molecule))
            If Not [Enum].IsDefined(GetType(AlkaneFormulaKind), kind) Then
                Throw New ArgumentOutOfRangeException(NameOf(kind))
            End If
            If options Is Nothing Then options = New ChemfigRenderOptions()
            options.Validate()

            Dim isFull As Boolean = (kind = AlkaneFormulaKind.Ausfuehrlich)
            Dim sb As New StringBuilder()
            If options.IncludeComments Then
                sb.AppendLine($"% Hauptkette (Zeichenachse): {molecule.MainChainLength} C-Atome, von links nach rechts")
                Dim details As String = String.Join(", ", molecule.Branches.Select(
                    Function(b) $"{b.CarbonCount} C an C{b.Position}"))
                sb.AppendLine("% Seitenketten: " & If(details.Length = 0, "keine", details))
                sb.AppendLine($"% Summenformel: {molecule.MolecularFormula} (noch kein IUPAC-gepruefter Name)")
            End If

            Dim useMacros As Boolean = options.UseSubmolecules AndAlso molecule.Branches.Count > 0
            If useMacros Then
                sb.AppendLine("\begingroup")
                If isFull Then
                    If AnyBranchInDirection(molecule, 90) Then
                        sb.AppendLine("\definesubmol{AKMeUp}{" & ExplicitBranchTerminal(90, options) & "}")
                    End If
                    If AnyBranchInDirection(molecule, 270) Then
                        sb.AppendLine("\definesubmol{AKMeDown}{" & ExplicitBranchTerminal(270, options) & "}")
                    End If
                Else
                    sb.AppendLine("\definesubmol{AKMe}{CH_3}")
                End If
            End If

            sb.AppendLine("\chemfig{")
            For position As Integer = 1 To molecule.MainChainLength
                Dim branchesAtPosition As AlkaneBranch() = molecule.Branches.
                    Where(Function(b) b.Position = position).ToArray()
                Dim line As New StringBuilder("  ")
                If position > 1 Then line.Append(Bond(0, options.MainBondLength))
                If isFull Then
                    line.Append("C")
                    If position = 1 Then line.Append(BondInParentheses(180, options.HydrogenBondLength, "H"))
                    If position = molecule.MainChainLength Then
                        line.Append(BondInParentheses(0, options.HydrogenBondLength, "H"))
                    End If
                Else
                    Dim h As Integer = molecule.HydrogenCountAtMainPosition(position)
                    line.Append(HalfLabel(h, position = 1, useMacros))
                End If

                Dim usedDirections As New HashSet(Of Integer)()
                For index As Integer = 0 To branchesAtPosition.Length - 1
                    Dim angle As Integer = BranchDirection(position, index, branchesAtPosition.Length)
                    usedDirections.Add(angle)
                    line.Append("(")
                    line.Append(RenderBranch(branchesAtPosition(index), angle, kind, options, useMacros))
                    line.Append(")")
                Next
                If isFull Then
                    For Each angle As Integer In {90, 270}
                        If Not usedDirections.Contains(angle) Then
                            line.Append(BondInParentheses(angle, options.HydrogenBondLength, "H"))
                        End If
                    Next
                End If
                If options.IncludeComments Then
                    line.Append($"% C{position}")
                Else
                    line.Append("%")
                End If
                sb.AppendLine(line.ToString())
            Next
            sb.AppendLine("}")
            If useMacros Then sb.AppendLine("\endgroup")
            Return sb.ToString().TrimEnd()
        End Function

        Private Shared Function AnyBranchInDirection(molecule As AlkaneMolecule, angle As Integer) As Boolean
            For position As Integer = 1 To molecule.MainChainLength
                Dim count As Integer = molecule.Branches.Count(Function(b) b.Position = position)
                For index As Integer = 0 To count - 1
                    If BranchDirection(position, index, count) = angle Then Return True
                Next
            Next
            Return False
        End Function

        Private Shared Function BranchDirection(position As Integer, branchIndex As Integer, count As Integer) As Integer
            If count = 2 Then Return If(branchIndex = 0, 90, 270)
            Return If(position Mod 2 = 0, 90, 270)
        End Function

        Private Shared Function RenderBranch(branch As AlkaneBranch,
                                             angle As Integer,
                                             kind As AlkaneFormulaKind,
                                             options As ChemfigRenderOptions,
                                             useMacros As Boolean) As String
            Dim sb As New StringBuilder()
            For i As Integer = 1 To branch.CarbonCount
                sb.Append(Bond(angle, options.BranchBondLength))
                Dim last As Boolean = (i = branch.CarbonCount)
                If kind = AlkaneFormulaKind.Halbstruktur Then
                    sb.Append(If(last, If(useMacros, "!{AKMe}", "CH_3"), "CH_2"))
                ElseIf last AndAlso useMacros Then
                    sb.Append(If(angle = 90, "!{AKMeUp}", "!{AKMeDown}"))
                Else
                    sb.Append("C")
                    sb.Append(BondInParentheses(0, options.HydrogenBondLength, "H"))
                    sb.Append(BondInParentheses(180, options.HydrogenBondLength, "H"))
                    If last Then sb.Append(BondInParentheses(angle, options.HydrogenBondLength, "H"))
                End If
            Next
            Return sb.ToString()
        End Function

        Private Shared Function ExplicitBranchTerminal(angle As Integer, options As ChemfigRenderOptions) As String
            Return "C" & BondInParentheses(0, options.HydrogenBondLength, "H") &
                BondInParentheses(180, options.HydrogenBondLength, "H") &
                BondInParentheses(angle, options.HydrogenBondLength, "H")
        End Function

        Private Shared Function HalfLabel(hydrogens As Integer, leftEnd As Boolean, macros As Boolean) As String
            Select Case hydrogens
                Case 0
                    Return "C"
                Case 1
                    Return "CH"
                Case 2
                    Return "CH_2"
                Case 3
                    If leftEnd Then Return "H_3C"
                    Return If(macros, "!{AKMe}", "CH_3")
                Case 4
                    Return "CH_4"
                Case Else
                    Throw New InvalidOperationException("Unerwartete Kohlenstoffvalenz.")
            End Select
        End Function

        Private Shared Function Bond(angle As Integer, length As Decimal) As String
            Return "-[:" & angle.ToString(CultureInfo.InvariantCulture) & "," &
                length.ToString("0.###", CultureInfo.InvariantCulture) & "]"
        End Function

        Private Shared Function BondInParentheses(angle As Integer,
                                                  length As Decimal,
                                                  label As String) As String
            Return "(" & Bond(angle, length) & label & ")"
        End Function
    End Class
End Namespace
