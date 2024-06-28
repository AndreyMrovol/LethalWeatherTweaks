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
    internal static bool IsLLLPresent = false;

    internal static GeneralImprovementsWeather GeneralImprovements;

    private void Awake()
    {
      logger = Logger;

      var harmony = new Harmony(PluginInfo.PLUGIN_GUID);

      harmony.PatchAll();

      NetworkedConfig.Init();
      ConfigManager.Init(Config);
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

  [HarmonyPatch(typeof(Terminal), "Start")]
  public static class TerminalPatch
  {
    [HarmonyPostfix]
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
