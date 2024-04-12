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

    internal readonly ConfigFile configFile;

    // i love creating config hell

    public static ConfigEntry<bool> MapScreenPatch { get; private set; }
    public static ConfigEntry<bool> SunAnimatorPatch { get; private set; }

    public static ConfigEntry<int> FirstDaySeed { get; private set; }

    public static ConfigEntry<bool> UncertainWeatherEnabled { get; private set; }

    public static ConfigEntry<bool> AlwaysUncertain { get; private set; }
    public static ConfigEntry<bool> AlwaysUnknown { get; private set; }
    public static ConfigEntry<bool> AlwaysClear { get; private set; }

    public static ConfigEntry<float> GameLengthMultiplier { get; private set; }
    public static ConfigEntry<float> GamePlayersMultiplier { get; private set; }

    public static ConfigEntry<float> MaxMultiplier { get; private set; }
    public static ConfigEntry<bool> ScaleDownClearWeather { get; private set; }

    // per-weather weights

    public static ConfigEntry<int> NoneToNoneWeight { get; private set; }
    public static ConfigEntry<int> NoneToRainyWeight { get; private set; }
    public static ConfigEntry<int> NoneToStormyWeight { get; private set; }
    public static ConfigEntry<int> NoneToFloodedWeight { get; private set; }
    public static ConfigEntry<int> NoneToFoggyWeight { get; private set; }
    public static ConfigEntry<int> NoneToEclipsedWeight { get; private set; }

    public static ConfigEntry<int> RainyToNoneWeight { get; private set; }
    public static ConfigEntry<int> RainyToRainyWeight { get; private set; }
    public static ConfigEntry<int> RainyToStormyWeight { get; private set; }
    public static ConfigEntry<int> RainyToFloodedWeight { get; private set; }
    public static ConfigEntry<int> RainyToFoggyWeight { get; private set; }
    public static ConfigEntry<int> RainyToEclipsedWeight { get; private set; }

    public static ConfigEntry<int> StormyToNoneWeight { get; private set; }
    public static ConfigEntry<int> StormyToRainyWeight { get; private set; }
    public static ConfigEntry<int> StormyToStormyWeight { get; private set; }
    public static ConfigEntry<int> StormyToFloodedWeight { get; private set; }
    public static ConfigEntry<int> StormyToFoggyWeight { get; private set; }
    public static ConfigEntry<int> StormyToEclipsedWeight { get; private set; }

    public static ConfigEntry<int> FloodedToNoneWeight { get; private set; }
    public static ConfigEntry<int> FloodedToRainyWeight { get; private set; }
    public static ConfigEntry<int> FloodedToStormyWeight { get; private set; }
    public static ConfigEntry<int> FloodedToFloodedWeight { get; private set; }
    public static ConfigEntry<int> FloodedToFoggyWeight { get; private set; }
    public static ConfigEntry<int> FloodedToEclipsedWeight { get; private set; }

    public static ConfigEntry<int> FoggyToNoneWeight { get; private set; }
    public static ConfigEntry<int> FoggyToRainyWeight { get; private set; }
    public static ConfigEntry<int> FoggyToStormyWeight { get; private set; }
    public static ConfigEntry<int> FoggyToFloodedWeight { get; private set; }
    public static ConfigEntry<int> FoggyToFoggyWeight { get; private set; }
    public static ConfigEntry<int> FoggyToEclipsedWeight { get; private set; }

    public static ConfigEntry<int> EclipsedToNoneWeight { get; private set; }
    public static ConfigEntry<int> EclipsedToRainyWeight { get; private set; }
    public static ConfigEntry<int> EclipsedToStormyWeight { get; private set; }
    public static ConfigEntry<int> EclipsedToFloodedWeight { get; private set; }
    public static ConfigEntry<int> EclipsedToFoggyWeight { get; private set; }
    public static ConfigEntry<int> EclipsedToEclipsedWeight { get; private set; }

    // weight sums for easy calculations

    public static int NoneWeightSum { get; private set; }
    public static int RainyWeightSum { get; private set; }
    public static int StormyWeightSum { get; private set; }
    public static int FloodedWeightSum { get; private set; }
    public static int FoggyWeightSum { get; private set; }
    public static int EclipsedWeightSum { get; private set; }

    // dictionaries for easier access

    public static Dictionary<LevelWeatherType, int> NoneWeights { get; private set; }
    public static Dictionary<LevelWeatherType, int> RainyWeights { get; private set; }
    public static Dictionary<LevelWeatherType, int> StormyWeights { get; private set; }
    public static Dictionary<LevelWeatherType, int> FloodedWeights { get; private set; }
    public static Dictionary<LevelWeatherType, int> FoggyWeights { get; private set; }
    public static Dictionary<LevelWeatherType, int> EclipsedWeights { get; private set; }

    public static Dictionary<LevelWeatherType, Dictionary<LevelWeatherType, int>> Weights { get; private set; }

    private ConfigManager(ConfigFile config)
    {
      // god forgive me for this
      configFile = config;

      // create config entries

      MapScreenPatch = configFile.Bind("0> General", "MapScreenPatch", true, "Enable map screen patch (weather in top row)");
      SunAnimatorPatch = configFile.Bind("0> General", "SunAnimatorPatch", true, "Enable sun animator patch (sun animators for weather)");

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

      AlwaysUncertain = configFile.Bind("4> Special modes", "AlwaysUncertain", false, "Always make weather uncertain");
      AlwaysUnknown = configFile.Bind("4> Special modes", "AlwaysUnknown", false, "Always make weather unknown");
      AlwaysClear = configFile.Bind("4> Special modes", "AlwaysClear", false, "Always make weather clear - good for testing");

      NoneToNoneWeight = configFile.Bind("Weights > Clear", "NoneToNoneWeight", 80, "Weight for changing from none to none");
      NoneToRainyWeight = configFile.Bind("Weights > Clear", "NoneToRainyWeight", 50, "Weight for changing from none to rainy");
      NoneToStormyWeight = configFile.Bind("Weights > Clear", "NoneToStormyWeight", 35, "Weight for changing from none to stormy");
      NoneToFloodedWeight = configFile.Bind("Weights > Clear", "NoneToFloodedWeight", 10, "Weight for changing from none to flooded");
      NoneToFoggyWeight = configFile.Bind("Weights > Clear", "NoneToFoggyWeight", 20, "Weight for changing from none to foggy");
      NoneToEclipsedWeight = configFile.Bind("Weights > Clear", "NoneToEclipsedWeight", 5, "Weight for changing from none to eclipsed");

      RainyToNoneWeight = configFile.Bind("Weights > Rainy", "RainyToNoneWeight", 50, "Weight for changing from rainy to none");
      RainyToRainyWeight = configFile.Bind("Weights > Rainy", "RainyToRainyWeight", 30, "Weight for changing from rainy to rainy");
      RainyToStormyWeight = configFile.Bind("Weights > Rainy", "RainyToStormyWeight", 20, "Weight for changing from rainy to stormy");
      RainyToFloodedWeight = configFile.Bind("Weights > Rainy", "RainyToFloodedWeight", 15, "Weight for changing from rainy to flooded");
      RainyToFoggyWeight = configFile.Bind("Weights > Rainy", "RainyToFoggyWeight", 25, "Weight for changing from rainy to foggy");
      RainyToEclipsedWeight = configFile.Bind("Weights > Rainy", "RainyToEclipsedWeight", 10, "Weight for changing from rainy to eclipsed");

      StormyToNoneWeight = configFile.Bind("Weights > Stormy", "StormyToNoneWeight", 80, "Weight for changing from stormy to none");
      StormyToRainyWeight = configFile.Bind("Weights > Stormy", "StormyToRainyWeight", 55, "Weight for changing from stormy to rainy");
      StormyToStormyWeight = configFile.Bind("Weights > Stormy", "StormyToStormyWeight", 5, "Weight for changing from stormy to stormy");
      StormyToFloodedWeight = configFile.Bind("Weights > Stormy", "StormyToFloodedWeight", 60, "Weight for changing from stormy to flooded");
      StormyToFoggyWeight = configFile.Bind("Weights > Stormy", "StormyToFoggyWeight", 10, "Weight for changing from stormy to foggy");
      StormyToEclipsedWeight = configFile.Bind("Weights > Stormy", "StormyToEclipsedWeight", 40, "Weight for changing from stormy to eclipsed");

      FloodedToNoneWeight = configFile.Bind("Weights > Flooded", "FloodedToNoneWeight", 80, "Weight for changing from flooded to none");
      FloodedToRainyWeight = configFile.Bind("Weights > Flooded", "FloodedToRainyWeight", 30, "Weight for changing from flooded to rainy");
      FloodedToStormyWeight = configFile.Bind("Weights > Flooded", "FloodedToStormyWeight", 25, "Weight for changing from flooded to stormy");
      FloodedToFloodedWeight = configFile.Bind("Weights > Flooded", "FloodedToFloodedWeight", 5, "Weight for changing from flooded to flooded");
      FloodedToFoggyWeight = configFile.Bind("Weights > Flooded", "FloodedToFoggyWeight", 30, "Weight for changing from flooded to foggy");
      FloodedToEclipsedWeight = configFile.Bind(
        "Weights > Flooded",
        "FloodedToEclipsedWeight",
        20,
        "Weight for changing from flooded to eclipsed"
      );

      FoggyToNoneWeight = configFile.Bind("Weights > Foggy", "FoggyToNoneWeight", 100, "Weight for changing from foggy to none");
      FoggyToRainyWeight = configFile.Bind("Weights > Foggy", "FoggyToRainyWeight", 30, "Weight for changing from foggy to rainy");
      FoggyToStormyWeight = configFile.Bind("Weights > Foggy", "FoggyToStormyWeight", 25, "Weight for changing from foggy to stormy");
      FoggyToFloodedWeight = configFile.Bind("Weights > Foggy", "FoggyToFloodedWeight", 5, "Weight for changing from foggy to flooded");
      FoggyToFoggyWeight = configFile.Bind("Weights > Foggy", "FoggyToFoggyWeight", 15, "Weight for changing from foggy to foggy");
      FoggyToEclipsedWeight = configFile.Bind("Weights > Foggy", "FoggyToEclipsedWeight", 10, "Weight for changing from foggy to eclipsed");

      EclipsedToNoneWeight = configFile.Bind("Weights > Eclipsed", "EclipsedToNoneWeight", 150, "Weight for changing from eclipsed to none");
      EclipsedToRainyWeight = configFile.Bind("Weights > Eclipsed", "EclipsedToRainyWeight", 20, "Weight for changing from eclipsed to rainy");
      EclipsedToStormyWeight = configFile.Bind("Weights > Eclipsed", "EclipsedToStormyWeight", 8, "Weight for changing from eclipsed to stormy");
      EclipsedToFloodedWeight = configFile.Bind(
        "Weights > Eclipsed",
        "EclipsedToFloodedWeight",
        10,
        "Weight for changing from eclipsed to flooded"
      );
      EclipsedToFoggyWeight = configFile.Bind("Weights > Eclipsed", "EclipsedToFoggyWeight", 30, "Weight for changing from eclipsed to foggy");
      EclipsedToEclipsedWeight = configFile.Bind(
        "Weights > Eclipsed",
        "EclipsedToEclipsedWeight",
        5,
        "Weight for changing from eclipsed to eclipsed"
      );

      // calculate sums

      NoneWeightSum =
        NoneToNoneWeight.Value
        + NoneToRainyWeight.Value
        + NoneToStormyWeight.Value
        + NoneToFloodedWeight.Value
        + NoneToFoggyWeight.Value
        + NoneToEclipsedWeight.Value;
      RainyWeightSum =
        RainyToNoneWeight.Value
        + RainyToRainyWeight.Value
        + RainyToStormyWeight.Value
        + RainyToFloodedWeight.Value
        + RainyToFoggyWeight.Value
        + RainyToEclipsedWeight.Value;
      StormyWeightSum =
        StormyToNoneWeight.Value
        + StormyToRainyWeight.Value
        + StormyToStormyWeight.Value
        + StormyToFloodedWeight.Value
        + StormyToFoggyWeight.Value
        + StormyToEclipsedWeight.Value;
      FloodedWeightSum =
        FloodedToNoneWeight.Value
        + FloodedToRainyWeight.Value
        + FloodedToStormyWeight.Value
        + FloodedToFloodedWeight.Value
        + FloodedToFoggyWeight.Value
        + FloodedToEclipsedWeight.Value;
      FoggyWeightSum =
        FoggyToNoneWeight.Value
        + FoggyToRainyWeight.Value
        + FoggyToStormyWeight.Value
        + FoggyToFloodedWeight.Value
        + FoggyToFoggyWeight.Value
        + FoggyToEclipsedWeight.Value;
      EclipsedWeightSum =
        EclipsedToNoneWeight.Value
        + EclipsedToRainyWeight.Value
        + EclipsedToStormyWeight.Value
        + EclipsedToFloodedWeight.Value
        + EclipsedToFoggyWeight.Value
        + EclipsedToEclipsedWeight.Value;

      // create dictionaries

      NoneWeights = new Dictionary<LevelWeatherType, int>
      {
        { LevelWeatherType.None, NoneToNoneWeight.Value },
        { LevelWeatherType.Rainy, NoneToRainyWeight.Value },
        { LevelWeatherType.Stormy, NoneToStormyWeight.Value },
        { LevelWeatherType.Flooded, NoneToFloodedWeight.Value },
        { LevelWeatherType.Foggy, NoneToFoggyWeight.Value },
        { LevelWeatherType.Eclipsed, NoneToEclipsedWeight.Value }
      };

      RainyWeights = new Dictionary<LevelWeatherType, int>
      {
        { LevelWeatherType.None, RainyToNoneWeight.Value },
        { LevelWeatherType.Rainy, RainyToRainyWeight.Value },
        { LevelWeatherType.Stormy, RainyToStormyWeight.Value },
        { LevelWeatherType.Flooded, RainyToFloodedWeight.Value },
        { LevelWeatherType.Foggy, RainyToFoggyWeight.Value },
        { LevelWeatherType.Eclipsed, RainyToEclipsedWeight.Value }
      };

      StormyWeights = new Dictionary<LevelWeatherType, int>
      {
        { LevelWeatherType.None, StormyToNoneWeight.Value },
        { LevelWeatherType.Rainy, StormyToRainyWeight.Value },
        { LevelWeatherType.Stormy, StormyToStormyWeight.Value },
        { LevelWeatherType.Flooded, StormyToFloodedWeight.Value },
        { LevelWeatherType.Foggy, StormyToFoggyWeight.Value },
        { LevelWeatherType.Eclipsed, StormyToEclipsedWeight.Value }
      };

      FloodedWeights = new Dictionary<LevelWeatherType, int>
      {
        { LevelWeatherType.None, FloodedToNoneWeight.Value },
        { LevelWeatherType.Rainy, FloodedToRainyWeight.Value },
        { LevelWeatherType.Stormy, FloodedToStormyWeight.Value },
        { LevelWeatherType.Flooded, FloodedToFloodedWeight.Value },
        { LevelWeatherType.Foggy, FloodedToFoggyWeight.Value },
        { LevelWeatherType.Eclipsed, FloodedToEclipsedWeight.Value }
      };

      FoggyWeights = new Dictionary<LevelWeatherType, int>
      {
        { LevelWeatherType.None, FoggyToNoneWeight.Value },
        { LevelWeatherType.Rainy, FoggyToRainyWeight.Value },
        { LevelWeatherType.Stormy, FoggyToStormyWeight.Value },
        { LevelWeatherType.Flooded, FoggyToFloodedWeight.Value },
        { LevelWeatherType.Foggy, FoggyToFoggyWeight.Value },
        { LevelWeatherType.Eclipsed, FoggyToEclipsedWeight.Value }
      };

      EclipsedWeights = new Dictionary<LevelWeatherType, int>
      {
        { LevelWeatherType.None, EclipsedToNoneWeight.Value },
        { LevelWeatherType.Rainy, EclipsedToRainyWeight.Value },
        { LevelWeatherType.Stormy, EclipsedToStormyWeight.Value },
        { LevelWeatherType.Flooded, EclipsedToFloodedWeight.Value },
        { LevelWeatherType.Foggy, EclipsedToFoggyWeight.Value },
        { LevelWeatherType.Eclipsed, EclipsedToEclipsedWeight.Value }
      };

      Weights = new Dictionary<LevelWeatherType, Dictionary<LevelWeatherType, int>>
      {
        { LevelWeatherType.None, NoneWeights },
        { LevelWeatherType.Rainy, RainyWeights },
        { LevelWeatherType.Stormy, StormyWeights },
        { LevelWeatherType.Flooded, FloodedWeights },
        { LevelWeatherType.Foggy, FoggyWeights },
        { LevelWeatherType.Eclipsed, EclipsedWeights }
      };
    }
  }
}
