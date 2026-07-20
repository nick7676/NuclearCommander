using UnityEngine;

namespace Commander.Placement;

internal static class DeploymentAreaValidator
{
    public static bool IsAllowed(
        FactionHQ localHq,
        Vector3 position,
        float fobRadius,
        out string reason)
    {
        if (IsInsideFriendlyAirbase(localHq, position) ||
            IsNearFriendlyFob(localHq, position, fobRadius))
        {
            reason = string.Empty;
            return true;
        }

        reason = $"outside a friendly airport and more than {Mathf.Max(0f, fobRadius):0} m from a friendly FOB";
        return false;
    }

    private static bool IsInsideFriendlyAirbase(FactionHQ localHq, Vector3 position)
    {
        foreach (Airbase airbase in localHq.GetAirbases())
        {
            if (airbase == null || airbase.disabled || airbase.center == null)
            {
                continue;
            }

            float radius = Mathf.Max(0f, airbase.GetRadius());
            if (HorizontalSqrDistance(position, airbase.center.position) <= radius * radius)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNearFriendlyFob(FactionHQ localHq, Vector3 position, float fobRadius)
    {
        float radius = Mathf.Max(0f, fobRadius);
        return radius > 0f && localHq.GetNearestDepot(position, radius) != null;
    }

    private static float HorizontalSqrDistance(Vector3 first, Vector3 second)
    {
        float x = first.x - second.x;
        float z = first.z - second.z;
        return x * x + z * z;
    }
}
