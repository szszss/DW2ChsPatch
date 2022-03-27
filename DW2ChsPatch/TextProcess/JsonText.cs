using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DW2ChsPatch.TextProcess
{
	public class JsonText : IEnumerable<JsonText.JsonTextItem>
	{
		public static bool StoreOrderOfItems = false;

		private Dictionary<string, JsonTextItem> _items = new Dictionary<string, JsonTextItem>();

		private LinkedList<JsonTextItem> _itemOrder = new LinkedList<JsonTextItem>();

		public JsonText()
		{

		}

		public JsonText(string filepath)
		{
			ImportFromFile(filepath);
		}

		public void SetString(string key, string original, string translation = null, string context = null)
		{
			SetStringAfter(null, key, original, translation, context);
		}

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
		}

		public string GetString(string key, string original)
		{
			return GetString(key, original, 0);
		}

		public string GetString(string key, string original, int minStage)
		{
			if (_items.TryGetValue(key, out var item) && item.Stage >= minStage)
				return string.IsNullOrEmpty(item.Translation) ? original : item.Translation;
			return original;
		}

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
			var array = JsonConvert.DeserializeObject<JsonTextItem[]>(str);

			if (array?.Length > 0)
			{
				if (StoreOrderOfItems)
				{
					foreach (var item in array.Distinct(new JsonTextItemComparer()))
					{
						item.Node = _itemOrder.AddLast(item);
						_items[item.Key] = item;
					}
				}
				else
				{
					foreach (var item in array)
					{
						_items[item.Key] = item;
					}
				}
			}
		}

		public void ExportToFile(string filepath)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.Formatting = Formatting.Indented;

			JsonTextItem[] array = StoreOrderOfItems ? _itemOrder.ToArray() : _items.Values.ToArray();
			var str = JsonConvert.SerializeObject(array, settings).Replace("\\r\\n", "\\n");
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

		public class JsonTextItem
		{
			[JsonProperty("key")]
			public string Key { private set; get; }

			[JsonProperty("original")]
			public string Original { private set; get; }

			[JsonProperty("translation")]
			public string Translation { set; get; }

			[JsonProperty("context")]
			public string Context { set; get; }

			[JsonProperty("stage")]
			public int Stage { set; get; }

			[JsonIgnore]
			internal LinkedListNode<JsonTextItem> Node { set; get; }

			internal JsonTextItem(string key, string original, string translation = null,
				string context = null, int stage = 0)
			{
				this.Key = key;
				this.Original = original;
				this.Translation = translation;
				this.Context = context;
				this.Stage = stage;
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