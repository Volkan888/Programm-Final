// Einfaches Verwaltungsprogramm für Kunden, Artikel und Mitarbeiter  // Kommentar
// Mit Admin-Login, JSON-Speicherung, Urlaubskalender, Resturlaub und Druck über Notepad  // Kommentar

using System;                      // Basisfunktionen (Console usw.)
using System.Collections.Generic;  // Für List<T>
using System.Diagnostics;          // Für Process.Start (Notepad)
using System.Globalization;        // Für deutsches Datums-/Zahlformat
using System.IO;                   // Für Dateioperationen
using System.Linq;                // Für LINQ (OrderBy, Where, usw.)
using System.Text.Json;            // Für JSON-Speicherung

// ===============================
// ADMIN KONFIGURATION
// ===============================

class AdminConfig                       // Klasse für Admin-Einstellungen
{
    public string Passwort { get; set; } = "Admin"; // Standardpasswort
}

// ===============================
// DATENKLASSEN
// ===============================

class Kunde                              // Klasse für Kunde
{
    public int Id { get; set; }          // Laufende Kunden-ID
    public string Name { get; set; } = "";      // Name des Kunden
    public string Adresse { get; set; } = "";   // Straße, Hausnummer
    public string PLZ { get; set; } = "";       // Postleitzahl
    public string Ort { get; set; } = "";       // Wohnort
    public string Telefon { get; set; } = "";   // Telefonnummer
    public string Email { get; set; } = "";     // E-Mail-Adresse
    public DateTime AngelegtAm { get; set; } = DateTime.Now; // Erstellungsdatum
}

class Artikel                            // Klasse für Artikel
{
    public int Id { get; set; }          // Laufende Artikel-ID
    public string ArtikelNummer { get; set; } = ""; // Artikelnummer (automatisch nach Id)
    public string Bezeichnung { get; set; } = "";   // Name des Artikels
    public decimal Preis { get; set; }             // Verkaufspreis
    public int Bestand { get; set; }               // Stück im Lager
}

class Mitarbeiter                        // Klasse für Mitarbeiter
{
    public int Id { get; set; }          // Laufende Mitarbeiter-ID
    public string Vorname { get; set; } = "";   // Vorname
    public string Nachname { get; set; } = "";  // Nachname
    public decimal Gehalt { get; set; }         // Gehalt in Euro
    public int JahresUrlaub { get; set; } = 30; // Standard-Urlaubstage pro Jahr
    public List<DateTime> Urlaubstage { get; set; } = new(); // Liste der Urlaubstage
}

class Daten                              // Sammelklasse für alle Listen
{
    public List<Kunde> Kunden { get; set; } = new();         // Liste aller Kunden
    public List<Artikel> Artikel { get; set; } = new();      // Liste aller Artikel
    public List<Mitarbeiter> Mitarbeiter { get; set; } = new(); // Liste aller Mitarbeiter
}

// ===============================
// HAUPTPROGRAMM
// ===============================

class Program                            // Hauptklasse
{
    static Daten daten = new Daten();    // Zentrale Dateninstanz
    static AdminConfig admin = new AdminConfig(); // Admin-Konfiguration

    const string Datei = "daten.json";        // Dateiname für Daten
    const string AdminDatei = "admin.json";   // Dateiname für Admin-Config

    static int kundenID = 1;             // Start-ID für Kunden
    static int artikelID = 1;            // Start-ID für Artikel
    static int mitarbeiterID = 1;        // Start-ID für Mitarbeiter

    static void Main()                   // Einstiegspunkt
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE"); // Deutsches Format setzen
        Console.OutputEncoding = System.Text.Encoding.UTF8;                 // UTF-8 für Sonderzeichen

        AdminLaden();   // Admin-Daten laden
        AdminLogin();   // Login abfragen

        if (File.Exists(Datei)) // Prüfen ob Daten-Datei existiert
        {
            Laden();    // Daten laden
        }

        Hauptmenü();    // Hauptmenü starten
    }

    // ===============================
    // ADMIN-FUNKTIONEN
    // ===============================

    static void AdminLaden()             // Admin-Konfig aus Datei laden
    {
        try                               // Fehler abfangen
        {
            string json = File.ReadAllText(AdminDatei);                   // Dateiinhalt lesen
            var temp = JsonSerializer.Deserialize<AdminConfig>(json);     // JSON zu Objekt
            if (temp != null) admin = temp;                               // Falls gültig, übernehmen
        }
        catch                             // Wenn etwas schiefgeht
        {
            AdminSpeichern();             // Neue Standarddatei erstellen
        }
    }

    static void AdminSpeichern()         // Admin-Konfig speichern
    {
        string json = JsonSerializer.Serialize(admin, new JsonSerializerOptions { WriteIndented = true }); // Objekt zu JSON
        File.WriteAllText(AdminDatei, json); // JSON in Datei schreiben
    }

    static void AdminLogin()             // Admin-Login
    {
        Console.Clear();                 // Bildschirm leeren
        Console.WriteLine("==== ADMIN LOGIN ====\n"); // Überschrift

        while (true)                     // Endlosschleife bis korrektes Passwort
        {
            Console.Write("Passwort: "); // Eingabeaufforderung
            string pw = PasswortLesen(); // Passwort einlesen

            if (pw == admin.Passwort)    // Prüfen ob korrekt
            {
                return;                  // Login ok → zurück ins Programm
            }

            Console.WriteLine("Falsches Passwort.\n"); // Hinweis bei Fehler
        }
    }

    static void PasswortÄndern()         // Passwort ändern
    {
        Console.Clear();                 // Bildschirm leeren
        Console.WriteLine("==== PASSWORT ÄNDERN ====\n"); // Überschrift

        Console.Write("Aktuelles Passwort: "); // Alte Eingabe
        string alt = PasswortLesen();          // Altes Passwort lesen

        if (alt != admin.Passwort)            // Überprüfen
        {
            Weiter("Falsches Passwort.");     // Fehlermeldung
            return;                           // Abbrechen
        }

        Console.Write("Neues Passwort: ");    // Neues Passwort
        string neu1 = PasswortLesen();        // Neue Eingabe lesen

        Console.Write("Neues Passwort wiederholen: "); // Wiederholung
        string neu2 = PasswortLesen();                 // Nochmals lesen

        if (neu1 != neu2)                     // Prüfen ob gleich
        {
            Weiter("Passwörter stimmen nicht überein."); // Hinweis
            return;                         // Abbrechen
        }

        admin.Passwort = neu1;              // Passwort setzen
        AdminSpeichern();                   // In Datei speichern
        Weiter("Passwort wurde geändert."); // Rückmeldung
    }

    static string PasswortLesen()          // Passwort mit Sternchen einlesen
    {
        string pw = "";                    // Leerer String
        while (true)                       // Schleife bis Enter
        {
            ConsoleKeyInfo key = Console.ReadKey(true); // Taste lesen (ohne Anzeige)

            if (key.Key == ConsoleKey.Enter)            // Wenn Enter
            {
                Console.WriteLine();                    // Zeilenumbruch
                return pw;                              // Passwort zurückgeben
            }

            if (key.Key == ConsoleKey.Backspace)        // Wenn Backspace
            {
                if (pw.Length > 0)                      // Nur wenn Zeichen drin
                {
                    pw = pw.Substring(0, pw.Length - 1); // Letztes Zeichen entfernen
                    Console.Write("\b \b");              // Anzeige zurücksetzen
                }
            }
            else                                        // Normale Zeichen
            {
                pw += key.KeyChar;                      // Zeichen anhängen
                Console.Write("*");                     // Stern anzeigen
            }
        }
    }

    // ===============================
    // HAUPTMENÜ
    // ===============================

    static void Hauptmenü()              // Zentrales Menü
    {
        while (true)                     // Endlosschleife bis Beenden
        {
            Console.Clear();             // Bildschirm leeren
            Console.WriteLine("==== HAUPTMENÜ ====\n"); // Überschrift
            Console.WriteLine("1) Kunden");             // Option
            Console.WriteLine("2) Artikel");            // Option
            Console.WriteLine("3) Mitarbeiter");        // Option
            Console.WriteLine("4) Statistiken");        // Option
            Console.WriteLine("5) Passwort ändern");    // Option
            Console.WriteLine("6) Speichern");          // Option
            Console.WriteLine("7) Laden");              // Option
            Console.WriteLine("0) Beenden");            // Option
            Console.Write("\nAuswahl: ");               // Eingabeaufforderung

            string auswahl = Console.ReadLine() ?? "";  // Eingabe lesen

            switch (auswahl)                           // Auswahl auswerten
            {
                case "1": KundenMenü(); break;         // Kunden-Menü
                case "2": ArtikelMenü(); break;        // Artikel-Menü
                case "3": MitarbeiterMenü(); break;    // Mitarbeiter-Menü
                case "4": Statistiken(); break;        // Statistik anzeigen
                case "5": PasswortÄndern(); break;     // Passwort ändern
                case "6": Speichern(); break;          // Daten speichern
                case "7": Laden(); break;              // Daten laden
                case "0": return;                      // Programm beenden
                default: Weiter("Ungültige Eingabe."); break; // Fehlermeldung
            }
        }
    }

    // ===============================
    // KUNDEN
    // ===============================

    static void KundenMenü()            // Untermenü Kunden
    {
        while (true)                    // Schleife für Kunden-Menü
        {
            Console.Clear();            // Bildschirm leeren
            Console.WriteLine("==== KUNDEN ====\n"); // Überschrift
            Console.WriteLine("1) Neuer Kunde");     // Option
            Console.WriteLine("2) Kunden anzeigen"); // Option
            Console.WriteLine("3) Kunden suchen");   // Option
            Console.WriteLine("4) Kunden löschen");  // Option
            Console.WriteLine("5) Kunden bearbeiten"); // Option
            Console.WriteLine("6) Kundenliste drucken"); // Option
            Console.WriteLine("0) Zurück");          // Option
            Console.Write("\nAuswahl: ");            // Eingabeaufforderung

            string auswahl = Console.ReadLine() ?? ""; // Eingabe

            switch (auswahl)            // Auswahl prüfen
            {
                case "1": KundeNeu(); break;             // Neuer Kunde
                case "2": KundenAnzeigen(); break;       // Liste anzeigen
                case "3": KundeSuchen(); break;          // Suche
                case "4": KundeLöschen(); break;         // Löschen
                case "5": KundeBearbeiten(); break;      // Bearbeiten
                case "6": KundenListeDrucken(); break;   // Drucken
                case "0": return;                        // Zurück zum Hauptmenü
                default: Weiter("Ungültige Eingabe."); break; // Hinweis
            }
        }
    }

    static void KundeNeu()              // Neuen Kunden anlegen
    {
        Console.Clear();                // Bildschirm leeren
        Console.WriteLine("==== NEUER KUNDE ====\n"); // Überschrift

        Kunde k = new Kunde();          // Neues Kundenobjekt
        k.Id = kundenID++;              // Neue ID vergeben

        Console.Write("Name: ");        // Eingabe Name
        k.Name = TextEin("Name darf nicht leer sein"); // Name lesen

        Console.Write("Adresse: ");     // Eingabe Adresse
        k.Adresse = TextEin("");        // Adresse lesen

        Console.Write("PLZ: ");         // Eingabe PLZ
        k.PLZ = TextEin("");            // PLZ lesen

        Console.Write("Ort: ");         // Eingabe Ort
        k.Ort = TextEin("");            // Ort lesen

        Console.Write("Telefon: ");     // Eingabe Telefon
        k.Telefon = TextEin("");        // Telefon lesen

        Console.Write("E-Mail: ");      // Eingabe E-Mail
        k.Email = TextEin("");          // E-Mail lesen

        daten.Kunden.Add(k);            // Kunde zur Liste hinzufügen
        Weiter("Kunde gespeichert.");   // Rückmeldung
    }

    static void KundenAnzeigen()        // Alle Kunden anzeigen
    {
        Console.Clear();                // Bildschirm leeren
        Console.WriteLine("==== KUNDENLISTE ====\n"); // Überschrift

        if (daten.Kunden.Count == 0)    // Prüfen ob Kunden existieren
        {
            Weiter("Keine Kunden vorhanden."); // Hinweis
            return;                    // Abbrechen
        }

        foreach (var k in daten.Kunden.OrderBy(x => x.Name)) // Nach Name sortieren
        {
            Console.WriteLine($"{k.Id}: {k.Name} – {k.Ort} – {k.Telefon}"); // Ausgabe
        }

        Weiter();                       // Warten
    }

    static void KundeSuchen()           // Kunde suchen
    {
        Console.Clear();                // Bildschirm leeren
        Console.Write("Suchbegriff: "); // Eingabeaufforderung
        string s = (Console.ReadLine() ?? "").ToLower(); // Eingabe klein

        var treffer = daten.Kunden      // LINQ-Suche
            .Where(k => k.Name.ToLower().Contains(s) ||
                        k.Ort.ToLower().Contains(s) ||
                        k.Telefon.Contains(s))
            .ToList();                  // Liste erzeugen

        Console.WriteLine();            // Leerzeile

        if (treffer.Count == 0)         // Keine Treffer
        {
            Weiter("Keine Treffer.");   // Hinweis
            return;                     // Abbruch
        }

        foreach (var k in treffer)      // Alle Treffer ausgeben
        {
            Console.WriteLine($"{k.Id}: {k.Name} – {k.Ort} – {k.Telefon}"); // Zeile
        }

        Weiter();                       // Warten
    }

    static void KundeBearbeiten()       // Kunde bearbeiten
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== KUNDE BEARBEITEN ====\n"); // Überschrift

        Console.Write("Kunden-ID: ");   // Eingabeaufforderung
        int id = IntEin();              // ID einlesen

        var k = daten.Kunden.FirstOrDefault(x => x.Id == id); // Kunde suchen

        if (k == null)                  // Prüfen ob gefunden
        {
            Weiter("Kunde nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        Console.Write($"Neuer Name (Enter = keine Änderung, aktuell {k.Name}): "); // Info
        string name = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(name)) k.Name = name; // Falls nicht leer, setzen

        Console.Write($"Neue Adresse (Enter = keine Änderung, aktuell {k.Adresse}): "); // Info
        string adr = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(adr)) k.Adresse = adr; // Setzen

        Console.Write($"Neue PLZ (Enter = keine Änderung, aktuell {k.PLZ}): "); // Info
        string plz = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(plz)) k.PLZ = plz; // Setzen

        Console.Write($"Neuer Ort (Enter = keine Änderung, aktuell {k.Ort}): "); // Info
        string ort = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(ort)) k.Ort = ort; // Setzen

        Console.Write($"Neue Telefonnummer (Enter = keine Änderung, aktuell {k.Telefon}): "); // Info
        string tel = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(tel)) k.Telefon = tel; // Setzen

        Console.Write($"Neue E-Mail (Enter = keine Änderung, aktuell {k.Email}): "); // Info
        string mail = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(mail)) k.Email = mail; // Setzen

        Weiter("Kunde aktualisiert.");  // Meldung
    }

    static void KundeLöschen()          // Kunde löschen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== KUNDE LÖSCHEN ====\n"); // Überschrift

        Console.Write("Kunden-ID: ");   // Eingabe
        int id = IntEin();              // ID lesen

        var k = daten.Kunden.FirstOrDefault(x => x.Id == id); // Kunde suchen

        if (k == null)                  // Falls nicht gefunden
        {
            Weiter("Kunde nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        daten.Kunden.Remove(k);         // Kunde entfernen
        Weiter("Kunde gelöscht.");      // Meldung
    }

    static void KundenListeDrucken()    // Kundenliste drucken
    {
        string text = "";               // Textpuffer

        foreach (var k in daten.Kunden.OrderBy(x => x.Name)) // Sortierte Kunden
        {
            text += $"{k.Id}: {k.Name} – {k.Ort} – {k.Telefon}\r\n"; // Zeile anhängen
        }

        Drucken("Kundenliste", text);   // Druck-Funktion aufrufen
        Weiter("Kundenliste exportiert."); // Meldung
    }

    // ===============================
    // ARTIKEL
    // ===============================

    static void ArtikelMenü()           // Untermenü Artikel
    {
        while (true)                    // Schleife
        {
            Console.Clear();            // Bildschirm
            Console.WriteLine("==== ARTIKEL ====\n"); // Überschrift
            Console.WriteLine("1) Neuer Artikel");        // Option
            Console.WriteLine("2) Artikel anzeigen");     // Option
            Console.WriteLine("3) Artikel suchen");       // Option
            Console.WriteLine("4) Artikel löschen");      // Option
            Console.WriteLine("5) Artikel bearbeiten");   // Option
            Console.WriteLine("6) Artikelliste drucken"); // Option
            Console.WriteLine("0) Zurück");               // Option
            Console.Write("\nAuswahl: ");                 // Eingabe

            string auswahl = Console.ReadLine() ?? "";    // Eingabe lesen

            switch (auswahl)             // Auswerten
            {
                case "1": ArtikelNeu(); break;           // Neuer Artikel
                case "2": ArtikelAnzeigen(); break;      // Liste
                case "3": ArtikelSuchen(); break;        // Suche
                case "4": ArtikelLöschen(); break;       // Löschen
                case "5": ArtikelBearbeiten(); break;    // Bearbeiten
                case "6": ArtikelListeDrucken(); break;  // Drucken
                case "0": return;                        // Zurück
                default: Weiter("Ungültige Eingabe."); break; // Hinweis
            }
        }
    }

    static void ArtikelNeu()            // Artikel anlegen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== NEUER ARTIKEL ====\n"); // Überschrift

        Artikel a = new Artikel();      // Neues Artikelobjekt
        a.Id = artikelID++;             // ID setzen
        a.ArtikelNummer = a.Id.ToString(); // Artikelnummer automatisch nach Id

        Console.WriteLine($"Automatisch zugewiesene Artikelnummer: {a.ArtikelNummer}"); // Info

        Console.Write("Bezeichnung: ");           // Eingabe
        a.Bezeichnung = TextEin("Name fehlt");    // Lesen

        Console.Write("Preis (€): ");             // Eingabe
        a.Preis = DecimalEin();                   // Decimal lesen

        Console.Write("Bestand: ");               // Eingabe
        a.Bestand = IntEin();                     // Int lesen

        daten.Artikel.Add(a);                     // Zur Liste hinzufügen
        Weiter("Artikel gespeichert.");           // Meldung
    }

    static void ArtikelAnzeigen()       // Liste aller Artikel
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== ARTIKELLISTE ====\n"); // Überschrift

        if (daten.Artikel.Count == 0)   // Prüfen ob vorhanden
        {
            Weiter("Keine Artikel vorhanden."); // Hinweis
            return;                            // Abbrechen
        }

        foreach (var a in daten.Artikel.OrderBy(x => x.Bezeichnung)) // Sortieren
        {
            Console.WriteLine($"{a.Id}: {a.Bezeichnung} – {a.Preis:C} – Bestand: {a.Bestand} – ArtNr: {a.ArtikelNummer}"); // Ausgabe
        }

        Weiter();                       // Warten
    }

    static void ArtikelSuchen()         // Artikel nach Text suchen
    {
        Console.Clear();                // Bildschirm
        Console.Write("Suchbegriff: "); // Eingabe
        string s = (Console.ReadLine() ?? "").ToLower(); // Klein

        var treffer = daten.Artikel     // LINQ-Suche
            .Where(a => a.Bezeichnung.ToLower().Contains(s) ||
                        a.ArtikelNummer.ToLower().Contains(s))
            .ToList();                  // Liste erzeugen

        Console.WriteLine();            // Leerzeile

        if (treffer.Count == 0)         // Keine
        {
            Weiter("Keine Treffer.");   // Hinweis
            return;                     // Abbruch
        }

        foreach (var a in treffer)      // Treffer ausgeben
        {
            Console.WriteLine($"{a.Id}: {a.Bezeichnung} – {a.Preis:C} – Bestand: {a.Bestand} – ArtNr: {a.ArtikelNummer}"); // Zeile
        }

        Weiter();                       // Warten
    }

    static void ArtikelBearbeiten()     // Artikel ändern
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== ARTIKEL BEARBEITEN ====\n"); // Überschrift

        Console.Write("Artikel-ID: ");  // Eingabe
        int id = IntEin();              // ID lesen

        var a = daten.Artikel.FirstOrDefault(x => x.Id == id); // Artikel suchen

        if (a == null)                  // Prüfen
        {
            Weiter("Artikel nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        Console.Write($"Neue Bezeichnung (Enter = keine Änderung, aktuell {a.Bezeichnung}): "); // Info
        string bez = Console.ReadLine() ?? ""; // Eingabe
        if (!string.IsNullOrWhiteSpace(bez)) a.Bezeichnung = bez; // Setzen

        Console.Write($"Neuer Preis (Enter = keine Änderung, aktuell {a.Preis:C}): "); // Info
        string pre = Console.ReadLine() ?? ""; // Eingabe
        if (decimal.TryParse(pre, out decimal p)) a.Preis = p; // Setzen

        Console.Write($"Neuer Bestand (Enter = keine Änderung, aktuell {a.Bestand}): "); // Info
        string bst = Console.ReadLine() ?? ""; // Eingabe
        if (int.TryParse(bst, out int b)) a.Bestand = b; // Setzen

        Weiter("Artikel aktualisiert."); // Meldung
    }

    static void ArtikelLöschen()        // Artikel löschen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== ARTIKEL LÖSCHEN ====\n"); // Überschrift

        Console.Write("Artikel-ID: ");  // Eingabe
        int id = IntEin();              // Lesen

        var a = daten.Artikel.FirstOrDefault(x => x.Id == id); // Suchen

        if (a == null)                  // Prüfen
        {
            Weiter("Artikel nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        daten.Artikel.Remove(a);        // Entfernen
        Weiter("Artikel gelöscht.");    // Meldung
    }

    static void ArtikelListeDrucken()   // Artikel-Liste drucken
    {
        string text = "";               // Puffer

        foreach (var a in daten.Artikel.OrderBy(x => x.Bezeichnung)) // Sortiert
        {
            text += $"{a.Id}: {a.Bezeichnung} – {a.Preis:C} – Bestand: {a.Bestand} – ArtNr: {a.ArtikelNummer}\r\n"; // Zeile
        }

        Drucken("Artikelliste", text);  // Druck
        Weiter("Artikelliste exportiert."); // Meldung
    }

    // ===============================
    // MITARBEITER & URLAUB
    // ===============================

    static void MitarbeiterMenü()       // Untermenü Mitarbeiter
    {
        while (true)                    // Schleife
        {
            Console.Clear();            // Bildschirm
            Console.WriteLine("==== MITARBEITER ====\n"); // Überschrift
            Console.WriteLine("1) Neuer Mitarbeiter");         // Option
            Console.WriteLine("2) Mitarbeiter anzeigen");      // Option
            Console.WriteLine("3) Urlaub eintragen");          // Option
            Console.WriteLine("4) Urlaubstag löschen");        // Option
            Console.WriteLine("5) Urlaubskalender anzeigen");  // Option
            Console.WriteLine("6) Urlaubskalender drucken");   // Option
            Console.WriteLine("7) Mitarbeiter bearbeiten");    // Option
            Console.WriteLine("8) Mitarbeiterliste drucken");  // Option
            Console.WriteLine("0) Zurück");                    // Option
            Console.Write("\nAuswahl: ");                      // Eingabe

            string auswahl = Console.ReadLine() ?? "";         // Eingabe lesen

            switch (auswahl)               // Auswerten
            {
                case "1": MitarbeiterNeu(); break;            // Neu
                case "2": MitarbeiterAnzeigen(); break;       // Liste
                case "3": UrlaubHinzufügen(); break;          // Urlaub eintragen
                case "4": UrlaubLöschen(); break;             // Urlaub löschen
                case "5": Urlaubskalender(); break;           // Kalender anzeigen
                case "6": UrlaubskalenderDrucken(); break;    // Kalender drucken
                case "7": MitarbeiterBearbeiten(); break;     // Bearbeiten
                case "8": MitarbeiterListeDrucken(); break;   // Drucken
                case "0": return;                             // Zurück
                default: Weiter("Ungültige Eingabe."); break; // Hinweis
            }
        }
    }

    static void MitarbeiterNeu()        // Neuen Mitarbeiter anlegen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== NEUER MITARBEITER ====\n"); // Überschrift

        Mitarbeiter m = new Mitarbeiter(); // Neuer Mitarbeiter
        m.Id = mitarbeiterID++;            // ID setzen

        Console.WriteLine($"Automatisch zugewiesene Mitarbeiter-Nr.: {m.Id}"); // Info

        Console.Write("Vorname: ");     // Eingabe
        m.Vorname = TextEin("Vorname fehlt"); // Lesen

        Console.Write("Nachname: ");    // Eingabe
        m.Nachname = TextEin("Nachname fehlt"); // Lesen

        Console.Write("Gehalt (€): "); // Eingabe
        m.Gehalt = DecimalEin();       // Decimal lesen

        daten.Mitarbeiter.Add(m);      // Zur Liste hinzufügen
        Weiter("Mitarbeiter gespeichert."); // Meldung
    }

    static void MitarbeiterAnzeigen()   // Alle Mitarbeiter anzeigen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== MITARBEITERLISTE ====\n"); // Überschrift

        if (daten.Mitarbeiter.Count == 0) // Prüfen
        {
            Weiter("Keine Mitarbeiter vorhanden."); // Hinweis
            return;                               // Abbruch
        }

        foreach (var m in daten.Mitarbeiter)       // Liste durchgehen
        {
            int rest = m.JahresUrlaub - m.Urlaubstage.Count; // Resturlaub berechnen
            Console.WriteLine($"{m.Id}: {m.Vorname} {m.Nachname} – Gehalt: {m.Gehalt:C} – Urlaubstage: {m.Urlaubstage.Count} – Resturlaub: {rest}"); // Zeile
        }

        Weiter();                       // Warten
    }

    static void MitarbeiterBearbeiten() // Mitarbeiter bearbeiten
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== MITARBEITER BEARBEITEN ====\n"); // Überschrift

        Console.Write("Mitarbeiter-ID: "); // Eingabe
        int id = IntEin();              // Lesen

        var m = daten.Mitarbeiter.FirstOrDefault(x => x.Id == id); // Suchen

        if (m == null)                  // Prüfen
        {
            Weiter("Mitarbeiter nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        Console.Write($"Neuer Vorname (Enter = keine Änderung, aktuell {m.Vorname}): "); // Info
        string v = Console.ReadLine() ?? "";  // Eingabe
        if (!string.IsNullOrWhiteSpace(v)) m.Vorname = v; // Setzen

        Console.Write($"Neuer Nachname (Enter = keine Änderung, aktuell {m.Nachname}): "); // Info
        string n = Console.ReadLine() ?? "";  // Eingabe
        if (!string.IsNullOrWhiteSpace(n)) m.Nachname = n; // Setzen

        Console.Write($"Neues Gehalt (Enter = keine Änderung, aktuell {m.Gehalt:C}): "); // Info
        string g = Console.ReadLine() ?? "";  // Eingabe
        if (decimal.TryParse(g, out decimal geh)) m.Gehalt = geh; // Setzen

        Console.Write($"Neuer Jahresurlaub (Enter = keine Änderung, aktuell {m.JahresUrlaub}): "); // Info
        string ju = Console.ReadLine() ?? "";  // Eingabe
        if (int.TryParse(ju, out int jUrlaub)) m.JahresUrlaub = jUrlaub; // Setzen

        Weiter("Mitarbeiter aktualisiert."); // Meldung
    }

    static void UrlaubHinzufügen()      // Urlaubstag hinzufügen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== URLAUB EINTRAGEN ====\n"); // Überschrift

        Console.Write("Mitarbeiter-ID: "); // Eingabe
        int id = IntEin();              // Lesen

        var m = daten.Mitarbeiter.FirstOrDefault(x => x.Id == id); // Suchen

        if (m == null)                  // Prüfen
        {
            Weiter("Mitarbeiter nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        Console.Write("Urlaubstag (TT.MM.JJJJ): "); // Eingabe Datum
        string s = Console.ReadLine() ?? "";        // Lesen

        if (!DateTime.TryParse(s, out DateTime tag)) // Versuch
        {
            Weiter("Ungültiges Datum."); // Hinweis
            return;                     // Abbruch
        }

        tag = tag.Date;                 // Nur Datum ohne Uhrzeit

        if (!m.Urlaubstage.Contains(tag)) // Nur wenn nicht vorhanden
        {
            m.Urlaubstage.Add(tag);      // Tag hinzufügen
            Weiter("Urlaubstag eingetragen."); // Meldung
        }
        else
        {
            Weiter("Dieser Urlaubstag ist schon eingetragen."); // Hinweis
        }
    }

    static void UrlaubLöschen()         // Urlaubstag löschen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== URLAUBSTAG LÖSCHEN ====\n"); // Überschrift

        Console.Write("Mitarbeiter-ID: "); // Eingabe
        int id = IntEin();              // Lesen

        var m = daten.Mitarbeiter.FirstOrDefault(x => x.Id == id); // Suchen

        if (m == null)                  // Prüfen
        {
            Weiter("Mitarbeiter nicht gefunden."); // Hinweis
            return;                     // Abbruch
        }

        if (m.Urlaubstage.Count == 0)   // Keine Urlaubstage
        {
            Weiter("Dieser Mitarbeiter hat noch keine Urlaubstage."); // Hinweis
            return;                     // Abbruch
        }

        Console.WriteLine("Eingetragene Urlaubstage:"); // Liste anzeigen
        for (int i = 0; i < m.Urlaubstage.Count; i++)   // Durchgehen
        {
            Console.WriteLine($"{i + 1}) {m.Urlaubstage[i]:dd.MM.yyyy}"); // Zeile
        }

        Console.Write("Nummer zum Löschen: "); // Eingabe
        int nr = IntEin();                     // Lesen

        if (nr < 1 || nr > m.Urlaubstage.Count) // Prüfen
        {
            Weiter("Ungültige Auswahl.");      // Hinweis
            return;                            // Abbruch
        }

        m.Urlaubstage.RemoveAt(nr - 1);        // Urlaubstag entfernen
        Weiter("Urlaubstag gelöscht.");        // Meldung
    }

    static void Urlaubskalender()       // Alle Urlaubstage anzeigen
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== URLAUBSKALENDER ====\n"); // Überschrift

        var liste = daten.Mitarbeiter   // Liste aufbauen
            .SelectMany(m => m.Urlaubstage.Select(t => new { Datum = t.Date, Name = $"{m.Vorname} {m.Nachname}" }))
            .OrderBy(x => x.Datum)      // Nach Datum sortieren
            .ToList();                  // Liste daraus machen

        if (liste.Count == 0)           // Prüfen
        {
            Weiter("Keine Urlaubstage eingetragen."); // Hinweis
            return;                     // Abbruch
        }

        foreach (var e in liste)        // Ausgeben
        {
            Console.WriteLine($"{e.Datum:dd.MM.yyyy} – {e.Name}"); // Zeile
        }

        Weiter();                       // Warten
    }

    static void UrlaubskalenderDrucken() // Urlaubskalender drucken
    {
        var liste = daten.Mitarbeiter   // Gleiche Liste wie Anzeige
            .SelectMany(m => m.Urlaubstage.Select(t => new { Datum = t.Date, Name = $"{m.Vorname} {m.Nachname}" }))
            .OrderBy(x => x.Datum)
            .ToList();

        if (liste.Count == 0)           // Prüfen
        {
            Weiter("Keine Urlaubstage eingetragen."); // Hinweis
            return;
        }

        string text = "";               // Puffer

        foreach (var e in liste)        // Durchgehen
        {
            text += $"{e.Datum:dd.MM.yyyy} – {e.Name}\r\n"; // Zeile
        }

        Drucken("Urlaubskalender", text); // Drucken
        Weiter("Urlaubskalender exportiert."); // Meldung
    }

    static void MitarbeiterListeDrucken() // Mitarbeiterliste drucken
    {
        string text = "";               // Puffer

        foreach (var m in daten.Mitarbeiter.OrderBy(x => x.Nachname)) // Sortiert
        {
            int rest = m.JahresUrlaub - m.Urlaubstage.Count;          // Resturlaub
            text += $"{m.Id}: {m.Vorname} {m.Nachname} – Gehalt: {m.Gehalt:C} – Urlaubstage: {m.Urlaubstage.Count} – Resturlaub: {rest}\r\n"; // Zeile
        }

        Drucken("Mitarbeiterliste", text); // Druckfunktion
        Weiter("Mitarbeiterliste exportiert."); // Meldung
    }

    // ===============================
    // STATISTIKEN
    // ===============================

    static void Statistiken()           // Kleine Übersicht
    {
        Console.Clear();                // Bildschirm
        Console.WriteLine("==== STATISTIK ====\n"); // Überschrift

        Console.WriteLine($"Kunden gesamt: {daten.Kunden.Count}");   // Anzahl Kunden
        Console.WriteLine($"Artikel gesamt: {daten.Artikel.Count}"); // Anzahl Artikel
        Console.WriteLine($"Mitarbeiter gesamt: {daten.Mitarbeiter.Count}"); // Anzahl Mitarbeiter

        decimal lagerwert = daten.Artikel.Sum(a => a.Preis * a.Bestand); // Lagerwert berechnen
        Console.WriteLine($"Lagerwert (VK): {lagerwert:C}");             // Ausgabe

        if (daten.Artikel.Any())                    // Nur wenn Artikel vorhanden
        {
            var knapp = daten.Artikel.OrderBy(a => a.Bestand).First(); // Artikel mit wenig Bestand
            Console.WriteLine($"Niedrigster Bestand: {knapp.Bezeichnung} (Bestand: {knapp.Bestand})");
        }

        if (daten.Mitarbeiter.Any())                // Nur wenn Mitarbeiter vorhanden
        {
            var vielUrlaub = daten.Mitarbeiter.OrderByDescending(m => m.Urlaubstage.Count).First(); // Meiste Urlaubstage
            int rest = vielUrlaub.JahresUrlaub - vielUrlaub.Urlaubstage.Count;                       // Resturlaub
            Console.WriteLine($"Meiste Urlaubstage: {vielUrlaub.Vorname} {vielUrlaub.Nachname} ({vielUrlaub.Urlaubstage.Count} Tage, Rest: {rest})");
        }

        int gesamtRest = daten.Mitarbeiter.Sum(m => m.JahresUrlaub - m.Urlaubstage.Count); // Summe Resturlaub
        Console.WriteLine($"Resturlaub gesamt (alle Mitarbeiter): {gesamtRest} Tage");

        Weiter();                       // Warten
    }

    // ===============================
    // SPEICHERN / LADEN
    // ===============================

    static void Speichern()             // Daten speichern
    {
        string json = JsonSerializer.Serialize(daten, new JsonSerializerOptions { WriteIndented = true }); // Objekt zu JSON
        File.WriteAllText(Datei, json); // JSON in Datei
        Weiter("Daten gespeichert.");   // Meldung
    }

    static void Laden()                 // Daten laden
    {
        try                             // Fehler abfangen
        {
            string json = File.ReadAllText(Datei);        // Datei lesen
            var temp = JsonSerializer.Deserialize<Daten>(json); // JSON zu Objekt
            if (temp != null) daten = temp;               // Falls gültig, übernehmen

            // IDs nach Laden anpassen, damit fortlaufende Nummern stimmen
            kundenID = daten.Kunden.Any() ? daten.Kunden.Max(k => k.Id) + 1 : 1;            // Nächste Kunden-ID
            artikelID = daten.Artikel.Any() ? daten.Artikel.Max(a => a.Id) + 1 : 1;         // Nächste Artikel-ID
            mitarbeiterID = daten.Mitarbeiter.Any() ? daten.Mitarbeiter.Max(m => m.Id) + 1 : 1; // Nächste Mitarbeiter-ID

            Weiter("Daten geladen.");   // Meldung
        }
        catch                           // Beim Fehler
        {
            daten = new Daten();        // Neue leere Daten
            kundenID = artikelID = mitarbeiterID = 1; // IDs zurücksetzen
            Weiter("Fehler beim Laden, neue leere Daten erstellt."); // Hinweis
        }
    }

    // ===============================
    // DRUCKFUNKTION
    // ===============================

    static void Drucken(string titel, string text) // Einfache "Druck" Funktion
    {
        string dateiname = $"Druck_{titel}_{DateTime.Now:yyyyMMdd_HHmmss}.txt"; // Dateiname bauen

        string inhalt =
            titel + Environment.NewLine + // Titel
            "Erstellt am: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + Environment.NewLine + // Datum
            "----------------------------------------" + Environment.NewLine + // Trennlinie
            text; // Der eigentliche Inhalt

        File.WriteAllText(dateiname, inhalt); // Text in Datei schreiben

        Process.Start(new ProcessStartInfo   // Notepad starten
        {
            FileName = "notepad.exe",        // Programm
            Arguments = dateiname,           // Datei öffnen
            UseShellExecute = false          // Direkter Start
        });

        Console.WriteLine("Datei erstellt und in Notepad geöffnet."); // Meldung
    }

    // ===============================
    // HILFSFUNKTIONEN
    // ===============================

    static void Weiter(string text = "Weiter mit Taste...") // Pause-Funktion
    {
        Console.WriteLine("\n" + text); // Meldung ausgeben
        Console.ReadKey();              // Taste abwarten
    }

    static string TextEin(string fehlerWennLeer) // Text eingeben mit optionaler Pflicht
    {
        while (true)                   // Schleife bis ok
        {
            string s = Console.ReadLine() ?? ""; // Eingabe lesen
            if (!string.IsNullOrWhiteSpace(s)) return s; // Wenn Text da, zurück

            if (!string.IsNullOrEmpty(fehlerWennLeer))   // Falls Fehlermeldung gesetzt
            {
                Console.Write(fehlerWennLeer + ": ");    // Hinweis zeigen
            }
            else
            {
                return "";                               // Leere Eingabe zulassen
            }
        }
    }

    static int IntEin()                // Ganzzahl einlesen
    {
        while (true)                   // Schleife
        {
            string s = Console.ReadLine() ?? ""; // Eingabe
            if (int.TryParse(s, out int wert)) return wert; // Wenn ok, zurück
            Console.Write("Ungültige Zahl, erneut: "); // Fehlermeldung
        }
    }

    static decimal DecimalEin()        // Dezimalzahl einlesen
    {
        while (true)                   // Schleife
        {
            string s = Console.ReadLine() ?? ""; // Eingabe
            if (decimal.TryParse(s, out decimal wert)) return wert; // Wenn ok, zurück
            Console.Write("Ungültiger Betrag, erneut: "); // Fehlermeldung
        }
    }
}
