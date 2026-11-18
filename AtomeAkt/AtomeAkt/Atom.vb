Imports System.Globalization
Imports System.Runtime.Remoting.Messaging
Imports System.Security.Cryptography
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Xml.Schema
Imports System.Net.Security

Public Class Atom
    Implements ICloneable

    Public myPSEData As DataSources = Module1.dieDS
    Public myPSE As PSE = Module1.dasPSE

    Public Sub New()

    End Sub

    Public Sub New(iOrdnungsZahl As Integer)
        SetValues(iOrdnungsZahl)
    End Sub

    Public Sub New(iOrdnungsZahl As Integer, iNeutronenZahl As Integer)
        SetValues(iOrdnungsZahl)
        SetNeutronenZahl(iNeutronenZahl)
    End Sub

    Sub New(iIsotopenListNumber As Integer, Optional bolFake As Boolean = False)

        SetValues(myPSEData.IsotopenListe(iIsotopenListNumber)(0))
        SetNeutronenZahl(myPSEData.IsotopenListe(iIsotopenListNumber)(1))

    End Sub

    Private Sub SetNeutronenZahl(iNeutronenZahl As Integer)
        For Each NeutronenList In lstLstIsotope
            If NeutronenList.Count = iNeutronenZahl Then
                lNeutronen = NeutronenList
                SetNukleonenRadien()
                Exit For
            End If
        Next
    End Sub

    Private Sub SetValues(iOrdnungsZahl As Integer)
        If iOrdnungsZahl < 1 Or iOrdnungsZahl > 118 Then
            MsgBox("Ordnungszahl muss zwischen 1 und 118 liegen. Wert war aber: " & iOrdnungsZahl)
            Exit Sub
        End If

        Dim sPSEData As Array = myPSEData.PSEData
        Dim sIsotope As Array = myPSEData.IsotopenArray
        With Me
            .iOrdnungsZahl = Me.CheckForEmpty(iOrdnungsZahl)
            Dim lIsotope1 = From iValues In myPSEData.IsotopenListe Where ((iValues(0) = iOrdnungsZahl) And (iValues(2) <> 0)) Select iValues
            .sName = sPSEData(iOrdnungsZahl)(1)
            .sSymbol = sPSEData(iOrdnungsZahl)(2)
            .dAtomMasse = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(3))
            lNeutronen.Clear()
            Dim dblIsotopenHäufigkeit As Double
            For Each Isotope In lIsotope1
                Dim myList As New List(Of Teilchen)
                dblIsotopenHäufigkeit = Isotope(2)
                lstIsotopnenHäufigkeit.Add(dblIsotopenHäufigkeit)
                For i As Integer = 1 To Isotope(1)
                    myList.Add(New Teilchen(Teilchen.Typ.Neutron))
                Next
                lstLstIsotope.Add(myList)
                If dblIsotopenHäufigkeit = 100 Then lNeutronen = myList
            Next
            lProtonen.Clear()
            For i As Integer = 1 To sPSEData(iOrdnungsZahl)(5)
                lProtonen.Add(New Teilchen(Teilchen.Typ.Proton))
            Next
            SetNukleonenRadien()
            lElektronen.Clear()
            Dim oElek As New Elektron()
            With oElek
                .parentAtom = Me
                .HauptQuantenZahlN = 1
                .OrbitalTyp = "s"
                .NebenQuantenZahlL = 0
                .MagnetQuantenZahlM = 0
                .OrbitalTypExakt = "s"
                .SpinS = 0.5
                .TeilchenTyp = Teilchen.Typ.Elektron
            End With
            lElektronen.Add(oElek)
            For i As Integer = 2 To sPSEData(iOrdnungsZahl)(6)
                oElek = New Elektron(lElektronen(i - 2), Me)
                lElektronen.Add(oElek)
            Next
            setElektronenRadien()
            .iPeriode = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(7))
            .iGruppe = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(8))
            .sAggregatZustand = sPSEData(iOrdnungsZahl)(9)
            If sPSEData(iOrdnungsZahl)(10) = "yes" Then
                .bolRadioaktiv = True
            Else
                .bolRadioaktiv = False
            End If
            If sPSEData(iOrdnungsZahl)(11) = "yes" Then
                .bolNatürlich = True
            Else
                .bolNatürlich = False
            End If
            If sPSEData(iOrdnungsZahl)(12) = "yes" Then
                .bolMetall = True
            Else
                .bolMetall = False
            End If
            If sPSEData(iOrdnungsZahl)(13) = "yes" Then
                .bolNichtMetall = True
            Else
                .bolNichtMetall = False
            End If
            If sPSEData(iOrdnungsZahl)(14) = "yes" Then
                .bolHalbMetall = True
            Else
                .bolHalbMetall = False
            End If
            Select Case sPSEData(iOrdnungsZahl)(15)
                Case "Noble Gas"
                    .sArt = "Edelgas"
                Case "Nonmetal"
                    .sArt = "Nichtmetall"
                Case "Alkali Metal"
                    .sArt = "Alkalimetall"
                Case "Alkaline Earth Metal"
                    .sArt = "Erdalkalimetall"
                Case "Metalloid"
                    .sArt = "Halbmetall"
                Case "Halogen"
                    .sArt = "Halogen"
                Case "Transition Metal"
                    .sArt = "Übergangsmetall"
                Case "Lanthanide"
                    .sArt = "Lanthanoid"
                Case "Actinide"
                    .sArt = "Aktinoid"
                Case "Transactinide"
                    .sArt = "Transaktinoid"
                Case Else
                    .sArt = sPSEData(iOrdnungsZahl)(15)
            End Select
            .dAtomRadius = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(16))
            .dElektroNegativität = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(17))
            .dErsteIonisierungsEnergie = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(18))
            .dDichte = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(19))
            .dSchmelzPunkt = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(20))
            .dSiedePunkt = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(21))
            .iIsotopenZahl = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(22))
            .iEntdeckungsJahr = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(24))
            .dSpezifischeWaermeKapazität = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(25))
            .iSchalenZahl = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(26))
            .iValenz = Me.CheckForEmpty(sPSEData(iOrdnungsZahl)(27))
        End With
    End Sub

    Public Sub Ionisierung(iLadung As Integer)
        Dim iCurrentLadung As Integer = lProtonen.Count - lElektronen.Count
        If iLadung = iCurrentLadung Then
            Exit Sub
        ElseIf iLadung < iCurrentLadung Then
            Do While (lstRemovedElektrons.Count > 0) And iLadung < (lProtonen.Count - lElektronen.Count)
                lElektronen.Add(lstRemovedElektrons.Last)
                lstRemovedElektrons.Remove(lstRemovedElektrons.Last)
            Loop
            Do While iLadung < (lProtonen.Count - lElektronen.Count)
                lElektronen.Add(New Elektron)
            Loop
        End If

    End Sub

    Private lstRemovedElektrons As List(Of Elektron)
    Public Property RemovedElektrons() As List(Of Elektron)
        Get
            Return lstRemovedElektrons
        End Get
        Set(ByVal value As List(Of Elektron))
            lstRemovedElektrons = value
        End Set
    End Property

    Private lstAddedElektrons As List(Of Elektron)
    Public Property AddedElektrons() As List(Of Elektron)
        Get
            Return lstAddedElektrons
        End Get
        Set(ByVal value As List(Of Elektron))
            lstAddedElektrons = value
        End Set
    End Property

    Private lstLstIsotope As New List(Of List(Of Teilchen))
    Public Property LstIsotope() As List(Of List(Of Teilchen))
        Get
            Return lstLstIsotope
        End Get
        Set(ByVal value As List(Of List(Of Teilchen)))
            lstLstIsotope = value
        End Set
    End Property

    Private lstIsotopnenHäufigkeit As New List(Of Double)
    Public Property IsotopenHäufigkeit() As List(Of Double)
        Get
            Return lstIsotopnenHäufigkeit
        End Get
        Set(ByVal value As List(Of Double))
            lstIsotopnenHäufigkeit = value
        End Set
    End Property

    Public ReadOnly Property IsotopenListeZahl() As Integer
        Get
            Return lstLstIsotope.Count
        End Get

    End Property

    Public ReadOnly Property LadungsbetragHGIon() As Integer
        Get
            Select Case Gruppe
                Case 1, 2
                    Return Gruppe
                Case 13
                    Return 3
                Case 14
                    Return 4
                Case 15, 16, 17
                    Return 18 - Gruppe
                Case Else
                    Return 0
            End Select
        End Get

    End Property

    Private Function CheckForEmpty(theString As String) As Double
        If theString = "" Then Return 0
        Try
            Return CDbl(theString.Replace(".", ","))
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Private iOrdnungsZahl As Integer
    Public Property OrdnungsZahl() As Integer
        Get
            Return iOrdnungsZahl
        End Get
        Set(ByVal value As Integer)
            iOrdnungsZahl = value
        End Set
    End Property

    Private dAtomMasse As Double
    Public Property AtomMasse() As Double
        Get
            Return dAtomMasse
        End Get
        Set(ByVal value As Double)
            dAtomMasse = value
        End Set
    End Property

    Private iPeriode As Integer
    Public Property Periode() As Integer
        Get
            Return iPeriode
        End Get
        Set(ByVal value As Integer)
            iPeriode = value
        End Set
    End Property

    Private iGruppe As Integer
    Public Property Gruppe() As Integer
        Get
            Return iGruppe
        End Get
        Set(ByVal value As Integer)
            iGruppe = value
        End Set
    End Property

    Private sAggregatZustand As String
    Public Property AggregatZustand() As String
        Get
            Return sAggregatZustand
        End Get
        Set(ByVal value As String)
            sAggregatZustand = value
        End Set
    End Property

    Private bolRadioaktiv As Boolean
    Public Property Radioaktiv() As Boolean
        Get
            Return bolRadioaktiv
        End Get
        Set(ByVal value As Boolean)
            bolRadioaktiv = value
        End Set
    End Property

    Private bolNatürlich As Boolean
    Public Property Natürlich() As Boolean
        Get
            Return bolNatürlich
        End Get
        Set(ByVal value As Boolean)
            bolNatürlich = value
        End Set
    End Property

    Private bolMetall As Boolean
    Public Property Metall() As Boolean
        Get
            Return bolMetall
        End Get
        Set(ByVal value As Boolean)
            bolMetall = value
        End Set
    End Property

    Private bolNichtMetall As Boolean
    Public Property NichtMetall() As Boolean
        Get
            Return bolNichtMetall
        End Get
        Set(ByVal value As Boolean)
            bolNichtMetall = value
        End Set
    End Property

    Private bolHalbMetall As Boolean
    Public Property HalbMetall() As Boolean
        Get
            Return bolHalbMetall
        End Get
        Set(ByVal value As Boolean)
            bolHalbMetall = value
        End Set
    End Property

    Private sArt As String
    Public Property Art() As String
        Get
            Return sArt
        End Get
        Set(ByVal value As String)
            sArt = value
        End Set
    End Property

    Private dAtomRadius As Double
    Public Property AtomRadius() As Double
        Get
            Return dAtomRadius
        End Get
        Set(ByVal value As Double)
            dAtomRadius = value
        End Set
    End Property

    Private dElektroNegativität As Double
    Public Property Elektronegativität() As Double
        Get
            Return dElektroNegativität
        End Get
        Set(ByVal value As Double)
            dElektroNegativität = value
        End Set
    End Property

    Private dErsteIonisierungsEnergie As Double
    Public Property ErsteIonisierungsenergie() As Double
        Get
            Return dErsteIonisierungsEnergie
        End Get
        Set(ByVal value As Double)
            dErsteIonisierungsEnergie = value
        End Set
    End Property

    Private dDichte As Double
    Public Property Dichte() As Double
        Get
            Return dDichte
        End Get
        Set(ByVal value As Double)
            dDichte = value
        End Set
    End Property

    Private dSchmelzPunkt As Double
    Public Property Schmelzpunkt() As Double
        Get
            Return dSchmelzPunkt
        End Get
        Set(ByVal value As Double)
            dSchmelzPunkt = value
        End Set
    End Property

    Private dSiedePunkt As Double
    Public Property SiedePunkt() As Double
        Get
            Return dSiedePunkt
        End Get
        Set(ByVal value As Double)
            dSiedePunkt = value
        End Set
    End Property

    Private iIsotopenZahl As Integer
    Public Property IsotopenZahl() As Integer
        Get
            Return iIsotopenZahl
        End Get
        Set(ByVal value As Integer)
            iIsotopenZahl = value
        End Set
    End Property

    Private sEntdecker As String
    Public Property Entdecker() As String
        Get
            Return sEntdecker
        End Get
        Set(ByVal value As String)
            sEntdecker = value
        End Set
    End Property

    Private iEntdeckungsJahr As Integer
    Public Property EntdeckungsJahr() As Integer
        Get
            Return iEntdeckungsJahr
        End Get
        Set(ByVal value As Integer)
            iEntdeckungsJahr = value
        End Set
    End Property

    Private dSpezifischeWaermeKapazität As Double
    Public Property SpezifischeWaermeKapazität() As Double
        Get
            Return dSpezifischeWaermeKapazität
        End Get
        Set(ByVal value As Double)
            dSpezifischeWaermeKapazität = value
        End Set
    End Property

    Private iSchalenZahl As Integer
    Public Property SchalenZahl() As Integer
        Get
            Return iSchalenZahl
        End Get
        Set(ByVal value As Integer)
            iSchalenZahl = value
        End Set
    End Property

    Private iValenz As Integer
    Public Property Valenz() As Integer
        Get
            Return iValenz
        End Get
        Set(ByVal value As Integer)
            iValenz = value
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

    Private sSymbol As String
    Public Property Symbol() As String
        Get
            Return sSymbol
        End Get
        Set(ByVal value As String)
            sSymbol = value
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

    Private dX As Double
    Public Property X() As Double
        Get
            Return dX
        End Get
        Set(ByVal value As Double)
            dX = value
        End Set
    End Property

    Private dY As Double
    Public Property Y() As Double
        Get
            Return dY
        End Get
        Set(ByVal value As Double)
            dY = value
        End Set
    End Property

    Private lProtonen As New List(Of Teilchen)
    Public Property Protonen() As List(Of Teilchen)
        Get
            Return lProtonen
        End Get
        Set(ByVal value As List(Of Teilchen))
            lProtonen = value
        End Set
    End Property

    Private lNeutronen As New List(Of Teilchen)
    Public Property Neutronen() As List(Of Teilchen)
        Get
            Return lNeutronen
        End Get
        Set(ByVal value As List(Of Teilchen))
            lNeutronen = value
        End Set
    End Property

    Private lElektronen As New List(Of Elektron)
    Public Property Elektronen() As List(Of Elektron)
        Get
            Return lElektronen
        End Get
        Set(ByVal value As List(Of Elektron))
            lElektronen = value
        End Set
    End Property

    Private lNukleonen As New List(Of Teilchen)
    Public Property Nukleonen() As List(Of Teilchen)
        Get
            Return lNukleonen
        End Get
        Set(ByVal value As List(Of Teilchen))
            lNukleonen = value
        End Set
    End Property

    Public ReadOnly Property NukleonenAnzahl() As Integer
        Get
            Return lNukleonen.Count
        End Get
    End Property

    Private _lstNachbarAtome As New List(Of Atom)
    Public Property lstNachbarAtome() As List(Of Atom)
        Get
            Return _lstNachbarAtome
        End Get
        Set(ByVal value As List(Of Atom))
            _lstNachbarAtome = value
        End Set
    End Property

    Private _bolNoLoop As Boolean
    Public Property NoLoop() As Boolean
        Get
            Return _bolNoLoop
        End Get
        Set(ByVal value As Boolean)
            _bolNoLoop = value
        End Set
    End Property

    Private _iNumberInMolecule As Integer
    Public Property NummerImMolekül() As Integer
        Get
            Return _iNumberInMolecule
        End Get
        Set(ByVal value As Integer)
            _iNumberInMolecule = value
        End Set
    End Property

    Private _iLevelOfBranching As Integer
    Public Property LevelOfBranching() As Integer
        Get
            Return _iLevelOfBranching
        End Get
        Set(ByVal value As Integer)
            _iLevelOfBranching = value
        End Set
    End Property

    Private _ParentAtom As Atom
    Public Property ParentAtom() As Atom
        Get
            Return _ParentAtom
        End Get
        Set(ByVal value As Atom)
            _ParentAtom = value
        End Set
    End Property

    Private _KohlenstoffKette As KohlenstoffKette
    Public Property CKette() As KohlenstoffKette
        Get
            Return _KohlenstoffKette
        End Get
        Set(ByVal value As KohlenstoffKette)
            _KohlenstoffKette = value
        End Set
    End Property

    Private _KettenNummer As Integer
    Public Property KettenNummer() As Integer
        Get
            Return _KettenNummer
        End Get
        Set(ByVal value As Integer)
            _KettenNummer = value
        End Set
    End Property

    Private _KettenNummerBack As Integer
    Public Property KettenNummerBack() As Integer
        Get
            Return _KettenNummerBack
        End Get
        Set(ByVal value As Integer)
            _KettenNummerBack = value
        End Set
    End Property

    Public Sub ResetKettenNummer()
        _KettenNummer = 0
        For Each iAtom In lstNachbarAtome
            If iAtom.KettenNummer <> 0 Then iAtom.ResetKettenNummer()
        Next
    End Sub

    Public Function GetKettenLänge(Optional iOZ As Integer = 6, Optional bolMainChainNullOne As Boolean = True) As Integer
        If iOrdnungsZahl <> iOZ Then Return 0
        Dim iLength As Integer = 0
        Dim iTmp As Integer = 0
        For Each iAtom As Atom In _lstNachbarAtome
            If iAtom.KettenNummer = 0 Then iAtom.KettenNummer = _KettenNummer + 1
            If (iAtom.KettenNummer > _KettenNummer) And (iAtom.OrdnungsZahl = iOZ) Then iTmp = iAtom.GetKettenLänge
            If iTmp > iLength Then
                iLength = iTmp
            End If
        Next
        _KettenNummerBack = iLength + 1
        Return _KettenNummerBack
    End Function

    Public Sub SortMainChain()
        If _lstNachbarAtome.Count <= 1 Then Exit Sub
        For Each i In {-1, 1}
            Dim myAt As Atom = _lstNachbarAtome.Where(Function(x) x.KettenNummerBack = KettenNummerBack + i).First
            If (myAt IsNot Nothing) AndAlso (myAt IsNot _lstNachbarAtome(IIf(i = -1, 0, 1))) Then
                Dim tmpA As Atom = _lstNachbarAtome(0)
                _lstNachbarAtome(0) = myAt
                myAt = tmpA
            End If
        Next
        _lstNachbarAtome(1).SortMainChain()
    End Sub

    Public Function GetTypeOfC() As String

        If iOrdnungsZahl <> 6 Then Return ""
        Dim iCCount As Integer = 0

        For Each iAtom In lstNachbarAtome
            If iAtom.OrdnungsZahl = 6 Then iCCount += 1
        Next

        Select Case iCCount
            Case 1
                Return "primär"
            Case 2
                Return "sekundär"
            Case 3
                Return "tertiär"
            Case 4
                Return "quarterniär"
            Case Else
                Return ""
        End Select

    End Function

    Public Function AggregateFormular() As String
        Dim sFml As String = Symbol
        For i = 0 To lstNachbarAtome.Count - 1
            If lstNachbarAtome(i).lstNachbarAtome.Count = 1 Then sFml += lstNachbarAtome(i).Symbol
        Next
        For i = lstNachbarAtome.Count - 1 To 1 Step -1
            If lstNachbarAtome(i).lstNachbarAtome.Count > 1 Then
                If i > 1 Then sFml += "("
                sFml += lstNachbarAtome(i).AggregateFormular()
                If i > 1 Then sFml += ")"
            End If
        Next
        Return sFml
    End Function

    Public Function AggregateFormularMixed() As String
        Dim sForm As String = AggregateFormular()
        Dim myReg As New Regex("\([CH]{1,}\)")
        Dim sMatch As String
        Dim matchc As MatchCollection = myReg.Matches(sForm)

        If matchc.Count > 2 Then
            Dim i, j, k As Integer
            Dim s1, s2, s3 As String
            If sForm.Substring(matchc(2).Index - 1, 1) = ")" Then
                i = matchc(1).Index
                sMatch = matchc(1).Value
            Else
                i = matchc(2).Index
                sMatch = matchc(2).Value
            End If
            s2 = ReverseChain(sForm.Substring(i + 1, sMatch.Length - 2))
            s3 = sForm.Substring(i + sMatch.Length)
            If sForm.Substring(i - 1, 1) = "H" Then
                    s1 = sForm.Substring(0, i - 2)
                    Return s2 & "CH(" & ReverseChain(s1) & ")" & s3
                Else
                    s1 = sForm.Substring(0, i - 1)
                    Return s2 & "C(" & ReverseChain(s1) & ")" & s3
                End If
            End If
            Return ""
    End Function

    Public Function ReverseChain(sChainToReverse As String) As String
        Dim sC As String = sChainToReverse
        Dim sR As String = ""
        Dim i, j As Integer
        Dim bolInKlammer As Boolean = False
        j = sC.Length
        For i = j - 1 To 0 Step -1
            If sC.Substring(i, 1) = ")" Then bolInKlammer = True
            If sC.Substring(i, 1) = "(" Then bolInKlammer = False
            If sC.Substring(i, 1) = "C" And Not bolInKlammer Then
                sR = sR & sC.Substring(i, j - i)
                j = i
            End If
        Next
        Return sR

    End Function

    Public Function SimpleFormular(Optional bolMixed As Boolean = False) As String
        Dim sForm As String
        If bolMixed Then
            sForm = AggregateFormularMixed()
        Else
            sForm = AggregateFormular()
        End If

        Dim i, j As Integer
        Dim sResult As String = ""

        Dim nChar As Char() = sForm.ToCharArray
        For i = 0 To nChar.Count - 1
            j = 0
            If nChar(i) = "H" Then
                j = 1
                Do Until i + j > nChar.Count - 1 OrElse nChar(i + j) <> "H"
                    j += 1
                Loop
                If j > 0 Then
                    sResult += "H" & IIf(j > 1, j.ToString, "")
                    i += j - 1
                End If
            Else
                sResult += nChar(i)
            End If
        Next
        Return sResult
    End Function

    Public Function NumberInList(aAtom As Atom)
        For i = 0 To _lstNachbarAtome.Count - 1
            If aAtom Is _lstNachbarAtome(i) Then Return i
        Next
        Return -1
    End Function

    Public Function LatexFromSimpleChainFormular(Optional bolMixed As Boolean = False) As String

        Dim sLat As String = SimpleFormular(bolMixed)
        Dim s() As String
        Dim i As Integer = 0
        Dim k As Integer

        'Methylen- und Methylgruppen in Seitenketten
        For Each sText In {")(", "(", ")C(", ")CH("}
            Dim j As Integer = sLat.IndexOf(sText, 0)
            Do While j > 0
                s = SplitInBrackets(sLat, j)
                Select Case i
                    Case 0
                        s(1) = s(1).Replace("CH3", IIf(s(1).IndexOf("CH2") > -1, "!{Mlr}", "!{Mlu}"))
                        s(1) = s(1).Substring(0, 3).Replace("CH2", "!{Mc}") & s(1).Substring(3)
                        s(1) = s(1).Replace("CH2", "!{Ma}")
                    Case 1
                        s(1) = s(1).Replace("CH3", IIf(s(1).IndexOf("CH2") > -1, "!{Mlr}", "!{Mlo}"))
                        s(1) = s(1).Substring(0, 3).Replace("CH2", "!{Mb}") & s(1).Substring(3)
                        s(1) = s(1).Replace("CH2", "!{Ma}")
                End Select
                sLat = String.Concat(s)
                j = sLat.IndexOf(sText, j + 1)
            Loop
            i += 1
        Next

        'erste C-C-Bindung kürzen bei benachbarten Seitenketten 
        Dim myReg As New Regex("CH{0,1}(\([Ma-z\{\}\!]{5,}\)){1,2}CH{0,1}\(")
        Dim matchc As MatchCollection = myReg.Matches(sLat)

        For Each m As Match In matchc
            i = sLat.IndexOf(m.Value)
            k = m.Value.Length
            If m.Value.IndexOf("CH") = -1 Then
#Disable Warning BC42104 ' Die Variable wurde verwendet, bevor ihr ein Wert zugewiesen wurde.
                s(0) = sLat.Substring(0, i) & m.Value.Replace("Mc", "Mck").Replace("Mlu", "Mluk")
#Enable Warning BC42104 ' Die Variable wurde verwendet, bevor ihr ein Wert zugewiesen wurde.
            Else
                s(0) = sLat.Substring(0, i + k)
            End If
            s(1) = sLat.Substring(i + k, 6).Replace("}", "k}")
            s(2) = sLat.Substring(i + k + 6)
            sLat = String.Concat(s)
        Next

        Dim myReg1 As New Regex("C\(([^C]+)(\)C\([^C]+){2}")
        Dim matchc1 As MatchCollection = myReg1.Matches(sLat)
        Dim myReg2 As New Regex("\)\(")
        Dim bolK As Boolean
        For Each m As Match In matchc1
            i = m.Value.IndexOf(")(")
            If m.Value.Substring(i, 8).Contains("Mck") Or m.Value.Substring(i, 8).Contains("Mluk") Then
                bolK = True
            Else
                bolK = False
            End If
            For j As Integer = 0 To 1
                i = m.Value.IndexOf(")(", i + 8)
                If m.Value.Substring(i, 8).Contains("Mck") Or m.Value.Substring(i, 8).Contains("Mluk") Then
                    If bolK And sLat.IndexOf(m.Value) > -1 Then                'zwei benachbarte verkürzte Seitenketten => zweite länger machen
                        'k = m.Value.Length
                        s(0) = sLat.Substring(0, sLat.IndexOf(m.Value)) & m.Value.Substring(0, i) & m.Value.Substring(i, 8).Replace("{Mck}", "{Mc}").Replace("{Mluk}", "{Mlu}")
                        s(1) = sLat.Substring(sLat.IndexOf(m.Value) + i + 6)
                        sLat = s(0) & s(1)
                        bolK = False
                    Else
                        bolK = True
                    End If
                Else
                    If Not bolK Then 'zwei benachbarte Seitenketten nicht verkürzt => zweite kürzer machen
                        'k = m.Value.Length
                        s(0) = sLat.Substring(0, sLat.IndexOf(m.Value)) & m.Value.Substring(0, i) & m.Value.Substring(i, 8).Replace("{Mc}", "{Mck}").Replace("{Mlu}", "{Mluk}")
                        s(1) = sLat.Substring(sLat.IndexOf(m.Value) + i + 8)
                        sLat = s(0) & s(1)
                        bolK = True
                    Else
                        bolK = False
                    End If
                End If
            Next
        Next
        'Methylgruppen
        sLat = sLat.Replace("CH2", "!{Ma}")
        sLat = "!{Mll}" & sLat.Substring(3)
        sLat = sLat.Replace("CH3", "!{Mlr}")

        'Verkürzung benachbarter Gruppen
        'myReg = New Regex("(C\([CH]+\([!\{\}a-zA-Z]+\))(!\{Ma\})")
        'sLat = myReg.Replace(sLat, "$1!{Mb}")
        'myReg = New Regex("(C\([CH]+\([!\{\}a-zA-Z]+\))(!\{Mlr\})")
        'sLat = myReg.Replace(sLat, "$1!{Mlo}")

        sLat = sLat.Replace("C(C(", "C(!{Ko}(")
        sLat = sLat.Replace("CH(C(", "CH(!{Ko}(")
        sLat = sLat.Replace("C(CH(", "C(!{KoH}(")
        sLat = sLat.Replace("CH(CH(", "CH(!{KoH}(")
        sLat = sLat.Replace("CH", "C!{Hu}")
        sLat = sLat.Replace("C", "-[::0]C")
        Debug.Print("Vorher:  " & sLat)
        'Sonderfälle
        myReg = New Regex("!{Ko}\([KoMalrbc\!\{\}]+\)\([KoMalrbc\!\{\}]+\)[KoMalrbc\!\{\}]+\)\([KoMalrbc\!\{\}]+\)\-\[::0")
        Dim myM As Match = myReg.Match(sLat)
        If myM.Value <> "" Then
            Dim p() As String = myM.Value.Split("("c)
            If CountStringOccurrences(p(1), "{") = 1 Then
                p(1) = "!{Mlo})"
            Else
                p(1) = "!{Mb}" & p(1).Substring(5)
            End If
            Dim q As String() = p(2).Split(")")
            Dim r As String() = q(0).Split("!")
            Select Case r.Length
                Case 1
                    q(0) = "!{Mlu}"
                Case 2
                    q(0) = "!{Mc}!{Mlo}"
                Case Else
                    q(0) = "!{Mc}!{Mb}"
                    For i = 0 To r.Length - 4
                        q(0) += "!{Ma}"
                    Next
                    q(0) += "!{Mlr}"
            End Select
            r = q(1).Split("!")
            Select Case r.Length
                Case 1
                    q(1) = "!{Mlr}"
                Case Else
                    q(1) = ""
                    For i = 0 To r.Length - 2
                        q(1) += "!{Ma}"
                    Next
                    q(1) += "!{Mlr}"
            End Select
            p(2) = q(0) & ")" & q(1) & ")"
            s(0) = sLat.Substring(0, sLat.IndexOf(myM.Value))
            s(0) += p(0) & "(" & p(1) & "(" & p(2) & "(" & p(3)
            s(1) = ",2" & sLat.Substring(sLat.IndexOf(myM.Value) + myM.Value.Length)
            sLat = s(0) & s(1)
        End If

        Return sLat

    End Function

    Function CountStringOccurrences(value As String, pattern As String) As Integer
        ' Count instances of pattern in value.
        Dim count As Integer = 0
        Dim i As Integer = 0
        While True
            ' Get next position of pattern.
            i = value.IndexOf(pattern, i)
            ' If not found, exit the loop.
            If i = -1 Then
                Exit While
            End If
            ' Record instance of the pattern, and skip past its characters.
            i += pattern.Length
            count += 1
        End While
        Return count
    End Function

    Public Function SplitInBrackets(sText As String, Optional iStart As Integer = 0, Optional sFirstString As String = "(", Optional sSecondString As String = ")") As String()

        Dim s(2) As String
        Dim i As String = sText.Length
        Dim j As Integer = sText.IndexOf(sFirstString, iStart)
        Dim k As Integer = sText.IndexOf(sSecondString, j + 1)

        s(0) = sText.Substring(0, j + sFirstString.Length)
        s(1) = sText.Substring(j + sFirstString.Length, k - j - 1)
        s(2) = sText.Substring(k)

        Return s

    End Function

    Public Sub FillFreeBondsWithAtomType(Optional iOrdnungsZahlFiller As Integer = 1)
        For i = 1 To lstNachbarAtome.Count - 1
            If lstNachbarAtome(i).OrdnungsZahl <> iOrdnungsZahlFiller Then
                lstNachbarAtome(i).FillFreeBondsWithAtomType(iOrdnungsZahlFiller)
            End If
        Next
        Dim aAtom As Atom
        For i = 1 To Bindigkeit() - lstNachbarAtome.Count
            aAtom = New Atom(iOrdnungsZahlFiller)
            lstNachbarAtome.Add(aAtom)
            aAtom.lstNachbarAtome.Add(Me)
        Next
    End Sub

    Public Sub RandomIsotope()
        lNeutronen = lstLstIsotope(GenerateRandomNumber(0, IsotopenListeZahl))
        SetNukleonenRadien()
    End Sub

    Public Sub MainIsotope()
        For j As Integer = 0 To lstIsotopnenHäufigkeit.Count - 1
            If lstIsotopnenHäufigkeit(j) = 100 Then
                lNeutronen = lstLstIsotope(j)
                SetNukleonenRadien()
                Exit For
            End If
        Next
    End Sub

    Public ReadOnly Property LatexCodePSEBahnenModell(Optional bolAlleNukleonen As Boolean = True,
                                          Optional bolProtonen As Boolean = True, Optional bolNeutronen As Boolean = True,
                                          Optional bolBahnen As Boolean = True, Optional bolElektronen As Boolean = True,
                                          Optional bolNeutronenColored As Boolean = True,
                                          Optional bolProtonenColored As Boolean = True,
                                          Optional bolElektronenColored As Boolean = True,
                                          Optional iZahlenStattKugelnNukleonenZahl As Integer = 20) As String
        Get
            Dim sLatex As String = ""
            Dim i As Integer

            SetElektronenColored(bolElektronenColored)
            SetProtonenColored(bolProtonenColored)
            SetNeutronenColored(bolNeutronenColored)
            Dim myDY As Double = lNukleonen(NukleonenAnzahl - 1).YPos
            Dim sDY As String = myDY.ToString("F", CultureInfo.InvariantCulture)
            Dim sKreis As String = (myDY + 1).ToString("F", CultureInfo.InvariantCulture)
            Dim sTextStart As String = "0.2"

            If bolAlleNukleonen Or (bolProtonen And bolNeutronen) Then
                If NukleonenAnzahl > iZahlenStattKugelnNukleonenZahl Then
                    sLatex += "\draw (0,0) circle [radius=" & sKreis & "];"
                    sLatex += "\node[text width=" & sKreis &
                        ", align=justify] at (0:-" & sTextStart & "){\Huge" &
                        IIf(bolProtonen, lProtonen.Count, "\underline{\hspace{0.5cm}}") & "\,\ce{p^{$\oplus$}}\par " &
                        IIf(bolNeutronen, lNeutronen.Count, "\underline{\hspace{0.5cm}}") & "\,n};"
                Else
                    For i = lNukleonen.Count - 1 To 0 Step -1
                        sLatex += lNukleonen(i).LatexCode & vbCrLf
                    Next
                End If
            Else
                If bolProtonen Then
                    If lProtonen.Count > iZahlenStattKugelnNukleonenZahl / 2 Then
                        sLatex += "\node[text width=" & sKreis &
                            ", align=justify] at (0:-" & sTextStart & "){\Huge" &
                            lProtonen.Count & "\,\ce{p^{$\oplus$}}\par " &
                            "\underline{\hspace{0.5cm}}\,n};"
                    Else
                        For i = lProtonen.Count - 1 To 0 Step -1
                            sLatex += lProtonen(i).LatexCode & vbCrLf
                        Next
                    End If
                End If
                If bolNeutronen Then
                    If lNeutronen.Count > iZahlenStattKugelnNukleonenZahl / 2 Then
                        sLatex += "\node[text width=" & sKreis &
                            ", align=justify] at (0:-" & sTextStart & "){\Huge\underline{\hspace{0.5cm}}\,\ce{p^{$\oplus$}}\par " &
                            lNeutronen.Count & "\,n};"
                    Else
                        For i = lNeutronen.Count - 1 To 0 Step -1
                            sLatex += lNeutronen(i).LatexCode & vbCrLf
                        Next
                    End If
                End If
            End If


            If bolBahnen Then
                For i = 1 To GetBahnNummer(lElektronen.Count + 1)
                    sLatex += "\draw[draw=blue] (0,0) circle [radius=" &
                        (myDY + 1 + i * 0.7).ToString("F", CultureInfo.InvariantCulture) & "];" & vbCrLf
                Next
            End If

            If bolElektronen Then
                For i = 0 To lElektronen.Count - 1
                    sLatex += lElektronen(i).LatexCode & vbCrLf
                Next
            End If
            Return sLatex
        End Get
    End Property
    Public Sub SetNeutronenColored(bolColored As Boolean)
        If bolColored Then
            For i = 0 To lNeutronen.Count - 1
                lNeutronen(i).Colored = True
            Next
        Else
            For i = 0 To lNeutronen.Count - 1
                lNeutronen(i).Colored = False
            Next
        End If
    End Sub

    Public Sub SetProtonenColored(bolColored As Boolean)
        If bolColored Then
            For i = 0 To lProtonen.Count - 1
                lProtonen(i).Colored = True
            Next
        Else
            For i = 0 To lProtonen.Count - 1
                lProtonen(i).Colored = False
            Next
        End If
    End Sub

    Public Sub SetElektronenColored(bolColored As Boolean)
        If bolColored Then
            For i = 0 To lElektronen.Count - 1
                lElektronen(i).Colored = True
            Next
        Else
            For i = 0 To lElektronen.Count - 1
                lElektronen(i).Colored = False
            Next
        End If
    End Sub

    Public ReadOnly Property LatexCodeAtomSymbol(Optional bolMasseZahl As Boolean = False,
                                                 Optional bolOrdnungsZahl As Boolean = False,
                                                 Optional bolLadung As Boolean = True,
                                                 Optional bolSymbol As Boolean = True,
                                                 Optional iLadungsBetrag As Integer = -1) As String
        Get
            Dim myLatexString As String = "\huge\ce{"
            If bolMasseZahl Then
                myLatexString += "^{" & Me.NukleonenAnzahl & "}"
            Else
                myLatexString += "^{\underline{\hspace{0.4cm}}}"
            End If
            If bolOrdnungsZahl Then
                myLatexString += "_{" & Me.OrdnungsZahl & "}"
            Else
                myLatexString += "_{\underline{\hspace{0.4cm}}}"
            End If
            myLatexString += IIf(bolSymbol, Me.Symbol, "\underline{\hspace{0.5cm}}")

            If iLadungsBetrag = -1 Then iLadungsBetrag = Me.lProtonen.Count - Me.lElektronen.Count

            If bolLadung And iLadungsBetrag <> 0 Then
                Select Case iLadungsBetrag
                    Case 1
                        myLatexString += "^+"
                    Case -1
                        myLatexString += "^-"
                    Case Else
                        myLatexString += "^{" & iLadungsBetrag.ToString("+#;-#;0") & "}"
                End Select
            End If
            myLatexString += "}"
            Return myLatexString
        End Get
    End Property

    Private Function LastKernKreisNummer() As Integer
        If Me.NukleonenAnzahl > 0 Then
            Return GetKreisnumber(Me.NukleonenAnzahl)
        Else
            Return -1
        End If
    End Function

    Private Function LastKernKreisNukleonenZahl() As Integer
        If Me.NukleonenAnzahl > 0 Then
            Return Me.NukleonenAnzahl - NukleonenBisKreisNumberVoll(LastKernKreisNummer() - 1)
        Else
            Return -1
        End If
    End Function

    Private Sub CreateNukleonenList()
        lNukleonen = New List(Of Teilchen)
        Dim longerList, shorterList As List(Of Teilchen)
        If lProtonen.Count < lNeutronen.Count Then
            shorterList = lProtonen
            longerList = lNeutronen
        Else
            shorterList = lNeutronen
            longerList = lProtonen
        End If
        If shorterList.Count = 0 Then
            lNukleonen = longerList
            Exit Sub
        End If
        Dim listLengthRatio As Double = longerList.Count / shorterList.Count
        Dim longListCount As Integer = 0
        Dim shortListCount As Integer = 0

        Do While (longListCount < longerList.Count Or shortListCount < shorterList.Count)
            Do While (longListCount < shortListCount * listLengthRatio)
                If (longListCount < longerList.Count) Then
                    lNukleonen.Add(longerList(longListCount))
                    longListCount += 1
                Else
                    Exit Do
                End If
            Loop

            If (shortListCount < shorterList.Count) Then
                lNukleonen.Add(shorterList(shortListCount))
                shortListCount += 1
            End If
        Loop
    End Sub

    Private Sub SetNukleonenRadien()
        CreateNukleonenList()
        Dim dRadius() As Double
        For i = 0 To lNukleonen.Count - 1
            dRadius = RadiusNukleonByNumber(i + 1)
            lNukleonen(i).XPos = dRadius(0)
            lNukleonen(i).YPos = dRadius(1)
        Next
    End Sub

    Private Sub setElektronenRadien()
        Dim dRadius() As Double
        For i = 0 To lElektronen.Count - 1
            dRadius = RadiusElektronByNumber(i + 1)
            lElektronen(i).XPos = dRadius(0)
            lElektronen(i).YPos = dRadius(1)
        Next
    End Sub

    Private Function GetKreisnumber(iNumberOfNukleon As Integer) As Integer
        If iNumberOfNukleon = 1 Then Return 1
        Dim iCounter As Integer = 1
        Dim iNukleonenZahl As Integer = 1
        While iNumberOfNukleon > iNukleonenZahl
            iCounter += 1
            iNukleonenZahl += NukleonenOfKreis(iCounter)
        End While
        Return iCounter
    End Function

    Private Function NukleonenBisKreisNumberVoll(iKreisnumber As Integer) As Integer
        Dim iNukNumber As Integer = 0
        For i = 1 To iKreisnumber
            iNukNumber += NukleonenOfKreis(i)
        Next
        Return iNukNumber
    End Function

    Private Function NukleonenOfKreis(iKreisnumber As Integer) As Integer
        If iKreisnumber < 1 Then Return 0
        If iKreisnumber = 1 Then Return 1
        Return (iKreisnumber - 1) * 6
    End Function

    Private Function RadiusNukleonByNumber(iNumberOfNukleon As Integer) As Double()
        Dim iKreisNumber As Integer = GetKreisnumber(iNumberOfNukleon)
        Dim iSegmentNumber As Integer = iNumberOfNukleon - NukleonenBisKreisNumberVoll(iKreisNumber - 1)
        Dim dWinkel As Double = 0
        Dim dRadius As Double = (iKreisNumber - 1) * 0.33
        If iKreisNumber <> LastKernKreisNummer() Then
            dWinkel = 360 * iSegmentNumber / NukleonenOfKreis(iKreisNumber) + iKreisNumber * 15
        Else
            If Me.LastKernKreisNukleonenZahl() > 0 Then
                dWinkel = 360 * iSegmentNumber / Me.LastKernKreisNukleonenZahl
            Else
                dWinkel = 0
            End If
        End If
        Dim myResult() As Double = {dWinkel, dRadius}
        'Dim myResult() As Double = {dWinkel * (1 + GenerateRandomNumber() / 10000), dRadius * (1 + GenerateRandomNumber() / 50000)}
        Return myResult
    End Function

    Private Function GetBahnNummer(iNummerOfElektron As Integer)
        If iNummerOfElektron > 118 Then Return 8
        Dim i As Integer = 1
        While iNummerOfElektron > myPSEData.ElektronenBisPeriode(i - 1)
            i += 1
        End While
        Return i
    End Function

    Private Function GetNumberOfElektronInBahn(iNumberOfElektron As Integer) As Integer
        If iNumberOfElektron < 3 Then Return iNumberOfElektron
        Return myPSEData.ElektronenBisPeriode(GetBahnNummer(iNumberOfElektron) - 1) - iNumberOfElektron - 1
    End Function

    Private Function RadiusElektronByNumber(iNumberOfElektron As Integer) As Double()
        Dim dRadius As Double = lNukleonen(lNukleonen.Count - 1).YPos + 1 + GetBahnNummer(iNumberOfElektron) * 0.7
        Dim dWinkel As Double = GetNumberOfElektronInBahn(iNumberOfElektron) * 360 / myPSEData.ElektronenPerPeriode(GetBahnNummer(iNumberOfElektron) - 1)
        Dim myResult() As Double = {dWinkel, dRadius}
        Return myResult
    End Function

    Public Function GetNumberOfAußenElektronen() As Integer
        Return GetNumberOfElektronInBahn(lElektronen.Count - 1)
    End Function

    Public Function Bindigkeit() As Integer
        Return myPSEData.ElektronenPerPeriode(Periode - 1) - GetNumberOfAußenElektronen()
    End Function

    Function GenerateRandomNumber() As Double
        Static Random_Number As New Random()
        Return Random_Number.Next(-100, 100)
    End Function

    Private Random_Number As New Random()

    Function GenerateRandomNumber(iVon As Integer, iBis As Integer) As Integer
        If iVon = iBis Then Return iVon

        Dim iRand As Integer
        If iVon < iBis Then
            iRand = Random_Number.Next(iVon, iBis)
        Else
            iRand = Random_Number.Next(iBis, iVon)
        End If
        Return iRand
    End Function

    Public Function Clone() As Object Implements ICloneable.Clone

        Return New Atom(Me.iOrdnungsZahl)

    End Function
End Class