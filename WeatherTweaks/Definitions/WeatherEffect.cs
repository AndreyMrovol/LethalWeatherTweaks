using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeatherRegistry;
using WeatherRegistry.Patches;

namespace WeatherTweaks.Definitions
{
  public class WeatherTweaksEffect : ImprovedWeatherEffect
  {
    public List<LevelWeatherType> weatherTypes;

    public WeatherTweaksEffect(GameObject effectObject, GameObject worldObject, List<LevelWeatherType> weatherTypes)
      : base(effectObject, worldObject)
    {
      this.weatherTypes = weatherTypes;
    }

    public override bool EffectEnabled
    {
      get => base.EffectEnabled;
      set
      {
        if (value)
        {
          if (Variables.CombinedWeatherTypes.Contains(LevelWeatherType))
          {
            EnableCombinedEffect();
            return;
          }
          else if (Variables.ProgressingWeatherTypes.Contains(LevelWeatherType))
          {
            EnableProgressingEffect();
            return;
          }
        }

        base.EffectEnabled = value;
      }
    }

    public void EnableCombinedEffect()
    {
      foreach (LevelWeatherType weatherType in weatherTypes)
      {
        Weather weather = WeatherRegistry.WeatherManager.GetWeather(weatherType);
        weather.Effect.EffectEnabled = true;

        WeatherRegistry.WeatherEffectController.SetTimeOfDayEffect(weatherType, weather.Effect.EffectEnabled);

        if (weatherType == weatherTypes.Max())
        {
          SunAnimator.OverrideSunAnimator(weatherType);
        }
      }
    }

    public void EnableProgressingEffect()
    {
      Definitions.ProgressingWeatherType currentWeather = Variables.ProgressingWeathers.First(weather =>
        weather.VanillaWeatherType == StartOfRound.Instance.currentLevel.currentWeather
      );

      LevelWeatherType startingWeatherType = currentWeather.StartingWeather;

      WeatherEffectController.SetWeatherEffects(startingWeatherType);
    }

    public override void DisableEffect(bool permament = false)
    {
      if (!permament)
      {
        foreach (LevelWeatherType weatherType in weatherTypes)
        {
          WeatherRegistry.WeatherManager.GetWeather(weatherType).Effect.EffectObject?.SetActive(false);
        }
      }

      base.DisableEffect(permament);
    }
  }
}
