using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using WeatherRegistry;
using WeatherTweaks.Patches;

namespace WeatherTweaks
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  [BepInDependency("MrovLib", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("imabatby.lethallevelloader", BepInDependency.DependencyFlags.SoftDependency)]
  // [BepInDependency("ShaosilGaming.GeneralImprovements", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("com.malco.lethalcompany.moreshipupgrades", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("com.github.fredolx.meteomultiplier", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("xxxstoner420bongmasterxxx.open_monitors", BepInDependency.DependencyFlags.SoftDependency)]
  public class Plugin : BaseUnityPlugin
  {
    internal static ManualLogSource logger;
    internal static MrovLib.Logger DebugLogger = new(PluginInfo.PLUGIN_GUID);
    internal static bool IsLLLPresent = false;

    internal static GeneralImprovementsWeather GeneralImprovements;

    internal static Weather BlackoutWeather;

    private void Awake()
    {
      logger = Logger;
      ConfigManager.Init(Config);

      var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
      harmony.PatchAll();

      NetworkedConfig.Init();
      UncertainWeather.Init();

      WeatherRegistry.EventManager.DisableAllWeathers.AddListener(() => DisableAllWeathers.DisableAllWeather());
      WeatherRegistry.EventManager.SetupFinished.AddListener(() => TerminalStartPatch.Start());

      WeatherRegistry.EventManager.ShipLanding.AddListener(args => FoggyPatch.ToggleFogExclusionZones(args.level, false));
      WeatherRegistry.EventManager.DisableAllWeathers.AddListener(
        () => FoggyPatch.ToggleFogExclusionZones(StartOfRound.Instance.currentLevel, true)
      );

      if (WeatherRegistry.WeatherCalculation.WeatherSelectionAlgorithm != WeatherRegistry.WeatherCalculation.VanillaAlgorithm)
      {
        WeatherRegistry.WeatherCalculation.WeatherSelectionAlgorithm = WeatherCalculation.weatherTweaksWeatherAlgorithm;
      }

      MrovLib.EventManager.TerminalStart.AddListener((terminal) => TerminalPatch.Postfix());
      MrovLib.EventManager.LobbyDisabled.AddListener((startofround) => Reset.ResetThings());

      if (Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader"))
      {
        Patches.LLL.Init();
      }

      var weatherMethod = typeof(StartOfRound).GetMethod("SetPlanetsWeather");
      harmony.Unpatch(weatherMethod, HarmonyPatchType.Postfix, "imabatby.lethallevelloader");

      GeneralImprovements = new GeneralImprovementsWeather("ShaosilGaming.GeneralImprovements");

      // if (Chainloader.PluginInfos.ContainsKey("com.github.fredolx.meteomultiplier"))
      // {
      //   Patches.MeteoMultiplierPatches.Init();
      // }

      if (Chainloader.PluginInfos.ContainsKey("xxxstoner420bongmasterxxx.open_monitors"))
      {
        Patches.OpenMonitorsPatch.Init();
      }

      if (Chainloader.PluginInfos.ContainsKey("com.zealsprince.malfunctions"))
      {
        Patches.Malfunctions.Init();
      }

      if (Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
      {
        LobbyCompatibilityCompatibility.Init();
      }

      Weather cloudyWeather =
        new("Cloudy", new(null, null) { SunAnimatorBool = "overcast" })
        {
          Color = new(r: 0, g: 1f, b: 0.55f, a: 1),
          Config =
          {
            ScrapAmountMultiplier = new(1.6f),
            ScrapValueMultiplier = new(0.8f),
            WeatherToWeatherWeights = new(["Eclipsed@50", "Stormy@80"]),
            DefaultWeight = new(25),
          },
        };
      WeatherRegistry.WeatherManager.RegisterWeather(cloudyWeather);

      GameObject blackoutObject = GameObject.Instantiate(new GameObject() { name = "BlackoutWeather" });
      blackoutObject.hideFlags = HideFlags.HideAndDontSave;
      blackoutObject.AddComponent<Weathers.Blackout>();
      GameObject.DontDestroyOnLoad(blackoutObject);

      var BlackoutAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "blackout"));
      AnimationClip blackoutClip = BlackoutAssets.LoadAsset<AnimationClip>("BlackoutSunClip");

      Weather Blackout =
        new("Blackout", new(null, blackoutObject) { SunAnimatorBool = "eclipse", })
        {
          Color = new(r: 0.2f, g: 0.2f, b: 0.2f, a: 1),
          Config =
          {
            ScrapAmountMultiplier = new(0.65f),
            ScrapValueMultiplier = new(1.7f),
            DefaultWeight = new(25),
            LevelWeights = new("Rend@200; Dine@200; Titan@200"),
            WeatherToWeatherWeights = new("None@200; Cloudy@250")
          },
          AnimationClip = blackoutClip
        };
      WeatherRegistry.WeatherManager.RegisterWeather(Blackout);
      BlackoutWeather = Blackout;

      Init.InitMethod();

      logger.LogInfo(
        @"
                  .::.                  
                  :==:                  
         :-.      :==:      .-:         
        .-==-.    .::.    .-===.        
          .-=-  .:----:.  -==.          
              -==========-              
             ==============             
               .-==========- :-----     
         :-==-:. .=========- :-----     
       .========:   .-=====             
       ============-. :==-              
       -=============. .  -==.          
        :-==========:     .-==-.        
            ......          .-:         "
      );

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
  }

  public static class TerminalPatch
  {
    public static void Postfix()
    {
      if (Plugin.GeneralImprovements.IsModPresent)
      {
        Plugin.logger.LogInfo("GeneralImprovements is present");
        GeneralImprovementsWeather.Init();
      }
    }
  }
}
