# ChemfigCore – isolierter Start für AtomeAkt

**Stand: 18.11.2026 · Entwicklungszweig.** Die Projekte `ChemfigCore` und `ChemfigCore.SmokeTests` sind seitdem in `AtomeAkt.sln` eingetragen. Die bisherige Datei `AtomeAkt.vbproj` sowie `Atom.vb`, `Kohlenwasserstoff.vb` und LEK-Skripte wurden nicht verändert.

## Umsetzung im neuen Modul

- Validierter **C-Atomgraph** eines gesättigten, azyklischen Alkans. Die H-Zahl wird aus der Kohlenstoffvalenz berechnet.
- Explizite Eingabe: Länge einer **Zeichen-Hauptkette** und Position/Länge linearer Seitenketten; zwei Seitenketten am selben inneren C-Atom sind möglich.
- **Zwei Formeln aus derselben Struktur:** Halbstrukturformel mit CH3-/CH2-/CH-Gruppen und wirklich ausführliche Strukturformel mit jedem C, jedem H und jeder C-C-/C-H-Bindung.
- Gut bearbeitbarer mehrzeiliger `\chemfig`-Code mit Positionierungs-Kommentaren, optionalen lokalen `\definesubmol`-Makros und einstellbaren Bindungslängen.
- Eigenständige VB.NET-Klassenbibliothek auf .NET 10 und Smoke-Test-Konsolenprojekt ohne externes Testframework.

## Grenzen / offene Prüfungen

- **Keine IUPAC-Benennung und kein Namensparser:** Die Zeichen-Hauptkette ist noch nicht automatisch als längste IUPAC-Hauptkette validiert. Überschriften im Testprogramm sind handgeprüfte Referenzbezeichnungen.
- Lineare Seitenketten, keine untergeordnet verzweigten Seitenketten, Ringe oder Doppelbindungen.
- Noch kein Zufallsgenerator mit Seed und keine dokumentierte JSON-CLI. Eine erste **Windows-GUI zum lokalen Testen** ist unter `../ChemfigDesktop` vorhanden; sie ist noch kein vollständiger IUPAC-Nomenklaturgenerator.
- **CI-Basistest erfolgreich:** Der VB.NET-Renderer wurde unter .NET 10 gebaut, die sieben Referenzmoleküle wurden in beiden Formelarten generiert und tatsächlich mit `pdflatex` kompiliert. Das bestehende Atom-/LEK-Programm wird bei diesen Tests nicht gestartet.
- Unabhängiger geometrischer TeX-Prototyp: sieben Referenzmoleküle × beide Formeltypen mit `pdflatex` erfolgreich kompiliert und visuell kontrolliert. Das beweist den Layoutansatz, **nicht** den Build des VB.NET-Moduls.

## Lokal prüfen (Visual Studio / .NET 10 SDK)

Alle drei Projekte sind auf `net10.0` eingestellt. `../global.json` wählt die .NET-10-SDK-Familie (statt eines parallel installierten .NET-11-SDKs). Unter Visual Studio muss eine Version mit .NET-10-Unterstützung installiert sein.

In der Solution sollten nun vier Projekte sichtbar sein: `AtomeAkt`, `ChemfigCore`, `ChemfigCore.SmokeTests` und **`ChemfigDesktop`**. Zum Testen das Startprojekt auf **ChemfigCore.SmokeTests** setzen. Die alte LEK-Anwendung wird nicht ausgeführt.

Nach dem Wechsel auf den Entwicklungszweig aus `D:\hilfsprogramme\AtomeAkt`:

```powershell
dotnet --version
dotnet build .\ChemfigCore\ChemfigCore.vbproj
dotnet run --project .\ChemfigCore.SmokeTests\ChemfigCore.SmokeTests.vbproj -- .\ausgabe
cd .\ausgabe
pdflatex -halt-on-error -interaction=nonstopmode chemfig-referenz.tex
```

Die generierte PDF visuell prüfen. `pdflatex` benötigt das Paket `chemfig`.

## Minimaler Aufruf

```vb
Imports AtomeAkt.Chemistry

Dim mol As New AlkaneMolecule(4, New AlkaneBranch(2, 1), New AlkaneBranch(3, 1))
Dim halb As String = ChemfigRenderer.Render(mol, AlkaneFormulaKind.Halbstruktur)
Dim voll As String = ChemfigRenderer.Render(mol, AlkaneFormulaKind.Ausfuehrlich)
```

Ein zusätzlicher Test kontrolliert bei ausgeschalteten Makros für jede Vollstruktur die explizite Anzahl der C- und H-Atome. **Der CI-Build und die TeX-Kompilation der Referenzfälle sind erfolgreich.** Ein interaktiver Start der Windows-GUI auf dem Nutzerrechner steht noch aus.

Der erzeugte Code ist für Dokumente mit `\usepackage{chemfig}` vorgesehen. Keine `\mathrm`-Konstruktionen erforderlich.

## Als Nächstes

1. **Auf Nutzerrechner kompilieren**, tatsächliche vom VB-Programm geschriebene `.tex`-Datei mit `pdflatex` prüfen und PDF kontrollieren.
2. Graph-/Namenslogik erweitern (wirklich längste Hauptkette, Lokantenvergleich und Substituenten), zusätzliche Grenzfalltests.
3. Layout verbessern, dann eine stabile CLI-/JSON-Schnittstelle für Arbeitsblätter.
4. Die erste Windows-GUI ist jetzt als lokaler Testeinstieg vorhanden; sie wird nach dem Nutzerfeedback weiterentwickelt. Später folgt die Hilfsprogramm-Zentrale.

Der verbindliche Gesamtplan steht unter [../PLAN-CHEMFIG-UND-GUI.md](../PLAN-CHEMFIG-UND-GUI.md).
