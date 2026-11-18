Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq

Namespace Chemistry
    ''' <summary>Lineare Seitenkette, an einem nummerierten C-Atom der Zeichen-Hauptkette.</summary>
    Public NotInheritable Class AlkaneBranch
        Public ReadOnly Property Position As Integer
        Public ReadOnly Property CarbonCount As Integer

        Public Sub New(position As Integer, carbonCount As Integer)
            If carbonCount < 1 Then Throw New ArgumentOutOfRangeException(NameOf(carbonCount))
            Me.Position = position
            Me.CarbonCount = carbonCount
        End Sub
    End Class

    ''' <summary>
    ''' Validierter, gesaettigter, azyklischer C-Atomgraph. H-Zahlen ergeben sich aus der Valenz.
    ''' Die Zeichen-Hauptkette ist noch keine IUPAC-gepruefte Hauptkette.
    ''' </summary>
    Public NotInheritable Class AlkaneMolecule
        Private ReadOnly _adjacency As List(Of List(Of Integer))
        Private ReadOnly _branches As ReadOnlyCollection(Of AlkaneBranch)

        Public ReadOnly Property MainChainLength As Integer
        Public ReadOnly Property Branches As IReadOnlyList(Of AlkaneBranch)
            Get
                Return _branches
            End Get
        End Property
        Public ReadOnly Property CarbonCount As Integer
            Get
                Return _adjacency.Count
            End Get
        End Property
        Public ReadOnly Property HydrogenCount As Integer
            Get
                Return _adjacency.Sum(Function(neighbors) 4 - neighbors.Count)
            End Get
        End Property
        Public ReadOnly Property MolecularFormula As String
            Get
                Return $"C{CarbonCount}H{HydrogenCount}"
            End Get
        End Property

        Public Sub New(mainChainLength As Integer, ParamArray branches() As AlkaneBranch)
            If mainChainLength < 1 OrElse mainChainLength > 100 Then
                Throw New ArgumentOutOfRangeException(NameOf(mainChainLength),
                    "Zwischen 1 und 100 C-Atome in der Zeichen-Hauptkette.")
            End If
            If branches Is Nothing Then Throw New ArgumentNullException(NameOf(branches))
            If branches.Any(Function(b) b Is Nothing) Then
                Throw New ArgumentException("Eine Seitenkette darf nicht Nothing sein.", NameOf(branches))
            End If
            Dim total As Integer = mainChainLength + branches.Sum(Function(b) b.CarbonCount)
            If total > 100 Then Throw New ArgumentOutOfRangeException(NameOf(branches),
                "Hoechstens 100 C-Atome insgesamt.")

            Me.MainChainLength = mainChainLength
            _adjacency = New List(Of List(Of Integer))()
            For i As Integer = 1 To mainChainLength
                AddCarbon()
            Next
            For i As Integer = 0 To mainChainLength - 2
                AddBond(i, i + 1)
            Next

            Dim ordered As AlkaneBranch() = branches.OrderBy(Function(b) b.Position).
                ThenByDescending(Function(b) b.CarbonCount).ToArray()
            For Each branch As AlkaneBranch In ordered
                If branch.Position <= 1 OrElse branch.Position >= mainChainLength Then
                    Throw New ArgumentOutOfRangeException(NameOf(branches),
                        "Seitenketten muessen an inneren Hauptketten-Atomen ansetzen.")
                End If
                Dim parent As Integer = branch.Position - 1
                For k As Integer = 1 To branch.CarbonCount
                    Dim child As Integer = AddCarbon()
                    AddBond(parent, child)
                    parent = child
                Next
            Next
            _branches = Array.AsReadOnly(ordered)
            ValidateGraph()
        End Sub

        Private Function AddCarbon() As Integer
            Dim id As Integer = _adjacency.Count
            _adjacency.Add(New List(Of Integer)())
            Return id
        End Function

        Private Sub AddBond(a As Integer, b As Integer)
            If a = b OrElse _adjacency(a).Contains(b) Then
                Throw New InvalidOperationException("Doppelte Bindung oder Selbstbindung.")
            End If
            _adjacency(a).Add(b)
            _adjacency(b).Add(a)
            If _adjacency(a).Count > 4 OrElse _adjacency(b).Count > 4 Then
                Throw New ArgumentException("Ein Kohlenstoffatom haette mehr als vier C-C-Bindungen.")
            End If
        End Sub

        Public Function CarbonNeighbors(id As Integer) As IReadOnlyList(Of Integer)
            If id < 0 OrElse id >= CarbonCount Then Throw New ArgumentOutOfRangeException(NameOf(id))
            Return _adjacency(id).AsReadOnly()
        End Function

        Public Function HydrogenCountAtCarbon(id As Integer) As Integer
            Return 4 - CarbonNeighbors(id).Count
        End Function

        Public Function HydrogenCountAtMainPosition(position As Integer) As Integer
            If position < 1 OrElse position > MainChainLength Then
                Throw New ArgumentOutOfRangeException(NameOf(position))
            End If
            Return HydrogenCountAtCarbon(position - 1)
        End Function

        Private Sub ValidateGraph()
            Dim edgeCount As Integer = _adjacency.Sum(Function(x) x.Count) \ 2
            If edgeCount <> CarbonCount - 1 Then Throw New InvalidOperationException("C-Atomgraph ist nicht azyklisch.")
            Dim seen As New HashSet(Of Integer)()
            Dim queue As New Queue(Of Integer)()
            queue.Enqueue(0)
            seen.Add(0)
            While queue.Count > 0
                For Each adjacent As Integer In _adjacency(queue.Dequeue())
                    If seen.Add(adjacent) Then queue.Enqueue(adjacent)
                Next
            End While
            If seen.Count <> CarbonCount Then Throw New InvalidOperationException("C-Atomgraph ist nicht zusammenhaengend.")
            If _adjacency.Any(Function(x) x.Count > 4 OrElse x.Count < 0) Then
                Throw New InvalidOperationException("Ungueltige C-Valenz.")
            End If
            If HydrogenCount <> 2 * CarbonCount + 2 Then
                Throw New InvalidOperationException("Ungueltige Summenformel fuer ein acyclisches Alkan.")
            End If
        End Sub
    End Class
End Namespace
