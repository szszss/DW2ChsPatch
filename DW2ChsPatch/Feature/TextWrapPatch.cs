using System;
using System.Collections.Generic;
using System.Linq;
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

		private static Regex _nextwordRegex;

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

			_nextwordRegex =
				new Regex("\\S", RegexOptions.Compiled);

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
						       parameters.Length == 13 && parameters[10].ParameterType == typeof(int);

					}), null, null,
				new HarmonyMethod(typeof(TextWrapPatch), nameof(TextWrapTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:FindStartOfNextWord"),
				new HarmonyMethod(typeof(TextWrapPatch), nameof(FindStartOfNextWord)));
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

		// TODO: FIXME: ctrl + right not work
		public static bool FindStartOfNextWord(out int __result, string __0, int __1, int __2)
		{
			__result = -1;
			if (__0?.Length == __2)
			{
				__result = 0;
				__2 = 0;
			}
			if (!string.IsNullOrEmpty(__0) && __1 >= 0 && __1 < __0.Length)
			{
				var match = _nextwordRegex.Match(__0, __1);
				if (match.Success)
				{
					__result = Math.Min(match.Index + __2, __0.Length - 1);
				}
				else
					__result = Math.Min(__1 + __2, __0.Length - 1);

				if (__0[__result] == '\\' &&
				    __result < __0.Length - 2 &&
				    __0[__result + 1] == 'n')
				{
					__result = __result - 1;
				}
				else if (__0[__result] == 'n' &&
				         __result > 0 &&
				         __0[__result - 1] == '\\')
				{
					__result = __result + 1;
				}
			}
			return false;
		}
	}
}