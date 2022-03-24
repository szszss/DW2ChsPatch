using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public class AmbiguousWordsFixPatch
	{
		private static FieldInfo _label;

		public static void Patch(Harmony harmony)
		{
			_label = AccessTools.Field("DistantWorlds.UI.DWButtonData:Label");

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.CharacterPanel:BindCharacter"),
				null, null,
				new HarmonyMethod(typeof(AmbiguousWordsFixPatch), nameof(DismissTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.CharacterPanel:GenerateControls"),
				null, null,
				new HarmonyMethod(typeof(AmbiguousWordsFixPatch), nameof(DismissTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.CharacterPanel:DismissButton_Clicked"),
				null, null,
				new HarmonyMethod(typeof(AmbiguousWordsFixPatch), nameof(DismissTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.StartNewGameDialog:SetNextPreviousButtonLabels"),
				null, null,
				new HarmonyMethod(typeof(AmbiguousWordsFixPatch), nameof(PrevNextTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.StartNewGameDialog:SetupUserInterfaceControls"),
				null, null,
				new HarmonyMethod(typeof(AmbiguousWordsFixPatch), nameof(PrevNextTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.StartNewGameDialog:AddControlsToPanel"),
				null, null,
				new HarmonyMethod(typeof(AmbiguousWordsFixPatch), nameof(AddControlsToPanelTranspiler)));
		}

		private static IEnumerable<CodeInstruction> DismissTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Dismiss": newStr = "Dismiss Character"; break;
					}

					instruction.operand = newStr;
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> PrevNextTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Next": newStr = "Next Page"; break;
						case "Previous": newStr = "Previous Page"; break;
					}

					instruction.operand = newStr;
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> AddControlsToPanelTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (instruction.opcode == OpCodes.Newobj &&
				         instruction.operand is ConstructorInfo method &&
				         method.DeclaringType.Name.Contains("DWButtonData"))
				{
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Call, 
						AccessTools.Method(typeof(AmbiguousWordsFixPatch), nameof(FixEmpireListTitle)));
				}
			}
		}

		public static void FixEmpireListTitle(object __instance)
		{
			if (_label != null)
			{
				var label = _label.GetValue(__instance) as string;
				if (label != null)
				{
					switch (label)
					{
						case "Name": label = "国名"; break;
						case "Race": label = "种族"; break;
						case "Expansion": label = "扩张范围"; break;
						case "Proximity": label = "与玩家距离"; break;
						case "Government": label = "政体"; break;
						case "Remove": label = "移除"; break;
						default: return;
					}
					_label.SetValue(__instance, label);
				}
			}
		}
	}
}