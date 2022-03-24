using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class CharacterNamePatch
	{
		private static string _separator = " ";

		public static void Patch(Harmony harmony, string separator)
		{
			_separator = separator;

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Character:GenerateName"),
				null, null,
				new HarmonyMethod(typeof(CharacterNamePatch), nameof(GenerateNameTranspiler)));
		}

		private static IEnumerable<CodeInstruction> GenerateNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
				    instruction.operand is string str)
				{
					string newStr = str;
					switch (str)
					{
						case "New Character": newStr = "新人物"; break;
					}

					instruction.operand = newStr;
				}
				else if (instruction.opcode == OpCodes.Call &&
				         instruction.operand is MethodInfo method &&
				         method.Name == "Concat" &&
				         method.GetParameters().Length == 3)
				{
					instruction.operand = AccessTools.Method(typeof(CharacterNamePatch), nameof(ConcatCharacterName));
				}
				yield return instruction;
			}
		}

		public static string ConcatCharacterName(string firstname, string _, string surname)
		{
			return firstname + _separator + surname;
		}
	}
}