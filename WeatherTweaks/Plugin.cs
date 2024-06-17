using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace WeatherTweaks
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  [BepInDependency("xxxstoner420bongmasterxxx.open_monitors", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInDependency("MrovLib", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.HardDependency)]
  public class Plugin : BaseUnityPlugin
  {
    internal static ManualLogSource logger;

    private void Awake()
    {
      logger = Logger;

      var harmony = new Harmony(PluginInfo.PLUGIN_GUID);

      harmony.PatchAll();

      NetworkedConfig.Init();
      ConfigManager.Init(Config);
      UncertainWeather.Init();

      GeneralImprovementsWeather.Init();

      if (Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader"))
      {
        Patches.LLL.Init();
      }

      if (Chainloader.PluginInfos.ContainsKey("com.zealsprince.malfunctions"))
      {
        Patches.Malfunctions.Init();
      }

      if (Chainloader.PluginInfos.ContainsKey("xxxstoner420bongmasterxxx.open_monitors"))
      {
        Patches.OpenMonitorsPatch.Init();
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

      WeatherRegistry.Settings.SelectWeathers = false;
      WeatherRegistry.Settings.ScreenMapColors.Add("+", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("/", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("?", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("[UNKNOWN]", new Color(0.29f, 0.29f, 0.29f));

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
  }
}
