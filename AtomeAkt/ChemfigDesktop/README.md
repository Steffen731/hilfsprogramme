# Chemfig-Werkstatt (Windows-GUI)

**Entwicklungsstand, 18.11.2026.** Eigenständige Windows-Forms-Anwendung auf .NET 10. Der bestehende Atom-/LEK-Generator wird **nicht** verändert oder gestartet. Der gemeinsame Renderer liegt unter `../ChemfigCore`.

## Starten

1. GitHub-Entwicklungszweig lokal aktivieren und aktualisieren: `feature/atomeakt-chemfig-core`.
2. In Visual Studio: `AtomeAkt.sln` öffnen, im Projektmappen-Explorer `ChemfigDesktop` mit rechter Maustaste **Als Startprojekt festlegen**, dann **F5**.
3. Alternativ über ein PowerShell-Terminal im Ordner `D:\hilfsprogramme`:

```powershell
git fetch origin
git switch feature/atomeakt-chemfig-core
git pull --ff-only
cd .\AtomeAkt
dotnet run --project .\ChemfigDesktop\ChemfigDesktop.vbproj
```

Wenn der lokale Entwicklungszweig noch nicht existiert: `git switch --track origin/feature/atomeakt-chemfig-core` statt `git switch feature/atomeakt-chemfig-core` verwenden. Nur bei sauberem Arbeitsverzeichnis wechseln; nie lokale, ungesicherte Änderungen überschreiben.

**Voraussetzungen:** Windows, .NET 10 SDK sowie Visual Studio mit .NET-10-Unterstützung. Für die PDF-Vorschau zusätzlich `pdflatex` (z. B. MiKTeX), einschließlich LaTeX-Paket `chemfig`. Das Erzeugen, Kopieren und Speichern von TeX funktioniert **ohne** LaTeX-Installation.

## Bedienung

- **Referenzmolekül auswählen:** sieben vordefinierte Alkane; die Bezeichnungen dienen nur als bekannte Testbeispiele.
- **Zeichen-Hauptkette:** Anzahl der linear angeordneten C-Atome, z. B. `4`.
- **Seitenketten:** `Position:Länge`, mehrere mit Semikolon trennen. `2:1;3:2` = an C2 eine Methyl- und an C3 eine Ethylseitenkette. `2:1;2:1` = zwei Methylgruppen an C2. Nur **lineare** Seitenketten und valide Kohlenstoffvalenzen werden zugelassen.
- **Darstellung:** Halbstrukturformel oder wirklich ausführliche Strukturformel, in der jedes C und H und alle Bindungen explizit vorkommen.
- **Makros / Kommentare:** wahlweise lokale `\definesubmol`-Abkürzungen und Positionskommentare.
- **Bindungslängen:** Werte für Hauptkette, Seitenketten und C-H-Bindungen.
- **Chemfig erzeugen:** füllt den editierbaren Quelltextbereich. Eigene Änderungen bleiben bis zum nächsten Generieren erhalten.
- **Code kopieren:** kopiert den sichtbaren Quelltext einschließlich evtl. benötigter `\definesubmol`-Definitionen.
- **TeX speichern:** schreibt ein vollständiges, editierbares LaTeX-Dokument.
- **PDF-Vorschau:** kompiliert den aktuell sichtbaren Code mit lokalem `pdflatex`, öffnet das PDF mit dem Standardprogramm; Fehlermeldungen enthalten den Pfad zur temporären Logdatei.

Die PDF-Vorschau verwendet `-no-shell-escape`. Keine Netzwerkdienste und keine Schülerdaten werden benötigt.

## Fachliche Einschränkungen (wichtig!)

- Die **Zeichen-Hauptkette ist nicht automatisch die längste IUPAC-Hauptkette**. Die GUI ist im jetzigen Stand ein **Formelgenerator**, noch kein umfassender Nomenklaturgenerator.
- Die Bezeichnung des gewählten Referenzmoleküls wird **nicht** aus dem Graphen berechnet. Bei manuellen Eingaben keine automatisch geprüften Namen annehmen.
- Seitenketten dürfen nicht selbst weiter verzweigt sein. Zyklische und ungesättigte Stoffe sind noch nicht unterstützt.
- Lange und stark verzweigte Moleküle können für A4 zu breit sein. Bindungslängen anpassen und PDF prüfen.
- Der Windows-GUI-Build läuft als eigener CI-Job. Eine erfolgreiche Kompilierung allein ist noch kein interaktiver Windows-GUI-Test; bitte lokal starten und Formeln/PDF kontrollieren.

## Weiterer Entwicklungsweg

Nach lokalem GUI-Test: IUPAC-Hauptkettenbestimmung, korrekte Benennung und kontrollierter Zufallsgenerator; dann JSON/CLI-Anbindung für Arbeitsblätter und später ein Starter für weitere Programme in `hilfsprogramme`.

Gesamtplan: [../PLAN-CHEMFIG-UND-GUI.md](../PLAN-CHEMFIG-UND-GUI.md)
