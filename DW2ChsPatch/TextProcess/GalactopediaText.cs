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

		private static bool ReadTranslatedGalactopedia(string filename, string path, ref string title, ref string text)
		{
			var json = JsonText.CreateOrGetJsonText(filename, path);
			if (json != null)
			{
				json.GetString("title", title, out var resultTitle);
				json.GetString("text", text, out var resultText);
				title = resultTitle;
				text = resultText;
				return true;
			}

			return false;
		}

		public static object CreateGalactopediaTopic(int galactopediaTopicId, string title, string text,
			int category, object relatedItem, bool isCategoryHeading)
		{
			if (category == 1) // GameConcepts
			{
				var filename = $"Galactopedia\\GameConcepts\\{title}.json";
				var filepath = $"chs\\{filename}";
				ReadTranslatedGalactopedia(filename, filepath, ref title, ref text);
			}
			else if (category == 2) // GameScreens
			{
				var filename = $"Galactopedia\\GameScreens\\{title}.json";
				var filepath = $"chs\\{filename}";
				ReadTranslatedGalactopedia(filename, filepath, ref title, ref text);
			}

			var typedCategory = Enum.GetValues(_typeGalactopediaCategory).GetValue(category);
			return _constructorGalactopediaTopic.Invoke(new[]
			{
				galactopediaTopicId, title, text, typedCategory, relatedItem, isCategoryHeading
			});
		}
	}
}