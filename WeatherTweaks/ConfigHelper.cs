using System.Linq;
using BepInEx.Configuration;

namespace WeatherTweaks
{
  public abstract class ConfigHandler<T, CT> : MrovLib.ConfigHandler<T, CT>
  {
    public ConfigHandler(CT defaultValue, string configTitle, ConfigDescription configDescription = null)
    {
      // Any additional initialization for the derived class can be done here

      DefaultValue = defaultValue;
      ConfigEntry = ConfigManager.configFile.Bind($"5> Foggy patch", configTitle, DefaultValue, configDescription);
    }
  }

  public class LevelListConfigHandler : ConfigHandler<SelectableLevel[], string>
  {
    public LevelListConfigHandler(string defaultValue, string configTitle, ConfigDescription configDescription)
      : base(defaultValue, configTitle, configDescription)
    {
      // Any additional initialization for the derived class can be done here
    }

    public override SelectableLevel[] Value
    {
      get { return WeatherRegistry.ConfigHelper.ConvertStringToLevels(ConfigEntry.Value); }
    }

    public void SetNewLevelsToIgnore(SelectableLevel[] levels)
    {
      string newValue = string.Join(";", levels.Select(level => MrovLib.StringResolver.GetNumberlessName(level)));

      DefaultValue = newValue;
      ConfigEntry.Value = newValue;
    }
  }
}
