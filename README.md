# Nuclear Commander

Nuclear Commander is an unofficial [BepInEx 5](https://github.com/BepInEx/BepInEx) mod for **Nuclear Option** that lets the host purchase and place individual ground vehicles during a mission.

The mod uses the game's vehicle catalogue, prices, player allocation, factions, and native server spawner. Vehicles are purchased individually rather than as platoons.

## Features

- Complete individual ground-vehicle catalogue, including air-defense units
- Search and air-defense filters
- Native in-game prices and player funds
- Green/red placement preview
- Preview rotation with `Q` and `E`
- Terrain slope, water, and collision checks
- Placement inside friendly airport areas
- Placement within 1 km of friendly FOBs
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
6. Start Nuclear Option.

The BepInEx log should contain:

```text
Nuclear Commander 0.13.0 loaded successfully.
```

## Usage

1. Join or start a mission as the host.
2. Make sure you are not controlling an aircraft and the tactical map is closed.
3. Press `F6` to open Nuclear Commander.
4. Search for a vehicle or select the air-defense filter.
5. Select a vehicle from the list.
6. Move the pointer over the terrain.
7. Rotate the preview with `Q` and `E`.
8. Left-click to purchase and place the vehicle.

The preview turns green when placement is valid and red when it is not. Placement is allowed only inside a friendly airport area or within 1,000 metres of a friendly FOB. The deployment area restriction is not drawn on the terrain.

Press `F6` again or use the **Close** button to leave placement mode.

## Multiplayer

Vehicle placement currently works in single-player and for the multiplayer host. Connected clients cannot submit purchase requests yet, even when they have the mod installed.

Vehicles placed by the host use Nuclear Option's native server spawner and synchronize as normal game units. Using the same mod version for everyone in the lobby is recommended.

Always obtain the server owner's permission before using gameplay-changing mods, especially in public or competitive sessions.

## Configuration

BepInEx creates the configuration file after the first launch:

```text
BepInEx/config/com.nick7676.nuclearcommander.cfg
```

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

## Troubleshooting

- Verify that `Commander.dll` is inside `BepInEx/plugins/Commander/`.
- Check `BepInEx/LogOutput.log` for loading or runtime errors.
- Make sure BepInEx is installed next to `NuclearOption.exe`, not inside `NuclearOption_Data`.
- Update Nuclear Commander after a Nuclear Option update if internal game APIs have changed.
- Temporarily disable other plugins when investigating a conflict.

## Disclaimer

Nuclear Commander is an unofficial community mod and is not affiliated with or endorsed by Shockfront Studios or BepInEx.

Nuclear Option is in Early Access. Game updates may break unofficial code mods until they are updated.

## License

Released under the [MIT License](LICENSE).
