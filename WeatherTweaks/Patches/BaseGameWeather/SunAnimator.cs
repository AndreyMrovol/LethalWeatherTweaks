using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using DunGen;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  partial class BasegameWeatherPatch
  {
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

    internal static AnimatorOverrideController animatorOverrideController;

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

      Animator animator = TimeOfDay.Instance.sunAnimator;
      // animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
      // AnimationClipOverrides clipOverrides = new(animatorOverrideController.overridesCount);

      // i want to change the animation clip of the sun based on the weather type

      // log all clips in the animator
      animator.runtimeAnimatorController.animationClips.ToList().ForEach(clip => Plugin.logger.LogInfo(clip.name));

      // get the name of the sun animator controller
      string animatorControllerName = animator.runtimeAnimatorController.name;
      Plugin.logger.LogInfo($"animatorControllerName: {animatorControllerName}, weatherType: {weatherType}");

      if (!animatorControllerNames.Contains(animatorControllerName))
      {
        Plugin.logger.LogError($"Animator controller {animatorControllerName} not found in list of supported animator controllers");
        return;
      }

      var animationClips = animator.runtimeAnimatorController.animationClips.ToList();
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
        Plugin.logger.LogError($"No animation clip found for weather type {weatherType}");
        return;
      }

      // get the hash of the animation clip
      int animationClipHash = Animator.StringToHash(animationClipName);

      Plugin.logger.LogInfo($"Changing animation clip to {animationClipName} ({animationClipHash})");
      Plugin.logger.LogInfo($"Current bools: {animator.GetBool("overcast")} {animator.GetBool("eclipsed")}");

      // set the animation clip with 2s transition
      animator.CrossFadeInFixedTime(animationClipHash, 2.5f, 0);
      // animator.Play(animationClipHash, 0, 0);
    }

    // [HarmonyPatch("Update")]
    // [HarmonyPostfix]
    // public static void UpdatePatch()
    // {
    //   Plugin.logger.LogInfo($"{TimeOfDay.Instance.sunAnimator.GetFloat("timeOfDay")}");
    //   Plugin.logger.LogInfo($"{TimeOfDay.Instance.sunIndirect.intensity} {TimeOfDay.Instance.sunDirect.intensity}");
    // }
  }
}
