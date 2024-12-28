using System.Collections.Generic;
using LethalNetworkAPI;
using Newtonsoft.Json;
using WeatherRegistry;
using static WeatherTweaks.Definitions.Types;

namespace WeatherTweaks
{
  internal class NetworkedConfig
  {
    public static LethalNetworkVariable<string> currentWeatherStringsSynced = new("previousWeatherStrings");
    public static LethalNetworkVariable<string> currentProgressingWeatherEntry = new("currentProgressingWeatherEntry");

    public static void Init()
    {
      currentWeatherStringsSynced.OnValueChanged += WeatherDisplayDataReceived;
      // weatherEffectsSynced.OnValueChanged += WeatherEffectsReceived;
      // weatherTypeSynced.OnValueChanged += WeatherTypeReceived;

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

    public static void WeatherEffectsReceived(string weatherEffects)
    {
      // input type: LevelWeatherType[]
      Plugin.logger.LogDebug($"Received weather effects: {weatherEffects}");
      List<LevelWeatherType> effectsDeserialized = JsonConvert.DeserializeObject<List<LevelWeatherType>>(weatherEffects);

      List<ImprovedWeatherEffect> currentEffects = [];
      if (currentEffects == null)
      {
        return;
      }

      foreach (Weather weather in WeatherRegistry.WeatherManager.Weathers)
      {
        // check if the weather name (not the full object) is in the deserizlied list
        if (effectsDeserialized.Contains(weather.VanillaWeatherType))
        {
          currentEffects.Add(weather.Effect);
        }
      }

      Plugin.logger.LogInfo($"Received weather effects data {weatherEffects} from server, applying");

      currentEffects.ForEach(effect => Plugin.logger.LogDebug($"Effect: {effect}"));

      GameInteraction.SetWeatherEffects(TimeOfDay.Instance, currentEffects);
    }

    // public static void WeatherTypeReceived(string weatherType)
    // {
    //   WeatherType currentWeather = JsonConvert.DeserializeObject<WeatherType>(weatherType);

    //   if (currentWeather == null)
    //   {
    //     return;
    //   }

    //   if (StartOfRound.Instance.IsHost)
    //   {
    //     return;
    //   }

    //   Plugin.logger.LogWarning($"Received weather type data {weatherType} from server, applying");

    //   Variables.CurrentLevelWeather = Variables.GetFullWeatherType(currentWeather);
    //   StartOfRound.Instance.currentLevel.currentWeather = Variables.CurrentLevelWeather.weatherType;
    // }

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

    // public static void SetWeatherEffects(List<Weather> weathers)
    // {
    //   Plugin.logger.LogDebug($"Setting weather effects: {weathers}");

    //   if (weathers == null)
    //   {
    //     return;
    //   }

    //   weathers.ForEach(weather => Plugin.logger.LogDebug($"Weather: {weather}"));

    //   weathers.Where(weather => weather.Effect != null).Select(weather => weather.VanillaWeatherType != LevelWeatherType.None);
    //   Variables.CurrentEffects.RemoveAll(effect => effect == null);

    //   string serialized = JsonConvert.SerializeObject(
    //     weathers.Select(weather => weather.VanillaWeatherType),
    //     Formatting.None,
    //     new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
    //   );

    //   if (serialized == weatherEffectsSynced.Value)
    //   {
    //     return;
    //   }

    //   weatherEffectsSynced.Value = serialized;
    //   Plugin.logger.LogInfo($"Set weather effects on server: {serialized}");
    // }

    public static void SetWeatherType(WeatherType weatherType)
    {
      // string serialized = JsonConvert.SerializeObject(
      //   weatherType,
      //   Formatting.None,
      //   new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      // );

      // Plugin.logger.LogInfo($"Set weather type on server: {serialized}");
      // weatherTypeSynced.Value = serialized;
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
