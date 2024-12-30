using BepInEx.Configuration;
using WeatherRegistry;

namespace WeatherTweaks
{
  public class LevelListConfigHandler : WeatherRegistry.LevelListConfigHandler
  {
    public LevelListConfigHandler(string defaultValue, bool enabled = true)
      : base(defaultValue, enabled)
    {
      // Any additional initialization for the derived class can be done here
    }

    public void CreateConfigEntry(string configTitle, ConfigDescription configDescription = null)
    {
      ConfigEntry = ConfigManager.configFile.Bind($"Foggy patch", configTitle, DefaultValue, configDescription);
    }
  }

  public class ConfigHelper
  {
    public static Weather GetWeatherFromString(string weatherName)
    {
      return WeatherRegistry.ConfigHelper.ResolveStringToWeather(weatherName);
    }
  }
}
