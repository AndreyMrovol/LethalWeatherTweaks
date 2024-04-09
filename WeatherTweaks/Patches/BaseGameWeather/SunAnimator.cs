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
    // dictionary storing <animatorcontoller name, <weather type, animation clip name>>
    internal static Dictionary<string, Dictionary<LevelWeatherType, string>> animationClips = new Dictionary<
      string,
      Dictionary<LevelWeatherType, string>
    >()
    {
      {
        "SunAnimContainer",
        new Dictionary<LevelWeatherType, string>()
        {
          { LevelWeatherType.None, "TimeOfDaySun" },
          { LevelWeatherType.Stormy, "TimeOfDaySunStormy" },
          { LevelWeatherType.Eclipsed, "TimeOfDaySunEclipse" },
        }
      },
      {
        "SunAnimContainer 1",
        new Dictionary<LevelWeatherType, string>()
        {
          { LevelWeatherType.None, "TimeOfDaySunTypeB" },
          { LevelWeatherType.Stormy, "TimeOfDaySunTypeBStormy" },
          { LevelWeatherType.Eclipsed, "TimeOfDaySunTypeBEclipse" },
        }
      },
      {
        "BlizzardSunAnimContainer",
        new Dictionary<LevelWeatherType, string>()
        {
          { LevelWeatherType.None, "TimeOfDaySunTypeC" },
          { LevelWeatherType.Stormy, "TimeOfDaySunTypeCStormy" },
          { LevelWeatherType.Eclipsed, "TimeOfDaySunTypeCEclipse" },
        }
      }
    };

    public static void OverrideSunAnimator(LevelWeatherType weatherType)
    {
      // Right now the possible transitions are:
      // state TimeOfDaySun -> state TimeOfDaySunEclipse (bool eclipsed = true is the condition)
      // state TimeOfDaySun -> state TimeOfDaySunStormy (bool overcast = true is the condition)

      Animator animator = TimeOfDay.Instance.sunAnimator;

      // i want to change the animation clip of the sun based on the weather type

      // get the name of the sun animator controller
      string animatorControllerName = animator.runtimeAnimatorController.name;
      Plugin.logger.LogInfo($"animatorControllerName: {animatorControllerName}");

      var clips = animationClips[animatorControllerName];

      // get the name of the animation clip for the current weather type
      string animationClipName = clips.TryGetValue(weatherType, out string clipName) ? clipName : null;
      if (animationClipName == null)
      {
        Plugin.logger.LogError($"No animation clip found for weather type {weatherType}");
        return;
      }

      // get the hash of the animation clip
      int animationClipHash = Animator.StringToHash(animationClipName);

      Plugin.logger.LogInfo($"Changing animation clip to {animationClipName} ({animationClipHash})");

      // set the animation clip with 2s transition
      // possibly not using crossfade
      animator.CrossFadeInFixedTime(animationClipHash, 2.5f, 0);
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
