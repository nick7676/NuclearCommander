using System;
using UnityEngine;

namespace Commander.Placement;

internal sealed class PlacementPreview : IDisposable
{
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

    private readonly GameObject _root;
    private readonly Renderer[] _renderers;
    private float _yawOffset;

    public PlacementPreview(VehicleDefinition vehicle)
    {
        Vehicle = vehicle;
        _root = UnityEngine.Object.Instantiate(vehicle.unitPrefab);
        _root.name = $"NuclearCommander_Preview_{vehicle.jsonKey}";

        DisableGameplayComponents(_root);
        _renderers = _root.GetComponentsInChildren<Renderer>(true);
        ApplyTint(new Color(1f, 0.35f, 0.35f, 1f));
    }

    public VehicleDefinition Vehicle { get; }
    public bool HasSurfaceHit { get; private set; }
    public bool IsValid { get; private set; }
    public string InvalidReason { get; private set; } = "no terrain point selected";
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; } = Quaternion.identity;

    public void Rotate(float degrees)
    {
        _yawOffset = Mathf.Repeat(_yawOffset + degrees, 360f);
    }

    public void UpdateFromCursor(float maximumSlope, float fobPlacementRadius)
    {
        if (!GameManager.GetLocalHQ(out FactionHQ localHq) || localHq == null)
        {
            SetNoSurface("no local faction is available");
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            SetNoSurface("no camera is available");
            return;
        }

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                100000f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            SetNoSurface("no terrain point selected");
            return;
        }

        Vector3 surfaceUp = hit.normal.sqrMagnitude > 0.001f ? hit.normal.normalized : Vector3.up;
        Position = hit.point + surfaceUp * Vehicle.spawnOffset.y;
        Rotation = Quaternion.AngleAxis(camera.transform.eulerAngles.y + _yawOffset, surfaceUp);

        _root.transform.SetPositionAndRotation(Position, Rotation);
        HasSurfaceHit = true;
        IsValid = PlacementValidator.Validate(
            Vehicle,
            _root,
            localHq,
            hit,
            Position,
            Rotation,
            maximumSlope,
            fobPlacementRadius,
            out string reason);
        InvalidReason = reason;

        ApplyTint(IsValid
            ? new Color(0.35f, 1f, 0.45f, 1f)
            : new Color(1f, 0.35f, 0.35f, 1f));
    }

    public void Dispose()
    {
        if (_root != null)
        {
            UnityEngine.Object.Destroy(_root);
        }
    }

    private void SetNoSurface(string reason)
    {
        HasSurfaceHit = false;
        IsValid = false;
        InvalidReason = reason;
    }

    private static void DisableGameplayComponents(GameObject preview)
    {
        foreach (Collider collider in preview.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (Rigidbody rigidbody in preview.GetComponentsInChildren<Rigidbody>(true))
        {
            rigidbody.detectCollisions = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        foreach (MonoBehaviour behaviour in preview.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behaviour.enabled = false;
        }

        foreach (AudioSource audioSource in preview.GetComponentsInChildren<AudioSource>(true))
        {
            audioSource.Stop();
            audioSource.enabled = false;
        }

        foreach (Light light in preview.GetComponentsInChildren<Light>(true))
        {
            light.enabled = false;
        }

        foreach (ParticleSystem particleSystem in preview.GetComponentsInChildren<ParticleSystem>(true))
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void ApplyTint(Color color)
    {
        MaterialPropertyBlock properties = new();

        foreach (Renderer renderer in _renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(properties);
            properties.SetColor(ColorProperty, color);
            properties.SetColor(BaseColorProperty, color);
            renderer.SetPropertyBlock(properties);
            properties.Clear();
        }
    }
}
