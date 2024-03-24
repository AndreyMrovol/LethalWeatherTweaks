using System.Collections.Generic;
using System.Linq;
using Dissonance;
using LethalNetworkAPI;
using Newtonsoft.Json;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> currentWeatherDictionarySynced = new("previousWeather");
    public static LethalNetworkVariable<string> currentWeatherStringsSynced = new("previousWeatherStrings");
    public static LethalNetworkVariable<string> weatherEffectsSynced = new("weatherEffects");
    public static LethalNetworkVariable<string> weatherTypeSynced = new("weatherType");

    public static LethalNetworkVariable<string> currentProgressingWeatherEntry = new("currentProgressingWeatherEntry");

    public static void Init()
    {
      currentWeatherDictionarySynced.OnValueChanged += WeatherDataReceived;
      currentWeatherStringsSynced.OnValueChanged += WeatherDisplayDataReceived;
      weatherEffectsSynced.OnValueChanged += WeatherEffectsReceived;
      weatherTypeSynced.OnValueChanged += WeatherTypeReceived;

      currentProgressingWeatherEntry.OnValueChanged += ProgressingWeatherEntryReceived;
    }

    public static void WeatherDataReceived(string weatherData)
    {
      Dictionary<string, WeatherType> currentWeather = JsonConvert.DeserializeObject<Dictionary<string, WeatherType>>(weatherData);

      Plugin.logger.LogWarning(weatherData.Count());

      if (weatherData == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo($"Received weather data {weatherData} from server, applying");

      GameInteraction.SetWeather(currentWeather);
      DisplayTable.DisplayWeathersTable();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();

      Dictionary<SelectableLevel, WeatherType> newWeathers = [];
      List<SelectableLevel> levels = Variables.GetGameLevels(StartOfRound.Instance, true);

      // for every planet, find weather type and add it to newWeathers
      Variables.CurrentWeathers = [];
      levels
        .ToList()
        .ForEach(level =>
        {
          KeyValuePair<string, WeatherType> entry = currentWeather.FirstOrDefault(x => x.Key == level.PlanetName);

          if (entry.Key != null)
          {
            Variables.CurrentWeathers.Add(level, entry.Value);
          }
        });
    }

    public static void WeatherDisplayDataReceived(string weatherData)
    {
      Dictionary<string, string> weatherDisplayData = JsonConvert.DeserializeObject<Dictionary<string, string>>(weatherData);

      if (weatherDisplayData == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo($"Received weather display data {weatherData} from server, applying");

      UncertainWeather.uncertainWeathers = weatherDisplayData;
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    public static void WeatherEffectsReceived(string weatherEffects)
    {
      List<LevelWeatherType> effectsDeserialized = JsonConvert.DeserializeObject<List<LevelWeatherType>>(weatherEffects);
      List<WeatherEffect> currentEffects = [];
      if (currentEffects == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      foreach (WeatherEffect effect in TimeOfDay.Instance.effects)
      {
        Plugin.logger.LogDebug($"Checking effect {effect.name} ({TimeOfDay.Instance.effects.ToList().IndexOf(effect)}");

        if (effectsDeserialized.Contains((LevelWeatherType)TimeOfDay.Instance.effects.ToList().IndexOf(effect)))
        {
          currentEffects.Add(effect);

          Plugin.logger.LogDebug($"Adding effect {effect.name}");
        }
      }

      Plugin.logger.LogInfo($"Received weather effects data {weatherEffects} from server, applying");

      GameInteraction.SetWeatherEffects(TimeOfDay.Instance, currentEffects);
    }

    public static void WeatherTypeReceived(string weatherType)
    {
      WeatherType currentWeather = JsonConvert.DeserializeObject<WeatherType>(weatherType);

      if (currentWeather == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo($"Received weather type data {weatherType} from server, applying");

      Variables.CurrentLevelWeather = Variables.GetFullWeatherType(currentWeather);
      StartOfRound.Instance.currentLevel.currentWeather = Variables.CurrentLevelWeather.weatherType;
    }

    public static void ProgressingWeatherEntryReceived(string progressingWeatherEntry)
    {
      ProgressingWeatherEntry entry = JsonConvert.DeserializeObject<ProgressingWeatherEntry>(progressingWeatherEntry);

      if (entry == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo($"Received progressing weather entry data {progressingWeatherEntry} from server, applying");

      ChangeMidDay.DoMidDayChange(entry);
    }

    public static void SetWeather(Dictionary<string, WeatherType> currentWeathers)
    {
      string serialized = JsonConvert.SerializeObject(
        currentWeathers,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );

      currentWeatherDictionarySynced.Value = serialized;
      Plugin.logger.LogInfo($"Set weather data on server: {serialized}");
    }

    public static void SetDisplayWeather(Dictionary<string, string> uncertainWeathers)
    {
      string serialized = JsonConvert.SerializeObject(
        uncertainWeathers,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );

      currentWeatherStringsSynced.Value = serialized;
      Plugin.logger.LogInfo($"Set weather display data on server: {serialized}");
    }

    public static void SetWeatherEffects(List<LevelWeatherType> weatherEffectsIndexes)
    {
      if (weatherEffectsIndexes == null)
      {
        return;
      }

      weatherEffectsIndexes.Remove(LevelWeatherType.None);
      Variables.CurrentEffects.RemoveAll(effect => effect == null);

      string serialized = JsonConvert.SerializeObject(
        weatherEffectsIndexes,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );

      if (serialized == weatherEffectsSynced.Value)
      {
        return;
      }

      weatherEffectsSynced.Value = serialized;
      Plugin.logger.LogInfo($"Set weather effects on server: {serialized}");
    }

    public static void SetWeatherType(WeatherType weatherType)
    {
      string serialized = JsonConvert.SerializeObject(
        weatherType,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );

      Plugin.logger.LogInfo($"Set weather type on server: {serialized}");
      weatherTypeSynced.Value = serialized;
    }

    public static void SetProgressingWeatherEntry(ProgressingWeatherEntry entry)
    {
      string serialized = JsonConvert.SerializeObject(
        entry,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );

      Plugin.logger.LogInfo($"Set progressing weather entry on server: {serialized}");
      currentProgressingWeatherEntry.Value = serialized;
    }
  }
}
