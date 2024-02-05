using System.Collections.Generic;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> currentWeatherSynced = new("previousWeather");
    public static LethalNetworkVariable<string> currentWeatherStringsSynced = new("previousWeatherStrings");

    public static void Init()
    {
      currentWeatherSynced.OnValueChanged += WeatherDataReceived;
      currentWeatherStringsSynced.OnValueChanged += WeatherDisplayDataReceived;
    }

    public static void WeatherDataReceived(string weatherData)
    {
      Dictionary<string, LevelWeatherType> currentWeather = JsonConvert.DeserializeObject<Dictionary<string, LevelWeatherType>>(weatherData);

      if (currentWeather == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo("Received weather data from server, applying");
      Plugin.logger.LogDebug($"Received data: {weatherData}");

      GameInteraction.SetWeather(currentWeather);
      DisplayTable.DisplayWeathersTable();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
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

      Plugin.logger.LogInfo("Received weather display data from server, applying");
      Plugin.logger.LogDebug($"Received data: {weatherData}");

      UncertainWeather.uncertainWeathers = weatherDisplayData;
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    public static void SetWeather(Dictionary<string, LevelWeatherType> previousWeather)
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
  }
}
