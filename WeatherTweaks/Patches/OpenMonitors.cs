using HarmonyLib;

namespace WeatherTweaks.Patches
{
  public class OpenMonitorsPatch
  {
    private static string guid = "xxxstoner420bongmasterxxx.open_monitors";

    public static void Init()
    {
      Plugin.logger.LogDebug("OpenMonitorsPatch Init");

      // var screenMethod = tyStartOfRound.SetMapScreenInfoToCurrentLevel;

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
      Plugin.logger.LogWarning("Unpatched StartOfRound.SetMapScreenInfoToCurrentLevel");

      harmony.Unpatch(
        typeof(Terminal).GetMethod("TextPostProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
        HarmonyPatchType.All,
        guid
      );
      Plugin.logger.LogWarning("Unpatched Terminal.TextPostProcess");
    }
  }
}
