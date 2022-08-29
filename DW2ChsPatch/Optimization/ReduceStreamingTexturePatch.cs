using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DW2ChsPatch.Optimization
{
	public static class ReduceStreamingTexturePatch
	{
		private static PropertyInfo _storageProperty;

		private static PropertyInfo _urlProperty;

		private static PropertyInfo _totalMipLevelsProperty;

		private static PropertyInfo _totalWidthProperty;

		private static PropertyInfo _totalHeightProperty;

		private static bool _optimizeShipTex;

		private static bool _optimizeOtherTex;

		public static void Patch(Harmony harmony,
			bool optimizeShipTex,
			bool optimizeOtherTex)
		{
			_storageProperty = AccessTools.Property("Stride.Streaming.StreamableResource:Storage");
			_urlProperty = AccessTools.Property("Stride.Core.Streaming.ContentStorage:Url");
			_totalMipLevelsProperty = AccessTools.Property("Stride.Streaming.StreamingTexture:TotalMipLevels");
			_totalWidthProperty = AccessTools.Property("Stride.Streaming.StreamingTexture:TotalWidth");
			_totalHeightProperty = AccessTools.Property("Stride.Streaming.StreamingTexture:TotalHeight");

			_optimizeShipTex = optimizeShipTex;
			_optimizeOtherTex = optimizeOtherTex;

			harmony.Patch(AccessTools.Method("Stride.Streaming.StreamingTexture:CalculateTargetResidency"),
				null, null,
				new HarmonyMethod(typeof(ReduceStreamingTexturePatch), nameof(CalculateTargetResidencyTranspiler)));
		}

		private static IEnumerable<CodeInstruction> CalculateTargetResidencyTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ret)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, _storageProperty.GetMethod);
					yield return new CodeInstruction(OpCodes.Callvirt, _urlProperty.GetMethod);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, _totalMipLevelsProperty.GetMethod);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, _totalWidthProperty.GetMethod);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, _totalHeightProperty.GetMethod);
					yield return new CodeInstruction(OpCodes.Call, 
						AccessTools.Method(typeof(ReduceStreamingTexturePatch), nameof(ReduceStreamingTextureResidency)));
				}
				yield return instruction;
			}
		}

		private static int ReduceStreamingTextureResidency(int residency, string url, 
			int maxMipmap, int width, int height)
		{
			if (_optimizeShipTex && url.StartsWith("Ships/"))
			{
				if (maxMipmap >= 13)
					residency = Math.Min(residency, 12); // Mipmap 12 = 2048 x 2048
				else
					residency = Math.Min(residency, 11); // Mipmap 11 = 1024 x 1024
			}
			else if (_optimizeOtherTex && url.StartsWith("Environment/Asteroids/"))
			{
				residency = Math.Min(residency, 11); // Mipmap 11 = 1024 x 1024
			}
			else if (_optimizeOtherTex && url.StartsWith("Environment/Components/"))
			{
				residency = Math.Min(residency, 11); // Mipmap 11 = 1024 x 1024
			}
			else if (_optimizeShipTex && url.StartsWith("PlanetDestroyer"))
			{
				residency = Math.Min(residency, 11); // Mipmap 11 = 1024 x 1024
			}
			else if (_optimizeShipTex && url.StartsWith("Creatures/"))
			{
				residency = Math.Min(residency, 12); // Mipmap 12 = 2048 x 2048
			}
			else if (_optimizeOtherTex && url.StartsWith("Effects/Weapons"))
			{
				residency = Math.Min(residency, 12); // Mipmap 12 = 2048 x 2048
			}
			else if (_optimizeOtherTex && url.StartsWith("Extras/"))
			{
				residency = Math.Min(residency, 10); // Mipmap 10 = 512x512
			}
			return residency;
		}
	}
}