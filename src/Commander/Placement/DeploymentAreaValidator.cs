using System;
using UnityEngine;

namespace Commander.Placement;

internal static class DeploymentAreaValidator
{
    private const float LogisticsCacheLifetime = 1f;

    private static VehicleDepot[] _vehicleDepots = Array.Empty<VehicleDepot>();
    private static Rearmer[] _rearmers = Array.Empty<Rearmer>();
    private static Refueler[] _refuelers = Array.Empty<Refueler>();
    private static float _nextLogisticsRefresh;

    public static bool IsAllowed(
        FactionHQ localHq,
        Vector3 position,
        float fobRadius,
        out string reason)
    {
        if (IsInsideFriendlyAirbase(localHq, position) ||
            IsNearFriendlyFob(localHq, position, fobRadius) ||
            IsNearFriendlyMobileFob(localHq, position, fobRadius))
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

    private static bool IsNearFriendlyMobileFob(
        FactionHQ localHq,
        Vector3 position,
        float fobRadius)
    {
        float radius = Mathf.Max(0f, fobRadius);
        if (radius <= 0f)
        {
            return false;
        }

        RefreshLogisticsCache();

        foreach (VehicleDepot depot in _vehicleDepots)
        {
            if (IsFriendlyOperationalUnit(depot, localHq) &&
                IsWithinHorizontalRadius(position, depot.transform.position, radius))
            {
                return true;
            }
        }

        foreach (Rearmer rearmer in _rearmers)
        {
            if (IsFriendlyOperationalComponent(rearmer, localHq) &&
                IsWithinHorizontalRadius(position, rearmer.transform.position, radius))
            {
                return true;
            }
        }

        foreach (Refueler refueler in _refuelers)
        {
            if (IsFriendlyOperationalComponent(refueler, localHq) &&
                IsWithinHorizontalRadius(position, refueler.transform.position, radius))
            {
                return true;
            }
        }

        return false;
    }

    private static void RefreshLogisticsCache()
    {
        if (Time.unscaledTime < _nextLogisticsRefresh)
        {
            return;
        }

        _vehicleDepots = UnityEngine.Object.FindObjectsOfType<VehicleDepot>();
        _rearmers = UnityEngine.Object.FindObjectsOfType<Rearmer>();
        _refuelers = UnityEngine.Object.FindObjectsOfType<Refueler>();
        _nextLogisticsRefresh = Time.unscaledTime + LogisticsCacheLifetime;
    }

    private static bool IsFriendlyOperationalComponent(Component component, FactionHQ localHq)
    {
        if (component == null)
        {
            return false;
        }

        Unit unit = component.GetComponentInParent<Unit>();
        return IsFriendlyOperationalUnit(unit, localHq);
    }

    private static bool IsFriendlyOperationalUnit(Unit unit, FactionHQ localHq)
    {
        return unit != null && !unit.Networkdisabled && unit.NetworkHQ == localHq;
    }

    private static bool IsWithinHorizontalRadius(Vector3 position, Vector3 center, float radius)
    {
        return HorizontalSqrDistance(position, center) <= radius * radius;
    }

    private static float HorizontalSqrDistance(Vector3 first, Vector3 second)
    {
        float x = first.x - second.x;
        float z = first.z - second.z;
        return x * x + z * z;
    }
}
