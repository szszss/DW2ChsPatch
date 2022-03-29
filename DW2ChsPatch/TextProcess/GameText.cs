using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using DW2ChsPatch.Feature;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class GameText
	{
		private const string FILENAME = "GameText.json";

		private static string _dir;

		private static JsonText _json;

		private static Dictionary<string, JsonText> _dialogJson = new Dictionary<string, JsonText>();

		private static bool _chineseCCS;

		public static void Patch(Harmony harmony, string textDir, bool chineseComponentCategoryShort)
		{
			_dir = textDir;
			_chineseCCS = chineseComponentCategoryShort;

			_json = JsonText.CreateOrGetJsonText(FILENAME, Path.Combine(_dir, FILENAME));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextResolver:LoadText", new[] { typeof(string) }),
				null, new HarmonyMethod(typeof(GameText), nameof(GameTextPostfix)),
				new HarmonyMethod(typeof(GameText), nameof(GameTextTranspiler)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.DialogSystem:LoadFile", new[] { typeof(string) }),
				new HarmonyMethod(typeof(GameText), nameof(DialogPrefix)),
				null,
				new HarmonyMethod(typeof(GameText), nameof(DialogTranspiler)));
		}

		private static IEnumerable<CodeInstruction> GameTextTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt &&
				    instruction.operand is MethodInfo method &&
				    method.Name.Contains("Add"))
				{
					instruction.opcode = OpCodes.Call;
					instruction.operand = AccessTools.Method(typeof(GameText), nameof(OnAddGameText));
				}
				yield return instruction;
			}
		}

		public static void OnAddGameText(Dictionary<string, string> texts, string key, string text)
		{
			if (_json != null)
			{
				_json.GetString(key, text, out var result);
				texts[key] = result;
			}
			else
			{
				texts[key] = text;
			}
		}

		public static void GameTextPostfix()
		{
			//var file = Path.Combine(_dir, FILENAME);
			var texts = AccessTools.Field(
				AccessTools.TypeByName("DistantWorlds.Types.TextResolver"),
				"_Text").GetValue(null) as Dictionary<string, string>;

			if (texts != null)
			{
				if (_chineseCCS)
					TranslateComponentCategoryAbbr(texts);
			}

			MainClass.PostLoadFix();
		}

		public static void DialogPrefix(ref string __0)
		{
			if (!__0.EndsWith("/defaultDialog.txt"))
			{
				var raceName = Path.GetFileNameWithoutExtension(__0);
				raceName = RacePatch.GetRaceOriginalName(raceName);
				if (!string.IsNullOrEmpty(raceName))
					__0 = $"dialog/{raceName}.txt";
			}

			var outputName = Path.ChangeExtension(__0, "json");
			var json = JsonText.CreateOrGetJsonText(outputName, Path.Combine(_dir, outputName));
			_dialogJson[__0] = json;
		}

		private static IEnumerable<CodeInstruction> DialogTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt &&
				    instruction.operand is MethodInfo method &&
				    method.Name.Contains("Add"))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(GameText), nameof(OnAddDialogText)));
					continue;
				}
				yield return instruction;
			}
		}

		public static void OnAddDialogText(Dictionary<string, string> texts, string key, string text, string path)
		{
			_dialogJson.TryGetValue(path, out var json);
			if (json != null)
			{
				json.GetString(key, text, out var result);
				texts[key] = result;
			}
			else
			{
				texts[key] = text;
			}
		}

		/*public static void DialogPostfix(ref Dictionary<string, string> __result, string __0)
		{
			if (__result != null)
			{
				var file = Path.Combine(_dir, __0);
				var json = JsonText.CreateOrGetJsonText(__0, file);
				if (json != null)
				{
					foreach (var item in json)
					{
						if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrEmpty(item.Translation))
							__result[item.Key] = item.Translation;
					}
				}
			}
		}*/

		/*public static IEnumerable<Tuple<string, string>> ReadTextIntoLines(string filepath)
		{
			if (File.Exists(filepath))
			{
				var lines = File.ReadAllLines(filepath, Encoding.UTF8);

				foreach (var line in lines)
				{
					var trimmedLine = line.Trim();
					if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("'"))
					{
						int num2 = trimmedLine.IndexOf(";", StringComparison.Ordinal);
						if (num2 >= 0)
						{
							var key = trimmedLine.Substring(0, num2).Trim();
							var text = trimmedLine.Substring(num2 + 1, trimmedLine.Length - (num2 + 1)).Trim();
							yield return Tuple.Create(key, text.Replace("\\n", Environment.NewLine));
						}
					}
				}
			}
		}

		public static Dictionary<string, string> PutLinesIntoDictionary(
			IEnumerable<Tuple<string, string>> lines,
			Dictionary<string, string> results = null)
		{
			if (results == null)
				results = new Dictionary<string, string>();
			foreach (var tuple in lines)
			{
				results[tuple.Item1] = tuple.Item2;
			}

			return results;
		}

		public static Dictionary<string, string> ReadTextIntoDictionary(string filepath)
		{
			var lines = ReadTextIntoLines(filepath);
			return PutLinesIntoDictionary(lines);
		}

		public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate)
		{
			var gameTextOrigin = GameText.ReadTextIntoLines(pathOrigin);
			var gameTextTranslatedJson = new JsonText();
			if (File.Exists(pathTranslate))
				gameTextTranslatedJson.ImportFromFile(pathTranslate);
			var gameTextTranslated = File.Exists(pathTranslate) 
				? GameText.ReadTextIntoLines(pathTranslate) :
				new List<Tuple<string, string>>();

			var vanillaGameTextKeys = new HashSet<string>();
			var gameTextTranslatedDic = GameText.PutLinesIntoDictionary(gameTextTranslated);
			var gameTextJson = new JsonText();

			foreach (var pair in gameTextOrigin)
			{
				var key = pair.Item1;
				if (string.IsNullOrWhiteSpace(key))
					continue;
				var original = pair.Item2;
				vanillaGameTextKeys.Add(key);
				gameTextTranslatedDic.TryGetValue(key, out var translation);
				gameTextJson.SetString(key, original, translation);
			}

			foreach (var pair in gameTextTranslated)
			{
				var key = pair.Item1;
				if (string.IsNullOrWhiteSpace(key))
					continue;
				if (!vanillaGameTextKeys.Contains(key))
				{
					var translation = pair.Item2;
					gameTextJson.SetString(key, "", translation);
				}
			}

			gameTextJson.ExportToFile(pathOutput);
		}*/

		private static void TranslateComponentCategoryAbbr(Dictionary<string, string> texts)
		{
			texts["ComponentCategory AreaShieldRecharge Short"] = "盾充";
			texts["ComponentCategory Armor Short"] = "装甲";
			texts["ComponentCategory AssaultPod Short"] = "登舰";
			texts["ComponentCategory CargoBay Short"] = "货仓";
			texts["ComponentCategory Colonization Short"] = "殖民";
			texts["ComponentCategory CommandCenter Short"] = "舰桥";
			texts["ComponentCategory CommerceCenter Short"] = "商业";
			texts["ComponentCategory Construction Short"] = "建造";
			texts["ComponentCategory Countermeasures Short"] = "闪避";
			texts["ComponentCategory CountermeasuresFleet Short"] = "群闪";
			texts["ComponentCategory CrewQuarters Short"] = "宿舍";
			texts["ComponentCategory DamageControl Short"] = "损管";
			texts["ComponentCategory DockingBay Short"] = "空港";
			texts["ComponentCategory EnergyCollector Short"] = "辅能";
			texts["ComponentCategory EnergyToFuel Short"] = "造油";
			texts["ComponentCategory Engine Short"] = "推进";
			texts["ComponentCategory EngineVectoring Short"] = "转向";
			texts["ComponentCategory Extractor Short"] = "采矿";
			texts["ComponentCategory RemoteFuelTransfer Short"] = "输油";
			texts["ComponentCategory FighterBay Short"] = "战机";
			texts["ComponentCategory FuelStorage Short"] = "油箱";
			texts["ComponentCategory HyperBlock Short"] = "阻跳";
			texts["ComponentCategory HyperDeny Short"] = "反跳";
			texts["ComponentCategory HyperDrive Short"] = "跃迁";
			texts["ComponentCategory IonDefense Short"] = "防瘫";
			texts["ComponentCategory MedicalCenter Short"] = "医疗";
			texts["ComponentCategory PassengerCompartment Short"] = "载客";
			texts["ComponentCategory Reactor Short"] = "能量";
			texts["ComponentCategory RecreationCenter Short"] = "娱乐";
			texts["ComponentCategory ResearchLab Short"] = "科研";
			texts["ComponentCategory ScannerEmpireMasking Short"] = "匿名";
			texts["ComponentCategory ScannerExploration Short"] = "探索";
			texts["ComponentCategory ScannerExplorationSurvey Short"] = "勘探";
			texts["ComponentCategory ScannerJammer Short"] = "干扰";
			texts["ComponentCategory ScannerJumpTracking Short"] = "跳感";
			texts["ComponentCategory ScannerLongRange Short"] = "远感";
			texts["ComponentCategory ScannerShortRange Short"] = "近感";
			texts["ComponentCategory ScannerRoleMasking Short"] = "伪装";
			texts["ComponentCategory ScannerTrace Short"] = "追踪";
			texts["ComponentCategory Shields Short"] = "护盾";
			texts["ComponentCategory ShieldEnhancement Short"] = "盾强";
			texts["ComponentCategory Stealth Short"] = "隐形";
			texts["ComponentCategory TargetingComputer Short"] = "命中";
			texts["ComponentCategory TargetingComputerFleet Short"] = "群中";
			texts["ComponentCategory TractorBeam Short"] = "牵引";
			texts["ComponentCategory TroopCompartment Short"] = "陆军";
			texts["ComponentCategory WeaponArea Short"] = "范围";
			texts["ComponentCategory WeaponBombard Short"] = "轰炸";
			texts["ComponentCategory WeaponCloseIn Short"] = "近程";
			texts["ComponentCategory WeaponIntercept Short"] = "拦截";
			texts["ComponentCategory WeaponIon Short"] = "瘫痪";
			texts["ComponentCategory WeaponStandoff Short"] = "远程";
		}
	}
}