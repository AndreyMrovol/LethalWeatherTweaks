using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  internal class Variables
  {
    public static List<SelectableLevel> GameLevels = [];

    internal static void GetGameLevels(StartOfRound __instance)
    {
      GameLevels = __instance.levels.Where(level => level.PlanetName != "71 Gordion").ToList();
    }
  }
}
