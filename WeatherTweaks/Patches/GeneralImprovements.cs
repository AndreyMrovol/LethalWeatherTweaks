using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using TMPro;

// using static GeneralImprovements.Utilities.MonitorsHelper;

namespace WeatherTweaks
{
  public class GeneralImprovementsWeather
  {
    static Type type;

    public static void Init()
    {
      var pluginsLoaded = Chainloader.PluginInfos;
      bool isGeneralImprovementsLoaded = pluginsLoaded.ContainsKey("ShaosilGaming.GeneralImprovements");

      if (!isGeneralImprovementsLoaded)
      {
        return;
      }

      string nspace = "GeneralImprovements.Utilities";
      string className = "MonitorsHelper";
      string assemblyName = "GeneralImprovements"; // Replace with the actual assembly name

      // Get the assembly that contains the class
      var assembly = Assembly.Load(assemblyName);

      // Get the Type object for the class
      type = assembly.GetType($"{nspace}.{className}");

      if (type != null)
      {
        // The class was found. You can now use the Type object to access the class using reflection.
        Plugin.logger.LogWarning("GeneralImprovements found, patching weather displays");

        // Get the method to patch
        var method = type.GetMethod("UpdateGenericTextList", BindingFlags.Static | BindingFlags.NonPublic);

        // Create a Harmony instance
        var harmony = new Harmony("Weathertweaks.GIPatch");

        // Create a HarmonyMethod for the postfix
        var patch = new HarmonyMethod(
          typeof(GeneralImprovementsWeather).GetMethod(nameof(TextPatch), BindingFlags.Static | BindingFlags.Public)
        );

        // Apply the patch
        harmony.Patch(method, prefix: patch);
      }
      else
      {
        // The class was not found. Handle this situation as needed.
      }
    }

    public static void TextPatch(List<TextMeshProUGUI> textList, ref string text)
    {
      bool isWeatherMonitor = false;

      textList.Do(monitor =>
      {
        if (monitor == null)
        {
          return;
        }

        if (monitor.name.Contains("WeatherText"))
        {
          isWeatherMonitor = true;
        }
      });

      if (isWeatherMonitor)
      {
        var weather = Variables.GetPlanetCurrentWeather(StartOfRound.Instance.currentLevel);
        bool isWeatherStringDifferentThanType = StartOfRound.Instance.currentLevel.currentWeather.ToString() != weather;

        if (text.Contains("WEATHER:\n"))
        {
          // weather monitor
          var weatherText = $"WEATHER:\n{weather}";
          Plugin.logger.LogDebug($"Changing {text.Replace("\n", " ")} to {weatherText.Replace("\n", " ")}");

          text = weatherText;
        }
        else
        {
          // fancy monitor

          if (isWeatherStringDifferentThanType)
          {
            text = "???";
          }
        }
      }
    }
  }
}
