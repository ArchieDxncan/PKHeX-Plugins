using PKHeX.Core;

namespace PKHeX.Core.Enhancements;

/// <summary>
/// Extension methods for Showdown related classes
/// </summary>
public static class ShowdownExtensions
{
    /// <summary>
    /// Gets the text representation of a ShowdownSet
    /// </summary>
    /// <param name="set">ShowdownSet to convert</param>
    /// <param name="settings">Export settings</param>
    /// <returns>String representation</returns>
    public static string GetText(this ShowdownSet set, BattleTemplateExportSettings settings)
    {
        return set.Text;
    }
} 