using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class SituationDescriptionPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Empire:GenerateSituationDescription"),
				null, null,
				new HarmonyMethod(typeof(SituationDescriptionPatch), nameof(GenerateSituationDescriptionTranspiler)));
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