Imports System.Globalization

Public Class PSE

    Public Sub New()
        listOfElemente.Clear()
        listOfHauptgruppenElemente.Clear()

        Dim myAtom As Atom
        For i As Integer = 1 To 118
            myAtom = New Atom(i)
            listOfElemente.Add(myAtom)
            Select Case myAtom.Gruppe
                Case 1, 2, 13, 14, 15, 16, 17
                    listOfHauptgruppenElemente.Add(myAtom)
                Case 3 To 12
                    listOfNebenGruppenElemente.Add(myAtom)
            End Select
            If Not myAtom.Radioaktiv Then
                Select Case myAtom.Gruppe
                    Case 1, 2, 13
                        If myAtom.OrdnungsZahl > 1 Then listOfHGKationenBildner.Add(myAtom)
                    Case 15, 16, 17
                        If myAtom.Elektronegativität > 2 Then
                            listOfHGAnionenBildner.Add(myAtom)
                        End If
                End Select
                Select Case myAtom.Symbol
                    Case "C", "Si", "Ge"
                        listOfHGAnionenBildner.Add(myAtom)
                    Case "Sn", "Pb"
                        listOfHGKationenBildner.Add(myAtom)
                End Select
            End If
        Next
    End Sub

    Private listOfElemente As New List(Of Atom)
    Public Property lElemente() As List(Of Atom)
        Get
            Return listOfElemente
        End Get
        Set(ByVal value As List(Of Atom))
            listOfElemente = value
        End Set
    End Property

    Private listOfHauptgruppenElemente As New List(Of Atom)
    Public Property lHauptGruppenElemente() As List(Of Atom)
        Get
            Return listOfHauptgruppenElemente
        End Get
        Set(ByVal value As List(Of Atom))
            listOfHauptgruppenElemente = value
        End Set
    End Property

    Private listOfNebenGruppenElemente As New List(Of Atom)
    Public Property lNebenGruppenElemente() As List(Of Atom)
        Get
            Return listOfNebenGruppenElemente
        End Get
        Set(ByVal value As List(Of Atom))
            listOfNebenGruppenElemente = value
        End Set
    End Property

    Private listOfHGKationenBildner As New List(Of Atom)
    Public Property lHauptgruppeKationenBildner() As List(Of Atom)
        Get
            Return listOfHGKationenBildner
        End Get
        Set(ByVal value As List(Of Atom))
            listOfHGKationenBildner = value
        End Set
    End Property

    Private listOfHGAnionenBildner As New List(Of Atom)
    Public Property lHauptgruppeAnionenBildner() As List(Of Atom)
        Get
            Return listOfHGAnionenBildner
        End Get
        Set(ByVal value As List(Of Atom))
            listOfHGAnionenBildner = value
        End Set
    End Property


End Class
