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
        Weather vanillaWeather = WeatherRegistry.WeatherManager.GetWeather(Weather);
        return Variables.WeatherTypes.First(weatherType =>
          weatherType.Weather == vanillaWeather && weatherType.Type == CustomWeatherType.Normal
        );
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

      public new float weightModify = 0.6f;

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

      public bool DoesHaveWeatherHappening(LevelWeatherType weatherType)
      {
        return WeatherEntries.Any(entry => entry.Weather == weatherType);
      }

      public ProgressingWeatherType(string name, LevelWeatherType baseWeather, List<ProgressingWeatherEntry> weatherEntries)
        : base(name, CustomWeatherType.Progressing)
      {
        Name = name;

        Plugin.logger.LogDebug($"Creating ChangingWeatherType: {Name}");

        // TODO
        // create configFile bindings
        Enabled = ConfigManager.Instance.configFile.Bind("1c> Changing mechanics", $"{Name} Enabled", true, $"Enable {Name} changing weather");

        WeatherEntries = weatherEntries;
        WeatherEntries.Sort((a, b) => a.DayTime.CompareTo(b.DayTime));

        StartingWeather = baseWeather;
        Weather = WeatherRegistry.WeatherManager.GetWeather(baseWeather);
        weatherType = baseWeather;

        Variables.ProgressingWeatherTypes.Add(this);
      }
    }
  }
}
