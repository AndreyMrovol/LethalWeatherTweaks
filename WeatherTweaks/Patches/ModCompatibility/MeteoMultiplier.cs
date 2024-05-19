using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using Newtonsoft.Json;
using WeatherTweaks.Definitions;
using static WeatherTweaks.Definitions.Types;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks.Patches
{
  [JsonObject(MemberSerialization.OptIn)]
  internal class MeteoMultipliersData
  {
    [JsonProperty]
    public LevelWeatherType weatherType;

    [JsonProperty]
    public float multiplier;

    [JsonProperty]
    public float spawnMultiplier;

    //create constructor
    public MeteoMultipliersData(LevelWeatherType weatherType, float multiplier, float spawnMultiplier)
    {
      this.weatherType = weatherType;
      this.multiplier = multiplier;
      this.spawnMultiplier = spawnMultiplier;
    }
  }

  internal class MeteoMultiplierPatches
  {
    internal static MeteoMultipliersData meteoMultipliersData;

    internal static Harmony mmHarmony = new("WeatherTweaks.MeteoMultiplier");
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks MM");

    public static void Init()
    {
      logger.LogInfo("Initializing MeteoMultiplierPatches");

      System.Type roundManagerType = typeof(RoundManager);

      mmHarmony.Patch(
        roundManagerType.GetMethod("GenerateNewLevelClientRpc", BindingFlags.Public | BindingFlags.Instance),
        prefix: new HarmonyMethod(
          typeof(MeteoMultiplierPatches).GetMethod(nameof(GenerateNewLevelClientRpcPatch), BindingFlags.NonPublic | BindingFlags.Static)
        )
      );

      logger.LogWarning("Patching RoundManager.GenerateNewLevelClientRpc");

      mmHarmony.Patch(
        roundManagerType.GetMethod("SpawnScrapInLevel", BindingFlags.Public | BindingFlags.Instance),
        prefix: new HarmonyMethod(
          typeof(MeteoMultiplierPatches).GetMethod(nameof(MeteoMultiplierPatch), BindingFlags.NonPublic | BindingFlags.Static)
        )
      );
      logger.LogWarning("Patching RoundManager.SpawnScrapInLevel");

      mmHarmony.Unpatch(
        roundManagerType.GetMethod("SpawnScrapInLevel", BindingFlags.Public | BindingFlags.Instance),
        HarmonyPatchType.Prefix,
        "com.github.fredolx.meteomultiplier"
      );
      logger.LogWarning("Unpatching MeteoMultiplier's RoundManager.SpawnScrapInLevel");
    }

    internal static float GetMultiplier(LevelWeatherType weatherType, bool spawnMultiplier)
    {
      if (spawnMultiplier)
      {
        if (!MeteoMultiplier.Plugin.SpawnMultipliers.ContainsKey(weatherType))
        {
          logger.LogWarning($"No spawn multiplier found for {weatherType}");
          return MeteoMultiplier.Plugin.SpawnMultipliers[LevelWeatherType.None].Value;
        }

        return MeteoMultiplier.Plugin.SpawnMultipliers[weatherType].Value;
      }
      else
      {
        if (!MeteoMultiplier.Plugin.Multipliers.ContainsKey(weatherType))
        {
          logger.LogWarning($"No multiplier found for {weatherType}");
          return MeteoMultiplier.Plugin.Multipliers[LevelWeatherType.None].Value;
        }

        return MeteoMultiplier.Plugin.Multipliers[weatherType].Value;
      }
    }

    internal static MeteoMultipliersData GetMeteoMultiplierData(LevelWeatherType weatherType)
    {
      return new MeteoMultipliersData(weatherType, GetMultiplier(weatherType, false), GetMultiplier(weatherType, true));
    }

    internal static void GenerateNewLevelClientRpcPatch()
    {
      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }

      WeatherType currentWeather = Variables.GetPlanetCurrentWeatherType(StartOfRound.Instance.currentLevel);

      switch (currentWeather.Type)
      {
        case CustomWeatherType.Normal:
          SetMeteoMultiplierData(GetMeteoMultiplierData(currentWeather.Weather.VanillaWeatherType));
          break;
        case CustomWeatherType.Combined:

          Definitions.Types.CombinedWeatherType currentCombinedWeather = Variables.CombinedWeatherTypes.First(weather =>
            weather.Name == currentWeather.Name
          );

          MeteoMultipliersData combinedData = new(currentWeather.Weather.VanillaWeatherType, 0, 0);

          foreach (Weather weather in currentCombinedWeather.Weathers)
          {
            MeteoMultipliersData data = GetMeteoMultiplierData(weather.VanillaWeatherType);
            combinedData.multiplier += data.multiplier * 0.7f;
            combinedData.spawnMultiplier += data.spawnMultiplier * 0.7f;
          }

          SetMeteoMultiplierData(combinedData);
          break;
        case CustomWeatherType.Progressing:
          MeteoMultipliersData progressingData = new(currentWeather.weatherType, 0, 0);

          float sumMultiplier = 0;
          float sumSpawnMultiplier = 0;

          float sumChances = 0;

          foreach (
            ProgressingWeatherEntry entry in Variables
              .ProgressingWeatherTypes.First(weather => weather.Name == currentWeather.Name)
              .WeatherEntries
          )
          {
            MeteoMultipliersData data = GetMeteoMultiplierData(entry.GetWeatherType().weatherType);
            sumMultiplier += data.multiplier * entry.Chance;
            sumSpawnMultiplier += data.spawnMultiplier * entry.Chance;
            sumChances += entry.Chance;
          }

          progressingData.multiplier = sumMultiplier / sumChances;
          progressingData.spawnMultiplier = sumSpawnMultiplier / sumChances;

          SetMeteoMultiplierData(progressingData);
          break;
        default:
          logger.LogError($"Unknown weather type {currentWeather.Type}");
          break;
      }

      logger.LogInfo(
        $"Setting MeteoMultiplierData for {currentWeather.Name}: {meteoMultipliersData.multiplier}, {meteoMultipliersData.spawnMultiplier}"
      );
    }

    internal static void MeteoMultiplierPatch(RoundManager __instance)
    {
      logger.LogWarning($"Checking MeteoMultiplierData for {meteoMultipliersData.weatherType}");

      if (MeteoMultiplier.Plugin.MultipliersEnabled.Value)
      {
        logger.LogWarning($"Applying MeteoMultiplierData: multiplier {meteoMultipliersData.multiplier}");
        __instance.scrapValueMultiplier = meteoMultipliersData.multiplier;
      }

      if (MeteoMultiplier.Plugin.SpawnMultipliersEnabled.Value)
      {
        logger.LogWarning($"Applying MeteoMultiplierData: spawnMultiplier {meteoMultipliersData.spawnMultiplier}");
        __instance.scrapAmountMultiplier = meteoMultipliersData.spawnMultiplier;
      }
    }

    internal static void SetMeteoMultiplierData(MeteoMultipliersData data)
    {
      logger.LogInfo(
        $"Setting MeteoMultiplierData for {data.weatherType} with multiplier {data.multiplier} and spawnMultiplier {data.spawnMultiplier}"
      );

      meteoMultipliersData = data;
    }
  }
}
