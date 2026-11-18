Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports AtomeAkt.Chemistry

Namespace UI
    ''' <summary>
    ''' Lokales Werkzeug zur Erstellung editierbarer chemfig-Quellen.
    ''' Bewusst unabhängig von Schülerdaten und von den alten LEK-Routinen.
    ''' </summary>
    Public NotInheritable Class MainForm
        Inherits Form

        Private NotInheritable Class Example
            Public ReadOnly Property Title As String
            Public ReadOnly Property ChainLength As Integer
            Public ReadOnly Property BranchInput As String

            Public Sub New(title As String, chainLength As Integer, branchInput As String)
                Me.Title = title
                Me.ChainLength = chainLength
                Me.BranchInput = branchInput
            End Sub

            Public Overrides Function ToString() As String
                Return Title
            End Function
        End Class

        Private ReadOnly presetSelector As New ComboBox()
        Private ReadOnly chainInput As New NumericUpDown()
        Private ReadOnly branchInput As New TextBox()
        Private ReadOnly formulaSelector As New ComboBox()
        Private ReadOnly submolOption As New CheckBox()
        Private ReadOnly commentsOption As New CheckBox()
        Private ReadOnly mainBondInput As New NumericUpDown()
        Private ReadOnly sideBondInput As New NumericUpDown()
        Private ReadOnly hydrogenBondInput As New NumericUpDown()
        Private ReadOnly editor As New RichTextBox()
        Private ReadOnly description As New Label()
        Private ReadOnly status As New ToolStripStatusLabel()
        Private ReadOnly lightBackground As Color = Color.FromArgb(247, 249, 252)
        Private ReadOnly ink As Color = Color.FromArgb(25, 43, 64)

        Public Sub New()
            Me.Text = "Chemfig-Werkstatt | AtomeAkt"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.MinimumSize = New Size(980, 640)
            Me.Size = New Size(1260, 800)
            Me.BackColor = Color.White
            Me.Font = New Font("Segoe UI", 10.0F)
            BuildWindow()
            LoadExamples()
        End Sub

        Private Sub BuildWindow()
            Dim header As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 82,
                .BackColor = ink
            }
            header.Controls.Add(New Label With {
                .Text = "Chemfig-Werkstatt",
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI Semibold", 20.0F, FontStyle.Bold),
                .Location = New Point(22, 10),
                .Size = New Size(500, 38)
            })
            header.Controls.Add(New Label With {
                .Text = "Verzweigte Alkane  ·  Halbstruktur- und ausführliche Strukturformeln",
                .ForeColor = Color.FromArgb(216, 228, 241),
                .Location = New Point(24, 49),
                .Size = New Size(840, 24)
            })

            Dim split As New SplitContainer With {
                .Dock = DockStyle.Fill,
                .Orientation = Orientation.Vertical,
                .SplitterWidth = 6,
                .Width = 1200,
                .Height = 650,
                .Panel1MinSize = 305,
                .Panel2MinSize = 430,
                .SplitterDistance = 360
            }
            split.Panel1.BackColor = lightBackground
            split.Panel2.BackColor = Color.White

            Dim scrollArea As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True}
            Dim fields As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 1,
                .Padding = New Padding(18, 16, 18, 24)
            }
            fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))

            AddField(fields, New Label With {
                .Text = "Molekül auswählen oder selbst eingeben",
                .Font = New Font("Segoe UI Semibold", 11.0F, FontStyle.Bold),
                .ForeColor = ink
            }, 32)
            presetSelector.DropDownStyle = ComboBoxStyle.DropDownList
            AddField(fields, presetSelector, 33)
            AddField(fields, MakeLabel("Anzahl der C-Atome in der Zeichen-Hauptkette"), 39)
            ConfigureNumber(chainInput, 1D, 20D, 4D, 0)
            AddField(fields, chainInput, 34)

            AddField(fields, MakeLabel("Seitenketten (Position:Länge, getrennt durch ;)"), 34)
            branchInput.Text = "2:1;3:1"
            AddField(fields, branchInput, 34)
            AddField(fields, New Label With {
                .Text = "Beispiel: 2:1;3:2 bedeutet Methyl an C2 und Ethyl an C3.",
                .ForeColor = Color.FromArgb(79, 95, 113),
                .Font = New Font("Segoe UI", 8.5F)
            }, 45)

            AddField(fields, MakeLabel("Darstellung"), 28)
            formulaSelector.DropDownStyle = ComboBoxStyle.DropDownList
            formulaSelector.Items.AddRange(New Object() {
                "Halbstrukturformel",
                "Ausführliche Strukturformel (jedes C und H)"
            })
            formulaSelector.SelectedIndex = 0
            AddField(fields, formulaSelector, 34)

            submolOption.Text = "Sinnvolle Submol-Makros verwenden"
            submolOption.Checked = True
            AddField(fields, submolOption, 31)
            commentsOption.Text = "Kommentare mit Hauptkettenpositionen"
            commentsOption.Checked = True
            AddField(fields, commentsOption, 31)

            AddField(fields, MakeLabel("Bindungslängen (Chemfig-Einheiten)"), 28)
            ConfigureNumber(mainBondInput, 0.3D, 4D, 2.2D, 1)
            ConfigureNumber(sideBondInput, 0.3D, 4D, 1.6D, 1)
            ConfigureNumber(hydrogenBondInput, 0.3D, 4D, 0.7D, 1)
            AddField(fields, LabeledNumber("Hauptkette", mainBondInput), 36)
            AddField(fields, LabeledNumber("Seitenketten", sideBondInput), 36)
            AddField(fields, LabeledNumber("C–H", hydrogenBondInput), 36)

            Dim generateButton As Button = StyledButton("Chemfig erzeugen", True, 182)
            AddHandler generateButton.Click, AddressOf GenerateCode
            AddField(fields, generateButton, 43)

            AddField(fields, New Label With {
                .Text = "Hinweis: Die Eingabe beschreibt die Zeichenachse. Eine IUPAC-Benennung und die Prüfung der längsten Hauptkette folgen später.",
                .ForeColor = Color.FromArgb(126, 72, 30),
                .Font = New Font("Segoe UI", 9.0F)
            }, 75)
            scrollArea.Controls.Add(fields)
            split.Panel1.Controls.Add(scrollArea)

            Dim rightSide As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .RowCount = 3,
                .ColumnCount = 1,
                .Padding = New Padding(16, 14, 16, 12)
            }
            rightSide.RowStyles.Add(New RowStyle(SizeType.Absolute, 60.0F))
            rightSide.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            rightSide.RowStyles.Add(New RowStyle(SizeType.Absolute, 66.0F))
            rightSide.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            description.Dock = DockStyle.Fill
            description.Text = "Editierbarer Chemfig-Quelltext" & Environment.NewLine &
                               "In Arbeitsblätter mit \usepackage{chemfig} einfügen."
            description.Font = New Font("Segoe UI Semibold", 10.5F, FontStyle.Regular)
            description.ForeColor = ink
            rightSide.Controls.Add(description, 0, 0)

            editor.Dock = DockStyle.Fill
            editor.WordWrap = False
            editor.AcceptsTab = True
            editor.DetectUrls = False
            editor.BorderStyle = BorderStyle.FixedSingle
            editor.Font = New Font("Consolas", 10.0F)
            editor.BackColor = Color.FromArgb(252, 253, 255)
            editor.ForeColor = Color.FromArgb(33, 45, 59)
            editor.ScrollBars = RichTextBoxScrollBars.Both
            rightSide.Controls.Add(editor, 0, 1)

            Dim actions As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = True,
                .Padding = New Padding(0, 8, 0, 0)
            }
            Dim copyButton As Button = StyledButton("Code kopieren", False, 140)
            Dim saveButton As Button = StyledButton("TeX speichern", False, 141)
            Dim previewButton As Button = StyledButton("PDF-Vorschau", True, 141)
            AddHandler copyButton.Click, AddressOf CopyCode
            AddHandler saveButton.Click, AddressOf SaveTexDocument
            AddHandler previewButton.Click, AddressOf OpenPdfPreview
            actions.Controls.Add(copyButton)
            actions.Controls.Add(saveButton)
            actions.Controls.Add(previewButton)
            rightSide.Controls.Add(actions, 0, 2)
            split.Panel2.Controls.Add(rightSide)

            Dim strip As New StatusStrip With {.Dock = DockStyle.Bottom, .SizingGrip = False}
            status.Spring = True
            status.TextAlign = ContentAlignment.MiddleLeft
            status.Text = "Bereit."
            strip.Items.Add(status)
            Me.Controls.Add(split)
            Me.Controls.Add(header)
            Me.Controls.Add(strip)
        End Sub

        Private Shared Sub AddField(layout As TableLayoutPanel, control As Control, height As Integer)
            Dim row As Integer = layout.RowCount
            layout.RowCount += 1
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, CSng(height)))
            control.Dock = DockStyle.Fill
            control.Margin = New Padding(0, 2, 0, 4)
            layout.Controls.Add(control, 0, row)
        End Sub

        Private Shared Function MakeLabel(value As String) As Label
            Return New Label With {
                .Text = value,
                .Font = New Font("Segoe UI Semibold", 9.2F),
                .ForeColor = Color.FromArgb(34, 51, 69)
            }
        End Function

        Private Shared Sub ConfigureNumber(box As NumericUpDown,
                                           minimum As Decimal, maximum As Decimal,
                                           initial As Decimal, decimals As Integer)
            box.Minimum = minimum
            box.Maximum = maximum
            box.DecimalPlaces = decimals
            box.Increment = If(decimals = 0, 1D, 0.1D)
            box.Value = initial
            box.TextAlign = HorizontalAlignment.Center
        End Sub

        Private Shared Function LabeledNumber(caption As String, box As NumericUpDown) As Control
            Dim panel As New TableLayoutPanel With {.ColumnCount = 2, .RowCount = 1}
            panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 62.0F))
            panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38.0F))
            panel.Controls.Add(New Label With {
                .Text = caption, .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0)
            box.Dock = DockStyle.Fill
            panel.Controls.Add(box, 1, 0)
            Return panel
        End Function

        Private Shared Function StyledButton(caption As String,
                                             primary As Boolean,
                                             width As Integer) As Button
            Dim btn As New Button With {
                .Text = caption,
                .Width = width,
                .Height = 36,
                .FlatStyle = FlatStyle.Flat,
                .Cursor = Cursors.Hand,
                .BackColor = If(primary, Color.FromArgb(27, 85, 147), Color.FromArgb(233, 239, 247)),
                .ForeColor = If(primary, Color.White, Color.FromArgb(26, 54, 85)),
                .Font = New Font("Segoe UI Semibold", 9.2F),
                .Margin = New Padding(0, 1, 9, 5)
            }
            btn.FlatAppearance.BorderSize = 0
            Return btn
        End Function

        Private Sub LoadExamples()
            presetSelector.Items.AddRange(New Object() {
                New Example("Butan (keine Verzweigung)", 4, ""),
                New Example("2-Methylbutan", 4, "2:1"),
                New Example("2,2-Dimethylpropan", 3, "2:1;2:1"),
                New Example("2,3-Dimethylbutan", 4, "2:1;3:1"),
                New Example("2,2,4-Trimethylpentan", 5, "2:1;2:1;4:1"),
                New Example("3-Ethyl-2-methylhexan", 6, "2:1;3:2"),
                New Example("3,3-Diethylpentan", 5, "3:2;3:2")
            })
            AddHandler presetSelector.SelectedIndexChanged, AddressOf SelectExample
            presetSelector.SelectedIndex = 3
        End Sub

        Private Sub SelectExample(sender As Object, e As EventArgs)
            Dim example As Example = TryCast(presetSelector.SelectedItem, Example)
            If example Is Nothing Then Return
            chainInput.Value = CDec(example.ChainLength)
            branchInput.Text = example.BranchInput
            GenerateCode(sender, e)
        End Sub

        Private Shared Function ParseBranches(value As String) As AlkaneBranch()
            If String.IsNullOrWhiteSpace(value) Then Return Array.Empty(Of AlkaneBranch)()
            Dim items As New List(Of AlkaneBranch)()
            For Each item As String In value.Replace(vbCrLf, ";").Replace(vbLf, ";").Split(";"c)
                If String.IsNullOrWhiteSpace(item) Then Continue For
                Dim pair As String() = item.Trim().Split(":"c)
                Dim position As Integer
                Dim length As Integer
                If pair.Length <> 2 OrElse
                   Not Integer.TryParse(pair(0).Trim(), NumberStyles.Integer,
                                        CultureInfo.InvariantCulture, position) OrElse
                   Not Integer.TryParse(pair(1).Trim(), NumberStyles.Integer,
                                        CultureInfo.InvariantCulture, length) Then
                    Throw New FormatException("Seitenketten bitte als Position:Länge eingeben, z. B. 2:1;3:2.")
                End If
                items.Add(New AlkaneBranch(position, length))
            Next
            Return items.ToArray()
        End Function

        Private Sub GenerateCode(sender As Object, e As EventArgs)
            Try
                Dim molecule As New AlkaneMolecule(CInt(chainInput.Value), ParseBranches(branchInput.Text))
                Dim options As New ChemfigRenderOptions With {
                    .UseSubmolecules = submolOption.Checked,
                    .IncludeComments = commentsOption.Checked,
                    .MainBondLength = mainBondInput.Value,
                    .BranchBondLength = sideBondInput.Value,
                    .HydrogenBondLength = hydrogenBondInput.Value
                }
                Dim kind As AlkaneFormulaKind = If(formulaSelector.SelectedIndex = 1,
                    AlkaneFormulaKind.Ausfuehrlich, AlkaneFormulaKind.Halbstruktur)
                editor.Text = ChemfigRenderer.Render(molecule, kind, options)
                description.Text = "Editierbarer Chemfig-Quelltext" & Environment.NewLine &
                    $"{molecule.MolecularFormula} · {molecule.Branches.Count} Seitenketten · " &
                    If(kind = AlkaneFormulaKind.Ausfuehrlich, "ausführlich", "Halbstruktur")
                status.Text = "Erzeugt. Änderungen am rechten Quelltext bleiben bis zur nächsten Generierung erhalten."
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse
                                       TypeOf ex Is FormatException OrElse
                                       TypeOf ex Is InvalidOperationException
                status.Text = "Eingabe ungültig: " & ex.Message
                MessageBox.Show(Me, ex.Message, "Ungültige Molekülstruktur",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub CopyCode(sender As Object, e As EventArgs)
            If String.IsNullOrWhiteSpace(editor.Text) Then Return
            Try
                Clipboard.SetText(editor.Text)
                status.Text = "Chemfig-Quelltext in die Zwischenablage kopiert."
            Catch ex As ExternalException
                MessageBox.Show(Me, ex.Message, "Zwischenablage nicht verfügbar",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Shared Function TexDocument(fragment As String) As String
            Dim source As New StringBuilder()
            source.AppendLine("\documentclass[a4paper,12pt]{article}")
            source.AppendLine("\usepackage[margin=2cm]{geometry}")
            source.AppendLine("\usepackage{chemfig}")
            source.AppendLine("\setchemfig{atom sep=2em}")
            source.AppendLine("\pagestyle{empty}")
            source.AppendLine("\begin{document}")
            source.AppendLine("\noindent")
            source.AppendLine(fragment)
            source.AppendLine("\end{document}")
            Return source.ToString()
        End Function

        Private Sub SaveTexDocument(sender As Object, e As EventArgs)
            If String.IsNullOrWhiteSpace(editor.Text) Then Return
            Using dialog As New SaveFileDialog With {
                .Title = "Chemfig als editierbare LaTeX-Datei speichern",
                .Filter = "LaTeX-Dokument (*.tex)|*.tex|Alle Dateien (*.*)|*.*",
                .DefaultExt = "tex",
                .AddExtension = True,
                .FileName = "alkan-struktur.tex"
            }
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                Try
                    File.WriteAllText(dialog.FileName,
                                      TexDocument(editor.Text),
                                      New UTF8Encoding(False))
                    status.Text = "TeX-Dokument gespeichert: " & dialog.FileName
                Catch ex As IOException
                    MessageBox.Show(Me, ex.Message, "Datei konnte nicht gespeichert werden",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                Catch ex As UnauthorizedAccessException
                    MessageBox.Show(Me, ex.Message, "Kein Schreibzugriff",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Sub

        Private Sub OpenPdfPreview(sender As Object, e As EventArgs)
            If String.IsNullOrWhiteSpace(editor.Text) Then Return
            Dim folder As String = Path.Combine(Path.GetTempPath(),
                                                "AtomeAkt-Chemfig-Vorschau",
                                                Guid.NewGuid().ToString("N"))
            Try
                Directory.CreateDirectory(folder)
                File.WriteAllText(Path.Combine(folder, "formel.tex"),
                                  TexDocument(editor.Text),
                                  New UTF8Encoding(False))

                Dim startInfo As New ProcessStartInfo("pdflatex") With {
                    .WorkingDirectory = folder,
                    .UseShellExecute = False,
                    .CreateNoWindow = True
                }
                startInfo.ArgumentList.Add("-no-shell-escape")
                startInfo.ArgumentList.Add("-halt-on-error")
                startInfo.ArgumentList.Add("-interaction=nonstopmode")
                startInfo.ArgumentList.Add("formel.tex")

                Using compiler As Process = Process.Start(startInfo)
                    If compiler Is Nothing Then
                        Throw New InvalidOperationException("pdflatex konnte nicht gestartet werden.")
                    End If
                    If Not compiler.WaitForExit(45000) Then
                        compiler.Kill(entireProcessTree:=True)
                        Throw New TimeoutException("pdflatex hat die Zeitgrenze von 45 Sekunden überschritten.")
                    End If
                    If compiler.ExitCode <> 0 Then
                        Dim logPath As String = Path.Combine(folder, "formel.log")
                        Dim detail As String = ""
                        If File.Exists(logPath) Then
                            detail = String.Join(Environment.NewLine,
                                File.ReadLines(logPath).
                                    Where(Function(line) line.StartsWith("!", StringComparison.Ordinal)).
                                    Take(3))
                        End If
                        Throw New InvalidOperationException(
                            "LaTeX-Kompilation fehlgeschlagen." & Environment.NewLine & detail &
                            Environment.NewLine & "Protokoll: " & logPath)
                    End If
                End Using

                Dim pdfPath As String = Path.Combine(folder, "formel.pdf")
                If Not File.Exists(pdfPath) Then
                    Throw New IOException("pdflatex hat keine PDF-Datei erzeugt.")
                End If
                Process.Start(New ProcessStartInfo(pdfPath) With {.UseShellExecute = True})
                status.Text = "PDF-Vorschau geöffnet. Temporäre Dateien: " & folder
            Catch ex As Win32Exception
                MessageBox.Show(Me,
                    "pdflatex wurde nicht gefunden oder die PDF konnte nicht geöffnet werden." &
                    Environment.NewLine & "Installiere MiKTeX/TeX Live bzw. prüfe PATH." &
                    Environment.NewLine & ex.Message,
                    "PDF-Vorschau nicht verfügbar", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is InvalidOperationException OrElse
                                       TypeOf ex Is TimeoutException
                MessageBox.Show(Me, ex.Message,
                                "PDF-Vorschau fehlgeschlagen",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub
    End Class
End Namespace
