using UnityEngine;

namespace Commander.Placement;

internal static class PlacementValidator
{
    private static readonly Collider[] OverlapBuffer = new Collider[64];

    public static bool Validate(
        VehicleDefinition vehicle,
        GameObject preview,
        FactionHQ localHq,
        RaycastHit hit,
        Vector3 position,
        Quaternion rotation,
        float maximumSlope,
        float fobPlacementRadius,
        out string reason)
    {
        float slope = Vector3.Angle(hit.normal, Vector3.up);
        if (slope > Mathf.Clamp(maximumSlope, 0f, 89f))
        {
            reason = $"terrain is too steep ({slope:0}°)";
            return false;
        }

        if (hit.point.y <= Datum.LocalSeaY + 0.1f)
        {
            reason = "the position is at sea level or underwater";
            return false;
        }

        if (!DeploymentAreaValidator.IsAllowed(
                localHq,
                position,
                fobPlacementRadius,
                out reason))
        {
            return false;
        }

        Vector3 halfExtents = new(
            Mathf.Max(vehicle.width * 0.45f, 0.75f),
            Mathf.Max(vehicle.height * 0.4f, 0.5f),
            Mathf.Max(vehicle.length * 0.45f, 0.75f));

        Vector3 overlapCenter = position + hit.normal.normalized * (halfExtents.y + 0.2f);
        int overlapCount = Physics.OverlapBoxNonAlloc(
            overlapCenter,
            halfExtents,
            OverlapBuffer,
            rotation,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        string? obstacle = null;
        for (int index = 0; index < overlapCount; index++)
        {
            Collider collider = OverlapBuffer[index];
            OverlapBuffer[index] = null!;

            if (collider == null || collider is TerrainCollider || collider.transform.IsChildOf(preview.transform))
            {
                continue;
            }

            obstacle ??= collider.transform.root.name;
        }

        if (obstacle != null)
        {
            reason = $"the space is occupied by {obstacle}";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
