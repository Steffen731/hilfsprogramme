Public Class Schüler

    Sub New()

    End Sub

    Sub New(sCSVArray As Array)
        sName = sCSVArray(1)
        sVorname = sCSVArray(2)
        sKlasse = sCSVArray(3)
        If sCSVArray(4) = "A" Then
            eGruppe = Gruppe.A
        Else
            eGruppe = Gruppe.B
        End If
    End Sub

    Private eGruppe As Gruppe
    Public Property InGruppe() As Gruppe
        Get
            Return eGruppe
        End Get
        Set(ByVal value As Gruppe)
            eGruppe = value
        End Set
    End Property


    Private sName As String
    Public Property Name() As String
        Get
            Return sName
        End Get
        Set(ByVal value As String)
            sName = value
        End Set
    End Property

    Private sVorname As String
    Public Property Vorname() As String
        Get
            Return sVorname
        End Get
        Set(ByVal value As String)
            sVorname = value
        End Set
    End Property

    Private sKlasse As String
    Public Property Klasse() As String
        Get
            Return sKlasse
        End Get
        Set(ByVal value As String)
            sKlasse = value
        End Set
    End Property

    Enum Gruppe
        A
        B
    End Enum

End Class
