using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DW2ChsPatch.Feature;
using DW2ChsPatch.Optimization;
using DW2ChsPatch.TextProcess;
using HarmonyLib;
using XmlText = DW2ChsPatch.TextProcess.XmlText;

namespace DW2ChsPatch
{
    public class MainClass
    {
	    public void Init()
	    {
		    var textPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "chs\\");

		    var configPath = Path.Combine(textPath, "Patch.config");
		    var skipCheck = false;
		    var enableTextBoxPaste = false;
		    var fixChineseTextWrap = false;
		    var fontTexSize = 1024;
		    var chineseRandomName = false;
		    var chineseRandomShipName = false;
		    var chineseOrdinalNumber = false;
		    var chineseComponentCategoryShort = false;
		    var removeStarPostfix = false;
			var characterNameSeparator = " ";
		    var postfixForRandomShipName = "";

		    var optimizeOrb = false;
		    var optimizeShipTex = false;
		    var optimizeOtherTex = false;

			try
		    {
			    if (File.Exists(configPath))
			    {
				    var doc = new XmlDocument();
					doc.Load(configPath);

					var skipCheckNode = doc.SelectSingleNode("//SkipAvailableRaceCheck");
					skipCheck = skipCheckNode?.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

					var enableTextBoxPasteNode = doc.SelectSingleNode("//EnableTextBoxPaste");
					enableTextBoxPaste = enableTextBoxPasteNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var fixChineseTextWrapNode = doc.SelectSingleNode("//FixChineseTextWrap");
					fixChineseTextWrap = fixChineseTextWrapNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var fontTexSizeNode = doc.SelectSingleNode("//FontCacheTextureSize");
					if (fontTexSizeNode != null && int.TryParse(fontTexSizeNode.InnerText, out var fSize))
						fontTexSize = Math.Max(Math.Min(fSize, 8192), 1024);

					var chineseRandomNameNode = doc.SelectSingleNode("//EnableChineseRandomName");
					chineseRandomName = chineseRandomNameNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var chineseRandomShipNameNode = doc.SelectSingleNode("//EnableChineseRandomShipName");
					chineseRandomShipName = chineseRandomShipNameNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var chineseOrdinalNumberNode = doc.SelectSingleNode("//EnableChineseOrdinalNumber");
					chineseOrdinalNumber = chineseOrdinalNumberNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var chineseComponentCategoryShortNode = doc.SelectSingleNode("//EnableChineseOrdinalNumber");
					chineseComponentCategoryShort = chineseComponentCategoryShortNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var removeStarPostfixNode = doc.SelectSingleNode("//RemoveStarNamePostfix");
					removeStarPostfix = removeStarPostfixNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var characterNameSeparatorNode = doc.SelectSingleNode("//SeparatorBetweenPersonName");
					if (characterNameSeparatorNode != null)
						characterNameSeparator = characterNameSeparatorNode.InnerText;

					var postfixForRandomShipNameNode = doc.SelectSingleNode("//PostfixForRandomShipName");
					if (postfixForRandomShipNameNode != null)
						postfixForRandomShipName = postfixForRandomShipNameNode.InnerText;

					var optimizeOrbNode = doc.SelectSingleNode("//OptimizeOrbModel");
					optimizeOrb = optimizeOrbNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var optimizeShipTexNode = doc.SelectSingleNode("//OptimizeShipTexture");
					optimizeShipTex = optimizeShipTexNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;

					var optimizeOtherTexNode = doc.SelectSingleNode("//OptimizeOtherTexture");
					optimizeOtherTex = optimizeOtherTexNode?.InnerText.Equals(
						"true", StringComparison.OrdinalIgnoreCase) == true;
				}
			}
		    catch (Exception e)
		    {
			    var sb = new StringBuilder("汉化补丁配置文件存在一个错误，");
			    if (e is XmlException xmle)
			    {
				    sb.Append($"位于第{xmle.LineNumber}行，");
			    }

			    sb.AppendLine($"错误信息为{e.Message}");
			    sb.Append("程序会以默认模式运行，大部分汉化效果会被禁用。");

				MessageBox.Show(sb.ToString(), "配置文件错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
		    }

			try
			{
				var harmony = new Harmony("DW2ChsPatch");
				FontPatch.Patch(harmony);
				GameText.Patch(harmony, textPath, chineseComponentCategoryShort);
				XmlText.Patch(harmony, textPath);
				GalactopediaText.Patch(harmony, textPath);
				HintText.Patch(harmony, textPath);
				SystemNameText.Patch(harmony, textPath);

				XenkoFix.Patch(harmony, fontTexSize);
				RacePatch.Patch(harmony, skipCheck);
				AmbiguousWordsFixPatch.Patch(harmony);
				SituationDescriptionPatch.Patch(harmony);
				GenerateRuinsPatch.Patch(harmony);
				if (enableTextBoxPaste)
					TextBoxPatch.Patch(harmony);
				if (chineseRandomName)
					RandomNamePatch.Patch(harmony);
				if (chineseOrdinalNumber)
					OrdinalNumberPatch.Patch(harmony);
				if (fixChineseTextWrap)
					TextWrapPatch.Patch(harmony);
				if (removeStarPostfix)
					RemoveStarPostfixPatch.Patch(harmony);
				if (chineseRandomShipName)
					RandomShipNamePatch.Patch(harmony, postfixForRandomShipName);
				CharacterNamePatch.Patch(harmony, characterNameSeparator);

				if (optimizeOrb)
					ReduceMeshPatch.Patch(harmony);
				if (optimizeShipTex || optimizeOtherTex)
					ReduceStreamingTexturePatch.Patch(harmony, optimizeShipTex, optimizeOtherTex);
			}
			catch (Exception e)
			{
				var sb = new StringBuilder("汉化补丁在注入文本时发生了一个错误，");
				sb.AppendLine($"错误信息为{e.Message}");
				sb.AppendLine("这可能是由于补丁版本与游戏不符导致（比如在游戏更新后）。");
				MessageBox.Show(sb.ToString(), "汉化补丁错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw;
			}
	    }

	    public static void PostLoadFix()
	    {
		    var getTextMethod = AccessTools.Method("DistantWorlds.Types.TextResolver:GetText");
		    if (getTextMethod != null)
		    {
			    try
			    {
				    var governmentType = AccessTools.TypeByName("DistantWorlds.Types.Government");
				    if (governmentType != null)
				    {
					    var governmentNameField = AccessTools.PropertySetter(governmentType, "Name");
					    if (governmentNameField != null)
					    {
						    var government = AccessTools.Field(governmentType, "PirateRaider")?.GetValue(null);

						    if (government != null)
							    governmentNameField.Invoke(government, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Pirate Raider"})
							    });

						    government = AccessTools.Field(governmentType, "ZombieHive")?.GetValue(null);

						    if (government != null)
							    governmentNameField.Invoke(government, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Zombie Hive"})
							    });

						    government = AccessTools.Field(governmentType, "BerserkAI")?.GetValue(null);

						    if (government != null)
							    governmentNameField.Invoke(government, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Berserk AI"})
							    });
					    }
				    }
				}
			    catch
			    {
				    // ignored
			    }

			    try
			    {
				    var troopType = AccessTools.TypeByName("DistantWorlds.Types.TroopDefinition");
				    if (troopType != null)
				    {
					    var troopNameField = AccessTools.PropertySetter(troopType, "Name");
					    if (troopNameField != null)
					    {
						    var troop = AccessTools.Field(troopType, "GenericInfantry")?.GetValue(null);

						    if (troop != null)
							    troopNameField.Invoke(troop, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Standard Infantry"})
							    });

						    troop = AccessTools.Field(troopType, "GenericMilitia")?.GetValue(null);

						    if (troop != null)
							    troopNameField.Invoke(troop, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Standard Militia"})
							    });

						    troop = AccessTools.Field(troopType, "GenericRaider")?.GetValue(null);

						    if (troop != null)
							    troopNameField.Invoke(troop, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Standard Raider"})
							    });

						    troop = AccessTools.Field(troopType, "HiveRaider")?.GetValue(null);

						    if (troop != null)
							    troopNameField.Invoke(troop, new object[]
							    {
								    (string) getTextMethod.Invoke(null, new object[] {"Hive Raider"})
							    });
					    }
				    }
				}
				catch
				{
					// ignored
				}
			}
	    }
    }
}
