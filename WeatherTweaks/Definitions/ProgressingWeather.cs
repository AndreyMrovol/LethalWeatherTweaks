using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;

namespace WeatherTweaks.Modules
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
      public LevelWeatherType WeatherType;

      internal WeatherType GetWeatherType()
      {
        return Variables.GetVanillaWeatherType(WeatherType);
      }

      internal List<DialogueSegment> GetDialogueSegment()
      {
        return
        [
          new DialogueSegment
          {
            speakerText = "Weather Forecast",
            bodyText = $"The weather will be changing to {WeatherType}",
            waitTime = 7f
          }
        ];
      }
    }

    public abstract class ProgressingWeatherType
    {
      public string Name;

      // public abstract string CreateChangingString(SelectableLevel level, System.Random random);
      public ConfigEntry<bool> Enabled;

      public List<ProgressingWeatherEntry> WeatherEntries = [];
      public WeatherType WeatherType;

      public bool CanWeatherBeApplied(SelectableLevel level)
      {
        var randomWeathers = level.randomWeathers;
        List<LevelWeatherType> remainingWeathers = WeatherEntries
          .Select(entry => entry.WeatherType)
          .Append(WeatherType.weatherType)
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

      public ProgressingWeatherType(string name, LevelWeatherType baseWeather, List<ProgressingWeatherEntry> weatherEntries)
      {
        Name = name;

        Plugin.logger.LogDebug($"Creating ChangingWeatherType: {Name}");

        // TODO
        // create configFile bindings
        Enabled = ConfigManager.Instance.configFile.Bind("1c> Changing mechanics", $"{Name} Enabled", true, $"Enable {Name} changing weather");

        WeatherEntries = weatherEntries;
        WeatherEntries.Sort((a, b) => a.DayTime.CompareTo(b.DayTime));

        WeatherType = new(Name, (LevelWeatherType)baseWeather, [baseWeather], CustomWeatherType.Progressing) { Effects = [], };
        Variables.ProgressingWeatherTypes.Add(this);
      }
    }
  }
}
