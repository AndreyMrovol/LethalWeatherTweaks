using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public class TerminalStartPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void Postfix(Terminal __instance)
    {
      Plugin.logger.LogInfo("Terminal Start Patch");

      WeatherEffect[] effects = TimeOfDay.Instance.effects;

      if (effects == null || effects.Count() == 0)
      {
        Plugin.logger.LogWarning("Effects are null");
      }
      else
      {
        Plugin.logger.LogWarning($"Effects: {effects.Count()}");
      }

      Plugin.logger.LogMessage("Creating NoneWeather type");

      Weather noneWeather = new Weather("None", new Definitions.WeatherEffect(null, null))
      {
        WeatherType = Definitions.Type.Clear,
        Color = new Color(0, 0, 0, 0),
        VanillaWeatherType = LevelWeatherType.None
      };

      Variables.NoneWeather = new WeatherType("None", [noneWeather], CustomWeatherType.Normal);

      for (int i = 0; i < effects.Count(); i++)
      {
        WeatherEffect effect = effects[i];
        Plugin.logger.LogWarning($"Effect: {effect.name}");

        LevelWeatherType weatherType = (LevelWeatherType)i;

        Definitions.WeatherEffect weatherEffect =
          new(effect.effectObject, effect.effectPermanentObject) { SunAnimatorBool = effect.sunAnimatorBool, };

        Weather weather =
          new(weatherType.ToString(), weatherEffect)
          {
            WeatherType = Definitions.Type.Vanilla,
            Color = Color.magenta,
            VanillaWeatherType = weatherType,
          };
      }

      for (int i = 0; i < Variables.RegisteredWeathers.Count; i++)
      {
        Plugin.logger.LogWarning($"Registered Weather: {Variables.RegisteredWeathers[i].Name}");

        Weather weather = Variables.RegisteredWeathers[i];
        Variables.Weathers.Add(weather);
      }

      Plugin.logger.LogWarning($"Weathers: {Variables.Weathers.Count}");

      List<SelectableLevel> levels = MrovLib.API.SharedMethods.GetGameLevels();

      foreach (Weather weather in Variables.Weathers)
      {
        List<LevelWeatherVariables> levelWeatherVariables = [];

        if (weather.WeatherType == Type.Clear)
        {
          continue;
        }

        foreach (SelectableLevel level in levels)
        {
          Plugin.logger.LogInfo($"Level: {level.name}, weather: {weather.Name}");

          LevelWeather levelWeather =
            new()
            {
              Weather = weather,
              Level = level,
              Variables = new()
            };

          RandomWeatherWithVariables randomWeather = null;
          // randomWeather = level
          //   .randomWeathers.Where(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType)
          //   .ToList()
          //   .FirstOrDefault();

          // do that, but [continue] the loop when the result is null
          randomWeather = level.randomWeathers.FirstOrDefault(randomWeather => randomWeather.weatherType == weather.VanillaWeatherType);

          Plugin.logger.LogWarning(
            $"Random Weather for weather {weather.Name} for level {level.PlanetName}: " + randomWeather?.weatherType.ToString() ?? "null"
          );

          if (randomWeather == null)
          {
            Plugin.logger.LogWarning("Random Weather is null");
            continue;
          }

          // if (randomWeather?.weatherType.ToString() == "")
          // {
          //   continue;
          // }

          levelWeather.Variables.Level = level;
          levelWeather.Variables.WeatherVariable1 = randomWeather?.weatherVariable ?? 1;
          levelWeather.Variables.WeatherVariable2 = randomWeather?.weatherVariable2 ?? 1;

          Variables.LevelWeathers.Add(levelWeather);
          levelWeatherVariables.Add(levelWeather.Variables);
        }

        weather.WeatherVariables = levelWeatherVariables;
      }

      Variables.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
    }
  }
}
