# Entwicklungsplan AtomeAkt: chemfig, Schnittstellen und GUI

**Stand:** 2026-11-18  
**Status:** Anforderungen und Zielrichtung vom Nutzer bestätigt; technische Ausgestaltung ist ein Vorschlag, keine bereits implementierte Funktion.  
**Geltungsbereich:** `hilfsprogramme/AtomeAkt`, mit späterer Einbindung weiterer Hilfsprogramme.  
**Arbeitsmodus:** Schrittweise Entwicklung; zuerst Bestand prüfen, dann kleine isolierte Änderungen, Tests und erst danach Integration. Bestehende Skripte und Funktionen nicht ungeprüft ersetzen.

## 1. Verbindliches Gesamtziel

AtomeAkt soll zuverlässig **wirkliche ausführliche Strukturformeln** und **Halbstrukturformeln** von **verzweigten Alkanen** als sauberen, direkt in LaTeX nutzbaren `chemfig`-Code erzeugen. Die Ausgabe muss chemisch korrekt, visuell übersichtlich und **für Menschen leicht lesbar und manuell editierbar** sein.

Der Code soll durch ChatGPT bzw. automatisierte Arbeitsblattgenerierung **ohne schwer lesbare `\mathrm`-Konstruktionen** verwendbar sein, z. B. für Aufgaben zur Isomerie und IUPAC-Nomenklatur verzweigter Alkane in Klasse 10.

Zusätzlich soll eine GUI für den Nutzer entstehen. **Später** soll dieselbe Oberfläche einen einfachen Zugang zu den vielen anderen Programmen und Skripten des Repositories `hilfsprogramme` ermöglichen.

### Zwei verbindliche Ausgabeformen

1. **Ausführliche Strukturformel (vollständig):** Alle C- und H-Atome und **sämtliche C–C- und C–H-Bindungen** sind explizit dargestellt. Keine bloße Skelettformel und keine implizit weggelassenen H-Atome.
2. **Halbstrukturformel:** Zusammengefasste Gruppen wie `CH_3`, `CH_2` und `CH`, sichtbar und eindeutig verbundene Haupt- und Seitenketten; gut für die Nomenklatur.

**Nicht als Ersatz geeignet:** Skelettformeln. Diese wären höchstens ein zusätzliches, späteres Ausgabeformat.

### Qualitätsanforderungen an TeX

- Direkt einsetzbares, standardkonformes `\chemfig{...}`; keine Umwege über `\mathrm{...}`.
- Klar formatierter, mehrzeiliger, **deterministischer** Quelltext mit Einrückungen, sinnvollen Kommentaren, z. B. zu Hauptkette, Verzweigungspositionen und Ausgaberichtung.
- **`\definesubmol` gezielt einsetzen**, wo es Wiederholungen verkürzt und die manuelle Editierbarkeit tatsächlich verbessert. Keine unübersichtliche Makroflut oder von globalen Definitionen abhängige Einzelbeispiele.
- Vollständiger kopierbarer LaTeX-Block, optional auch nur der `\chemfig`-Ausdruck; Makrodefinitionen bei Bedarf im selben Block oder eindeutig dokumentiert.
- Grafisch klare Abstände, definierte Bindungsrichtungen und -längen, Vermeidung überlagerter Atom- und Bindungsbeschriftungen, gute Lesbarkeit auf A4-Arbeitsblättern.
- Kompilationsprüfung und Sichtkontrolle mit Testfällen. Formatierung, Kommentare und Submol-Makros dürfen die Chemie nicht verändern.
- Ausgabe soll sich in die bestehende LaTeX-Materialbasis (u. a. `kw-layout.sty`) einfügen; keine unnötigen Paketabhängigkeiten.

## 2. Ausgangsbefund (Review vom 2026-11-18)

- Projekt: VB.NET / SDK-Projekt `AtomeAkt/AtomeAkt/AtomeAkt.vbproj` mit `net8.0`.
- `Kohlenwasserstoff.vb`: zufallsbasierte Erzeugung von Haupt- und Seitenketten und Namensbildung.
- `Atom.vb`: Atom-Nachbarschaften, `SimpleFormular`, `LatexFromSimpleChainFormular`.
- `Module1.vb`: derzeit verschiedene Hilfsprogramme und LEK-Erzeugung; `CreateLEKAlkane` verwendet den vorhandenen Generator.
- Bestehende Chemfig-Ausgabe repariert Text mit `Replace`, Regex und festen `Substring`-Indizes. Das ist bei komplexen Verzweigungen fragil.
- Die Namensvergabe orientiert sich an erzeugten Ketten und Summen von Lokanten; eine vollständige regelgerechte Hauptketten- und Lokantenauswahl ist damit noch nicht hinreichend abgesichert.
- Zufallsfunktionen und Verzweigungspositionen brauchen Vorabprüfung auf zulässige und ausreichend viele freie Plätze; keine unbeschränkten Auswahlschleifen.
- **Noch nicht durchgeführt:** VB.NET-Build, systematische Laufzeittests, TeX-Kompilations- und Bildprüfungen.

## 3. Architektur (Vorschlag)

```text
Eingabe (festes Molekül / Namensangabe / Zufallsparameter)
  -> Chemischer Molekülgraph als Quelle der Wahrheit
  -> Chemische Validierung, kanonische Beschreibung, IUPAC-Name
  -> Layoutmodell (Hauptkette, Seitenketten, Winkel/Abstände)
  -> Renderer: Vollstruktur | Halbstruktur
  -> chemfig-Code + optional TeX-Dokument + Metadaten
  -> CLI/JSON-Schnittstelle und später GUI
```

**Regel:** Chemische Topologie, Nomenklatur und gezeichnetes Layout strikt trennen. Keine nachträglichen Regex-Manipulationen an chemischen Gruppen, um grafische Sonderfälle zu reparieren. Beide Renderer verwenden **denselben validierten Molekülgraphen**.

### Ein- und Ausgaben

- Zuerst **ein konkretes Molekül** reproduzierbar aus Strukturangabe, einer geprüften internen Repräsentation oder explizit angegebenen Seitenketten erzeugen.
- Danach **Zufallsgenerator** für Unterrichtsaufgaben mit festlegbaren Grenzen (Kettenlängen, Verzweigungsgrad, zugelassene Seitenketten, Ausschlüsse, Seed).
- Fachlich korrekte IUPAC-Benennung getrennt ermitteln und prüfen; mehrdeutige/ungültige Eingaben mit verständlichen Fehlermeldungen zurückweisen.
- **Zwei Ausgabeweisen:** `snippet` (gut kopierbarer `\chemfig`-Block) und `standalone` (kompilierbare Testdatei mit benötigten `\usepackage`-/Submol-Definitionen).
- Metadaten: Name, Summenformel, Anzahl C/H, gewählte Hauptkette, Verzweigungslokanten, Ausgabeart, Seed, Renderer-Version; keine personenbezogenen Daten.

## 4. Schnittstelle für ChatGPT und Arbeitsblätter

**Verbindliches Ziel:** ChatGPT soll aus einer strukturierten Anfrage direkt verwendbaren Chemfig-Code erhalten oder ein reproduzierbares lokales Hilfsprogramm mit maschinenlesbarer Ausgabe ansteuern können.

**Vorgeschlagen:** Kleine CLI neben der bisherigen VB.NET-Anwendung mit versioniertem JSON-Ein-/Ausgabeformat; kein GUI-Zwang, keine manuellen Zwischenschritte. Eine spätere Integration kann diese CLI als lokalen Dienst oder Bibliotheksfunktion nutzen.

Beispiel einer **noch festzulegenden** Anfrage:

```json
{
  "schemaVersion": "1",
  "molecule": {"name": "2,3-Dimethylbutan"},
  "render": {
    "kind": "halbstruktur",
    "style": "arbeitsblatt",
    "comments": true,
    "submols": "when-useful"
  }
}
```

Die tatsächliche API wird erst nach Festlegung der Eingabegrammatik und Tests verbindlich. **Nicht behaupten**, dass beliebige IUPAC-Namen schon sicher geparst werden können. Für den ersten Schritt ist auch eine explizite Hauptketten-/Substituenten-Repräsentation möglich.

Benötigte Arbeitsblatt-Workflows:

- `Name -> Halbstrukturformel` und `Name -> ausführliche Strukturformel`, falls Namensparser verfügbar und validiert.
- `Struktur -> (geprüfter) Name`: Lösungen, Erwartungshorizonte und automatische Kontrolle.
- Aufgaben mit **nur Struktur** und getrenntem **Lösungsnamen**; mehrere eindeutige, validierte Varianten pro Aufgabenblatt.
- Einzelbeispiele und Stapelerzeugung; identischer Seed => identische Ergebnisse.
- Klare Ausschnitte, die direkt in TeX/LaTeX eingefügt werden können; optionale kommentarreiche Fassung für manuelle Nachbearbeitung.
- Keine Abhängigkeit von Schülerlisten; DSGVO-bewusste Protokolle und Testdaten.

## 5. GUI und weitere Hilfsprogramme

### GUI für den Chemfig-Generator (nach stabilem Kern)

- Eingabe eines konkreten Moleküls sowie später konfigurierte Zufallserzeugung.
- Umschaltung **ausführliche Strukturformel / Halbstrukturformel**.
- Sofortige Vorschau und daneben **editierbarer oder kopierbarer Chemfig-Quelltext**.
- Steuerelemente für Winkel, Abstände, kompakte/große Darstellung, Kommentare, Submol-Verwendung, Seed.
- Fachliche Informationen: IUPAC-Name, Summenformel und Validierungsfehler.
- Aktionen: TeX kopieren, `.tex` speichern, PDF-Vorschau erzeugen, Stapel exportieren.
- GUI nutzt dieselbe Bibliothek/Schnittstelle wie CLI; keine zweite Implementierung der Chemie- oder Rendererlogik.

**Technologieentscheidung offen:** Bestehendes VB.NET/.NET-8-Projekt respektieren; später Windows-UI-Optionen nach geringster Komplexität und Wartbarkeit auswählen. Plattformübergreifend nur, wenn tatsächlich benötigt.

### Spätere Erweiterung: gemeinsamer Hilfsprogramm-Starter

- Programme/Skripte im Repository inventarisieren und kategorisieren.
- Einheitlicher Startpunkt mit Such-/Filterfunktion, Beschreibung, Parametereingabe und Ausgabeort.
- Skripte modular über Adapter oder deklarative Metadaten anbinden; keine große monolithische Hauptdatei.
- Chemfig-Generator ist **erster sauber integrierter Anwendungsfall**.
- Bestehende Hilfsprogramme nicht ungeprüft verändern. Persönliche Pfade, Zugangsdaten und Schülerdaten nicht in Git oder gemeinsame Konfiguration übernehmen.

## 6. Verbindlicher Etappenplan

| Phase | Inhalt | Abschlusskriterium | Stand |
|---|---|---|---|
| **0. Bestand sichern und prüfen** | Aktuelle GitHub-Fassung lesen, Branch/Build prüfen, vorhandene Abhängigkeiten und Beispiele inventarisieren; reproduzierbare Baseline schaffen | Bestandsbericht + erfolgreich/fehlerhaft protokollierter Ausgangs-Build | Offen |
| **1. Molekülmodell härten** | C-C-Graph, eindeutige Atom-IDs, Valenzprüfung, Zusammenhang, Azyklizität, H-Zahlen, Formeln, Fehlerbehandlung | Validierte Alkane unabhängig von Rendering | Offen |
| **2. Direkte Halbstrukturformel** | Strukturierter Renderer mit `chemfig`-Branches, gut lesbarem Quelltext und optional Kommentaren/Submols | Referenzfälle chemisch korrekt, kompilierbar, gut editierbar | Offen |
| **3. Wirkliche ausführliche Strukturformel** | Alle C/H-Atome und C-C/C-H-Bindungen explizit zeichnen; Winkel-/Abstandsregeln | Referenzfälle vollständig, ohne Überlagerungen, kompilierbar | Offen |
| **4. Nomenklatur und Generator** | Längste Hauptkette, relevante IUPAC-Tie-Breaks, Lokantenvergleich, Verzweigungen, Seed, Parametergrenzen | Name und Struktur stimmen; ungültige Fälle abgefangen | Offen |
| **5. Qualitätsautomatisierung** | Unit-/Property-Tests, Referenzsuite, `pdflatex`-Smoke-Tests und visuelle Stichproben | Regressionstests für beide Formelarten + schwierige Verzweigungen | Offen |
| **6. Arbeitsblatt-/KI-Schnittstelle** | CLI und dokumentiertes JSON-Schema, Snippet/Standalone, Losungsmodus, Batch-Export | ChatGPT kann gezielt reproduzierbaren lesbaren Chemfig-Code anfordern | Offen |
| **7. Nutzer-GUI** | Eingabe, Vorschau, Einstellungen, Quelltext und Export | Ohne Codekenntnisse im Alltag bedienbar | Offen |
| **8. Hilfsprogramm-Zentrale** | Andere Projektprogramme inventarisieren, Starter und Adapter schrittweise ergänzen | Verschiedene Skripte über einen gemeinsamen Einstieg nutzbar | Später |

**Prioritätsregel:** Zuerst korrekte und gut editierbare **beide** Formelarten, dann standardisierte Schnittstelle; GUI folgt dem stabilen Kern, andere Skripte erst anschließend. Manche Arbeiten aus Phasen 4/5 müssen in Phase 2/3 als frühe Prüffälle parallel erfolgen.

## 7. Tests und fachliche Invarianten

Pflicht-Referenzfälle:

- Butan (ohne Seitenkette)
- 2-Methylbutan
- 2,2-Dimethylpropan (quartäres C)
- 2,3-Dimethylbutan (benachbarte Verzweigungen)
- 2,2,4-Trimethylpentan (mehrere Methylgruppen)
- 3-Ethyl-2-methylhexan (verschiedene Substituenten)
- 3,3-Diethylpentan (zwei längere Substituenten an einem Atom)
- Zusätzliche Fälle mit zwei Verzweigungen an benachbarten und identischen Positionen sowie großen Ketten.

Prüfung je Fall: zusammenhängender azyklischer Graph, keine mehrfachen/fehlenden C-Atome, genau vier Bindungen pro C (unter Berücksichtigung der explizit oder implizit ergänzten H), Summenformel `C_nH_(2n+2)`, korrekter IUPAC-Name, übereinstimmende Topologie beider Renderer, TeX-Kompilation und lesbare Seitengeometrie. Im Vollstrukturmodus ist **jedes** H-Atom mit Bindungsstrich zu sehen.

Tests für zufällige Generation: fester Seed und Reproduzierbarkeit, keine Endlosschleifen, keine ungültigen Parameter/Verzweigungspositionen, keine identischen Ergebnisse bei geforderter Eindeutigkeit.

**Review-Grenze:** Eine formal erfolgreiche TeX-Kompilation beweist noch nicht die chemische oder grafische Korrektheit.

## 8. Arbeits- und Änderungsregeln

1. Vor jeder Änderung aktuelle synchronisierte GitHub-Fassung prüfen; lokale Benutzeränderungen nicht stillschweigend überschreiben.
2. Kleine, einzeln testbare Module bevorzugen: Graph/Validator, Name, Halbstruktur-Renderer, Vollstruktur-Renderer, Layout, CLI, GUI.
3. Bestehende `CreateLEKAlkane` und weitere LEK-/Atom-Skripte zunächst funktionsfähig lassen; Umstellung später kontrolliert über Adapter.
4. Tests mit synthetischen Molekülen, **nicht mit personenbezogenen Schülerdaten**. Keine Schülernamen, privaten Pfade oder Geheimnisse in Commits/Logs.
5. Offene Bugs und Entscheidungen in diesem Plan fortschreiben; Erledigtes ausdrücklich markieren.
6. `chemfig`-Code in tatsächlichen LaTeX-Ausgaben visuell prüfen, insbesondere auf überlappende H-Atome und Seitenketten.
7. Keine voreilige Komplettneuentwicklung und kein Sprachenwechsel, solange sich das Vorhaben in .NET 8 sauber lösen lässt.

## 9. Nächster konkreter Schritt

**Phase 0 beginnen:** Projektstruktur und aufrufbare Formelerzeugung vollständig untersuchen, Build-/LaTeX-Testumgebung feststellen, kleine anonymisierte Baseline mit 7 Referenzmolekülen vorbereiten. Anschließend die Datengrenze zwischen Molekülgraph und Rendering definieren. **Noch keine GUI bauen**, bevor beide Formeltypen zuverlässig generiert werden.

---

**Entscheidungsvermerk:** Die Anforderungen „ausführliche Strukturformeln **und** Halbstrukturformeln“, „gut lesbarer und manuell editierbarer `chemfig`-Code mit sinnvollen Submols/Kommentaren“, „leicht nutzbare KI-/Arbeitsblatt-Schnittstelle“, „GUI für den Nutzer“ sowie „später Zugriff auf weitere Programme/Skripte“ sind Nutzeranforderungen. Alle konkreten API-, Modul- und UI-Entscheidungen bleiben bis zur Umsetzung Vorschläge.
