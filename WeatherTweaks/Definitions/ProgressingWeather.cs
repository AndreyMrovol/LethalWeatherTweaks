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

      internal WeatherType GetWeatherType()
      {
        Weather vanillaWeather = GetWeather();
        return Variables.WeatherTypes.First(weatherType =>
          weatherType.Weather == vanillaWeather && weatherType.Type == CustomWeatherType.Normal
        );
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
            bodyText = $"The weather will be changing to {GetWeatherType().Name}",
            waitTime = 7f
          }
        ];
      }
    }

    public class ProgressingWeatherType : WeatherType
    {
      // public abstract string CreateChangingString(SelectableLevel level, System.Random random);
      public ConfigEntry<bool> Enabled;

      public List<ProgressingWeatherEntry> WeatherEntries = [];
      public LevelWeatherType StartingWeather;

      private Weather _weather = null;
      public override Weather Weather
      {
        get
        {
          if (_weather == null)
          {
            _weather = WeatherRegistry.WeatherManager.GetWeather(weatherType);
          }

          return _weather;
        }
        set { _weather = value; }
      }

      public new float WeightModify = 0.45f;

      public new bool CanWeatherBeApplied(SelectableLevel level)
      {
        if (!Enabled.Value)
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

      public override (float valueMultiplier, float amountMultiplier) GetMultiplierData()
      {
        WeatherMultiplierData Data = new(this.weatherType, 0, 0);

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

      public ProgressingWeatherType(
        string name,
        LevelWeatherType baseWeather,
        List<ProgressingWeatherEntry> weatherEntries,
        float weightModifier = 0.45f
      )
        : base(name, CustomWeatherType.Progressing)
      {
        Name = name;

        Plugin.logger.LogDebug($"Creating ChangingWeatherType: {Name}");

        // TODO
        // create configFile bindings
        Enabled = ConfigManager.configFile.Bind("1c> Changing mechanics", $"{Name} Enabled", true, $"Enable {Name} changing weather");

        WeatherEntries = weatherEntries;
        WeatherEntries.Sort((a, b) => a.DayTime.CompareTo(b.DayTime));

        StartingWeather = baseWeather;
        weatherType = baseWeather;

        WeightModify = weightModifier;

        Variables.ProgressingWeatherTypes.Add(this);
      }
    }
  }
}
