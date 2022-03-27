using System.Text;

namespace DW2ChsPatch
{
	public static class TextHelper
	{
		public static string ConcatThreeStringWithoutSpace(string s1, string s2, string s3)
		{
			var sb = new StringBuilder(s1.Length + s2.Length + s3.Length);
			if (!string.IsNullOrWhiteSpace(s1))
				sb.Append(s1);
			if (!string.IsNullOrWhiteSpace(s2))
				sb.Append(s2);
			if (!string.IsNullOrWhiteSpace(s3))
				sb.Append(s3);
			return sb.ToString();
		}

		public static string ConcatManyStringWithoutSpace(string[] str)
		{
			var sb = new StringBuilder();
			foreach (var s in str)
			{
				if (!string.IsNullOrWhiteSpace(s))
					sb.Append(s);
			}
			return sb.ToString();
		}

		public static string ConcatThreeStringWithoutMidSpace(string s1, string _, string s3)
		{
			return s1 + s3;
		}

		public static void CheckAndGetTranslatedString(string newOrigial, string oldOriginal, string oldTranslation,
			out string newTranslation, out string context)
		{
			if (newOrigial == oldOriginal)
			{
				newTranslation = oldTranslation;
				context = null;
			}
			else
			{
				newTranslation = null;
				context = $"旧原文: {oldOriginal}\n\n新原文: {newOrigial}\n\n旧译文: {oldTranslation}";
			}
		}
	}
}