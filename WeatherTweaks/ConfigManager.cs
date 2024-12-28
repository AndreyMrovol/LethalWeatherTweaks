using BepInEx.Configuration;

namespace WeatherTweaks
{
  public class ConfigManager
  {
    public static ConfigManager Instance { get; private set; }

    public static void Init(ConfigFile config)
    {
      Instance = new ConfigManager(config);
    }

    internal static ConfigFile configFile;

    // i love creating config hell

    public static ConfigEntry<bool> LogWeatherSelection { get; private set; }
    public static ConfigEntry<bool> LogWeatherVariables { get; private set; }
    public static ConfigEntry<bool> LogLogs { get; private set; }

    public static ConfigEntry<int> FirstDaySeed { get; private set; }
    public static ConfigEntry<bool> FirstDaySpecial { get; private set; }
    public static ConfigEntry<bool> FirstDayRandomSeed { get; private set; }

    public static ConfigEntry<bool> UncertainWeatherEnabled { get; private set; }

    public static ConfigEntry<bool> GenerateSpecialWeatherEntries { get; private set; }

    public static ConfigEntry<bool> AlwaysUncertain { get; private set; }
    public static ConfigEntry<bool> AlwaysUnknown { get; private set; }
    public static ConfigEntry<bool> AlwaysClear { get; private set; }

    public static ConfigEntry<float> GameLengthMultiplier { get; private set; }
    public static ConfigEntry<float> GamePlayersMultiplier { get; private set; }

    public static ConfigEntry<float> MaxMultiplier { get; private set; }
    public static ConfigEntry<bool> ScaleDownClearWeather { get; private set; }

    public static LevelListConfigHandler FoggyIgnoreLevels;

    private ConfigManager(ConfigFile config)
    {
      // god forgive me for this
      configFile = config;

      // create config entries

      LogWeatherSelection = configFile.Bind("Debug", "LogWeatherSelection", true, "Log weather selection");
      LogWeatherVariables = configFile.Bind("Debug", "LogWeatherVariables", true, "Log resolving weather variables");
      LogLogs = configFile.Bind("Debug", "Logs", true, "Log logging logs");

      UncertainWeatherEnabled = configFile.Bind("Uncertain Weathers", "UncertainWeatherEnabled", true, "Enable uncertain weather mechanic");

      MaxMultiplier = configFile.Bind(
        "Multiplier Settings",
        "MaxMultiplier",
        0.8f,
        new ConfigDescription("Maximum difficulty multiplier (between 0 and 1)", new AcceptableValueRange<float>(0, 1))
      );
      ScaleDownClearWeather = configFile.Bind(
        "Multiplier Settings",
        "ScaleDownClearWeather",
        true,
        "Scale down clear weather's weight to keep its % chance the same, no matter how many weathers are in the pool"
      );

      GameLengthMultiplier = configFile.Bind(
        "Difficulty Multiplier Settings",
        "GameLengthMultiplier",
        0.05f,
        "Difficulty multiplier - game length (quotas done)"
      );
      GamePlayersMultiplier = configFile.Bind(
        "Difficulty Multiplier Settings",
        "GamePlayersMultiplier",
        0.01f,
        "Difficulty multiplier - players amount"
      );

      FirstDaySeed = configFile.Bind("First Day", "FirstDaySeed", 0, "Seed for the first day's weather");
      FirstDaySpecial = configFile.Bind("First Day", "FirstDaySpecial", true, "Enable special weather picking algorithm for the first day");
      FirstDayRandomSeed = configFile.Bind("First Day", "FirstDayRandomSeed", true, "Use random seed for the first day's weather");

      GenerateSpecialWeatherEntries = configFile.Bind(
        "Special Weather Configs",
        "GenerateSpecialWeatherEntries",
        false,
        "Generate special weather entries for all levels"
      );

      AlwaysUncertain = configFile.Bind("Special Modes", "AlwaysUncertain", false, "Always make weather uncertain");
      AlwaysUnknown = configFile.Bind("Special Modes", "AlwaysUnknown", false, "Always make weather unknown");
      AlwaysClear = configFile.Bind("Special Modes", "AlwaysClear", false, "Always make weather clear - good for testing");

      FoggyIgnoreLevels = new LevelListConfigHandler("", false);
      FoggyIgnoreLevels.CreateConfigEntry("Foggy patch", new ConfigDescription("Levels to ignore foggy weather on"));
    }
  }
}
