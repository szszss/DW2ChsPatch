using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using System.Xml;

namespace DW2ChsPatch.Feature
{
	public static class TourFix
	{
		private static string _researchScreenName = "Research Screen";

		private static string _shipDesignScreenName = "Ship Design Screen";

		public static void Patch(Harmony harmony)
		{
			var nodes = MainClass.HardcodedTextDoc.SelectNodes("//TourScreenTitle");
			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					var attr = node.Attributes?["Key"];
					if (attr != null)
					{
						switch (attr.Value)
						{
							case "Research Screen":
								_researchScreenName = node.InnerText;
								break;
							case "Ship Design Screen":
								_shipDesignScreenName = node.InnerText;
								break;
						}
					}
				}
			}

			harmony.Patch(AccessTools.Method("DistantWorlds.UI.ResearchTree:LaunchTourClick"),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.UI.ResearchTree"), 
					info => info.Name == "Render" && info.GetParameters().Length == 4),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds2.ShipDesignRenderer:LaunchTourClick"),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds2.ShipDesignRenderer:Update"),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds2.ShipDesignRenderer:DrawCorePostScene"),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds2.ScaledRenderer:LaunchTourForSection"),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.Method("DistantWorlds2.ScaledRenderer:LaunchTourForSection"),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));

			harmony.Patch(AccessTools.FirstMethod(AccessTools.TypeByName("DistantWorlds.UI.UserInterfaceController"),
					info => info.Name == "ShowTourDialog" && info.GetParameters().Length == 3),
				null, null, new HarmonyMethod(typeof(TourFix), nameof(Transpiler)));
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str)
				{
					switch (str)
					{
						case "Research Screen":
							instruction.operand = _researchScreenName;
							break;
						case "Ship Design Screen":
							instruction.operand = _shipDesignScreenName;
							break;
					}
				}
				yield return instruction;
			}
		}
	}
}