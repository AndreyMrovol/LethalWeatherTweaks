using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace WeatherTweaks
{
  partial class BasegameWeatherPatch
  {
    internal static LocalVolumetricFog foggyObject = null;

    internal static void FogPatch(TimeOfDay __instance, LocalVolumetricFog ___foggyWeather)
    {
      logger.LogInfo("Changing fog");

      if (___foggyWeather == null)
      {
        Plugin.logger.LogWarning("Failed to find LocalVolumetricFog \"Foggy\"");
        return;
      }

      ChangeFog(___foggyWeather);
    }

    internal static void ChangeFog()
    {
      // Get fog and call ChangeFog

      if (ConfigManager.FoggyIgnoreLevels.Value.Contains(StartOfRound.Instance.currentLevel))
      {
        return;
      }

      LocalVolumetricFog Fog = Resources
        .FindObjectsOfTypeAll<LocalVolumetricFog>()
        .ToList()
        .Where(fog => fog.name == "Foggy")
        .ToList()
        .FirstOrDefault();

      if (Fog == null)
      {
        Plugin.logger.LogWarning("Failed to find LocalVolumetricFog \"Foggy\"");
        return;
      }

      ChangeFog(Fog);
    }

    internal static void ChangeFog(LocalVolumetricFog Fog)
    {
      Plugin.logger.LogInfo("ChangeFog called");

      try
      {
        Plugin.logger.LogWarning($"Fog null? : {foggyObject == null}");

        if (foggyObject != null)
        {
          Plugin.logger.LogInfo(
            $"FOG is already changed to: {Fog.name} {Fog.parameters.size} {Fog.transform.position} {Fog.parameters.albedo} {Fog.parameters.meanFreePath}"
          );
          throw new Exception("Fog has already been changed");
        }

        LocalVolumetricFog localFog = Fog;
        LocalVolumetricFogArtistParameters parameters = localFog.parameters;

        Plugin.logger.LogWarning($"FOG: {Fog.name} {parameters.size} {Fog.transform.position} {parameters.albedo} {parameters.meanFreePath}");

        // change the position of the fog to be 128 units higher
        Fog.transform.position = new Vector3(Fog.transform.position.x, Fog.transform.position.y + 128f, Fog.transform.position.z);

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

        Plugin.logger.LogWarning($"FOG: {Fog.name} {parameters.size} {Fog.transform.position} {parameters.albedo} {parameters.meanFreePath}");

        Fog.parameters = parameters;
        foggyObject = Fog;

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
