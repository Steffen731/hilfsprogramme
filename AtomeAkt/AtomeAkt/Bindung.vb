Public Class Bindung

    Sub New()

    End Sub

    Sub New(eEins As Elektron, eZwei As Elektron)
        _ElektronEins = eEins
        _ElektronZwei = eZwei
    End Sub

    Private _ElektronEins As Elektron
    Public Property ElektronEins() As Elektron
        Get
            Return _ElektronEins
        End Get
        Set(ByVal value As Elektron)
            _ElektronEins = value
        End Set
    End Property

    Private _ElektronZwei As Elektron
    Public Property ElektronZwei() As Elektron
        Get
            Return _ElektronZwei
        End Get
        Set(ByVal value As Elektron)
            _ElektronZwei = value
        End Set
    End Property



End Class
