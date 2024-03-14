using System;
using System.Collections;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(FloodWeather))]
  partial class BasegameWeatherPatch
  {
    internal static void CurrentWeatherVariablePatch(ILContext il, LevelWeatherType weatherType, string wherefrom, bool variable2 = false)
    {
      var cursor = new ILCursor(il);

      if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<TimeOfDay>("currentWeatherVariable")))
      {
        Plugin.logger.LogError($"Failed IL hook for {wherefrom}");
        return;
      }
      else
      {
        Plugin.logger.LogInfo($"IL hook for {wherefrom}");
      }

      // Replace the field load instruction with a call to 'Variables.GetLevelWeatherVariables'
      cursor.Remove(); // Remove the 'ldfld' instruction
      cursor.EmitDelegate<Func<float>>(() =>
      {
        if (variable2)
        {
          return Variables.GetLevelWeatherVariables(weatherType).weatherVariable2;
        }

        return Variables.GetLevelWeatherVariables(weatherType).weatherVariable;
      });
      cursor.Emit(OpCodes.Stloc_0); // Store the result in a local variable

      // Emit instructions to load the value stored in the local variable onto the evaluation stack
      cursor.Emit(OpCodes.Ldloc_0);
    }
  }
}
