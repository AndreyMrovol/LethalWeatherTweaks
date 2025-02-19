using BepInEx.Configuration;

namespace WeatherTweaks.Modules
{
  public abstract class UncertainWeatherType
  {
    public string Name;
    public abstract string CreateUncertaintyString(SelectableLevel level, System.Random random);
    public ConfigEntry<bool> Enabled;

    public UncertainWeatherType(string name)
    {
      Name = name;

      Plugin.logger.LogDebug($"Creating UncertainWeatherType: {Name}");

      // create configFile bindings
      Enabled = ConfigManager.configFile.Bind("Uncertain mechanics", $"{Name} Enabled", true, $"Enable {Name} uncertainty");
    }
  }
}
