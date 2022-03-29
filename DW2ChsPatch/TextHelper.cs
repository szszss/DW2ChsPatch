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

		public static string UniteNewline(this string str)
		{
			if (str.IndexOf('\n') >= 0)
				return new StringBuilder(str).Replace("\\n", "\n").Replace("\r\n", "\n").Replace("\n", "\\n").ToString();
			return str;
		}
	}
}