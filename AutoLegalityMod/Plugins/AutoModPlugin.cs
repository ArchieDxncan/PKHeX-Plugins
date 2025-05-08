using AutoModPlugins.GUI;
using AutoModPlugins.Properties;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoModPlugins;

/// <summary>
/// Base plugin logic; automatically adds plugin info
/// </summary>
public abstract class AutoModPlugin : IPlugin
{
    private const string ParentMenuName = "Menu_ArchieDxncan";
    private const string ParentMenuText = "ArchieDxncan's Transporter";
    private const string ParentMenuParent = "Menu_Tools";
    private const string LoggingPrefix = "[ArchieDxncan's Transporter]";

    private readonly CancellationTokenSource Source = new();

    /// <summary>
    /// Main Plugin Variables
    /// </summary>
    public abstract string Name { get; }
    public abstract int Priority { get; }

    // Initialized during plugin startup
    public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
    protected IPKMView PKMEditor { get; private set; } = null!;
    internal static readonly string almconfig = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "archiedxncan_config.json");
    internal static PluginSettings _settings = new() { ConfigPath = almconfig };

    public void Initialize(params object[] args)
    {
        Debug.WriteLine($"{LoggingPrefix} Loading {Name}");
        SaveFileEditor = (ISaveFileProvider)(Array.Find(args, z => z is ISaveFileProvider) ?? throw new Exception("Null ISaveFileProvider"));
        PKMEditor = (IPKMView)(Array.Find(args, z => z is IPKMView) ?? throw new Exception("Null IPKMView"));
        var menu = (ToolStrip)(Array.Find(args, z => z is ToolStrip) ?? throw new Exception("Null ToolStrip"));
        LoadMenuStrip(menu);

        // Load settings
        if (File.Exists(_settings.ConfigPath))
        {
            var text = File.ReadAllText(_settings.ConfigPath);
            _settings = JsonSerializer.Deserialize<PluginSettings>(text)!;
        }

        // Match PKHeX Versioning and ALM Settings only on parent plugin
        if (Priority != 0)
            return;

        Task.Run(async () =>
            {
                var (hasError, error) = await SetUpEnvironment(Source.Token).ConfigureAwait(false);
                if (hasError && error is not null)
                {
                    if (error.InvokeRequired)
                    {
                        await error.InvokeAsync(() => ShowAlmErrorDialog(error, menu));
                    }
                    else
                    {
                        ShowAlmErrorDialog(error, menu);
                    }
                }
            },
            Source.Token);
    }

    private static void ShowAlmErrorDialog(ALMError error, ToolStrip menu)
    {
        SystemSounds.Hand.Play();
        var res = error.ShowDialog(menu);
        if (res != DialogResult.Retry)
            return;

        var info = new ProcessStartInfo
        {
            FileName = "https://github.com/santacrab2/PKHeX-Plugins/wiki/Installing-PKHeX-Plugins",
            UseShellExecute = true,
        };
        Process.Start(info);
    }

    private async Task<(bool, ALMError?)> SetUpEnvironment(CancellationToken token)
    {
        ShowdownSetLoader.SetAPILegalitySettings(_settings);
        await TranslateInterface(token).ConfigureAwait(false);
        return CheckForMismatch();
    }

    private async Task TranslateInterface(CancellationToken token)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        var form = ((ContainerControl)SaveFileEditor).ParentForm;
        if (form is null)
            return;

        // wait for all plugins to be loaded
        while (!form.IsHandleCreated)
            await Task.Delay(0_100, token).ConfigureAwait(false);

        if (form.InvokeRequired)
        {
            await form.InvokeAsync(() => form.TranslateInterface(WinFormsTranslator.CurrentLanguage), token);
        }
        else
        {
            form.TranslateInterface(WinFormsTranslator.CurrentLanguage);
        }

        Debug.WriteLine($"{LoggingPrefix} Translated form.");
    }

    private static (bool, ALMError?) CheckForMismatch()
    {
        bool mismatch = ALMVersion.GetIsMismatch();
        bool reset =ALMVersion.Versions.CoreVersionCurrent > new Version(_settings.LatestAllowedVersion);
        if (reset)
            _settings.LatestAllowedVersion = "0.0.0.0";

        _settings.EnableDevMode = _settings.EnableDevMode && !mismatch;
        if (mismatch || reset)
            _settings.Save();

        return (mismatch, mismatch ? WinFormsUtil.ALMErrorMismatch(ALMVersion.Versions) : null);
    }

    private void LoadMenuStrip(ToolStrip menuStrip)
    {
        var items = menuStrip.Items;
        if (items.Find(ParentMenuParent, false)[0] is not ToolStripDropDownItem tools)
            return;

        var toolsitems = tools.DropDownItems;
        var modmenusearch = toolsitems.Find(ParentMenuName, false);
        var modmenu = GetModMenu(tools, modmenusearch);
        
        // Clear existing items
        modmenu.DropDownItems.Clear();
        
        // Add Legalize Active Pokemon
        var legalize = new ToolStripMenuItem("Legalize Active Pokemon") { Image = Resources.legalizeboxes };
        legalize.Click += (s, e) => {
            try
            {
                var box = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                if (!box)
                {
                    // Legalize Active Pokemon
                    var pk = PKMEditor.PreparePKM();
                    var la = new LegalityAnalysis(pk);
                    if (la.Valid)
                        return; // already valid, don't modify it

                    var sav = SaveFileEditor.SAV;
                    var result = sav.Legalize(pk, la);

                    la = new LegalityAnalysis(result);
                    if (!la.Valid)
                    {
                        const string errorstr = "Unable to make the Active Pokemon legal!\n\n"
                                        + "No legal Pokémon matches the provided traits.\n\n"
                                        + "Visit the Wiki to learn how to import Showdown Sets.";

                        var error = WinFormsUtil.ALMErrorBasic(errorstr);
                        error.ShowDialog();

                        var res = error.DialogResult;
                        if (res == DialogResult.Retry)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "https://github.com/santacrab2/PKHeX-Plugins/wiki/Getting-Started-with-Auto-Legality-Mod",
                                UseShellExecute = true,
                            });
                        }
                        return;
                    }

                    PKMEditor.PopulateFields(result);
                    WinFormsUtil.Alert("Legalized Active Pokemon!");
                    return;
                }

                var all = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
                if (!all)
                {
                    // Legalize Current Box
                    var sav = SaveFileEditor.SAV;
                    var count = sav.LegalizeBox(sav.CurrentBox);
                    if (count <= 0) // failed to modify anything, let the user know.
                    {
                        WinFormsUtil.Alert("Unable to legalize any of the Pokémon in the Current Box");
                        return;
                    }

                    SaveFileEditor.ReloadSlots();
                    WinFormsUtil.Alert($"Legalized {count} Pokémon in Current Box!");
                }
                else
                {
                    // Legalize All Boxes
                    var sav = SaveFileEditor.SAV;
                    var count = sav.LegalizeBoxes();
                    if (count <= 0) // failed to modify anything, let the user know.
                    {
                        WinFormsUtil.Alert("Unable to legalize any of the Pokémon in All Boxes");
                        return;
                    }

                    SaveFileEditor.ReloadSlots();
                    WinFormsUtil.Alert($"Legalized {count} Pokémon across all boxes!");
                }
            }
            catch (MissingMethodException)
            {
                var errorstr = "The PKHeX-Plugins version does not match the PKHeX version.\nRefer to the Wiki for how to fix this error.\n\n"
                           + $"The current ALM Version is {ALMVersion.Versions.AlmVersionCurrent}\n"
                           + $"The current PKHeX Version is {ALMVersion.Versions.CoreVersionCurrent}";

                var error = WinFormsUtil.ALMErrorBasic(errorstr);
                error.ShowDialog();

                var res = error.DialogResult;
                if (res == DialogResult.Retry)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/santacrab2/PKHeX-Plugins/wiki/Installing-PKHeX-Plugins",
                        UseShellExecute = true,
                    });
                }
            }
        };
        legalize.Name = "Menu_LeaglizeBoxes";
        modmenu.DropDownItems.Add(legalize);

        // Add Plugin Settings
        var settings = new ToolStripMenuItem("Plugin Settings") { Image = Resources.settings };
        settings.Click += (s, e) => new ALMSettings(_settings).ShowDialog();
        settings.Name = "Menu_ALMSettingsEditor";
        modmenu.DropDownItems.Add(settings);
    }

    private static ToolStripMenuItem GetModMenu(ToolStripDropDownItem tools, ToolStripItem[] search)
    {
        if (search.Length != 0)
            return (ToolStripMenuItem)search[0];

        var modmenu = CreateBaseGroupItem();
        tools.DropDownItems.Insert(0, modmenu);
        return modmenu;
    }

    private static ToolStripMenuItem CreateBaseGroupItem() => new(ParentMenuText) { Image = Resources.menuautolegality, Name = ParentMenuName };

    protected abstract void AddPluginControl(ToolStripDropDownItem modmenu);

    public virtual void NotifySaveLoaded()
    {
        Console.WriteLine($"{Name} was notified that a Save File was just loaded.");
    }

    public virtual bool TryLoadFile(string filePath)
    {
        Console.WriteLine($"{Name} was provided with the file path, but chose to do nothing with it.");
        return false; // no action taken
    }

    public virtual void NotifyDisplayLanguageChanged(string language)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        var form = ((ContainerControl)SaveFileEditor).ParentForm;
        form?.TranslateInterface(language);
    }
}
