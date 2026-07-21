# Nuclear Commander

Nuclear Commander is a [BepInEx 5](https://github.com/BepInEx/BepInEx) mod for **Nuclear Option** that lets the host purchase and place individual ground vehicles during a mission.

The mod uses the game's vehicle catalogue, prices, player allocation, factions, and native server spawner. Vehicles are purchased individually rather than as platoons.

## Version 0.16.0

- Vehicle prices are available in the configurator before the mod is launched for the first time.
- Deployed logistics containers, repair vehicles, native mobile FOBs, and Kar Mobile FOBs can provide a placement area.
- Kar integration is optional and is ignored safely when the mod is not installed.
- Placement-area detection has been reorganized into a cached, shared validation pipeline.

## Features

- Complete individual ground-vehicle catalogue, including air-defense units
- Search and category filters for support, light, AFV, tanks, artillery, and air defense
- Configurable price for every vehicle, with native in-game prices as defaults
- Green/red placement preview
- Preview rotation with `Q` and `E`
- Terrain slope, water, and collision checks
- Placement inside friendly airport areas
- Placement within 1 km of friendly static FOBs, mobile logistics vehicles, and deployed cargo FOBs
- Optional Kar Mobile FOB compatibility; Kar is not required
- Optional hold-position order for newly placed vehicles
- Automatic refund when spawning fails

## Requirements

- Nuclear Option
- BepInEx 5 x64

## Installation

1. Install BepInEx 5 x64 in the folder containing `NuclearOption.exe`.
2. Start and close the game once so BepInEx creates its folders.
3. Download `Commander.dll` from the [latest release](https://github.com/nick7676/NuclearCommander/releases/latest).
4. Create this folder if it does not already exist:

   ```text
   <Nuclear Option>/BepInEx/plugins/Commander/
   ```

5. Copy `Commander.dll` into that folder.
6. Start Nuclear Option and reach the main menu once.

The BepInEx log should contain:

```text
Nuclear Commander 0.16.0 loaded successfully.
```

## Usage

1. Join or start a mission as the host.
2. Make sure you are not controlling an aircraft and the tactical map is closed.
3. Press `F6` to open Nuclear Commander.
4. Search for a vehicle or select a vehicle category.
5. Select a vehicle from the list.
6. Move the pointer over the terrain.
7. Rotate the preview with `Q` and `E`.
8. Left-click to purchase and place the vehicle.

The preview turns green when placement is valid and red when it is not. Placement is allowed only inside a friendly airport area or within 1,000 metres of a friendly static or mobile FOB. Munitions, fuel, supply, rearming, refuelling, and repair containers deployed by friendly helicopters or cargo vehicles count as FOB support units after they have been released and landed. Friendly mobile logistics and repair vehicles also count. Cargo still attached to an aircraft or descending under a parachute does not become active until it reaches the ground. The deployment area restriction is not drawn on the terrain.

Kar Mobile FOB support is optional. Nuclear Commander has no binary reference to Kar and continues to work with native airports, depots, and logistics units when Kar or the Mobile FOB content is not installed.

Press `F6` again or use the **Close** button to leave placement mode.

## Multiplayer

Vehicle placement currently works in single-player and for the multiplayer host. Connected clients cannot submit purchase requests yet, even when they have the mod installed.

Vehicles placed by the host use Nuclear Option's native server spawner and synchronize as normal game units. Using the same mod version for everyone in the lobby is recommended.

## Configuration

The plugin creates its configuration file during its first load:

```text
BepInEx/config/com.nick7676.nuclearcommander.cfg
```

You can also create and edit this file before the first launch with the configuration app included in the release.

Default settings:

```ini
[General]
Enabled = true

[Input]
TogglePlacementMode = F6
RotatePreviewLeft = Q
RotatePreviewRight = E
ConfirmPlacement = Mouse0

[Placement]
RotationSpeed = 90
MaximumSlope = 18
FobPlacementRadius = 1000
HoldPosition = true
```

## Configuration app

Download `NuclearCommander.Configurator.exe` from the [latest release](https://github.com/nick7676/NuclearCommander/releases/latest) to edit Nuclear Commander settings without starting the game. The configurator is a standalone desktop application and does not load the mod DLL.

Place the executable in the folder containing `NuclearOption.exe` and run it while the game is closed. It automatically finds `BepInEx/config` and provides:

- clear General, Controls, and Placement sections;
- direct editing of every Nuclear Commander setting;
- a searchable price table for every vehicle, including its original game price;
- reload and reset-to-default actions;
- automatic `.bak` backups before saving;
- automatic creation of the configuration file when necessary.

The configurator requires the [.NET 8 Desktop Runtime for Windows x64](https://dotnet.microsoft.com/download/dotnet/8.0). If automatic detection fails, use **Choose folder** and select either the Nuclear Option folder or `BepInEx/config` directly.

The configurator includes the complete vehicle price catalogue, so prices can be configured before Nuclear Commander is loaded for the first time. Open the configurator, choose the Nuclear Option folder if it is not detected automatically, edit **Vehicle prices**, and save. The DLL registers and reads the same settings during its `Awake` preload stage. Prices cannot be negative, and **Defaults** restores every vehicle's native game price.

## Troubleshooting

- Verify that `Commander.dll` is inside `BepInEx/plugins/Commander/`.
- Check `BepInEx/LogOutput.log` for loading or runtime errors.
- Make sure BepInEx is installed next to `NuclearOption.exe`, not inside `NuclearOption_Data`.
- Update Nuclear Commander after a Nuclear Option update if internal game APIs have changed.
- Temporarily disable other plugins when investigating a conflict.

## License

Released under the [MIT License](LICENSE).
