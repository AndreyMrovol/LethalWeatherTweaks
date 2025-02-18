//using System.Collections.Generic;
//using System.Reflection.Emit;
//using GameNetcodeStuff;
//using HarmonyLib;
//using UnityEngine.AI;
//using UnityEngine;

//namespace WeatherTweaks
//{
//    [HarmonyPatch(typeof(RoundManager))]
//    partial class BasegameWeatherPatch
//    {
//        [HarmonyTranspiler]
//        [HarmonyPatch(typeof(RoundManager), "SpawnOutsideHazards")]
//        static IEnumerable<CodeInstruction> SpawnOutsideHazardsPatch(IEnumerable<CodeInstruction> instructions)
//        {
//            CodeMatcher codeMatcher = new(instructions);

//            codeMatcher = codeMatcher.MatchForward(
//            false,
//            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "Instance")),
//            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentLevelWeather"))
//            );
//            logger.LogDebug($"Matched Ldfld for RoundManager.SpawnOutsideHazards");

//            codeMatcher.Repeat(match =>
//            {
//                // Remove original instruction
//                codeMatcher.RemoveInstruction(); // removes  call class TimeOfDay  TimeOfDay::get_Instance()
//                codeMatcher.RemoveInstruction(); // removes  ldfld float32 TimeOfDay::currentWeatherVariable

//                // Get the current weather variable
//                // Variables.GetLevelWeatherVariable method takes 2 arguments: int and bool - set them
//                codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, (int)LevelWeatherType.Rainy));
//                codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Variables), "LevelHasWeather")));
//            });

//            return codeMatcher.InstructionEnumeration();
//        }


//        internal static bool isRainyProgressingMidday = false;

//        internal static float mudSqrDistance = 100f;
//        internal static int MudPlacementAttempts => 10;

//        internal static bool IsMudPickValid(Vector3 position)
//        {
//            bool isValidPick = true;
//            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
//            {
//                if (Vector3.SqrMagnitude(position - player.transform.position) < mudSqrDistance)
//                {
//                    isValidPick = false;
//                    break;
//                }
//            }
//            return isValidPick;
//        }

//        internal static Vector3 TryGetValidMudPick(Vector3 position)
//        {
//            if (!isRainyProgressingMidday)
//                return position;
//            System.Random random = new(StartOfRound.Instance.randomMapSeed + 2);
//            NavMeshHit navHit = new();
//            int attemptNum = 0;
//            Vector3 adjustedPosition = position;
//            while (attemptNum < MudPlacementAttempts && !IsMudPickValid(position))
//            {
//                adjustedPosition = RoundManager.Instance.outsideAINodes[random.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
//                adjustedPosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(adjustedPosition, 30f, navHit, random, -1) + Vector3.up;
//                attemptNum++;
//            }

//            return adjustedPosition;
//        }
//        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnOutsideHazards))]
//        [HarmonyTranspiler]
//        static IEnumerable<CodeInstruction> SpawnOutsideHazardsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
//        {
//            CodeMatcher codeMatcher = new(instructions);

//            codeMatcher.MatchForward(true,
//                new CodeMatch(OpCodes.Ldloc_1),
//                new CodeMatch(OpCodes.Ldloc_0),
//                new CodeMatch(OpCodes.Ldc_I4_M1),
//                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GetRandomNavMeshPositionInBoxPredictable)))
//            ).Advance(1);

//            if (codeMatcher.IsInvalid)
//            {
//                Debug.LogError("Couldn't match GetRandomNavMeshPositionInBoxPredictable in SpawnOutsideHazards");
//                return instructions;
//            }
//            codeMatcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BasegameWeatherPatch), nameof(BasegameWeatherPatch.TryGetValidMudPick))));

//            codeMatcher.MatchForward(false,
//                new CodeMatch(OpCodes.Ldc_I4_0),
//                new CodeMatch(OpCodes.Stloc_S),
//                new CodeMatch(OpCodes.Newobj),
//                new CodeMatch(OpCodes.Stloc_S)
//            );

//            if (codeMatcher.IsInvalid)
//            {
//                Debug.LogError("Could not insert early return in SpawnOutsideHazards");
//                return instructions;
//            }

//            Label jumpTargetLabel = generator.DefineLabel();
//            codeMatcher.Instruction.labels.Add(jumpTargetLabel);

//            codeMatcher.Insert(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(BasegameWeatherPatch), nameof(isRainyProgressingMidday))),
//               new CodeInstruction(OpCodes.Brfalse_S, jumpTargetLabel),
//               new CodeInstruction(OpCodes.Ret));

//            return codeMatcher.InstructionEnumeration();
//        }


//    }
//}
