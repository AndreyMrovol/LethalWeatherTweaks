using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MoreShipUpgrades;
using TMPro;

namespace WeatherTweaks.Patches
{
  internal class LateGameUpgrades
  {
    static Type type;

    internal static void Init()
    {
      var assemblyName = "com.malco.lethalcompany.moreshipupgrades";

      // Get the assembly that contains the class
      var assembly = Chainloader.PluginInfos[assemblyName].Instance.GetType().Assembly;

      // Get the Type object for the class
      type = typeof(MoreShipUpgrades.Managers.LguStore);

      if (type != null)
      {
        // The class was found. You can now use the Type object to access the class using reflection.
        Plugin.logger.LogWarning("LateGameUpgrades found, patching WeatherSync");

        // Get the method to patch
        var method = type.GetMethod("SyncWeather", BindingFlags.NonPublic | BindingFlags.Instance);

        Plugin.logger.LogDebug($"Method: {method}");

        // Create a Harmony instance
        var harmony = new Harmony("WeatherTweaks.LateGameUpgrades");

        // Create a HarmonyMethod for the postfix
        var patch = new HarmonyMethod(
          typeof(Patches.LateGameUpgrades).GetMethod("LGUSyncWeatherPatch", BindingFlags.Static | BindingFlags.NonPublic)
        );

        // Apply the patch
        harmony.Patch(method, postfix: patch);
      }
    }

    internal static void LGUSyncWeatherPatch(string level, LevelWeatherType selectedWeather)
    {
      SelectableLevel pickedLevel = Variables.GameLevels.First(x => x.PlanetName.Contains(level));
      WeatherType newWeather = Variables.WeatherTypes.First(x => x.Name == selectedWeather.ToString() && x.Type == CustomWeatherType.Vanilla);

      Dictionary<string, WeatherType> newWeathers = Variables.GetAllPlanetWeathersDictionary();
      newWeathers[pickedLevel.PlanetName] = newWeather;

      Variables.CurrentWeathers[pickedLevel] = newWeather;

      // TODO: cleanup

      if (StartOfRound.Instance.currentLevel.PlanetName == pickedLevel.PlanetName)
      {
        GameInteraction.SetWeather(newWeather);

        StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
      }

      if (StartOfRound.Instance.IsHost)
      {
        NetworkedConfig.SetWeather(newWeathers);
      }
    }
  }
}
