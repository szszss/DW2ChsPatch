using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class TextBoxPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds.UI.DWTextBox:AcceptKeyPresses"),
				null, null,
				new HarmonyMethod(typeof(TextBoxPatch), nameof(Transpiler)));
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "Concat" &&
				    method.GetParameters().Length == 3)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldflda, 
						AccessTools.Field("DistantWorlds.UI.DWTextBox:CaretIndex"));
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(TextBoxPatch), nameof(ProcessInput)));
					continue;
				}
				yield return instruction;
			}
		}

		public static string ProcessInput(string textBefore, string inputText, string textAfter,
			bool ctrlPressed, ref int caret)
		{
			if (ctrlPressed && inputText == "v")
			{
				try
				{
					// https://stackoverflow.com/questions/518701/clipboard-gettext-returns-null-empty-string
					string result = "";
					Thread staThread = new Thread(
						delegate ()
						{
							try
							{
								var text = Clipboard.GetText();
								if (string.IsNullOrEmpty(text))
								{
									var obj = Clipboard.GetDataObject();
									if (obj != null)
									{
										var data = obj.GetData(typeof(string));
										if (data != null && data is string str)
										{
											text = str;
										}
									}
								}
								Volatile.Write(ref result, text);
							}

							catch (Exception)
							{
								// ignored
							}
						});
					staThread.SetApartmentState(ApartmentState.STA);
					staThread.Start();
					staThread.Join();
					inputText = Volatile.Read(ref result);
				}
				catch (Exception)
				{
					// ignored
				}
			}
			caret += inputText.Length;
			caret -= 1; // cancel CaretIndex++;
			return textBefore + inputText + textAfter;
		}
	}
}