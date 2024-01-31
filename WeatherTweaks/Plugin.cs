using BepInEx;
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

      // Plugin startup logic
      Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
  }
}
