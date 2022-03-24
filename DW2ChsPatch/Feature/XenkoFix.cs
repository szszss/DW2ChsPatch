using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class XenkoFix
	{
		private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		private static int _fontTexSize;

		public static void Patch(Harmony harmony, int fontTexSize)
		{
			_fontTexSize = fontTexSize;

			var klass = AccessTools.TypeByName("Xenko.Graphics.SpriteFont");
			harmony.Patch(AccessTools.Method(klass, "InternalDraw"),
				null, null, new HarmonyMethod(typeof(XenkoFix), nameof(CallForEachGlyphTranspiler)));
			harmony.Patch(AccessTools.Method(klass, "InternalUIDraw"),
				null, null, new HarmonyMethod(typeof(XenkoFix), nameof(CallForEachGlyphTranspiler)));
			harmony.Patch(AccessTools.FirstMethod(klass, x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 2 && parameters[0].ParameterType.Name.Contains("StringProxy");
				}),
				null, null, new HarmonyMethod(typeof(XenkoFix), nameof(CallForEachGlyphTranspiler)));

			harmony.Patch(AccessTools.Method("Xenko.Graphics.Font.RuntimeRasterizedSpriteFont:GetGlyph"),
				null, null, new HarmonyMethod(typeof(XenkoFix), nameof(GetGlyphTranspiler)));

			harmony.Patch(AccessTools.Method("Xenko.Graphics.Font.FontSystem:Load"),
				null, null, new HarmonyMethod(typeof(XenkoFix), nameof(LoadTranspiler)));
		}

		private static IEnumerable<CodeInstruction> CallForEachGlyphTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "ForEachGlyph")
				{
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(XenkoFix), nameof(Lock)));
					yield return instruction;
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(XenkoFix), nameof(Unlock)));
					continue;
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> GetGlyphTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt &&
				    instruction.operand is MethodInfo method &&
				    method.Name.Contains("GenerateBitmap"))
				{
					yield return new CodeInstruction(OpCodes.Pop);
					yield return new CodeInstruction(OpCodes.Ldc_I4_1);
				}
				yield return instruction;
			}
		}

		private static IEnumerable<CodeInstruction> LoadTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_I4 &&
				    instruction.operand is int value &&
				    value == 1024)
				{
					instruction.operand = _fontTexSize;
				}
				yield return instruction;
			}
		}

		//private static Thread _currentThread = null;

		public static void Lock()
		{
			/*var cur = Thread.CurrentThread;
			if (_currentThread != null && cur != _currentThread)
			{
				Thread.MemoryBarrier();
			}
			_currentThread = cur;*/
			_lock.EnterWriteLock();
		}

		public static void Unlock()
		{
			_lock.ExitWriteLock();
		}
	}
}