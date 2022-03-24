﻿using System;
using System.IO;
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

		private static Type _tourItemListType;

		private static Type _colonyEventDefinitionListType;

		private static Type _componentDefinitionListType;

		private static Type _creatureTypeListType;

		private static Type _governmentListType;

		private static Type _orbTypeListType;

		private static Type _planetaryFacilityDefinitionListType;

		private static Type _artifactListType;

		private static Type _raceListType;

		private static Type _researchProjectDefinitionListType;

		private static Type _resourceListType;

		private static Type _shipHullListType;

		private static Type _spaceItemDefinitionListType;

		private static Type _troopDefinitionListType;

		private static Type _fleetTemplateListType;

		private static Type _armyTemplateListType;

		private static Type _gameEventListType;

		private static Type _locationEffectGroupDefinitionListType;

		public static void Patch(Harmony harmony, string textDir)
		{
			_dir = textDir;
			var loaderType = AccessTools.TypeByName("DistantWorlds.Types.XmlSerializationHelper`1");
			_tourItemListType = AccessTools.TypeByName("DistantWorlds.Types.TourItemList");
			_colonyEventDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.ColonyEventDefinitionList");
			_componentDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.ComponentDefinitionList");
			_creatureTypeListType = AccessTools.TypeByName("DistantWorlds.Types.CreatureTypeList");
			_governmentListType = AccessTools.TypeByName("DistantWorlds.Types.GovernmentList");
			_orbTypeListType = AccessTools.TypeByName("DistantWorlds.Types.OrbTypeList");
			_planetaryFacilityDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.PlanetaryFacilityDefinitionList");
			_artifactListType = AccessTools.TypeByName("DistantWorlds.Types.ArtifactList");
			_raceListType = AccessTools.TypeByName("DistantWorlds.Types.RaceList");
			_researchProjectDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.ResearchProjectDefinitionList");
			_resourceListType = AccessTools.TypeByName("DistantWorlds.Types.ResourceList");
			_shipHullListType = AccessTools.TypeByName("DistantWorlds.Types.ShipHullList");
			_spaceItemDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.SpaceItemDefinitionList");
			_troopDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.TroopDefinitionList");
			_fleetTemplateListType = AccessTools.TypeByName("DistantWorlds.Types.FleetTemplateList");
			_armyTemplateListType = AccessTools.TypeByName("DistantWorlds.Types.ArmyTemplateList");
			_gameEventListType = AccessTools.TypeByName("DistantWorlds.Types.GameEventList");
			_locationEffectGroupDefinitionListType = AccessTools.TypeByName("DistantWorlds.Types.LocationEffectGroupDefinitionList");
			harmony.Patch(AccessTools.Method(loaderType.MakeGenericType(typeof(object)), "LoadFromStream", new []{typeof(Stream)}),
				new HarmonyMethod(typeof(XmlText), nameof(LoadFromStreamPrefix)));
		}

		private static XmlDocument GetTextXml(string name)
		{
			var filepath = Path.Combine(_dir, name);
			if (File.Exists(filepath))
			{
				var doc = new XmlDocument();
				doc.Load(filepath);
				return doc;
			}

			return null;
		}

		private static Stream XmlToStream(XmlDocument xmlDoc)
		{
			var sb = new StringBuilder();
			using (var output = new StringWriter(sb))
			{
				xmlDoc.Save(output);
			}
			return new MemoryStream(Encoding.Unicode.GetBytes(sb.ToString()));
		}

		delegate void OnProcessNode(XmlNode dataNode, XmlNode textNode);

		delegate void OnCreateNodeJson(JsonText json, XmlNode dataNode, XmlNode textNode, string keyPrefix);

		private static Stream CopyText(string fileName, Stream dataStream,
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
		}

		private static void JsonSetStringWithoutUnmodifiedTranslation(
			JsonText json, string key, string origin, string translation)
		{
			if (origin == translation)
				translation = null;
			json.SetString(key, origin, translation);
		}

		private static JsonText CreateTranslationJsonForTour(
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
		}

		private static JsonText CreateTranslationJsonDo(
			XmlDocument originDoc, XmlDocument translateDoc,
			string rootNodeName, string childNodeName, string idName,
			params string[] copiedKeys)
		{
			return CreateTranslationJsonDo(originDoc, translateDoc,
				rootNodeName, childNodeName, idName,
				null, copiedKeys);
		}

		private static JsonText CreateTranslationJsonDo(
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
		}

		private static void CreateArrayJson(JsonText json, XmlNode originNode, XmlNode translateNode, string keyPrefix)
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
		}

		public static void CreateTranslationJson(string pathOutput, string pathOrigin, string pathTranslate, string type)
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
									CreateArrayJson(nodeJson, node1, node2,
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
		}

		private static void OnCreateNodeJsonForEventDo(JsonText json, XmlNodeList dataEvents, XmlNodeList textEvents, string keyPrefix)
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
		}

		private static void LoadFromStreamPrefix(object __instance, ref Stream __0)
		{
			var genericType = __instance.GetType().GenericTypeArguments[0];

			if (genericType == _tourItemListType)
			{
				var textDoc = GetTextXml("TourItems.xml");
				if (textDoc == null)
					return;

				__0 = XmlToStream(textDoc);
			}
			else if (genericType == _colonyEventDefinitionListType)
			{
				__0 = CopyText("ColonyEventDefinitions.xml", __0,
					"ArrayOfColonyEventDefinition",
					"ColonyEventDefinition",
					"ColonyEventDefinitionId",
					"Name", "Description");
			}
			else if (genericType == _componentDefinitionListType)
			{
				__0 = CopyText("ComponentDefinitions.xml", __0,
					"ArrayOfComponentDefinition",
					"ComponentDefinition",
					"ComponentId",
					"Name", "Description");
			}
			else if (genericType == _creatureTypeListType)
			{
				__0 = CopyText("CreatureTypes.xml", __0,
					"ArrayOfCreatureType",
					"CreatureType",
					"CreatureTypeId",
					"Name", "Description");
			}
			else if (genericType == _governmentListType)
			{
				__0 = CopyText("Governments.xml", __0,
					"ArrayOfGovernment",
					"Government",
					"GovernmentId",
					OnProcessGovernmentNode,
					"Name", "Description", "LeaderTitle", "CabinetTitle");
			}
			else if (genericType == _orbTypeListType)
			{
				__0 = CopyText("OrbTypes.xml", __0,
					"ArrayOfOrbType",
					"OrbType",
					"OrbTypeId",
					OnProcessOrbNode,
					"Name", "Description");
			}
			else if (genericType == _planetaryFacilityDefinitionListType)
			{
				__0 = CopyText("PlanetaryFacilityDefinitions.xml", __0,
					"ArrayOfPlanetaryFacilityDefinition",
					"PlanetaryFacilityDefinition",
					"PlanetaryFacilityDefinitionId",
					"Name");
			}
			else if (genericType == _artifactListType)
			{
				__0 = CopyText("Artifacts.xml", __0,
					"ArrayOfArtifact",
					"Artifact",
					"ArtifactId",
					"Name", "Description");
			}
			else if (genericType == _raceListType)
			{
				__0 = CopyText("Races.xml", __0,
					"ArrayOfRace",
					"Race",
					"RaceId",
					OnProcessRaceNode,
					"Description");
			}
			else if (genericType == _researchProjectDefinitionListType)
			{
				__0 = CopyText("ResearchProjectDefinitions.xml", __0,
					"ArrayOfResearchProjectDefinition",
					"ResearchProjectDefinition",
					"ResearchProjectId",
					OnProcessResearchNode,
					"Name", "Description"); // the Description is empty now
			}
			else if (genericType == _resourceListType)
			{
				__0 = CopyText("Resources.xml", __0,
					"ArrayOfResource",
					"Resource",
					"ResourceId",
					"Name", "Description");
			}
			else if (genericType == _shipHullListType)
			{
				__0 = CopyText("ShipHulls.xml", __0,
					"ArrayOfShipHull",
					"ShipHull",
					"ShipHullId",
					"Name");
			}
			else if (genericType == _spaceItemDefinitionListType)
			{
				__0 = CopyText("SpaceItemDefinitions.xml", __0,
					"ArrayOfSpaceItemDefinition",
					"SpaceItemDefinition",
					"SpaceItemDefinitionId",
					"Name", "Description"); // the Description is empty now
			}
			else if (genericType == _troopDefinitionListType)
			{
				__0 = CopyText("TroopDefinitions.xml", __0,
					"ArrayOfTroopDefinition",
					"TroopDefinition",
					"TroopDefinitionId",
					"Name");
			}
			else if (genericType == _fleetTemplateListType)
			{
				__0 = CopyText("FleetTemplates.xml", __0,
					"ArrayOfFleetTemplate",
					"FleetTemplate",
					"FleetTemplateId",
					"Name");
			}
			else if (genericType == _armyTemplateListType)
			{
				__0 = CopyText("ArmyTemplates.xml", __0,
					"ArrayOfArmyTemplate",
					"ArmyTemplate",
					"ArmyTemplateId",
					"Name");
			}
			else if (genericType == _gameEventListType)
			{
				__0 = CopyText("GameEvents.xml", __0,
					"ArrayOfGameEvent",
					"GameEvent",
					"Name",
					OnProcessGameEventNode,
					"Title", "Description");
			}
			else if (genericType == _locationEffectGroupDefinitionListType)
			{ // no present in game for now
			}
		}

		private static void ReplaceStringList(XmlNode dataStringList, XmlNode textStringList, string strNodeName = "string")
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
		}
	}
}