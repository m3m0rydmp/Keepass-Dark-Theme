# CustomTheme for KeePass 2.x

A from-scratch, dependency-free theme plugin prototype. Initial target: official
KeePass 2.62 on Windows with .NET Framework 4.x. **Not compiled or visually tested
yet; compatibility is a target, not a verified claim.** Future KeePass releases
need regression testing before being declared supported.

## Build on Windows

1. Download a portable KeePass copy from https://keepass.info/download.html and
   verify it using the official hashes/signatures. Keep a separate test directory.
2. Copy this project to Windows. In PowerShell, from the project directory, run:

   ```powershell
   .\build.ps1 -KeePassPath 'C:\Tools\KeePass-Test'
   ```

   The script uses the Windows .NET Framework C# compiler and references your
   official KeePass.exe. It downloads nothing and prints the DLL SHA-256 hash.
3. Close the test KeePass instance. Copy `dist\CustomTheme` into its `Plugins`
   directory, yielding `Plugins\CustomTheme\CustomTheme.dll` and an adjacent
   `palettes` directory. Restart KeePass.
4. Use **Tools > CustomTheme > Charcoal**, **Midnight**, or **Cyberpunk**.
   Theme selection is opt-in every session. Choose **System appearance** to restore
   original control colors. To uninstall, close KeePass and remove that plugin folder.

## Customize

Copy a palette XML file beside the DLL and edit its name and four `#RRGGBB`
attributes. Restart KeePass to discover new files. Background colors windows,
surface colors text fields and lists, text colors foregrounds, and accent colors
menu borders. XML is size-limited; DTDs and external entity resolution are disabled.
Choose readable text against both background and surface. Accent coverage is
currently limited; it does not change every Windows selection highlight.

## Scope and review

All plugin source is in `src/CustomTheme.cs`. It uses KeePass only for plugin
registration and menu integration. It does not access database objects, read
control text, make network requests, or implement update checks. Palettes are local
data files. This small scope assists review; it is not an independent security audit.

This first pass colors standard WinForms controls and tool strips, including
controls discovered in open dialogs. It preserves native drawing modes and restores
saved colors/renderers on disable/unload. Windows high-contrast mode takes priority.
It does not replace secure input controls or render password text itself.

Native title bars, message boxes, list headers, tab/button painting, context menu
coverage, KeePass-specific drawing and per-entry colors may remain partially light
or override these colors. Full native control coverage needs Windows investigation;
this prototype does not promise a completely dark interface. Other appearance
plugins should be disabled during testing.

## Windows acceptance checks (pending)

Use a dummy database. Verify plugin loading on official 2.62; switch all three
palettes and return to system appearance. Check main tree/list, search, menus,
entry editor, database settings and unlock dialog. Confirm masked passwords,
keyboard focus, disabled text, selection and per-entry colors remain usable.
Open/close dialogs repeatedly; test scaling at 100%, 150%, and 200%, high contrast,
and restart/uninstall. Record screenshots and any remaining light controls before
extending drawing behavior. No Windows runtime or C# compiler was available in the
initial Linux development environment.

References: https://keepass.info/help/v2_dev/plg_index.html
and https://keepass.info/help/v2/plugins.html
