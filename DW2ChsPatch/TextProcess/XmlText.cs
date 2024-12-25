using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using DW2ChsPatch.Feature;
using HarmonyLib;

namespace DW2ChsPatch.TextProcess
{
	public class XmlText
	{
		private static string _dir;

		public static void Patch(Harmony harmony, string textDir)
		{
			_dir = textDir;
			
			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:LoadStaticBaseData"), null, null,
				new HarmonyMethod(AccessTools.Method(typeof(XmlText), nameof(Transpiler))));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:ReloadComponentsAndShipHulls"), null, null,
				new HarmonyMethod(AccessTools.Method(typeof(XmlText), nameof(Transpiler))));

			harmony.Patch(AccessTools.Method("DistantWorlds.Types.Galaxy:ReloadResearch"), null, null,
				new HarmonyMethod(AccessTools.Method(typeof(XmlText), nameof(Transpiler))));
		}

		private static JsonText GetTextJson(string name)
		{
			var filepath = Path.Combine(_dir, name);
			return JsonText.CreateOrGetJsonText(name, filepath);
		}

		/*private static XmlDocument GetTextXml(string name)
		{
			var filepath = Path.Combine(_dir, name);
			if (File.Exists(filepath))
			{
				var doc = new XmlDocument();
				doc.Load(filepath);
				return doc;
			}

			return null;
		}*/

		private static Stream XmlToStream(XmlDocument xmlDoc)
		{
			var sb = new StringBuilder();
			using (var output = new StringWriter(sb))
			{
				xmlDoc.Save(output);
			}
			return new MemoryStream(Encoding.Unicode.GetBytes(sb.ToString()));
		}

		delegate void OnProcessNodeJson(XmlNode dataNode, JsonText json, string keyPrefix);

		//delegate void OnProcessNode(XmlNode dataNode, XmlNode textNode);

		//delegate void OnCreateNodeJson(JsonText json, XmlNode dataNode, XmlNode textNode, string keyPrefix);

		private static Stream ApplyJson(string fileName, Stream dataStream,
			string rootNodeName, string childNodeName, string idName,
			params string[] copiedKeys)
		{
			return ApplyJson(fileName, dataStream,
				rootNodeName, childNodeName, idName,
				null, copiedKeys);
		}

		private static Stream ApplyJson(string fileName, Stream dataStream,
			string rootNodeName, string childNodeName, string idName,
			OnProcessNodeJson onProcessNode,
			params string[] copiedKeys)
		{
			var filepath = Path.Combine(_dir, fileName);
			var json = JsonText.CreateOrGetJsonText(fileName, filepath);
			if (json == null)
				return dataStream;
			
			var dataDoc = new XmlDocument();
			dataDoc.Load(dataStream);

			var list = dataDoc[rootNodeName];
			var usedId = new HashSet<string>();
			foreach (XmlNode childNode in list.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Comment)
					continue;

				var id = childNode[idName].InnerText;
				var jsonId = id;
				int i = 1;
				while (!usedId.Add(jsonId))
				{
					jsonId = $"{id}__{i}";
					i++;
				}
				var basicKey = $"{childNodeName}_{jsonId}";

				foreach (var key in copiedKeys)
				{
					var jsonKey = $"{basicKey}_{key}";
					
					var copyTo = childNode[key];
					if (copyTo != null)
					{
						json.GetString(jsonKey, copyTo.InnerText, out var copyFrom);
						copyTo.InnerText = copyFrom;
					}
				}

				if (onProcessNode != null)
					onProcessNode(childNode, json, basicKey);
			}

			return XmlToStream(dataDoc);
		}

		/*private static Stream CopyText(string fileName, Stream dataStream,
			string rootNodeName, string childNodeName, string idName,
			params string[] copiedKeys)
		{
			return CopyText(fileName, dataStream,
				rootNodeName, childNodeName, idName,
				null, copiedKeys);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TryCopyInnerText(XmlNode copyTo, XmlNode copyFrom)
		{
			if (copyTo != null && copyFrom != null)
				copyTo.InnerText = copyFrom.InnerText;
		}

		private static Stream CopyText(string fileName, Stream dataStream,
			string rootNodeName, string childNodeName, string idName,
			OnProcessNode onProcessNode,
			params string[] copiedKeys)
		{
			var textDoc = GetTextXml(fileName);
			if (textDoc == null)
				return dataStream;

			var dataDoc = new XmlDocument();
			dataDoc.Load(dataStream);

			var list = dataDoc[rootNodeName];
			foreach (XmlNode childNode in list.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Comment)
					continue;

				var id = childNode[idName].InnerText;
				var textNode = textDoc.SelectSingleNode(
					$"/{rootNodeName}/{childNodeName}[{idName}='{id}']");
				if (textNode != null)
				{
					foreach (var key in copiedKeys)
					{
						var copyTo = childNode[key];
						var copyFrom = textNode[key];
						TryCopyInnerText(copyTo, copyFrom);
					}

					if (onProcessNode != null)
						onProcessNode(childNode, textNode);
				}
			}

			return XmlToStream(dataDoc);
		}*/

		/*private static void JsonSetStringWithoutUnmodifiedTranslation(
			JsonText json, string key, string origin, string translation)
		{
			if (origin == translation)
				translation = null;
			json.SetString(key, origin, translation);
		}*/

		/*private static JsonText CreateTranslationJsonForTour(
			XmlDocument originDoc, XmlDocument translateDoc)
		{
			var rootNodeName = "ArrayOfTourItem";
			var childNodeName = "TourItem";
			var json = new JsonText();

			var originList = originDoc.SelectNodes($"/{rootNodeName}/{childNodeName}");
			var translateList = translateDoc.SelectNodes($"/{rootNodeName}/{childNodeName}");
			int i = 0;
			foreach (XmlNode originNode in originList)
			{
				XmlNode translateNode = null;
				if (i < translateList.Count)
				{
					translateNode = translateList[i];
				}

				json.SetString($"TourItem_{i}_Title", 
					originNode["Title"]?.InnerText,
					translateNode?["Title"]?.InnerText);

				var originSteps = originNode["Steps"];

				if (originSteps != null)
				{
					int j = 0;
					foreach (XmlNode originStepNode in originSteps.ChildNodes)
					{
						XmlNode translateStepNode = null;
						if (translateNode != null && translateNode["Steps"]?.ChildNodes.Count > j)
						{
							translateStepNode = translateNode["Steps"]?.ChildNodes[j];
						}

						json.SetString($"TourItem_{i}_Step_{j}_StepTitle",
							originStepNode["StepTitle"]?.InnerText,
							translateStepNode?["StepTitle"]?.InnerText);

						json.SetString($"TourItem_{i}_Step_{j}_MarkupText",
							originStepNode["MarkupText"]?.InnerText,
							translateStepNode?["MarkupText"]?.InnerText);

						j++;
					}
				}
				
				i++;
			}

			for (; i < translateList.Count;)
			{
				var translateNode = translateList[i];

				json.SetString($"TourItem_{i}_Title",
					"",
					translateNode?["Title"]?.InnerText);

				if (translateNode["Steps"] != null)
				{
					int j = 0;
					foreach (XmlNode translateStepNode in translateNode["Steps"].ChildNodes)
					{
						json.SetString($"TourItem_{i}_Step_{j}_StepTitle",
							"",
							translateStepNode["StepTitle"]?.InnerText);

						json.SetString($"TourItem_{i}_Step_{j}_MarkupText",
							"",
							translateStepNode["MarkupText"]?.InnerText);

						j++;
					}
				}

				i++;
			}

			return json;
		}*/

		/*private static JsonText CreateTranslationJsonDo(
			XmlDocument originDoc, XmlDocument translateDoc,
			string rootNodeName, string childNodeName, string idName,
			params string[] copiedKeys)
		{
			return CreateTranslationJsonDo(originDoc, translateDoc,
				rootNodeName, childNodeName, idName,
				null, copiedKeys);
		}*/

		/*private static JsonText CreateTranslationJsonDo(
			XmlDocument originDoc, XmlDocument translateDoc,
			string rootNodeName, string childNodeName, string idName,
			OnCreateNodeJson onProcessNode,
			params string[] copiedKeys)
		{
			var json = new JsonText();
			var list = originDoc[rootNodeName];
			foreach (XmlNode childNode in list.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Comment)
					continue;

				var id = childNode[idName].InnerText;
				var textNode = translateDoc.SelectSingleNode(
					$"/{rootNodeName}/{childNodeName}[{idName}='{id}']");

				foreach (var key in copiedKeys)
				{
					var copyTo = childNode[key];
					var copyFrom = textNode?[key];
					var jsonKey = $"{childNodeName}_{id}_{key}";
					var origin = copyTo?.InnerText;
					var translation = copyFrom?.InnerText;
					if (!string.IsNullOrEmpty(origin) || !string.IsNullOrEmpty(translation))
						JsonSetStringWithoutUnmodifiedTranslation(json, jsonKey, origin, translation);
				}

				if (onProcessNode != null)
					onProcessNode(json, childNode, textNode, $"{childNodeName}_{id}");
			}

			return json;
		}*/

		/*private static void CreateArrayJson(JsonText json, XmlNode originNode, XmlNode translateNode, string keyPrefix)
		{
			if (originNode == null)
				return;

			var originStrs = originNode.SelectNodes("string");
			var translateStrs = translateNode?.SelectNodes("string");

			int i = 0;

			for (; i < originStrs.Count; i++)
			{
				string ori = originStrs[i].InnerText;
				string tran = translateStrs?.Count > i ? translateStrs[i].InnerText : null;
				JsonSetStringWithoutUnmodifiedTranslation(json, $"{keyPrefix}_{i}", ori, tran);
			}

			if (translateStrs != null && translateStrs.Count > i)
			{
				for (; i < translateStrs.Count; i++)
				{
					string ori = "";
					string tran = translateStrs[i].InnerText;
					json.SetString($"{keyPrefix}_{i}", ori, tran);
				}
			}
		}*/

		/*public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate, string type)
		{
			var originDoc = new XmlDocument();
			var translateDoc = new XmlDocument();

			originDoc.Load(pathOrigin);
			if (File.Exists(pathTranslate))
				translateDoc.Load(pathTranslate);

			JsonText json = null;

			switch (type)
			{
				case "TourItems":
					json = CreateTranslationJsonForTour(originDoc, translateDoc);
					break;
				case "ColonyEventDefinitions":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfColonyEventDefinition",
						"ColonyEventDefinition",
						"ColonyEventDefinitionId",
						"Name", "Description");
					break;
				case "ComponentDefinitions":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfComponentDefinition",
						"ComponentDefinition",
						"ComponentId",
						"Name");
					break;
				case "CreatureTypes":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfCreatureType",
						"CreatureType",
						"CreatureTypeId",
						"Name");
					break;
				case "Governments":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfGovernment",
						"Government",
						"GovernmentId",
						(nodeJson, originNode, transNode, prefix) =>
						{
							var node1 = originNode["EmpireNameAdjectives"];
							var node2 = transNode?["EmpireNameAdjectives"];
							CreateArrayJson(nodeJson, node1, node2, $"{prefix}_EmpireNameAdjectives");

							node1 = originNode["EmpireNameNouns"];
							node2 = transNode?["EmpireNameNouns"];
							CreateArrayJson(nodeJson, node1, node2, $"{prefix}_EmpireNameNouns");
						},
						"Name", "Description", "LeaderTitle", "CabinetTitle");
					break;
				case "OrbTypes":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfOrbType",
						"OrbType",
						"OrbTypeId",
						(nodeJson, originNode, transNode, prefix) =>
						{
							var node1 = originNode["RuinLocationDescriptions"];
							var node2 = transNode?["RuinLocationDescriptions"];
							CreateArrayJson(nodeJson, node1, node2, $"{prefix}_RuinLocationDescriptions");

							var dataBonusList = originNode.SelectNodes("CommonBonuses/BonusRange");
							var textBonusList = transNode?.SelectNodes("CommonBonuses/BonusRange");
							if (dataBonusList != null)
							{
								for (var i = 0; i < dataBonusList.Count; i++)
								{
									node1 = dataBonusList[i]["Descriptions"];
									node2 = null;
									if (textBonusList != null && textBonusList.Count > i)
									{
										node2 = textBonusList[i]["Descriptions"];
									}
									CreateArrayJson(nodeJson, node1, node2,
										$"{prefix}_CommonBonuses_{i}");
								}
							}
						},
						"Name");
					break;
				case "PlanetaryFacilityDefinitions":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfPlanetaryFacilityDefinition",
						"PlanetaryFacilityDefinition",
						"PlanetaryFacilityDefinitionId",
						"Name");
					break;
				case "Artifacts":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfArtifact",
						"Artifact",
						"ArtifactId",
						"Name", "Description");
					break;
				case "Races":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfRace",
						"Race",
						"RaceId",
						(nodeJson, originNode, transNode, prefix) =>
						{
							var node1 = originNode["CharacterFirstNames"];
							var node2 = transNode?["CharacterFirstNames"];
							CreateArrayJson(nodeJson, node1, node2, $"{prefix}_CharacterFirstNames");

							node1 = originNode["CharacterLastNames"];
							node2 = transNode?["CharacterLastNames"];
							CreateArrayJson(nodeJson, node1, node2, $"{prefix}_CharacterLastNames");
						},
						"Name", "Description");
					break;
				case "ResearchProjectDefinitions":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfResearchProjectDefinition",
						"ResearchProjectDefinition",
						"ResearchProjectId",
						(nodeJson, originNode, transNode, prefix) =>
						{
							var dataIncidentsList = originNode.SelectNodes("DiplomacyFactors/EmpireIncidentFactor");
							var textIncidentsList = transNode?.SelectNodes("DiplomacyFactors/EmpireIncidentFactor");
							if (dataIncidentsList != null)
							{
								for (var i = 0; i < dataIncidentsList.Count; i++)
								{
									var node1 = dataIncidentsList[i]["Descriptions"];
									XmlNode node2 = null;
									if (textIncidentsList != null && textIncidentsList.Count > i)
									{
										node2 = textIncidentsList[i]["Descriptions"];
									}
									CreateArrayJson(nodeJson, node1, node2, // TODO: FIXME: BUG
										$"{prefix}_DiplomacyFactors_{i}");
								}
							}
						},
						"Name");
					break;
				case "Resources":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfResource",
						"Resource",
						"ResourceId",
						"Name", "Description");
					break;
				case "ShipHulls":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfShipHull",
						"ShipHull",
						"ShipHullId",
						"Name");
					break;
				case "SpaceItemDefinitions":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfSpaceItemDefinition",
						"SpaceItemDefinition",
						"SpaceItemDefinitionId",
						"Name");
					break;
				case "TroopDefinitions":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfTroopDefinition",
						"TroopDefinition",
						"TroopDefinitionId",
						"Name");
					break;
				case "FleetTemplates":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfFleetTemplate",
						"FleetTemplate",
						"FleetTemplateId",
						"Name");
					break;
				case "ArmyTemplates":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfArmyTemplate",
						"ArmyTemplate",
						"ArmyTemplateId",
						"Name");
					break;
				case "GameEvents":
					json = CreateTranslationJsonDo(originDoc, translateDoc,
						"ArrayOfGameEvent",
						"GameEvent",
						"Name",
						(nodeJson, originNode, transNode, prefix) =>
						{
							var dataPlacementActions = originNode.SelectNodes("PlacementActions/GameEventAction");
							var textPlacementActions = transNode?.SelectNodes("PlacementActions/GameEventAction");
							OnCreateNodeJsonForEventDo(nodeJson, dataPlacementActions, textPlacementActions, 
								$"{prefix}_PlacementActions");

							var dataTriggerActions = originNode.SelectNodes("TriggerActions/GameEventAction");
							var textTriggerActions = transNode?.SelectNodes("TriggerActions/GameEventAction");
							OnCreateNodeJsonForEventDo(nodeJson, dataTriggerActions, textTriggerActions,
								$"{prefix}_TriggerActions");
						},
						"Title", "Description");
					break;
			}

			if (json != null)
			{
				json.ExportToFile(pathOutput);
			}
		}*/

		/*private static void OnCreateNodeJsonForEventDo(JsonText json, XmlNodeList dataEvents, XmlNodeList textEvents, string keyPrefix)
		{
			if (dataEvents == null)
				return;
			for (var i = 0; i < dataEvents.Count; i++)
			{
				var dataEvent = dataEvents[i];
				XmlNode textEvent = null;

				if (textEvents != null && i < textEvents.Count)
				{
					textEvent = textEvents[i];
				}

				var type = dataEvent["Type"]?.InnerText ?? "";

				var key = $"{keyPrefix}_{i}_MessageTitle";
				var ori = dataEvent["MessageTitle"]?.InnerText;
				var tran = textEvent?["MessageTitle"]?.InnerText;
				if (!string.IsNullOrEmpty(ori))
					json.SetString(key, ori, tran);

				key = $"{keyPrefix}_{i}_Description";
				ori = dataEvent["Description"]?.InnerText;
				tran = textEvent?["Description"]?.InnerText;
				if (!string.IsNullOrEmpty(ori))
					json.SetString(key, ori, tran);

				key = $"{keyPrefix}_{i}_ChoiceButtonText";
				ori = dataEvent["ChoiceButtonText"]?.InnerText;
				tran = textEvent?["ChoiceButtonText"]?.InnerText;
				if (!string.IsNullOrEmpty(ori))
					json.SetString(key, ori, tran);

				key = $"{keyPrefix}_{i}_GeneratedItemName";
				ori = dataEvent["GeneratedItemName"]?.InnerText;
				tran = textEvent?["GeneratedItemName"]?.InnerText;
				if (!string.IsNullOrEmpty(ori))
				{
					switch (type)
					{
						case "EnableEvent":
						case "DisableEvent":
						case "TriggerEvent":
						case "CustomCode":
							break;
						default:
							json.SetString(key, ori, tran);
							break;
					}
				}

				key = $"{keyPrefix}_{i}_ActionLocationItemName";
				ori = dataEvent["ActionLocationItemName"]?.InnerText;
				tran = textEvent?["ActionLocationItemName"]?.InnerText;
				if (!string.IsNullOrEmpty(ori))
					json.SetString(key, ori, tran);

				key = $"{keyPrefix}_{i}_GeneratedItemArtifactNames";
				var dataPlacementActions = dataEvent["GeneratedItemArtifactNames"];
				var textPlacementActions = textEvent?["GeneratedItemArtifactNames"];
				CreateArrayJson(json, dataPlacementActions, textPlacementActions, key);
			}
		}*/

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt && instruction.operand is MethodInfo method &&
				    method.Name == "LoadFromStream")
				{
					yield return new CodeInstruction(OpCodes.Ldstr, method.ReturnType.FullName);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(XmlText), nameof(LoadFromStreamPreprocess)));
				}
				yield return instruction;
			}
		}

		private static Stream LoadFromStreamPreprocess(Stream __0, string type)
		{
			if (type == "DistantWorlds.Types.TourItemList")
			{
				var json = GetTextJson("TourItems.json");
				if (json == null)
					return __0;

				var dataDoc = new XmlDocument();
				dataDoc.Load(__0);
				var items = dataDoc.SelectNodes("//TourItem");

				var generating = TranslationTextGenerator.Enable;
				var translationTable = generating ? null : json.CreateOriginalTranslationMappingMap();
				int indexOfTour = 0;
				foreach (XmlNode item in items)
				{
					var titleNode = item["Title"];
					if (titleNode != null)
					{
						if (generating)
						{
							json.GetString($"TourItem_{indexOfTour}_Title", titleNode.InnerText, out var result);
							titleNode.InnerText = result;
						}
						else if (translationTable.TryGetValue(titleNode.InnerText.UniteNewline(), out var newStr1))
							titleNode.InnerText = newStr1;
					}

					var steps = item.SelectNodes("Steps/TourStep");
					int indexOfStep = 0;
					foreach (XmlNode step in steps)
					{
						var stepTitleNode = step["StepTitle"];
						if (stepTitleNode != null)
						{
							if (generating)
							{
								json.GetString($"TourItem_{indexOfTour}_Step_{indexOfStep}_StepTitle", 
									stepTitleNode.InnerText, out var result);
								stepTitleNode.InnerText = result;
							}
							else if (translationTable.TryGetValue(stepTitleNode.InnerText.UniteNewline(), out var newStr1))
								stepTitleNode.InnerText = newStr1;
						}

						var markupTextNode = step["MarkupText"];
						if (markupTextNode != null)
						{
							var text = markupTextNode.InnerText;
							if (generating)
							{
								json.GetString($"TourItem_{indexOfTour}_Step_{indexOfStep}_MarkupText",
									text, out var result);
								markupTextNode.InnerText = result;
							}
							else if (translationTable.TryGetValue(text.UniteNewline(), out var newStr1))
								markupTextNode.InnerText = newStr1;
						}

						indexOfStep++;
					}

					indexOfTour++;
				}
				
				return XmlToStream(dataDoc);
			}
			else if (type == "DistantWorlds.Types.ColonyEventDefinitionList")
			{
				return ApplyJson("ColonyEventDefinitions.json", __0,
					"ArrayOfColonyEventDefinition",
					"ColonyEventDefinition",
					"ColonyEventDefinitionId",
					"Name", "Description");
			}
			else if (type == "DistantWorlds.Types.ComponentDefinitionList")
			{
				return ApplyJson("ComponentDefinitions.json", __0,
					"ArrayOfComponentDefinition",
					"ComponentDefinition",
					"ComponentId",
					"Name", "Description");
			}
			else if (type == "DistantWorlds.Types.CreatureTypeList")
			{
				return ApplyJson("CreatureTypes.json", __0,
					"ArrayOfCreatureType",
					"CreatureType",
					"CreatureTypeId",
					"Name", "Description");
			}
			else if (type == "DistantWorlds.Types.GovernmentList")
			{
				return ApplyJson("Governments.json", __0,
					"ArrayOfGovernment",
					"Government",
					"GovernmentId",
					(node, json, key) =>
					{
						ReplaceStringList(node["EmpireNameAdjectives"], json, $"{key}_EmpireNameAdjectives");
						ReplaceStringList(node["EmpireNameNouns"], json, $"{key}_EmpireNameNouns");
					},
					"Name", "Description", "LeaderTitle", "CabinetTitle");
			}
			else if (type == "DistantWorlds.Types.OrbTypeList")
			{
				return ApplyJson("OrbTypes.json", __0,
					"ArrayOfOrbType",
					"OrbType",
					"OrbTypeId",
					(node, json, key) =>
					{
						ReplaceStringList(node["RuinLocationDescriptions"], json, $"{key}_RuinLocationDescriptions");

						var dataBonusList = node.SelectNodes("CommonBonuses/BonusRange");
						if (dataBonusList != null && dataBonusList.Count > 0)
						{
							if (TranslationTextGenerator.Enable || JsonText.StrictMode)
							{
								for (var i = 0; i < dataBonusList.Count; i++)
								{
									var bonusNode = dataBonusList[i];
									ReplaceStringList(bonusNode["Descriptions"], json, $"{key}_CommonBonuses_{i}");
								}
							}
							else
							{
								var map = new Dictionary<string, string>();
								for (int i = 0; i < 10000; i++)
								{
									for (int j = 0; j < 10000; j++)
									{
										var basicKey = $"{key}_CommonBonuses_{i}_{j}";
										if (json.GetOriginalAndTranslatedString(basicKey, out var ori, out var tran))
											map[ori] = tran;
										else
										{
											if (j > 0)
												break;
											else
												goto BREAK2;
										}
									}
								}
								BREAK2:
								for (var i = 0; i < dataBonusList.Count; i++)
								{
									ReplaceStringList(dataBonusList[i]["Descriptions"], map);
								}
							}
						}
					},
					"Name", "Description");
			}
			else if (type == "DistantWorlds.Types.PlanetaryFacilityDefinitionList")
			{
				return ApplyJson("PlanetaryFacilityDefinitions.json", __0,
					"ArrayOfPlanetaryFacilityDefinition",
					"PlanetaryFacilityDefinition",
					"PlanetaryFacilityDefinitionId",
					"Name");
			}
			else if (type == "DistantWorlds.Types.ArtifactList")
			{
				return ApplyJson("Artifacts.json", __0,
					"ArrayOfArtifact",
					"Artifact",
					"ArtifactId",
					"Name", "Description");
			}
			else if (type == "DistantWorlds.Types.RaceList")
			{
				return ApplyJson("Races.json", __0,
					"ArrayOfRace",
					"Race",
					"RaceId",
					(node, json, key) =>
					{
						var dataName = node["Name"];
						if (dataName != null)
						{
							var oldName = dataName.InnerText;
							if (json.CheckOriginal($"{key}_Name", oldName))
							{
								json.GetString($"{key}_Name", oldName, out var newName);
								RacePatch.SetRaceOriginalName(oldName, newName);
								dataName.InnerText = newName;

								var descName = node["Description"];
								if (descName != null)
								{
									json.GetString($"{key}_Description", descName.InnerText, out var newDesc);
									descName.InnerText = newDesc;
								}

								ReplaceStringList(node["CharacterFirstNames"], json, $"{key}_CharacterFirstNames");
								ReplaceStringList(node["CharacterLastNames"], json, $"{key}_CharacterLastNames");
							}
						}
					});
			}
			else if (type == "DistantWorlds.Types.ResearchProjectDefinitionList")
			{
				return ApplyJson("ResearchProjectDefinitions.json", __0,
					"ArrayOfResearchProjectDefinition",
					"ResearchProjectDefinition",
					"ResearchProjectId",
					(node, json, key) =>
					{
						var dataIncidentsList = node.SelectNodes("DiplomacyFactors/EmpireIncidentFactor");
						if (dataIncidentsList != null)
						{
							if (TranslationTextGenerator.Enable || JsonText.StrictMode)
							{
								for (var i = 0; i < dataIncidentsList.Count; i++)
								{
									var node1 = dataIncidentsList[i]["Description"];
									if (node1 != null)
									{
										json.GetString($"{key}_DiplomacyFactors_{i}", node1.InnerText, out var newStr);
										node1.InnerText = newStr;
									}
								}
							}
							else
							{
								var map = new Dictionary<string, string>();
								for (int i = 0; i < 10000; i++)
								{
									var basicKey = $"{key}_DiplomacyFactors_{i}";
									if (json.GetOriginalAndTranslatedString(basicKey, out var ori, out var tran))
										map[ori] = tran;
									else
										break;
								}

								for (var i = 0; i < dataIncidentsList.Count; i++)
								{
									var node1 = dataIncidentsList[i]["Description"];
									if (node1 != null && map.TryGetValue(node1.InnerText.UniteNewline(), out var newStr))
									{
										node1.InnerText = newStr;
									}
								}
							}
						}
					},
					"Name", "Description"); // the Description is empty now
			}
			else if (type == "DistantWorlds.Types.ResourceList")
			{
				return ApplyJson("Resources.json", __0,
					"ArrayOfResource",
					"Resource",
					"ResourceId",
					"Name", "Description");
			}
			else if (type == "DistantWorlds.Types.ShipHullList")
			{
				return ApplyJson("ShipHulls.json", __0,
					"ArrayOfShipHull",
					"ShipHull",
					"ShipHullId",
					"Name");
			}
			else if (type == "DistantWorlds.Types.SpaceItemDefinitionList")
			{
				return ApplyJson("SpaceItemDefinitions.json", __0,
					"ArrayOfSpaceItemDefinition",
					"SpaceItemDefinition",
					"SpaceItemDefinitionId",
					"Name", "Description"); // the Description is empty now
			}
			else if (type == "DistantWorlds.Types.TroopDefinitionList")
			{
				return ApplyJson("TroopDefinitions.json", __0,
					"ArrayOfTroopDefinition",
					"TroopDefinition",
					"TroopDefinitionId",
					"Name");
			}
			else if (type == "DistantWorlds.Types.FleetTemplateList")
			{
				return ApplyJson("FleetTemplates.json", __0,
					"ArrayOfFleetTemplate",
					"FleetTemplate",
					"FleetTemplateId",
					"Name");
			}
			else if (type == "DistantWorlds.Types.ArmyTemplateList")
			{
				return ApplyJson("ArmyTemplates.json", __0,
					"ArrayOfArmyTemplate",
					"ArmyTemplate",
					"ArmyTemplateId",
					"Name");
			}
			else if (type == "DistantWorlds.Types.GameEventList")
			{
				return ApplyJson("GameEvents.json", __0,
					"ArrayOfGameEvent",
					"GameEvent",
					"Name",
					(node, json, key) =>
					{
						for (int pass = 0; pass < 2; pass++)
						{
							var nodeKey = pass == 0 
								? "PlacementActions/GameEventAction" 
								: "TriggerActions/GameEventAction";
							var jsonKey = pass == 0
								? "PlacementActions"
								: "TriggerActions";
							var eventNodes = node.SelectNodes(nodeKey);

							if (eventNodes != null && eventNodes.Count > 0)
							{
								if (TranslationTextGenerator.Enable || JsonText.StrictMode)
								{
									for (int i = 0; i < eventNodes.Count; i++)
									{
										var dataEvent = eventNodes[i];
										if (dataEvent != null)
										{
											XmlNode childNode = null;
											var basicKey = $"{key}_{jsonKey}_{i}_";

											if ((childNode = dataEvent["MessageTitle"]) != null)
											{
												json.GetString(basicKey + "MessageTitle", childNode.InnerText, out var result);
												childNode.InnerText = result;
											}

											if ((childNode = dataEvent["Description"]) != null)
											{
												json.GetString(basicKey + "Description", childNode.InnerText, out var result);
												childNode.InnerText = result;
											}

											if ((childNode = dataEvent["ChoiceButtonText"]) != null)
											{
												json.GetString(basicKey + "ChoiceButtonText", childNode.InnerText, out var result);
												childNode.InnerText = result;
											}

											if ((childNode = dataEvent["GeneratedItemName"]) != null)
											{
												var type = dataEvent["Type"]?.InnerText;
												switch (type)
												{
													case "EnableEvent":
													case "DisableEvent":
													case "TriggerEvent":
													case "CustomCode":
														break;
													default:
														json.GetString(basicKey + "GeneratedItemName", childNode.InnerText, out var result);
														childNode.InnerText = result;
														break;
												}
											}

											if ((childNode = dataEvent["ActionLocationItemName"]) != null)
											{
												json.GetString(basicKey + "ActionLocationItemName", childNode.InnerText, out var result);
												childNode.InnerText = result;
											}

											ReplaceStringList(dataEvent["GeneratedItemArtifactNames"], json, basicKey + "GeneratedItemArtifactNames");
										}
									}
								}
								else
								{
									var titleMap = new Dictionary<string, string>();
									var descMap = new Dictionary<string, string>();
									var choiceMap = new Dictionary<string, string>();
									var generatedMap = new Dictionary<string, string>();
									var locationMap = new Dictionary<string, string>();
									var generatedItemMap = new Dictionary<string, string>();

									for (int i = 0; i < eventNodes.Count; i++)
									{
										var basicKey = $"{key}_{jsonKey}_{i}_";
										if (json.GetOriginalAndTranslatedString(basicKey + "MessageTitle",
											out var oriTitle, out var tranTitle))
											titleMap[oriTitle] = tranTitle;
										if (json.GetOriginalAndTranslatedString(basicKey + "Description",
											out var oriDesc, out var tranDesc))
											descMap[oriDesc] = tranDesc;
										if (json.GetOriginalAndTranslatedString(basicKey + "ChoiceButtonText",
											out var oriChoice, out var tranChoice))
											choiceMap[oriChoice] = tranChoice;
										if (json.GetOriginalAndTranslatedString(basicKey + "GeneratedItemName",
											out var oriGenerated, out var tranGenerated))
											generatedMap[oriGenerated] = tranGenerated;
										if (json.GetOriginalAndTranslatedString(basicKey + "ActionLocationItemName",
											out var oriLocation, out var tranLocation))
											locationMap[oriLocation] = tranLocation;
										for (int j = 0; j < 10000; j++)
										{
											if (json.GetOriginalAndTranslatedString(
												basicKey + "GeneratedItemArtifactNames_" + j,
												out var oriItem, out var tranItem))
												generatedItemMap[oriItem] = tranItem;
											else
												break;
										}
									}

									for (var i = 0; i < eventNodes.Count; i++)
									{
										var dataEvent = eventNodes[i];
										if (dataEvent != null)
										{
											XmlNode childNode = null;

											if ((childNode = dataEvent["MessageTitle"]) != null
												&& titleMap.TryGetValue(childNode.InnerText.UniteNewline(), out var newTitle))
												childNode.InnerText = newTitle;

											if ((childNode = dataEvent["Description"]) != null
												&& descMap.TryGetValue(childNode.InnerText.UniteNewline(), out var newDesc))
												childNode.InnerText = newDesc;

											if ((childNode = dataEvent["ChoiceButtonText"]) != null
												&& choiceMap.TryGetValue(childNode.InnerText.UniteNewline(), out var newChoice))
												childNode.InnerText = newChoice;

											if ((childNode = dataEvent["GeneratedItemName"]) != null
												&& generatedMap.TryGetValue(childNode.InnerText.UniteNewline(), out var newItem))
												childNode.InnerText = newItem;

											if ((childNode = dataEvent["ActionLocationItemName"]) != null
												&& locationMap.TryGetValue(childNode.InnerText.UniteNewline(), out var newLocation))
												childNode.InnerText = newLocation;

											ReplaceStringList(dataEvent["GeneratedItemArtifactNames"], generatedItemMap);
										}
									}
								}
							}
						}

						// Fix Shakturi DLC
						var conditionNodes = node.SelectNodes("Conditions/GameEventCondition");
						if (conditionNodes != null)
						{
							foreach (XmlNode conditionNode in conditionNodes)
							{
								var n = conditionNode.SelectSingleNode("VariableName");
								if (n?.InnerText == "Shaktur Axis")
								{
									n.InnerText = ShakturiPatch.AllianceName_Axis;
								}
							}
						}
					},
					"Title", "Description");
			}
			else if (type == "DistantWorlds.Types.LocationEffectGroupDefinitionList")
			{
				return __0;
			}

			return __0;
		}

		private static void ReplaceStringList(XmlNode dataStringList, JsonText json, string jsonKeyPrefix)
		{
			if (dataStringList != null)
			{
				if (TranslationTextGenerator.Enable || JsonText.StrictMode)
				{
					var list = new List<string>();
					foreach (XmlNode n in dataStringList.ChildNodes)
					{
						list.Add(n.InnerText);
					}

					json.GetStringArray(jsonKeyPrefix, list.ToArray(), out var results);

					int i = 0;
					foreach (XmlNode n in dataStringList.ChildNodes)
					{
						n.InnerText = results[i];
						i++;
					}
				}
				else
				{
					var map = new Dictionary<string, string>();
					for (int i = 0; i < 10000; i++)
					{
						var key = $"{jsonKeyPrefix}_{i}";
						if (json.GetOriginalAndTranslatedString(key, out var ori, out var tran))
							map[ori] = tran;
						else
							break;
					}

					ReplaceStringList(dataStringList, map);
				}
			}
		}

		private static void ReplaceStringList(XmlNode dataStringList, Dictionary<string, string> translation)
		{
			if (dataStringList != null)
			{
				foreach (XmlNode n in dataStringList.ChildNodes)
				{
					var text = n.InnerText.UniteNewline();
					if (translation.TryGetValue(text, out var newStr))
						n.InnerText = newStr;
				}
			}
		}

		/*private static void ReplaceStringList(XmlNode dataStringList, XmlNode textStringList, string strNodeName = "string")
		{
			if (dataStringList != null && textStringList != null)
			{
				dataStringList.RemoveAll();
				foreach (XmlNode n in textStringList.ChildNodes)
				{
					var newNode = dataStringList.OwnerDocument.CreateElement(strNodeName);
					newNode.InnerText = n.InnerText;
					dataStringList.AppendChild(newNode);
				}
			}
		}
		 
		private static void OnProcessOrbNode(XmlNode dataNode, XmlNode textNode)
		{
			var node1 = dataNode["RuinLocationDescriptions"];
			var node2 = textNode["RuinLocationDescriptions"];
			ReplaceStringList(node1, node2);

			var dataBonusList = dataNode.SelectNodes("CommonBonuses/BonusRange");
			var textBonusList = textNode.SelectNodes("CommonBonuses/BonusRange");
			if (dataBonusList != null && textBonusList != null)
			{
				for (var i = 0; i < dataBonusList.Count; i++)
				{
					if (i < textBonusList.Count)
					{
						node1 = dataBonusList[i]["Type"];
						node2 = textBonusList[i]["Type"];
						if (node1?.InnerText == node2?.InnerText)
						{
							node1 = dataBonusList[i]["Descriptions"];
							node2 = textBonusList[i]["Descriptions"];
							ReplaceStringList(node1, node2);
						}
					}
				}
			}
		}

		private static void OnProcessRaceNode(XmlNode dataNode, XmlNode textNode)
		{
			var dataName = dataNode["Name"];
			var textName = textNode["Name"];
			if (dataName != null && textName != null)
			{
				var oldName = dataName.InnerText;
				var newName = textName.InnerText;
				RacePatch.SetRaceOriginalName(oldName, newName);
				dataName.InnerText = newName;
			}

			var node1 = dataNode["CharacterFirstNames"];
			var node2 = textNode["CharacterFirstNames"];
			ReplaceStringList(node1, node2);

			node1 = dataNode["CharacterLastNames"];
			node2 = textNode["CharacterLastNames"];
			ReplaceStringList(node1, node2);
		}

		private static void OnProcessGovernmentNode(XmlNode dataNode, XmlNode textNode)
		{
			var node1 = dataNode["EmpireNameAdjectives"];
			var node2 = textNode["EmpireNameAdjectives"];
			ReplaceStringList(node1, node2);

			node1 = dataNode["EmpireNameNouns"];
			node2 = textNode["EmpireNameNouns"];
			ReplaceStringList(node1, node2);
		}

		private static void OnProcessResearchNode(XmlNode dataNode, XmlNode textNode)
		{
			var dataIncidentsList = dataNode.SelectNodes("DiplomacyFactors/EmpireIncidentFactor");
			var textIncidentsList = textNode.SelectNodes("DiplomacyFactors/EmpireIncidentFactor");
			if (dataIncidentsList != null && textIncidentsList != null)
			{
				for (var i = 0; i < dataIncidentsList.Count; i++)
				{
					if (i < textIncidentsList.Count)
					{
						var node1 = dataIncidentsList[i]["Type"];
						var node2 = textIncidentsList[i]["Type"];
						if (node1?.InnerText == node2?.InnerText)
						{
							node1 = dataIncidentsList[i]["Descriptions"];
							node2 = textIncidentsList[i]["Descriptions"];
							TryCopyInnerText(node1, node2);
						}
					}
				}
			}
		}

		private static void OnProcessGameEventNodeDo(XmlNodeList dataEvents, XmlNodeList textEvents)
		{
			for (var i = 0; i < dataEvents.Count; i++)
			{
				if (i < textEvents.Count)
				{
					var dataEvent = dataEvents[i];
					var textEvent = textEvents[i];
					if (dataEvent["Type"].InnerText == textEvent["Type"].InnerText)
					{
						TryCopyInnerText(dataEvent["MessageTitle"], textEvent["MessageTitle"]);
						TryCopyInnerText(dataEvent["Description"], textEvent["Description"]);
						TryCopyInnerText(dataEvent["ChoiceButtonText"], textEvent["ChoiceButtonText"]);
						TryCopyInnerText(dataEvent["GeneratedItemName"], textEvent["GeneratedItemName"]);
						TryCopyInnerText(dataEvent["ActionLocationItemName"], textEvent["ActionLocationItemName"]);
						ReplaceStringList(
							dataEvent["GeneratedItemArtifactNames"],
							textEvent["GeneratedItemArtifactNames"]);
					}
				}
			}
		}

		private static void OnProcessGameEventNode(XmlNode dataNode, XmlNode textNode)
		{
			var dataPlacementActions = dataNode.SelectNodes("PlacementActions/GameEventAction");
			var textPlacementActions = textNode.SelectNodes("PlacementActions/GameEventAction");
			OnProcessGameEventNodeDo(dataPlacementActions, textPlacementActions);

			var dataTriggerActions = dataNode.SelectNodes("TriggerActions/GameEventAction");
			var textTriggerActions = textNode.SelectNodes("TriggerActions/GameEventAction");
			OnProcessGameEventNodeDo(dataTriggerActions, textTriggerActions);
		}*/
	}
}