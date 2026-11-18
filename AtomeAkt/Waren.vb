Imports Microsoft.VisualBasic

Public Class Waren

    Pos.	Type, Description	Quantity	Unit	Price EUR	Total EUR

    Private _PosNr As Integer
    Public Property PosNr() As Integer
        Get
            Return _PosNr
        End Get
        Set(ByVal value As Integer)
            _PosNr = value
        End Set
    End Property

    Private _Type As String
    Public Property Produkttyp() As String
        Get
            Return _Type
        End Get
        Set(ByVal value As String)
            _Type = value
        End Set
    End Property

    Private _soll As Integer
    Public Property Soll-Menge() As Integer
        Get
            Return _soll
        End Get
        Set(ByVal value As Integer)
            _soll = value
        End Set
    End Property

    Private _Einheit As String
    Public Property Einheit() As String
        Get
            Return _Einheit
        End Get
        Set(ByVal value As String)
            _Einheit = value
        End Set
    End Property

    Private _VKPreis As Double
    Public Property VKPreis() As Double
        Get
            Return _VKPreis
        End Get
        Set(ByVal value As Double)
            _VKPreis = value
        End Set
    End Property

    Public ReadOnly Property VKPosPreis() As Double
        Get
            Return _soll * _VKPreis
        End Get
    End Property

    Private _fiLief As Integer
    Public Property fiLieferant() As Integer
        Get
            Return _fiLief
        End Get
        Set(ByVal value As Integer)
            _fiLief = value
        End Set
    End Property

    Private _fiAuftrag As Integer
    Public Property fiAuftrag() As Integer
        Get
            Return _fiAuftrag
        End Get
        Set(ByVal value As Integer)
            _fiAuftrag = value
        End Set
    End Property

End Class
