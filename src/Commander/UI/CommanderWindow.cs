using System;
using Commander.Placement;
using NuclearOption.Networking;
using UnityEngine;

namespace Commander.UI;

internal sealed class CommanderWindow
{
    private const int WindowId = 19820426;

    private Rect _rect = new(20f, 80f, 500f, 640f);
    private Vector2 _scroll;
    private string _search = string.Empty;
    private VehicleFilter _filter = VehicleFilter.All;
    private PlacementController? _controller;

    private enum VehicleFilter
    {
        All,
        Support,
        Light,
        ArmoredFightingVehicles,
        Tanks,
        Artillery,
        AirDefense
    }

    public void Draw(PlacementController controller)
    {
        _controller = controller;
        _rect = GUI.Window(WindowId, _rect, DrawContents, "Nuclear Commander");
    }

    public bool IsMouseOver()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 guiPosition = new(mousePosition.x, Screen.height - mousePosition.y);
        return _rect.Contains(guiPosition);
    }

    private void DrawContents(int windowId)
    {
        if (_controller == null)
        {
            return;
        }

        GUILayout.Space(8f);
        GUILayout.Label("Purchase and place units");

        if (!GameManager.GetLocalPlayer<Player>(out Player localPlayer) || localPlayer.HQ == null)
        {
            GUILayout.Label("No local player or faction is available.");
        }
        else
        {
            DrawPlayerFunds(localPlayer);
            DrawVehicleCatalog(localPlayer, _controller);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Close"))
        {
            _controller.Close();
        }

        GUI.DragWindow(new Rect(0f, 0f, _rect.width, 28f));
    }

    private static void DrawPlayerFunds(Player localPlayer)
    {
        GUILayout.Label($"Available funds: {UnitConverter.ValueReading(localPlayer.Allocation)}");
        GUILayout.Space(6f);
    }

    private void DrawVehicleCatalog(Player localPlayer, PlacementController controller)
    {
        Encyclopedia encyclopedia = Encyclopedia.i;
        if (encyclopedia == null)
        {
            GUILayout.Label("The unit catalogue is not available yet.");
            return;
        }

        DrawFilters();
        GUILayout.Label("Search:");
        _search = GUILayout.TextField(_search);
        GUILayout.Space(4f);

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(380f));

        int visibleVehicles = 0;
        bool allowEventContent = MissionManager.AllowEventContent;

        foreach (VehicleDefinition vehicle in encyclopedia.vehicles)
        {
            if (vehicle == null ||
                !vehicle.IsAllowed(allowEventContent) ||
                !MatchesFilter(vehicle) ||
                !MatchesSearch(vehicle))
            {
                continue;
            }

            visibleVehicles++;
            DrawVehicleButton(localPlayer, vehicle, controller);
        }

        if (visibleVehicles == 0)
        {
            GUILayout.Label("No vehicles match the current filter.");
        }

        GUILayout.EndScrollView();
        DrawSelectionSummary(controller);
    }

    private void DrawFilters()
    {
        GUILayout.BeginHorizontal();
        DrawFilterButton(VehicleFilter.All, "All");
        DrawFilterButton(VehicleFilter.Support, "Support");
        DrawFilterButton(VehicleFilter.Light, "Light");
        DrawFilterButton(VehicleFilter.ArmoredFightingVehicles, "AFV");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawFilterButton(VehicleFilter.Tanks, "Tanks");
        DrawFilterButton(VehicleFilter.Artillery, "Artillery");
        DrawFilterButton(VehicleFilter.AirDefense, "Air defense");
        GUILayout.EndHorizontal();
    }

    private void DrawFilterButton(VehicleFilter filter, string label)
    {
        bool selected = _filter == filter;
        if (GUILayout.Toggle(selected, label, GUI.skin.button) && !selected)
        {
            _filter = filter;
            _scroll = Vector2.zero;
        }
    }

    private static void DrawVehicleButton(
        Player localPlayer,
        VehicleDefinition vehicle,
        PlacementController controller)
    {
        float price = controller.GetVehiclePrice(vehicle);
        bool canAfford = localPlayer.Allocation >= price;
        string cost = UnitConverter.ValueReading(price);
        string type = VehicleTypeLabels.Get(vehicle.vehicleType);

        Color previousColor = GUI.backgroundColor;
        bool previousEnabled = GUI.enabled;

        if (controller.SelectedVehicle == vehicle)
        {
            GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
        }

        GUI.enabled = canAfford;
        if (GUILayout.Button($"[{type}]  {vehicle.unitName}  -  {cost}", GUILayout.Height(30f)))
        {
            controller.SelectVehicle(vehicle);
        }

        GUI.enabled = previousEnabled;
        GUI.backgroundColor = previousColor;
    }

    private static void DrawSelectionSummary(PlacementController controller)
    {
        VehicleDefinition? selected = controller.SelectedVehicle;
        if (selected != null)
        {
            GUILayout.Label($"Selected: {selected.unitName}");
            GUILayout.Label(
                $"Type: {VehicleTypeLabels.Get(selected.vehicleType)} - " +
                $"Cost: {UnitConverter.ValueReading(controller.GetVehiclePrice(selected))}");
            GUILayout.Label(controller.PreviewStatus);
            GUILayout.Label("Q / E: rotate - left click: purchase and place.");
        }
        else
        {
            GUILayout.Label("Select a vehicle from the list.");
        }

        if (!string.IsNullOrEmpty(controller.LastMessage))
        {
            GUILayout.Label(controller.LastMessage);
        }
    }

    private bool MatchesSearch(VehicleDefinition vehicle)
    {
        return string.IsNullOrWhiteSpace(_search) ||
               vehicle.unitName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool MatchesFilter(VehicleDefinition vehicle)
    {
        return _filter switch
        {
            VehicleFilter.All => true,
            VehicleFilter.Support => vehicle.vehicleType == VehicleType.TRUCK,
            VehicleFilter.Light => vehicle.vehicleType is VehicleType.UGV or VehicleType.LCV,
            VehicleFilter.ArmoredFightingVehicles => vehicle.vehicleType == VehicleType.AFV,
            VehicleFilter.Tanks => vehicle.vehicleType == VehicleType.MBT,
            VehicleFilter.Artillery => vehicle.vehicleType == VehicleType.ART,
            VehicleFilter.AirDefense => vehicle.vehicleType is
                VehicleType.AAA or
                VehicleType.IR_SAM or
                VehicleType.R_SAM or
                VehicleType.RDR,
            _ => false
        };
    }
}
