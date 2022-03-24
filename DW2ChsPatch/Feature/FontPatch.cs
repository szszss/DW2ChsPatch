using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class FontPatch
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(AccessTools.Method("DistantWorlds2.ScaledRenderer:SetupFonts"),
				null, null,
				new HarmonyMethod(typeof(FontPatch), nameof(Transpiler)));
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			const int maxRegular = 5;
			const int maxBold = 5;
			int regularCount = 0;
			int boldCount = 0;

			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldfld,
				AccessTools.Field(AccessTools.TypeByName("DistantWorlds2.ScaledRenderer"), "_Game"));
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.PropertyGetter(AccessTools.TypeByName("Xenko.Games.GameBase"), "Content"));
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.PropertyGetter(AccessTools.TypeByName("Xenko.Core.Serialization.Contents.ContentManager"), "FileProvider"));
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.PropertyGetter(AccessTools.TypeByName("Xenko.Core.IO.DatabaseFileProvider"), "ObjectDatabase"));
			yield return new CodeInstruction(OpCodes.Ldstr, "ChsFonts");
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.Method("Xenko.Core.Storage.ObjectDatabase:LoadBundle", new[] { typeof(string) }));
			yield return new CodeInstruction(OpCodes.Call,
				AccessTools.Method("System.Threading.Tasks.Task:Wait", new Type[0]));

			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && str.StartsWith("UserInterface/Fonts/Font"))
				{
					instruction.operand = str.Replace("UserInterface/Fonts", "ChsFonts");
					// if it's regular font
					/*if (!str.EndsWith("Bold"))
					{
						switch (regularCount)
						{
							case 0:
								instruction.operand = "ChsFonts/Font8";
								break;
							case 1:
								instruction.operand = "ChsFonts/Font12";
								break;
							case 2:
								instruction.operand = "ChsFonts/Font16";
								break;
							case 3:
								instruction.operand = "ChsFonts/Font20";
								break;
							case 4:
								instruction.operand = "ChsFonts/Font40";
								break;
						}
						regularCount++;
					}
					else
					{
						switch (boldCount)
						{
							case 0:
								instruction.operand = "ChsFonts/Font8Bold";
								break;
							case 1:
								instruction.operand = "ChsFonts/Font12Bold";
								break;
							case 2:
								instruction.operand = "ChsFonts/Font16Bold";
								break;
							case 3:
								instruction.operand = "ChsFonts/Font20Bold";
								break;
							case 4:
								instruction.operand = "ChsFonts/Font40Bold";
								break;
						}
						boldCount++;
					}*/
				}
				/*else if (instruction.opcode == OpCodes.Callvirt && instruction.operand is MethodInfo method && method.Name == "Add")
				{
					if (regularCount > maxRegular && (boldCount <= 0 || boldCount > maxBold))
					{
						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Pop);
						continue;
					}
				}*/
				yield return instruction;
			}
		}
	}
}