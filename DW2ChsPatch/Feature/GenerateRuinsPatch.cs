using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class GenerateRuinsPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.Types.PlanetaryFacilityDefinition"), 
					x => x.Name.Contains("GenerateRuins") && x.GetParameters().Length > 6),
				null, null,
				new HarmonyMethod(typeof(GenerateRuinsPatch), nameof(GenerateRuinsTranspiler)));
		}

		private static IEnumerable<CodeInstruction> GenerateRuinsTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Hall":
							newStr = "大厅";
							break;
						case "Temple":
							newStr = "神殿";
							break;
						case "Pyramid":
							newStr = "金字塔";
							break;
						case "Citadel":
							newStr = "城堡";
							break;
						case "Fortress":
							newStr = "要塞";
							break;
						case "Tower":
							newStr = "高塔";
							break;
						case "Tomb":
							newStr = "古墓";
							break;
						case "Sanctuary":
							newStr = "圣所";
							break;
						case "Library":
							newStr = "图书馆";
							break;
						case "Palace":
							newStr = "宫殿";
							break;
						case "Archives":
							newStr = "档案馆";
							break;
						case "Monastery":
							newStr = "修道院";
							break;
						case "Retreat":
							newStr = "隐居所";
							break;
						case "Nexus":
							newStr = "枢纽";
							break;
						case "Chamber":
							newStr = "密室";
							break;
						case "Pillar":
							newStr = "台柱";
							break;
						case "Obelisk":
							newStr = "方尖石塔";
							break;
						case "Gate":
							newStr = "大门";
							break;
						case "Shrine":
							newStr = "神社";
							break;

						case "Hidden":
							newStr = "隐秘之";
							break;
						case "Great":
							newStr = "伟大之";
							break;
						case "Grand":
							newStr = "宏大之";
							break;
						case "Forgotten":
							newStr = "被遗忘的";
							break;
						case "Granite":
							newStr = "不朽之";
							break;
						case "Lofty":
							newStr = "巍峨之";
							break;
						case "High":
							newStr = "高耸之";
							break;
						case "Exalted":
							newStr = "华丽之";
							break;
						case "Stone":
							newStr = "砖石";
							break;
						case "Secluded":
							newStr = "僻静之";
							break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(GenerateRuinsPatch),
						nameof(RuinNameConcat3));
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method2 &&
				         method2.Name == "Concat" &&
				         method2.GetParameters().Length == 2)
				{
					instruction.operand = AccessTools.Method(typeof(GenerateRuinsPatch),
						nameof(RuinNameConcat2));
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method3 &&
				         method3.Name == "Concat" &&
				         method3.GetParameters().Length == 1 &&
				         method3.GetParameters()[0].ParameterType.IsArray)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatManyStringWithoutSpace));
				}

				yield return instruction;
			}
		}

		public static string RuinNameConcat2(string s1, string s2)
		{
			if (s1 == "of ")
				return s2 + "的";
			return s1 + s2;
		}

		public static string RuinNameConcat3(string s1, string s2, string s3)
		{
			if (s1 == "of the ")
				return s2 + "的";
			if (s3 == " ")
				return s2;
			if (s2 == " ")
				return s3 + s1;
			return TextHelper.ConcatThreeStringWithoutSpace(s1, s2, s3);
		}
	}
}