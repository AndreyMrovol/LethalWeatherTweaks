using System.Collections.Generic;
using System.Linq;
using WeatherRegistry;

namespace WeatherTweaks.Definitions
{
  public class CombinedWeatherType : WeatherTweaksWeather
  {
    // private List<Weather> _weathers = [];
    public List<Weather> Weathers
    {
      get { return WeatherTypes.Select(weatherType => WeatherRegistry.WeatherManager.GetWeather(weatherType.WeatherType)).ToList(); }
    }

    public new WeatherTweaksConfig Config
    {
      get { return (WeatherTweaksConfig)base.Config; }
    }

    public float WeightModify;

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
      int averageWeight = (int)Weathers.Average(weather => weather.DefaultWeight);
      Config.DefaultWeight = new((int)(averageWeight * WeightModify / Weathers.Count));

      base.Init();
    }

    public CombinedWeatherType(string name, List<WeatherResolvable> weathers, float weightModifier = 0.2f)
      : base(name, CustomWeatherType.Combined, weathers.ToArray())
    {
      if (weathers.Count == 0)
      {
        return;
      }

      WeightModify = weightModifier;

      Name = name;

      this.CustomType = CustomWeatherType.Combined;

      Plugin.logger.LogDebug($"Created CombinedWeatherType: {Name}");

      Variables.CombinedWeathers.Add(this);
      WeatherManager.RegisterWeather(this);
    }
  }
}
