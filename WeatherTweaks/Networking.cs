using System.Collections.Generic;
using System.Linq;
using Dissonance;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> currentWeatherSynced = new("previousWeather");
    public static LethalNetworkVariable<string> currentWeatherStringsSynced = new("previousWeatherStrings");
    public static LethalNetworkVariable<string> weatherEffectsSynced = new("weatherEffects");

    public static void Init()
    {
      currentWeatherSynced.OnValueChanged += WeatherDataReceived;
      currentWeatherStringsSynced.OnValueChanged += WeatherDisplayDataReceived;
      weatherEffectsSynced.OnValueChanged += WeatherEffectsReceived;
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
      List<SelectableLevel> levels = Variables.GetGameLevels(StartOfRound.Instance);

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
      Plugin.logger.LogWarning("Weather effects received");
      // List<WeatherEffect> currentEffects = JsonConvert.DeserializeObject<List<WeatherEffect>>(serializedWeatherEffects);

      List<LevelWeatherType> effectsDeserialized = JsonConvert.DeserializeObject<List<LevelWeatherType>>(weatherEffects);
      List<WeatherEffect> currentEffects = [];

      foreach (WeatherEffect effect in TimeOfDay.Instance.effects)
      {
        Plugin.logger.LogDebug($"Checking effect {effect.name}");

        /// if indexof effect in effects matches the effectsDeserialized index
        ///

        Plugin.logger.LogDebug($"Index of effect: {TimeOfDay.Instance.effects.ToList().IndexOf(effect)}");

        if (effectsDeserialized.Contains((LevelWeatherType)TimeOfDay.Instance.effects.ToList().IndexOf(effect)))
        {
          currentEffects.Add(effect);

          Plugin.logger.LogDebug($"Adding effect {effect.name}");
        }
      }

      foreach (WeatherEffect effect in currentEffects)
      {
        Plugin.logger.LogDebug($"Effect: {effect.name}");
      }

      if (currentEffects == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo($"Received weather effects data {weatherEffects} from server, applying");

      GameInteraction.SetWeatherEffects(TimeOfDay.Instance, currentEffects);
    }

    public static void SetWeather(Dictionary<string, WeatherType> previousWeather)
    {
      string serialized = JsonConvert.SerializeObject(
        previousWeather,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );

      currentWeatherSynced.Value = serialized;
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
  }
}
