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
        bool isUncertainWeather = UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName);

        table.AddRow(
          MrovLib.StringResolver.GetNumberlessName(level),
          Variables.GetPlanetCurrentWeather(level, false),
          isUncertainWeather ? UncertainWeather.uncertainWeathers[level.PlanetName] : ""
        );
      }

      Plugin.logger.LogMessage("Currently set weathers: \n" + table.ToMinimalString());
    }
  }
}
