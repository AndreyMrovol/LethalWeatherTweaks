using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  public static class DisplayTable
  {
    public static void DisplayWeathersTable()
    {
      if (!Variables.IsSetupFinished)
      {
        return;
      }

      var table = new ConsoleTables.ConsoleTable("Planet", "Level weather", "Uncertain weather");

      if (StartOfRound.Instance == null)
      {
        return;
      }

      List<SelectableLevel> levels = MrovLib.LevelHelper.SortedLevels;
      foreach (SelectableLevel level in levels)
      {
        Plugin.logger.LogWarning($"Level: {level.PlanetName}, is null: {level == null}");

        bool isUncertainWeather = UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName);
        Plugin.logger.LogDebug($"Is uncertain weather: {isUncertainWeather}");

        table.AddRow(
          MrovLib.StringResolver.GetNumberlessName(level),
          Variables.GetPlanetCurrentWeather(level, false),
          isUncertainWeather ? UncertainWeather.uncertainWeathers[level.PlanetName] : ""
        );
      }

      Plugin.logger.LogInfo("Currently set weathers: \n" + table.ToMinimalString());
    }
  }
}
