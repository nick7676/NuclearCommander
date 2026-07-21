using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Commander.Placement;

internal static class MobileFobLocator
{
    private const float CacheDurationSeconds = 1f;

    private static readonly string[] FobTerms = { "mobile fob", "mobile_fob", "mobilefob" };
    private static readonly string[] LogisticsTerms =
    {
        "munitions",
        "ammo",
        "fuel",
        "supply",
        "logistic",
        "rearm",
        "refuel",
        "fob"
    };

    private static readonly List<Transform> Anchors = new();
    private static readonly HashSet<int> AnchorIds = new();

    private static FactionHQ? _cachedHq;
    private static float _nextRefreshTime;

    public static bool IsNear(FactionHQ localHq, Vector3 position, float radius)
    {
        if (radius <= 0f)
        {
            return false;
        }

        RefreshIfNeeded(localHq);
        foreach (Transform anchor in Anchors)
        {
            if (anchor != null
                && PlacementDistance.IsWithinHorizontalRadius(position, anchor.position, radius))
            {
                return true;
            }
        }

        return false;
    }

    private static void RefreshIfNeeded(FactionHQ localHq)
    {
        if (_cachedHq == localHq && Time.unscaledTime < _nextRefreshTime)
        {
            return;
        }

        _cachedHq = localHq;
        _nextRefreshTime = Time.unscaledTime + CacheDurationSeconds;
        Anchors.Clear();
        AnchorIds.Clear();

        AddMobileFobAirbases(localHq);
        AddLogisticsContainers(localHq);
        AddVehicleDepots(localHq);
        AddLogisticsComponents<Rearmer>(localHq);
        AddLogisticsComponents<Refueler>(localHq);
        AddLogisticsComponents<Repairer>(localHq);
    }

    private static void AddMobileFobAirbases(FactionHQ localHq)
    {
        foreach (Airbase airbase in Object.FindObjectsOfType<Airbase>())
        {
            if (airbase != null
                && !airbase.disabled
                && IsMobileFob(airbase)
                && IsOwnedByFaction(airbase, localHq))
            {
                AddAnchor(airbase.center != null ? airbase.center : airbase.transform);
            }
        }
    }

    private static void AddLogisticsContainers(FactionHQ localHq)
    {
        foreach (Container container in Object.FindObjectsOfType<Container>())
        {
            if (container != null
                && IsLogisticsContainer(container)
                && FriendlyUnitValidator.IsOperational(container, localHq))
            {
                AddAnchor(container.transform);
            }
        }
    }

    private static void AddVehicleDepots(FactionHQ localHq)
    {
        foreach (VehicleDepot depot in Object.FindObjectsOfType<VehicleDepot>())
        {
            if (depot != null && FriendlyUnitValidator.IsOperational(depot.GetComponentInParent<Unit>(), localHq))
            {
                AddAnchor(depot.transform);
            }
        }
    }

    private static void AddLogisticsComponents<T>(FactionHQ localHq) where T : Component
    {
        foreach (T component in Object.FindObjectsOfType<T>())
        {
            if (component != null && FriendlyUnitValidator.IsOperational(ResolveUnit(component), localHq))
            {
                AddAnchor(component.transform);
            }
        }
    }

    private static Unit? ResolveUnit(Component component)
    {
        Unit? assignedUnit = component switch
        {
            Rearmer rearmer => rearmer.unit,
            Refueler refueler => refueler.attachedUnit,
            Repairer repairer => repairer.attachedUnit,
            _ => null
        };

        return assignedUnit != null ? assignedUnit : component.GetComponentInParent<Unit>();
    }

    private static void AddAnchor(Transform anchor)
    {
        if (anchor != null && AnchorIds.Add(anchor.GetInstanceID()))
        {
            Anchors.Add(anchor);
        }
    }

    private static bool IsMobileFob(Airbase airbase)
    {
        return ContainsAny(airbase.networkUniqueName, FobTerms)
            || ContainsAny(airbase.name, FobTerms)
            || ContainsAny(airbase.center != null ? airbase.center.name : null, FobTerms);
    }

    private static bool IsOwnedByFaction(Airbase airbase, FactionHQ localHq)
    {
        return airbase.CurrentHQ == localHq || localHq.GetAirbases().Contains(airbase);
    }

    private static bool IsLogisticsContainer(Container container)
    {
        foreach (Component component in container.GetComponentsInChildren<Component>(true))
        {
            if (component is Rearmer or Refueler or Repairer or VehicleDepot)
            {
                return true;
            }
        }

        return ContainsAny(container.name, LogisticsTerms);
    }

    private static bool ContainsAny(string? value, IReadOnlyList<string> terms)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        for (int index = 0; index < terms.Count; index++)
        {
            if (value.IndexOf(terms[index], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
