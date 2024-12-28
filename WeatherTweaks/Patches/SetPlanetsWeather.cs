using System.Collections.Generic;
using HarmonyLib;
using WeatherRegistry;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetPlanetsWeatherPatch
  {
    [HarmonyPatch("SetPlanetsWeather")]
    [HarmonyPrefix]
    [HarmonyAfter(WeatherRegistry.Plugin.GUID)]
    private static bool GameMethodPatch(int connectedPlayersOnServer, StartOfRound __instance)
    {
      Plugin.logger.LogMessage("SetPlanetsWeather called.");

      if (!Variables.IsSetupFinished || !WeatherManager.IsSetupFinished)
      {
        Plugin.logger.LogWarning("Setup not finished");
        return true;
      }

      if (__instance == null)
      {
        Plugin.logger.LogWarning("Instance is null");
        return true;
      }

      List<SelectableLevel> Levels = Variables.GetGameLevels();
      if (Levels == null)
      {
        Plugin.logger.LogWarning("Levels are null");
        return true;
      }

      // Variables.PopulateWeathers(__instance);
      ChangeMidDay.Reset();
      WeatherRegistry.Patches.EntranceTeleportPatch.isPlayerInside = false;

      // NetworkedConfig.SetWeatherEffects([]);
      // Variables.CurrentWeathers = [];

      // there are 3 possible cases:
      // we're hosting - mod is active, we're syncing weather data
      // we're a client - mod is active, we're refreshing weathers based on received data
      // we're a client - mod is not active, we're refreshing weathers based on vanilla data

      // because of how the vanilla mechanic works, weather is set on every client separately from seeded randomness
      // but this mod needs previous day's weather to calculate current day's weather, so it's not synced on joining
      // late joining mods are kinda broken - they pull previous weather from ass (and also two times?)

      // based on lobby tag we decide what scenario we're in

      // this would probably require two-way communication - if all the clients don't have this mod, using it would cause desync

      bool isLobby = GameNetworkManager.Instance.currentLobby != null;

      if (__instance.IsHost)
      {
        // Variables.CurrentWeathers = [];

        // Dictionary<string, WeatherType> newWeathers = WeatherCalculation.NewWeathers(connectedPlayersOnServer, __instance);

        // newWeathers.Do(entry =>
        // {
        //   Plugin.logger.LogDebug($"{entry.Key} :: {entry.Value}");
        // });

        // GameInteraction.SetWeather(newWeathers);
        // NetworkedConfig.SetWeather(newWeathers);

        Dictionary<string, string> uncertainWeathers = UncertainWeather.GenerateUncertainty();

        NetworkedConfig.SetDisplayWeather(uncertainWeathers);

        __instance.SetMapScreenInfoToCurrentLevel();

        if (isLobby)
        {
          GameNetworkManager.Instance.currentLobby?.SetData("WeatherTweaks", "true");
        }
      }

      return false;
    }

    [HarmonyPatch("SetPlanetsWeather")]
    [HarmonyPostfix]
    private static void DisplayCurrentWeathers()
    {
      DisplayTable.DisplayWeathersTable();
    }
  }
}
