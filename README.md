# Rocket League Movement for CarX

A standalone movement tool for **CarX Drift Racing Online**, created by **rez**.

Add forward and reverse thrust, jumps and air steering to your car. Adjust movement strength, enable mid-air jumps and control rotation using a keyboard or controller.

## Features

- Forward and reverse thrust.
- Ground and optional mid-air jumps.
- Air pitch, yaw and roll controls.
- Adjustable thrust, jump strength, turn speed and thrust speed limit.
- Optional air rotation stabilization.
- Controller and keyboard support.
- An option to prevent supported automatic airborne resets and flips.
- A configurable menu key, saved between sessions.
- Standalone launcher with a cyan bat-and-moon icon.
- No RCLI, Kino or BepInEx installation required.

## Requirements

- Windows x64.
- CarX Drift Racing Online installed through Steam.
- A fully loaded local car in a driving session.

This release targets the game assemblies inspected during development. Game updates may require a matching tool update.

## Download and installation

1. Open this repository’s **Releases** page.
2. Download the attached release archive.
3. Extract it into its own folder.
4. Keep these files together:

   ```text
   Rocket League Movement/
   ├── Rocket League Movement.exe
   ├── Rocket-League-Movement.Mod.dll
   ├── README.md
   └── Licenses/
   ```

5. Run **Rocket League Movement.exe**.
6. Select **Launch CarX & load tool**.

The launcher can start CarX through Steam or connect to an already running game. The tool does not replace game files.

## Quick start

1. Enter a driving session and wait for your car to finish loading.
2. Press **F7** to open the menu, unless you changed the menu key.
3. Enable **Rocket movement**.
4. Adjust the settings if needed.
5. Hide the menu to use the movement controls.

Press **F10** to stop rocket movement.

The mode starts disabled each time the tool loads.

## Keyboard controls

| Key | Action |
| --- | --- |
| F7, by default | Open or close the menu |
| F10 | Stop rocket movement |
| Left Shift | Forward thrust |
| Left Alt | Reverse thrust |
| Space | Jump |
| Up / Down arrows | Air pitch |
| Left / Right arrows | Air yaw |
| Q / E | Air roll |

Normal driving controls remain active. Air steering applies while the wheels are off the ground.

## Controller controls

| Input | Action |
| --- | --- |
| RT | Forward thrust |
| LT | Reverse thrust |
| LB | Double thrust while held |
| X / Square | Jump |
| Left stick | Air pitch and yaw |
| Right stick left / right | Air roll |

Controller input depends on the game exposing a compatible active controller through Unity’s Input System. Live controller behavior still requires in-game testing.

## Movement settings

Changes to strength settings apply while the mode is active.

| Setting | Default | Available range |
| --- | --- | --- |
| Forward thrust | 20 m/s² | 5–60 m/s² |
| Reverse thrust | 12 m/s² | 5–40 m/s² |
| Jump strength | 7 m/s | 2–15 m/s |
| Air turn speed | 110°/s | 30–240°/s |
| Thrust speed limit | 350 km/h | 80–500 km/h |

### Forward and reverse thrust

Adds acceleration along your car’s forward or reverse direction.

The controller’s **LB** button doubles the requested thrust while held.

### Thrust speed limit

Limits added thrust based on speed along the car’s forward direction. Opposing thrust can still slow the car when it is above the limit.

This is not a hard limit on total vehicle speed. Gravity, normal driving and other forces can still affect your speed.

### Jump strength

Controls the upward velocity added by a jump.

Jump requests have a short cooldown to prevent repeated activation every frame.

### Allow mid-air jumps

Allows additional jumps while airborne.

When disabled, jumping requires the game to report that the car is on the ground.

### Air turn speed

Controls the target rotation rate when using pitch, yaw or roll input in the air.

### Stabilize air rotation

Helps reduce unwanted rotation when steering input is released.

This stabilizes angular movement; it does not automatically turn the car upright or hold it level.

### Reset rocket settings

Restores the default thrust, jump, turn and speed-limit settings, and enables mid-air jumps and rotation stabilization.

## Automatic airborne respawn protection

The menu includes an option to prevent supported automatic airborne resets and flips while rocket movement is active.

The protection targets the inspected native Workshop reset and flip routines. It applies to your bound local car while the mode and option are enabled.

It does not cover every possible reset source. Map-specific kill zones or other mods may still reset your car.

## Changing the menu key

Change the menu key in either place:

- **Desktop launcher → Menu key**
- **In-game menu → Menu key**

The preference is saved between sessions and stored separately from Shadow Drift.

Changes made in the launcher are checked by the loaded tool approximately once per second. If the other standalone movement tool already uses that key, the current key is kept. Choose a different key.

**F9 and F10 are reserved for movement stop actions** and are not offered as menu keys.

## Pausing and stopping

Rocket input pauses when:

- Its menu is open.
- The game loses focus.
- A supported game input lock is active, such as during some menus.

The tool clears its movement input while paused.

Rocket movement stops when:

- You press **F10**.
- You disable it in the menu.
- Your car or driving session changes.
- The tool unloads.
- A movement error is detected.

Normal gravity and collisions remain enabled during rocket movement.

## Using alongside other mods

Rocket League Movement and the standalone Shadow Drift tool can both be loaded, but only one of their movement modes can control the car at a time.

Before enabling rocket movement:

- Stop active movement modes in RCLI.
- Disable the original RL mod or other overlapping movement tools.
- Stop Shadow Drift if it is currently following a player.

The ownership check between these two standalone tools does not manage unrelated movement mods.

## Multiplayer behavior

The tool applies movement to your local car using the game’s physics.

Other players’ view depends on CarX’s normal vehicle replication and network conditions. The tool does not control remote cars or guarantee identical movement on every client.

Live multiplayer appearance and handling still require in-game testing.

## Updating

1. Disable rocket movement.
2. Close CarX and the launcher.
3. Extract the new release.
4. Replace the launcher and mod DLL together.
5. Start the updated launcher.

Do not mix binaries from different releases.

## Troubleshooting

### The launcher reports a missing DLL

Keep `Rocket-League-Movement.Mod.dll` beside `Rocket League Movement.exe`.

Extract the archive before running the launcher.

### The menu does not open

- Return to CarX and wait for loading to finish.
- Check the menu key shown in the launcher.
- Try another key if it conflicts with another mod.
- Check the launcher for a loading error.

### Rocket movement will not enable

- Spawn your local car outside the garage.
- Wait for its visuals and physics to finish loading.
- Stop Shadow Drift or other movement modes.
- Read the status message in the menu.

### Thrust or steering does not respond

- Hide the menu.
- Make sure CarX has focus.
- Close chat or other game menus.
- Check that rocket movement is enabled.
- Remember that air steering only applies off the ground.

### The controller does not respond

Try the keyboard controls first. Confirm that the game recognizes your controller and that it is the active controller.

Include the controller model and connection method when reporting an issue.

### The car still resets while flying

Check that airborne respawn protection is enabled and rocket movement is active.

The protection covers specific inspected automatic reset and flip routines. It may not prevent map-specific resets or resets caused by other mods.

### Movement feels too strong

Reduce thrust, jump strength or air turn speed. Use **Reset rocket settings** to return to the defaults.

Press **F10** at any time to stop the mode.

## Reporting an issue

Include:

- Rocket League Movement release version.
- CarX version.
- Game mode and track.
- Keyboard or controller used.
- Other movement mods loaded.
- Your movement settings.
- The exact launcher or menu error.
- Steps to reproduce the issue.
- A short video, if possible.

## Development status

This is a **beta release**.

Builds, launcher connection self-tests and automated movement logic checks passed. These checks do not replace live testing in CarX.

Driving behavior, controller compatibility and multiplayer appearance still require in-game verification.

## Credits

Created by **rez**.

Unofficial tool for CarX Drift Racing Online. Not affiliated with or endorsed by the developers of CarX or Rocket League.

Includes HarmonyX, Mono.Cecil and MonoMod components. Third-party license notices are included in the `Licenses` folder.
