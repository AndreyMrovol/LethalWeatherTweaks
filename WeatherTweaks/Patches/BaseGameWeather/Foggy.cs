using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace WeatherTweaks
{
  partial class BasegameWeatherPatch
  {
    internal static void FogPatchInit()
    {
      try
      {
        Plugin.logger.LogWarning("Patching FoggyWeather");
        harmony.Patch(
          typeof(TimeOfDay).GetMethod(
            "SetWeatherBasedOnVariables",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
          ),
          postfix: new HarmonyMethod(typeof(BasegameWeatherPatch), "ChangeFog")
        );
      }
      catch
      {
        Plugin.logger.LogWarning("Failed to patch FoggyWeather");
      }

      // TODO replace this with proper [HarmonyPostfix] when v49 will be irrelevant
    }

    internal static void ChangeFog(TimeOfDay __instance, LocalVolumetricFog ___foggyWeather)
    {
      logger.LogInfo("Changing fog");

      if (___foggyWeather == null)
      {
        Plugin.logger.LogWarning("Failed to find LocalVolumetricFog \"Foggy\"");
        return;
      }

      WeatherType currentWeather = Variables.GetPlanetCurrentWeatherType(__instance.currentLevel);

      try
      {
        // change the position of the fog to be 128 units lower
        ___foggyWeather.transform.position += new Vector3(0, -128, 0);

        ___foggyWeather.parameters.albedo = new Color(0.25f, 0.35f, 0.55f, 1f);

        ___foggyWeather.parameters.meanFreePath *= 0.75f;
        ___foggyWeather.parameters.size.y += 800f;

        ___foggyWeather.parameters.size.x *= 5;
        ___foggyWeather.parameters.size.z *= 5;
      }
      catch (Exception e)
      {
        Plugin.logger.LogWarning("Failed to change fog");
      }
    }
  }
}
