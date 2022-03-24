using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class HintText
	{
		private const string FILENAME = "Hints.txt";

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
				var lines = File.ReadAllLines(filepath);
				hints.Clear();
				foreach (var line in lines)
				{
					var line2 = line.Trim();
					if (!string.IsNullOrWhiteSpace(line2))
						hints.Add(line2);
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