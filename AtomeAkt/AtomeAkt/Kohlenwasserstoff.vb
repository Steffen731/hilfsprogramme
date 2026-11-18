Imports System
Imports System.Collections.ObjectModel
Imports System.Data.SqlClient
Imports System.Diagnostics.Contracts
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions

Public Class Kohlenwasserstoff

    Private Const iMaxNumOfC As Integer = 100

    Public Sub New()

    End Sub

    Public Sub New(sCKettenFormelLang As String)
        Dim i As Integer = 1
        Dim aAtom As Atom

        For Each sLet In sCKettenFormelLang.ToArray
            If sLet = "C" Then
                aAtom = New Atom(6)
                If _Kettenstart Is Nothing Then
                    _Kettenstart = aAtom
                    Dim bAtom As New Atom(1)
                    aAtom.lstNachbarAtome.Add(bAtom)
                    bAtom.lstNachbarAtome.Add(aAtom)
                Else


                End If

            End If



        Next



    End Sub

    Private _H As Atom
    Public Property H() As Atom
        Get
            Return _H
        End Get
        Set(ByVal value As Atom)
            _H = value
        End Set
    End Property

    Public ReadOnly Property CKettenLänge() As Integer
        Get
            _Kettenstart.KettenNummer = 1
            Return _Kettenstart.GetKettenLänge(6)
        End Get
    End Property

    Private _Kettenstart As Atom
    Public Property Kettenstart() As Atom
        Get
            Return _Kettenstart
        End Get
        Set(ByVal value As Atom)
            _Kettenstart = value
        End Set
    End Property

    Public Const sSubmolH = "	\definesubmol{X}{H}"
    Public Const sSubmolNoH = " \definesubmol{X}{\vphantom{H}}"

    Public Const sSubmolMain = "\definesubmol{x}{(-[::+90, 0.8]!{X})(-[::-90,0.8]!{X})}
	                            \definesubmol{y}{(-[::+90,0.8]!{X})(-[::-90,0.8]!{X})-[::0,0.8]!{X}}
	                            \definesubmol{Ma}{-[::0,1.5]C!{x}}
	                            \definesubmol{Mll}{!{X}-C!{x}}
	                            \definesubmol{Mlr}{-[::0,1.5]C!{y}}
	                            \definesubmol{Mlo}{-[::90,1.5]C!{y}}
	                            \definesubmol{Mlok}{-[::90,.75]C!{y}}
	                            \definesubmol{Mlu}{-[::270,1.5]C!{y}}
	                            \definesubmol{Mluk}{-[::270,.75]C!{y}}
	                            \definesubmol{Mb}{-[::90,1.5]C!{x}}
	                            \definesubmol{Mbk}{-[::90,0.75]C!{x}}
	                            \definesubmol{Mc}{-[::270,1.5]C!{x}}
	                            \definesubmol{Mck}{-[::270,.75]C!{x}}
                                \definesubmol{KeHu}{-[::0]C(-[::270]{x})}
                                \definesubmol{Ko}{-[::90]C}
                                \definesubmol{KoH}{-[::90]C(-[::270]{x})}
                                \definesubmol{Hu}{(-[::270]{x})}"

    Public Const sSubmolSuFo = "\definesubmol{Ma}{-[::0]CH_2}
	                            \definesubmol{Mll}{H_3C}
	                            \definesubmol{Mlr}{-[::0]CH_3}
	                            \definesubmol{Mlo}{-[::90]CH_3}
	                            \definesubmol{Mlok}{-[::90]CH_3}
	                            \definesubmol{Mlu}{-[::270]CH_3}
	                            \definesubmol{Mluk}{-[::270]CH_3}
	                            \definesubmol{Mb}{-[::90]CH_2}
	                            \definesubmol{Mbk}{-[::90]CH_2}
	                            \definesubmol{Mc}{-[::270]CH_2}
	                            \definesubmol{Mck}{-[::270]CH_2}
                                \definesubmol{KeHu}{-[::0]CH}
                                \definesubmol{Ko}{-[::90]C}
                                \definesubmol{KoH}{-[::90]CH}
                                \definesubmol{Hu}{(H)}"


    Public Sub CreateEinfachVerzweigteKette(iNumberC As Integer)
        If iNumberC < 1 Or iNumberC > iMaxNumOfC Then Exit Sub
        Dim iCCounter As Integer = 1
        Dim iHKette As Integer = ZufallszahlMM(MinLeKetteEinfVerzwg(iNumberC), iNumberC - 1)
        Dim iSeitenKettenAnzahl As Integer =
            ZufallszahlMM(MinimumSeitenkette(iHKette, iNumberC), Math.Min(iHKette - 2, iNumberC - iHKette))
        CreateMainChain(iHKette)
        CreateAndAppendSeitenKettenZufall(iSeitenKettenAnzahl, iNumberC - iHKette, iHKette)
        _Kettenstart.FillFreeBondsWithAtomType(1)
    End Sub

    Public Sub CreateMainChain(iLength As Integer)
        Dim aAtom As New Atom(6)
        _Kettenstart = aAtom
        Dim bAtom As New Atom(1)
        aAtom.lstNachbarAtome.Add(bAtom)
        bAtom.lstNachbarAtome.Add(aAtom)
        CreateChain(iLength, aAtom)
    End Sub

    Public Function LengthOfSimpleChain(startAtom As Atom) As Integer
        Dim iCCounter As Integer = 1
        Dim cAtom As Atom = startAtom
        Do Until cAtom.lstNachbarAtome.Count = 1 OrElse cAtom.lstNachbarAtome(1).Bindigkeit = 1
            iCCounter += 1
            cAtom = cAtom.lstNachbarAtome(1)
        Loop
        Return iCCounter
    End Function

    Private _sName As String
    Public Property Name() As String
        Get
            Return _sName
        End Get
        Set(ByVal value As String)
            _sName = value
        End Set
    End Property

    Public ReadOnly Property HauptKette() As KohlenstoffKette
        Get
            If _SeitenKetten.Count > 0 Then
                Return _SeitenKetten.Where(Function(x) x.Position = 0).FirstOrDefault
            Else
                Return Nothing
            End If
        End Get
    End Property

    Private _SeitenKetten As New List(Of KohlenstoffKette)
    Public Property ChainNames() As List(Of KohlenstoffKette)
        Get
            Return _SeitenKetten
        End Get
        Set(ByVal value As List(Of KohlenstoffKette))
            _SeitenKetten = value
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

    Public Sub NamesOfChains()
        Dim iCCounter As Integer = 0
        Dim i As Integer
        Dim sLKK As New List(Of KohlenstoffKette)()
        Dim tmpAtom As Atom
        Dim cAtom As Atom = _Kettenstart
        sLKK.Add(New KohlenstoffKette(cAtom, 0))
        Do Until cAtom.lstNachbarAtome.Count = 1 OrElse cAtom.lstNachbarAtome(1).Bindigkeit = 1
            iCCounter += 1
            If cAtom.GetTypeOfC = "tertiär" Then
                If cAtom.lstNachbarAtome(2).Bindigkeit > 1 Then
                    tmpAtom = cAtom.lstNachbarAtome(2)
                Else
                    tmpAtom = cAtom.lstNachbarAtome(3)
                End If
                sLKK.Add(New KohlenstoffKette(tmpAtom, iCCounter, sLKK(0)))
            End If
            If cAtom.GetTypeOfC = "quarterniär" Then
                For j As Integer = 2 To 3
                    i = LengthOfSimpleChain(cAtom.lstNachbarAtome(j))
                    sLKK.Add(New KohlenstoffKette(cAtom.lstNachbarAtome(j), iCCounter, sLKK(0)))
                Next
            End If
            cAtom = cAtom.lstNachbarAtome(1)
        Loop
        _SeitenKetten = sLKK.OrderBy(Function(x) x.Name).ToList

    End Sub

    Public Sub NameFromChains()

        Dim s1 = _SeitenKetten.Sum(Function(x) x.Position)
        Dim s2 = _SeitenKetten.Sum(Function(x) x.AlternativePosition)

        Dim myChains = (From v In _SeitenKetten
                        Where v.Position > 0
                        Order By v.Name, IIf(s1 > s2, v.AlternativePosition, v.Position)).ToList

        Dim i, j As Integer
        Dim sTmp, sTmp1, sName As String
        sName = ""
        For i = 0 To myChains.Count - 1
            sTmp = myChains(i).Name
            sTmp1 = IIf(s1 < s2, myChains(i).Position, myChains(i).AlternativePosition)
            j = 1
            While i + j < myChains.Count AndAlso sTmp = myChains(i + j).Name
                sTmp1 += "," & IIf(s1 < s2, myChains(i + j).Position, myChains(i + j).AlternativePosition)
                j += 1
            End While
            sName += sTmp1 & "-" & IIf(j = 1, "", sPref(j - 1).ToLower) & sTmp.Substring(0, sTmp.Length - 2).ToLower & "yl" & "-"
            i = i + j - 1
        Next
        sName += (From v In _SeitenKetten
                  Where v.Position = 0
                  Select v.Name).FirstOrDefault.ToLower
        If Char.IsNumber(sName.First) Then
            Dim iPos As Integer = sName.IndexOf("-")
            sName = sName.Substring(0, iPos + 1) & sName.Substring(iPos + 1, 1).ToUpper & sName.Substring(iPos + 2)
        Else
            sName = sName.Substring(0, 1).ToUpper & sName.Substring(1)
        End If

        _sName = sName
    End Sub

    Private ReadOnly sPref As String() = {"mono", "di", "tri", "tetra", "penta", "hexa", "hepta", "octa", "nona", "deca"}

    Private Sub CreateAndAppendSeitenKettenZufall(iKettenAnzahl As Integer, iNumberOfC As Integer, iMainChainLength As Integer)

        Dim iChainLengths(iKettenAnzahl - 1) As Integer
        Dim iChainPosition(iKettenAnzahl - 1) As Integer
        Dim i, j, k As Integer
        Dim iRestlicheC As Integer = iNumberOfC
        For i = 1 To iKettenAnzahl
            iChainLengths(i - 1) = ZufallszahlMM(1, Math.Min(iRestlicheC - iKettenAnzahl + i, (iMainChainLength / 2 - 1)))
            iRestlicheC -= iChainLengths(i - 1)
        Next
        Array.Sort(iChainLengths)
        If iRestlicheC > 0 Then
            For i = 0 To iKettenAnzahl - 1
                If iChainLengths(i) > (iMainChainLength / 2 - 1) Then Exit For
            Next
            If i > 0 Then
                Do Until iRestlicheC = 0
                    k = ZufallszahlMM(0, i - 1)
                    iRestlicheC += iChainLengths(k)
                    iChainLengths(k) = ZufallszahlMM(iChainLengths(k) + 1, Math.Min(iRestlicheC, (iMainChainLength / 2 - 1)))
                    iRestlicheC -= iChainLengths(k)
                Loop
            End If
        End If
        Array.Sort(iChainLengths)
        Array.Reverse(iChainLengths)
        Dim bolOk As Boolean

        For i = 0 To iKettenAnzahl - 1
            bolOk = False
            Do Until bolOk
                j = ZufallszahlMM(1, iMainChainLength - 2 * iChainLengths(i))
                j += iChainLengths(i)
                If iChainPosition.Count(Function(x) x = j) < 2 Then bolOk = True
            Loop
            iChainPosition(i) = j
        Next

        Dim tmpA As Atom
        For i = 0 To iKettenAnzahl - 1
            tmpA = _Kettenstart
            For j = 0 To iChainPosition(i) - 1
                tmpA = tmpA.lstNachbarAtome(1)
            Next
            CreateChain(iChainLengths(i), tmpA)
        Next
        NamesOfChains()
        NameFromChains()

    End Sub

    Public Sub CreateAndAppendSeitenketten(iAnzahlVerschSK As Integer, iAnzahlGleiche() As Integer, iMainChainMinLength As Integer, iMainChainMaxLength As Integer, Optional iMaxLenSK As Integer = -1)
        Dim iMainChainLength As Integer = ZufallszahlMM(iMainChainMinLength, iMainChainMaxLength)                           'Länge der Hauptkette
        CreateMainChain(iMainChainLength)
        Dim iChains(iAnzahlVerschSK - 1) As Integer                                                                         'Länge der verschiedenen SKs
        Dim mySumSKTotal = iAnzahlGleiche.Sum() - iAnzahlGleiche.Length + iAnzahlVerschSK - 1                               'Gesamtzahl SK
        'Dim iChainLengths(mySumSKTotal) As Integer                                                                          'Längen der Seitenketten inkl. Wiederholungen
        'Dim iChainPosition(mySumSKTotal) As Integer                                                                         'Positionen der Seitenketten an der Hauptkette
        If iAnzahlVerschSK > Math.Floor(iMainChainLength / 2 - 1) Then iAnzahlVerschSK = Math.Floor(iMainChainLength / 2 - 1)
        If iMaxLenSK = -1 Then iMaxLenSK = Math.Floor(IIf(iMainChainLength Mod 2 = 0, iMainChainLength / 2 - 1, iMainChainLength / 2))
        Dim iMaxSKLength As Integer = Math.Min(Math.Floor(IIf(iMainChainLength Mod 2 = 0, iMainChainLength / 2 - 1, iMainChainLength / 2)), iMaxLenSK)       'Maximal mögliche Länge der Seitenketten

        'Welche Seitenketten kommen mehrfach vor

        Dim i, j As Integer

        For i = 0 To iAnzahlVerschSK - 1                                        'Festlegung der Längen der verschiedenen Seitenketten
            iChains(i) = ZufallszahlMMNoRepeat(1, iMaxSKLength, iChains)
        Next
        Array.Sort(iChains)
        Array.Reverse(iChains)


        'Dim iAnz = Math.Min(iAnzahlGleiche.Length - 1, iAnzahlVerschSK)         'Festlegung der Wiederholungen, Maximal so wiele wie versch. wie verschiedene SK
        Dim iAnz = iChains.Count - 1
        Dim iWelcheSKWieivel(iAnz) As Integer
        For i = 0 To iAnz
            iWelcheSKWieivel(i) = ZufallszahlMM(IIf(iWelcheSKWieivel.Contains(1), iAnzahlGleiche(0), 1), Math.Min((iMainChainLength - 2 * iChains(i)) * 2 - iWelcheSKWieivel.Sum, iAnzahlGleiche(1)))
        Next

        Dim iChainLengths(iWelcheSKWieivel.Sum - 1) As Integer
        Dim iChainPosition(iWelcheSKWieivel.Sum - 1) As Integer

        Dim iTotal As Integer = 0
        For i = 0 To iAnz
            For j = 1 To iWelcheSKWieivel(i)
                iChainLengths(iTotal) = iChains(i)
                iTotal += 1
            Next
        Next i

        Array.Sort(iChainLengths)
        Array.Reverse(iChainLengths)
        Dim bolOk As Boolean

        For i = 0 To iTotal - 1
            bolOk = False
            Do Until bolOk
                j = ZufallszahlMM(iChainLengths(i) + 1, iMainChainLength - iChainLengths(i))
                If iChainPosition.Count(Function(x) x = j) < 2 Then bolOk = True
            Loop
            iChainPosition(i) = j
        Next

        'Dim bolKnick As Boolean = False
        'Dim iKnickpos As Integer = GetMedianOfArray(iChainPosition)
        Dim tmpA As Atom
        For i = 0 To iTotal - 1
            tmpA = _Kettenstart
            For j = 0 To iChainPosition(i) - 1
                tmpA = tmpA.lstNachbarAtome(1)
            Next
            CreateChain(iChainLengths(i), tmpA)
            'If iChainPosition(i) = iKnickpos Then

            'End If
        Next
        NamesOfChains()
        NameFromChains()

    End Sub

    Public Function GetMedianOfArray(theArray As Integer()) As Integer
        Dim workArray(theArray.Length) As Integer
        Array.Copy(theArray, workArray, theArray.Length)
        Array.Sort(workArray)
        Return workArray(workArray.Length / 2)
    End Function


    Public Sub CreateChain(iLength As Integer, ByRef startAtom As Atom)
        If startAtom Is Nothing Then Exit Sub
        Dim nAtom As Atom
        For i = 1 To iLength
            nAtom = New Atom(6)
            startAtom.lstNachbarAtome.Add(nAtom)
            nAtom.lstNachbarAtome.Add(startAtom)
            startAtom = nAtom
        Next
    End Sub

    Public Sub CreateTotalVerzweigt(iNumberOfCAtoms As Integer)
        If iNumberOfCAtoms < 1 Or iNumberOfCAtoms > iMaxNumOfC Then Exit Sub
        Dim iCCounter As Integer = 1
        Dim iLevel As Integer = 1
        Dim i, j As Integer
        Dim aAtom As Atom

        aAtom = New Atom(6)
        _Kettenstart = aAtom
        Dim lstAtom As List(Of Atom) = _Kettenstart.lstNachbarAtome
        i = ZufallszahlMM(1, IIf(iNumberOfCAtoms - iCCounter > 4, 4, iNumberOfCAtoms - iCCounter))
        For j = 1 To i
            aAtom = New Atom(6)
            lstAtom.Add(aAtom)
            iCCounter += 1
        Next

    End Sub

    Private Function MinimumSeitenkette(iHkette As Integer, iGesamtC As Integer, Optional iMaxSKLength As Integer = 1 - 1) As Integer
        Dim iCSeiten As Integer = iGesamtC - iHkette            'Anzahl C-Atome, die in Seitenketten untergebracht werden müssen
        Dim iCCounter As Integer = 0                            'C-Atom-Zähler der C-Atome in Seitenketten
        Dim iSKCounter As Integer = 0                           'Anzahl mindestens notwendiger Seitenketten
        If iMaxSKLength < 0 Then iMaxSKLength = iHkette / 2 - 1 'Maximale Länge der Seitenkette bei gerade C-Atomzahl der Hauptkette
        If iHkette Mod 2 = 0 Then                               'Bei gerader Zahl C-Atome
            For i = iMaxSKLength To 1 Step -1
                For j = 1 To 4
                    iCCounter += i
                    iSKCounter += 1
                    If iCCounter >= iCSeiten Then Return iSKCounter
                Next
            Next
        Else                                                    'bei ungerader Zahl C-Atome
            iCCounter += (iHkette + 1) / 2 - 1
            If iCCounter >= iCSeiten Then Return 1
            iCCounter += (iHkette + 1) / 2 - 1
            iSKCounter = 2
            If iCCounter >= iCSeiten Then Return 2
            For i = iMaxSKLength To 1 Step -19
                For j = 1 To 4
                    iCCounter += i
                    iSKCounter += 1
                    If iCCounter >= iCSeiten Then Return iSKCounter
                Next
            Next
        End If
        Return iSKCounter
    End Function

    Private ReadOnly Generator As New System.Random

    Private Function Zufallszahl(iMax As Integer) As Integer
        Return Generator.Next(iMax + 1)
    End Function

    Public Function ZufallszahlMM(ByVal Min As Integer, ByVal Max As Integer) As Integer
        If Min > Max Then
            Dim i As Integer = Min
            Min = Max
            Max = i
        End If
        Return Generator.Next(Min, Max + 1)
    End Function

    Public Function ZufallszahlMMNoRepeat(ByVal Min As Integer, ByVal Max As Integer, ByRef iAlready As Integer()) As Integer
        Dim i, myZuf As Integer

        If Min > Max Then
            i = Min
            Min = Max
            Max = i
        End If

        If Not ArrayContainsFreePlaces(Min, Max, iAlready) Then Return -1

        myZuf = Generator.Next(Min, Max + 1)
        If iAlready IsNot Nothing Then
            i = 0
            Do While iAlready.Contains(myZuf)
                myZuf = Generator.Next(Min, Max + 1)
                i += 1
                If i > 1000 Then Return -1
            Loop
        End If

        Return myZuf

    End Function

    Private Function ArrayContainsFreePlaces(iMin As Integer, iMax As Integer, theArray As Integer()) As Boolean
        If theArray Is Nothing Then Return True
        Dim i As Integer
        Dim bolFrei As Boolean = False
        If iMin > iMax Then
            i = iMin
            iMin = iMax
            iMax = iMin
        End If
        If iMin = iMax Then Return False


        For i = iMin To iMax
            If Not theArray.Contains(i) Then Return True
        Next
        Return False

    End Function

    Private Function MinLeKetteEinfVerzwg(iGesamtZahlCAtome As Integer) As Integer
        Return CInt(Math.Abs(Math.Sqrt(9 + 4 * iGesamtZahlCAtome) - 3))
    End Function

End Class
