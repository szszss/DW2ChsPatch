using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class FleetDesignFixPatch
	{
		private static float _size;

		public static void Patch(Harmony harmony, float size)
		{
			_size = Math.Max(Math.Min(size, 2f), 0.1f);

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.FleetTemplateDialog:GenerateControls"),
				null, null, new HarmonyMethod(typeof(FleetDesignFixPatch), nameof(GenerateControlsTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(
				AccessTools.TypeByName("DistantWorlds.UI.FleetTemplateDialog"),
				method => method.GetParameters().Length == 4),
				null, null, new HarmonyMethod(typeof(FleetDesignFixPatch), nameof(GenerateControlsTranspiler)));
		}

		private static IEnumerable<CodeInstruction> GenerateControlsTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (instruction.opcode == OpCodes.Callvirt &&
				    instruction.operand is MethodInfo method &&
				    method.Name.Contains("GetTotalLineSpacing"))
				{
					yield return new CodeInstruction(OpCodes.Ldc_R4, _size);
					yield return new CodeInstruction(OpCodes.Mul);
				}
			}
		}
	}
}