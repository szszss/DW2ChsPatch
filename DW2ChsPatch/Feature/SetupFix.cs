using System.Threading;
using DW2ChsPatch.TextProcess;
using HarmonyLib;

namespace DW2ChsPatch.Feature
{
	public static class SetupFix
	{
		private static Thread _thread;

		public static void Patch(Harmony harmony, Thread thread)
		{
			_thread = thread;

			harmony.Patch(AccessTools.Method("Stride.Games.GameBase:InitializeBeforeRun"),
				new HarmonyMethod(typeof(SetupFix), nameof(Prefix)));
		}

		private static void Prefix()
		{
			_thread.Join();
		}
	}
}