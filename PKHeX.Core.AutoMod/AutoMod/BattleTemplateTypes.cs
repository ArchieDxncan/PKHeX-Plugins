using System;

namespace PKHeX.Core.AutoMod;

/// <summary>
/// Display style for Battle Template exports.
/// </summary>
public enum BattleTemplateDisplayStyle
{
    /// <summary>
    /// Default Showdown format
    /// </summary>
    Showdown,
    
    /// <summary>
    /// Legacy format (Gen 3-4 templates)
    /// </summary>
    Legacy,
}

/// <summary>
/// Settings for exporting Battle Templates.
/// </summary>
public class BattleTemplateExportSettings
{
    /// <summary>
    /// Standard Showdown export format
    /// </summary>
    public static readonly BattleTemplateExportSettings Showdown = new();

    /// <summary>
    /// Community Standard format
    /// </summary>
    public static readonly BattleTemplateExportSettings CommunityStandard = new();
} 