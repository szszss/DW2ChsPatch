using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
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

		private static Dictionary<string, string> _texts;

		private static bool _chineseCCS;

		public static void Patch(Harmony harmony, string textDir, bool chineseComponentCategoryShort)
		{
			_dir = textDir;
			_chineseCCS = chineseComponentCategoryShort;

			_json = JsonText.CreateOrGetJsonText(FILENAME, Path.Combine(_dir, FILENAME));

			/*harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextResolver:LoadText", new[] { typeof(string) }),
				null, new HarmonyMethod(typeof(GameText), nameof(GameTextPostfix)),
				new HarmonyMethod(typeof(GameText), nameof(GameTextTranspiler)));*/

			harmony.Patch(AccessTools.Method("DistantWorlds2.DWGame:Initialize"),
				null, new HarmonyMethod(typeof(GameText), nameof(GameTextPostfix)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.DialogSystem:LoadFile", new[] { typeof(string) }),
				new HarmonyMethod(typeof(GameText), nameof(DialogPrefix)),
				null,
				new HarmonyMethod(typeof(GameText), nameof(DialogTranspiler)));

			_texts = AccessTools.Field(
				AccessTools.TypeByName("DistantWorlds.Types.TextResolver"),
				"_Text").GetValue(null) as Dictionary<string, string>;
		}

		/*private static IEnumerable<CodeInstruction> GameTextTranspiler(IEnumerable<CodeInstruction> instructions)
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
				texts[key] = result.ToWindowsNewline();
			}
			else
			{
				texts[key] = text;
			}
		}*/

		public static void GameTextPostfix()
		{
			//var file = Path.Combine(_dir, FILENAME);

			if (_texts != null)
			{
				foreach (var (key, value) in _texts.ToArray())
				{
					_json.GetString(key, value, out var result);
					_texts[key] = result;
				}

				if (_chineseCCS)
					TranslateComponentCategoryAbbr(_texts);
			}

			if (MainClass.HardcodedTextDoc != null && _texts != null)
			{
				var extraTextNodes = MainClass.HardcodedTextDoc.SelectNodes("//ExtraGameText");
				if (extraTextNodes != null)
				{
					foreach (XmlNode node in extraTextNodes)
					{
						var keyNode = node.Attributes["Key"];
						if (keyNode != null 
						    && !string.IsNullOrWhiteSpace(keyNode.Value)
						    && !string.IsNullOrEmpty(node.InnerText))
							_texts[keyNode.Value] = node.InnerText.UniteNewline().ToWindowsNewline();
					}
				}
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
				{
					raceName = raceName.ToLower();
					__0 = $"dialog/{raceName}.txt";
				}
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
				texts[key] = result.ToWindowsNewline();
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
			var nodes = MainClass.HardcodedTextDoc.SelectNodes("//ComponentCategoryAbbr");
			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					var attr = node.Attributes?["Key"];
					if (attr != null)
					{
						texts[$"ComponentCategory {attr.Value} Short"] = node.InnerText;
					}
				}
			}
		}
	}
}