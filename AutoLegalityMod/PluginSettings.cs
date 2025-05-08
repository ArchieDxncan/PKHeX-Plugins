using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace AutoModPlugins;

public class PluginSettings
{
    private const string Trainer = nameof(Trainer);
    private const string Connection = nameof(Connection);
    private const string Customization = nameof(Customization);
    private const string Legality = nameof(Legality);
    private const string LivingDex = nameof(LivingDex);
    private const string TransferDex = nameof(TransferDex);
    private const string Miscellaneous = nameof(Miscellaneous);
    private const string Development = nameof(Development);

    [Browsable(false)]
    public string? ConfigPath { get; init; }

    // Trainer
    [Category(Trainer)]
    [Description("Allows overriding trainer data with \"OT\", \"TID\", \"SID\", and \"OTGender\" as part of a Showdown set.")]
    public bool AllowTrainerOverride { get; set; } = false;

    [Category(Trainer)]
    [Description("Enables use of custom trainer data based on the \"trainers\" folder.")]
    public bool UseTrainerData { get; set; } = false;

    [Category(Trainer)]
    [Description("Default OT Name to use while generating Pokémon.")]
    public string DefaultOT { get; set; } = "ALM";

    [Category(Trainer)]
    [Description("Default TID to use while generating Pokémon. (TID16)")]
    public ushort DefaultTID16 { get; set; } = 54321;

    [Category(Trainer)]
    [Description("Default SID to use while generating Pokémon. (SID16)")]
    public ushort DefaultSID16 { get; set; } = 12345;

    // Connection
    [Category(Connection)]
    [Description("Stores the last IP used by LiveHeX.")]
    public string LatestIP { get; set; } = "192.168.1.65";

    [Category(Connection)]
    [Description("Allows LiveHeX to use USB-Botbase instead of sys-botbase.")]
    public bool USBBotBasePreferred { get; set; } = false;

    [Category(Connection)]
    [Description("Uses cached pointers for LiveHeX.")]
    public bool UseCachedPointers { get; set; } = false;

    // Legality
    [Category(Legality)]
    [Description("Forces the specified ball when generating Pokémon.")]
    public bool ForceSpecifiedBall { get; set; } = true;

    [Category(Legality)]
    [Description("Prioritizes the specified game version when generating Pokémon.")]
    public bool PrioritizeGame { get; set; } = true;

    [Category(Legality)]
    [Description("The game version to prioritize when generating Pokémon.")]
    public GameVersion PriorityGameVersion { get; set; } = (GameVersion)72;

    [Category(Legality)]
    [Description("Sets all legal ribbons on generated Pokémon.")]
    public bool SetAllLegalRibbons { get; set; } = false;

    [Category(Legality)]
    [Description("Sets the battle version on generated Pokémon.")]
    public bool SetBattleVersion { get; set; } = true;

    [Category(Legality)]
    [Description("Sets matching balls on generated Pokémon.")]
    public bool SetMatchingBalls { get; set; } = false;

    [Category(Legality)]
    [Description("Forces level 100 for level 50 Pokémon.")]
    public bool ForceLevel100for50 { get; set; } = true;

    // Customization
    [Category(Customization)]
    [Description("The format to use when exporting Pokémon.")]
    public BattleTemplateDisplayStyle ExportFormat { get; set; } = (BattleTemplateDisplayStyle)0;

    [Category(Customization)]
    [Description("Timeout in seconds for API calls.")]
    public int Timeout { get; set; } = 15;

    [Category(Customization)]
    [Description("Prioritized encounter types when generating Pokémon.")]
    public List<EncounterTypeGroup> PrioritizeEncounters { get; set; } = new()
    {
        (EncounterTypeGroup)1,
        (EncounterTypeGroup)4,
        (EncounterTypeGroup)8,
        (EncounterTypeGroup)16,
        (EncounterTypeGroup)2
    };

    [Category(Customization)]
    [Description("Enables easter eggs in the plugin.")]
    public bool EnableEasterEggs { get; set; } = true;

    [Category(Customization)]
    [Description("Includes alternate forms when generating Pokémon.")]
    public bool IncludeForms { get; set; } = true;

    [Category(Customization)]
    [Description("Sets shiny status on generated Pokémon.")]
    public bool SetShiny { get; set; } = false;

    [Category(Customization)]
    [Description("Sets alpha status on generated Pokémon.")]
    public bool SetAlpha { get; set; } = false;

    [Category(Customization)]
    [Description("Only uses native encounters when generating Pokémon.")]
    public bool NativeOnly { get; set; } = false;

    [Category(Customization)]
    [Description("The version to use for transfers.")]
    public GameVersion TransferVersion { get; set; } = (GameVersion)51;

    [Category(Customization)]
    [Description("Prompts for Smogon import.")]
    public bool PromptForSmogonImport { get; set; } = false;

    [Category(Customization)]
    [Description("Uses markings on generated Pokémon.")]
    public bool UseMarkings { get; set; } = true;

    [Category(Customization)]
    [Description("Uses competitive markings on generated Pokémon.")]
    public bool UseCompetitiveMarkings { get; set; } = true;

    [Category(Customization)]
    [Description("Random types to use when generating Pokémon.")]
    public MoveType[] RandomTypes { get; set; } = [];

    // Development
    [Category(Development)]
    [Description("Enables developer mode.")]
    public bool EnableDevMode { get; set; } = false;

    [Category(Development)]
    [Description("Latest allowed version.")]
    public string LatestAllowedVersion { get; set; } = "0.0.0.0";

    public void Save()
    {
        JsonSerializerOptions options = new() { WriteIndented = true };
        string output = JsonSerializer.Serialize(this, options);
        using StreamWriter sw = new(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "archiedxncan_config.json"));
        sw.WriteLine(output);
    }
}
