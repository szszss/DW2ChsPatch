using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class RemoveStarPostfixPatch
	{
		private static string _planetName = null;

		private static string _moonName = null;

		private static MethodInfo _getText;

		public static void Patch(Harmony harmony)
		{
			_getText = AccessTools.Method("DistantWorlds.Types.TextResolver:GetText");

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:ResolveOrbTypeDescription"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(ResolveOrbTypeDescriptionTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.GalactopediaTopicList:GenerateTopicsForRelatedItems"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(GenerateTopicsForRelatedItemsTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Location:GetLocationTypeAndSizeDescription"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(GetLocationTypeAndSizeDescriptionTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.Types.Orb"), x =>
					x.Name.Contains("DoExploration") && x.GetParameters().Length > 7),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DoExplorationTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.Types.PrioritizedTarget"), x =>
					x.Name.Contains("DrawItem")),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DrawItemTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.Types.TextHelper"), x =>
					x.Name.Contains("ResolveMissionDescription") && x.GetParameters().Length > 6),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(ResolveMissionDescriptionTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.Types.UserInterfaceHelper"), x =>
					x.Name.Contains("DrawColonySummary") && !x.GetParameters()[1].ParameterType.Name.Contains("Orb")),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DrawColonySummaryTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.UserInterfaceHelper:DrawConstructionYardSummary"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DrawConstructionYardSummaryTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.UserInterfaceHelper:DrawOrbHover"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DrawOrbHoverTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.UserInterfaceHelper:DrawSystemCartouche"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DrawSystemCartoucheTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.ScrollablePanel:RenderColonyDetail"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(RenderColonyDetailTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.SelectionPanel:RenderOrb"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(RenderOrbTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.UserInterfaceHelper:DrawOrbCartouche"),
				null, null,
				new HarmonyMethod(typeof(RemoveStarPostfixPatch), nameof(DrawOrbCartoucheTranspiler)));
		}

		private static IEnumerable<CodeInstruction> ResolveOrbTypeDescriptionTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var count = 0;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					count++;
					if (count == 1)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Pop);
						continue;
					}
					else
					{
						instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
					}
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateTopicsForRelatedItemsTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var start = false;
			var count = 0;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str &&
				    str == "Asteroid")
				{
					start = true;
				}
				else if (start && instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					count++;
					if (count == 1 || count == 3)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Pop);
						continue;
					}
					else if (count == 2)
					{
						instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
					}
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GetLocationTypeAndSizeDescriptionTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> DoExplorationTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> DrawItemTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var isFirst = true;
			foreach (var instruction in instructions)
			{
				if (isFirst && instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					isFirst = false;
					instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> ResolveMissionDescriptionTranspiler(IEnumerable<CodeInstruction> instructions)
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
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method2 &&
				         method2.Name == "Concat" &&
				         method2.GetParameters().Length == 1 &&
				         method2.GetParameters()[0].ParameterType.IsArray)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatManyStringWithoutSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> DrawColonySummaryTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var isFirst = true;
			foreach (var instruction in instructions)
			{
				if (isFirst && instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					isFirst = false;
					instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> DrawConstructionYardSummaryTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var isFirst = true;
			foreach (var instruction in instructions)
			{
				if (isFirst && instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					isFirst = false;
					instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> DrawOrbHoverTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var count = 0;
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
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method2 &&
				         method2.Name == "Concat" &&
				         method2.GetParameters().Length == 1 &&
				         method2.GetParameters()[0].ParameterType.IsArray)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatManyStringWithoutSpace));
				}
				yield return instruction;
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method3 &&
				    method3.Name == "ResolveDescription")
				{
					count++;
					if (count == 2)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Ldstr, "");
					}
				}
			}
		}

		private static IEnumerable<CodeInstruction> DrawSystemCartoucheTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var removeNextConcat = false;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					if (removeNextConcat)
					{
						instruction.operand = AccessTools.Method(typeof(TextHelper),
							nameof(TextHelper.ConcatThreeStringWithoutSpace));
						removeNextConcat = false;
					}
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method3 &&
				         method3.Name == "ResolveDescription")
				{
					removeNextConcat = true;
					instruction.operand =
						AccessTools.Method(typeof(RemoveStarPostfixPatch), nameof(GetStarDescription));
				}

				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> RenderColonyDetailTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var isFirst = true;
			foreach (var instruction in instructions)
			{
				if (isFirst && instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					isFirst = false;
					instruction.operand = AccessTools.Method(typeof(TextHelper), nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> RenderOrbTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var removeNextConcat = false;
			var isFirst = true;
			var isFirstLdcI4 = true;
			var count = 0;
			CodeInstruction lastCode = null;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					if (removeNextConcat)
					{
						instruction.operand = AccessTools.Method(typeof(TextHelper),
							nameof(TextHelper.ConcatThreeStringWithoutSpace));
						removeNextConcat = false;
					}
				}
				else if (isFirst && instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method2 &&
				         method2.Name == "Concat" &&
				         method2.GetParameters().Length == 1 &&
				         method2.GetParameters()[0].ParameterType.IsArray)
				{
					isFirst = false;
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatManyStringWithoutSpace));
				} 
				else if (isFirstLdcI4 && instruction.opcode == OpCodes.Ldc_I4_4)
				{
					// ReSharper disable once PossibleNullReferenceException
					if (lastCode.opcode == OpCodes.Ldfld)
					{
						isFirstLdcI4 = false;
						yield return new CodeInstruction(OpCodes.Dup);
						instruction.opcode = OpCodes.Call;
						instruction.operand = AccessTools.Method(typeof(RemoveStarPostfixPatch), nameof(IsNotStar));
					}
				}
				else if (instruction.opcode == OpCodes.Call &&
				           instruction.operand is MethodInfo method3 &&
				           method3.Name == "ResolveDescription")
				{
					count++;
					if (count > 1)
					{
						removeNextConcat = true;
					}
				}

				lastCode = instruction;
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> DrawOrbCartoucheTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var removeNextConcat = false;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					if (removeNextConcat)
					{
						instruction.operand = AccessTools.Method(typeof(TextHelper),
							nameof(TextHelper.ConcatThreeStringWithoutSpace));
						removeNextConcat = false;
					}
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method3 &&
				         method3.Name == "ResolveDescription")
				{
					removeNextConcat = true;
					instruction.operand =
						AccessTools.Method(typeof(RemoveStarPostfixPatch), nameof(GetStarDescription));
				}
				
				yield return instruction;
			}
		}

		public static int IsNotStar(int type)
		{
			switch (type)
			{
				case 1:
				case 4:
					return type;
				default:
					return -1;
			}
		}

		public static string GetStarDescription(int type)
		{
			switch (type)
			{
				case 2:
					return _planetName ?? (_planetName = (string) _getText.Invoke(null, new object[] {"Planet"}));
				case 3:
					return _moonName ?? (_moonName = (string)_getText.Invoke(null, new object[] { "Moon" }));
				default:
					return string.Empty;
			}
		}
	}
}