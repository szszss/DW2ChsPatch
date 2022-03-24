using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class TextWrapPatch
	{
		private static Regex _chineseRegex;

		private const string UNICODE_CJK_RANGE = 
			"\\u2E80-\\u2EFF" +
			"\\u2F00-\\u2FDF" +
			"\\u3000-\\u303F" +
			"\\u31C0-\\u31EF" +
			"\\u3200-\\u32FF" +
			"\\u3300-\\u33FF" +
			"\\u3400-\\u4DBF" +
			"\\u4DC0-\\u4DFF" +
			"\\u4E00-\\u9FBF" +
			"\\uF900-\\uFAFF" +
			"\\uFE30-\\uFE4F" +
			"\\uFF00-\\uFFEF";

		public static void Patch(Harmony harmony)
		{
			_chineseRegex =
				new Regex($"[{UNICODE_CJK_RANGE}]{{1,2}}|[^\\s{UNICODE_CJK_RANGE}]+|\\s", RegexOptions.Compiled);

			var method1 = AccessTools.FirstMethod(
				AccessTools.TypeByName("DistantWorlds.Types.DrawingHelper"),
				method =>
				{
					var parameters = method.GetParameters();
					return method.Name.Contains("MeasureStringDropShadowWordWrapWithSize") &&
					       parameters.Length == 4 && parameters[3].ParameterType.Name.Contains("List");

				});

			harmony.Patch(method1, null, null,
				new HarmonyMethod(typeof(TextWrapPatch), nameof(TextWrapTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(
					AccessTools.TypeByName("DistantWorlds.Types.DrawingHelper"),
					method =>
					{
						var parameters = method.GetParameters();
						return method.Name.Contains("DrawStringDropShadowWordWrapWithSize") &&
						       parameters.Length == 10 && parameters[9].ParameterType == typeof(int);

					}), null, null,
				new HarmonyMethod(typeof(TextWrapPatch), nameof(TextWrapTranspiler)));
		}

		public static string[] SplitText(string text)
		{
			var result = _chineseRegex.Matches(text);
			var count = result.Count;
			string[] results = new string[count];

			for (var i = 0; i < count; i++)
			{
				var match = result[i];
				results[i] = text.Substring(match.Index, match.Length);
			}

			return results;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string DummyConcat(string _, string str)
		{
			return str;
		}

		private static IEnumerable<CodeInstruction> TextWrapTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var splitCount = 0;
			var firstConcat = true;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Split")
				{
					splitCount++;
					if (splitCount == 2)
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Pop);
						instruction.opcode = OpCodes.Call;
						instruction.operand = AccessTools.Method(typeof(TextWrapPatch), nameof(SplitText));
					}
				}
				else if (firstConcat &&
				         instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method2 &&
				         method2.Name == "Concat" &&
				         method2.GetParameters().Length == 2)
				{
					instruction.operand = AccessTools.Method(typeof(TextWrapPatch),
						nameof(DummyConcat));
					firstConcat = false;
				}
				yield return instruction;
			}
		}
	}
}