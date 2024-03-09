using System;
using System.Collections;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StartOfRound))]
  internal class OpeningDoorsSequencePatch
  {
    // // to be completely honest, I have no idea what I'm doing
    // // https://github.com/SylviBlossom/LC-SimpleWeatherDisplay/blob/2d252b92dcd4d8ef259b8072d9339ff5ccdc4d0b/src/Plugin.cs#L127-L153
    // // this is just this, but repurposed

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
    internal static void StartOfRound_openingDoorsSequence(ILContext il)
    {
      var cursor = new ILCursor(il);

      if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<SelectableLevel>("currentWeather")))
      {
        Plugin.logger.LogError("Failed IL weather hook for StartOfRound.openingDoorsSequence");
        return;
      }
      else
      {
        Plugin.logger.LogInfo("IL weather hook for StartOfRound.openingDoorsSequence");
      }
      cursor.EmitDelegate<Action>(SetWeatherEffects);

      if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<SelectableLevel>("LevelDescription")))
      {
        Plugin.logger.LogError("Failed IL hook for StartOfRound.openingDoorsSequence");
        return;
      }

      cursor.Emit(OpCodes.Ldloc_1);
      cursor.EmitDelegate<Func<string, StartOfRound, string>>(
        (desc, self) =>
        {
          if (self.currentLevel.currentWeather == LevelWeatherType.None)
          {
            return desc;
          }

          var weatherName =
            self.currentLevel.currentWeather != LevelWeatherType.None ? Variables.GetPlanetCurrentWeather(self.currentLevel, false) : "Clear";
          var weatherLine = $"WEATHER: {weatherName}";

          return $"{weatherLine}\n{desc}";
        }
      );
    }

    internal static void SetWeatherEffects()
    {
      GameInteraction.SetWeatherEffects(TimeOfDay.Instance, Variables.CurrentWeathers[TimeOfDay.Instance.currentLevel].Effects);
      GameInteraction.SetWeatherEffects(
        TimeOfDay.Instance,
        Variables.GetFullWeatherType(Variables.CurrentWeathers[TimeOfDay.Instance.currentLevel]).Effects
      );
    }
  }
}
