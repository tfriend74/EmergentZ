# Character and combat update

Open Assets/EMERGENTZ/Scenes/Prototype.unity and press Play. Click inside the Game view. WASD moves, mouse aims, and holding left click fires. R restarts after death.

Rifle correction: movement now strafes/backpedals without turning away from camera aim. A muzzle attached to the rifle mesh supplies both tracer origin and damage-ray origin. The camera selects the crosshair target; the barrel ray checks intervening cover. Camera and animated pose update before aiming/firing. The rifle alignment Play Mode check passed for four aim directions, muzzle origin, barrel/tracer agreement, target kills and blocking cover.

The player uses Quaternius Matt with his rifle and idle/run animations. Zombies use the Basic zombie model and animations. Kenney's additional blasters are available under Assets/ThirdParty/KenneyBlasters; weapon switching is not implemented.

Enter the five-metre ring around the campfire to heal and become immune to damage. Infection, score time and wave scheduling pause while resting. Shooting is disabled inside camp. Leaving restores normal play; resting never cures the infection.

Health and infection time are displayed by a scalable Canvas with TextMesh Pro fonts. Green is health, orange is remaining infection time.

## Verification

In a fresh Play Mode run, EMERGENTZ > Test Combat and Safe Camp checks real raycast shots, kill counting, infection reward, camp detection, healing, damage immunity, paused infection and score, firing restriction, and damage after leaving. The check temporarily relocates the player; stop Play Mode afterward. Final check passed with zero Console errors or warnings. HUD text/bars and character models were visually checked in the Game view. Mouse feel, long runs and a standalone macOS build still need player testing.

Source licenses are recorded in Assets/ThirdParty/ASSET-SOURCES.md. The existing movement controller remains in use; the imported kits supply the character and weapon art.
