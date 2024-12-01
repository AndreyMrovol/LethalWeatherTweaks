using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using WeatherRegistry;

namespace WeatherTweaks.Definitions
{
  partial class Types
  {
    [JsonObject(MemberSerialization.OptIn)]
    public class ProgressingWeatherEntry
    {
      [JsonProperty]
      public float DayTime;

      [JsonProperty]
      public float Chance;

      [JsonProperty]
      public LevelWeatherType Weather;

      [Obsolete("Use GetWeather() instead")]
      internal Weather GetWeatherType()
      {
        return GetWeather();
      }

      internal Weather GetWeather()
      {
        return WeatherRegistry.WeatherManager.GetWeather(Weather);
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
      // public abstract string CreateChangingString(SelectableLevel level, System.Random random);
      public bool Enabled => Config.EnableWeather.Value;

      public List<ProgressingWeatherEntry> WeatherEntries = [];
      public LevelWeatherType StartingWeather;

      private Weather _weather = null;
      public Weather Weather
      {
        get
        {
          if (_weather == null)
          {
            _weather = WeatherRegistry.WeatherManager.GetWeather(this.VanillaWeatherType);
          }

          return _weather;
        }
        set { _weather = value; }
      }

      public new float WeightModify => Config.WeightModify.Value;

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
        List<LevelWeatherType> remainingWeathers = WeatherEntries.Select(entry => entry.Weather).Append(StartingWeather).Distinct().ToList();
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
        Weather baseWeather = WeatherManager.GetWeather(BaseWeatherType);
        int newWeight = (int)(baseWeather.DefaultWeight * WeightModify);

        Config.DefaultWeight = new(newWeight, false);

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
        return WeatherEntries.Any(entry => entry.Weather == weatherType);
      }

      // public override List<LevelWeatherType> WeatherTypes { get; set; } = [];

      public ProgressingWeatherType(
        string name,
        LevelWeatherType baseWeather,
        List<ProgressingWeatherEntry> weatherEntries,
        float weightModifier = 0.45f
      )
        : base(name, CustomWeatherType.Progressing, weatherEntries.Select(entry => entry.Weather).Append(baseWeather).Distinct().ToArray())
      {
        Name = name;

        Plugin.logger.LogDebug($"Creating ChangingWeatherType: {Name}");

        WeatherEntries = weatherEntries;
        WeatherEntries.Sort((a, b) => a.DayTime.CompareTo(b.DayTime));

        Plugin.logger.LogWarning($"{Config} is null? {Config == null}");
        Config.WeightModify = new(weightModifier);

        StartingWeather = baseWeather;

        Variables.ProgressingWeathers.Add(this);
        WeatherManager.RegisterWeather(this);
        // this.Init();
      }
    }
  }
}
