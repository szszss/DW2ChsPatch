using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class HintText
	{
		private const string FILENAME = "Hints.json";

		private static string _dir;

		public static void Patch(Harmony harmony, string textDir)
		{
			_dir = textDir;

			harmony.Patch(AccessTools.Method("DistantWorlds2.DWGame:Initialize"),
				null, new HarmonyMethod(typeof(HintText), nameof(Postfix)));
		}

		private static void Postfix()
		{
			var filepath = Path.Combine(_dir, FILENAME);
			var field = AccessTools.Field("DistantWorlds.Types.Galaxy:HintText");
			var hints = field.GetValue(null) as List<string>;
			if (hints != null && File.Exists(filepath))
			{
				var json = new JsonText(filepath);
				var translationTable = json.CreateOriginalTranslationMappingMap();

				for (var i = 0; i < hints.Count; i++)
				{
					if (translationTable.TryGetValue(hints[i], out var newStr))
						hints[i] = newStr;
				}
			}
		}

		public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate)
		{
			var json = new JsonText();
			var originLines = File.ReadAllLines(pathOrigin);
			var translateLines = File.Exists(pathTranslate) 
				? File.ReadAllLines(pathTranslate)
				: new string[0];
			int i = 0;
			for (; i < originLines.Length; i++)
			{
				var origin = originLines[i];
				var translate = i < translateLines.Length ? translateLines[i] : null;
				json.SetString(i.ToString(), origin, translate);
			}
			for (; i < translateLines.Length; i++)
			{
				var origin = "";
				var translate = translateLines[i];
				json.SetString(i.ToString(), origin, translate);
			}
			json.ExportToFile(pathOutput);
		}
	}
}