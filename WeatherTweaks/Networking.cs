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
    public static LethalNetworkVariable<List<LevelWeatherType>> weatherEffectsSynced = new("weatherEffects");

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

    public static void WeatherEffectsReceived(List<LevelWeatherType> weatherTypes)
    {
      // List<WeatherEffect> currentEffects = JsonConvert.DeserializeObject<List<WeatherEffect>>(serializedWeatherEffects);

      List<WeatherEffect> currentEffects = weatherTypes.Select(x => TimeOfDay.Instance.effects[(int)x]).ToList();

      if (currentEffects == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo($"Received weather effects data {weatherTypes} from server, applying");

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

      if (weatherEffectsIndexes == weatherEffectsSynced.Value)
      {
        return;
      }

      weatherEffectsSynced.Value = weatherEffectsIndexes;
      Plugin.logger.LogInfo($"Set weather effects on server: {weatherEffectsIndexes}");
    }
  }
}
