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
  [HarmonyPatch(typeof(TimeOfDay))]
  partial class BasegameWeatherPatch
  {
    internal static ManualLogSource loggerSun = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks SunAnimator");

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

      if (animator == null)
      {
        animator = TimeOfDay.Instance.sunAnimator;
      }

      // get the name of the sun animator controller
      string animatorControllerName = animator.runtimeAnimatorController.name;
      loggerSun.LogInfo($"animatorControllerName: {animatorControllerName}, weatherType: {weatherType}");

      if (!animatorControllerNames.Contains(animatorControllerName) && !animatorControllerName.Contains("override"))
      {
        loggerSun.LogError($"Animator controller {animatorControllerName} not found in list of supported animator controllers");
        return;
      }

      if (animatorOverrideController == null)
      {
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController)
        {
          name = $"{animatorControllerName}override"
        };
      }

      AnimationClipOverrides clipOverrides = new(animatorOverrideController.overridesCount);
      loggerSun.LogWarning($"Overrides: {animatorOverrideController.overridesCount}");
      animatorOverrideController.GetOverrides(clipOverrides);

      LogOverrides(clipOverrides);

      // log all clips in the animator
      var animationClips = animatorOverrideController.runtimeAnimatorController.animationClips.ToList();
      animationClips.ForEach(clip =>
      {
        loggerSun.LogInfo($"clip: {clip.name}");
        loggerSun.LogInfo($"clip length: {clip.length}");
        loggerSun.LogInfo($"clip framerate: {clip.frameRate}");
      });

      Dictionary<LevelWeatherType, AnimationClip> clips =
        new()
        {
          { LevelWeatherType.Stormy, animationClips.Find(clip => clip.name.Contains(clipNames[LevelWeatherType.Stormy])) },
          { LevelWeatherType.Eclipsed, animationClips.Find(clip => clip.name.Contains(clipNames[LevelWeatherType.Eclipsed])) },
          {
            LevelWeatherType.None,
            animationClips.Find(clip =>
              !clip.name.Contains(clipNames[LevelWeatherType.Stormy]) && !clip.name.Contains(clipNames[LevelWeatherType.Eclipsed])
            )
          },
        };

      // try to get clip names dynamically from the animator controller
      // use contains to check through the dictionary

      // get the name of the animation clip for the current weather type
      string animationClipName = clips.TryGetValue(weatherType, out AnimationClip clip) ? clip.name : null;
      if (animationClipName == null)
      {
        loggerSun.LogError($"No animation clip found for weather type {weatherType}");
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
            loggerSun.LogInfo($"Setting override from {clipPair.Value.name} to {clips[weatherType].name}");
          }
          else
          {
            clipOverrides[clipPair.Value.name] = null;
            loggerSun.LogInfo($"Setting override from {clipPair.Value.name} to null");
          }
        });

      // get the hash of the animation clip
      int animationClipHash = Animator.StringToHash(animationClipName);
      loggerSun.LogInfo($"animationClipHash: {animationClipHash}");

      // animationClips.ForEach(clip =>
      // {
      //   clipOverrides[clip.name] = clips[weatherType];

      //   loggerSun.LogInfo($"Setting override from {clip.name} to {clips[weatherType]}");
      // });

      loggerSun.LogInfo($"Changing animation clip to {animationClipName} ({animationClipHash})");
      loggerSun.LogInfo($"Current bools: {animator.GetBool("overcast")} {animator.GetBool("eclipsed")}");

      // // set the overrides
      // if (weatherType == LevelWeatherType.None)
      // {
      //   loggerSun.LogInfo($"Setting {animationClipName} to {clips[LevelWeatherType.None].name}");
      //   clipOverrides[animationClipName] = clips[LevelWeatherType.None];
      // }
      // else if (weatherType == LevelWeatherType.Stormy)
      // {
      //   loggerSun.LogInfo($"Setting animation clip to {clips[LevelWeatherType.Stormy].name}");
      //   clipOverrides[animationClipName] = clips[LevelWeatherType.Stormy];
      // }
      // else if (weatherType == LevelWeatherType.Eclipsed)
      // {
      //   clipOverrides[animationClipName] = clips[LevelWeatherType.Eclipsed];
      // }

      LogOverrides(clipOverrides);

      if (weatherType != LevelWeatherType.None)
      {
        animatorOverrideController.ApplyOverrides(clipOverrides);
        animator.runtimeAnimatorController = animatorOverrideController;
      }
      else
      {
        animator.runtimeAnimatorController = animatorOverrideController.runtimeAnimatorController;
      }

      // animator.CrossFadeInFixedTime(animationClipHash, 2.5f, 0);
      // animator.Play(animationClipHash, 0, 0);

      // log the current clip name
      loggerSun.LogInfo($"Current clip: {animator.GetCurrentAnimatorClipInfo(0)[0].clip.name}");

      // if (weatherType == LevelWeatherType.None)
      // {
      //   animator.Play(0, 1, 0);
      // }
      // else
      // {
      // }

      // set the animation clip with 2s transition
      // animator.Play(animationClipHash, 0, 0);
    }

    internal static void LogOverrides(AnimationClipOverrides clipOverrides)
    {
      loggerSun.LogWarning($"Overrides: {clipOverrides.Count}");
      clipOverrides
        .ToList()
        .ForEach(clip =>
        {
          loggerSun.LogInfo($"clip {(clip.Key ? clip.Key.name : "null")} : {(clip.Value ? clip.Value.name : "null")}");
        });
    }

    // [HarmonyPatch("Update")]
    // [HarmonyPostfix]
    // public static void UpdatePatch()
    // {
    //   loggerSun.LogInfo($"{TimeOfDay.Instance.sunAnimator.GetFloat("timeOfDay")}");
    //   loggerSun.LogInfo($"{TimeOfDay.Instance.sunIndirect.intensity} {TimeOfDay.Instance.sunDirect.intensity}");
    // }
  }
}
