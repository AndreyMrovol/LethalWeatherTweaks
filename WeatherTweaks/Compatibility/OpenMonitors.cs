using HarmonyLib;

namespace WeatherTweaks.Compatibility
{
  public class OpenMonitorsCompat
  {
    private static string guid = "xxxstoner420bongmasterxxx.open_monitors";

    public static void Init()
    {
      Plugin.logger.LogInfo("OpenMonitorsPatch:");
      Harmony harmony = new("WeatherTweaks.OpenMonitors");

      // unpatch the methods that we're patching ourselves
      harmony.Unpatch(
        typeof(StartOfRound).GetMethod(
          "SetMapScreenInfoToCurrentLevel",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
        ),
        HarmonyPatchType.All,
        guid
      );
      Plugin.logger.LogInfo("Unpatched StartOfRound.SetMapScreenInfoToCurrentLevel");

      harmony.Unpatch(
        typeof(Terminal).GetMethod("TextPostProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
        HarmonyPatchType.All,
        guid
      );
      Plugin.logger.LogInfo("Unpatched Terminal.TextPostProcess");
    }
  }
}
