Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports AtomeAkt.Chemistry

Friend NotInheritable Class FormulaExample
    Public ReadOnly Property Caption As String
    Public ReadOnly Property MainLength As Integer
    Public ReadOnly Property Branches As AlkaneBranch()

    Public Sub New(caption As String, mainLength As Integer, ParamArray branches() As AlkaneBranch)
        Me.Caption = caption
        Me.MainLength = mainLength
        Me.Branches = branches
    End Sub
End Class

Module Program
    Sub Main(args As String())
        Dim cases As New List(Of FormulaExample) From {
            New FormulaExample("Butan", 4),
            New FormulaExample("2-Methylbutan", 4, New AlkaneBranch(2, 1)),
            New FormulaExample("2,2-Dimethylpropan", 3, New AlkaneBranch(2, 1), New AlkaneBranch(2, 1)),
            New FormulaExample("2,3-Dimethylbutan", 4, New AlkaneBranch(2, 1), New AlkaneBranch(3, 1)),
            New FormulaExample("2,2,4-Trimethylpentan", 5,
                New AlkaneBranch(2, 1), New AlkaneBranch(2, 1), New AlkaneBranch(4, 1)),
            New FormulaExample("3-Ethyl-2-methylhexan", 6,
                New AlkaneBranch(2, 1), New AlkaneBranch(3, 2)),
            New FormulaExample("3,3-Diethylpentan", 5,
                New AlkaneBranch(3, 2), New AlkaneBranch(3, 2))
        }

        Dim tex As New StringBuilder()
        tex.AppendLine("\documentclass[a4paper,11pt]{article}")
        tex.AppendLine("\usepackage[margin=1.5cm]{geometry}")
        tex.AppendLine("\usepackage{chemfig}")
        tex.AppendLine("\setchemfig{atom sep=2em}")
        tex.AppendLine("\begin{document}")

        For Each example As FormulaExample In cases
            Dim molecule As New AlkaneMolecule(example.MainLength, example.Branches)
            Dim expectedCarbons As Integer = example.MainLength
            For Each branch As AlkaneBranch In example.Branches
                expectedCarbons += branch.CarbonCount
            Next
            Require(molecule.CarbonCount = expectedCarbons, "Falsche Anzahl C: " & example.Caption)
            Require(molecule.HydrogenCount = 2 * expectedCarbons + 2, "Falsche Anzahl H: " & example.Caption)
            Require(molecule.MolecularFormula = $"C{expectedCarbons}H{2 * expectedCarbons + 2}",
                    "Falsche Summenformel: " & example.Caption)
            For c As Integer = 0 To molecule.CarbonCount - 1
                Require(molecule.CarbonNeighbors(c).Count + molecule.HydrogenCountAtCarbon(c) = 4,
                        "Ungueltige C-Valenz: " & example.Caption)
            Next

            ' Ohne Submols/Kommentare muss die Vollstruktur explizit alle C/H-Atome enthalten.
            Dim verifyOptions As New ChemfigRenderOptions With {
                .UseSubmolecules = False,
                .IncludeComments = False
            }
            Dim explicitFull As String = ChemfigRenderer.Render(
                molecule, AlkaneFormulaKind.Ausfuehrlich, verifyOptions)
            Dim writtenCarbonAtoms As Integer = Regex.Matches(explicitFull, "C(?=\(|-\[)").Count
            Dim writtenHydrogenAtoms As Integer = Regex.Matches(explicitFull, "H\)").Count
            Require(writtenCarbonAtoms = molecule.CarbonCount,
                    "Vollstruktur enthaelt nicht alle C-Atome: " & example.Caption)
            Require(writtenHydrogenAtoms = molecule.HydrogenCount,
                    "Vollstruktur enthaelt nicht alle H-Atome: " & example.Caption)

            For Each kind As AlkaneFormulaKind In {AlkaneFormulaKind.Halbstruktur, AlkaneFormulaKind.Ausfuehrlich}
                Dim fragment As String = ChemfigRenderer.Render(molecule, kind)
                Require(fragment = ChemfigRenderer.Render(molecule, kind), "Nichtdeterministische Ausgabe.")
                Require(fragment.Contains("\chemfig{"), "Fehlender chemfig-Befehl.")
                Require(Not fragment.Contains("\mathrm"), "Unerwuenschter mathrm-Code.")
                Require(fragment.Contains("% C"), "Bearbeitungskommentare fehlen.")
                Dim suffix As String = If(kind = AlkaneFormulaKind.Ausfuehrlich,
                    " (ausfuehrlich)", " (halbstruktur)")
                tex.AppendLine("\section*{" & example.Caption & suffix & "}")
                tex.AppendLine(fragment)
                tex.AppendLine("\par\medskip")
            Next
        Next
        tex.AppendLine("\end{document}")

        CheckInvalidInput()
        Dim outputFolder As String = If(args.Length > 0, args(0), Environment.CurrentDirectory)
        Directory.CreateDirectory(outputFolder)
        Dim texFile As String = Path.Combine(outputFolder, "chemfig-referenz.tex")
        File.WriteAllText(texFile, tex.ToString(), New UTF8Encoding(False))
        Console.WriteLine($"OK: {cases.Count} Referenzmolekuele, 14 chemfig-Ausgaben: {texFile}")
        Console.WriteLine("Jetzt pdflatex -halt-on-error chemfig-referenz.tex aufrufen und PDF visuell kontrollieren.")
    End Sub

    Private Sub CheckInvalidInput()
        Dim rejected As Boolean = False
        Try
            Dim invalid As New AlkaneMolecule(4, New AlkaneBranch(1, 1))
        Catch ex As ArgumentException
            rejected = True
        End Try
        Require(rejected, "Endstaendige Seitenkette muss abgewiesen werden.")

        rejected = False
        Try
            Dim invalid As New AlkaneMolecule(4,
                New AlkaneBranch(2, 1), New AlkaneBranch(2, 1), New AlkaneBranch(2, 1))
        Catch ex As ArgumentException
            rejected = True
        End Try
        Require(rejected, "C-Atom mit fuenf C-C-Bindungen muss abgewiesen werden.")
    End Sub

    Private Sub Require(condition As Boolean, message As String)
        If Not condition Then Throw New InvalidOperationException(message)
    End Sub
End Module
