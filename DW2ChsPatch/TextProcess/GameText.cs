﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DW2ChsPatch.Feature;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class GameText
	{
		private const string FILENAME = "GameText.txt";

		private static string _dir;

		private static bool _chineseCCS;

		public static void Patch(Harmony harmony, string textDir, bool chineseComponentCategoryShort)
		{
			_dir = textDir;
			_chineseCCS = chineseComponentCategoryShort;

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextResolver:LoadText", new[] { typeof(string) }),
				null, new HarmonyMethod(typeof(GameText), nameof(GameTextPostfix)));
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.DialogSystem:LoadFile", new[] { typeof(string) }),
				new HarmonyMethod(typeof(GameText), nameof(DialogPrefix)),
				new HarmonyMethod(typeof(GameText), nameof(DialogPostfix)));
		}

		public static void GameTextPostfix()
		{
			var file = Path.Combine(_dir, FILENAME);
			var texts = AccessTools.Field(
				AccessTools.TypeByName("DistantWorlds.Types.TextResolver"),
				"_Text").GetValue(null) as Dictionary<string, string>;

			if (texts != null)
			{
				foreach (var pair in ReadTextIntoDictionary(file))
				{
					//if (texts.ContainsKey(pair.Key))
						texts[pair.Key] = pair.Value;
				}

				if (_chineseCCS)
					TranslateComponentCategoryAbbr(texts);
			}
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
		}

		public static void DialogPostfix(ref Dictionary<string, string> __result, string __0)
		{
			if (__result != null)
			{
				var file = Path.Combine(_dir, __0);
				if (File.Exists(file))
				{
					foreach (var pair in ReadTextIntoDictionary(file))
					{
						//if (__result.ContainsKey(pair.Key))
							__result[pair.Key] = pair.Value;
					}
				}
			}
			MainClass.PostLoadFix();
		}

		public static IEnumerable<Tuple<string, string>> ReadTextIntoLines(string filepath)
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
		}

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

		/*
		 private const string FILENAME = "GameText.csv";
		
		private static readonly CsvOptions _csvOptions = new CsvOptions()
		{
			Separator = ';',
			HeaderMode = HeaderMode.HeaderAbsent
		};

		private static string _dir;

		private static void ReadTextIntoDictionary(string filepath, 
			Dictionary<string, string> textDic,
			Dictionary<string, int> hashDic = null)
		{
			IEnumerable<ICsvLine> lines = null;

			using (var stream = File.OpenRead(filepath))
			{
				lines = CsvReader.ReadFromStream(stream, _csvOptions);
			}

			foreach (var line in lines)
			{
				if (line.ColumnCount >= 3)
				{
					var key = line.Values[0].Trim();
					if (!string.IsNullOrEmpty(key))
					{
						var oldValueHash = Convert.ToInt32(line.Values[1].Trim(), 16);
						var newValue = line.Values[2].Replace("\\n", Environment.NewLine).Trim();
						textDic[key] = newValue;
						if (hashDic != null)
							hashDic[key] = oldValueHash;
					}
				}
			}
		}

		public static void Update(string gameTextDir, string exportDir)
		{
			var outputFile = Path.Combine(exportDir, FILENAME);
			var textDic = new Dictionary<string, string>();
			var hashDic = new Dictionary<string, int>();
			if (File.Exists(outputFile))
			{
				ReadTextIntoDictionary(outputFile, textDic, hashDic);
			}

			if (File.Exists(gameTextDir))
			{
				var lines = File.ReadAllLines(gameTextDir, Encoding.UTF8);
				List<string[]> output = new List<string[]>();

				foreach (var line in lines)
				{
					var trimmedLine = line.Trim();
					string key = null;
					string text = null;
					int hash = 0;
					if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("'"))
					{
						int num2 = trimmedLine.IndexOf(";", StringComparison.Ordinal);
						if (num2 >= 0)
						{
							key = trimmedLine.Substring(0, num2).Trim();
							text = trimmedLine.Substring(num2 + 1, trimmedLine.Length - (num2 + 1)).Trim();
							hash = text.Replace("\\n", "").GetHashCode();

							if (hashDic.TryGetValue(key, out int oldHash) && oldHash == hash)
							{
								text = textDic[key];
							}
						}
					}

					if (key != null)
					{
						output.Add(new [] {key, Convert.ToString(hash, 16), text});
					}
					else
					{
						output.Add(new [] {"", "", ""});
					}
				}

				using (var stream = new StreamWriter(File.OpenWrite(outputFile), Encoding.UTF8))
				{
					CsvWriter.Write(stream, new string[3], output, ';', true);
				}
			}
		}

		public static void Patch(Harmony harmony, string textDir)
		{
			_dir = textDir;
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextResolver:LoadText", new [] {typeof(string)}),
				null, new HarmonyMethod(typeof(GameTextPatch), nameof(GameTextPatch.Postfix)));
		}

		public static class GameTextPatch
		{
			public static void Postfix()
			{
				var file = Path.Combine(_dir, FILENAME);
				var texts = AccessTools.Field(
					AccessTools.TypeByName("DistantWorlds.Types.TextResolver"),
					"_Text").GetValue(null) as Dictionary<string, string>;

				if (File.Exists(file) && texts != null)
				{
					ReadTextIntoDictionary(file, texts);
				}
			}
		}*/
	}
}