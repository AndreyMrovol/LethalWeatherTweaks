using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetPlanetsWeatherPatch
  {
    [HarmonyPatch("SetPlanetsWeather")]
    [HarmonyPrefix]
    private static bool GameMethodPatch(int connectedPlayersOnServer, StartOfRound __instance)
    {
      Plugin.logger.LogMessage("SetPlanetsWeather called.");

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
        Dictionary<string, LevelWeatherType> newWeathers = WeatherCalculation.NewWeathers(__instance);
        GameInteraction.SetWeather(newWeathers);
        NetworkedConfig.SetWeather(newWeathers);

        Dictionary<string, string> uncertainWeathers = UncertainWeather.GenerateUncertainty();
        NetworkedConfig.SetDisplayWeather(uncertainWeathers);

        __instance.SetMapScreenInfoToCurrentLevel();

        if (isLobby)
        {
          GameNetworkManager.Instance.currentLobby?.SetData("WeatherTweaks", "true");
        }
      }
      else
      {
        Plugin.logger.LogMessage("Not a host");

        if (isLobby)
        {
          if (GameNetworkManager.Instance.currentLobby?.GetData("WeatherTweaks") != null)
          {
            Plugin.logger.LogMessage("Detected mod on host, waiting for weather data");
          }
          else
          {
            Plugin.logger.LogMessage("Mod not detected on host, falling back to vanilla");
            return true;
          }
        }

        Plugin.logger.LogDebug($"Current data: {NetworkedConfig.currentWeatherSynced.Value}");
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
