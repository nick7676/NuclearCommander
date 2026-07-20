using System;
using BepInEx.Logging;
using NuclearOption.Networking;
using UnityEngine;

namespace Commander.Placement;

internal sealed class VehiclePurchaseService
{
    private readonly CommanderSettings _settings;
    private readonly ManualLogSource _log;

    public VehiclePurchaseService(CommanderSettings settings, ManualLogSource log)
    {
        _settings = settings;
        _log = log;
    }

    public bool TryPurchase(
        VehicleDefinition vehicle,
        Vector3 position,
        Quaternion rotation,
        out string message)
    {
        if (!GameManager.GetLocalPlayer<Player>(out Player localPlayer) || localPlayer.HQ == null)
        {
            return Reject("no local player or faction is available", out message);
        }

        if (!localPlayer.IsServer)
        {
            return Reject(
                "placement is currently available only in single-player or to the host",
                out message);
        }

        float cost = vehicle.value;
        if (localPlayer.Allocation < cost)
        {
            return Reject("insufficient funds", out message);
        }

        Spawner spawner = Spawner.i;
        if (spawner == null || !spawner.IsServer)
        {
            return Reject("the server spawner is unavailable", out message);
        }

        string uniqueName = $"{vehicle.jsonKey}_commander_{Guid.NewGuid():N}";
        bool fundsDeducted = false;

        try
        {
            localPlayer.AddAllocation(-cost);
            fundsDeducted = true;

            GroundVehicle spawnedVehicle = spawner.SpawnVehicle(
                vehicle.unitPrefab,
                position.ToGlobalPosition(),
                rotation,
                Vector3.zero,
                localPlayer.HQ,
                uniqueName,
                1f,
                _settings.HoldPosition.Value,
                null);

            if (spawnedVehicle == null)
            {
                throw new InvalidOperationException("Spawner.SpawnVehicle did not return a vehicle.");
            }

            Physics.SyncTransforms();

            string costText = UnitConverter.ValueReading(cost);
            _log.LogInfo($"Vehicle purchased: {vehicle.unitName} ({costText}) at {position}");
            message = $"Purchased: {vehicle.unitName} ({costText}).";
            return true;
        }
        catch (Exception exception)
        {
            if (fundsDeducted)
            {
                localPlayer.AddAllocation(cost);
            }

            _log.LogError($"Failed to spawn {vehicle.unitName}; funds refunded: {exception}");
            message = "Spawn failed: funds were refunded. Check the log for details.";
            return false;
        }
    }

    private bool Reject(string reason, out string message)
    {
        message = $"Purchase rejected: {reason}.";
        _log.LogWarning(message);
        return false;
    }
}
