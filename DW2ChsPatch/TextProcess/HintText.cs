﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
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

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:LoadStaticBaseData"),
				null, new HarmonyMethod(typeof(HintText), nameof(Postfix)));
		}

		private static void Postfix()
		{
			var filepath = Path.Combine(_dir, FILENAME);
			var json = JsonText.CreateOrGetJsonText(FILENAME, filepath);
			var field = AccessTools.Field("DistantWorlds.Types.Galaxy:HintText");
			var hints = field.GetValue(null) as List<string>;
			if (hints == null)
				return;

			if (json != null)
			{
				if (TranslationTextGenerator.Enable)
				{
					json.GetStringArray("", hints.ToArray(), out var results, false);

					for (var i = 0; i < hints.Count; i++)
					{
						hints[i] = results[i];
					}
				}
				else
				{
					var translationTable = json.CreateOriginalTranslationMappingMap();

					for (var i = 0; i < hints.Count; i++)
					{
						if (translationTable.TryGetValue(hints[i], out var newStr))
							hints[i] = newStr;
					}
				}
			}

			if (MainClass.HardcodedTextDoc != null)
			{
				var extraHintNodes = MainClass.HardcodedTextDoc.SelectNodes("//ExtraHint");
				if (extraHintNodes != null)
				{
					foreach (XmlNode node in extraHintNodes)
					{
						if (!string.IsNullOrEmpty(node.InnerText))
							hints.Add(node.InnerText);
					}
				}
			}

			var distinct = hints.Distinct().ToArray();
			hints.Clear();
			hints.AddRange(distinct);
		}

		/*public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate)
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
		}*/
	}
}