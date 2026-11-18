
Imports System.Globalization

Public Class Teilchen

    Public Sub New()

    End Sub

    Public Sub New(eTyp As Typ, Optional dXPos As Double = 0, Optional dYPos As Double = 0)
        XPos = dXPos
        YPos = dYPos
        TeilchenTyp = eTyp
        Select Case eTeilchenTyp
            Case Typ.Elektron
                sName = "Elektron"
                dMasse = 0.0005485
                iLadung = -1
                bolColored = True
                sColor = "blue"
            Case Typ.Proton
                sName = "Proton"
                dMasse = 1.0072276
                iLadung = 1
                bolColored = True
                sColor = "red"
            Case Typ.Neutron
                sName = "Neutron"
                dMasse = 1.0086649
                iLadung = 0
                bolColored = True
                sColor = "yellow"
        End Select
    End Sub


    Private eTeilchenTyp As Typ
    Public Property TeilchenTyp() As Typ
        Get
            Return eTeilchenTyp
        End Get
        Set(ByVal value As Typ)
            eTeilchenTyp = value
            Select Case eTeilchenTyp
                Case Typ.Elektron
                    sName = "Elektron"
                    dMasse = 0.0005485
                    iLadung = -1
                    bolColored = True
                    sColor = "blue"
                Case Typ.Proton
                    sName = "Proton"
                    dMasse = 1.0072276
                    iLadung = 1
                    bolColored = True
                    sColor = "red"
                Case Typ.Neutron
                    sName = "Neutron"
                    dMasse = 1.0086649
                    iLadung = 0
                    bolColored = True
                    sColor = "yellow"
            End Select
        End Set
    End Property

    Public Enum Typ
        Proton
        Neutron
        Elektron
    End Enum

    Private sName As String
    Public Property Name() As String
        Get
            Return sName
        End Get
        Set(ByVal value As String)
            sName = value
        End Set
    End Property

    Private dMasse As Double
    Public Property Masse() As Double
        Get
            Return dMasse
        End Get
        Set(ByVal value As Double)
            dMasse = value
        End Set
    End Property

    Private iLadung As Integer
    Public Property Ladung() As Integer
        Get
            Return iLadung
        End Get
        Set(ByVal value As Integer)
            iLadung = value
        End Set
    End Property

    Private sLatexCode As String
    Public ReadOnly Property LatexCode() As String
        Get
            Select Case eTeilchenTyp
                Case Typ.Elektron
                    sLatexCode = "\shade[ball color=" & IIf(Me.bolColored, sColor, "white") & "] (" & Me.sXPos & ":" _
                        & Me.sYPos & ") circle (.2);" & vbCrLf _
                        & "\draw (" & Me.sXPos & ":" & Me.sYPos & ") node{$-$};"
                Case Typ.Proton
                    sLatexCode = "\shade[ball color=" & IIf(Me.bolColored, sColor, "white") & "] (" & Me.sXPos & ":" _
                        & Me.sYPos & ") circle (.25);" & vbCrLf _
                        & "\draw (" & Me.sXPos & ":" & Me.sYPos & ") node{$+$};"
                Case Typ.Neutron
                    sLatexCode = "\shade[ball color=" & IIf(Me.bolColored, sColor, "white") & "] (" & Me.sXPos & ":" _
                    & Me.sYPos & ") circle (.25);"
            End Select
            Return sLatexCode
        End Get
    End Property

    Private dXPos As Double
    Public Property XPos() As Double
        Get
            Return dXPos
        End Get
        Set(ByVal value As Double)
            dXPos = value
        End Set
    End Property

    Public ReadOnly Property sXPos() As String
        Get
            Return dXPos.ToString("F", CultureInfo.InvariantCulture)
        End Get
    End Property

    Public ReadOnly Property sYPos() As String
        Get
            Return dYPos.ToString("F", CultureInfo.InvariantCulture)
        End Get
    End Property

    Private dYPos As Double
    Public Property YPos() As Double
        Get
            Return dYPos
        End Get
        Set(ByVal value As Double)
            dYPos = value
        End Set
    End Property

    Private bolColored As Boolean
    Public Property Colored() As Boolean
        Get
            Return bolColored
        End Get
        Set(ByVal value As Boolean)
            bolColored = value
        End Set
    End Property

    Private sColor As String
    Public Property Color() As String
        Get
            Return sColor
        End Get
        Set(ByVal value As String)
            sColor = value
        End Set
    End Property

End Class
