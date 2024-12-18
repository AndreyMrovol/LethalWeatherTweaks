using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WeatherTweaks.Weathers
{
  public class Blackout : MonoBehaviour
  {
    List<Light> AllPoweredLights = [];
    Dictionary<GrabbableObject, Light> Flashlights = [];

    private MrovLib.Logger Logger = new("Blackout");

    private float FloodlightRange = 44;
    private float FloodlightAngle = 116.7f;
    private float FloodlightIntensity = 762;

    public class LightUtils
    {
      // Get all lights in a specific scene
      public static List<Light> GetLightsInScene(string sceneName)
      {
        List<Light> sceneLights = [];
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
        {
          Plugin.DebugLogger.LogWarning($"Scene {sceneName} is not loaded!");
          return sceneLights;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
          // Get lights in children of root objects
          Light[] childLights = root.GetComponentsInChildren<Light>(true);
          sceneLights.AddRange(childLights);
        }

        return sceneLights;
      }

      // Get lights under a specific parent transform
      public static List<Light> GetLightsUnderParent(Transform parent, bool includeInactive = true)
      {
        List<Light> lights = [];
        if (parent == null)
          return lights;

        Light[] childLights = parent.GetComponentsInChildren<Light>(includeInactive);
        lights.AddRange(childLights);

        return lights;
      }

      // Get lights in specific scene and under specific parent
      public static List<Light> GetLightsInSceneUnderParent(string sceneName, string parentName, bool includeInactive = true)
      {
        List<Light> lights = [];
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
        {
          Plugin.DebugLogger.LogWarning($"Scene {sceneName} is not loaded!");
          return lights;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
          if (root.name == parentName)
          {
            lights.AddRange(GetLightsUnderParent(root.transform, includeInactive));
            break;
          }

          // Search in children if not found at root
          Transform parent = root.transform.Find(parentName);
          if (parent != null)
          {
            lights.AddRange(GetLightsUnderParent(parent, includeInactive));
            break;
          }
        }

        return lights;
      }

      // Get all lights in multiple scenes
      public static Dictionary<string, List<Light>> GetLightsInScenes(string[] sceneNames)
      {
        Dictionary<string, List<Light>> sceneLights = [];

        foreach (string sceneName in sceneNames)
        {
          sceneLights[sceneName] = GetLightsInScene(sceneName);
        }

        return sceneLights;
      }

      // The previous methods remain unchanged...
      public static Light[] GetAllLightsInScene(bool includeInactive = true)
      {
        return Resources.FindObjectsOfTypeAll<Light>();
      }

      public static Light[] GetActiveLightsInScene()
      {
        return GameObject.FindObjectsOfType<Light>();
      }

      public static List<Light> GetLightsByType(LightType lightType)
      {
        Light[] allLights = GetAllLightsInScene();
        List<Light> filteredLights = [];

        foreach (Light light in allLights)
        {
          if (light.type == lightType)
          {
            filteredLights.Add(light);
          }
        }

        return filteredLights;
      }
    }

    private void OnEnable()
    {
      if (!WeatherRegistry.WeatherManager.IsSetupFinished)
      {
        return;
      }

      LungProp currentApparatus = UnityEngine.Object.FindObjectOfType<LungProp>();

      // disable all lights in the level's scene
      AllPoweredLights = LightUtils.GetLightsInScene(StartOfRound.Instance.currentLevel.sceneName);
      Logger.LogInfo($"Found {AllPoweredLights.Count} lights in scene {StartOfRound.Instance.currentLevel.sceneName}");
      foreach (Light light in AllPoweredLights)
      {
        if (currentApparatus != null)
        {
          if (light.transform.parent == currentApparatus.gameObject.transform)
          {
            Plugin.logger.LogDebug($"Skipping light {light.name} (parent {light.transform.parent.name})");

            light.gameObject.TryGetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>(out var hdLight);
            if (hdLight != null)
            {
              hdLight.SetIntensity(600);
            }
            continue;
          }
        }

        Logger.LogDebug($"Disabling light {light.name} (parent {light.transform.parent.name})");
        light.gameObject.SetActive(false);
      }

      // set power to true, so doors stay closed
      RoundManager.Instance.SwitchPower(true);

      // vanilla method for disabling lights
      RoundManager.Instance.TurnOnAllLights(false);

      // destroy the breaker box so power can't be turned back on
      BreakerBox currentBreakerBox = UnityEngine.Object.FindObjectOfType<BreakerBox>();
      if (currentBreakerBox != null)
      {
        currentBreakerBox.gameObject.SetActive(false);
      }

      // disable sun
      UnityEngine.GameObject sun = UnityEngine.GameObject.Find("Sun");
      if (sun != null)
      {
        sun.SetActive(false);
      }

      // improve the range of floodlights
      // it's like that because of imperium race-condition
      try
      {
        Transform FloodlightParentTransform = GameObject.Find("ShipLightsPost").GetComponent<Transform>();
        List<Light> floodlights = LightUtils.GetLightsUnderParent(FloodlightParentTransform);
        Logger.LogInfo($"Found {floodlights.Count} floodlights in scene SampleSceneRelay");

        foreach (Light light in floodlights)
        {
          light.gameObject.TryGetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>(out var hdLight);
          if (hdLight != null)
          {
            hdLight.SetIntensity(30000);
            hdLight.SetSpotAngle(120);
            hdLight.SetRange(600);
          }
        }
      }
      catch (Exception ex)
      {
        Logger.LogWarning($"Error while trying to modify floodlights: {ex}");
      }

      // improve the range of visibility inside the dungeon
      // List<PlayerControllerB> players = UnityEngine.Object.FindObjectsOfType<PlayerControllerB>().ToList();
      // players.ForEach(player =>
      // {
      //   Light nightVision = player.GetComponent
      // });

      // turn off ship lights

      // tweak flashlights a little

      // reduce range of helmet light
    }

    private void OnDisable()
    {
      if (!WeatherRegistry.WeatherManager.IsSetupFinished)
      {
        return;
      }

      AllPoweredLights.Clear();

      try
      {
        // revert all lights to their original state
        Transform FloodlightParentTransform = GameObject.Find("ShipLightsPost").GetComponent<Transform>();
        List<Light> floodlights = LightUtils.GetLightsUnderParent(FloodlightParentTransform);
        Logger.LogInfo($"Found {floodlights.Count} floodlights in scene SampleSceneRelay");

        foreach (Light light in floodlights)
        {
          light.gameObject.TryGetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>(out var hdLight);
          if (hdLight != null)
          {
            hdLight.SetIntensity(FloodlightIntensity);
            hdLight.SetSpotAngle(FloodlightAngle);
            hdLight.SetRange(FloodlightRange);
          }
        }
      }
      catch (Exception ex)
      {
        Logger.LogWarning($"Error while trying to modify floodlights: {ex}");
      }
    }
  }
}
