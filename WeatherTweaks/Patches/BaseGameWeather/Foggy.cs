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
        LocalVolumetricFogArtistParameters parameters = ___foggyWeather.parameters;

        // change the position of the fog to be 128 units lower
        ___foggyWeather.transform.position = new Vector3(
          ___foggyWeather.transform.position.x,
          ___foggyWeather.transform.position.y + 128f,
          ___foggyWeather.transform.position.z
        );

        parameters.albedo = new Color(0.25f, 0.35f, 0.55f, 1f);

        // parameters.meanFreePath *= 1.45f;
        parameters.meanFreePath = 11f;
        parameters.falloffMode = LocalVolumetricFogFalloffMode.Linear;

        parameters.distanceFadeEnd = 200;
        parameters.distanceFadeStart = 0;
        parameters.blendingMode = LocalVolumetricFogBlendingMode.Additive;

        parameters.size.y += 256f;

        parameters.size.x *= 5;
        parameters.size.z *= 5;

        ___foggyWeather.parameters = parameters;

        // logger.LogWarning(
        //   $"Changing freeMeanPath from {___foggyWeather.parameters.meanFreePath} to {___foggyWeather.parameters.meanFreePath * 1.5f}"
        // );
      }
      catch (Exception e)
      {
        Plugin.logger.LogWarning("Failed to change fog");
      }
    }
  }
}
