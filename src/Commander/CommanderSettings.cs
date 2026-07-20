using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Commander;

internal sealed class CommanderSettings
{
    private readonly ConfigFile _config;
    private readonly Dictionary<string, ConfigEntry<float>> _vehiclePrices = new();
    private bool _vehiclePricesRegistered;

    public CommanderSettings(ConfigFile config)
    {
        _config = config;
        Enabled = config.Bind(
            "General",
            "Enabled",
            true,
            "Enables the plugin.");

        TogglePlacementMode = config.Bind(
            "Input",
            "TogglePlacementMode",
            new KeyboardShortcut(KeyCode.F6),
            "Toggles placement mode.");

        RotatePreviewLeft = config.Bind(
            "Input",
            "RotatePreviewLeft",
            new KeyboardShortcut(KeyCode.Q),
            "Rotates the placement preview to the left.");

        RotatePreviewRight = config.Bind(
            "Input",
            "RotatePreviewRight",
            new KeyboardShortcut(KeyCode.E),
            "Rotates the placement preview to the right.");

        ConfirmPlacement = config.Bind(
            "Input",
            "ConfirmPlacement",
            new KeyboardShortcut(KeyCode.Mouse0),
            "Purchases and places the selected vehicle.");

        RotationSpeed = config.Bind(
            "Placement",
            "RotationSpeed",
            90f,
            "Preview rotation speed in degrees per second.");

        MaximumSlope = config.Bind(
            "Placement",
            "MaximumSlope",
            18f,
            "Maximum allowed terrain slope in degrees.");

        FobPlacementRadius = config.Bind(
            "Placement",
            "FobPlacementRadius",
            1000f,
            "Maximum placement distance in metres from a friendly FOB or mobile logistics unit.");

        HoldPosition = config.Bind(
            "Placement",
            "HoldPosition",
            true,
            "Orders newly placed vehicles to hold position.");
    }

    public ConfigEntry<bool> Enabled { get; }
    public ConfigEntry<KeyboardShortcut> TogglePlacementMode { get; }
    public ConfigEntry<KeyboardShortcut> RotatePreviewLeft { get; }
    public ConfigEntry<KeyboardShortcut> RotatePreviewRight { get; }
    public ConfigEntry<KeyboardShortcut> ConfirmPlacement { get; }
    public ConfigEntry<float> RotationSpeed { get; }
    public ConfigEntry<float> MaximumSlope { get; }
    public ConfigEntry<float> FobPlacementRadius { get; }
    public ConfigEntry<bool> HoldPosition { get; }

    public void RegisterVehiclePrices()
    {
        if (_vehiclePricesRegistered ||
            Encyclopedia.i == null ||
            Encyclopedia.i.vehicles == null ||
            Encyclopedia.i.vehicles.Count == 0)
        {
            return;
        }

        bool saveOnConfigSet = _config.SaveOnConfigSet;
        _config.SaveOnConfigSet = false;
        try
        {
            foreach (VehicleDefinition vehicle in Encyclopedia.i.vehicles)
            {
                if (vehicle == null || string.IsNullOrWhiteSpace(vehicle.jsonKey))
                {
                    continue;
                }

                _vehiclePrices[vehicle.jsonKey] = _config.Bind(
                    "Vehicle Prices",
                    vehicle.jsonKey,
                    vehicle.value,
                    $"Purchase price for {vehicle.unitName}.");
            }

            _config.Save();
            _vehiclePricesRegistered = true;
        }
        finally
        {
            _config.SaveOnConfigSet = saveOnConfigSet;
        }
    }

    public float GetVehiclePrice(VehicleDefinition vehicle)
    {
        RegisterVehiclePrices();
        return _vehiclePrices.TryGetValue(vehicle.jsonKey, out ConfigEntry<float> price)
            ? Mathf.Max(0f, price.Value)
            : Mathf.Max(0f, vehicle.value);
    }
}
