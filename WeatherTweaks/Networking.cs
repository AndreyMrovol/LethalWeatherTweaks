using System.Collections.Generic;
using LethalNetworkAPI;
using Newtonsoft.Json;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> currentWeatherStringsSynced = new("previousWeatherStrings");
    public static LethalNetworkVariable<string> currentProgressingWeatherEntry = new("currentProgressingWeatherEntry");

    public static void Init()
    {
      currentWeatherStringsSynced.OnValueChanged += WeatherDisplayDataReceived;
      currentProgressingWeatherEntry.OnValueChanged += ProgressingWeatherEntryReceived;
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

      Plugin.logger.LogWarning($"Received progressing weather entry data {progressingWeatherEntry} from server, applying");

      entry.Weather = new WeatherNameResolvable(entry.WeatherName);
      TimeOfDay.Instance.StartCoroutine(ChangeMidDay.DoMidDayChange(entry));
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

    public static void SetProgressingWeatherEntry(ProgressingWeatherEntry entry)
    {
      entry.WeatherName = entry.Weather.WeatherName;

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
