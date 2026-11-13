using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using KeePass.Plugins;

[assembly: AssemblyTitle("CustomTheme")]
[assembly: AssemblyDescription("Local configurable theme prototype for KeePass 2.x")]
[assembly: AssemblyProduct("KeePass Plugin")]
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

namespace CustomTheme
{
    public sealed class CustomThemeExt : Plugin
    {
        private sealed class Original
        {
            public Color Back, Fore;
            public ToolStripRenderer Renderer;
        }
        private readonly Dictionary<Control, Original> originals = new Dictionary<Control, Original>();
        private Palette palette;
        private bool applying;
        private string paletteDirectory;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            paletteDirectory = Path.Combine(Path.GetDirectoryName(typeof(CustomThemeExt).Assembly.Location), "palettes");
            // Opt-in each session until the prototype has been tested on Windows.
            Application.Idle += OnIdle;
            return true;
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType type)
        {
            if (type != PluginMenuType.Main) return null;
            ToolStripMenuItem menu = new ToolStripMenuItem("CustomTheme");
            ToolStripMenuItem off = new ToolStripMenuItem("System appearance");
            off.Click += delegate { palette = null; Restore(); };
            menu.DropDownItems.Add(off);
            if (Directory.Exists(paletteDirectory))
            {
                foreach (string file in Directory.GetFiles(paletteDirectory, "*.xml"))
                {
                    string path = file;
                    ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileNameWithoutExtension(path));
                    item.Click += delegate
                    {
                        try { Palette next = Palette.Load(path); Restore(); palette = next; ApplyOpenForms(); }
                        catch (Exception ex)
                        {
                            if (!(ex is IOException || ex is UnauthorizedAccessException || ex is XmlException || ex is FormatException)) throw;
                            MessageBox.Show("Cannot load palette: " + ex.Message, "CustomTheme", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    menu.DropDownItems.Add(item);
                }
            }
            return menu;
        }

        private void OnIdle(object sender, EventArgs args) { ApplyOpenForms(); }
        private void ApplyOpenForms()
        {
            if (applying) return;
            if (SystemInformation.HighContrast) { Restore(); return; }
            if (palette == null) return;
            applying = true;
            try
            {
                Form[] forms = new Form[Application.OpenForms.Count];
                Application.OpenForms.CopyTo(forms, 0);
                foreach (Form form in forms) Apply(form);
            }
            finally { applying = false; }
        }
        private void Apply(Control control)
        {
            if (control.IsDisposed) return;
            if (!originals.ContainsKey(control))
            {
                ToolStrip strip = control as ToolStrip;
                originals.Add(control, new Original { Back = control.BackColor, Fore = control.ForeColor,
                    Renderer = strip == null ? null : strip.Renderer });
                control.Disposed += OnDisposed;
                // Do not alter drawing modes or inspect text, entries, or database objects.
                control.BackColor = (control is TextBoxBase || control is ListView || control is TreeView || control is ComboBox)
                    ? palette.Surface : palette.Background;
                control.ForeColor = palette.Text;
                if (strip != null) strip.Renderer = new ThemeRenderer(palette);
            }
            foreach (Control child in control.Controls) Apply(child);
        }
        private void OnDisposed(object sender, EventArgs args)
        {
            Control control = (Control)sender;
            control.Disposed -= OnDisposed;
            originals.Remove(control);
        }
        private void Restore()
        {
            foreach (KeyValuePair<Control, Original> entry in originals)
            {
                Control control = entry.Key;
                control.Disposed -= OnDisposed;
                if (control.IsDisposed) continue;
                control.BackColor = entry.Value.Back;
                control.ForeColor = entry.Value.Fore;
                ToolStrip strip = control as ToolStrip;
                if (strip != null) strip.Renderer = entry.Value.Renderer;
            }
            originals.Clear();
        }
        public override void Terminate()
        {
            Application.Idle -= OnIdle;
            palette = null;
            Restore();
        }
    }

    internal sealed class Palette
    {
        public Color Background, Surface, Text, Accent;
        public static Palette Load(string path)
        {
            XmlReaderSettings settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = 16384 };
            XmlDocument document = new XmlDocument { XmlResolver = null };
            using (XmlReader reader = XmlReader.Create(path, settings)) document.Load(reader);
            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "palette") throw new FormatException("Expected a palette root element.");
            return new Palette { Background = Read(root, "background"), Surface = Read(root, "surface"),
                Text = Read(root, "text"), Accent = Read(root, "accent") };
        }
        private static Color Read(XmlElement root, string name)
        {
            string value = root.GetAttribute(name);
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$"))
                throw new FormatException(name + " must be a #RRGGBB color.");
            return ColorTranslator.FromHtml(value);
        }
    }
    internal sealed class ThemeRenderer : ToolStripProfessionalRenderer
    {
        private readonly Palette palette;
        public ThemeRenderer(Palette value) : base(new ThemeColors(value)) { palette = value; }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? palette.Text : SystemColors.GrayText;
            base.OnRenderItemText(e);
        }
    }
    internal sealed class ThemeColors : ProfessionalColorTable
    {
        private readonly Palette p;
        public ThemeColors(Palette palette) { p = palette; UseSystemColors = false; }
        public override Color ToolStripDropDownBackground { get { return p.Background; } }
        public override Color ImageMarginGradientBegin { get { return p.Background; } }
        public override Color ImageMarginGradientMiddle { get { return p.Background; } }
        public override Color ImageMarginGradientEnd { get { return p.Background; } }
        public override Color MenuStripGradientBegin { get { return p.Background; } }
        public override Color MenuStripGradientEnd { get { return p.Background; } }
        public override Color ToolStripGradientBegin { get { return p.Background; } }
        public override Color ToolStripGradientMiddle { get { return p.Background; } }
        public override Color ToolStripGradientEnd { get { return p.Background; } }
        public override Color MenuItemSelected { get { return p.Surface; } }
        public override Color MenuItemBorder { get { return p.Accent; } }
        public override Color MenuItemSelectedGradientBegin { get { return p.Surface; } }
        public override Color MenuItemSelectedGradientEnd { get { return p.Surface; } }
        public override Color MenuItemPressedGradientBegin { get { return p.Surface; } }
        public override Color MenuItemPressedGradientMiddle { get { return p.Surface; } }
        public override Color MenuItemPressedGradientEnd { get { return p.Surface; } }
    }
}
