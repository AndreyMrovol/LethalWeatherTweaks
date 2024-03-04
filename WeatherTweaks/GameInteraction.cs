using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class GameInteraction
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks GameInteraction");

    internal static void SetWeather(Dictionary<string, WeatherType> weatherData)
    {
      List<SelectableLevel> levels = Variables.GetGameLevels(StartOfRound.Instance);
      foreach (SelectableLevel level in levels)
      {
        string levelName = level.PlanetName;

        if (weatherData.ContainsKey(levelName))
        {
          level.currentWeather = weatherData[level.PlanetName].weatherType;
          logger.LogDebug($"Setting weather for {levelName} to {level.currentWeather}");
        }
        else
        {
          Plugin.logger.LogWarning($"Weather data for {levelName} somehow not found, skipping");
        }
      }

      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    internal static void SetWeatherEffects(TimeOfDay timeOfDay, List<WeatherEffect> weatherEffects)
    {
      // logger.LogDebug("---");
      // timeOfDay.currentLevel.DaySpeedMultiplier = 5f;

      // foreach (WeatherEffect selectedEffects in weatherEffects)
      // {
      //   Plugin.logger.LogInfo($"Selected Effect: {selectedEffects.name}");
      //   Plugin.logger.LogInfo($"Effect Enabled: {selectedEffects.effectEnabled}");
      // }

      logger.LogDebug($"Setting weather effects for {timeOfDay.currentLevel.PlanetName}");

      // timeOfDay.Instance.DisableAllWeather();
      foreach (WeatherEffect timeOfDayEffect in timeOfDay.effects)
      {
        int index = timeOfDay.effects.ToList().IndexOf(timeOfDayEffect);
        RandomWeatherWithVariables weatherVariables = StartOfRound
          .Instance.currentLevel.randomWeathers.ToList()
          .Find(x => x.weatherType == (LevelWeatherType)index);

        logger.LogDebug($"Effect: {timeOfDayEffect.name}");

        // logger.LogDebug("---");
        // log every property of the effect
        // logger.LogInfo($"Effect: {timeOfDayEffect.name}");
        // logger.LogInfo($"Effect Object: {timeOfDayEffect.effectObject}");
        // logger.LogInfo($"Effect Permanent Object: {timeOfDayEffect.effectPermanentObject}");
        // logger.LogInfo($"Effect Lerp Position: {timeOfDayEffect.lerpPosition}");
        // logger.LogInfo($"Effect Enabled: {timeOfDayEffect.effectEnabled}");
        // logger.LogInfo($"Effect Sun Animator Bool: {timeOfDayEffect.sunAnimatorBool}");
        // logger.LogInfo($"Effect Transitioning: {timeOfDayEffect.transitioning}");

        if (weatherEffects.Contains(timeOfDayEffect))
        {
          logger.LogDebug($"Contains: {timeOfDayEffect.name}");

          timeOfDayEffect.effectEnabled = true;

          if (timeOfDayEffect.effectObject != null)
          {
            timeOfDayEffect.effectObject.SetActive(true);
          }

          if (timeOfDayEffect.effectPermanentObject != null)
          {
            timeOfDayEffect.effectPermanentObject.SetActive(true);
          }

          if (timeOfDayEffect.sunAnimatorBool == "eclipse")
          {
            timeOfDay.sunAnimator.SetBool(timeOfDay.effects[index].sunAnimatorBool, true);
          }

          if (timeOfDayEffect.name == "flooded")
          {
            timeOfDay.currentWeatherVariable = weatherVariables.weatherVariable;
            timeOfDay.currentWeatherVariable2 = weatherVariables.weatherVariable2;
          }

          System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);

          // logger.LogDebug($"Weather Variable: {weatherVariables.weatherVariable}");
          // logger.LogDebug($"Weather Variable2: {weatherVariables.weatherVariable2}");

          // timeOfDay.currentWeatherVariable = weatherVariables.weatherVariable * (float)random.Next(20, 80) * 0.02f;
          // timeOfDay.currentWeatherVariable2 = weatherVariables.weatherVariable2 * (float)random.Next(20, 80) * 0.02f;

          // timeOfDay.effects[index] = timeOfDayEffect;
        }
        else { }
      }
    }
  }
}
