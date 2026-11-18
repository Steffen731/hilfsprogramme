Public Class Elektron
    Inherits Teilchen

    Public OrbitalsOrder As String() = {"1s", "2s", "2p", "3s", "3p", "4s", "3d", "4p", "5s", "4d", "5p", "6s", "5d", "4f", "5d", "6p", "7s", "6d", "5f", "6d", "7p"}
    Public sOrbitals As String() = {"s", "s", "p", "s", "p", "s", "d", "p", "s", "d", "p", "s", "d", "f", "d", "p", "s", "d", "f", "d", "p"}
    Public sPOrbitals As String() = {"px", "py", "pz"}
    Public sDOrbitals As String() = {"dxy", "dyz", "dxz", "dx2y2", "dz2"}
    Public sFOrbitals As String() = {"fx(x2-3y2)", "fz(x2-y2)", "fxz2", "fz2", "fyz2", "fxyz", "fy(y2-3x2)"}

    Public Sub New()

    End Sub

    Public Sub New(previousElektron As Elektron, pAtom As Atom)
        _parentAtom = pAtom
        Dim i As Integer
        TeilchenTyp = Typ.Elektron
        With previousElektron
            If .dSpin = 0.5 And .sOrbitalTyp = "s" Then
                sOrbitalTypExakt = .OrbitalTypExakt
                sOrbitalTyp = .OrbitalTyp
                iHauptQuantenZahlN = .HauptQuantenZahlN
                iNebenQuantenZahlL = .NebenQuantenZahlL
                iMagnetQuantenZahlM = .MagnetQuantenZahlM
                dSpin = -0.5
            Else
                Dim j As Integer = OrbitalsOrder.Count - 1
                For i = 0 To j
                    If .Orbital = OrbitalsOrder(i) And i < j Then
                        If .MagnetQuantenZahlM = .NebenQuantenZahlL Then
                            If .dSpin = 0.5 Then
                                sOrbitalTyp = .OrbitalTyp
                                iHauptQuantenZahlN = .HauptQuantenZahlN
                                iNebenQuantenZahlL = .NebenQuantenZahlL
                                iMagnetQuantenZahlM = -1 * iNebenQuantenZahlL
                                dSpin = -0.5
                            Else
                                sOrbitalTyp = OrbitalsOrder(i + 1).Substring(1, 1)
                                iHauptQuantenZahlN = Integer.Parse(OrbitalsOrder(i + 1).Substring(0, 1))
                                iNebenQuantenZahlL = GetNebenQuantenZahlFromOrbitalTyp(sOrbitalTyp)
                                iMagnetQuantenZahlM = -1 * iNebenQuantenZahlL
                                dSpin = 0.5
                            End If
                        Else
                            sOrbitalTyp = .OrbitalTyp
                            iHauptQuantenZahlN = .HauptQuantenZahlN
                            iNebenQuantenZahlL = .NebenQuantenZahlL
                            iMagnetQuantenZahlM = .MagnetQuantenZahlM + 1
                            dSpin = .dSpin
                        End If
                        'If sOrbitalTyp = "s" Then dSpin = 0.5
                        sOrbitalTypExakt = GetOrbitalTypExaktFromLM(iNebenQuantenZahlL, iMagnetQuantenZahlM)
                    End If
                Next
            End If
        End With
    End Sub

    Private Function GetNebenQuantenZahlFromOrbitalTyp(sTyp As String) As Integer
        Select Case sTyp
            Case "s"
                Return 0
            Case "p"
                Return 1
            Case "d"
                Return 2
            Case "f"
                Return 3
            Case "g"
                Return 4
            Case "h"
                Return 5
            Case Else
                Return -1
        End Select
    End Function

    Private Function GetOrbitalTypExaktFromLM(iL As Integer, iM As Integer) As String
        If iM < -1 * iL Or iM > iL Then Return ""
        Try
            Select Case iL
                Case 0
                    Return "s"
                Case 1
                    Return sPOrbitals(iM + 1)
                Case 2
                    Return sDOrbitals(iM + 2)
                Case 3
                    Return sFOrbitals(iM + 3)
                Case Else
                    Return ""
            End Select
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Private _parentAtom As Atom
    Public Property parentAtom() As Atom
        Get
            Return _parentAtom
        End Get
        Set(ByVal value As Atom)
            _parentAtom = value
        End Set
    End Property

    Public ReadOnly Property Orbital() As String
        Get
            Return iHauptQuantenZahlN.ToString & sOrbitalTyp
        End Get
    End Property

    Private sOrbitalTypExakt As String
    Public Property OrbitalTypExakt() As String
        Get
            Return sOrbitalTypExakt
        End Get
        Set(ByVal value As String)
            sOrbitalTypExakt = value
        End Set
    End Property

    Private sOrbitalTyp As String
    Public Property OrbitalTyp() As String
        Get
            Return sOrbitalTyp
        End Get
        Set(ByVal value As String)
            sOrbitalTyp = value
        End Set
    End Property

    Private iHauptQuantenZahlN As Integer
    Public Property HauptQuantenZahlN() As Integer
        Get
            Return iHauptQuantenZahlN
        End Get
        Set(ByVal value As Integer)
            iHauptQuantenZahlN = value
        End Set
    End Property

    Private iNebenQuantenZahlL As Integer
    Public Property NebenQuantenZahlL() As Integer
        Get
            Return iNebenQuantenZahlL
        End Get
        Set(ByVal value As Integer)
            iNebenQuantenZahlL = value
        End Set
    End Property

    Private iMagnetQuantenZahlM As Integer
    Public Property MagnetQuantenZahlM() As Integer
        Get
            Return iMagnetQuantenZahlM
        End Get
        Set(ByVal value As Integer)
            iMagnetQuantenZahlM = value
        End Set
    End Property


    Private dSpin As Double
    Public Property SpinS() As Double
        Get
            Return dSpin
        End Get
        Set(ByVal value As Double)
            dSpin = value
        End Set
    End Property

End Class
