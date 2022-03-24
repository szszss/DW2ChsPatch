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

		private static bool ReadTranslatedGalactopedia(string path, ref string title, ref string text, Encoding encoding)
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

		public static object CreateGalactopediaTopic(int galactopediaTopicId, string title, string text,
			int category, object relatedItem, bool isCategoryHeading)
		{
			if (category == 1) // GameConcepts
			{
				var filename = $"chs\\Galactopedia\\GameConcepts\\{title}.txt";
				ReadTranslatedGalactopedia(filename, ref title, ref text, Encoding.UTF8);
			}
			else if (category == 2) // GameScreens
			{
				var filename = $"chs\\Galactopedia\\GameScreens\\{title}.txt";
				ReadTranslatedGalactopedia(filename, ref title, ref text, Encoding.UTF8);
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
			ReadTranslatedGalactopedia(pathOrigin, ref originTitle, ref originText, Encoding.GetEncoding(1252));

			string tranlatedTitle = null;
			string tranlatedText = null;
			if (File.Exists(pathTranslate))
			{
				ReadTranslatedGalactopedia(pathTranslate, ref tranlatedTitle, ref tranlatedText, Encoding.GetEncoding(1252));
			}

			var gameTextJson = new JsonText();
			gameTextJson.SetString("title", originTitle, tranlatedTitle);
			gameTextJson.SetString("text", originText, tranlatedText);

			gameTextJson.ExportToFile(pathOutput);
		}
	}
}