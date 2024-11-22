using WeatherRegistry;
using WeatherRegistry.Modules;

namespace WeatherTweaks.Definitions
{
  public class WeatherTweaksConfig : RegistryWeatherConfig
  {
    public BooleanConfigHandler EnableWeather = new(true);
    public FloatConfigHandler WeightModify = new(1f);

    public override void Init(Weather weather)
    {
      Plugin.logger.LogInfo($"Creating WeatherConfig: {weather}");

      WeatherTweaksWeather tweaksWeather = (WeatherTweaksWeather)weather;

      EnableWeather.SetConfigEntry(weather, "Enable Weather", new("Enable this weather type"));
      WeightModify.SetConfigEntry(
        weather,
        "Weight Modify",
        new($"Scale the weight of this weather type (compared to default weight of {tweaksWeather.BaseWeatherType})")
      );

      base.Init(weather);
    }
  }
}
