using System;
using PKHeX.Core;

namespace PKHeX.Core.AutoMod;

/// <summary>
/// Verifier for Trainer IDs
/// </summary>
public static class TrainerIDVerifier
{
    /// <summary>
    /// Try to get a SID that makes a Pokémon shiny with the given PID and TID
    /// </summary>
    /// <param name="pid">PID of the Pokémon</param>
    /// <param name="tid16">Trainer ID</param>
    /// <param name="version">Game version</param>
    /// <param name="sid16">Resulting SID if successful</param>
    /// <returns>True if a valid SID was found</returns>
    public static bool TryGetShinySID(uint pid, ushort tid16, GameVersion version, out ushort sid16)
    {
        // For generation < 3, shiny is determined by IVs
        if (version.GetGeneration() <= 2)
        {
            sid16 = 0;
            return false;
        }

        // Calculate a SID that would make this PID shiny
        var shinyPID = (pid & 0xFFFF) ^ tid16 ^ 8;
        sid16 = (ushort)shinyPID;
        return true;
    }
} 