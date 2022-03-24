using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class RandomNamePatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:GenerateRandomNameLong"),
				new HarmonyMethod(typeof(RandomNamePatch), nameof(RandomLongPrefix)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:GenerateRandomNameShort"),
				new HarmonyMethod(typeof(RandomNamePatch), nameof(RandomShortPrefix)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:GenerateBlackHoleName"),
				null, null,
				new HarmonyMethod(typeof(RandomNamePatch), nameof(GenerateBlackHoleNameTranspiler)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:GenerateEmpireName"),
				null, null,
				new HarmonyMethod(typeof(RandomNamePatch), nameof(GenerateEmpireNameTranspiler)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateSystemName"),
				null, null,
				new HarmonyMethod(typeof(RandomNamePatch), nameof(GenerateSystemNameTranspiler)));

			var effectType = AccessTools.TypeByName("DistantWorlds.Types.LocationEffectGroupDefinition");
			var galacticStormNames = AccessTools.Field(effectType, "GalacticStormNames");
			var gravityStormNames = AccessTools.Field(effectType, "GravityStormNames");
			var gravityStormMinorNames = AccessTools.Field(effectType, "GravityStormMinorNames");
			var ionStormNames = AccessTools.Field(effectType, "IonStormNames");
			var radiationZoneNames = AccessTools.Field(effectType, "RadiationZoneNames");

			galacticStormNames?.SetValue(null, new[]
			{
				"{0}大风暴",
				"{0}大漩涡",
			});

			gravityStormNames?.SetValue(null, new[]
			{
				"{0}陷阱",
				"{0}之坑",
				"{0}之潭",
				"{0}迷宫",
				"{0}之陷",
			});

			gravityStormMinorNames?.SetValue(null, new[]
			{
				"{0}扭曲带",
				"{0}裂隙带",
				"{0}裂口带",
				"{0}裂痕带",
				"{0}断裂带",
			});

			ionStormNames?.SetValue(null, new[]
			{
				"{0}放电区",
				"{0}电离区",
				"{0}湍流区",
				"{0}扰乱区",
			});

			radiationZoneNames?.SetValue(null, new[]
			{
				"{0}荒原",
				"{0}荒地",
				"{0}废土",
			});
		}

		private static string GenerateRandomNameDo(Random rnd, int minLength)
		{
			var sb = new StringBuilder();
			var lastChar = "";

			for (int i = 0; i < minLength; i++)
			{
				var nextChar = "";
				if (i == 0)
					nextChar = _randomNameStart[rnd.Next(_randomNameStart.Length)];
				else if (i == minLength - 1) // it's last char
				{
					for (int j = 0; j < 10; j++)
					{
						nextChar = _randomNameEnd[rnd.Next(_randomNameEnd.Length)];
						if (nextChar != lastChar)
							break;
					}
				}
				else
				{
					for (int j = 0; j < 10; j++)
					{
						nextChar = _randomNameMid[rnd.Next(_randomNameMid.Length)];
						if (nextChar != lastChar)
							break;
					}
				}

				lastChar = nextChar;
				sb.Append(nextChar);
				i += nextChar.Length - 1;
			}

			return sb.ToString();
		}

		private static bool RandomLongPrefix(Random __0, int __1 /* minimumLength */, out string __result)
		{
			var min = Math.Max(__1 / 2, 1);
			var max = Math.Max(__1 * 2 / 3, min + 1);
			min = __0.Next(min, max);
			__result = GenerateRandomNameDo(__0, min);
			return false;
		}

		private static bool RandomShortPrefix(Random __0, int __1 /* maximumLength */, out string __result)
		{
			var min = Math.Min(__1, 2);
			var max = Math.Max(__1 * 3 / 5, min + 1);
			min = __0.Next(min, max);
			__result = GenerateRandomNameDo(__0, min);
			return false;
		}

		private static IEnumerable<CodeInstruction> GenerateEmpireNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Bloody": newStr = "血腥"; break;
						case "Dread": newStr = "恐怖"; break;
						case "Black": newStr = "黑色"; break;
						case "Dirty": newStr = "肮脏"; break;
						case "Evil": newStr = "邪恶"; break;
						case "Iron": newStr = "钢铁"; break;
						case "Red": newStr = "红色"; break;
						case "Fierce": newStr = "残暴"; break;
						case "Cruel": newStr = "冷酷"; break;
						case "Sinister": newStr = "凶残"; break;
						case "Vicious": newStr = "恶毒"; break;
						case "Lone": newStr = "孤独"; break;
						case "Savage": newStr = "野蛮"; break;
						case "Fearsome": newStr = "骇人"; break;
						case "Deadly": newStr = "致命"; break;
						case "Venomous": newStr = "歹毒"; break;
						case "Murderous": newStr = "残忍"; break;
						case "Dark": newStr = "黑暗"; break;
						case "Grim": newStr = "冷酷"; break;
						case "Haunted": newStr = "魔鬼"; break;
						case "Menacing": newStr = "险恶"; break;
						case "Blood": newStr = "血色"; break;
						case "Burning": newStr = "烈焰"; break;
						case "Hidden": newStr = "神隐"; break;
						case "Fire": newStr = "火焰"; break;
						case "Lost": newStr = "失落"; break;

						case "Sun": newStr = "之日"; break;
						case "Star": newStr = "之星"; break;
						case "Rock": newStr = "之石"; break;
						case "Moon": newStr = "之月"; break;
						case "Storm": newStr = "风暴"; break;
						case "Fang": newStr = "獠牙"; break;
						case "Claw": newStr = "之爪"; break;
						case "Dagger": newStr = "之刃"; break;

						case "Pirates": newStr = "海盗"; break;
						case "Marauders": newStr = "掠夺者"; break;
						case "Bandits": newStr = "匪帮"; break;
						case "Raiders": newStr = "劫掠者"; break;
						case "Buccaneers": newStr = "盗匪"; break;
						case "Outlaws": newStr = "歹徒"; break;
						case "Corsairs": newStr = "海贼"; break;
						case "Pillagers": newStr = "强盗"; break;
						case "Gangsters": newStr = "贼团"; break;
						case "Ravagers": newStr = "破坏者"; break;
						case "Prowlers": newStr = "盗贼团"; break;
						case "Intruders": newStr = "闯入者"; break;
						case "Invaders": newStr = "入侵者"; break;
						case "Skyjackers": newStr = "拦路贼"; break;
						case "Gang": newStr = "帮"; break;
						case "Mercenaries": newStr = "佣兵团"; break;
						case "Council": newStr = "议会"; break;
						case "Network": newStr = "网络"; break;
						case "League": newStr = "联盟"; break;
						case "Force": newStr = "武装部队"; break;
						case "Clan": newStr = "氏族"; break;
						case "Authority": newStr = "政权"; break;
						case "Confederacy": newStr = "同盟"; break;
						case "Confederation": newStr = "邦联"; break;
						case "Security": newStr = "安保"; break;
						case "Warriors": newStr = "战帮"; break;
						case "Army": newStr = "军团"; break;
						case "Fleet": newStr = "舰队"; break;
						case "Horde": newStr = "游牧团"; break;
						case "Corporation": newStr = "公司"; break;
						case "Consortium": newStr = "财团"; break;
						case "Syndicate": newStr = "辛迪加"; break;
						case "Cartel": newStr = "卡特尔"; break;
						case "Guild": newStr = "公会"; break;

						case "United": newStr = "统一"; break;
						case "Combined": newStr = "联合"; break;
						case "Imperial": newStr = "帝制"; break;
						case "Great": newStr = "大"; break;
						case "Grand": newStr = "大"; break;

						case "Empire": newStr = "帝国"; break;
						case "Alliance": newStr = "联盟"; break;
						case "Group": newStr = "会"; break;
						case "Dominion": newStr = "领"; break;
						case "Territory": newStr = "领"; break;
						case "Nation": newStr = "国"; break;
						case "Realm": newStr = "领"; break;
						case "Federation": newStr = "联邦"; break;
						case "Enclave": newStr = "飞地领"; break;
						case "Coalition": newStr = "联合体"; break;
						case "Domain": newStr = "领"; break;
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

		private static IEnumerable<CodeInstruction> GenerateBlackHoleNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Devil's": newStr = "恶魔"; break;
						case "Dark": newStr = "黑暗"; break;
						case "Ravenous": newStr = "吞噬"; break;
						case "Deadly": newStr = "致命"; break;
						case "Perilous": newStr = "危险"; break;
						case "Traitor's": newStr = "叛徒"; break;
						case "Wretched": newStr = "悲惨"; break;
						case "Devouring": newStr = "饕餮"; break;
						case "Destroyer's": newStr = "毁灭"; break;

						case "Gate": newStr = "之门"; break;
						case "Vortex": newStr = "漩涡"; break;
						case "Whirlpool": newStr = "风暴"; break;
						case "Wheel": newStr = "之轮"; break;
						case "Lair": newStr = "之巢"; break;
						case "Snare": newStr = "之蔑"; break;
						case "Desolation": newStr = "之弃"; break;
						case "End": newStr = "终焉"; break;
						case "Mouth": newStr = "之口"; break;
						case "Cauldron": newStr = "之炉"; break;
						case "Pit": newStr = "之坑"; break;
						case "Abyss": newStr = "深渊"; break;
						case "Chasm": newStr = "隙间"; break;
						case "Dungeon": newStr = "地牢"; break;
						case "Inferno": newStr = "炼狱"; break;
						case "Void": newStr = "虚空"; break;
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
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateSystemNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Major": newStr = "大"; break;
						case "Minor": newStr = "小"; break;
						case "Prime": newStr = "主"; break;
						case "Junction": newStr = "伴"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(RandomNamePatch), nameof(ConcatSystemAlternativeName));
				}
				yield return instruction;
			}
		}

		public static string ConcatSystemAlternativeName(string s1, string _, string s3)
		{
			return s3 + s1;
		}

		private static string[] _randomNameStart = new[]
		{
			"阿", "厄", "伊", "俄", "宇", "埃", "奥", "欧", "乌", "安", "恩", "翁",
			"布", "巴", "柏", "比", "玻", "彼", "拜", "班", "本",
			"格", "伽", "戈",
			"德", "达", "狄", "多", "底", "道", "杜", "丹", "冬",
			"兹",
			"特", "塔", "忒", "提", "托", "堤", "泰", "陶", "图", "坦", "廷",
			"克", "卡", "喀", "科", "库", "考", "坎", "肯", "孔",
			"拉", "勒", "利", "罗", "吕", "莱", "劳", "琉", "兰", "隆",
			"马", "墨", "弥", "摩", "密", "穆", "曼", "门", "明", "蒙",
			"那", "涅", "尼", "诺", "奈", "努", "宁",
			"普", "帕", "珀", "庇", "波", "皮", "浦", "蓬",
			"瑞", "里", "洛", "戎",
			"斯", "萨", "塞", "索", "修", "苏",
			"法", "菲", "费", "芬", "丰",
			"赫", "希", "荷", "胡", "亨",
			"贵", "戈",
			"雅", "伊", "宇", "埃", "英",
			"逵", "奎", "库", "考", "昆", "孔",
			"乌", "瓦", "威", "维", "沃", "宇", "翁"
		};

		private static string[] _randomNameMid = new[]
		{
			"阿", "厄", "伊", "俄", "宇", "埃", "奥", "欧", "乌", "安", "恩", "印", "翁",
			"布", "巴", "柏", "比", "拜", "班", "本", "朋",
			"格", "伽", "革", "戈", "古", "高", "根",
			"德", "达", "狄", "多", "底", "代", "道", "丢", "丹", "顿",
			"兹", "佐", "赞", "晋",
			"特", "塔", "忒", "提", "托", "堤", "泰", "陶", "图", "坦", "廷",
			"克", "卡", "喀", "科", "库", "坎", "肯", "孔",
			"尔", "拉", "勒", "利", "罗", "吕", "莱", "劳", "琉", "路", "兰", "林", "隆",
			"马", "墨", "弥", "摩", "密", "迈", "曼", "门",
			"恩", "涅", "尼", "诺", "奈", "纽", "努", "宁", "农",
			"克斯", "克萨", "克塞", "克西", "克索", "克赛", "克骚", "克修", "克苏", "克森", "克辛",
			"普", "帕", "珀", "庇", "波", "皮", "派", "浦", "潘", "彭",
			"瑞", "里", "洛", "律", "柔", "戎",
			"斯", "萨", "塞", "索", "修", "苏", "辛",
			"佛", "法", "斐", "菲", "福", "费", "芬",
			"普斯", "普萨", "普塞", "普西", "普索", "普绪", "普赛", "普修", "普苏", "普珊", "普森", "普辛", "普宋",
			"赫", "哈", "赫", "希", "荷", "许",
			"戈",
			"依", "雅", "伊", "英",
			"奎", "库", "考", "昆",
			"瓦", "威", "维", "沃", "宇"
		};

		private static string[] _randomNameEnd = new[]
		{
			"阿", "欧", "乌", "安", "恩", "翁",
			"巴", "柏", "比", "拜", "班", "本",
			"格", "伽", "戈", "古", "艮",
			"德", "达", "狄", "多", "底", "道", "丹", "顿",
			"兹",
			"塔", "忒", "提", "托", "泰", "陶", "图", "坦", "廷",
			"克", "卡", "刻", "喀", "科", "肯",
			"拉", "勒", "罗", "隆",
			"弥", "曼", "门",
			"涅", "尼", "诺",
			"克斯", "克西", "克辛",
			"帕", "庇", "波",
			"里", "洛",
			"斯", "萨", "西", "索", "修", "苏",
			"菲",
			"普斯", "普西", "普辛",
			"希",
			"库",
			"乌", "瓦", "威", "沃",
			"尼亚", "尼亚", "尼亚"
		};
	}
}