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

    private readonly ConfigFile configFile;

    // i love creating config hell


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

      NoneToNoneWeight = configFile.Bind("1> No weather", "NoneToNoneWeight", 30, "Weight for changing from none to none");
      NoneToRainyWeight = configFile.Bind("1> No weather", "NoneToRainyWeight", 40, "Weight for changing from none to rainy");
      NoneToStormyWeight = configFile.Bind("1> No weather", "NoneToStormyWeight", 30, "Weight for changing from none to stormy");
      NoneToFloodedWeight = configFile.Bind("1> No weather", "NoneToFloodedWeight", 15, "Weight for changing from none to flooded");
      NoneToFoggyWeight = configFile.Bind("1> No weather", "NoneToFoggyWeight", 20, "Weight for changing from none to foggy");
      NoneToEclipsedWeight = configFile.Bind("1> No weather", "NoneToEclipsedWeight", 10, "Weight for changing from none to eclipsed");

      RainyToNoneWeight = configFile.Bind("2> Rainy", "RainyToNoneWeight", 30, "Weight for changing from rainy to none");
      RainyToRainyWeight = configFile.Bind("2> Rainy", "RainyToRainyWeight", 15, "Weight for changing from rainy to rainy");
      RainyToStormyWeight = configFile.Bind("2> Rainy", "RainyToStormyWeight", 20, "Weight for changing from rainy to stormy");
      RainyToFloodedWeight = configFile.Bind("2> Rainy", "RainyToFloodedWeight", 40, "Weight for changing from rainy to flooded");
      RainyToFoggyWeight = configFile.Bind("2> Rainy", "RainyToFoggyWeight", 25, "Weight for changing from rainy to foggy");
      RainyToEclipsedWeight = configFile.Bind("2> Rainy", "RainyToEclipsedWeight", 20, "Weight for changing from rainy to eclipsed");

      StormyToNoneWeight = configFile.Bind("3> Stormy", "StormyToNoneWeight", 25, "Weight for changing from stormy to none");
      StormyToRainyWeight = configFile.Bind("3> Stormy", "StormyToRainyWeight", 40, "Weight for changing from stormy to rainy");
      StormyToStormyWeight = configFile.Bind("3> Stormy", "StormyToStormyWeight", 2, "Weight for changing from stormy to stormy");
      StormyToFloodedWeight = configFile.Bind("3> Stormy", "StormyToFloodedWeight", 20, "Weight for changing from stormy to flooded");
      StormyToFoggyWeight = configFile.Bind("3> Stormy", "StormyToFoggyWeight", 15, "Weight for changing from stormy to foggy");
      StormyToEclipsedWeight = configFile.Bind("3> Stormy", "StormyToEclipsedWeight", 5, "Weight for changing from stormy to eclipsed");

      FloodedToNoneWeight = configFile.Bind("4> Flooded", "FloodedToNoneWeight", 25, "Weight for changing from flooded to none");
      FloodedToRainyWeight = configFile.Bind("4> Flooded", "FloodedToRainyWeight", 35, "Weight for changing from flooded to rainy");
      FloodedToStormyWeight = configFile.Bind("4> Flooded", "FloodedToStormyWeight", 20, "Weight for changing from flooded to stormy");
      FloodedToFloodedWeight = configFile.Bind("4> Flooded", "FloodedToFloodedWeight", 1, "Weight for changing from flooded to flooded");
      FloodedToFoggyWeight = configFile.Bind("4> Flooded", "FloodedToFoggyWeight", 20, "Weight for changing from flooded to foggy");
      FloodedToEclipsedWeight = configFile.Bind("4> Flooded", "FloodedToEclipsedWeight", 15, "Weight for changing from flooded to eclipsed");

      FoggyToNoneWeight = configFile.Bind("5> Foggy", "FoggyToNoneWeight", 30, "Weight for changing from foggy to none");
      FoggyToRainyWeight = configFile.Bind("5> Foggy", "FoggyToRainyWeight", 25, "Weight for changing from foggy to rainy");
      FoggyToStormyWeight = configFile.Bind("5> Foggy", "FoggyToStormyWeight", 10, "Weight for changing from foggy to stormy");
      FoggyToFloodedWeight = configFile.Bind("5> Foggy", "FoggyToFloodedWeight", 20, "Weight for changing from foggy to flooded");
      FoggyToFoggyWeight = configFile.Bind("5> Foggy", "FoggyToFoggyWeight", 5, "Weight for changing from foggy to foggy");
      FoggyToEclipsedWeight = configFile.Bind("5> Foggy", "FoggyToEclipsedWeight", 15, "Weight for changing from foggy to eclipsed");

      EclipsedToNoneWeight = configFile.Bind("6> Eclipsed", "EclipsedToNoneWeight", 30, "Weight for changing from eclipsed to none");
      EclipsedToRainyWeight = configFile.Bind("6> Eclipsed", "EclipsedToRainyWeight", 15, "Weight for changing from eclipsed to rainy");
      EclipsedToStormyWeight = configFile.Bind("6> Eclipsed", "EclipsedToStormyWeight", 25, "Weight for changing from eclipsed to stormy");
      EclipsedToFloodedWeight = configFile.Bind("6> Eclipsed", "EclipsedToFloodedWeight", 25, "Weight for changing from eclipsed to flooded");
      EclipsedToFoggyWeight = configFile.Bind("6> Eclipsed", "EclipsedToFoggyWeight", 20, "Weight for changing from eclipsed to foggy");
      EclipsedToEclipsedWeight = configFile.Bind("6> Eclipsed", "EclipsedToEclipsedWeight", 1, "Weight for changing from eclipsed to eclipsed");

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
