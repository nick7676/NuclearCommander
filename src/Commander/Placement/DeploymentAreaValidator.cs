using UnityEngine;

namespace Commander.Placement;

internal static class DeploymentAreaValidator
{
    public static bool IsAllowed(
        FactionHQ localHq,
        Vector3 position,
        float fobPlacementRadius,
        out string reason)
    {
        if (IsInsideFriendlyAirbase(localHq, position))
        {
            reason = string.Empty;
            return true;
        }

        float radius = Mathf.Max(0f, fobPlacementRadius);
        if (IsNearFriendlyDepot(localHq, position, radius)
            || MobileFobLocator.IsNear(localHq, position, radius))
        {
            reason = string.Empty;
            return true;
        }

        reason = radius > 0f
            ? $"Invalid placement: outside a friendly airport and more than {radius:0} m from a friendly FOB."
            : "Invalid placement: outside a friendly airport.";
        return false;
    }

    private static bool IsInsideFriendlyAirbase(FactionHQ localHq, Vector3 position)
    {
        foreach (Airbase airbase in localHq.GetAirbases())
        {
            if (airbase != null
                && !airbase.disabled
                && airbase.center != null
                && PlacementDistance.IsWithinHorizontalRadius(
                    position,
                    airbase.center.position,
                    Mathf.Max(0f, airbase.GetRadius())))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNearFriendlyDepot(FactionHQ localHq, Vector3 position, float radius)
    {
        if (radius <= 0f)
        {
            return false;
        }

        return localHq.GetNearestDepot(position, radius) != null;
    }
}
