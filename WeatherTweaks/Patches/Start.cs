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
    [HarmonyPriority(Priority.Last)]
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

      List<LevelWeatherType> VanillaWeathers =
      [
        LevelWeatherType.None,
        LevelWeatherType.DustClouds,
        LevelWeatherType.Foggy,
        LevelWeatherType.Rainy,
        LevelWeatherType.Stormy,
        LevelWeatherType.Flooded,
        LevelWeatherType.Eclipsed
      ];

      Dictionary<LevelWeatherType, Color> VanillaWeatherColors =
        new()
        {
          { LevelWeatherType.None, new Color(0.41f, 1f, 0.42f, 1f) },
          { LevelWeatherType.DustClouds, new Color(0.41f, 1f, 0.42f, 1f) },
          { LevelWeatherType.Foggy, new Color(1f, 0.86f, 0f, 1f) },
          { LevelWeatherType.Rainy, new Color(1f, 0.86f, 0f, 1f) },
          { LevelWeatherType.Stormy, new Color(1f, 0.57f, 0f, 1f) },
          { LevelWeatherType.Flooded, new Color(1f, 0.57f, 0f, 1f) },
          { LevelWeatherType.Eclipsed, new Color(1f, 0f, 0f, 1f) }
        };

      Plugin.logger.LogMessage("Creating NoneWeather type");

      Weather noneWeather = new Weather("None", new Definitions.WeatherEffect(null, null))
      {
        Type = Definitions.Type.Clear,
        Color = VanillaWeatherColors[LevelWeatherType.None],
        VanillaWeatherType = LevelWeatherType.None
      };

      Variables.NoneWeather = new WeatherType("None", CustomWeatherType.Normal) { Weather = noneWeather };

      for (int i = 0; i < effects.Count(); i++)
      {
        WeatherEffect effect = effects[i];
        Plugin.logger.LogWarning($"Effect: {effect.name}");

        LevelWeatherType weatherType = (LevelWeatherType)i;
        bool isVanilla = VanillaWeathers.Contains(weatherType);

        Type weatherTypeType = isVanilla ? Definitions.Type.Vanilla : Definitions.Type.Modded;
        Color weatherColor = isVanilla ? VanillaWeatherColors[weatherType] : Color.blue;

        Definitions.WeatherEffect weatherEffect =
          new(effect.effectObject, effect.effectPermanentObject) { SunAnimatorBool = effect.sunAnimatorBool, };

        Weather weather =
          new(weatherType.ToString(), weatherEffect)
          {
            Type = weatherTypeType,
            Color = weatherColor,
            VanillaWeatherType = weatherType,
          };
      }

      // if (Plugin.LethalLibPatch.IsModPresent)
      // {
      //   Plugin.logger.LogWarning("Getting LethalLib Weathers");

      //   List<Weather> lethalLibWeathers = LethalLibPatch.ConvertLLWeathers();
      // }

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

        if (weather.Type == Type.Clear)
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
          weather.WeatherVariables.Add(level, levelWeather.Variables);
        }
      }

      Variables.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
    }
  }
}
