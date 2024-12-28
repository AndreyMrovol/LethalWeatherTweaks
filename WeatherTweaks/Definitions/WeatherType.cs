using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherRegistry;

namespace WeatherTweaks.Definitions
{
  public class WeatherTweaksWeather : WeatherRegistry.Weather
  {
    public CustomWeatherType CustomType { get; set; } = CustomWeatherType.Normal;

    public virtual bool CanWeatherBeApplied(SelectableLevel level)
    {
      return true;
    }

    public virtual (float valueMultiplier, float amountMultiplier) GetDefaultMultiplierData()
    {
      return (this?.ScrapValueMultiplier ?? 1f, this?.ScrapAmountMultiplier ?? 1f);
    }

    public virtual List<LevelWeatherType> WeatherTypes { get; set; } = [];
    public LevelWeatherType BaseWeatherType => WeatherTypes.Max();

    public override string ConfigCategory =>
      this.Origin == WeatherOrigin.WeatherTweaks ? $"WeatherTweaks Weather: {this.name}" : base.ConfigCategory;

    public override void Init()
    {
      if (this.Origin == WeatherOrigin.WeatherTweaks)
      {
        (float valueMultiplier, float amountMultiplier) = GetDefaultMultiplierData();

        Config.ScrapValueMultiplier = new(valueMultiplier, ConfigManager.GenerateSpecialWeatherEntries.Value);
        Config.ScrapAmountMultiplier = new(amountMultiplier, ConfigManager.GenerateSpecialWeatherEntries.Value);

        // WeatherManagerManager.AddSpecialWeather(this);

        Effect.SunAnimatorBool = WeatherManager.GetWeather(WeatherTypes.Max()).Effect.SunAnimatorBool;
      }

      base.Init();
    }

    public WeatherTweaksWeather(Weather weather)
      : base(weather.Name, weather.Effect)
    {
      Plugin.DebugLogger.LogWarning("Creating WeatherTweaksWeather from Weather");

      // make the Weather object work as a WeatherTweaksWeather object
      Name = weather.Name;
      Effect = weather.Effect;
      VanillaWeatherType = weather.VanillaWeatherType;

      Type = weather.Type;
      Origin = weather.Origin;
      Config = weather.Config;

      AnimationClip = weather.AnimationClip;
      WeatherTypes = [weather.VanillaWeatherType];
    }

    public WeatherTweaksWeather(string name, CustomWeatherType type, LevelWeatherType[] weatherTypes)
      : base(name)
    {
      Plugin.DebugLogger.LogDebug($"Creating WeatherTweaksWeather: {name}");

      Name = name;
      CustomType = type;
      Type = WeatherType.Modded;
      Origin = WeatherOrigin.WeatherTweaks;

      WeatherTypes = weatherTypes.ToList();

      Effect = new WeatherTweaksEffect(null, null, WeatherTypes);

      Config = new WeatherTweaksConfig()
      {
        EnableWeather = new(true),
        DefaultWeight = new(0),
        ScrapAmountMultiplier = new(0, ConfigManager.GenerateSpecialWeatherEntries.Value),
        ScrapValueMultiplier = new(0, ConfigManager.GenerateSpecialWeatherEntries.Value),
        FilteringOption = new(true, ConfigManager.GenerateSpecialWeatherEntries.Value),
        LevelFilters = new("", ConfigManager.GenerateSpecialWeatherEntries.Value),
        LevelWeights = new(""),
        WeatherToWeatherWeights = new("", ConfigManager.GenerateSpecialWeatherEntries.Value),
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
