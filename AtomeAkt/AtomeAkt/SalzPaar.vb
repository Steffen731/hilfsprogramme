Public Class SalzPaar

    Sub New()

    End Sub

    Sub New(theSchwierigkeit As Integer, theAnion As Atom, theKation As Atom)
        iSchwierigkeit = theSchwierigkeit
        oAnion = theAnion
        oKation = theKation
    End Sub

    Private sLat As String
    Public ReadOnly Property LatexString() As String
        Get
            If Module1.GenerateRandomNumber(1, 100) < 51 Then
                sLat = oKation.Symbol & ", " & oAnion.Symbol & "&" & oKation.Elektronegativität & " & " & oAnion.Elektronegativität
            Else
                sLat = oAnion.Symbol & ", " & oKation.Symbol & "&" & oAnion.Elektronegativität & " & " & oKation.Elektronegativität
            End If
            sLat += "&&&\\\hline" & vbCrLf
            Return sLat
        End Get
    End Property

    Private _sLatSalz As String
    Public ReadOnly Property LatexSummenFormelSalz() As String
        Get
            Dim i, j, k As Integer
            i = oKation.Valenz
            j = 8 - oAnion.Valenz
            k = ggT(i, j)
            i = i / k
            j = j / k
            Return "\ce{" & oKation.Symbol & IIf(j = 1, "", j.ToString) & oAnion.Symbol & IIf(i = 1, "", i.ToString) & "}"
        End Get
    End Property

    Public Function ggT(ByVal a As Integer, ByVal b As Integer) As Integer
        While b > 0
            ggT = a Mod b
            a = b
            b = ggT
        End While

        Return a
    End Function

    Private iSchwierigkeit As Integer
    Public Property Schwierigkeit() As Integer
        Get
            Return iSchwierigkeit
        End Get
        Set(ByVal value As Integer)
            iSchwierigkeit = value
        End Set
    End Property

    Private oKation As Atom
    Public Property Kation() As Atom
        Get
            Return oKation
        End Get
        Set(ByVal value As Atom)
            oKation = value
        End Set
    End Property

    Private oAnion As Atom
    Public Property Anion() As Atom
        Get
            Return oAnion
        End Get
        Set(ByVal value As Atom)
            oAnion = value
        End Set
    End Property

End Class
