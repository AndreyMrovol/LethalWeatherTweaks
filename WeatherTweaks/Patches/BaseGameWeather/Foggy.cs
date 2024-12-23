using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using WeatherRegistry;
using WeatherRegistry.Definitions;

namespace WeatherTweaks.Patches
{
  class FoggyPatch
  {
    public static void CreateEffectOverrides()
    {
      Plugin.logger.LogInfo("CreateEffectOverrides called");

      Weather foggyWeather = WeatherManager.GetWeather(LevelWeatherType.Foggy);
      ImprovedWeatherEffect newFoggy = new(foggyWeather.Effect.EffectObject, GameObject.Instantiate(foggyWeather.Effect.WorldObject));

      GameObject newFoggyEffect = newFoggy.WorldObject;

      LocalVolumetricFog Fog = newFoggyEffect.GetComponent<LocalVolumetricFog>();

      // get new LocalVolumetricFog parameters
      LocalVolumetricFog newFog = ChangeFogParams(Fog);

      // replace old LocalVolumetricFog with new one
      List<LocalVolumetricFog> fogs = newFoggyEffect.GetComponents<LocalVolumetricFog>().ToList();
      fogs.Remove(Fog);
      fogs.Add(newFog);

      // replace old foggy effect with new one
      newFoggy.WorldObject = newFoggyEffect;

      // create weather override for every level

      foreach (SelectableLevel level in MrovLib.LevelHelper.Levels)
      {
        if (ConfigManager.FoggyIgnoreLevels.Value.Contains(level))
        {
          continue;
        }

        WeatherEffectOverride effectOverride = new(foggyWeather, level, newFoggy);
      }
    }

    public static LocalVolumetricFog ChangeFogParams(LocalVolumetricFog Fog)
    {
      Plugin.logger.LogInfo("ChangeFog called");

      Plugin.logger.LogInfo($"is null? {Fog == null}");

      try
      {
        LocalVolumetricFogArtistParameters parameters = Fog.parameters;

        Plugin.DebugLogger.LogWarning(
          $"FOG: {Fog.name} {parameters.size} {Fog.transform.position} {parameters.albedo} {parameters.meanFreePath}"
        );

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

        Plugin.DebugLogger.LogWarning(
          $"FOG: {Fog.name} {parameters.size} {Fog.transform.position} {parameters.albedo} {parameters.meanFreePath}"
        );

        Fog.parameters = parameters;
      }
      catch (Exception)
      {
        Plugin.logger.LogWarning("Failed to change fog");
      }

      return Fog;
    }

    public static void ToggleFogExclusionZones(SelectableLevel level, bool enable = true)
    {
      Plugin.logger.LogDebug("DisableFogExclusionZones called");

      // if (!MrovLib.Defaults.IsVanillaLevel(level) && !enable)
      // {
      //   Plugin.logger.LogDebug("Level is not vanilla, skipping");
      //   return;
      // }

      List<GameObject> fogExclusionZones = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == "FogExclusionZone").ToList();
      foreach (GameObject fogExclusionZone in fogExclusionZones)
      {
        Plugin.DebugLogger.LogDebug(
          $"Setting fog exclusion zone {fogExclusionZone.name} (parent {fogExclusionZone.transform.parent.name}) to {enable}"
        );
        fogExclusionZone.SetActive(enable);
      }
    }
  }
}
