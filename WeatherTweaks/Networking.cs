using System.Collections.Generic;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> currentWeatherSynced = new("previousWeather");

    public static void Init()
    {
      currentWeatherSynced.OnValueChanged += WeatherDataReceived;
    }

    public static void WeatherDataReceived(string previousWeatherData)
    {
      Dictionary<string, LevelWeatherType> previousWeather = JsonConvert.DeserializeObject<
        Dictionary<string, LevelWeatherType>
      >(previousWeatherData);

      if (previousWeather == null)
      {
        return;
      }

      if (StartOfRound.Instance.IsHost)
      {
        return;
      }

      Plugin.logger.LogInfo("Received weather data from server, applying");

      GameInteraction.SetWeather(previousWeather);
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
  }
}
