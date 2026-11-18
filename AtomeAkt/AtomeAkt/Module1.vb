Imports System.Globalization
Imports System.IO
Imports System.Xml.Linq
Imports System.Runtime.ConstrainedExecution

Module Module1

    ' Maschinenabhängige Pfade stehen in der lokalen AtomeAkt.local.xml.
    Private Function LokalerPfad(schluessel As String, standardPfad As String) As String
        Dim datei As String = Path.Combine(AppContext.BaseDirectory, "AtomeAkt.local.xml")
        If Not File.Exists(datei) Then Return standardPfad

        Dim eintrag = XDocument.Load(datei).Root.Elements("add").
            FirstOrDefault(Function(element) CStr(element.Attribute("key")) = schluessel)
        Return If(CStr(eintrag?.Attribute("value")), standardPfad)
    End Function

    Const UnifyPDFStart As String = "\documentclass[12pt,a4paper]{article}" & vbCrLf & "\usepackage{pdfpages}" & vbCrLf & "\begin{document}"
    Const UnifyPDFEnd As String = "\end{document}"
    Public dieDS As New DataSources
    Public dasPSE As New PSE


    Sub Main()

        'Debug.Print(LatexUebungAtommodell(1, 15, 15, True)(0))
        'SalzBildung()
        'CreateLEK()
        'CreateLEKAtomBau()
        CreateTestAtomBindung()
        'CreateAtommodellÜbung()
        'CreateLEKMolUndSaeure()
        'CreateLEKAlkane()
        'Dim myAt As New Atom(10)
        'Debug.Print(myAt.LatexCodePSEBahnenModell(True, True, True, True, True, True, True, True, 5))
        'Dim myKWST As Kohlenwasserstoff
        'Dim sRes(50) As String
        'Dim filePath As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\KWSt.csv"
        'Using writer As New System.IO.StreamWriter(filePath, True)
        '    For i = 1 To 2000
        '        myKWST = New Kohlenwasserstoff
        '        myKWST.CreateAndAppendSeitenketten(3, {1, 10}, 6, 11)
        '        myKWST.Kettenstart.FillFreeBondsWithAtomType(1)
        '        writer.WriteLine(myKWST.Name & ";" & myKWST.Kettenstart.SimpleFormular & ";" & myKWST.Kettenstart.LatexFromSimpleChainFormular)
        '        Debug.Print(myKWST.Kettenstart.AggregateFormular)
        '        'writer.WriteLine("%+++++++++++++++++++++++++")
        '        'Debug.Print(myKWST.Kettenstart.SimpleFormular(True))
        '        'Debug.Print(myKWST.Kettenstart.AggregateFormularMixed)
        '        'sRes(i) = "\chemname{\chemfig{" & myKWST.Kettenstart.LatexFromSimpleChainFormular & "}}{" & myKWST.Name & "}"
        '        'Debug.Print(myKWST.Kettenstart.LatexFromSimpleChainFormular(True))
        '    Next
        'End Using

        'For i = 1 To 5
        '    Debug.Print(sRes(i))
        'Next i


    End Sub

    Public Sub CreateAtommodellÜbung()

        Dim sInputFilePath As String = LokalerPfad("CreateAtommodellÜbung.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "PeriodensystemUndAtombauÜbungenVorlage.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-MM-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei

        If Not File.Exists(sPath) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)
        If Not Directory.Exists(sOutPutFolder & "\Pics") And Directory.Exists(sInputFilePath & "\Pics") Then
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sInputFilePath & "\Pics", sOutPutFolder & "\Pics", True)
        End If

        Dim sSource As String = File.ReadAllText(sPath)
        Dim sAtomModelle() As String = LatexUebungAtommodell()
        sSource = sSource.Replace("XXXXX", sAtomModelle(0))
        sSource = sSource.Replace("YYYYY", sAtomModelle(1))
        Dim sOutPutFile As String = sOutPutFolder & "\PeriodensystemUndAtombauÜbungen-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sSource)

        Dim sCommand As String = """" & LokalerPfad("CreateAtommodellÜbung.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        Dim shell As Object = CreateObject("WScript.Shell")
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub

    Public Function LatexUebungAtommodell(Optional iNumberOfModells As Integer = 22,
                                          Optional iMinOrdnungszahl As Integer = 1,
                                          Optional iMaxOrdnungszahl As Integer = 30,
                                          Optional bolIsotope As Boolean = True) As String()
        Dim iMinOZ, iMaxOZ As Integer
        If iMinOrdnungszahl > iMaxOrdnungszahl Then
            iMinOZ = iMaxOrdnungszahl
            iMaxOZ = iMinOrdnungszahl
        Else
            iMinOZ = iMinOrdnungszahl
            iMaxOZ = iMaxOrdnungszahl
        End If

        If iMinOZ < 1 Or iMinOZ > 118 Or iMaxOZ < 1 Or iMaxOZ > 118 Then Return {"", ""}

        If bolIsotope Then
            For i As Integer = 0 To dieDS.IsotopenListe.Count - 1
                If iMinOZ = dieDS.IsotopenListe(i)(0) Then
                    iMinOZ = i
                    Exit For
                End If
            Next
            For i = dieDS.IsotopenListe.Count - 1 To 0 Step -1
                If iMaxOZ = dieDS.IsotopenListe(i)(0) Then
                    iMaxOZ = i
                    Exit For
                End If
            Next
        End If

        If (iMaxOZ - iMinOZ) + 1 < iNumberOfModells Then Return {"", ""}
        Dim oAt As Atom
        Dim sLat(1) As String
        sLat(0) = "\begin{longtable}{m{5.1cm}m{2cm}m{0.5cm}m{5.1cm}m{2cm}}" & vbCrLf
        sLat(0) += "	\centering Atommodell & \centering Symbol & & \centering Atommodell & Symbol\\\hline & & & & \\\endhead"
        sLat(1) = sLat(0).Clone
        Dim sTik, sTikLsg As String
        Dim WertListe As New List(Of Integer)
        Dim sModells() As String
        For i = 1 To iNumberOfModells Step 2
            WertListe.Add(GenerateRandomNumberOhneDopplung(iMinOZ, iMaxOZ, WertListe))
            If bolIsotope Then
                oAt = New Atom(WertListe(i - 1), False)
            Else
                oAt = New Atom(WertListe(i - 1))
            End If
            sTikLsg = "\resizebox{5cm}{!}{" & vbCrLf & "\begin{tikzpicture}" & vbCrLf
            sTikLsg += oAt.LatexCodePSEBahnenModell(True, True, True, True, True, True, True, True, 20)
            sTikLsg += "\end{tikzpicture}} & " & oAt.LatexCodeAtomSymbol(True, True, False, True) & " &&"

            sModells = InCompleteModells(GenerateRandomNumber(1, 7), oAt)
            sTik = "\resizebox{5cm}{!}{" & vbCrLf & "\begin{tikzpicture}" & vbCrLf
            sTik += sModells(0) & vbCrLf
            sTik += "\end{tikzpicture}} & " & sModells(1) & " &&"

            WertListe.Add(GenerateRandomNumberOhneDopplung(iMinOZ, iMaxOZ, WertListe))
            If bolIsotope Then
                oAt = New Atom(WertListe(i), False)
            Else
                oAt = New Atom(WertListe(i))
            End If

            sTikLsg += "\resizebox{5cm}{!}{" & vbCrLf & "\begin{tikzpicture}" & vbCrLf
            sTikLsg += oAt.LatexCodePSEBahnenModell(True, True, True, True, True, True, True, True, 20)
            sTikLsg += "\end{tikzpicture}} & " & oAt.LatexCodeAtomSymbol(True, True, False, True) & " \\" & vbCrLf

            sModells = InCompleteModells(GenerateRandomNumber(1, 7), oAt)
            sTik += "\resizebox{5cm}{!}{" & vbCrLf & "\begin{tikzpicture}" & vbCrLf
            sTik += sModells(0) & vbCrLf
            sTik += "\end{tikzpicture}} & " & sModells(1) & "\\" & vbCrLf
            sLat(0) += sTik
            sLat(1) += sTikLsg
        Next
        sLat(0) += "\end{longtable}"
        sLat(1) += "\end{longtable}"
        Return sLat
    End Function

    Public Sub SalzBildung(Optional iNumberOfPaare As Integer = 120)
        If iNumberOfPaare < 1 Or iNumberOfPaare > 136 Then Exit Sub

        Dim oPSE As New PSE
        Dim oKat, oAn As Atom
        Dim sLat As String = "\begin{longtable}{|l|c|c|p{2cm}|p{2cm}|p{4cm}|p{5cm}|}" & vbCrLf
        sLat += "\hline Elementpaar & $\chi_1$ & $\chi_2$ & Kation & Anion & "
        sLat += "Formel & Name \\\hline & & & & & & \\\endhead" & vbCrLf

        Dim iAnionen As Integer = oPSE.lHauptgruppeAnionenBildner.Count
        Dim iKationen As Integer = oPSE.lHauptgruppeKationenBildner.Count
        Dim WertListe As New List(Of Array)
        For i = 1 To iNumberOfPaare
            WertListe.Add(GenerateRandomNumberPaarOhneDopplung(0, iKationen, 0, iAnionen, WertListe))
            oKat = oPSE.lHauptgruppeKationenBildner(WertListe(i - 1)(0))
            oAn = oPSE.lHauptgruppeAnionenBildner(WertListe(i - 1)(1))
            If GenerateRandomNumber(0, 2) = 0 Then
                sLat += oKat.Symbol & ", " & oAn.Symbol & "&" & oKat.Elektronegativität & " & " & oAn.Elektronegativität
            Else
                sLat += oAn.Symbol & ", " & oKat.Symbol & "&" & oAn.Elektronegativität & " & " & oKat.Elektronegativität
            End If
            sLat += "&&&&\\\hline" & "----" & oKat.LadungsbetragHGIon & "/" & oAn.LadungsbetragHGIon & "-----" & SalzeSchwierigkeit(oKat, oAn) & vbCrLf
        Next
        sLat += "\end{longtable}"
        Debug.Print(sLat)

    End Sub

    Public Function SalzPaarListe() As List(Of SalzPaar)

        Dim oPSE As PSE = dasPSE
        Dim oKat, oAn As Atom
        Dim theSalzPaar As New List(Of SalzPaar)

        Dim iAnionen As Integer = oPSE.lHauptgruppeAnionenBildner.Count
        Dim iKationen As Integer = oPSE.lHauptgruppeKationenBildner.Count
        Dim WertListe As New List(Of Array)
        For i = 1 To 136
            WertListe.Add(GenerateRandomNumberPaarOhneDopplung(0, iKationen, 0, iAnionen, WertListe))
            oKat = oPSE.lHauptgruppeKationenBildner(WertListe(i - 1)(0))
            oAn = oPSE.lHauptgruppeAnionenBildner(WertListe(i - 1)(1))
            theSalzPaar.Add(New SalzPaar(SalzeSchwierigkeit(oKat, oAn), oAn, oKat))
        Next

        Return theSalzPaar

    End Function

    Public Function OxidListe() As List(Of SalzPaar)

        Dim oPSE As PSE = dasPSE
        Dim oKat, oAn As Atom
        Dim theSalzPaar As New List(Of SalzPaar)
        oAn = New Atom(8)

        Dim iKationen As Integer = oPSE.lHauptgruppeKationenBildner.Count
        Dim WertListe As New List(Of Array)
        For i = 1 To 60
            WertListe.Add(GenerateRandomNumberPaarOhneDopplung(0, iKationen, 0, 10, WertListe))
            oKat = oPSE.lHauptgruppeKationenBildner(WertListe(i - 1)(0))
            theSalzPaar.Add(New SalzPaar(SalzeSchwierigkeit(oKat, oAn), oAn, oKat))
        Next

        Return theSalzPaar

    End Function

    Private Function InCompleteModells(iChoce As Integer, theAtom As Atom) As String()
        Dim sLatex(1) As String
        Select Case iChoce
            'Nur Elektronen und Massezahl
            Case 1
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(False, False, False, True, True, False, False, True)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(True, False, True, False)
            'Protonen und Neutronen
            Case 2
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(True, True, True, True, False)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(False, False, True, False)
            'Symbol und Massezahl
            Case 3
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(False, False, False, True, False, False, False, False)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(True, False, True, True)
            'Ordnungszahl und Neutronen
            Case 4
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(False, False, True, True, False, True, False, True)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(False, True, True, False)
            'Elektronen und Neutronen
            Case 5
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(False, False, True, True, True)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(False, False, True, False)
            'Protonen und Massezahl
            Case 6
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(False, True, False, True, False)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(False, False, True, False)
            'Ordnungszahl und Massezahl
            Case 7
                sLatex(0) = theAtom.LatexCodePSEBahnenModell(False, False, False, True, False)
                sLatex(1) = theAtom.LatexCodeAtomSymbol(True, True, True, False)
        End Select
        Return sLatex

    End Function

    Private Function SalzeSchwierigkeit(oAtom1 As Atom, ByVal oAtom2 As Atom) As Integer
        If oAtom1.LadungsbetragHGIon = oAtom2.LadungsbetragHGIon Then Return 1
        Dim iG1, iG2 As Integer
        iG1 = oAtom1.LadungsbetragHGIon
        iG2 = oAtom2.LadungsbetragHGIon

        Select Case iG1
            Case 1
                Return 2
            Case 2
                Select Case iG2
                    Case 1, 4
                        Return 2
                    Case 3
                        Return 3
                End Select
            Case 3
                Select Case iG2
                    Case 1
                        Return 2
                    Case 2, 4
                        Return 3
                End Select
            Case 4
                Select Case iG2
                    Case 1, 2
                        Return 2
                    Case 3
                        Return 3
                End Select
        End Select
        Return 0
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

    Function GenerateRandomNumberOhneDopplung(iVon As Integer, iBis As Integer, lBisherigeWerte As List(Of Integer)) As Integer
        If iVon = iBis Then Return iVon
        If Math.Abs(iBis - iVon) <= lBisherigeWerte.Count Then Return 0

        Static Random_Number As New Random()
        Dim iRand As Integer
        Do
            If iVon < iBis Then
                iRand = Random_Number.Next(iVon, iBis)
            Else
                iRand = Random_Number.Next(iBis, iVon)
            End If
        Loop Until bolNumberIsInListe(iRand, lBisherigeWerte) = False
        Return iRand
    End Function

    Function GenerateRandomNumberPaarOhneDopplung(iVon1 As Integer, iBis1 As Integer,
                                                  iVon2 As Integer, iBis2 As Integer,
                                                  lBisherigeWerte As List(Of Array)) As Array
        If iVon1 = iBis1 And iVon2 = iBis2 Then Return {iVon1, iVon2}
        If Math.Abs(iBis1 - iVon1) * Math.Abs(iBis2 - iVon2) <= lBisherigeWerte.Count Then Return {0, 0}

        Static Random_Number As New Random()
        Dim iRand1, iRand2 As Integer
        Do
            If iVon1 < iBis1 Then
                iRand1 = Random_Number.Next(iVon1, iBis1)
            Else
                iRand1 = Random_Number.Next(iBis1, iVon1)
            End If
            If iVon2 < iBis2 Then
                iRand2 = Random_Number.Next(iVon2, iBis2)
            Else
                iRand2 = Random_Number.Next(iBis2, iVon2)
            End If
        Loop Until bolPaarIsInListe({iRand1, iRand2}, lBisherigeWerte) = False
        Return {iRand1, iRand2}
    End Function

    Private Function bolPaarIsInListe(iArray As Array, lArrayListe As List(Of Array)) As Boolean
        Dim bolIsDrin As Boolean = False
        For i As Integer = 0 To lArrayListe.Count - 1
            If (iArray(0) = lArrayListe(i)(0)) And (iArray(1) = lArrayListe(i)(1)) Then
                bolIsDrin = True
                Exit For
            End If
        Next
        Return bolIsDrin
    End Function


    Private Function bolNumberIsInListe(iNumber As Integer, lIntegerListe As List(Of Integer)) As Boolean
        Dim bolIsDrin As Boolean = False
        For i As Integer = 0 To lIntegerListe.Count - 1
            If iNumber = lIntegerListe(i) Then
                bolIsDrin = True
                Exit For
            End If
        Next
        Return bolIsDrin
    End Function

    Private Function KlassenListe(sKlasseCSV As String) As List(Of Schüler)
        Dim myList As New List(Of Schüler)
        If File.Exists(sKlasseCSV) Then
            Dim sCSV As String = File.ReadAllText(sKlasseCSV)
            Dim myRows() As String = sCSV.Split(vbLf)
            Dim sKlasse As Array = myRows.Select(Function(x) x.Split(";")).ToArray()
            For i As Integer = 1 To UBound(sKlasse) - 1
                myList.Add(New Schüler(sKlasse(i)))
            Next
        End If

        Return myList
    End Function


    Public Sub CreateLEKSalze()

        Dim sKlasseCSV As String = LokalerPfad("CreateLEKSalze.Klassenliste", "Ch9c.csv")
        Dim sInputFilePath As String = LokalerPfad("CreateLEKSalze.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "LEKSalzeVorlage.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-mm-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei
        Dim sUnifiedPDF As String = UnifyPDFStart
        If Not File.Exists(sPath) Then Exit Sub
        If Not File.Exists(sKlasseCSV) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)

        Dim sSource As String = File.ReadAllText(sPath)
        Dim lKlasse As List(Of Schüler) = KlassenListe(sKlasseCSV)
        Dim lSalzPaar As List(Of SalzPaar) = SalzPaarListe()

        Dim sLeK As String
        Dim sLatexSalzPaar As String
        Dim sOutPutFile As String
        Dim sFileName As String
        Dim sCommand As String = """" & LokalerPfad("CreateLEKSalze.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        'MsgBox(sCommand)
        Dim shell As Object = CreateObject("WScript.Shell")

        For Each sSchüler As Schüler In lKlasse
            sLeK = sSource.Replace("YYYYY", sSchüler.Vorname & " " & sSchüler.Name & "/" & sSchüler.Klasse)
            sLatexSalzPaar = ""
            For Each sSalzPaar As SalzPaar In lSalzPaar
                If sSalzPaar.Schwierigkeit = 1 Then
                    sLatexSalzPaar += sSalzPaar.LatexString
                    lSalzPaar.Remove(sSalzPaar)
                    Exit For
                End If
            Next
            For Each sSalzPaar As SalzPaar In lSalzPaar
                If sSalzPaar.Schwierigkeit = 2 Then
                    sLatexSalzPaar += sSalzPaar.LatexString
                    lSalzPaar.Remove(sSalzPaar)
                    Exit For
                End If
            Next

            sLeK = sLeK.Replace("XXXXX", sLatexSalzPaar)
            sLeK = sLeK.Replace("ZZZZZ", MineralWasserGehalt())
            sFileName = sSchüler.Vorname & "-" & sSchüler.Name & "-LEK.tex"
            sOutPutFile = sOutPutFolder & "\" & sFileName
            File.WriteAllText(sOutPutFile, sLeK)
            sUnifiedPDF += LatexFileToUnifiedPDF(sFileName.Replace(".tex", ".pdf"))
            'MsgBox(sCommand & " """ & sOutPutFile & """")
            shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)
            While IsProcessRunning("pdflatex.exe")
                Threading.Thread.Sleep(5000)
            End While
        Next
        sUnifiedPDF += UnifyPDFEnd
        sOutPutFile = sOutPutFolder & "\LEK-" & lKlasse.First.Klasse & "-all-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sUnifiedPDF)
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub

    Public Sub CreateLEKAtomBau()

        Dim sKlasseCSV As String = LokalerPfad("CreateLEKAtomBau.Klassenliste", "Ch9a.csv")
        Dim sInputFilePath As String = LokalerPfad("CreateLEKAtomBau.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "LEKAtomeVorlage.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-MM-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei
        Dim sUnifiedPDF As String = UnifyPDFStart
        If Not File.Exists(sPath) Then Exit Sub
        If Not File.Exists(sKlasseCSV) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)
        If Not Directory.Exists(sOutPutFolder & "\Pics") And Directory.Exists(sInputFilePath & "\Pics") Then
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sInputFilePath & "\Pics", sOutPutFolder & "\Pics", True)
        End If

        Dim sSource As String = File.ReadAllText(sPath)
        Dim lKlasse As List(Of Schüler) = KlassenListe(sKlasseCSV)


        Dim sLeK As String
        Dim sOutPutFile As String
        Dim sFileName As String
        Dim sCommand As String = """" & LokalerPfad("CreateLEKAtomBau.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        'MsgBox(sCommand)
        Dim shell As Object = CreateObject("WScript.Shell")
        Dim oAt(4) As Atom
        Dim WertListe As List(Of Integer)
        Dim sTIKZ As String = "	{\resizebox*{5cm}{!}{\begin{tikzpicture}" & vbCrLf
        Dim sTIKZ1 As String = "\end{tikzpicture}}}" & vbCrLf
        For Each sSchüler As Schüler In lKlasse
            WertListe = New List(Of Integer)
            For i As Integer = 0 To 4
                WertListe.Add(GenerateRandomNumberOhneDopplung(3, 18, WertListe))
            Next
            For i As Integer = 0 To 4
                oAt(i) = New Atom(WertListe(i))
            Next i
            sLeK = sSource.Replace("XXXXX", sSchüler.Vorname & " " & sSchüler.Name & "/" & sSchüler.Klasse)

            sLeK = sLeK.Replace("YYYYY", oAt(0).Name)
            sLeK = sLeK.Replace("ZZZZZ", sTIKZ & oAt(1).LatexCodePSEBahnenModell(False, False, False, True, True,,,, 1) & sTIKZ1)
            sLeK = sLeK.Replace("WWWWW", sTIKZ & oAt(2).LatexCodePSEBahnenModell(False, False, False, True, False,,,, 1) & sTIKZ1)
            sLeK = sLeK.Replace("UUUUU", oAt(2).Symbol)
            sLeK = sLeK.Replace("VVVVV", sTIKZ & oAt(3).LatexCodePSEBahnenModell(False, True, True, True, False,,,, 1) & sTIKZ1)
            sLeK = sLeK.Replace("PPPPP", oAt(4).Name)
            sFileName = sSchüler.Vorname & "-" & sSchüler.Name & "-LEK.tex"
            sOutPutFile = sOutPutFolder & "\" & sFileName
            File.WriteAllText(sOutPutFile, sLeK)
            sUnifiedPDF += LatexFileToUnifiedPDF(sFileName.Replace(".tex", ".pdf"))
            'MsgBox(sCommand & " """ & sOutPutFile & """")
            shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)
            While IsProcessRunning("pdflatex.exe")
                Threading.Thread.Sleep(5000)
            End While
        Next
        sUnifiedPDF += UnifyPDFEnd
        sOutPutFile = sOutPutFolder & "\LEK-" & lKlasse.First.Klasse & "-all-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sUnifiedPDF)
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub

    'Public Function GetRandomChemfigFormulaA(chemfigFormeln As String(), listOfHauptgruppenElemente As List(Of Atom)) As String
    '    Dim rnd As New Random()

    '    ' 1. Zufällige Formel auswählen
    '    Dim formula As String = chemfigFormeln(rnd.Next(chemfigFormeln.Length))

    '    ' 2. Mapping: alte Nummer → echte Hauptgruppe
    '    Dim groupMap As New Dictionary(Of Integer, Integer) From {
    '    {4, 14}, {5, 15}, {6, 16}, {7, 17}
    '}

    '    ' 3. Platzhalter finden (Regex: X[4-7][a-z]?)
    '    Dim matches = System.Text.RegularExpressions.Regex.Matches(formula, "X([4-7])[a-z]?")

    '    ' 4. Zufallslisten für jede echte Hauptgruppe
    '    Dim selectedElements As New Dictionary(Of Integer, List(Of Atom))
    '    For Each oldGroup In matches.Cast(Of System.Text.RegularExpressions.Match)().
    '    Select(Function(m) Integer.Parse(m.Groups(1).Value)).
    '    Distinct()

    '        Dim realGroup = groupMap(oldGroup)
    '        Dim gruppeElements = listOfHauptgruppenElemente.
    '        Where(Function(e) e.Gruppe = realGroup AndAlso e.Periode >= 2 AndAlso e.Periode <= 4).
    '        ToList()

    '        Dim zufallsListe As New List(Of Atom)
    '        For i As Integer = 1 To 6
    '            zufallsListe.Add(gruppeElements(rnd.Next(gruppeElements.Count)))
    '        Next
    '        selectedElements(realGroup) = zufallsListe
    '    Next

    '    ' 5. Ersetzen jedes Platzhalters einzeln
    '    For Each match In matches
    '        Dim placeholder = match.Value
    '        Dim oldGroup = Integer.Parse(match.Groups(1).Value)
    '        Dim realGroup = groupMap(oldGroup)
    '        Dim zufallsListe = selectedElements(realGroup)
    '        formula = formula.Replace(placeholder, zufallsListe(rnd.Next(zufallsListe.Count)).Symbol)
    '    Next

    '    Return formula
    'End Function

    Public Function GetRandomChemfigFormulaA(chemfigFormeln As String(), listOfHauptgruppenElemente As List(Of Atom)) As String
        Dim rnd As New Random()

        ' 1. Zufällige Formel auswählen
        Dim formula As String = chemfigFormeln(rnd.Next(chemfigFormeln.Length))

        ' 2. Mapping: alte Nummer → echte Hauptgruppe
        Dim groupMap As New Dictionary(Of Integer, Integer) From {
        {4, 14}, {5, 15}, {6, 16}, {7, 17}
    }

        ' 3. Platzhalter finden (Regex: X[4-7][a-z]?)
        Dim matches = System.Text.RegularExpressions.Regex.Matches(formula, "X([4-7])[a-z]?")

        ' 4. Zufallslisten für jede echte Hauptgruppe
        Dim selectedElements As New Dictionary(Of Integer, List(Of Atom))
        For Each oldGroup In matches.Cast(Of System.Text.RegularExpressions.Match)().
        Select(Function(m) Integer.Parse(m.Groups(1).Value)).
        Distinct()

            Dim realGroup = groupMap(oldGroup)
            Dim gruppeElements = listOfHauptgruppenElemente.
            Where(Function(e) e.Gruppe = realGroup AndAlso e.Periode >= 2 AndAlso e.Periode <= 4).
            ToList()

            Dim zufallsListe As New List(Of Atom)
            For i As Integer = 1 To 6
                zufallsListe.Add(gruppeElements(rnd.Next(gruppeElements.Count)))
            Next
            selectedElements(realGroup) = zufallsListe
        Next

        ' 5. Wähle ein zufälliges Platzhalter-Vorkommen für den Fehler
        Dim errorIndex = rnd.Next(matches.Count)
        Dim errorMatch = matches(errorIndex)
        Dim errorOldGroup = Integer.Parse(errorMatch.Groups(1).Value)
        Dim errorRealGroup = groupMap(errorOldGroup)

        ' Falsche Gruppe für den Fehler bestimmen
        Dim wrongGroups = groupMap.Values.Where(Function(g) g <> errorRealGroup).ToList()
        Dim wrongGroup = wrongGroups(rnd.Next(wrongGroups.Count))

        ' 6. Ersetzen jedes Platzhalters
        For i As Integer = 0 To matches.Count - 1
            Dim placeholder = matches(i).Value
            Dim oldGroup = Integer.Parse(matches(i).Groups(1).Value)
            Dim realGroup = groupMap(oldGroup)

            Dim replacementSymbol As String
            If i = errorIndex Then
                ' Fehler: Element aus falscher Gruppe
                Dim wrongList = listOfHauptgruppenElemente.
                Where(Function(e) e.Gruppe = wrongGroup AndAlso e.Periode >= 2 AndAlso e.Periode <= 4).
                ToList()
                replacementSymbol = wrongList(rnd.Next(wrongList.Count)).Symbol
            Else
                ' Korrekt: Element aus richtiger Gruppe
                Dim zufallsListe = selectedElements(realGroup)
                replacementSymbol = zufallsListe(rnd.Next(zufallsListe.Count)).Symbol
            End If

            ' Nur erstes Vorkommen ersetzen
            Dim pos = formula.IndexOf(placeholder)
            If pos >= 0 Then
                formula = formula.Substring(0, pos) & replacementSymbol & formula.Substring(pos + placeholder.Length)
            End If
        Next

        Return formula
    End Function

    Public Function GetRandomChemfigFormulaB(chemfigFormeln As String(), listOfHauptgruppenElemente As List(Of Atom)) As String
        Dim rnd As New Random()

        ' 1. Zufällige Formel auswählen
        Dim formula As String = chemfigFormeln(rnd.Next(chemfigFormeln.Length))

        ' 2. Mapping der Platzhalter zu Hauptgruppen
        Dim placeholderGroups As New Dictionary(Of String, Integer) From {
        {"X5b", 15}, {"X5c", 15},
        {"X6b", 16},
        {"X7b", 17}, {"X7c", 17}
    }

        ' 3. Für jede Hauptgruppe eine Liste zufälliger Elemente erstellen
        Dim selectedElements As New Dictionary(Of Integer, List(Of Atom))
        For Each gruppe In placeholderGroups.Values.Distinct()
            Dim gruppeElements = listOfHauptgruppenElemente.
            Where(Function(e) e.Gruppe = gruppe AndAlso e.Periode >= 2 AndAlso e.Periode <= 4).
            ToList()

            Dim zufallsListe As New List(Of Atom)
            For i As Integer = 1 To 6 ' Anzahl der Variationen pro Gruppe
                Dim randomIndex = rnd.Next(gruppeElements.Count)
                zufallsListe.Add(gruppeElements(randomIndex))
            Next
            selectedElements(gruppe) = zufallsListe
        Next

        ' 4. Platzhalter ersetzen (jedes Vorkommen zufällig)
        For Each kvp In placeholderGroups
            Dim pattern = kvp.Key
            Dim gruppe = kvp.Value
            Dim zufallsListe = selectedElements(gruppe)

            While formula.Contains(pattern)
                Dim randomIndex = rnd.Next(zufallsListe.Count)
                formula = Replace(formula, pattern, zufallsListe(randomIndex).Symbol, 1, 1)
            End While
        Next

        Return formula
    End Function

    Public Function GetRandomLatexTableHG(tableTemplate As String, listOfHauptgruppenElemente As List(Of Atom)) As String
        Dim rnd As New Random()
        Dim formula As String = tableTemplate

        ' Platzhalter finden (Regex: X[4-7][a-z]?)
        Dim matches = System.Text.RegularExpressions.Regex.Matches(formula, "X([4-7])[a-z]?")

        ' Mapping: alte Nummer → echte Hauptgruppe
        Dim groupMap As New Dictionary(Of Integer, Integer) From {
            {4, 14}, {5, 15}, {6, 16}, {7, 17}
        }

        ' Gruppen extrahieren
        Dim groupsNeeded = matches.Cast(Of System.Text.RegularExpressions.Match)().
        Select(Function(m) groupMap(Integer.Parse(m.Groups(1).Value))).
        Distinct().
        ToList()

        ' Für jede Gruppe ein zufälliges Element (Perioden 2–4)
        Dim selectedElements As New Dictionary(Of Integer, Atom)
        For Each gruppe In groupsNeeded
            Dim gruppeElements = listOfHauptgruppenElemente.
            Where(Function(e) e.Gruppe = gruppe AndAlso e.Periode >= 2 AndAlso e.Periode <= 4).
            ToList()

            If gruppeElements.Count > 0 Then
                selectedElements(gruppe) = gruppeElements(rnd.Next(gruppeElements.Count))
            End If
        Next

        ' Platzhalter ersetzen
        For Each match In matches
            Dim placeholder = match.Value
            Dim oldGroup = Integer.Parse(match.Groups(1).Value)
            Dim realGroup = groupMap(oldGroup)
            formula = formula.Replace(placeholder, selectedElements(realGroup).Symbol)
        Next

        Return formula
    End Function


    Public Sub CreateTestAtomBindung()

        Dim sKlasseCSV As String = LokalerPfad("CreateTestAtomBindung.Klassenliste", "Ch9b.csv")
        Dim sInputFilePath As String = LokalerPfad("CreateTestAtomBindung.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "TestLewisVorlage.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-MM-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei
        Dim sUnifiedPDF As String = UnifyPDFStart
        If Not File.Exists(sPath) Then Exit Sub
        If Not File.Exists(sKlasseCSV) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)
        If Not Directory.Exists(sOutPutFolder & "\Pics") And Directory.Exists(sInputFilePath & "\Pics") Then
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sInputFilePath & "\Pics", sOutPutFolder & "\Pics", True)
        End If

        Dim sSource As String = File.ReadAllText(sPath)
        Dim lKlasse As List(Of Schüler) = KlassenListe(sKlasseCSV)


        Dim sLeK As String
        Dim sOutPutFile As String
        Dim sFileName As String
        Dim sCommand As String = """" & LokalerPfad("CreateTestAtomBindung.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        'MsgBox(sCommand)
        Dim shell As Object = CreateObject("WScript.Shell")
        Dim oAt(4) As Atom
        Dim WertListe As List(Of Integer)


        Dim chemfigFormelnA As String() = {
            "\chemfig{\charge{90=\|,180=\|,270=\|}{X7}-X4(=[::270]\charge{230=\|,310=\|}{X6})-\charge{90=\|,0=\|,270=\|}{X7}}",
            "\chemfig{\charge{120=\|,240=\|}{X6}=X4(-[::270]\charge{180=\|,270=\|,0=\|}{X7})-\charge{90=\|,0=\|,270=\|}{X7}}",
            "\chemfig{\charge{90=\|,180=\|,270=\|}{X7}-X4(-[::270]\charge{180=\|,270=\|,0=\|}{X7})=\charge{50=\|,-50=\|}{X6}}"
        }


        Dim chemfigFormelnB As String() = {
            "\chemfig{\charge{90=\|,180=\|,270=\|}{X7b}-\charge{90=\|,270=\|}{X6b}-X5b(-[::270]\charge{0=\|,180=\|,270=\|}{X7c})~\charge{0=\|}{X5c}}",
            "\chemfig{\charge{90=\|,180=\|,270=\|}{X7b}-\charge{90=\|}{X5b}=X6b(-[::270]\charge{0=\|,180=\|,270=\|}{X7c})~\charge{0=\|}{X5c}}",
            "\chemfig{\charge{90=\|,180=\|,270=\|}{X7b}-\charge{90=\|}{X5b}=X7c(=[::270]\charge{230=\|,310=\|}{X6b})~\charge{0=\|}{X5c}}"
        }

        Dim tableTemplate As String =
            "{\Large \begin{tabularx}{\linewidth}{YYYY}" & vbCrLf &
            "    X5a & X4a & X7a & X6a \\" & vbCrLf &
            "\end{tabularx}}\par\bigskip"

        Dim randomFormulaA, randomFormulaB, latexTable As String


        For Each sSchüler As Schüler In lKlasse
            WertListe = New List(Of Integer)
            For i As Integer = 0 To 4
                WertListe.Add(GenerateRandomNumberOhneDopplung(3, 18, WertListe))
            Next
            For i As Integer = 0 To 4
                oAt(i) = New Atom(WertListe(i))
            Next i
            sLeK = sSource.Replace("XXXXX", sSchüler.Vorname & " " & sSchüler.Name & "/" & sSchüler.Klasse)
            randomFormulaA = GetRandomChemfigFormulaA(chemfigFormelnA, dasPSE.lHauptGruppenElemente)
            randomFormulaB = GetRandomChemfigFormulaB(chemfigFormelnB, dasPSE.lHauptGruppenElemente)
            latexTable = GetRandomLatexTableHG(tableTemplate, dasPSE.lHauptGruppenElemente)
            sLeK = sLeK.Replace("TTTTT", latexTable)
            sLeK = sLeK.Replace("YYYYY", randomFormulaA)
            sLeK = sLeK.Replace("ZZZZZ", randomFormulaB)

            sFileName = sSchüler.Vorname & "-" & sSchüler.Name & "-Test.tex"
            sOutPutFile = sOutPutFolder & "\" & sFileName
            File.WriteAllText(sOutPutFile, sLeK)
            sUnifiedPDF += LatexFileToUnifiedPDF(sFileName.Replace(".tex", ".pdf"))
            'MsgBox(sCommand & " """ & sOutPutFile & """")
            shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)
            While IsProcessRunning("pdflatex.exe")
                Threading.Thread.Sleep(5000)
            End While
        Next
        sUnifiedPDF += UnifyPDFEnd
        sOutPutFile = sOutPutFolder & "\LEK-" & lKlasse.First.Klasse & "-all-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sUnifiedPDF)
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub

    Public Sub CreateSchüBriefs()

        Dim sKlasseCSV As String = LokalerPfad("CreateSchüBriefs.Klassenliste", "Ch 8.csv")
        Dim sInputFilePath As String = LokalerPfad("CreateSchüBriefs.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "LEKAtomeVorlage.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-MM-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei
        Dim sUnifiedPDF As String = UnifyPDFStart
        If Not File.Exists(sPath) Then Exit Sub
        If Not File.Exists(sKlasseCSV) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)
        If Not Directory.Exists(sOutPutFolder & "\Pics") And Directory.Exists(sInputFilePath & "\Pics") Then
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sInputFilePath & "\Pics", sOutPutFolder & "\Pics", True)
        End If

        Dim sSource As String = File.ReadAllText(sPath)
        Dim lKlasse As List(Of Schüler) = KlassenListe(sKlasseCSV)


        Dim sLeK As String
        Dim sOutPutFile As String
        Dim sFileName As String
        Dim sCommand As String = """" & LokalerPfad("CreateSchüBriefs.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        'MsgBox(sCommand)
        Dim shell As Object = CreateObject("WScript.Shell")
        Dim oAt(4) As Atom
        Dim WertListe As List(Of Integer)
        Dim sTIKZ As String = "	{\resizebox*{5cm}{!}{\begin{tikzpicture}" & vbCrLf
        Dim sTIKZ1 As String = "\end{tikzpicture}}}" & vbCrLf
        For Each sSchüler As Schüler In lKlasse
            WertListe = New List(Of Integer)
            For i As Integer = 0 To 4
                WertListe.Add(GenerateRandomNumberOhneDopplung(3, 18, WertListe))
            Next
            For i As Integer = 0 To 4
                oAt(i) = New Atom(WertListe(i))
            Next i
            sLeK = sSource.Replace("XXXXX", sSchüler.Vorname & " " & sSchüler.Name & "/" & sSchüler.Klasse)

            sLeK = sLeK.Replace("YYYYY", oAt(0).Name)
            sLeK = sLeK.Replace("ZZZZZ", sTIKZ & oAt(1).LatexCodePSEBahnenModell(False, False, False, True, True,,,, 1) & sTIKZ1)
            sLeK = sLeK.Replace("WWWWW", sTIKZ & oAt(2).LatexCodePSEBahnenModell(False, False, False, True, False,,,, 1) & sTIKZ1)
            sLeK = sLeK.Replace("UUUUU", oAt(2).Symbol)
            sLeK = sLeK.Replace("VVVVV", sTIKZ & oAt(3).LatexCodePSEBahnenModell(False, True, True, True, False,,,, 1) & sTIKZ1)
            sLeK = sLeK.Replace("PPPPP", oAt(4).Name)
            sFileName = sSchüler.Vorname & "-" & sSchüler.Name & "-LEK.tex"
            sOutPutFile = sOutPutFolder & "\" & sFileName
            File.WriteAllText(sOutPutFile, sLeK)
            sUnifiedPDF += LatexFileToUnifiedPDF(sFileName.Replace(".tex", ".pdf"))
            'MsgBox(sCommand & " """ & sOutPutFile & """")
            shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)
            While IsProcessRunning("pdflatex.exe")
                Threading.Thread.Sleep(5000)
            End While
        Next
        sUnifiedPDF += UnifyPDFEnd
        sOutPutFile = sOutPutFolder & "\LEK-" & lKlasse.First.Klasse & "-all-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sUnifiedPDF)
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub

    Public Sub CreateLEKMolUndSaeure()

        Dim sKlasseCSV As String = LokalerPfad("CreateLEKMolUndSaeure.Klassenliste", "Ch9d.csv")
        Dim sInputFilePath As String = LokalerPfad("CreateLEKMolUndSaeure.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "Test9d2024-03-05.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-MM-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei
        Dim sUnifiedPDF As String = UnifyPDFStart
        If Not File.Exists(sPath) Then Exit Sub
        If Not File.Exists(sKlasseCSV) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)
        If Not Directory.Exists(sOutPutFolder & "\Pics") And Directory.Exists(sInputFilePath & "\Pics") Then
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sInputFilePath & "\Pics", sOutPutFolder & "\Pics", True)
        End If

        Dim sSource As String = File.ReadAllText(sPath)
        Dim lKlasse As List(Of Schüler) = KlassenListe(sKlasseCSV)


        Dim sLeK As String
        Dim sOutPutFile As String
        Dim sFileName As String
        Dim sCommand As String = """" & LokalerPfad("CreateLEKMolUndSaeure.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        'MsgBox(sCommand)
        Dim shell As Object = CreateObject("WScript.Shell")
        Dim oAt(4) As Atom
        Dim WertListe As List(Of Integer)
        Dim mySäuren() As String
        Dim lOxide As List(Of SalzPaar) = OxidListe().Where(Function(x) x.Schwierigkeit = 1).ToList
        Dim lOxide1 As List(Of SalzPaar) = OxidListe().Where(Function(x) x.Schwierigkeit = 2).ToList

        For Each sSchüler As Schüler In lKlasse
            WertListe = New List(Of Integer)
            For i As Integer = 0 To 4
                WertListe.Add(GenerateRandomNumberOhneDopplung(3, 18, WertListe))
            Next
            For i As Integer = 0 To 4
                oAt(i) = New Atom(WertListe(i))
            Next i
            sLeK = sSource.Replace("NNNNN", sSchüler.Vorname & " " & sSchüler.Name & "/" & sSchüler.Klasse)
            mySäuren = ZweiSäuren()
            sLeK = sLeK.Replace("XXX", mySäuren(0))
            sLeK = sLeK.Replace("YYY", mySäuren(1))
            sLeK = sLeK.Replace("ZZZ", lOxide(GenerateRandomNumber(0, lOxide.Count - 1)).LatexSummenFormelSalz)
            sLeK = sLeK.Replace("VVV", lOxide1(GenerateRandomNumber(0, lOxide1.Count - 1)).LatexSummenFormelSalz)
            sFileName = sSchüler.Vorname & "-" & sSchüler.Name & "-LEK.tex"
            sOutPutFile = sOutPutFolder & "\" & sFileName
            File.WriteAllText(sOutPutFile, sLeK)
            sUnifiedPDF += LatexFileToUnifiedPDF(sFileName.Replace(".tex", ".pdf"))
            'MsgBox(sCommand & " """ & sOutPutFile & """")
            shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)
            While IsProcessRunning("pdflatex.exe")
                Threading.Thread.Sleep(5000)
            End While
        Next
        sUnifiedPDF += UnifyPDFEnd
        sOutPutFile = sOutPutFolder & "\LEK-" & lKlasse.First.Klasse & "-all-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sUnifiedPDF)
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub

    Public Sub CreateLEKAlkane()

        Dim sKlasseCSV As String = LokalerPfad("CreateLEKAlkane.Klassenliste", "Ch 10.csv")
        Dim sInputFilePath As String = LokalerPfad("CreateLEKAlkane.Vorlagenordner", ".")
        Dim sVorlagenDatei As String = "Die LEK202410.tex"
        Dim sOutPutFolder As String = sInputFilePath & "\" & Date.Today.ToString("yyyy-MM-dd")
        Dim sPath As String = sInputFilePath & "\" & sVorlagenDatei
        Dim sUnifiedPDF As String = UnifyPDFStart
        If Not File.Exists(sPath) Then Exit Sub
        If Not File.Exists(sKlasseCSV) Then Exit Sub
        If Not Directory.Exists(sOutPutFolder) Then Directory.CreateDirectory(sOutPutFolder)
        If Not Directory.Exists(sOutPutFolder & "\Pics") And Directory.Exists(sInputFilePath & "\Pics") Then
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sInputFilePath & "\Pics", sOutPutFolder & "\Pics", True)
        End If

        Dim sSource As String = File.ReadAllText(sPath)
        Dim lKlasse As List(Of Schüler) = KlassenListe(sKlasseCSV)


        Dim sLeK As String
        Dim sOutPutFile As String
        Dim sFileName As String
        Dim sCommand As String = """" & LokalerPfad("CreateLEKAlkane.PdfLatex", "pdflatex.exe") & """ -synctex=1 -interaction=nonstopmode -output-directory """
        sCommand += sOutPutFolder & """"
        'MsgBox(sCommand)
        Dim shell As Object = CreateObject("WScript.Shell")

        For Each sSchüler As Schüler In lKlasse
            Dim myKWST As New Kohlenwasserstoff
            myKWST.CreateAndAppendSeitenketten(3, {2, 4}, 7, 8, 10)
            myKWST.Kettenstart.FillFreeBondsWithAtomType(1)

            sLeK = sSource.Replace("ZZZ", sSchüler.Vorname & " " & sSchüler.Name & "/" & sSchüler.Klasse)

            sLeK = sLeK.Replace("XXX", myKWST.Kettenstart.LatexFromSimpleChainFormular)
            sLeK = sLeK.Replace("YYY", Isoalkane())

            Debug.Print(sSchüler.Vorname & " " & sSchüler.Name & myKWST.Name)

            sFileName = sSchüler.Vorname & "-" & sSchüler.Name & "-LEK.tex"
            sOutPutFile = sOutPutFolder & "\" & sFileName
            File.WriteAllText(sOutPutFile, sLeK)
            sUnifiedPDF += LatexFileToUnifiedPDF(sFileName.Replace(".tex", ".pdf"))
            'MsgBox(sCommand & " """ & sOutPutFile & """")
            shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)
            While IsProcessRunning("pdflatex.exe")
                Threading.Thread.Sleep(5000)
            End While
        Next
        sUnifiedPDF += UnifyPDFEnd
        sOutPutFile = sOutPutFolder & "\LEK-" & lKlasse.First.Klasse & "-all-" & Date.Today.ToString("yyyy-mm-dd") & ".tex"
        File.WriteAllText(sOutPutFile, sUnifiedPDF)
        shell.Run(sCommand & " """ & sOutPutFile & """", 0, True)

    End Sub


    Private Function IsProcessRunning(pProcessName)
        ' Function will do a WMI query to determine if a the process pProcessName is currently
        ' running on the local computer. Returns True if detected. 
        Dim objWMIService
        Dim strWMIQuery

        strWMIQuery = "Select * From Win32_Process Where name Like '" & pProcessName & "'"
        objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")

        ' Run The query against the WMI for the local machine
        If (objWMIService.ExecQuery(strWMIQuery).Count > 0) Then
            IsProcessRunning = True
        Else
            IsProcessRunning = False
        End If
    End Function

    Private Function ZweiSäuren() As String()

        Dim sSäuren() As String = {"HCl", "HNO3", "H2CO3", "H2SO4", "H2SO3", "H3PO4"}

        Dim arINt1() = GenerateZweiZahlen(6)
        Dim slatex() As String = {"\ce{", "\ce{"}

        slatex(0) += sSäuren(arINt1(0)) & "}"
        slatex(1) += sSäuren(arINt1(1)) & "}"

        Return slatex

    End Function

    Private Function IsoAlkane() As String

        Dim sAlkane() As String = {"Hexan", "Heptan", "Octan", "Nonan", "Decan"}
        Dim slatex As String = "iso-" & sAlkane(GenerateRandomNumber(0, 4)) & "e"

        Return slatex

    End Function


    Private Function MineralWasserGehalt() As String
        Dim sMetalle() As String = {"Ca & 382 \\\hline",
                                    "Mg & 107\\\hline",
                                    "Na&148\\\hline",
                                    "K&13,1\\\hline"}
        Dim sAnionen() As String = {"\ce{CO3^{2-}}&2008\\\hline",
                                    "\ce{Cl-}&59\\\hline",
                                    "\ce{SO4^{2-}}&42\\\hline",
                                    "\ce{F-}&0,12\\\hline"}
        Dim sLatex As String = ""
        Dim arInt() = GenerateDreiZahlen()
        Dim arINt1() = GenerateZweiZahlen()

        If GenerateRandomNumber(0, 2) = 0 Then
            sLatex += sMetalle(arInt(0)) & vbCrLf
            sLatex += sMetalle(arInt(1)) & vbCrLf
            sLatex += sMetalle(arInt(2)) & vbCrLf
            sLatex += "\multicolumn{2}{c}{}\\\hline" & vbCrLf
            sLatex += sAnionen(arINt1(0)) & vbCrLf
            sLatex += sAnionen(arINt1(1)) & vbCrLf
        Else
            sLatex += sMetalle(arINt1(0)) & vbCrLf
            sLatex += sMetalle(arINt1(1)) & vbCrLf
            sLatex += "\multicolumn{2}{c}{}\\\hline" & vbCrLf
            sLatex += sAnionen(arInt(0)) & vbCrLf
            sLatex += sAnionen(arInt(1)) & vbCrLf
            sLatex += sAnionen(arInt(2)) & vbCrLf
        End If
        Return sLatex

    End Function

    Private Function GenerateZweiZahlen(Optional iMax As Integer = 4) As Integer()
        Dim i, j As Integer
        i = GenerateRandomNumber(0, iMax)
        Do
            j = GenerateRandomNumber(0, iMax)
        Loop While (i = j)

        Dim ArInt(1) As Integer
        ArInt(0) = i
        ArInt(1) = j
        Return ArInt

    End Function

    Private Function GenerateDreiZahlen() As Integer()
        Dim i, j, k As Integer
        i = GenerateRandomNumber(0, 4)
        Do
            j = GenerateRandomNumber(0, 4)
        Loop While (i = j)
        Do
            k = GenerateRandomNumber(0, 4)
        Loop While (k = j) Or (k = i)
        Dim ArInt(2) As Integer
        ArInt(0) = i
        ArInt(1) = j
        ArInt(2) = k
        Return ArInt
    End Function

    Function LatexFileToUnifiedPDF(theFilePathName As String) As String
        Return "\includepdf[pages=-,fitpaper]{""" & theFilePathName & """}" & vbCrLf
    End Function

End Module
