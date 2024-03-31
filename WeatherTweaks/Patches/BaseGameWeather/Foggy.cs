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
    internal static void ChangeFog()
    {
      // get LocalVolumetricFog "Foggy" from all game objects
      LocalVolumetricFog Fog = GameObject.Find("Foggy").GetComponent<LocalVolumetricFog>();

      Fog.parameters.albedo = new Color(0.5f, 0.5f, 0.4f, 1f);
      Fog.parameters.meanFreePath = 15f;
      Fog.parameters.size.y = 255f;
    }
  }
}
