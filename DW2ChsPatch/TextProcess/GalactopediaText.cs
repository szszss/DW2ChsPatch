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

		private static string _dataDir;

		private static MethodInfo _methodCreateGalactopediaTopic;

		private static ConstructorInfo _constructorGalactopediaTopic;

		private static Type _typeGalactopediaTopic;

		private static Type _typeGalactopediaCategory;

		public static void Patch(Harmony harmony, string textDir, string dataDir)
		{
			_dir = textDir;

			_dataDir = dataDir;

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
					if (RacePatch.IsPlayableRace(instruction.operand as string))
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
				var filepath = $"{_dir}\\{filename}";
				ReadTranslatedGalactopedia(filename, filepath, ref title, ref text);
			}
			else if (category == 2) // GameScreens
			{
				var filename = $"Galactopedia\\GameScreens\\{title}.json";
				var filepath = $"{_dir}\\{filename}";
				ReadTranslatedGalactopedia(filename, filepath, ref title, ref text);
			}

			text = text.ToWindowsNewline();
			var typedCategory = Enum.GetValues(_typeGalactopediaCategory).GetValue(category);
			return _constructorGalactopediaTopic.Invoke(new[]
			{
				galactopediaTopicId, title, text, typedCategory, relatedItem, isCategoryHeading
			});
		}

		public static void CreateGalactopediaEarly()
		{
			var files = Directory.EnumerateFiles(Path.Combine(_dataDir, "Galactopedia\\"), "*.txt",
				SearchOption.AllDirectories);
			foreach (var file in files)
			{
				if (file.EndsWith("DOC.txt"))
					continue;
				ReadGalactopediaTxt(file, out var title, out var text, Encoding.GetEncoding(1252));
				var filename = GetRelativePath(file, _dataDir);
				filename = Path.ChangeExtension(filename, "json");
				ReadTranslatedGalactopedia(filename, Path.Combine(_dir, filename), ref title, ref text);
			}
		}

		private static bool ReadGalactopediaTxt(string path, out string title, out string text, Encoding encoding)
		{
			title = null;
			text = null;
			if (File.Exists(path))
			{
				title = Path.GetFileNameWithoutExtension(path);
				var content = File.ReadAllText(path, encoding);
				text = content;
				return true;
			}

			return false;
		}

		private static string GetRelativePath(string filespec, string folder)
		{
			Uri pathUri = new Uri(filespec);
			// Folders must end in a slash
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(folder);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}
	}
}