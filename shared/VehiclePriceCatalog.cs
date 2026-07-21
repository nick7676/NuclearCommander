using System.Collections.Generic;

namespace NuclearCommander.Shared;

internal sealed class VehiclePriceDefinition
{
    public VehiclePriceDefinition(string key, string displayName, decimal defaultPrice)
    {
        Key = key;
        DisplayName = displayName;
        DefaultPrice = defaultPrice;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public decimal DefaultPrice { get; }
}

internal static class VehiclePriceCatalog
{
    public static IReadOnlyList<VehiclePriceDefinition> All { get; } = new VehiclePriceDefinition[]
    {
        new("LightTruck1_AA", "LCV25 AA", 0.44m),
        new("HLT-T", "HLT Tractor", 0.45m),
        new("Truck2-T", "MSV Tractor", 0.45m),
        new("Truck2-L", "MSV Flatbed", 0.46m),
        new("HLT-L", "HLT Flatbed", 0.46m),
        new("HLT-M", "HLT Munitions Truck", 0.49m),
        new("Truck2-M", "MSV Munitions", 0.49m),
        new("Truck2-FT", "MSV Fuel Tanker", 0.51m),
        new("HLT-FT", "HLT Fuel Tanker", 0.51m),
        new("LightTruck1_AT", "LCV25 AT", 0.55m),
        new("LCV45", "LCV45 Recon Truck", 0.65m),
        new("Truck2-MRAP", "MSV MRAP", 0.65m),
        new("UGV1_grenade", "Hexhound GMG", 0.8m),
        new("6x6_1_APC", "AFV6 APC", 1.3m),
        new("6x6_1_IFV", "AFV6 IFV", 1.5m),
        new("UGV1_SAM", "Hexhound SAM", 1.5m),
        new("UGV1_AT_P", "Hexhound SAM", 1.5m),
        new("6x6_1_AA", "AFV6 AA", 1.8m),
        new("UGVDozer1", "M12 Jackknife", 2m),
        new("6x6_1_AT", "AFV6 AT", 2.1m),
        new("Truck2-FC", "MSV Fire Control", 2.3m),
        new("HLT-FC", "HLT Fire Control", 2.3m),
        new("AFV8_APC", "AFV8 APC", 5.2m),
        new("AFV8_EW_P", "AFV8 APC", 5.2m),
        new("AFV8_IFV", "AFV8 IFV", 6.1m),
        new("AFV8_SPG_P", "AFV8 IFV", 6.1m),
        new("SAMIRTurret1", "Boltstrike RAM45 Launcher", 7m),
        new("SAMTurret1", "Boltstrike RAM45 Launcher", 7m),
        new("ASHMTurret1", "Boltstrike RAM45 Launcher", 7m),
        new("Linebreaker_APC", "Linebreaker APC", 7.2m),
        new("Linebreaker_EW_P", "Linebreaker APC", 7.2m),
        new("AFV8_SAM", "AFV8 Mobile Air Defense", 7.5m),
        new("AFV8_MLRS_P", "AFV8 Mobile Air Defense", 7.5m),
        new("AFV8_MAD_P", "AFV8 Mobile Air Defense", 7.5m),
        new("SPAAG1", "AeroSentry SPAAG", 8m),
        new("Linebreaker_IFV", "Linebreaker IFV", 8.1m),
        new("Linebreaker_SPG_P", "Linebreaker IFV", 8.1m),
        new("Horse1", "FGA-57 Anvil", 9m),
        new("SPAAG2_CFV_P", "FGA-57 Anvil", 9m),
        new("SPAAG2", "FGA-57 Anvil", 9m),
        new("Linebreaker_SAM", "Linebreaker SAM", 9.5m),
        new("CRAMTrailer1", "HLT-CRAM", 10.5m),
        new("P_LRAA1", "Type-12 MBT", 11.2m),
        new("MBT", "Type-12 MBT", 11.2m),
        new("RadarContainer1", "Radar Container", 12.5m),
        new("RadarSAM1", "T9K41 Boltstrike", 12.5m),
        new("SAMTrailer1", "StratoLance R9 Launcher", 12.5m),
        new("Truck2-ASHM1", "MSV R9 Stratolance Launcher", 13.4m),
        new("Truck2-ASHM3", "MSV R9 Stratolance Launcher", 13.4m),
        new("Truck2-RSAM", "MSV R9 Stratolance Launcher", 13.4m),
        new("MBT1", "Spearhead MBT", 15.3m),
        new("LaserTrailer1", "HLT-HEL", 18.5m),
        new("HLT-R", "HLT Radar Truck", 20m),
        new("Truck2-CRAM", "MSV CRAM", 20m),
        new("Truck2-LADS", "MSV LADS", 20m),
        new("Truck2-R", "MSV Radar", 20m),
        new("Truck2-TBM", "MSV Ballistic Missile Launcher", 21.2m),
        new("Truck2-TBM-N", "MSV Nuclear Ballistic Missile Launcher", 76.2m)
    };
}
