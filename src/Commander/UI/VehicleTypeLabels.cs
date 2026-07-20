namespace Commander.UI;

internal static class VehicleTypeLabels
{
    public static string Get(VehicleType vehicleType)
    {
        return vehicleType switch
        {
            VehicleType.TRUCK => "Support",
            VehicleType.UGV => "UGV",
            VehicleType.LCV => "Light",
            VehicleType.AFV => "AFV",
            VehicleType.MBT => "Tank",
            VehicleType.ART => "Artillery",
            VehicleType.AAA => "Anti-air",
            VehicleType.IR_SAM => "IR SAM",
            VehicleType.R_SAM => "Radar SAM",
            VehicleType.RDR => "Radar",
            _ => vehicleType.ToString()
        };
    }
}
