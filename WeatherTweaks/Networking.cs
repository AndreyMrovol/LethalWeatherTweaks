using System.Collections.Generic;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> previousWeatherSynced = new("previousWeather");

    public static void Init()
    {
      previousWeatherSynced.OnValueChanged += WeatherDataReceived;
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

      Plugin.logger.LogInfo("Received previous day's weather data from server, applying");

      SetPlanetsWeatherPatch.SetWeathers(StartOfRound.Instance, previousWeather);
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    public static void SetPreviousWeather(Dictionary<string, LevelWeatherType> previousWeather)
    {
      NetworkedConfig.previousWeatherSynced.Value = JsonConvert.SerializeObject(
        previousWeather,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      );
    }

    public static Dictionary<string, LevelWeatherType> GetPreviousWeather()
    {
      return JsonConvert.DeserializeObject<Dictionary<string, LevelWeatherType>>(
        NetworkedConfig.previousWeatherSynced.Value
      );
    }
  }
}
