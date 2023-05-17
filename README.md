# Color Beluga

https://github.com/megalon/color-beluga/assets/27714637/77be604f-b4a0-42c4-a594-4e11960b1995


A color assistant app for Windows!

## FAQ

### What is this?
Color Beluga is an accessibility app designed to help people with color deficient vision.

It is designed to tell you the color under the mouse cursor at all times.

### Is this a color picker?
No, if you are looking for a color picker I suggest using [Windows Power Toys.](https://github.com/microsoft/PowerToys)

## Usage

Zoom in and out with the "+" and "-" buttons.

If you zoom in all the way, it will tell you the exact color of the pixel under your cursor.

Use the "Blur" toggle to blur the screenshot and get a better average of color.

## Settings

Right-click the main window to open the settings window.

| Name       | Description |
|--------------|-----------|
| Refresh Rate | How often the color is updated. Default is 16.67ms (60 fps) |
| Color Set | Which color set to use for the color detection. This determines which names appear in the UI. |
| Theme | Application theme |

# Developing

Color Beluga is built in WPF using C#.

To develop:
- Install Visual Studio 2022
- Clone this repo
- Open `Color Beluga.sln` in Visual Studio 2022
