using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DW2ChsPatch.Feature;
using DW2ChsPatch.TextProcess;
using HarmonyLib;

namespace DW2ChsPatch.Optimization
{
	public static class ReduceMeshPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds2.DWRendererBase:GenerateBlackHoleVertices"),
				null, null,
				new HarmonyMethod(typeof(ReduceMeshPatch), nameof(GenerateBlackHoleVerticesTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds2.DWRendererBase:GenerateSphereVertices"),
				null, null,
				new HarmonyMethod(typeof(ReduceMeshPatch), nameof(GenerateSphereVerticesTranspiler)));
		}

		private static IEnumerable<CodeInstruction> GenerateBlackHoleVerticesTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_I4 &&
				    instruction.operand is int value &&
				    value == 256)
				{
					instruction.operand = 32;
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateSphereVerticesTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_I4_S &&
				    instruction.operand is sbyte value &&
				    value == 64)
				{
					instruction.operand = (sbyte) 32;
				}
				yield return instruction;
			}
		}
	}
}