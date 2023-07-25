## Basic Config

**FancyMouse** stores its configuration in a json file called ```appSettings.json``` alongside the main ```FancyMouse.exe```.

In the absence of this file (or if any settings is not specified in the file), FancyMouse uses hard-coded built-in defaults.

Below are the config settings you're most likely to want to change:

```json
{
  "version": 2,

  "hotkey": "CTRL + ALT + SHIFT + F",

  "preview": {
    "size": {
      "width": 1600,
      "height": 1200
    }
  }

}
```

| Key | Description |
|-----|-------------|
| **version** | The version of config format used in this config file. Used for detemining how to read the rest of the file. This should be set to ```2``` for the latest version of the config format, and where possible you should manually reformat older config files to the latest version.
| **hotkey**  | The key combination used to activate the preview popup.
| **preview** | A container for style settings for the preview popup.
| **preview.size.width** | The maximum width of the preview popup in pixels
| **preview.size.height** | The maximum height of the preview popup in pixels

### See also

* [Advanced Config](./advanced_config.md)