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
    internal static void ChangeFog(float meanFreePath = 15f)
    {
      try
      {
        // get LocalVolumetricFog "Foggy" from all game objects
        LocalVolumetricFog Fog = GameObject.Find("Foggy").GetComponent<LocalVolumetricFog>();

        if (Fog == null)
        {
          Plugin.logger.LogError("Failed to find LocalVolumetricFog \"Foggy\"");
          return;
        }
        Fog.parameters.albedo = new Color(0.25f, 0.35f, 0.55f, 1f);
        Fog.parameters.meanFreePath = meanFreePath;
        Fog.parameters.size.y = 255f;
      }
      catch (Exception e)
      {
        Plugin.logger.LogError("Failed to change fog: " + e);
      }
    }
  }
}
