using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx.Logging;
using DunGen;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;

namespace WeatherTweaks
{
  internal class SunAnimator
  {
    public static void Init()
    {
      if (!ConfigManager.SunAnimatorPatch.Value)
      {
        return;
      }

      // create separate harmony instance for Animator
      var harmony = new Harmony("WeatherTweaks.SunAnimator");

      // patch the Animator class with the SetBool method
      harmony.Patch(
        AccessTools.Method(typeof(UnityEngine.Animator), "SetBool", new Type[] { typeof(string), typeof(bool) }),
        new HarmonyMethod(typeof(SunAnimator), "SetBoolStringPatch")
      );
      logger.LogWarning("Patching Animator.SetBool(string, bool)");

      // harmony.Patch(
      //   AccessTools.Method(typeof(UnityEngine.Animator), "SetBool", new Type[] { typeof(int), typeof(bool) }),
      //   new HarmonyMethod(typeof(SunAnimator), "SetBoolIntPatch")
      // );
      // logger.LogWarning("Patching Animator.SetBool(int, bool)");
    }

    public static bool SetBoolPatch(Animator __instance, object nameOrId, bool value)
    {
      string name = nameOrId as string;
      // int id = (nameOrId is int) ? (int)nameOrId : -1; // Assuming -1 is not a valid ID

      if (name == "overcast" || name == "eclipse")
      {
        return false;
      }

      if (SunAnimator.animator == null)
      {
        return true;
      }

      if (__instance == SunAnimator.animator)
      {
        if (name != null)
        {
          if ((name == "overcast" || name == "eclipse") && value == true)
          {
            // SunAnimator.logger.LogInfo($"Setting {name} to {false}");
            //
            // __instance.SetBool(name, false);
            return false;
          }
        }
      }

      return true;
    }

    public static bool SetBoolStringPatch(Animator __instance, string name, bool value)
    {
      return SetBoolPatch(__instance, name, value);
    }

    public static bool SetBoolIntPatch(Animator __instance, int id, bool value)
    {
      return SetBoolPatch(__instance, id, value);
    }

    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks SunAnimator");

    internal static Dictionary<LevelWeatherType, string> clipNames =
      new()
      {
        { LevelWeatherType.None, "" },
        { LevelWeatherType.Stormy, "Stormy" },
        { LevelWeatherType.Eclipsed, "Eclipse" },
      };

    internal static List<string> animatorControllerNames = new List<string>()
    {
      "SunAnimContainer",
      "SunAnimContainer 1",
      "BlizzardSunAnimContainer",
      "BasicSun",
      "StarlancerSolaceSunAnimContainer",
      "StarlancerAuralisSunAnimContainer",
      "StarlancerTriskelionSunAnimContainer",
    };

    internal static List<string> animatorControllerBlacklist = new List<string>() { "SunAnimContainerCompanyLevel" };

    internal static Animator animator;
    internal static AnimatorOverrideController animatorOverrideController;

    // internal static AnimationClipOverrides clipOverrides;

    public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
      public AnimationClipOverrides(int capacity)
        : base(capacity) { }

      public AnimationClip this[string name]
      {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
          int index = this.FindIndex(x => x.Key.name.Equals(name));
          if (index != -1)
            this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
      }
    }

    public static void OverrideSunAnimator(LevelWeatherType weatherType)
    {
      // Right now the possible transitions are:
      // state TimeOfDaySun -> state TimeOfDaySunEclipse (bool eclipsed = true is the condition)
      // state TimeOfDaySun -> state TimeOfDaySunStormy (bool overcast = true is the condition)
      // i want to change the animation clip of the sun based on the weather type

      if (!ConfigManager.SunAnimatorPatch.Value)
      {
        return;
      }

      if (animator == null)
      {
        animator = TimeOfDay.Instance.sunAnimator;
      }

      if (TimeOfDay.Instance.sunAnimator == null)
      {
        logger.LogWarning("sunAnimator is null, skipping");
        return;
      }

      logger.LogInfo($"Current clip: {animator.GetCurrentAnimatorClipInfo(0)[0].clip.name}");

      // get the name of the sun animator controller
      string animatorControllerName = animator.runtimeAnimatorController.name;
      logger.LogInfo($"animatorControllerName: {animatorControllerName}, weatherType: {weatherType}");

      if (animatorControllerBlacklist.Contains(animatorControllerName))
      {
        logger.LogWarning($"Animator controller {animatorControllerName} is blacklisted");
        return;
      }

      // if (!animatorControllerNames.Contains(animatorControllerName) && !animatorControllerName.Contains("override"))
      // {
      //   logger.LogError($"Animator controller {animatorControllerName} not found in list of supported animator controllers");
      //   return;
      // }

      if (animatorOverrideController == null)
      {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController)
        {
          name = $"{animatorControllerName}override"
        };
      }

      AnimationClipOverrides clipOverrides = new(animatorOverrideController.overridesCount);
      logger.LogDebug($"Overrides: {animatorOverrideController.overridesCount}");
      animatorOverrideController.GetOverrides(clipOverrides);

      // log all clips in the animator
      var animationClips = animatorOverrideController.runtimeAnimatorController.animationClips.ToList();
      // animationClips.ForEach(clip =>
      // {
      //   logger.LogInfo($"clip: {clip.name}");
      //   logger.LogInfo($"clip length: {clip.length}");
      //   logger.LogInfo($"clip framerate: {clip.frameRate}");
      // });

      // get the animation clips for the different weather types
      // if any of them cannot be found, log a warning and return
      Dictionary<LevelWeatherType, AnimationClip> clips = [];

      try
      {
        AnimationClip clipEclipsed = animationClips.Find(clip => clip.name.Contains(clipNames[LevelWeatherType.Eclipsed]));
        AnimationClip clipStormy = animationClips.Find(clip => clip.name.Contains(clipNames[LevelWeatherType.Stormy]));
        AnimationClip clipNone = animationClips.Find(clip =>
          !clip.name.Contains(clipNames[LevelWeatherType.Stormy]) && !clip.name.Contains(clipNames[LevelWeatherType.Eclipsed])
        );

        clips = new Dictionary<LevelWeatherType, AnimationClip>()
        {
          { LevelWeatherType.Eclipsed, clipEclipsed },
          { LevelWeatherType.Stormy, clipStormy },
          { LevelWeatherType.Flooded, clipStormy },
          { LevelWeatherType.Foggy, clipStormy },
          { LevelWeatherType.Rainy, clipStormy },
          { LevelWeatherType.None, clipNone },
        };

        if (clipEclipsed == null || clipStormy == null || clipNone == null)
        {
          return;
        }
      }
      catch (Exception e)
      {
        logger.LogWarning($"SunAnimator error: {e}");
        logger.LogError($"Detected a null clip: {e.Message}");
        return;
      }

      if (clips.Keys.Select(key => key == weatherType).Count() == 0)
      {
        logger.LogWarning($"No animation clip found for weather type {weatherType}");
        return;
      }

      // try to get clip names dynamically from the animator controller
      // use contains to check through the dictionary

      // get the name of the animation clip for the current weather type
      string animationClipName = clips.TryGetValue(weatherType, out AnimationClip clip) ? clip.name : null;
      if (animationClipName == null)
      {
        logger.LogWarning($"No animation clip found for weather type {weatherType}");
        return;
      }

      clips
        .ToList()
        .ForEach(clipPair =>
        {
          // override only clips different than the new one

          if (clipPair.Key != weatherType)
          {
            clipOverrides[clipPair.Value.name] = clips[weatherType];
            logger.LogDebug($"Setting override from {clipPair.Value.name} to {clips[weatherType].name}");
          }
          else
          {
            clipOverrides[clipPair.Value.name] = null;
            logger.LogDebug($"Setting override from {clipPair.Value.name} to null");
          }
        });

      // get the hash of the animation clip
      // int animationClipHash = Animator.StringToHash(animationClipName);
      // logger.LogDebug($"animationClipHash: {animationClipHash}");

      logger.LogDebug($"Current bools: {animator.GetBool("overcast")} {animator.GetBool("eclipsed")}");

      if (weatherType != LevelWeatherType.None)
      {
        animatorOverrideController.ApplyOverrides(clipOverrides);
        animator.runtimeAnimatorController = animatorOverrideController;
      }
      else
      {
        animator.runtimeAnimatorController = animatorOverrideController.runtimeAnimatorController;
      }

      // animator.PlayInFixedTime(animationClipHash, 0, 2.5f);

      // log the current clip name
      // logger.LogInfo($"Current clip: {animator.GetCurrentAnimatorClipInfo(0)[0].clip.name}");
      // logger.LogInfo($"Current bools: {animator.GetBool("overcast")} {animator.GetBool("eclipsed")}");
    }

    internal static void LogOverrides(AnimationClipOverrides clipOverrides)
    {
      logger.LogDebug($"Overrides: {clipOverrides.Count}");
      clipOverrides
        .ToList()
        .ForEach(clip =>
        {
          logger.LogInfo($"overrideclip {(clip.Key ? clip.Key.name : "null")} : {(clip.Value ? clip.Value.name : "null")}");
        });
    }

    internal static void Clear()
    {
      animator = null;
      animatorOverrideController = null;
    }
  }
}
