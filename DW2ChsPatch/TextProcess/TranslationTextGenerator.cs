using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public static class TranslationTextGenerator
	{
		public static bool Enable = false;

		public static string OutputDir = "";

		public const string REPORT_FILENAME = "Report.html";

		private static int _count_add = 0;

		private static int _count_modify = 0;

		private static int _count_remove = 0;

		private static List<string> _order = new List<string>();

		private static Dictionary<string, JsonText> _jsons = new Dictionary<string, JsonText>();

		private static Dictionary<string, List<Changelog>> _changelogMaps = new Dictionary<string, List<Changelog>>();

		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds2.DWGame:ShowMenu"),
				null, new HarmonyMethod(typeof(TranslationTextGenerator), nameof(ShowMenuPostfix)));
		}

		public static void ShowMenuPostfix()
		{
			if (Enable)
			{
				Enable = false;
				Output();
			}
		}

		public static void AddJson(string file, JsonText json)
		{
			_order.Add(file);
			_jsons.Add(file, json);
		}

		public static JsonText GetJson(string file)
		{
			_jsons.TryGetValue(file, out var value);
			return value;
		}

		public static void RecordAddedItem(string file, string newlineKey, string newlineOriginal)
		{
			if (!_changelogMaps.TryGetValue(file, out var list))
				_changelogMaps[file] = list = new List<Changelog>();
			list.Add(new AddChangelog() {Key = newlineKey, Original = newlineOriginal});
			_count_add++;
		}

		public static void RecordModifiedItem(string file, string newlineKey, 
			string newlineOldOriginal, string newlineNewOriginal, string newlineOldTranslation)
		{
			if (!_changelogMaps.TryGetValue(file, out var list))
				_changelogMaps[file] = list = new List<Changelog>();
			list.Add(new ModifiedChangelog() { Key = newlineKey, Original = newlineOldOriginal, 
				NewOriginal = newlineNewOriginal, Translation = newlineOldTranslation});
			_count_modify++;
		}

		public static void RecordRemovedItem(string file, string removedKey, string removedOriginal, string removedTranslation)
		{
			if (!_changelogMaps.TryGetValue(file, out var list))
				_changelogMaps[file] = list = new List<Changelog>();
			list.Add(new RemovedChangelog() { Key = removedKey, Original = removedOriginal, Translation = removedTranslation});
			_count_remove++;
		}

		public static void Output()
		{
			var versionMethod = AccessTools.Field("DistantWorlds2.DWGame:Version");
			var version = versionMethod != null
				? versionMethod.GetValue(null) as string
				: "未知";

			foreach (var pair in _jsons)
			{
				var file = pair.Key;
				var json = pair.Value;
				foreach (var item in json)
				{
					if (item.Status == JsonText.TextPresent.Unused)
					{
						RecordRemovedItem(file, item.Key, item.Original, item.Translation);
						item.Stage = (short) Math.Min((int)item.Stage, 1);
						item.Context = $"于{version}删除";
					}
				}

				json.ExportToFile(Path.Combine(OutputDir, file));
			}

			var sb = new StringBuilder();

			sb.AppendLine("<!DOCTYPE html><html lang=\"zh\"><head><meta charSet=\"utf-8\"/>");
			sb.AppendLine("<title>文本生成报告</title>");
			sb.AppendLine("</head>");
			sb.AppendLine("<body>");

			sb.AppendLine($"<p>游戏版本 {version}</p>");
			sb.AppendLine("<p>统计：<br/>");
			sb.AppendLine($"新增 {_count_add}条<br/>");
			sb.AppendLine($"变更 {_count_modify}条<br/>");
			sb.AppendLine($"删减 {_count_remove}条");
			sb.AppendLine("</p><br/><br/>");

			var unchangedList = new List<string>();
			var printChangedTitle = false;
			foreach (var file in _order)
			{
				if (_changelogMaps.TryGetValue(file, out var list))
				{
					int fileAdd = 0, fileMod = 0, fileRemove = 0;
					var hasDot = false;
					
					foreach (var changelog in list)
					{
						changelog.ApplyStats(ref fileAdd, ref fileMod, ref fileRemove);
					}

					if (!printChangedTitle)
					{
						printChangedTitle = true;
						sb.AppendLine("<h2>变化的文件</h2>");
						sb.AppendLine("<br/>");
					}

					sb.AppendLine($"<h3>文件 {file}</h3>");
					if (fileAdd > 0)
					{
						sb.Append($"新增 {fileAdd}条");
						hasDot = true;
					}
					if (fileMod > 0)
					{
						if (hasDot)
							sb.Append("，");
						sb.Append($"变更 {fileMod}条");
						hasDot = true;
					}
					if (fileRemove > 0)
					{
						if (hasDot)
							sb.Append("，");
						sb.Append($"删减 {fileRemove}条");
						hasDot = true;
					}
					if (hasDot)
						sb.AppendLine("。<br/>");
					sb.AppendLine("<br/>");

					foreach (var changelog in list)
					{
						sb.AppendLine(changelog.ToString());
					}

					sb.AppendLine("<br/><br/>");
				}
				else
				{
					unchangedList.Add(file);
				}
			}

			if (unchangedList.Count > 0)
			{
				sb.AppendLine($"<h2>无变化的文件</h2>");
				foreach (var file in unchangedList)
				{
					sb.AppendLine($"{file}<br/>");
				}
				sb.AppendLine("<br/><br/>");
			}

			sb.AppendLine("</body>");
			sb.AppendLine("</html>");
			File.WriteAllText(Path.Combine(OutputDir, REPORT_FILENAME), sb.ToString(), Encoding.UTF8);

			sb = new StringBuilder("已将文本输出至");
			sb.AppendLine($"{OutputDir}");
			sb.AppendLine("统计：");
			sb.AppendLine($"新增 {_count_add}条");
			sb.AppendLine($"变更 {_count_modify}条");
			sb.AppendLine($"删减 {_count_remove}条");
			sb.AppendLine($"详细报告见输出目录下的{REPORT_FILENAME}");

			MessageBox.Show(sb.ToString(), "文本输出完成", MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		private abstract class Changelog
		{
			public string Key;
			public string Original;

			public abstract void ApplyStats(ref int add, ref int modify, ref int remove);
		}

		private class AddChangelog : Changelog
		{
			public override string ToString()
			{
				var str = $"<b><font color=\"#0000FF\">[新增]</font></b> {Key}";
				if (Key == Original)
					return $"{str}<br/>";
				return $"{str} <b>|</b> {Original}<br/>";
			}

			public override void ApplyStats(ref int add, ref int modify, ref int remove)
			{
				add++;
			}
		}

		private class ModifiedChangelog : Changelog
		{
			public string NewOriginal;
			public string Translation;

			public override string ToString()
			{
				var str = $"<b><font color=\"#00FF00\">[变更]</font></b> {Key} <b>|</b> {Original} <b>=></b> {NewOriginal}";
				if (string.IsNullOrEmpty(Translation))
					return $"{str} (<b>无译文</b>)<br/>";
				return $"{str} (<b>现译文:</b> <i>{Translation}</i>)<br/>";
			}

			public override void ApplyStats(ref int add, ref int modify, ref int remove)
			{
				modify++;
			}
		}

		private class RemovedChangelog : Changelog
		{
			public string Translation;

			public override string ToString()
			{
				var str = $"<b><font color=\"#FF0000\">[删减]</font></b> {Key}";
				if (Key != Original)
					str = $"{str} <b>|</b> {Original}";
				if (string.IsNullOrEmpty(Translation))
					return $"{str} (<b>无译文</b>)<br/>";
				return $"{str} (<b>现译文:</b> <i>{Translation}</i>)<br/>";
			}

			public override void ApplyStats(ref int add, ref int modify, ref int remove)
			{
				remove++;
			}
		}
	}
}