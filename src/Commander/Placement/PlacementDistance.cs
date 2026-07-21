using UnityEngine;

namespace Commander.Placement;

internal static class PlacementDistance
{
    public static bool IsWithinHorizontalRadius(Vector3 first, Vector3 second, float radius)
    {
        if (radius <= 0f)
        {
            return false;
        }

        first.y = 0f;
        second.y = 0f;
        return (first - second).sqrMagnitude <= radius * radius;
    }
}
