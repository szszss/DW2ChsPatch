using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public class OrdinalNumberPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:GetOrdinalNumberDescription"),
				new HarmonyMethod(typeof(OrdinalNumberPatch), nameof(OrdinalNumberPrefix)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:OrderedNumberDescription"),
				new HarmonyMethod(typeof(OrdinalNumberPatch), nameof(OrdinalNumberPrefix)));
			harmony.Patch(AccessTools.FirstConstructor(AccessTools.TypeByName("DistantWorlds.Types.Troop"), 
					x => x.GetParameters().Length > 4),
				null, null,
				new HarmonyMethod(typeof(OrdinalNumberPatch), nameof(TroopTranspiler)));
		}

		public static bool OrdinalNumberPrefix(out string __result, int __0)
		{
			__result = $"第{__0}";
			return false;
		}

		private static IEnumerable<CodeInstruction> TroopTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}
				yield return instruction;
			}
		}
	}
}