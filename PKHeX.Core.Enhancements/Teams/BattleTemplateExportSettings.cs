namespace PKHeX.Core.Enhancements;

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