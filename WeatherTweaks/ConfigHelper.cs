using BepInEx.Configuration;

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

    // public override SelectableLevel[] Value
    // {
    //   get { return WeatherRegistry.ConfigHelper.ConvertStringToLevels(ConfigEntry.Value); }
    // }
  }
}
