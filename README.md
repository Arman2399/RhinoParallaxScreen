# Parallax Screen (Rhino 8 Plugin)

Parallax Screen is a Rhino 8 plugin that generates kinetic architectural facades without moving parts. Instead of optimizing for sunlight, this tool optimizes for human movement. It uses vector projection to angle individual louvers toward a specific walking path, creating a 3D "Visibility Corridor." Observers walking along this calibrated path experience continuous transparency through the facade, while anyone viewing from an off-axis angle sees a solid, opaque wall.

## Visual Documentation


### The Calibration
<img width="1783" height="1084" alt="ScreenBefore" src="https://github.com/user-attachments/assets/580e56da-6148-4dca-8f2a-1aa68f03adb7" />
*Base surface geometry and the optimal visibility curve.*

### Simulated Environment
<img width="1783" height="1084" alt="ScreenPost" src="https://github.com/user-attachments/assets/1d88b37e-bf7c-4529-aed9-b2e03258bbd5" />
*Output: Total transparency from the calibrated path (center) vs. solid massing from an oblique angle (peripheral).*

### Sightline Comparisons
<img width="1602" height="1022" alt="Sightline Map" src="https://github.com/user-attachments/assets/c5cf8c7a-0b54-4b56-8594-462ec7789168" />
*The three lines represent different sightline trajectories.*

| Optimal (Green) | Vertical Offset (Blue) | Lateral Offset (Red) |
| :---: | :---: | :---: |
| <img src="https://github.com/user-attachments/assets/cae6ec5a-f387-4f58-ac48-6f21b4e51b34" width="100%" /> | <img src="https://github.com/user-attachments/assets/457b9f52-9704-4db6-a222-c494d6bcb504" width="100%" /> | <img src="https://github.com/user-attachments/assets/58d382cc-0014-4491-8ab1-cc145d4d9502" width="100%" /> |

---

## Prerequisites
* **Rhinoceros 8** (Minimum)
* Windows OS (Not tested on Mac)

## Installation
1. Download the latest `ParallaxScreen.rhp` release.
2. Right-click the `.rhp` file, select **Properties**, and check **Unblock** if applicable.
3. Open Rhino 8.
4. Drag and drop the `.rhp` file into the Rhino viewport, or install it via the `PluginManager`.

## Usage
1. Model your host geometry. **Ensure the base facade is a single, continuous Surface (do not use split faces or polysurfaces).**
2. Draw a `Curve` representing the intended pedestrian circulation route. Calibrate the curve's elevation to standard human eye level (e.g., 1.5m offset from the floor plane) for maximum accuracy.
3. Type `ParallaxScreen` into the command line.
4. Prompt 1: Select the base surface for the screen plane. Press Enter.
5. Prompt 2: Select the observer tracking curve. Press Enter.

## Technical & Performance Notes
* **Horizontal Projection:** The script strips Z-axis data during vector projection. This locks the transparency frustum to the specific elevation of the input curve, ensuring the facade remains structurally opaque when viewed from non-standard heights.
* **Depth-to-Gap Tuning:** The strictness of the privacy boundary is dictated by the louver geometry. Increasing the louver depth (e.g., 400mm) while narrowing the division gaps shrinks the "Transparency Cone," resulting in a sharper visual cutoff when an observer steps laterally off the path.
* **Computation:** Sweeping hundreds of solid Breps along complex, non-orthogonal base surfaces requires moderate processing time. Ensure your document tolerances are appropriate and test generation limits on a smaller surface subdivision before applying to large-scale infrastructure models.
