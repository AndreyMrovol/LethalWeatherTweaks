using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
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

    private void Awake()
    {
      logger = Logger;
      ConfigManager.Init(Config);

      var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
      harmony.PatchAll();

      NetworkedConfig.Init();
      UncertainWeather.Init();

      new CombinedEclipsedFlooded();
      new CombinedFoggyRainy();
      new CombinedStormyFlooded();
      new CombinedStormyRainy();
      new CombinedEclipsedRainy();
      new CombinedMadness();
      new CombinedFoggyFlooded();
      new CombinedFoggyEclipsed();
      new CombinedStormyRainyEclipsed();
      new CombinedStormyRainyFlooded();

      new ProgressingNoneFoggy();
      new ProgressingNoneStormy();
      new ProgressingEclipsedFoggy();
      new ProgressingFoggyNone();
      new ProgressingHiddenEclipsed();
      new ProgressingStormyRainy();
      new ProgressingRainyEclipsed();
      new ProgressingMadness();

      // new SuperFoggy();

      // new ProgressingTesting();

      WeatherRegistry.Settings.SelectWeathers = false;

      WeatherRegistry.EventManager.DisableAllWeathers.AddListener(() => DisableAllWeathers.DisableAllWeather());
      WeatherRegistry.EventManager.ShipLanding.AddListener((data) => OpeningDoorsSequencePatch.SetWeatherEffects(data.level, data.weather));

      WeatherRegistry.EventManager.SetupFinished.AddListener(() => TerminalStartPatch.Start());
      WeatherRegistry.EventManager.SetupFinished.AddListener(() => Variables.PopulateWeathers());

      MrovLib.EventManager.TerminalStart.AddListener((terminal) => TerminalPatch.Postfix());
      MrovLib.EventManager.LobbyDisabled.AddListener((startofround) => Reset.ResetThings());

      if (Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader"))
      {
        Patches.LLL.Init();
      }

      var weatherMethod = typeof(StartOfRound).GetMethod("SetPlanetsWeather");
      harmony.Unpatch(weatherMethod, HarmonyPatchType.Postfix, "imabatby.lethallevelloader");

      GeneralImprovements = new GeneralImprovementsWeather("ShaosilGaming.GeneralImprovements");

      if (Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades"))
      {
        Patches.LateGameUpgrades.Init();
      }

      // if (Chainloader.PluginInfos.ContainsKey("com.github.fredolx.meteomultiplier"))
      // {
      //   Patches.MeteoMultiplierPatches.Init();
      // }

      if (Chainloader.PluginInfos.ContainsKey("xxxstoner420bongmasterxxx.open_monitors"))
      {
        Patches.OpenMonitorsPatch.Init();
      }

      // SunAnimator.Init();
      // BasegameWeatherPatch.FogPatchInit();

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
          Color = new(r: 0, g: 0.62f, b: 0.55f, a: 1),
          ScrapAmountMultiplier = 1.6f,
          ScrapValueMultiplier = 0.8f,
          DefaultWeatherToWeatherWeights = ["Eclipsed@200", "Stormy@80"],
          DefaultWeight = 10,
        };

      WeatherRegistry.WeatherManager.RegisterWeather(cloudyWeather);

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
