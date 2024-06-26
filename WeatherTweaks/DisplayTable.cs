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
      var table = new ConsoleTables.ConsoleTable("Planet", "Weather");
      List<SelectableLevel> levels = Variables.GetGameLevels();
      foreach (SelectableLevel level in levels)
      {
        table.AddRow(level.PlanetName, level.currentWeather);
      }

      Plugin.logger.LogInfo("Currently set weathers: \n" + table.ToMinimalString());
    }
  }
}
