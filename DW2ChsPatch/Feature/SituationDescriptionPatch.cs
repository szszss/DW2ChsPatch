using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class SituationDescriptionPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Empire:GenerateSituationDescription"),
				null, null,
				new HarmonyMethod(typeof(SituationDescriptionPatch), nameof(GenerateSituationDescriptionTranspilerNew)));
		}

		private static IEnumerable<CodeInstruction> GenerateSituationDescriptionTranspilerNew(
			IEnumerable<CodeInstruction> instructions)
		{
			int index = 0;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo method &&
				    method.Name == "Concat")
				{
					switch (method.GetParameters().Length)
					{
						case 1:
							if (method.GetParameters()[0].ParameterType == typeof(string[]))
								instruction.operand = AccessTools.Method(typeof(SituationDescriptionPatch), nameof(ConcatMany));
							break;
						case 2:
							yield return new CodeInstruction(OpCodes.Ldc_I4, index);
							instruction.operand = AccessTools.Method(typeof(SituationDescriptionPatch), nameof(Concat2));
							index++;
							break;
						case 3:
							yield return new CodeInstruction(OpCodes.Ldc_I4, index);
							instruction.operand = AccessTools.Method(typeof(SituationDescriptionPatch), nameof(Concat3));
							index += 2;
							break;
						case 4:
							yield return new CodeInstruction(OpCodes.Ldc_I4, index);
							instruction.operand = AccessTools.Method(typeof(SituationDescriptionPatch), nameof(Concat4));
							index += 3;
							break;
					}
				}
				yield return instruction;
			}
		}

		public static string ConcatMany(string[] strs)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < strs.Length; i++)
			{
				sb.Append(i == 0 ? strs[i] : Translate(strs[i]));
			}
			return sb.ToString();
		}

		public static string Concat2(string str1, string str2, int index)
		{
			return str1 + Translate(str2, index);
		}

		public static string Concat3(string str1, string str2, string str3, int index)
		{
			return str1 + Translate(str2, index) + Translate(str3, index + 1);
		}

		public static string Concat4(string str1, string str2, string str3, string str4, int index)
		{
			return str1 + Translate(str2, index) + Translate(str3, index + 1) + Translate(str4, index + 2);
		}

		private static string AssertString(string str, int index, string assumeStr, string newStr)
		{
			return str == assumeStr ? newStr : str;
		}

		private static string Translate(string str, int index = 0)
		{
			switch (index)
			{
				// don't try to translate those
				case 1: // faction name
				case 4: // government name
				case 11: // race name
				case 14: // race behavour
				case 18: // race name
				case 20: // race bonus
				case 32: // leader name
				case 34: // leader trait
				case 42: // homeworld name
				case 46: // homeworld biome
				case 48: // homeworld type
				case 50: // homeworld system
				case 55: // neiborhood system
				case 76: // colony name
				case 100: // independent colony name
				case 106: // other faction name
				case 109: // other faction name and desc
					return str;
				case 5:
					return AssertString(str, index, ".", "政府统治。");
				case 15:
					return AssertString(str, index, " and ", "并且");
				case 17:
					return AssertString(str, index, ". ", "而著称于世。");
				case 101:
					return AssertString(str, index, ")", "独立殖民地）");
				case 98:
					return AssertString(str, index, ".\n\n", "交战。\n\n");
				case 112:
					return AssertString(str, index, ".\n\n", "建交。\n\n");
				case 22:
				case 78:
				case 111:
					return AssertString(str, index, ", ", "、");
			}
			var newStr = str switch
			{
				"Our faction is known as the " => "我们的势力名叫",
				" Our government is " => "由一个",
				"We are the " => "我们是",
				"who are typically " => "以",
				" have natural skills in " => "天生善于",
				"We also have " => "我们还拥有",
				" amongst our population." => "人口",
				"Our leader is " => "我们的领导人是",
				", skilled in " => "，精于",
				"Our home colony is " => "我们的首都坐落在",
				", an " => "，一颗",
				", a " => "，一颗",
				" in the " => "位于",
				" system" => "星系",
				"Nearby is " => "毗邻着",
				"Nearby are " => "毗邻着",
				"Also nearby is " => "还毗邻着",
				"Also nearby are " => "还毗邻着",
				"We also have another colony: " => "我们还拥有另一座殖民地：",
				" other colonies: " => "座殖民地：",
				" other colonies." => "座殖民地。",
				"Our faction has primitive technology. We are just beginning to take our first steps into space." => 
					"我们的势力刚刚拥有最原始的宇航科技，我们刚刚踏上探索太空的第一步。",
				"Our faction has basic space-faring technology and we are exploring our home system." => 
					"我们的势力拥有基本的太空航行科技，已经开始着手探索我们的母星系。",
				"Our level of technology is expanding, allowing us to explore beyond our home system." => 
					"我们在航天科技上已经取得一些进展，拥有探索外星系的能力。",
				"We have much advanced technology and continue to research further." => 
					"我们已经拥有许多先进科技，尽管仍有一些领域是我们尚未涉及的。",
				"We have high levels of scientific knowledge, providing our faction with advanced technology." => 
					"我们拥有丰富的科学知识，掌握着大量顶尖科技。",
				"We are at war with " => "我们正在和",
				"We are in contact with " => "我们已同",
				" (an independent colony of " => "（一座",
				" (a " => "（一个",
				" pirate faction)" => "海盗势力）",
				" faction)" => "主权国家）",
				"Explore, Expand and Conquer!" => "探索、扩张、征服！",
				" and " => "以及",
				"." => "。",
				". " => "。",
				", " => "，",
				"the" => "",
				"the " => "",
				".\n\n" => "。\n\n",
				_ => str
			};
			return newStr;
		}

		private static IEnumerable<CodeInstruction> GenerateSituationDescriptionTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var dotCount = 0;
			var dot2Count = 0;
			var dot3Count = 0;
			var dot4Count = 0;
			var andCount = 0;
			var wahCount = 0;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Our faction is known as the ": newStr = "我们的势力名叫"; break;
						case " Our government is ": newStr = "由一个"; break;
						case "We are the ": newStr = "我们是"; break;
						case "who are typically ": newStr = "以"; break;
						case " have natural skills in ": newStr = "天生善于"; break;
						case "We also have ":
							wahCount++;
							switch (wahCount)
							{
								case 1:
									newStr = "此外，我们的势力还有大量的";
									break;
								default:
									newStr = "我们还拥有";
									break;
							}
							break;
						case " amongst our population.": newStr = "人口"; break;
						case "Our leader is ": newStr = "我们的领导人是"; break;
						case ", skilled in ": newStr = "，精于"; break;
						case "Our home colony is ": newStr = "我们的首都坐落在"; break;
						case ", an ": newStr = "，一颗"; break;
						case ", a ": newStr = "，一颗"; break;
						case " in the ": newStr = "位于"; break;
						case " system": newStr = "星系"; break;
						case "the": newStr = ""; break;
						case "the ": newStr = ""; break;
						case "Nearby is ": newStr = "毗邻着"; break;
						case "Nearby are ": newStr = "毗邻着"; break;
						case "Also nearby is ": newStr = "还毗邻着"; break;
						case "Also nearby are ": newStr = "还毗邻着"; break;
						case "We also have another colony: ": newStr = "我们还拥有另一座殖民地："; break;
						case " other colonies: ": newStr = "座殖民地："; break;
						case " other colonies.": newStr = "座殖民地。"; break;
						case "Our faction has primitive technology. We are just beginning to take our first steps into space.": 
							newStr = "我们的势力刚刚拥有最原始的宇航科技，我们刚刚踏上探索太空的第一步。"; break;
						case "Our faction has basic space-faring technology and we are exploring our home system.":
							newStr = "我们的势力拥有基本的太空航行科技，已经开始着手探索我们的母星系。"; break;
						case "Our level of technology is expanding, allowing us to explore beyond our home system.":
							newStr = "我们在航天科技上已经取得一些进展，拥有探索外星系的能力。"; break;
						case "We have much advanced technology and continue to research further.":
							newStr = "我们已经拥有许多先进科技，尽管仍有一些领域是我们尚未涉及的。"; break;
						case "We have high levels of scientific knowledge, providing our faction with advanced technology.":
							newStr = "我们拥有丰富的科学知识，掌握着大量顶尖科技。"; break;
						case "We are at war with ": newStr = "我们正在和"; break;
						case "We are in contact with ": newStr = "我们已同"; break;
						case " (an independent colony of ": newStr = "（一座"; break;
						case ")": newStr = "独立殖民地）"; break;
						case " (a ": newStr = "（一个"; break;
						case " pirate faction)": newStr = "海盗势力）"; break;
						case " faction)": newStr = "主权国家）"; break;
						case "Explore, Expand and Conquer!": newStr = "探索、扩张、征服！"; break;
						case " and ":
							andCount++;
							switch (andCount)
							{
								case 1:
									newStr = "并且";
									break;
								default:
									newStr = "以及";
									break;
							}
							break;
						case ".":
							dotCount++;
							switch (dotCount)
							{
								case 2:
									newStr = "政府统治。";
									break;
								default:
									newStr = "。";
									break;
							}
							break;
						case ". ":
							dot2Count++;
							switch (dot2Count)
							{
								case 2:
									newStr = "而著称于世。";
									break;
								default:
									newStr = "。";
									break;
							}
							break;
						case ", ":
							dot3Count++;
							switch (dot3Count)
							{
								case 2:
								case 3:
								case 4:
								case 5:
								case 6:
									newStr = "、";
									break;
								default:
									newStr = "，";
									break;
							}
							break;
						case ".\n\n":
							dot4Count++;
							switch (dot4Count)
							{
								case 1:
									newStr = "交战。\n\n";
									break;
								case 2:
									newStr = "建交。\n\n";
									break;
								default:
									newStr = "。\n\n";
									break;
							}
							break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
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
	}
}