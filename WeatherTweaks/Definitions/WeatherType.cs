using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherRegistry;
using WeatherRegistry.Modules;

namespace WeatherTweaks.Definitions
{
  public class WeatherTweaksWeather : WeatherRegistry.Weather
  {
    public CustomWeatherType CustomType { get; set; } = CustomWeatherType.Normal;

    public virtual float WeightModify { get; set; } = 1f;

    public virtual bool CanWeatherBeApplied(SelectableLevel level)
    {
      return true;
    }

    public virtual (float valueMultiplier, float amountMultiplier) GetMultiplierData()
    {
      return (this?.ScrapValueMultiplier ?? 1f, this?.ScrapAmountMultiplier ?? 1f);
    }

    public virtual List<LevelWeatherType> WeatherTypes { get; set; } = [];
    public LevelWeatherType BaseWeatherType => WeatherTypes.Max();

    public override string ConfigCategory => $"WeatherTweaks Weather: {this.name}";

    // public List<Weather> Weathers => WeatherTypes.Select(weatherType => WeatherRegistry.WeatherManager.GetWeather(weatherType)).ToList();

    public WeatherTweaksWeather(Weather weather)
      : base(weather.Name, weather.Effect)
    {
      // make the Weather object work as a WeatherTweaksWeather object
      Name = weather.Name;
      Effect = weather.Effect;
      VanillaWeatherType = weather.VanillaWeatherType;

      Type = weather.Type;
      Origin = weather.Origin;
      Config = weather.Config;

      AnimationClip = weather.AnimationClip;
      WeatherTypes = [weather.VanillaWeatherType];

      this.Init();
    }

    public WeatherTweaksWeather(string name, CustomWeatherType type, LevelWeatherType[] weatherTypes)
      : base(name)
    {
      Plugin.logger.LogWarning($"Creating WeatherTweaksWeather: {name}");

      Name = name;
      CustomType = type;
      Type = WeatherType.Modded;
      Origin = WeatherOrigin.WeatherTweaks;
      VanillaWeatherType = Variables.WeatherTweaksWeather.VanillaWeatherType;

      WeatherTypes = weatherTypes.ToList();

      Config = new WeatherTweaksConfig()
      {
        EnableWeather = new(true),
        WeightModify = new(1f),
        DefaultWeight = new(0, false),
        ScrapAmountMultiplier = new(0, false),
        ScrapValueMultiplier = new(0, false),
        FilteringOption = new(false, false),
        LevelFilters = new("", false),
        LevelWeights = new("", false),
        WeatherToWeatherWeights = new("", false),
      };
    }
  }

  public enum CustomWeatherType
  {
    Normal,
    Combined,
    Progressing
  }

  [JsonObject(MemberSerialization.OptIn)]
  internal class WeatherMultiplierData(LevelWeatherType weatherType, float valueMultiplier, float spawnMultiplier)
  {
    [JsonProperty]
    public LevelWeatherType weatherType = weatherType;

    [JsonProperty]
    public float valueMultiplier = valueMultiplier;

    [JsonProperty]
    public float spawnMultiplier = spawnMultiplier;
  }
}
