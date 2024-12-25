using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml;

namespace DW2ChsPatch.Feature
{
	public class ShakturiPatch
	{
		public static string AllianceName_Axis = "Shaktur Axis";
		public static string AllianceName_Alliance = "Freedom Alliance";

		public static void Patch(Harmony harmony)
		{
			var node = MainClass.HardcodedTextDoc?.SelectSingleNode("//Alliance[@Key='Shaktur Axis']");
			if (node != null)
				AllianceName_Axis = node.InnerText;

			node = MainClass.HardcodedTextDoc?.SelectSingleNode("//Alliance[@Key='Freedom Alliance']");
			if (node != null)
				AllianceName_Alliance = node.InnerText;

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:PerformGalaxyStartup"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixRaceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturiBeaconControlWormholes"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixRaceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:ShakturiEmergeFromRifts"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixRaceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckSendShakturiVanguardFleet"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixRaceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturiDiplomaticIntrigue"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixRaceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturiPlanetDestroyerFleet"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixRaceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckAlliancesEnabled"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturiDiplomaticIntrigue"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckFormShakturAxis"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckFormFreedomAlliance"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CreateShakturiAlliance"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CreateFreedomAlliance"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturiInfiltrationFriendlyEmpire"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturiInfiltratorPlayerColony"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckInitiateFoilShakturiSpy"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckShakturAxisBeginsWar"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckGrandStrategyMeeting"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckEmpireIsFreedomAllianceMemberAtWarWithShakturAxis"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.ShakturiStoryController:CheckEmpiresAreOpposingMembersOfFreedomShakturiAlliances"),
				null, null,
				new HarmonyMethod(typeof(ShakturiPatch), nameof(FixAllianceNameTranspiler)));
		}

		private static IEnumerable<CodeInstruction> FixRaceNameTranspiler(
			IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (instruction.opcode == OpCodes.Ldstr && RacePatch.IsImportantRace(instruction.operand as string))
				{
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(RacePatch), nameof(RacePatch.GetRaceTranslatedName)));
				}
			}
		}

		private static IEnumerable<CodeInstruction> FixAllianceNameTranspiler(
			IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str)
				{
					if (str == "Shaktur Axis")
						instruction.operand = AllianceName_Axis;
					else if (str == "Freedom Alliance")
						instruction.operand = AllianceName_Alliance;
				}
				yield return instruction;
			}
		}
	}
}