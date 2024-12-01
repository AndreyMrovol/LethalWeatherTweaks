using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Definitions
{
  public partial class Types
  {
    public class CombinedWeatherType : WeatherTweaksWeather
    {
      private List<Weather> _weathers = [];
      public List<Weather> Weathers
      {
        get
        {
          if (_weathers.Count == 0)
          {
            _weathers = WeatherTypes.Select(weatherType => WeatherRegistry.WeatherManager.GetWeather(weatherType)).ToList();
          }

          return _weathers;
        }
        private set { _weathers = value; }
      }

      public new WeatherTweaksConfig Config
      {
        get { return (WeatherTweaksConfig)base.Config; }
      }

      public new float WeightModify => Config.WeightModify.Value;

      public new bool CanWeatherBeApplied(SelectableLevel level)
      {
        if (!Enabled)
        {
          return false;
        }

        var randomWeathers = level.randomWeathers;
        List<LevelWeatherType> remainingWeathers = Weathers.Select(weather => weather.VanillaWeatherType).ToList();

        foreach (RandomWeatherWithVariables weather in randomWeathers)
        {
          if (remainingWeathers.Contains(weather.weatherType))
          {
            remainingWeathers.Remove(weather.weatherType);
          }
        }

        return remainingWeathers.Count == 0;
      }

      public override (float valueMultiplier, float amountMultiplier) GetDefaultMultiplierData()
      {
        WeatherMultiplierData Data = new(this.VanillaWeatherType, 0, 0);

        foreach (Weather weather in this.Weathers)
        {
          Data.valueMultiplier += weather.ScrapValueMultiplier * 0.45f;
          Data.spawnMultiplier += weather.ScrapAmountMultiplier * 0.45f;
        }

        return (Data.valueMultiplier, Data.spawnMultiplier);
      }

      public bool Enabled => this.Config.EnableWeather.Value;

      public override void Init()
      {
        Weather baseWeather = WeatherManager.GetWeather(BaseWeatherType);
        int newWeight = (int)(baseWeather.DefaultWeight * WeightModify);

        Config.DefaultWeight = new(newWeight, false);

        base.Init();
      }

      public CombinedWeatherType(string name, List<LevelWeatherType> weathers, float weightModifier = 0.15f)
        : base(name, CustomWeatherType.Combined, weathers.ToArray())
      {
        if (weathers.Count == 0)
        {
          return;
        }

        Plugin.logger.LogWarning($"{Config} is null? {Config == null}");
        Config.WeightModify = new(weightModifier);

        Name = name;

        this.CustomType = CustomWeatherType.Combined;

        Plugin.logger.LogDebug($"Created CombinedWeatherType: {Name}");

        Variables.CombinedWeathers.Add(this);
        WeatherManager.RegisterWeather(this);
      }
    }
  }
}
