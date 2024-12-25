using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class RacePatch
	{
		private static bool _skipCheck = false;

		private static readonly Dictionary<string, string> _raceOriginalNames = new Dictionary<string, string>();
		private static readonly Dictionary<string, string> _raceTranslatedNames = new Dictionary<string, string>();
		private static readonly HashSet<string> _playableRaces = new HashSet<string>();

		public static void InitRace()
		{
			var nodes = MainClass.HardcodedTextDoc?.SelectNodes("//PlayableRace");
			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					_playableRaces.Add(node.InnerText);
				}
			}
		}

		public static void Patch(Harmony harmony, bool skipCheck)
		{
			_skipCheck = skipCheck;

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.StartNewGameDialog:DisableUnavailableRaces"),
				new HarmonyMethod(typeof(RacePatch), nameof(DisableUnavailableRacesPrefix)), null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.TextHelper:GenerateEmpireName"),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(GenerateEmpireNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.CharacterAnimationList:GenerateDefault"),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.CharacterAnimationList:GenerateAnimation"),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.CharacterRoomList:GenerateDefault"),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.Types.Galaxy"), 
					x => x.Name.Contains("GenerateEmpire") && x.GetParameters().Length > 14),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:GenerateEmpireSingle"),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ModelEffectHelper:ObtainModelForWeaponBlast"),
				null, null,
				new HarmonyMethod(typeof(RacePatch), nameof(RaceNameTranslationToOriginTranspiler)));
		}

		private static IEnumerable<CodeInstruction> RaceNameTranslationToOriginTranspiler(
			IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (instruction.opcode == OpCodes.Callvirt
				    && instruction.operand is MethodInfo method
				    && method.Name.Contains("Name")
				    && method.DeclaringType.Name.Contains("Race"))
				{
					yield return new CodeInstruction(OpCodes.Call, 
						AccessTools.Method(typeof(RacePatch), nameof(GetRaceOriginalName)));
				}
			}
		}

		private static IEnumerable<CodeInstruction> GenerateEmpireNameTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt &&
				    instruction.operand is MethodInfo method &&
				    method.Name == "ToLowerInvariant")
				{
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(RacePatch), nameof(GetRaceOriginalName)));
				}
				yield return instruction;
			}
		}

		private static bool DisableUnavailableRacesPrefix()
		{
			return !_skipCheck;
		}

		public static void SetRaceOriginalName(string oldName, string newName)
		{
			_raceOriginalNames[newName] = oldName;
			_raceTranslatedNames[oldName] = newName;
		}

		public static string GetRaceOriginalName(string newName)
		{
			return _raceOriginalNames.TryGetValue(newName, out var name) ? name : newName;
		}

		public static string GetRaceTranslatedName(string oldName)
		{
			return _raceTranslatedNames.TryGetValue(oldName, out var name) ? name : oldName;
		}

		public static bool IsPlayableRace(string oldName)
		{
			return _playableRaces.Contains(oldName);
		}

		public static bool IsImportantRace(string oldName)
		{
			return IsPlayableRace(oldName) || "Shakturi" == oldName;
		}
	}
}