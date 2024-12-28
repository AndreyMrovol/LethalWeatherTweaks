using HarmonyLib;
using LethalLevelLoader;

namespace WeatherTweaks.Compatibility
{
  public static class LLL
  {
    internal static MrovLib.Logger logger = new("WeatherTweaks LLL", ConfigManager.LogLogs);

    internal static void Init()
    {
      var harmony = new Harmony("WeatherTweaks.LLL");

      if (MrovLib.Plugin.LLL.IsModPresent)
      {
        logger.LogWarning("Patching LethalLevelLoader");
        // Patch the ExtendedLevel GetWeatherConditions method
        harmony.Patch(
          AccessTools.Method(typeof(LethalLevelLoader.TerminalManager), "GetWeatherConditions"),
          postfix: new HarmonyMethod(typeof(Compatibility.LLL), "PatchNewLLL")
        );
      }

      Plugin.IsLLLPresent = true;
    }

    private static void PatchNewLLL(ExtendedLevel extendedLevel, ref string __result)
    {
      __result = PatchLLL(extendedLevel.SelectableLevel);
    }

    private static void PatchOldLLL(SelectableLevel selectableLevel, ref string __result)
    {
      __result = PatchLLL(selectableLevel);
    }

    private static string PatchLLL(SelectableLevel selectableLevel)
    {
      string currentWeather = Variables.GetPlanetCurrentWeather(selectableLevel);

      logger.LogDebug($"GetMoonConditions {selectableLevel.PlanetName}::{currentWeather}");

      if (currentWeather == "None")
      {
        currentWeather = "";
      }
      else if (currentWeather.Contains("[") || currentWeather.Contains("]"))
      {
        //
      }
      else
      {
        currentWeather = $"({currentWeather})";
      }

      return currentWeather;
    }
  }
}
