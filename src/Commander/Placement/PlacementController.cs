using System;
using BepInEx.Logging;
using UnityEngine;

namespace Commander.Placement;

internal sealed class PlacementController : IDisposable
{
    private readonly CommanderSettings _settings;
    private readonly ManualLogSource _log;
    private readonly Func<bool> _isPointerOverUi;
    private readonly VehiclePurchaseService _purchaseService;

    private PlacementPreview? _preview;
    private bool _cursorStateCaptured;
    private bool _previousCursorVisible;
    private CursorLockMode _previousCursorLockMode;

    public PlacementController(
        CommanderSettings settings,
        ManualLogSource log,
        Func<bool> isPointerOverUi)
    {
        _settings = settings;
        _log = log;
        _isPointerOverUi = isPointerOverUi;
        _purchaseService = new VehiclePurchaseService(settings, log);
    }

    public bool IsActive { get; private set; }
    public VehicleDefinition? SelectedVehicle { get; private set; }
    public string LastMessage { get; private set; } = string.Empty;

    public string PreviewStatus
    {
        get
        {
            if (_preview == null || !_preview.HasSurfaceHit)
            {
                return "Move the pointer over the terrain to display the preview.";
            }

            return _preview.IsValid
                ? "Valid placement."
                : $"Invalid placement: {_preview.InvalidReason}.";
        }
    }

    public void Tick(bool enabled)
    {
        if (!enabled)
        {
            Close();
            return;
        }

        if (IsActive && !CanUseMenu(out string closeReason))
        {
            Close();
            _log.LogInfo($"Commander closed automatically: {closeReason}");
            return;
        }

        if (_settings.TogglePlacementMode.Value.IsDown())
        {
            Toggle();
        }

        if (!IsActive)
        {
            return;
        }

        KeepCursorAvailable();

        if (_preview == null || _isPointerOverUi())
        {
            return;
        }

        RotatePreview();
        _preview.UpdateFromCursor(
            _settings.MaximumSlope.Value,
            _settings.FobPlacementRadius.Value);
        TryConfirmPlacement();
    }

    public void SelectVehicle(VehicleDefinition vehicle)
    {
        ClearSelection();

        if (vehicle.unitPrefab == null)
        {
            LastMessage = $"{vehicle.unitName} cannot be previewed.";
            _log.LogWarning($"{vehicle.unitName} does not have a valid preview prefab.");
            return;
        }

        SelectedVehicle = vehicle;
        _preview = new PlacementPreview(vehicle);
        string cost = UnitConverter.ValueReading(vehicle.value);
        _log.LogInfo($"Vehicle selected: {vehicle.unitName} [{vehicle.vehicleType}] ({cost})");
    }

    public void Close()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        ClearSelection();
        RestoreCursor();
        _log.LogInfo("Placement mode: OFF");
    }

    public void Dispose()
    {
        ClearSelection();
        RestoreCursor();
        IsActive = false;
    }

    private void Toggle()
    {
        if (IsActive)
        {
            Close();
            return;
        }

        if (!CanUseMenu(out string reason))
        {
            _log.LogWarning($"Commander is unavailable: {reason}");
            return;
        }

        IsActive = true;
        CaptureAndReleaseCursor();
        _log.LogInfo("Placement mode: ON");
    }

    private static bool CanUseMenu(out string reason)
    {
        if (GameManager.GetLocalAircraft(out _))
        {
            reason = "the player is controlling an aircraft";
            return false;
        }

        if (DynamicMap.mapMaximized)
        {
            reason = "the map is open";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private void RotatePreview()
    {
        float direction = 0f;

        if (_settings.RotatePreviewLeft.Value.IsPressed())
        {
            direction -= 1f;
        }

        if (_settings.RotatePreviewRight.Value.IsPressed())
        {
            direction += 1f;
        }

        _preview!.Rotate(direction * _settings.RotationSpeed.Value * Time.unscaledDeltaTime);
    }

    private void TryConfirmPlacement()
    {
        if (_preview == null ||
            !_preview.IsValid ||
            !_settings.ConfirmPlacement.Value.IsDown())
        {
            return;
        }

        bool purchased = _purchaseService.TryPurchase(
            _preview.Vehicle,
            _preview.Position,
            _preview.Rotation,
            out string message);

        LastMessage = message;
        if (purchased)
        {
            ClearSelection(clearMessage: false);
        }
    }

    private void ClearSelection(bool clearMessage = true)
    {
        _preview?.Dispose();
        _preview = null;
        SelectedVehicle = null;

        if (clearMessage)
        {
            LastMessage = string.Empty;
        }
    }

    private void CaptureAndReleaseCursor()
    {
        if (!_cursorStateCaptured)
        {
            _previousCursorVisible = Cursor.visible;
            _previousCursorLockMode = Cursor.lockState;
            _cursorStateCaptured = true;
        }

        KeepCursorAvailable();
    }

    private static void KeepCursorAvailable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        if (!_cursorStateCaptured)
        {
            return;
        }

        Cursor.lockState = _previousCursorLockMode;
        Cursor.visible = _previousCursorVisible;
        _cursorStateCaptured = false;
    }
}
