using System.Collections.Generic;
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

      LogWeatherSelection = configFile.Bind("0> Debug", "LogWeatherSelection", true, "Log weather selection");
      LogWeatherVariables = configFile.Bind("0> Debug", "LogWeatherVariables", true, "Log resolving weather variables");
      LogLogs = configFile.Bind("0> Debug", "Logs", true, "Log logging logs");

      UncertainWeatherEnabled = configFile.Bind("1> Uncertain weather", "UncertainWeatherEnabled", true, "Enable uncertain weather mechanic");

      MaxMultiplier = configFile.Bind("2> Multipliers", "MaxMultiplier", 0.8f, "Maximum difficulty multiplier (between 0 and 1)");
      ScaleDownClearWeather = configFile.Bind(
        "2> Multipliers",
        "ScaleDownClearWeather",
        true,
        "Scale down clear weather's weight based on planet's available random weathers to match % chance "
      );

      GameLengthMultiplier = configFile.Bind(
        "2a> Difficulty multipliers",
        "GameLengthMultiplier",
        0.05f,
        "Difficulty multiplier - game length (quotas done)"
      );
      GamePlayersMultiplier = configFile.Bind(
        "2a> Difficulty multipliers",
        "GamePlayersMultiplier",
        0.01f,
        "Difficulty multiplier - players amount"
      );

      FirstDaySeed = configFile.Bind("3> First day", "FirstDaySeed", 0, "Seed for the first day's weather");
      FirstDaySpecial = configFile.Bind("3> First day", "FirstDaySpecial", true, "Enable special weather picking algorithm for the first day");
      FirstDayRandomSeed = configFile.Bind("3> First day", "FirstDayRandomSeed", true, "Use random seed for the first day's weather");

      AlwaysUncertain = configFile.Bind("4> Special modes", "AlwaysUncertain", false, "Always make weather uncertain");
      AlwaysUnknown = configFile.Bind("4> Special modes", "AlwaysUnknown", false, "Always make weather unknown");
      AlwaysClear = configFile.Bind("4> Special modes", "AlwaysClear", false, "Always make weather clear - good for testing");

      FoggyIgnoreLevels = new LevelListConfigHandler(
        "",
        "FoggyIgnoreLevels",
        new ConfigDescription("Levels to ignore applying foggy weather patch on")
      );
    }
  }
}
