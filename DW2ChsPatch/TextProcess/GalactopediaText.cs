using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using DW2ChsPatch.Feature;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class GalactopediaText
	{
		private static string _dir;

		private static MethodInfo _methodCreateGalactopediaTopic;

		private static ConstructorInfo _constructorGalactopediaTopic;

		private static Type _typeGalactopediaTopic;

		private static Type _typeGalactopediaCategory;

		public static void Patch(Harmony harmony, string textDir)
		{
			_dir = textDir;

			_methodCreateGalactopediaTopic =
				AccessTools.Method(typeof(GalactopediaText), nameof(CreateGalactopediaTopic));
			_typeGalactopediaTopic = AccessTools.TypeByName("DistantWorlds.Types.GalactopediaTopic");
			_typeGalactopediaCategory = AccessTools.TypeByName("DistantWorlds.Types.GalactopediaCategory");
			_constructorGalactopediaTopic = AccessTools.FirstConstructor(_typeGalactopediaTopic, 
				x => x.GetParameters().Length > 1);

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.GalactopediaTopicList:GenerateTopicsForRelatedItems"),
				null, null,
				new HarmonyMethod(typeof(GalactopediaText), nameof(Transpiler)));
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Newobj &&
				    instruction.operand is ConstructorInfo method &&
				    method.IsConstructor &&
				    method.DeclaringType.FullName.Contains("DistantWorlds.Types.GalactopediaTopic"))
				{
					yield return new CodeInstruction(OpCodes.Call, _methodCreateGalactopediaTopic);
					yield return new CodeInstruction(OpCodes.Castclass, _typeGalactopediaTopic);
					continue;
				}

				yield return instruction;

				if (instruction.opcode == OpCodes.Ldstr)
				{
					if ("Human".Equals(instruction.operand) ||
						"Ackdarian".Equals(instruction.operand) ||
						"Zenox".Equals(instruction.operand) ||
						"Mortalen".Equals(instruction.operand) ||
						"Haakonish".Equals(instruction.operand) ||
						"Teekan".Equals(instruction.operand) ||
						"Boskara".Equals(instruction.operand))
					{
						yield return new CodeInstruction(OpCodes.Call,
							AccessTools.Method(typeof(RacePatch), nameof(RacePatch.GetRaceTranslatedName)));
					}
				}
			}
		}

		private static bool ReadOriginalGalactopedia(string path, ref string title, ref string text, Encoding encoding)
		{
			if (File.Exists(path))
			{
				var content = File.ReadAllText(path, encoding);
				if (!string.IsNullOrWhiteSpace(content))
				{
					var firstLine = content.Split('\r', '\n')[0].Trim();
					if (firstLine.StartsWith(";"))
					{
						title = firstLine.Substring(1);
						content = content.Substring(content.IndexOf('\n') + 1);
					}

					text = content;

					return true;
				}
			}

			return false;
		}

		private static bool ReadTranslatedGalactopedia(string path, ref string title, ref string text)
		{
			if (File.Exists(path))
			{
				JsonText json = new JsonText(path);
				title = json.GetString("title", title);
				text = json.GetString("text", text);
				return true;
			}

			return false;
		}

		private static bool ReadTranslatedGalactopedia(string path,
			out string oTitle, out string tTitle,
			out string oText, out string tText)
		{
			oTitle = null;
			tTitle = null;
			oText = null;
			tText = null;
			if (File.Exists(path))
			{
				JsonText json = new JsonText(path);
				json.GetOriginalAndTranslatedString("title", out oTitle, out tTitle);
				json.GetOriginalAndTranslatedString("text", out oText, out tText);
				return true;
			}

			return false;
		}

		public static object CreateGalactopediaTopic(int galactopediaTopicId, string title, string text,
			int category, object relatedItem, bool isCategoryHeading)
		{
			if (category == 1) // GameConcepts
			{
				var filename = $"chs\\Galactopedia\\GameConcepts\\{title}.json";
				ReadTranslatedGalactopedia(filename, ref title, ref text);
			}
			else if (category == 2) // GameScreens
			{
				var filename = $"chs\\Galactopedia\\GameScreens\\{title}.json";
				ReadTranslatedGalactopedia(filename, ref title, ref text);
			}

			var typedCategory = Enum.GetValues(_typeGalactopediaCategory).GetValue(category);
			return _constructorGalactopediaTopic.Invoke(new[]
			{
				galactopediaTopicId, title, text, typedCategory, relatedItem, isCategoryHeading
			});
		}

		public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate)
		{
			var originTitle = Path.GetFileNameWithoutExtension(pathOrigin);
			var originText = "";
			ReadOriginalGalactopedia(pathOrigin, ref originTitle, ref originText, Encoding.GetEncoding(1252));

			string tranlatedTitle = null;
			string tranlatedText = null;
			string oldTranslatedTitle = null;
			string oldTranslatedText = null;
			if (File.Exists(pathTranslate))
			{
				ReadTranslatedGalactopedia(pathTranslate, 
					out var oTitle, out var tTitle,
					out var oText, out var tText);

				TextHelper.CheckAndGetTranslatedString(originTitle, oTitle, tTitle, 
					out tranlatedTitle, out oldTranslatedTitle);
				TextHelper.CheckAndGetTranslatedString(originText, oText, tText,
					out tranlatedText, out oldTranslatedText);
			}

			var gameTextJson = new JsonText();
			gameTextJson.SetString("title", originTitle, tranlatedTitle, oldTranslatedTitle);
			gameTextJson.SetString("text", originText, tranlatedText, oldTranslatedText);

			gameTextJson.ExportToFile(pathOutput);
		}
	}
}