using System.Collections.Generic;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class GameInteraction
  {
    internal static void SetWeather(Dictionary<string, LevelWeatherType> weatherData)
    {
      var table = new ConsoleTables.ConsoleTable("Planet", "Weather");

      SelectableLevel[] levels = StartOfRound.Instance.levels;
      foreach (SelectableLevel level in levels)
      {
        string levelName = level.PlanetName;

        if (weatherData.ContainsKey(levelName))
        {
          level.currentWeather = weatherData[levelName];
          table.AddRow(levelName, level.currentWeather);
        }
        else
        {
          Plugin.logger.LogWarning($"Weather data for {levelName} somehow not found, skipping");
        }
      }

      var tableToPrint = table.ToMinimalString();
      Plugin.logger.LogInfo("\n" + tableToPrint);
    }
  }
}
