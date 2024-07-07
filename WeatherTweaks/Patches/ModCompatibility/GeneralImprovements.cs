using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;

// using static GeneralImprovements.Utilities.MonitorsHelper;

namespace WeatherTweaks.Patches
{
  public class GeneralImprovementsWeather(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    static Type type;

    static FieldInfo weatherMonitorsField;
    static FieldInfo fancyMonitorsField;

    static int frame = 0;

    internal static MrovLib.Logger logger = new("WeatherTweaks GI", ConfigManager.LogLogs);

    public static void Init()
    {
      var assemblyName = "ShaosilGaming.GeneralImprovements";

      var pluginsLoaded = Chainloader.PluginInfos;
      bool isGeneralImprovementsLoaded = pluginsLoaded.ContainsKey(assemblyName);

      if (!isGeneralImprovementsLoaded)
      {
        return;
      }

      string nspace = "GeneralImprovements.Utilities";
      string className = "MonitorsHelper";

      // Get the assembly that contains the class
      var assembly = Plugin.GeneralImprovements.GetModAssembly;

      // Get the Type object for the class
      type = assembly.GetType($"{nspace}.{className}");

      if (type != null)
      {
        // The class was found. You can now use the Type object to access the class using reflection.
        Plugin.logger.LogWarning("GeneralImprovements found, patching weather displays");

        // Get the method to patch
        var method = type.GetMethod("UpdateGenericTextList", BindingFlags.Static | BindingFlags.NonPublic);

        weatherMonitorsField = type.GetField("_weatherMonitorTexts", BindingFlags.Static | BindingFlags.NonPublic);
        fancyMonitorsField = type.GetField("_fancyWeatherMonitorTexts", BindingFlags.Static | BindingFlags.NonPublic);

        // Create a Harmony instance
        var harmony = new Harmony("WeatherTweaks.GeneralImprovements");

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

      var weathermonitors = weatherMonitorsField.GetValue(null) as List<TextMeshProUGUI>;
      var fancymonitors = fancyMonitorsField.GetValue(null) as List<TextMeshProUGUI>;

      textList.Do(monitor =>
      {
        if (monitor == null)
        {
          return;
        }

        if (weathermonitors.Contains(monitor) || fancymonitors.Contains(monitor))
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
          logger.LogDebug($"Changing {text.Replace("\n", " ")} to {weatherText.Replace("\n", " ")}");

          text = weatherText;
        }
        else
        {
          // fancy monitor

          if (isWeatherStringDifferentThanType)
          {
            text = "???????????????????????????????????";

            // replace every nth question mark with empty string
            text = Regex.Replace(text, @"[?]", m => (frame++ % 20 == 0) ? " " : m.Value);

            // after every 8 characters add \n
            text = Regex.Replace(text, ".{8}", "$0\n");

            // remove all text after 4 lines
            text = Regex.Replace(text, @"(?<=\n.*\n.*\n.*\n).+", "");

            frame++;

            // if frame is 20, reset it to 0
            if (frame == 20)
            {
              frame = 0;
            }
          }
        }
      }
    }
  }
}
