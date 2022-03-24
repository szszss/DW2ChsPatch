using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DW2ChsPatch.TextProcess
{
	public class JsonText
	{
		private int _currentIndex = 0;

		private Dictionary<string, JsonTextItem> _items = new Dictionary<string, JsonTextItem>();

		public void SetString(string key, string original, string translation = null, string context = null)
		{
			_items[key] = new JsonTextItem(_currentIndex++, key, original, translation, context);
		}

		public string GetString(string key, string original)
		{
			if (_items.TryGetValue(key, out var item))
				return string.IsNullOrEmpty(item.Translation) ? original : item.Translation;
			return original;
		}

		public void ExportToFile(string filepath)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.Formatting = Formatting.Indented;
			var array = _items.Values.ToArray();
			Array.Sort(array);
			var str = JsonConvert.SerializeObject(array, settings).Replace("\\r\\n", "\\n");
			Directory.CreateDirectory(Path.GetDirectoryName(filepath));
			File.WriteAllText(filepath, str, Encoding.UTF8);
		}

		public class JsonTextItem : IComparable<JsonTextItem>
		{
			[JsonProperty("key")]
			public string Key { private set; get; }

			[JsonProperty("original")]
			public string Original { private set; get; }

			[JsonProperty("translation")]
			public string Translation { set; get; }

			[JsonProperty("context")]
			public string Context { set; get; }

			[JsonIgnore]
			protected int Index { private set; get; }

			internal JsonTextItem(int index, string key, string original, string translation = null, string context = null)
			{
				this.Index = index;
				this.Key = key;
				this.Original = original;
				this.Translation = translation;
				this.Context = context;
			}

			public int CompareTo(JsonTextItem other)
			{
				if (ReferenceEquals(this, other)) return 0;
				if (ReferenceEquals(null, other)) return 1;
				return Index.CompareTo(other.Index);
			}
		}
	}
}