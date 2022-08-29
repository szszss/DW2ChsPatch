using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace DW2ChsPatch.TextProcess
{
	public class JsonText : IEnumerable<JsonText.JsonTextItem>
	{
		public static bool StoreOrderOfItems = false;

		public static bool StrictMode = false;

		public string Filename { set; get; }

		private Dictionary<string, JsonTextItem> _items = new Dictionary<string, JsonTextItem>();

		private LinkedList<JsonTextItem> _itemOrder = new LinkedList<JsonTextItem>();

		private JsonTextItem _lastTouchedItem = null;

		public int Count
		{
			get => _items.Count;
		}

		public JsonText()
		{

		}

		public JsonText(string filepath)
		{
			ImportFromFile(filepath);
		}

		public static JsonText CreateOrGetJsonText(string filename, string filepath)
		{
			JsonText json = null;

			if (TranslationTextGenerator.Enable)
			{
				json = TranslationTextGenerator.GetJson(filename);
				if (json != null)
					return json;
			}

			if (File.Exists(filepath))
				json = new JsonText(filepath);
			else if (TranslationTextGenerator.Enable)
				json = new JsonText();

			if (TranslationTextGenerator.Enable)
			{
				TranslationTextGenerator.AddJson(filename, json);
				json.Filename = filename;
			}

			return json;
		}

		/*[Obsolete]
		public void SetString(string key, string original, string translation = null, string context = null)
		{
			SetStringAfter(null, key, original, translation, context);
		}

		[Obsolete]
		public void SetStringAfter(string afterKey, string key, string original, string translation = null, string context = null)
		{
			var item = new JsonTextItem(key, original, translation, context);
			_items[key] = item;
			if (StoreOrderOfItems)
			{
				LinkedListNode<JsonTextItem> afterNode = null;
				if (!string.IsNullOrEmpty(afterKey))
					if (_items.TryGetValue(afterKey, out var after))
						afterNode = after.Node;
				item.Node = afterNode != null 
					? _itemOrder.AddAfter(afterNode, item)
					: _itemOrder.AddLast(item);
			}
		}*/

		public bool CheckOriginal(string key, string expectedOriginal)
		{
			if (_items.TryGetValue(key, out var existedItem))
				return existedItem.Original == expectedOriginal;
			return false;
		}

		public void GetString(string key, string original, out string outText)
		{
			if (string.IsNullOrEmpty(original))
			{
				outText = original;
				return;
			}

			original = original.UniteNewline();
			if (_items.TryGetValue(key, out var existedItem))
			{
				_lastTouchedItem = existedItem;
				if (existedItem.Original == original)
				{
					existedItem.Status = TextPresent.Used;
					outText = string.IsNullOrEmpty(existedItem.Translation)
						? original : existedItem.Translation;
				}
				else
				{
					outText = StrictMode ? original : existedItem.Translation;
					if (TranslationTextGenerator.Enable)
					{
						existedItem.Status = TextPresent.Modified;
						existedItem.Context =
							$"旧原文: {existedItem.Original}\n\n新原文: {original}\n\n旧译文: {existedItem.Translation}"; ;
						TranslationTextGenerator.RecordModifiedItem(Filename, key, 
							existedItem.Original, original, existedItem.Translation);
						existedItem.Original = original;
						existedItem.Translation = null;
						existedItem.Stage = 0;
					}
				}
			}
			else
			{
				outText = original;
				if (TranslationTextGenerator.Enable)
				{
					var item = new JsonTextItem(key, original);
					item.Status = TextPresent.New;
					TranslationTextGenerator.RecordAddedItem(Filename, key, original);
					_items[key] = item;
					if (StoreOrderOfItems)
					{
						item.Node = _lastTouchedItem != null
							? _itemOrder.AddAfter(_lastTouchedItem.Node, item)
							: _itemOrder.AddLast(item);
					}
					_lastTouchedItem = item;
				}
			}
		}

		public void GetStringArray(string keyPrefix, string[] original, out string[] outTexts, bool appendUnderline = true)
		{
			outTexts = new string[original.Length];

			for (var i = 0; i < original.Length; i++)
			{
				var key = appendUnderline 
					? $"{keyPrefix}_{i}"
					: $"{keyPrefix}{i}";
				var ori = original[i];
				GetString(key, ori, out var text);
				outTexts[i] = text;
			}
		}

		/*public string GetString(string key, string original)
		{
			return GetString(key, original, 0);
		}

		public string GetString(string key, string original, int minStage)
		{
			if (_items.TryGetValue(key, out var item) && item.Stage >= minStage)
				return string.IsNullOrEmpty(item.Translation) ? original : item.Translation;
			return original;
		}*/

		public bool GetOriginalAndTranslatedString(string key, out string original, out string translated)
		{
			if (_items.TryGetValue(key, out var item))
			{
				original = item.Original;
				translated = item.Translation;
				return true;
			}

			original = null;
			translated = null;
			return false;
		}

		public void ImportFromFile(string filepath)
		{
			if (!File.Exists(filepath))
				return;

			var str = File.ReadAllText(filepath, Encoding.UTF8);
			var array = JsonSerializer.Deserialize<JsonTextItem[]>(str);

			if (array?.Length > 0)
			{
				if (StoreOrderOfItems)
				{
					foreach (var item in array.Distinct(new JsonTextItemComparer()))
					{
						if (!string.IsNullOrEmpty(item.Original))
							item.Original = item.Original.UniteNewline();
						item.Node = _itemOrder.AddLast(item);
						_items[item.Key] = item;
					}
				}
				else
				{
					foreach (var item in array)
					{
						if (!string.IsNullOrEmpty(item.Original))
							item.Original = item.Original.UniteNewline();
						_items[item.Key] = item;
					}
				}
			}
		}

		public void ExportToFile(string filepath)
		{
			/*
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.Formatting = Formatting.Indented;
			*/

			JsonTextItem[] array = StoreOrderOfItems ? _itemOrder.ToArray() : _items.Values.ToArray();
			foreach (var item in array)
			{
				item.Original = item.Original?.Replace("\n", "\\n");
				item.Translation = item.Translation?.Replace("\n", "\\n");
			}

			var setting = new TextEncoderSettings();
			setting.AllowRange(UnicodeRanges.All);

			var str = JsonSerializer.Serialize(array, new JsonSerializerOptions()
			{
				Encoder = JavaScriptEncoder.Create(setting),
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = true
			}).Replace("\\r\\n", "\\n");
			Directory.CreateDirectory(Path.GetDirectoryName(filepath));
			File.WriteAllText(filepath, str, Encoding.UTF8);
		}

		public Dictionary<string, string> CreateOriginalTranslationMappingMap()
		{
			var map = new Dictionary<string, string>();

			foreach (var pair in _items)
			{
				var item = pair.Value;
				if (!string.IsNullOrEmpty(item.Original) 
				    && !string.IsNullOrEmpty(item.Translation))
					map[item.Original] = item.Translation;
			}

			return map;
		}

		public Dictionary<string, string> CreateOriginalTranslationMappingMap(Predicate<JsonTextItem> predicate)
		{
			var map = new Dictionary<string, string>();

			foreach (var pair in _items)
			{
				var item = pair.Value;
				if (!string.IsNullOrEmpty(item.Original) 
				    && !string.IsNullOrEmpty(item.Translation) 
				    && predicate(item))
					map[item.Original] = item.Translation;
			}

			return map;
		}

		public enum TextPresent : ushort
		{
			Unused = 0,
			Modified = 1,
			New = 2,
			Used = 3
		}

		public class JsonTextItem
		{
			[JsonPropertyName("key")]
			public string Key { set; get; }

			[JsonPropertyName("original")]
			public string Original { set; get; }

			[JsonPropertyName("translation")]
			public string Translation { set; get; }

			[JsonPropertyName("context")]
			public string Context { set; get; }

			[JsonPropertyName("stage")]
			public short Stage { set; get; }

			[JsonIgnore]
			public TextPresent Status { set; get; } = TextPresent.Unused;

			[JsonIgnore]
			internal LinkedListNode<JsonTextItem> Node { set; get; }

			internal JsonTextItem(string key, string original, string translation = null,
				string context = null, int stage = 0)
			{
				this.Key = key;
				this.Original = original;
				this.Translation = translation;
				this.Context = context;
				this.Stage = (short) stage;
			}

			public JsonTextItem()
			{
			}
		}

		private class JsonTextItemComparer : IEqualityComparer<JsonTextItem>
		{
			public bool Equals(JsonTextItem x, JsonTextItem y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return x.Key == y.Key;
			}

			public int GetHashCode(JsonTextItem obj)
			{
				return (obj.Key != null ? obj.Key.GetHashCode() : 0);
			}
		}

		public IEnumerator<JsonTextItem> GetEnumerator()
		{
			if (StoreOrderOfItems)
				return _itemOrder.GetEnumerator();
			return _items.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (StoreOrderOfItems)
				return _itemOrder.GetEnumerator();
			return _items.Values.GetEnumerator();
		}
	}
}