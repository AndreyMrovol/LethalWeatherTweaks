using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherRegistry;

namespace WeatherTweaks.Definitions
{
  [JsonObject(MemberSerialization.OptIn)]
  public class ProgressingWeatherEntry
  {
    /// <summary>
    /// Time to start the weather event.
    /// </summary>
    /// <value>
    /// Value between 0 and 1 representing the normalized time of day to start the weather event.
    /// </value>
    [JsonProperty]
    public float DayTime;

    /// <summary>
    /// The chance of the weather event occurring.
    /// </summary>
    /// <value>
    /// The probability of the weather event, represented as a float between 0 and 1.
    /// </value>
    [JsonProperty]
    public float Chance = 0.8f;

    [JsonProperty]
    public WeatherResolvable Weather;

    [JsonProperty]
    public string WeatherName;

    internal Weather GetWeather()
    {
      return WeatherRegistry.WeatherManager.GetWeather(Weather.WeatherType);
    }

    internal List<DialogueSegment> GetDialogueSegment()
    {
      return
      [
        new DialogueSegment
        {
          speakerText = "Weather Forecast",
          bodyText = $"The weather will be changing to {GetWeather().Name}",
          waitTime = 7f
        }
      ];
    }
  }

  public class ProgressingWeatherType : WeatherTweaksWeather
  {
    public bool Enabled => Config.EnableWeather.Value;

    public List<ProgressingWeatherEntry> WeatherEntries = [];
    public WeatherResolvable StartingWeather;

    private Weather _weather = null;
    public Weather Weather
    {
      get
      {
        if (_weather == null)
        {
          _weather = WeatherRegistry.WeatherManager.GetWeather(this.StartingWeather.WeatherType);
        }

        return _weather;
      }
      set { _weather = value; }
    }

    public float WeightModify;

    public new WeatherTweaksConfig Config
    {
      get { return (WeatherTweaksConfig)base.Config; }
    }

    public new bool CanWeatherBeApplied(SelectableLevel level)
    {
      if (!Enabled)
      {
        return false;
      }

      var randomWeathers = level.randomWeathers;
      List<LevelWeatherType> remainingWeathers = WeatherEntries
        .Select(entry => entry.Weather.WeatherType)
        .Append(StartingWeather.WeatherType)
        .Distinct()
        .ToList();
      remainingWeathers.RemoveAll(weather => weather == LevelWeatherType.None);

      foreach (RandomWeatherWithVariables weather in randomWeathers)
      {
        if (remainingWeathers.Contains(weather.weatherType))
        {
          remainingWeathers.Remove(weather.weatherType);
        }
      }

      return remainingWeathers.Count == 0;
    }

    public override void Init()
    {
      int averageWeight = (int)WeatherEntries.ToList().Select(entry => entry.GetWeather()).Average(weather => weather.DefaultWeight);
      Config.DefaultWeight = new((int)(averageWeight * WeightModify / WeatherEntries.Count));

      base.Init();
    }

    public override (float valueMultiplier, float amountMultiplier) GetDefaultMultiplierData()
    {
      WeatherMultiplierData Data = new(this.VanillaWeatherType, 0, 0);

      float sumMultiplier = 0;
      float sumSpawnMultiplier = 0;

      float sumChances = 0;

      foreach (ProgressingWeatherEntry entry in this.WeatherEntries)
      {
        Weather weather = entry.GetWeather();

        WeatherMultiplierData data = new(weather.VanillaWeatherType, weather.ScrapValueMultiplier, weather.ScrapAmountMultiplier);

        sumMultiplier += data.valueMultiplier * entry.Chance;
        sumSpawnMultiplier += data.spawnMultiplier * entry.Chance;
        sumChances += entry.Chance;
      }

      Data.valueMultiplier = sumMultiplier / sumChances;
      Data.spawnMultiplier = sumSpawnMultiplier / sumChances;

      return (Data.valueMultiplier, Data.spawnMultiplier);
    }

    public bool DoesHaveWeatherHappening(LevelWeatherType weatherType)
    {
      return WeatherEntries.Any(entry => entry.Weather.WeatherType == weatherType);
    }

    // public override List<LevelWeatherType> WeatherTypes { get; set; } = [];

    public ProgressingWeatherType(
      string name,
      WeatherResolvable baseWeather,
      List<ProgressingWeatherEntry> weatherEntries,
      float weightModifier = 0.3f
    )
      : base(name, CustomWeatherType.Progressing, weatherEntries.Select(entry => entry.Weather).Append(baseWeather).Distinct().ToArray())
    {
      Name = name;

      Plugin.logger.LogDebug($"Creating ChangingWeatherType: {Name}");

      WeatherEntries = weatherEntries;
      WeatherEntries.Sort((a, b) => a.DayTime.CompareTo(b.DayTime));

      Plugin.logger.LogWarning($"{Config} is null? {Config == null}");
      WeightModify = weightModifier;

      StartingWeather = baseWeather;

      Variables.ProgressingWeathers.Add(this);
      WeatherManager.RegisterWeather(this);
      // this.Init();
    }
  }
}
