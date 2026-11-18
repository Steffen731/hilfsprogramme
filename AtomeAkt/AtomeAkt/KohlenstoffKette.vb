Public Class KohlenstoffKette
    Implements IComparable

    Sub New()

    End Sub

    Sub New(oKettenstart As Atom, iPosition As Integer, Optional oMotherChain As KohlenstoffKette = Nothing)
        _aKettenstart = oKettenstart
        _iPosition = iPosition
        If oMotherChain IsNot Nothing Then
            _pCKette = oMotherChain
            _iAltPosition = oMotherChain.Länge - iPosition + 1
            _iWinkel = _pCKette.Winkel + 90
        Else
            Winkel = 0
        End If
        SetCKette()
    End Sub

    Private Sub SetCKette()
        Dim tmpAtom As Atom = Kettenstart
        tmpAtom.CKette = Me
        Do While tmpAtom.lstNachbarAtome.Count > 1 AndAlso tmpAtom.lstNachbarAtome(1).Bindigkeit > 1
            tmpAtom.lstNachbarAtome(1).CKette = Me
            tmpAtom = tmpAtom.lstNachbarAtome(1)
        Loop
    End Sub


    Public ReadOnly Property LatexStringRechtWinkligAusführlich() As String
        Get
            Dim sLat As String = ""
            Dim j As Integer = 0
            Dim aAtom As Atom = Kettenstart
            If _pCKette Is Nothing Then
                If aAtom.lstNachbarAtome.Count > 0 Then sLat += _aKettenstart.lstNachbarAtome(0).Symbol
                sLat += "-"
                If BindungJeWinkel(0) <> 0 Then sLat += "[" & BindungJeWinkel(0) & "]"

            End If
            Do Until aAtom.lstNachbarAtome.Count = 1
                sLat += aAtom.Symbol
                If aAtom.Bindigkeit > 1 Then
                    sLat += "(-[" & BindungJeWinkel(2) & "]"
                    If aAtom.lstNachbarAtome.Count > 2 Then
                        If aAtom.lstNachbarAtome(2).CKette IsNot Nothing Then
                            sLat += aAtom.lstNachbarAtome(2).CKette.LatexStringRechtWinkligAusführlich()
                        Else
                            sLat += aAtom.lstNachbarAtome(2).Symbol
                        End If
                    End If
                    sLat += ")"
                    sLat += "(-[" & BindungJeWinkel(6) & "]"
                    If aAtom.lstNachbarAtome.Count > 3 Then
                        If aAtom.lstNachbarAtome(3).CKette IsNot Nothing Then
                            sLat += aAtom.lstNachbarAtome(3).CKette.LatexStringRechtWinkligAusführlich()
                        Else
                            sLat += aAtom.lstNachbarAtome(3).Symbol
                        End If
                    End If
                    sLat += ")"
                    sLat += "-"
                    If BindungJeWinkel(0) <> 0 Then sLat += "[" & BindungJeWinkel(0) & "]"
                End If
                aAtom = aAtom.lstNachbarAtome(1)
            Loop
            sLat += aAtom.Symbol
            sLat = sLat.Replace("]C", ",1.5]C")
            sLat = sLat.Replace("-C", "-[,1.5]C")
            sLat = sLat.Replace("[,1.5]", "[0,1.5]")
            sLat = sLat.Replace("H-[0,1.5]C(-[2]H)(-[6]H)", "!{Mll}")
            sLat = sLat.Replace("-[2,1.5]C(-[4]H)(-[8]H)-[2]H", "!{Mlo}")
            sLat = sLat.Replace("-[6,1.5]C(-[4]H)(-[8]H)-[6]H", "!{Mlu}")
            sLat = sLat.Replace("-[0,1.5]C(-[2]H)(-[6]H)", "!{Ma}")
            sLat = sLat.Replace("-[2,1.5]C(-[4]H)(-[8]H)", "!{Mb}")
            sLat = sLat.Replace("-[6,1.5]C(-[4]H)(-H)", "!{Mc}")


            Return sLat
        End Get
    End Property

    Private Function BindungJeWinkel(iNummer As Integer) As String
        Select Case Winkel
            Case 90
                Return (iNummer + 2).ToString
            Case 180
                Return (iNummer + 4).ToString
            Case 270
                Return (iNummer + 6).ToString
            Case Else
                Return iNummer.ToString()
        End Select

    End Function

    Private _aKettenstart As Atom
    Public Property Kettenstart() As Atom
        Get
            Return _aKettenstart
        End Get
        Set(ByVal value As Atom)
            _aKettenstart = value
        End Set
    End Property

    Private _pCKette As KohlenstoffKette
    Public Property ParentCKette() As KohlenstoffKette
        Get
            Return _pCKette
        End Get
        Set(ByVal value As KohlenstoffKette)
            _pCKette = value
        End Set
    End Property

    Private _iWinkel As Integer
    Public Property Winkel() As Integer
        Get
            Return _iWinkel
        End Get
        Set(ByVal value As Integer)
            _iWinkel = value
        End Set
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return NameAusAnzahlCAtome(Länge())
        End Get

    End Property

    Private _iPosition As Integer
    Public Property Position() As Integer
        Get
            Return _iPosition
        End Get
        Set(ByVal value As Integer)
            _iPosition = value
        End Set
    End Property

    Private _iAltPosition As Integer
    Public Property AlternativePosition() As Integer
        Get
            Return _iAltPosition
        End Get
        Set(ByVal value As Integer)
            _iAltPosition = value
        End Set
    End Property

    Public ReadOnly Property Länge() As Integer
        Get
            Dim tmpAtom As Atom = Kettenstart
            Dim i As Integer = 1
            Do While tmpAtom.lstNachbarAtome.Count > 1 AndAlso tmpAtom.lstNachbarAtome(1).Bindigkeit > 1
                i += 1
                tmpAtom = tmpAtom.lstNachbarAtome(1)
            Loop
            Return i
        End Get
    End Property

    'Public iSeitenKettenListe(2) As Integer
    'Public Sub FillSeitenKettenListe(iMaxHauptKette As Integer)
    '    If iMaxHauptKette > 100 Or iMaxHauptKette < 1 Then iMaxHauptKette = 100
    '    Dim iCounter As Integer = 0
    '    For iCCount = 3 To iMaxHauptKette         'alle Alkane der Reihe nach, i=Hauptkettenlänge
    '        Dim IKW(iCCount) As Integer
    '        iSeitenKettenListe(iCounter) = (iCCount,)
    '        For iSKLenth = 1 To IIf(iCCount Mod 2 = 0, iCCount / 2 - 1, iCCount / 2) 'alle Seitenketten der Reihe nach
    '            For iSKCount = 1 To IIf(iCCount Mod 2 = 0, iCCount / 2 - 1, iCCount / 2)

    '            Next
    '        Next
    '    Next


    'End Sub

    Private sNamesOne As String() = {"Methan", "Ethan", "Propan", "Butan", "Pentan", "Hexan", "Heptan", "Octan", "Nonan", "Decan", "Undecan", "Dodecan"}
    Private sNamesTwo As String() = {"hen", "do", "tri", "tetra", "penta", "hexa", "hepta", "octa", "nona"}
    Private sNamesThree As String() = {"decan", "eicosan", "tricontan", "tetracontan", "pentacontan", "hexacontan", "heptacontan", "octacontan", "nonacontan"}

    Public Function NameAusAnzahlCAtome(iAnzahlC As Integer)
        If iAnzahlC < 1 Or iAnzahlC > 99 Then Return ""
        Select Case iAnzahlC
            Case < 13
                Return sNamesOne(iAnzahlC - 1)
            Case < 20
                Return sNamesTwo(iAnzahlC - 11) & "decan"
            Case Else
                Dim sMain As String = sNamesThree(Integer.Parse(iAnzahlC.ToString.Substring(0, 1)) - 1)
                If iAnzahlC Mod 10 <> 0 Then sMain = sNamesTwo(Integer.Parse(iAnzahlC.ToString.Substring(1, 1)) - 1) & sMain
                Return sMain
        End Select
    End Function

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        Dim other As KohlenstoffKette = DirectCast(obj, KohlenstoffKette)
        Return Me.Name.CompareTo(other.Name)
    End Function

End Class
