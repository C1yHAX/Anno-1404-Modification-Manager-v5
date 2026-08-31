# Changelog

All notable changes to the **Anno 1404 Modification Manager** are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com) and the project
follows [Semantic Versioning](https://semver.org).

## [5.0.4] - 2026-07-13

### Fixed
- **The backup dialog no longer asks again and again.** After creating the backup the
  manager saved the backup path but never marked the startup configuration as done, so on
  the next start `Settings.Upgrade()` replaced the current configuration with the previous
  version's and wiped that path again. The backup check then failed on every start and the
  "automatic or manual backup?" dialog reappeared forever.
- **The addon folder is backed up again.** The addon archives were only copied when the
  selected Anno version was an addon version, so anyone running the History Edition as
  "History Edition" (instead of "History Edition Addon") ended up with a maindata-only
  backup. The addon folder is now backed up whenever it exists.
- A backup is only rejected for a missing addon copy when the installation actually has an
  addon folder, and the backup is validated right after it was created - so the manager can
  no longer restart into the very same dialog without saying why.
- Backup error messages are localized (German/English) and explain what to do instead of
  showing "addon subfolder not found.".

**DE** *Behoben:* Der **Sicherungs-Dialog fragt nicht mehr endlos erneut** - der Manager
speicherte den Backup-Pfad, markierte die Ersteinrichtung aber nicht als abgeschlossen,
worauf `Settings.Upgrade()` beim naechsten Start die Einstellungen der Vorversion
darueberschrieb und den Pfad wieder loeschte. Ausserdem wurde der **addon-Ordner nicht
gesichert**, wenn als Version "History Edition" (statt "History Edition Addon") gewaehlt
war - er wird jetzt immer mitgesichert, sofern vorhanden. Fehlermeldungen zur Sicherung
sind jetzt verstaendlich und zweisprachig.

## [5.0.3] — 2026-07-13

### Fixed
- **English translation works again.** The redesigned main window had hard-coded German
  texts, so switching the language to English only affected the old dialogs. Every string
  of the modern window (navigation, mods view incl. status texts/tooltips, categories,
  about, settings incl. the update-check messages) now goes through the language system
  and follows the language setting.

**🇩🇪** *Behoben:* Die **englische Übersetzung** greift wieder — das neue Hauptfenster hatte
fest verdrahtete deutsche Texte; alle Texte laufen jetzt über das Sprachsystem und folgen
der Spracheinstellung.

## [5.0.2] — 2026-07-03

### Fixed
- **Activated mods now actually take effect in the History Edition.** The manager used to
  write all changes into the highest-priority archive (`data5.rda`) — which works on the
  Königsedition (patch RDAs override everything) but is ignored by the History Edition,
  which has no patch layer. Changes are now written **in place into the archive each file
  lives in** (the proven AMM4 behaviour), so activation works on both editions.

**🇩🇪** *Behoben:* Aktivierte Mods kamen in der **History Edition** nicht im Spiel an —
der Manager schrieb die Änderungen in `data5.rda`, das die HE (anders als die
Königsedition mit ihren Patch-RDAs) nicht bevorzugt lädt. Änderungen werden jetzt
**direkt im jeweiligen Archiv** geschrieben (bewährtes AMM4-Verhalten) — funktioniert auf
beiden Editionen. Nach dem Update empfohlen: einmal *Wiederherstellung* → RDAs aus dem
Backup zurückspielen und die Mods neu aktivieren.

## [5.0.1] — 2026-07-02

### Added
- **Update check & in-app update** — *Settings → Updates* checks the GitHub releases for a
  newer version; if one is found, the manager can **download and install it directly**
  ("Update jetzt installieren") — the installer replaces the old version automatically.

### Fixed
- **Userdefined values** — sliders no longer collapse to a bare thumb; every numeric value
  now shows a proper slider **plus an editable value box** for exact numbers, with sane
  min/max bounds. (Affected the "Benutzerdefinierte Werte" tab of the activation dialog.)

### Changed
- Installer is now produced as **`AMM5_Setup.msi`** (WiX Toolset v7), version 5.0.1.0.

**🇩🇪** *Neu:* Update-Prüfung unter *Einstellungen → Updates* — bei gefundenem Update lädt
der Manager die neue Version direkt herunter und installiert sie („Update jetzt
installieren“). *Behoben:* Benutzerdefinierte Werte zeigen jetzt echte Schieberegler
**plus Eingabefeld** für exakte Zahlen (vorher kollabierte Regler ohne Track). *Geändert:*
Installer heißt jetzt `AMM5_Setup.msi` (Version 5.0.1.0).

## [5.0.0] — 2026-07-02

First official release of the modernised **version 5** — a ground-up interface overhaul,
full **History Edition** support and a built-in **Nexus Mods** browser, all on top of the
proven 4.x mod engine.

### Added
- **Anno 1404 History Edition support** — auto-detects the Ubisoft Connect installation
  (registry / common paths / manual `.exe`) and patches the correct archives. Existing
  classic (Königsedition / Venice) mods stay usable, and mods can target *all* editions at
  once.
- **Nexus Mods, built in** — browse the newest / trending / all Anno 1404 mods from an
  in-app list (one-time personal API key), or open Nexus in an embedded browser where
  downloads are **caught and imported automatically** — no `nxm://` handler and no manual
  file picking.
- **Online mod browser** — discover and download community packages straight from a
  GitHub-hosted repository.
- **Integrated RDA Explorer** — open, extract and repack Anno's `.rda` archives
  (RDA **v2.0 and v2.2**) without leaving the suite.
- **Restore / backup manager** — every change writes a restore point first, so any change
  can be rolled back byte-for-byte. Now a built-in view instead of a separate window.

### Changed
- **Completely redesigned interface** — modern dark theme (harbour backdrop, gold logo,
  faction icons), resizable window, sidebar navigation, and reworked
  Overview / Mods / Categories / Settings / About views.
- **Development Tools 5** — redesigned, fully themed authoring toolkit: dashboard start
  page, card-based project settings, reworked XML / List / Userdefined / file-system module
  editors, and modern controls throughout.
- **Userdefined values** are now adjusted with a slider instead of a numeric spinner.
- **Faster mod loading** — activation status is computed on demand and cached, so the mod
  list appears immediately instead of after a long wait.
- Targets **.NET Framework 4.7.2**; bilingual UI (German & English).

### Fixed
- The game-folder picker now accepts **all** Anno executables
  (`Anno4.exe`, `Anno1404.exe`, `Addon.exe`, `Anno1404Addon.exe`) — the History Edition
  executable could not be selected before.
- Fixed an activation crash and removed a duplicate startup splash.

---

## 🇩🇪 Deutsch — [5.0.0] — 2026-07-02

Erste offizielle Ausgabe der modernisierten **Version 5**: rundum neue Oberfläche, volle
**History-Edition-Unterstützung** und ein eingebauter **Nexus-Browser** – auf Basis der
bewährten 4.x-Mod-Engine.

**Neu**
- **History-Edition-Unterstützung** – automatische Erkennung über Ubisoft Connect;
  klassische Mods (Königsedition / Venedig) bleiben nutzbar, All-Version-Mods möglich.
- **Nexus Mods eingebaut** – In-App-Liste (einmaliger API-Key) plus eingebetteter Browser,
  der Downloads **automatisch abfängt und importiert** (kein `nxm://` nötig).
- **Online-Mod-Browser** – Pakete aus einem GitHub-Repository laden.
- **Integrierter RDA Explorer** – `.rda`-Archive lesen/entpacken/packen (RDA v2.0 & v2.2).
- **Wiederherstellungs-/Backup-Manager** – vor jeder Änderung ein Restore-Punkt; jetzt als
  interne Ansicht statt eigenem Fenster.

**Geändert**
- **Komplett neue Oberfläche** – dunkles Design (Hafen-Hintergrund, Gold-Logo,
  Fraktions-Icons), skalierbares Fenster, Seitenleiste, überarbeitete Ansichten.
- **Development Tools 5** – neu gestaltetes, durchgängig dunkles Autorentool
  (Dashboard-Startseite, Karten-Layout, überarbeitete Editoren).
- **Userdefined-Werte** per Schieberegler statt Zahlenfeld.
- **Schnelleres Laden** der Mod-Liste (Status wird bei Bedarf berechnet und zwischengespeichert).
- Zielplattform **.NET Framework 4.7.2**; zweisprachig (Deutsch & Englisch).

**Behoben**
- EXE-Auswahl akzeptiert jetzt **alle** Anno-EXEs – die History-Edition-EXE war
  vorher nicht auswählbar.
- Aktivierungs-Absturz behoben und doppelten Start-Splash entfernt.
