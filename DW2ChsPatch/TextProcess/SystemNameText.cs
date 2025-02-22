﻿using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class SystemNameText
	{
		private const string FILENAME = "SystemNames.json";
		private const string FILENAME_1 = "SystemNamesChs.txt";

		private static string _dir;

		private static int _namingStyle;

		public static void Patch(Harmony harmony, string textDir, int namingStyle)
		{
			_dir = textDir;
			_namingStyle = namingStyle;

			harmony.Patch(AccessTools.Method("DistantWorlds2.DWGame:Initialize"),
				null, new HarmonyMethod(typeof(SystemNameText), nameof(Postfix)));
		}

		private static void Postfix()
		{
			var filepath = Path.Combine(_dir, FILENAME);
			var json = JsonText.CreateOrGetJsonText(FILENAME, filepath);
			var field = AccessTools.Field("DistantWorlds.Types.Galaxy:SystemNames");
			var names = field.GetValue(null) as List<string>;
			if (names != null && json != null)
			{
				for (var i = 0; i < names.Count; i++)
				{
					var str = names[i];
					json.GetString(str, str, out var result);
					names[i] = result;
				}
			}

			if (names != null)
			{
				switch (_namingStyle)
				{
					case 1:
						filepath = Path.Combine(_dir, FILENAME_1);
						if (File.Exists(filepath))
						{
							names.Clear();
							ReadTxtIntoList(filepath, names);
						}
						break;
				}
			}
		}

		private static void ReadTxtIntoList(string filepath, List<string> namelist)
		{
			if (File.Exists(filepath))
			{
				var lines = File.ReadAllLines(filepath);
				foreach (var line in lines)
				{
					var line2 = line.Trim();
					if (string.IsNullOrWhiteSpace(line2) || line2.StartsWith("\'"))
						continue;

					var splitNames = line2.Split(',');
					foreach (var name in splitNames)
					{
						var name2 = name.Trim();
						if (!string.IsNullOrWhiteSpace(name2))
							namelist.Add(name2);
					}
				}
			}
		}

		/*public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate)
		{
			var json = new JsonText();
			var originNames = new List<string>();
			var translateNames = new List<string>();
			ReadTxtIntoList(pathOrigin, originNames);
			ReadTxtIntoList(pathTranslate, translateNames);
			int i = 0;
			for (; i < originNames.Count; i++)
			{
				var origin = originNames[i];
				var translate = i < translateNames.Count ? translateNames[i] : null;
				json.SetString(origin, origin, translate);
			}
			for (; i < translateNames.Count; i++)
			{
				var origin = "";
				var translate = translateNames[i];
				json.SetString(i.ToString(), origin, translate);
			}
			json.ExportToFile(pathOutput);
		}*/
	}
}