using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class RandomShipNamePatch
	{
		private static string _shipPostfix = "";

		public static void Patch(Harmony harmony, string shipPostfix)
		{
			_shipPostfix = shipPostfix;

			HarmonyMethod shipnamePostfix = string.IsNullOrEmpty(shipPostfix)
				? null
				: new HarmonyMethod(typeof(RandomShipNamePatch), nameof(AddShipNamePostfix));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateShipName"),
				null, null,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GenerateShipNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GeneratePirateBaseName"),
				null, null,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GeneratePirateBaseTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateResortBaseName"),
				null, null,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GenerateResortBaseNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateRandomHiveShipName"),
				null, null,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GenerateRandomHiveShipNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateRandomPlanetDestroyerName"),
				null, null,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GenerateRandomPlanetDestroyerNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateRandomUniqueMilitaryShipName"),
				null, shipnamePostfix,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GenerateRandomUniqueMilitaryShipNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateRandomUniqueCivilianShipName"),
				null, shipnamePostfix,
				new HarmonyMethod(typeof(RandomShipNamePatch), nameof(GenerateRandomUniqueCivilianShipNameTranspiler)));
		}

		private static IEnumerable<CodeInstruction> GenerateShipNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						//case "Spaceport": newStr = "太空港"; break;
						case "Weapons Platform": newStr = "武器平台"; break;
						case "Defense Battery": newStr = "防御炮台"; break;
						case "Orbital Battery": newStr = "轨道炮台"; break;
						case "Research Center": newStr = "研究中心"; break;
						case "Station": newStr = "站点"; break;
						case "Research Station": newStr = "科研站"; break;
						case "Research Facility": newStr = "研究设施"; break;
						case "Beacon": newStr = "灯塔"; break;
						case "Sentinel": newStr = "哨站"; break;
						case "Monitoring Facility": newStr = "监听设施"; break;
						case "Interceptor": newStr = "拦截机"; break;
						case "Bomber": newStr = "轰炸机"; break;
						
					}

					instruction.operand = newStr;
				}
				/*else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatThreeStringWithoutSpace));
				}*/
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GeneratePirateBaseTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Secret": newStr = "秘密"; break;
						case "Eagles": newStr = "鹰犬"; break;
						case "Villainous": newStr = "邪恶"; break;
						case "Brigands": newStr = "土匪"; break;
						case "Outlaws": newStr = "法外"; break;
						case "Fugitives": newStr = "亡命徒"; break;
						case "Desperado": newStr = "暴徒"; break;
						case "Secluded": newStr = "避世"; break;
						case "Bounty Hunters": newStr = "好汉"; break;
						case "Lonely": newStr = "孤独"; break;
						case "Gamblers": newStr = "赌徒"; break;
						case "Bandits": newStr = "匪徒"; break;
						case "Smugglers": newStr = "走私贩"; break;

						case "Lair": newStr = "巢穴"; break;
						case "Base": newStr = "基地"; break;
						case "Hideout": newStr = "藏身处"; break;
						case "Retreat": newStr = "隐居所"; break;
						case "Fortress": newStr = "要塞"; break;
						case "Cave": newStr = "洞穴"; break;
						case "Cove": newStr = "河湾"; break;
						case "Outpost": newStr = "前哨"; break;
						case "Den": newStr = "之窝"; break;
						case "Haunt": newStr = "聚集处"; break;
						case "Hideaway": newStr = "隐蔽处"; break;
						case "Nest": newStr = "之巢"; break;
						case "Sanctuary": newStr = "避难所"; break;
						case "Refuge": newStr = "之家"; break;
						case "Shelter": newStr = "居所"; break;
						case "Haven": newStr = "天堂"; break;
						case "End": newStr = "山寨"; break;
						case "Rest": newStr = "休息处"; break;
						case "Station": newStr = "站点"; break;
						case "Stronghold": newStr = "要塞"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateResortBaseNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
					instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Royal": newStr = "皇家"; break;
						case "Holiday": newStr = "假日"; break;
						case "Luxury": newStr = "奢侈"; break;
						case "Grand": newStr = "盛大"; break;
						case "Horizon": newStr = "海天"; break;

						case "Resort": newStr = "度假村"; break;
						case "Hotel": newStr = "酒店"; break;
						case "Encounter": newStr = "会场"; break;
						case "Casino": newStr = "赌场"; break;
						case "Retreat": newStr = "隐居处"; break;
						case "Stopover": newStr = "歇脚处"; break;
						case "Lounge": newStr = "休息所"; break;
						case "Lodge": newStr = "会所"; break;
						case "Club": newStr = "俱乐部"; break;
						case "Palace": newStr = "宫殿"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
						 instruction.operand is MethodInfo method &&
						 method.Name == "Concat" &&
						 method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateRandomHiveShipNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Immortal": newStr = "不朽"; break;
						case "Merciless": newStr = "无情"; break;
						case "Dark": newStr = "遮天"; break;
						case "Raging": newStr = "狂怒"; break;
						case "Bitter": newStr = "好斗"; break;
						case "Angry": newStr = "愤怒"; break;
						case "Deadly": newStr = "致命"; break;
						case "Shattering": newStr = "噬杀"; break;

						case "Swarm": newStr = "虫群"; break;
						case "Horde": newStr = "蜂巢"; break;
						case "Hive": newStr = "虫巢"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateRandomPlanetDestroyerNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "World": newStr = "世界"; break;
						case "Planet": newStr = "行星"; break;

						case "Annihilator": newStr = "歼灭者"; break;
						case "Destroyer": newStr = "摧毁者"; break;
						case "Killer": newStr = "杀手"; break;
						case "Shatterer": newStr = "粉碎者"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(TextHelper),
						nameof(TextHelper.ConcatThreeStringWithoutMidSpace));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateRandomUniqueMilitaryShipNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Grievous": newStr = "悲哀"; break;
						case "Prime": newStr = "擎天"; break;
						case "Deadly": newStr = "致命"; break;
						case "Grand": newStr = "宏大"; break;
						case "Black": newStr = "黑色"; break;
						case "Swift": newStr = "敏捷"; break;
						case "Mighty": newStr = "浩瀚"; break;
						case "Dreadful": newStr = "可恶"; break;
						case "Crushing": newStr = "碾压"; break;
						case "Shattering": newStr = "粉碎"; break;
						case "Silent": newStr = "无声"; break;
						case "Dark": newStr = "黑暗"; break;
						case "Supreme": newStr = "最高"; break;
						case "Ultimate": newStr = "终焉"; break;
						case "Lethal": newStr = "致命"; break;
						case "Implacable": newStr = "不懈"; break;
						case "Immortal": newStr = "不朽"; break;
						case "Majestic": newStr = "庄严"; break;
						case "Forceful": newStr = "强力"; break;
						case "Potent": newStr = "天赋"; break;
						case "Great": newStr = "伟大"; break;
						case "Ruinous": newStr = "毁灭"; break;
						case "Sinister": newStr = "噩兆"; break;
						case "Bleak": newStr = "凄凉"; break;
						case "Grim": newStr = "冷酷"; break;
						case "Devious": newStr = "阴险"; break;
						case "Overwhelming": newStr = "无双"; break;
						case "Merciless": newStr = "无情"; break;
						case "Fearsome": newStr = "骇人"; break;
						case "Cruel": newStr = "残忍"; break;
						case "Iron": newStr = "钢铁"; break;
						case "Cunning": newStr = "狡猾"; break;
						case "Sly": newStr = "狡诈"; break;
						case "Fearless": newStr = "无畏"; break;
						case "Insidious": newStr = "埋伏"; break;
						case "Evil": newStr = "邪恶"; break;
						case "Eternal": newStr = "永恒"; break;
						case "Terrible": newStr = "恐怖"; break;
						case "Looming": newStr = "朦胧"; break;
						case "Overpowering": newStr = "强力"; break;
						case "Smashing": newStr = "粉碎"; break;
						case "Angry": newStr = "愤怒"; break;
						case "Raging": newStr = "狂怒"; break;
						case "Relentless": newStr = "不懈"; break;
						case "Intrepid": newStr = "勇气"; break;
						case "Wrathful": newStr = "怒火"; break;
						case "Bitter": newStr = "好斗"; break;
						case "Evasive": newStr = "回避"; break;
						case "Decisive": newStr = "决意"; break;
						case "Proud": newStr = "自豪"; break;
						case "Indomitable": newStr = "不屈"; break;
						case "Elusive": newStr = "无踪"; break;
						case "Inevitable": newStr = "命定"; break;
						case "Belligerent": newStr = "好战"; break;
						case "Courageous": newStr = "勇气"; break;
						case "Invincible": newStr = "无敌"; break;
						case "Shrouded": newStr = "遮蔽"; break;
						case "Growling": newStr = "咆哮"; break;
						case "Elite": newStr = "精英"; break;
						case "Final": newStr = "终极"; break;
						case "Assured": newStr = "自负"; break;
						case "Lamented": newStr = "可悲"; break;
						case "Wailing": newStr = "嚎哭"; break;
						case "Banished": newStr = "放逐"; break;
						case "Discarded": newStr = "遗弃"; break;
						case "Worthy": newStr = "可敬"; break;
						case "Desperate": newStr = "绝望"; break;
						case "Reckless": newStr = "鲁莽"; break;
						case "Fatal": newStr = "致命"; break;
						case "Hostile": newStr = "敌意"; break;
						case "Tenacious": newStr = "顽强"; break;
						case "Crimson": newStr = "猩红"; break;
						case "Red": newStr = "红色"; break;
						case "Scarlet": newStr = "绯红"; break;
						case "Formidable": newStr = "可畏"; break;

						case "Zenith": newStr = "极点"; break;
						case "Hand": newStr = "之手"; break;
						case "Vengeance": newStr = "复仇"; break;
						case "Axe": newStr = "之斧"; break;
						case "Dagger": newStr = "匕首"; break;
						case "Eclipse": newStr = "日食"; break;
						case "Moon": newStr = "之月"; break;
						case "Sun": newStr = "之日"; break;
						case "Phantom": newStr = "幻影"; break;
						case "Executioner": newStr = "刽子手"; break;
						case "Revenge": newStr = "仇恨"; break;
						case "Horizon": newStr = "地平线"; break;
						case "Star": newStr = "之星"; break;
						case "Crucible": newStr = "历练"; break;
						case "Action": newStr = "行动"; break;
						case "Devastation": newStr = "毁坏"; break;
						case "Shadow": newStr = "之影"; break;
						case "Exploit": newStr = "壮举"; break;
						case "Reprisal": newStr = "报复"; break;
						case "Surprise": newStr = "惊奇"; break;
						case "Strike": newStr = "之击"; break;
						case "Judgment": newStr = "审判"; break;
						case "Courage": newStr = "勇气"; break;
						case "Stealth": newStr = "潜行"; break;
						case "Enigma": newStr = "之谜"; break;
						case "Mystery": newStr = "谜团"; break;
						case "Fist": newStr = "之拳"; break;
						case "Death": newStr = "死亡"; break;
						case "Warrior": newStr = "勇士"; break;
						case "Assassin": newStr = "刺客"; break;
						case "Rendezvous": newStr = "交汇"; break;
						case "Fate": newStr = "命运"; break;
						case "Destiny": newStr = "天命"; break;
						case "Doom": newStr = "末日"; break;
						case "Despair": newStr = "徒劳"; break;
						case "Curse": newStr = "诅咒"; break;
						case "Thunder": newStr = "雷霆"; break;
						case "Demise": newStr = "消亡"; break;
						case "Revolution": newStr = "革命"; break;
						case "Annihilation": newStr = "歼灭"; break;
						case "Dominator": newStr = "统御"; break;
						case "Triumph": newStr = "凯旋"; break;
						case "Victory": newStr = "胜利"; break;
						case "Conquest": newStr = "征服"; break;
						case "Invader": newStr = "入侵者"; break;
						case "Downfall": newStr = "日落"; break;
						case "Chaos": newStr = "混沌"; break;
						case "Turmoil": newStr = "忧虑"; break;
						case "Anarchy": newStr = "混乱"; break;
						case "Rebellion": newStr = "反叛"; break;
						case "Sting": newStr = "刺痛"; break;
						case "Leader": newStr = "领袖"; break;
						case "Master": newStr = "大师"; break;
						case "Victor": newStr = "胜利者"; break;
						case "Assault": newStr = "突袭"; break;
						case "Cataclysm": newStr = "灾难"; break;
						case "Tyrant": newStr = "暴君"; break;
						case "Plague": newStr = "瘟疫"; break;
						case "Fury": newStr = "狂怒"; break;
						case "Justice": newStr = "正义"; break;
						case "Reckoning": newStr = "航线"; break;
						case "Emancipator": newStr = "救主"; break;
						case "Defender": newStr = "保卫者"; break;
						case "Defiance": newStr = "挑衅"; break;
						case "Liberty": newStr = "解放"; break;
						case "Retribution": newStr = "报应"; break;
						case "Adversary": newStr = "敌手"; break;
						case "Sentinel": newStr = "哨兵"; break;
						case "Sentry": newStr = "哨戒"; break;
						case "Ravager": newStr = "破坏者"; break;
						case "Subjugator": newStr = "制服者"; break;
						case "Starfall": newStr = "星陨"; break;
						case "Vigilance": newStr = "机警"; break;
						case "Starstream": newStr = "星流"; break;
						case "Inquisitor": newStr = "审判官"; break;
						case "Swarm": newStr = "蜂群"; break;
						case "Intruder": newStr = "闯入者"; break;
						case "Bandit": newStr = "悍匪"; break;
						case "Allegiance": newStr = "忠臣"; break;
						case "Behemoth": newStr = "巨兽"; break;
						case "Emperor": newStr = "皇帝"; break;
						case "Firestorm": newStr = "火风暴"; break;
						case "Nemesis": newStr = "复仇女神"; break;
						case "Onslaught": newStr = "屠杀"; break;
						case "Predator": newStr = "掠食者"; break;
						case "Rampage": newStr = "震怒"; break;
						case "Stalker": newStr = "追踪者"; break;
						case "Trap": newStr = "陷阱"; break;
						case "Arrow": newStr = "之箭"; break;
						case "Skirmish": newStr = "激战"; break;
						case "Spectre": newStr = "幽灵"; break;
						case "Hero": newStr = "英雄"; break;
						case "Verdict": newStr = "裁决"; break;
						case "Mandate": newStr = "天命"; break;
						case "Dictator": newStr = "独裁者"; break;
						case "Decree": newStr = "审判"; break;
						case "Revolt": newStr = "厌恶"; break;
						case "Protector": newStr = "保护者"; break;
						case "Bastion": newStr = "棱堡"; break;
						case "Vindication": newStr = "辩护"; break;
						case "Guardian": newStr = "守护者"; break;
						case "Shield": newStr = "之盾"; break;
						case "Champion": newStr = "冠军"; break;
						case "Advocate": newStr = "拥护"; break;
						case "Challenger": newStr = "挑战者"; break;
						case "Provocation": newStr = "挑战"; break;
						case "Spite": newStr = "怨恨"; break;
						case "Mutiny": newStr = "哗变"; break;
						case "Repulser": newStr = "反击"; break;
						case "Resistance": newStr = "抵抗"; break;
						case "Liberator": newStr = "解放者"; break;
						case "Deception": newStr = "骗局"; break;
						case "Exile": newStr = "流亡"; break;
						case "Outcast": newStr = "背弃"; break;
						case "Fugitive": newStr = "亡命"; break;
						case "Renegade": newStr = "复仇"; break;
						case "Cutlass": newStr = "弯刀"; break;
						case "Affliction": newStr = "苦难"; break;
						case "Conflict": newStr = "冲突"; break;
						case "Aggressor": newStr = "侵略者"; break;
						case "Banshee": newStr = "妖姬"; break;
						case "Battle": newStr = "之战"; break;
						case "Firelance": newStr = "火矛"; break;
						case "Chariot": newStr = "战车"; break;
						case "Conqueror": newStr = "征服者"; break;
						case "Demolisher": newStr = "摧毁者"; break;
						case "Desolation": newStr = "之弃"; break;
						case "Eminence": newStr = "非凡"; break;
						case "Encounter": newStr = "之遇"; break;
						case "Enforcer": newStr = "执法者"; break;
						case "Eviscerator": newStr = "挖心者"; break;
						case "Exactor": newStr = "勒索者"; break;
						case "Fireclaw": newStr = "火爪"; break;
						case "Dragon": newStr = "之龙"; break;
						case "Gauntlet": newStr = "拳套"; break;
						case "Claw": newStr = "之爪"; break;
						case "Hammer": newStr = "之锤"; break;
						case "Hunter": newStr = "猎手"; break;
						case "Hydra": newStr = "九头蛇"; break;
						case "Intimidator": newStr = "胁迫者"; break;
						case "Mauler": newStr = "拳手"; break;
						case "Mayhem": newStr = "骚乱"; break;
						case "Monarch": newStr = "君主"; break;
						case "Nexus": newStr = "枢纽"; break;
						case "Rage": newStr = "之怒"; break;
						case "Sovereign": newStr = "主权"; break;
						case "Scorpion": newStr = "天蝎"; break;
						case "Scourge": newStr = "天灾"; break;
						case "Serpent": newStr = "之蛇"; break;
						case "Terror": newStr = "恐怖"; break;
						case "Vendetta": newStr = "仇杀"; break;
						case "Warlord": newStr = "军阀"; break;
						case "Wolf": newStr = "之狼"; break;
						case "Nightfall": newStr = "夜幕"; break;
						case "Night": newStr = "午夜"; break;
						case "Legacy": newStr = "传奇"; break;
						case "Backstab": newStr = "谋害"; break;
						case "Fire": newStr = "之火"; break;
						case "Marauder": newStr = "掠夺者"; break;
						case "Nova": newStr = "新星"; break;
						case "Raider": newStr = "劫掠者"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(RandomShipNamePatch),
						nameof(ConcatShipNameStrings));
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GenerateRandomUniqueCivilianShipNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
					instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "Lucky": newStr = "幸运"; break;
						case "Grand": newStr = "盛大"; break;
						case "Bright": newStr = "明亮"; break;
						case "Sublime": newStr = "雄伟"; break;
						case "Lonesome": newStr = "独行"; break;
						case "Charming": newStr = "魅力"; break;
						case "Enchanted": newStr = "极乐"; break;
						case "Brazen": newStr = "叮当"; break;
						case "Serene": newStr = "安宁"; break;
						case "Placid": newStr = "平和"; break;
						case "Quiet": newStr = "安静"; break;
						case "Friendly": newStr = "友善"; break;
						case "Happy": newStr = "快乐"; break;
						case "Fortunate": newStr = "幸运"; break;
						case "Merry": newStr = "欢喜"; break;
						case "Smiling": newStr = "微笑"; break;
						case "Cautious": newStr = "好奇"; break;
						case "Idle": newStr = "惬意"; break;
						case "Brisk": newStr = "轻快"; break;
						case "Bold": newStr = "粗犷"; break;
						case "Solitary": newStr = "隐士"; break;
						case "Radiant": newStr = "流彩"; break;
						case "Lavish": newStr = "慷慨"; break;
						case "Handsome": newStr = "英俊"; break;
						case "Majestic": newStr = "庄重"; break;
						case "Bountiful": newStr = "富饶"; break;
						case "Gallant": newStr = "壮丽"; break;
						case "Intrepid": newStr = "勇气"; break;
						case "Valiant": newStr = "勇敢"; break;
						case "Stout": newStr = "坚定"; break;
						case "Superb": newStr = "极品"; break;
						case "Regal": newStr = "豪华"; break;
						case "Noble": newStr = "尊贵"; break;
						case "Hardy": newStr = "大胆"; break;
						case "Strange": newStr = "陌生"; break;
						case "Shining": newStr = "闪耀"; break;
						case "Glowing": newStr = "光彩"; break;
						case "Lively": newStr = "活力"; break;
						case "Daunting": newStr = "可畏"; break;
						case "Slippery": newStr = "狡猾"; break;
						case "Crafty": newStr = "巧手"; break;
						case "Risky": newStr = "冒险"; break;
						case "Sneaky": newStr = "潜行"; break;
						case "Lone": newStr = "孤独"; break;
						case "Arduous": newStr = "艰难"; break;
						case "Tenacious": newStr = "顽强"; break;
						case "Outrageous": newStr = "无常"; break;
						case "Distant": newStr = "遥远"; break;
						case "Doubtful": newStr = "疑虑"; break;
						case "Jubilant": newStr = "欢庆"; break;
						case "Cheerful": newStr = "兴奋"; break;
						case "Adamant": newStr = "鉴定"; break;
						case "Resolute": newStr = "坚毅"; break;
						case "Curious": newStr = "求知"; break;
						case "Extravagant": newStr = "奢靡"; break;
						case "Audacious": newStr = "创意"; break;
						case "Futile": newStr = "徒劳"; break;
						case "Vain": newStr = "虚荣"; break;
						case "Aimless": newStr = "随性"; break;
						case "Cryptic": newStr = "神秘"; break;
						case "Prudent": newStr = "节俭"; break;
						case "Worthy": newStr = "可敬"; break;
						case "Honest": newStr = "诚信"; break;
						case "Venerable": newStr = "尊贵"; break;
						case "Precious": newStr = "珍贵"; break;
						case "Celestial": newStr = "天界"; break;
						case "Foolish": newStr = "愚笨"; break;
						case "Roaming": newStr = "游荡"; break;
						case "Blind": newStr = "盲目"; break;
						case "Dusty": newStr = "尘埃"; break;
						case "Lost": newStr = "失落"; break;
						case "Solar": newStr = "星际"; break;
						case "Swift": newStr = "敏捷"; break;
						case "Stellar": newStr = "星河"; break;
						case "Last": newStr = "最终"; break;
						case "Wild": newStr = "旷野"; break;
						case "Express": newStr = "特快"; break;
						case "Rusty": newStr = "生锈"; break;
						case "Far": newStr = "遥远"; break;
						case "Broken": newStr = "残破"; break;
						case "Fading": newStr = "褪色"; break;
						case "Silent": newStr = "无声"; break;
						case "Ancient": newStr = "古老"; break;
						case "Pristine": newStr = "太古"; break;
						case "Shabby": newStr = "褴褛"; break;
						case "Tired": newStr = "疲劳"; break;
						case "Weary": newStr = "疲倦"; break;
						case "Secretive": newStr = "守密"; break;
						case "Conspicuous": newStr = "瞩目"; break;
						case "Hidden": newStr = "隐蔽"; break;
						case "Dubious": newStr = "无赖"; break;
						case "Devious": newStr = "迂回"; break;
						case "Elusive": newStr = "莫测"; break;
						case "Shady": newStr = "可疑"; break;
						case "Wily": newStr = "狡诈"; break;
						case "Lawless": newStr = "无法"; break;
						case "Crooked": newStr = "不义"; break;
						case "Forbidden": newStr = "禁止"; break;
						case "Wry": newStr = "扭曲"; break;
						case "Cowering": newStr = "畏缩"; break;
						case "Muffled": newStr = "静音"; break;
						case "Grasping": newStr = "贪婪"; break;
						case "Hasty": newStr = "草率"; break;
						case "Mocking": newStr = "嘲笑"; break;
						case "Humble": newStr = "慈悲"; break;
						case "Sombre": newStr = "阴沉"; break;
						case "Solemn": newStr = "庄重"; break;
						case "Eager": newStr = "热情"; break;
						case "Deep": newStr = "深邃"; break;
						case "Meagre": newStr = "贫弱"; break;
						case "Frugal": newStr = "简朴"; break;
						case "Daring": newStr = "大胆"; break;
						case "Nimble": newStr = "机敏"; break;
						case "Feeble": newStr = "虚弱"; break;
						case "Arcane": newStr = "奥秘"; break;
						case "Profound": newStr = "博闻"; break;
						case "Obscure": newStr = "朦胧"; break;
						case "Graceful": newStr = "优雅"; break;
						case "Vanishing": newStr = "无踪"; break;
						case "Trusty": newStr = "可靠"; break;
						case "Late": newStr = "迟暮"; break;
						case "Decrepit": newStr = "衰朽"; break;
						case "Grimy": newStr = "肮脏"; break;
						case "Surly": newStr = "易怒"; break;
						case "Dire": newStr = "可怕"; break;
						case "Tarnished": newStr = "无光"; break;
						case "Galactic": newStr = "银河"; break;

						case "Queen": newStr = "女王"; break;
						case "Princess": newStr = "公主"; break;
						case "Sun": newStr = "之日"; break;
						case "Star": newStr = "之星"; break;
						case "Hope": newStr = "希望"; break;
						case "Chance": newStr = "机会"; break;
						case "Gamble": newStr = "博弈"; break;
						case "Aspiration": newStr = "志向"; break;
						case "Traveller": newStr = "行者"; break;
						case "Voyager": newStr = "旅者"; break;
						case "Wayfarer": newStr = "旅人"; break;
						case "Scoundrel": newStr = "恶棍"; break;
						case "Wanderer": newStr = "流浪者"; break;
						case "Trader": newStr = "行商"; break;
						case "Merchant": newStr = "商人"; break;
						case "Encounter": newStr = "之遇"; break;
						case "Scout": newStr = "斥候"; break;
						case "Obsession": newStr = "沉迷"; break;
						case "Moon": newStr = "之月"; break;
						case "Empress": newStr = "女皇"; break;
						case "Dream": newStr = "之梦"; break;
						case "Fantasy": newStr = "幻想"; break;
						case "Illusion": newStr = "梦幻"; break;
						case "Mirage": newStr = "幻影"; break;
						case "Ruse": newStr = "诡计"; break;
						case "Bluff": newStr = "误导"; break;
						case "Miracle": newStr = "奇迹"; break;
						case "Novelty": newStr = "新品"; break;
						case "Wonder": newStr = "奇观"; break;
						case "Scheme": newStr = "计谋"; break;
						case "Impulse": newStr = "奋进"; break;
						case "Venture": newStr = "冒险"; break;
						case "Wager": newStr = "博弈"; break;
						case "Adventure": newStr = "探险"; break;
						case "Intrigue": newStr = "密谋"; break;
						case "Luxury": newStr = "享受"; break;
						case "Challenge": newStr = "挑战"; break;
						case "Maneuver": newStr = "机动"; break;
						case "Smuggler": newStr = "走私者"; break;
						case "Lurker": newStr = "小艇"; break;
						case "Prowler": newStr = "蟊贼"; break;
						case "Imposter": newStr = "骗子"; break;
						case "Subterfuge": newStr = "谎言"; break;
						case "Mystery": newStr = "谜团"; break;
						case "Enterprise": newStr = "奋进"; break;
						case "Escapade": newStr = "玩笑"; break;
						case "Peril": newStr = "之祸"; break;
						case "Ploy": newStr = "手法"; break;
						case "Quest": newStr = "任务"; break;
						case "Force": newStr = "力量"; break;
						case "Whim": newStr = "奇想"; break;
						case "Adversity": newStr = "困境"; break;
						case "Navigator": newStr = "导航者"; break;
						case "Gambit": newStr = "博弈"; break;
						case "Pearl": newStr = "珍珠"; break;
						case "Jewel": newStr = "珠宝"; break;
						case "Treasure": newStr = "财宝"; break;
						case "Prize": newStr = "嘉奖"; break;
						case "Hoard": newStr = "积蓄"; break;
						case "Rogue": newStr = "流氓"; break;
						case "Agent": newStr = "特工"; break;
						case "Envoy": newStr = "使者"; break;
						case "Guide": newStr = "向导"; break;
						case "Lady": newStr = "女士"; break;
						case "Pathfinder": newStr = "寻径"; break;
						case "Expedition": newStr = "扩张"; break;
						case "Journey": newStr = "旅途"; break;
						case "Odyssey": newStr = "长征"; break;
						case "Errand": newStr = "差使"; break;
						case "Sojourn": newStr = "旅客"; break;
						case "Bargain": newStr = "对弈"; break;
						case "Way": newStr = "之道"; break;
						case "Guardian": newStr = "守护者"; break;
						case "Dawn": newStr = "黎明"; break;
						case "Echo": newStr = "回响"; break;
						case "Interlude": newStr = "插曲"; break;
						case "Ranger": newStr = "游骑兵"; break;
						case "Victory": newStr = "胜利"; break;
						case "Renegade": newStr = "复仇"; break;
						case "Starseeker": newStr = "占星者"; break;
						case "Starwind": newStr = "星风"; break;
						case "Solace": newStr = "慰藉"; break;
						case "Pride": newStr = "之傲"; break;
						case "Rimrunner": newStr = "远行者"; break;
						case "Starway": newStr = "星辰路"; break;
						case "Beggar": newStr = "乞丐"; break;
						case "Rover": newStr = "私掠者"; break;
						case "Starfire": newStr = "星焰"; break;
						case "Raider": newStr = "掠夺者"; break;
						case "Deal": newStr = "约定"; break;
						case "Rendezvous": newStr = "交汇"; break;
						case "Twilight": newStr = "目光"; break;
						case "Courage": newStr = "勇气"; break;
						case "Burden": newStr = "担当"; break;
						case "Spirit": newStr = "之魂"; break;
						case "Nightstar": newStr = "晚星"; break;
						case "Profit": newStr = "利润"; break;
						case "Relic": newStr = "遗迹"; break;
						case "Bootlegger": newStr = "走私贩"; break;
						case "Shroud": newStr = "寿衣"; break;
						case "Remorse": newStr = "悔恨"; break;
						case "Disturbance": newStr = "扰乱"; break;
						case "Trailblazer": newStr = "开拓者"; break;
						case "Resolution": newStr = "决心"; break;
						case "Decoy": newStr = "诱饵"; break;
						case "Culprit": newStr = "罪犯"; break;
						case "Destiny": newStr = "天命"; break;
						case "Tramp": newStr = "步伐"; break;
						case "Vagrant": newStr = "流浪者"; break;
						case "Splendor": newStr = "色彩"; break;
						case "Starrider": newStr = "星骑士"; break;
						case "Negotiator": newStr = "谈判者"; break;
						case "Partisan": newStr = "游击队"; break;
						case "Discovery": newStr = "探索"; break;
						case "Distress": newStr = "悲痛"; break;
						case "Rebel": newStr = "反叛"; break;
						case "Evasion": newStr = "逃避"; break;
						case "Pathway": newStr = "路径"; break;
						case "Endeavour": newStr = "奋进"; break;
						case "Memory": newStr = "记忆"; break;
						case "Orbit": newStr = "轨道"; break;
						case "Impasse": newStr = "绝境"; break;
						case "Nova": newStr = "新星"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
						 instruction.operand is MethodInfo method &&
						 method.Name == "Concat" &&
						 method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(RandomShipNamePatch),
						nameof(ConcatShipNameStrings));
				}
				yield return instruction;
			}
		}

		private static void AddShipNamePostfix(ref string __result)
		{
			__result = __result + _shipPostfix;
		}

		public static string ConcatShipNameStrings(string s1, string mid, string s2)
		{
			if (mid == " of ")
			{
				if (s1.StartsWith("之"))
					return s2 + s1;
				return s2 + "之" + s1;
			}

			return s1 + s2;
		}
	}
}