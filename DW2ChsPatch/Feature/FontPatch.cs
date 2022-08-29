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
			/*
			 * this._Game.Content.FileProvider.ObjectDatabase.LoadBundle("ChsFonts").Wait()
			 */
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldfld,
				AccessTools.Field(AccessTools.TypeByName("DistantWorlds2.ScaledRenderer"), "_Game"));
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.PropertyGetter(AccessTools.TypeByName("Stride.Games.GameBase"), "Content"));
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.PropertyGetter(AccessTools.TypeByName("Stride.Core.Serialization.Contents.ContentManager"), "FileProvider"));
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.PropertyGetter(AccessTools.TypeByName("Stride.Core.IO.DatabaseFileProvider"), "ObjectDatabase"));
			yield return new CodeInstruction(OpCodes.Ldstr, "ChsFonts");
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.Method("Stride.Core.Storage.ObjectDatabase:LoadBundle", new[] { typeof(string) }));
			yield return new CodeInstruction(OpCodes.Call,
				AccessTools.Method("System.Threading.Tasks.Task:Wait", new Type[0]));

			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && str.StartsWith("UserInterface/Fonts/Font"))
				{
					instruction.operand = str.Replace("UserInterface/Fonts", "ChsFonts");
				}
				yield return instruction;
			}
		}
	}
}