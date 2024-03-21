using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace WeatherTweaks
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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

      if (Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades"))
      {
        Patches.LateGameUpgrades.Init();
      }

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
  }
}
