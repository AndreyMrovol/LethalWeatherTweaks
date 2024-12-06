using BepInEx.Configuration;
using WeatherRegistry;
using WeatherRegistry.Modules;

namespace WeatherTweaks.Definitions
{
  public class WeatherTweaksConfig : RegistryWeatherConfig
  {
    public BooleanConfigHandler EnableWeather = new(true);

    // public FloatConfigHandler WeightModify = new(1f);

    public override void Init(Weather weather)
    {
      Plugin.DebugLogger.LogInfo($"Creating WeatherConfig: {weather}");

      EnableWeather.SetConfigEntry(weather, "Enable Weather", new("Enable this weather type"));
      // WeightModify.SetConfigEntry(
      //   weather,
      //   "Weight Modify",
      //   new(
      //     $"Scale the weight of this weather type (compared to default weight of {tweaksWeather.BaseWeatherType})",
      //     new AcceptableValueRange<float>(0, 100)
      //   )
      // );

      base.Init(weather);
    }
  }
}