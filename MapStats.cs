﻿using UnityEngine;
using HarmonyLib;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using TMPro;

namespace AlweStats {
    [HarmonyPatch]
    public static class MapStats {
        private static Block mapBlock = null;
        private static readonly Dictionary<CustomPinData, Minimap.SpriteData> pinsDict = new() {
            {   
                new CustomPinData { 
                    name = "SunkenCrypt",
                    title = "$location_sunkencrypt",
                    hash = 0,
                    type = CustomPinType.Crypt 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length, 
                    m_icon = Utilities.GetSprite("TrophyDraugr".GetStableHashCode(), false) 
                } 
            },
            {   
                new CustomPinData { 
                    name = "TrollCave", 
                    title = "$location_forestcave",
                    hash = 0,
                    type = CustomPinType.TrollCave 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 1, 
                    m_icon = Utilities.GetSprite("TrophyFrostTroll".GetStableHashCode(), false) 
                } 
            },
            {   
                new CustomPinData { 
                    name = "FireHole", 
                    title = "$enemy_surtling",
                    hash = 0,
                    type = CustomPinType.FireHole 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 2, 
                    m_icon = Utilities.GetSprite("TrophySurtling".GetStableHashCode(), false) 
                } 
            },
            {   
                new CustomPinData { 
                    name = "Crypt",
                    title = "$location_forestcrypt",
                    hash = 0, 
                    type = CustomPinType.Crypt 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 3, 
                    m_icon = Utilities.GetSprite("TrophySkeleton".GetStableHashCode(), false) 
                } 
            },
            {   
                new CustomPinData { 
                    name = "MountainCave", 
                    title = "$location_mountaincave",
                    hash = 0,
                    type = CustomPinType.MountainCave 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 4, 
                    m_icon = Utilities.GetSprite("TrophyCultist".GetStableHashCode(), false) 
                } 
            },
            {   
                new CustomPinData { 
                    name = "Mistlands_DvergrTownEntrance", 
                    title = "$location_dvergrtown",
                    hash = 0,
                    type = CustomPinType.InfestedMine 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 5, 
                    m_icon = Utilities.GetSprite("TrophySeekerBrute".GetStableHashCode(), false) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Cart",
                    title = "$tool_cart",
                    hash = "Cart".GetStableHashCode(),
                    type = CustomPinType.Cart 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 6, 
                    m_icon = Utilities.GetSprite("Cart".GetStableHashCode(), true) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Raft", 
                    title = "$ship_raft",
                    hash = "Raft".GetStableHashCode(),
                    type = CustomPinType.Ship 
                },  
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 7, 
                    m_icon = Utilities.GetSprite("Raft".GetStableHashCode(), true) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Karve", 
                    title = "$ship_karve",
                    hash = "Karve".GetStableHashCode(),
                    type = CustomPinType.Ship 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 8, 
                    m_icon = Utilities.GetSprite("Karve".GetStableHashCode(), true) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Viking Ship",
                    title = "$ship_longship",
                    hash = "VikingShip".GetStableHashCode(),
                    type = CustomPinType.Ship 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 9, 
                    m_icon = Utilities.GetSprite("VikingShip".GetStableHashCode(), true) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Portal", 
                    title = "$piece_portal",
                    hash = "portal_wood".GetStableHashCode(),
                    type = CustomPinType.Portal 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 10, 
                    m_icon = Utilities.GetSprite("portal_wood".GetStableHashCode(), true) 
                } 
            }/*,
            { 
                new CustomPinData { 
                    name = "Boar", 
                    title = "Boar",
                    hash = 0,
                    type = CustomPinType.TamedAnimal 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 11, 
                    m_icon = Utilities.GetSprite("TrophyBoar".GetStableHashCode(), false) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Wolf", 
                    title = "Wolf",
                    hash = 0,
                    type = CustomPinType.TamedAnimal 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 12, 
                    m_icon = Utilities.GetSprite("TrophyWolf".GetStableHashCode(), false) 
                } 
            },
            { 
                new CustomPinData { 
                    name = "Lox", 
                    title = "Lox",
                    hash = 0,
                    type = CustomPinType.TamedAnimal 
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + 13, 
                    m_icon = Utilities.GetSprite("TrophyLox".GetStableHashCode(), false) 
                } 
            }*/
        };
        private static Dictionary<CustomPinData, Minimap.SpriteData> usedPins = new();
        private static GameObject cursorObj = null, exploredObj = null, bedObj = null, shipObj = null, portalObj = null, tombObj = null;
        public static Dictionary<ZDO, Minimap.PinData> zdoPins = new();
        private static Dictionary<Vector3, Minimap.PinData> locPins = new();
        private static Dictionary<string, List<Vector3>> locsFound = new();
        private static List<ZDO> shipsFound = new(), portalsFound = new(), tombsFound = new();
        public static List<Vector3> removedPins = new();
        private static List<Character> tempTamedAnimals = new();
        private static List<Minimap.PinData> tamedAnimalsPins = new();
        private static long exploredTotal = 0, mapSize = 0;
        private static bool isMinimalEffect = false;
        public static bool isOnBoat;
        private static float entrySpacingEffects = 0.0f;
        private static readonly int[] shipsHashes = {
            "VikingShip".GetStableHashCode(),
            "Karve".GetStableHashCode(),
            "Raft".GetStableHashCode(),
            "CargoShip".GetStableHashCode(),
            "BigCargoShip".GetStableHashCode(),
            "WarShip".GetStableHashCode(),
            "TransporterShip".GetStableHashCode(),
            "LittleBoat".GetStableHashCode(),
            "FishingBoat".GetStableHashCode(),
        };

        public static Block Start() {
            Minimap map = Minimap.instance;

            if (Main.enableMapStats.Value) {
                mapBlock = new Block(
                    "MapStats",
                    Main.mapStatsColor.Value,
                    Main.mapStatsSize.Value,
                    Main.mapStatsPosition.Value,
                    Main.mapStatsMargin.Value,
                    Main.mapStatsAlign.Value
                );
            }

            if (Main.showCursorCoordinates.Value) {
                GameObject original = map.m_biomeNameLarge.gameObject;
                cursorObj = UnityEngine.Object.Instantiate(original, original.transform);
                cursorObj.name = "CursorCoordinates";
                cursorObj.transform.SetParent(original.transform.parent);
                cursorObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
                cursorObj.GetComponent<TextMeshProUGUI>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform cursorRect = cursorObj.GetComponent<RectTransform>();
                cursorRect.anchorMin = cursorRect.anchorMax = cursorRect.pivot = Vector2.zero;
                cursorRect.anchoredPosition = new Vector2(15f, 5f);
                cursorObj.SetActive(true);
            }

            if (Main.showExploredPercentage.Value) {
                GameObject original = map.m_biomeNameLarge.gameObject;
                exploredObj = UnityEngine.Object.Instantiate(original, original.transform);
                exploredObj.name = "ExploredPercentage";
                exploredObj.transform.SetParent(original.transform.parent);
                exploredObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
                exploredObj.GetComponent<TextMeshProUGUI>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform exploredRect = exploredObj.GetComponent<RectTransform>();
                exploredRect.anchorMin = exploredRect.anchorMax = exploredRect.pivot = Vector2.up;
                exploredRect.anchoredPosition = new Vector2(15f, -5f);
                exploredObj.SetActive(true);
            }

            if (Main.enableBedStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                bedObj = UnityEngine.Object.Instantiate(template, template.transform);
                bedObj.name = "BedStatus";
                bedObj.transform.SetParent(template.transform.parent);
                bedObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                Outline statusOutline = bedObj.transform.Find("Icon").gameObject.AddComponent<Outline>();
                statusOutline.effectColor = Color.black;
                bedObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_bed");
                bedObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                bedObj.SetActive(false);
            }

            if (Main.enableShipStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                shipObj = UnityEngine.Object.Instantiate(template, template.transform);
                shipObj.name = "ShipStatus";
                shipObj.transform.SetParent(template.transform.parent);
                shipObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                Outline statusOutline = shipObj.transform.Find("Icon").gameObject.AddComponent<Outline>();
                statusOutline.effectColor = Color.black;
                shipObj.transform.Find("Name").GetComponent<Text>().text = "Ship";
                shipObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                shipObj.SetActive(false);
            }

            if (Main.enablePortalStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                portalObj = UnityEngine.Object.Instantiate(template, template.transform);
                portalObj.name = "PortalStatus";
                portalObj.transform.SetParent(template.transform.parent);
                portalObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                Outline statusOutline = portalObj.transform.Find("Icon").gameObject.AddComponent<Outline>();
                statusOutline.effectColor = Color.black;
                portalObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_portal");
                portalObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                portalObj.SetActive(false);
            }

            if (Main.enableTombStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                tombObj = UnityEngine.Object.Instantiate(template, template.transform);
                tombObj.name = "TombStatus";
                tombObj.transform.SetParent(template.transform.parent);
                tombObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                Outline statusOutline = tombObj.transform.Find("Icon").gameObject.AddComponent<Outline>();
                statusOutline.effectColor = Color.black;
                tombObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$alwe_tombstone");
                tombObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                tombObj.SetActive(false);
            }

            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.minimalstatuseffects")) {
                isMinimalEffect = true;
                GameObject[] myStatus = { bedObj, shipObj, portalObj, tombObj };
                Chainloader.PluginInfos.TryGetValue("randyknapp.mods.minimalstatuseffects", out BepInEx.PluginInfo modInfo);
                ConfigFile modConfig = modInfo.Instance.Config;
                modConfig.TryGetEntry(new ("General", "ListSize"), out ConfigEntry<Vector2> listSize);
                modConfig.TryGetEntry(new ("General", "EntrySpacing"), out ConfigEntry<float> entrySpacing);
                entrySpacingEffects = entrySpacing.Value;
                modConfig.TryGetEntry(new ("General", "IconSize"), out ConfigEntry<float> iconSize);
                modConfig.TryGetEntry(new ("General", "FontSize"), out ConfigEntry<int> fontSize);
                foreach (GameObject statusObj in myStatus.Where(obj => obj != null)) {
                    RectTransform nameRect = statusObj.transform.Find("Name") as RectTransform;
                    Text nameText = nameRect.GetComponent<Text>();
                    statusObj.transform.Find("TimeText").gameObject.SetActive(false);
                    nameText.alignment = TextAnchor.MiddleLeft;
                    nameText.supportRichText = true;
                    nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
                    nameText.resizeTextForBestFit = false;
                    nameText.fontSize = fontSize.Value;
                    nameRect.anchorMin = new Vector2(0, 0.5f);
                    nameRect.anchorMax = new Vector2(1, 0.5f);
                    nameRect.anchoredPosition = new (120 + iconSize.Value, 2);
                    nameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize.Value + 20);
                    nameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, listSize.Value.x);
                    RectTransform iconRect = statusObj.transform.Find("Icon") as RectTransform;
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize.Value);
                    iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize.Value);
                    iconRect.anchoredPosition = new Vector2(iconSize.Value, 0);
                }
            }

            foreach (Minimap.PinData pin in map.m_pins) {
                if (pin.m_type == Minimap.PinType.Ping) map.RemovePin(pin);
            }

            return mapBlock;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
        static void MinimapStart(Minimap __instance) {
            /*if (Main.showCustomMinimap.Value) {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string name = Enumerable.Single(assembly.GetManifestResourceNames(), str => str.EndsWith("CircleMask.png"));
                Stream stream = assembly.GetManifestResourceStream(name);
                byte[] byteArray;
                using (Stream resFilestream = assembly.GetManifestResourceStream(name)) {
                    byteArray = new byte[resFilestream.Length];
                    resFilestream.Read(byteArray, 0, byteArray.Length);
                }
                Texture2D bmp = new Texture2D(__instance.m_textureSize, __instance.m_textureSize, TextureFormat.RGBA32, false);
                bmp.name = "CircleTexture";
                bmp.LoadRawTextureData(byteArray);
                bmp.Apply();
                Sprite spt = Sprite.Create(bmp, new Rect(0, 0, __instance.m_textureSize, __instance.m_textureSize), Vector2.zero);
                spt.name = "CircleSprite";

                __instance.m_mapSmall.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                __instance.m_mapImageSmall.maskable = true;
                __instance.m_smallRoot.GetComponent<Image>().sprite = spt;
                __instance.m_smallRoot.GetComponent<Image>().preserveAspect = true;
                __instance.m_smallRoot.AddComponent<Mask>().showMaskGraphic = false;
            }*/
            if (Main.enableRotatingMinimap.Value) {
                __instance.m_pinRootSmall.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_smallMarker.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_smallShipMarker.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_windMarker.transform.SetParent(__instance.m_smallRoot.transform);
            }
            exploredTotal = 0;
            mapSize = __instance.m_explored.Length;
            float newSmallSize = __instance.m_smallMarker.sizeDelta.x * Utilities.GetCultureInvariant<float>(Main.playerMarkerScale.Value);
            float newLargeSize = __instance.m_largeMarker.sizeDelta.x * Utilities.GetCultureInvariant<float>(Main.playerMarkerScale.Value);
            __instance.m_smallMarker.sizeDelta = new(newSmallSize, newSmallSize);
            __instance.m_largeMarker.sizeDelta = new(newLargeSize, newLargeSize);
            if (!Chainloader.PluginInfos.ContainsKey("AMP_Configurable") 
                && !Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot")
                && !Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal") 
                && !Chainloader.PluginInfos.ContainsKey("Tekla_QoLPins")
                && !Chainloader.PluginInfos.ContainsKey("MarketplaceAndServerNPCs")) {
                __instance.m_visibleIconTypes = Enumerable.Repeat(true, Enum.GetValues(typeof(Minimap.PinType)).Length + usedPins.Count).ToArray();
            }
            usedPins.Do(p => __instance.m_icons.Add(p.Value));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Update))]
        static void MinimapUpdate(Minimap __instance) {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer == null) return;
            Vector2 mousePos = Input.mousePosition;
            RectTransform mapRect = __instance.m_largeRoot.GetComponent<RectTransform>();
            int totEffects = Hud.instance.m_statusEffects.Count + (Main.enableWeightStatus.Value ? 1 : 0);
            Vector3 playerPos3 = localPlayer.transform.position;
            Vector2 playerPos = new(playerPos3.x, playerPos3.z);
            Transform cameraTransform = Utils.GetMainCamera().transform;

            foreach (KeyValuePair<ZDO, Minimap.PinData> pin in zdoPins) {
                if (pin.Key.GetPrefab() == "portal_wood".GetStableHashCode()
                    && Utilities.CheckInEnum(CustomPinType.Portal, Main.showPinsTitles.Value)) pin.Value.m_name = pin.Key.GetString("tag", "");
                pin.Value.m_pos = pin.Key.GetPosition();            
            }

            foreach (KeyValuePair<string, List<Vector3>> loc in locsFound) { 
                foreach (Vector3 pos in loc.Value) {
                    if (!locPins.ContainsKey(pos) && !removedPins.Contains(pos.Round()) && Vector3.Distance(pos, playerPos3) <= 50f) {
                        KeyValuePair<CustomPinData, Minimap.SpriteData> pair = usedPins.Where(p => loc.Key.Contains(p.Key.name.ToLower())).FirstOrDefault();
                        string pinTitle = Utilities.CheckInEnum(pair.Key.type, Main.showPinsTitles.Value) ? pair.Key.title : "";
                        if (!__instance.HavePinInRange(pos, 1f)) {
                            Minimap.PinData locPin = __instance.AddPin(pos, pair.Value.m_name, Localization.instance.Localize(pinTitle), true, false);
                            locPin.m_doubleSize = Utilities.CheckInEnum(pair.Key.type, Main.biggerPins.Value);
                            locPins.Add(pos, locPin);
                        }
                    }
                }
            }

            if (Main.enableBedStatus.Value && bedObj != null) {
                PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                if (playerProfile.HaveCustomSpawnPoint()) {
                    Vector2 spawnPos = new(playerProfile.GetCustomSpawnPoint().x, playerProfile.GetCustomSpawnPoint().z);
                    float distance = Utils.DistanceXZ(playerPos3, playerProfile.GetCustomSpawnPoint());
                    string distanceText = distance < 1000f ? $"{distance:0.#} m" : $"{(distance / 1000f):0.#} km";
                    Vector2 cameraForward = new(cameraTransform.forward.x, cameraTransform.forward.z);
                    Vector2 cameraRight = new(cameraTransform.right.x, cameraTransform.right.z);
                    float forwardAngle = Vector2.Angle(spawnPos - playerPos, cameraForward);
                    float rightAngle = Vector2.Angle(spawnPos - playerPos, cameraRight);
                    Quaternion objRotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
                    if (isMinimalEffect) {
                        bedObj.GetComponent<RectTransform>().localPosition = new Vector3(0, - totEffects * (entrySpacingEffects - 1), 0);
                        Text nameText = bedObj.transform.Find("Name").GetComponent<Text>();
                        nameText.text = $"{Localization.instance.Localize("$piece_bed")} <color=#ffb75c>{distanceText}</color>";
                    } else {
                        bedObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - totEffects * Hud.instance.m_statusEffectSpacing, 0f);
                        bedObj.transform.Find("TimeText").GetComponent<Text>().text = distanceText;
                    }
                    bedObj.transform.Find("Icon").rotation = objRotation;
                    bedObj.SetActive(true);
                } else bedObj.SetActive(false);
            }

            if (Main.enablePortalStatus.Value && portalObj != null) {
                int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0);
                SetElementStatus(portalObj, portalsFound, playerPos, cameraTransform, space);
            }

            if (Main.enableShipStatus.Value && shipObj != null && !isOnBoat) {
                int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0) + 
                    (portalObj != null ? (portalObj.activeSelf ? 1 : 0) : 0);
                SetElementStatus(shipObj, shipsFound, playerPos, cameraTransform, space);
            } else if (Main.enableShipStatus.Value && shipObj != null && isOnBoat) shipObj.SetActive(false);

            if (Main.enableTombStatus.Value && tombObj != null) {
                int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0) + 
                    (portalObj != null ? (portalObj.activeSelf ? 1 : 0) : 0) + (shipObj != null ? (shipObj.activeSelf ? 1 : 0) : 0);
                SetElementStatus(tombObj, tombsFound, playerPos, cameraTransform, space);
            }

            if (Main.enableMapStats.Value && mapBlock != null) {
                Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit);
                mapBlock.SetText(String.Format(
                    Main.worldCoordinatesFormat.Value,
                    $"{playerPos3.x:0.#}", $"{playerPos3.y:0.#}", $"{playerPos3.z:0.#}",
                    $"{hit.point.x:0.#}", $"{hit.point.y:0.#}", $"{hit.point.z:0.#}"
                ));
            }

            if (Main.showCursorCoordinates.Value && cursorObj != null && RectTransformUtility.RectangleContainsScreenPoint(mapRect, mousePos)) {
                Vector3 cursor = __instance.ScreenToWorldPoint(mousePos);
                cursor.y = WorldGenerator.instance.GetHeight(cursor.x, cursor.z);
                cursorObj.GetComponent<TextMeshProUGUI>().text = String.Format(
                    Main.mapCoordinatesFormat.Value,
                    $"{cursor.x:0.#}", $"{cursor.z:0.#}", $"{cursor.y:0.#}"
                );
            }

            if (Main.enableRotatingMinimap.Value) {
                Vector3 playerAngles = localPlayer.m_eye.transform.rotation.eulerAngles;
                __instance.m_mapImageSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
                __instance.m_pinRootSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
                for (int i = 0; i < __instance.m_pinRootSmall.childCount; i++)
                    __instance.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
                if (__instance.m_mode == Minimap.MapMode.Small) {
                    __instance.m_smallMarker.rotation = Quaternion.identity;
                    Vector3 windAngles = Quaternion.LookRotation(EnvMan.instance.GetWindDir()).eulerAngles;
                    __instance.m_windMarker.rotation = Quaternion.Euler(0f, 0f, playerAngles.y - windAngles.y);
                    Ship controlledShip = localPlayer.GetControlledShip();
                    if (controlledShip) {
                        __instance.m_smallShipMarker.gameObject.SetActive(true);
                        __instance.m_smallShipMarker.transform.rotation = Quaternion.Euler(0f, 0f, controlledShip.GetShipYawAngle());
                    } else __instance.m_smallShipMarker.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RuneStone), nameof(RuneStone.Interact))]
        static void AddRuneStonePin(RuneStone __instance) {
            if (!Utilities.CheckInEnum(CustomPinType.RuneStone, Main.showCustomPins.Value)) return;
            Minimap map = Minimap.instance;
            if (__instance != null && map != null && string.IsNullOrEmpty(__instance.m_locationName)) {
                Vector3 runePos = __instance.transform.position;
                string pinTitle = Utilities.CheckInEnum(CustomPinType.RuneStone, Main.showPinsTitles.Value) ? 
                    (String.IsNullOrEmpty(__instance.m_label) ? 
                    Localization.instance.Localize(__instance.m_name) : 
                    Localization.instance.Localize(__instance.m_label)) : "";
                if (!locPins.ContainsKey(runePos) && !removedPins.Contains(runePos.Round())) {
                    Minimap.PinData runePin = map.AddPin(runePos, Minimap.PinType.Icon4, pinTitle, true, false);
                    runePin.m_doubleSize = Utilities.CheckInEnum(CustomPinType.RuneStone, Main.biggerPins.Value);
                    locPins.Add(runePos, runePin);
                    //Debug.Log($"Added runestone pin with position {runePos}");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Location), nameof(Location.Awake))]
        static void AddDungeonPin(Location __instance) {
            Location location = __instance.GetComponent<Location>();
            if (location != null) {
                Vector3 locPos = location.transform.position;
                string locName = location.name.ToLower();
                if (!locPins.ContainsKey(locPos) && !removedPins.Contains(locPos.Round()) && 
                    !locsFound.Any(p => p.Value.Contains(locPos)) && usedPins.Any(p => locName.Contains(p.Key.name.ToLower()))) {
                    if (locsFound.ContainsKey(locName)) {
                        locsFound.TryGetValue(locName, out List<Vector3> locations);
                        locations.Add(locPos);
                    } else locsFound.Add(locName, new List<Vector3> { locPos });
                    //Debug.Log($"Added location pin {locName} with position {locPos}");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNetScene), methodName: nameof(ZNetScene.AddInstance))]
        static void PatchCreateZDO(ZNetScene __instance, ZDO zdo, ZNetView nview) {
            Minimap map = Minimap.instance;
            int prefabHash = zdo.GetPrefab();
            if (map != null && zdo.IsValid() && usedPins.Any(p => prefabHash == p.Key.hash)) SetElementPin(map, zdo);
            TombStone tombComp = nview.gameObject.GetComponent<TombStone>();
            if (zdo.IsValid() && portalObj != null && prefabHash == "portal_wood".GetStableHashCode() && !portalsFound.Contains(zdo)) {
                portalsFound.Add(zdo);
            } else if (zdo.IsValid() && shipObj != null && shipsHashes.Contains(prefabHash) && !shipsFound.Contains(zdo)) {
                shipsFound.Add(zdo);
            } else if (zdo.IsValid() && tombObj != null && prefabHash == "Player_tombstone".GetStableHashCode() 
                && !tombsFound.Contains(zdo) && tombComp != null) {
                if ((Main.onlyOwnTomb.Value && tombComp.IsOwner()) || !Main.onlyOwnTomb.Value) tombsFound.Add(zdo);
            } 
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Destroy))]
        static bool PatchDestroyZDO(ZNetScene __instance, GameObject go) {
            if (!Utilities.CheckInEnum(CustomPinType.Disabled, Main.showCustomPins.Value) 
                && !Main.enablePortalStatus.Value && !Main.enableShipStatus.Value) return true;
            ZNetView component = go.GetComponent<ZNetView>();
            if (component && component.GetZDO() != null) {
                Minimap map = Minimap.instance;
                ZDO zdo = component.GetZDO();
                if (map && usedPins.Any(p => zdo.GetPrefab() == p.Key.hash)) {
                    Minimap.PinData customPin;
                    if (zdoPins.TryGetValue(zdo, out customPin)) {
                        map.RemovePin(customPin);
                        zdoPins.Remove(zdo);
                    }
                    if (locPins.TryGetValue(zdo.GetPosition(), out customPin)) {
                        map.RemovePin(customPin);
                        locPins.Remove(zdo.GetPosition());
                    }
                }
                if (shipsFound.Contains(zdo)) {
                    shipsFound.Remove(zdo);
                }
                if (portalsFound.Contains(zdo)) {
                    portalsFound.Remove(zdo);
                }
                if (tombsFound.Contains(zdo)) {
                    tombsFound.Remove(zdo);
                }
                component.ResetZDO();
                __instance.m_instances.Remove(zdo);
                if (zdo.IsOwner()) ZDOMan.instance.DestroyZDO(zdo);
            }
            UnityEngine.Object.Destroy(go);
            return false;
        }

        private static void SetElementStatus(GameObject element, List<ZDO> list, Vector2 playerPos, Transform camera, int space) {
            if (list.Count > 0) {
                List<float> distances = list.Select(p => {
                    Vector3 p3 = p.GetPosition();
                    return Vector2.Distance(playerPos, new Vector2(p3.x, p3.z));
                }).ToList();
                float closer = distances.Min();
                string distance = closer < 1000f ? $"{closer:0.#} m" : $"{(closer / 1000f):0.#} km";
                ZDO closerZDO = list[distances.IndexOf(closer)];
                Vector2 closerPos = new(closerZDO.GetPosition().x, closerZDO.GetPosition().z);
                string prefabName = "";
                if (list.All(shipsFound.Contains)) {
                    try {
                        var locPin = usedPins.Where(p => p.Key.hash == closerZDO.GetPrefab()).First();
                        prefabName = Localization.instance.Localize(locPin.Key.title);
                    } catch (System.Exception) { prefabName = "Ship"; }
                } else if (list.All(portalsFound.Contains)) prefabName = Localization.instance.Localize("$piece_portal");
                else if (list.All(tombsFound.Contains)) prefabName = Localization.instance.Localize("$alwe_tombstone");
                Vector2 cameraForward = new(camera.forward.x, camera.forward.z);
                Vector2 cameraRight = new(camera.right.x, camera.right.z);
                float forwardAngle = Vector2.Angle(closerPos - playerPos, cameraForward);
                float rightAngle = Vector2.Angle(closerPos - playerPos, cameraRight);
                Quaternion objRotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
                if (isMinimalEffect) {
                    element.GetComponent<RectTransform>().localPosition = new Vector3(0, - space * (entrySpacingEffects - 1), 0);
                    Text nameText = element.transform.Find("Name").GetComponent<Text>();
                    nameText.text = $"{prefabName} <color=#ffb75c>{distance}</color>";
                } else {
                    element.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - space * Hud.instance.m_statusEffectSpacing, 0f);
                    element.transform.Find("TimeText").GetComponent<Text>().text = distance;
                    element.transform.Find("Name").GetComponent<Text>().text = prefabName;
                }
                element.transform.Find("Icon").rotation = objRotation;
                element.SetActive(true);
            } else element.SetActive(false);
        }
        
        private static void SetElementPin(Minimap map, ZDO zdo) {
            if (!zdoPins.TryGetValue(zdo, out Minimap.PinData customPin)) {
                KeyValuePair<CustomPinData, Minimap.SpriteData> pair = usedPins.Where(p => p.Key.hash == zdo.GetPrefab()).FirstOrDefault();
                string pinTitle = Utilities.CheckInEnum(pair.Key.type, Main.showPinsTitles.Value) ? pair.Key.title : "";
                Minimap.PinData zdoPin = map.GetClosestPin(zdo.GetPosition(), 1f);
                if (zdoPin == null) zdoPin = map.AddPin(zdo.GetPosition(), pair.Value.m_name, name: Localization.instance.Localize(pinTitle), true, false);
                zdoPin.m_doubleSize = Utilities.CheckInEnum(pair.Key.type, Main.biggerPins.Value);
                zdoPins.Add(zdo, zdoPin);
            }
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateDynamicPins))]
        static void PatchDynamicPins(Minimap __instance, float dt) {
            if (Utilities.CheckInEnum(CustomPinType.TamedAnimal, Main.showCustomPins.Value) && Minimap.IsOpen()) {
                tempTamedAnimals.Clear();
                tempTamedAnimals = Character.GetAllCharacters().Where(c => c.IsTamed() && !c.IsPlayer() && !c.name.Contains("Hen")).ToList();
                if (tamedAnimalsPins.Count != tempTamedAnimals.Count) {
                    foreach (Minimap.PinData pin in tamedAnimalsPins) {
                        __instance.RemovePin(pin);
                    }
                    tamedAnimalsPins.Clear();
                    foreach (Character animal in tempTamedAnimals) {
                        Minimap.PinType pinType = Minimap.PinType.Icon3;
                        try {
                            KeyValuePair<CustomPinData, Minimap.SpriteData> pair = usedPins.Where(p => animal.name.Contains(p.Key.name)).First();
                            pinType = pair.Value.m_name;
                        } catch (Exception) {
                            pinType = Minimap.PinType.Icon3;
                        }
                        string localizedName = Localization.instance.Localize(animal.m_name);
                        string pinTitle = Utilities.CheckInEnum(CustomPinType.TamedAnimal, Main.showPinsTitles.Value) &&
                            animal.GetHoverName() != localizedName ? animal.GetHoverName() : "";
                        Minimap.PinData pin =  __instance.AddPin(animal.GetCenterPoint(), pinType, pinTitle, true, false);
                        pin.m_doubleSize = Utilities.CheckInEnum(CustomPinType.TamedAnimal, Main.biggerPins.Value);
                        tamedAnimalsPins.Add(pin);
                    }
                }
                for (int i = 0; i < tempTamedAnimals.Count; i++) {
                    Minimap.PinData pinData = tamedAnimalsPins[i];
                    Character tamedAnimal = tempTamedAnimals[i];
                    if (pinData.m_name == tamedAnimal.GetHoverName()) {
                        pinData.m_pos = Vector3.MoveTowards(pinData.m_pos, tamedAnimal.GetCenterPoint(), 200f * dt);
                    } else {
                        pinData.m_name = tamedAnimal.m_name;
                        pinData.m_pos = tamedAnimal.GetCenterPoint();
                    }
                }
            }
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
        static void PatchAddPin(ref Minimap.PinData __result, Minimap.PinType type) {
            if (Main.replaceBedPinIcon.Value && type == Minimap.PinType.Bed) __result.m_icon = Utilities.GetSprite("bed".GetStableHashCode(), true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.RemovePin), new Type[] { typeof(Minimap.PinData)})]
        static void PatchRemovePin(Minimap __instance, Minimap.PinData pin) {
            Vector3 roundedVec = pin.m_pos.Round(); 
            if (!removedPins.Contains(roundedVec) 
                && pinsDict.Any(pair => pair.Value.m_name == pin.m_type)) removedPins.Add(roundedVec);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetMapData))]
        static bool PatchGetMapData(Minimap __instance) {
            __instance.m_pins.ForEach(p => {
                if ((int) p.m_type >= Enum.GetValues(typeof(Minimap.PinType)).Length) p.m_save = false;
            });
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.LoadMapData))]
        static void PatchLoadMapData(Minimap __instance) {
            LoadLocations();
            LoadZDOs();
            LoadTombs();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), new Type[] { typeof(int), typeof(int)})]
        static void PatchExplore(ref bool __result) {
            if (Main.showExploredPercentage.Value && exploredObj != null && __result) {
                exploredTotal += 1;
                float exploredYouPercentage = exploredTotal * 100f / mapSize;
                exploredObj.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(text: $"$alwe_explored : {exploredYouPercentage:0.##} %");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Reset))]
        static void PatchReset() {
            if (Main.showExploredPercentage.Value && exploredObj != null) {
                exploredTotal = 0;
                float exploredYouPercentage = exploredTotal * 100f / mapSize;
                exploredObj.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(text: $"$alwe_explored : {exploredYouPercentage:0.##} %");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapDblClick))]
        static bool PatchOnMapDoubleClick(Minimap __instance) {
            if (Utilities.CheckInEnum(CustomPinType.Disabled, Main.showCustomPins.Value)) return true;
            if (__instance.m_selectedType == Minimap.PinType.Death) return false;
            Vector3 pos = __instance.ScreenToWorldPoint(Input.mousePosition);
            Minimap.PinData closestPin = __instance.GetClosestPin(pos, __instance.m_removeRadius * (__instance.m_largeZoom * 2f));
            if (closestPin != null) {
                if (closestPin.m_ownerID == 0L && (int) closestPin.m_type >= Enum.GetValues(typeof(Minimap.PinType)).Length) {
                    __instance.ShowPinNameInput(closestPin.m_pos);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chat), nameof(Chat.UpdateWorldTextField))]
        static void PatchPingDistance(Chat.WorldTextInstance wt) {
            if (Main.showPingDistance.Value && wt.m_type == Talker.Type.Ping) {
                float distance = Utils.DistanceXZ(Player.m_localPlayer.transform.position, wt.m_position);
                string distanceText = distance < 1000f ? $"\n{distance:0.#} m" : $"\n{(distance / 1000f):0.#} km";
                wt.m_textMeshField.text += distanceText;
            }
        }

        private static void LoadLocations() {
            Minimap map = Minimap.instance;
            if (!Utilities.CheckInEnum(CustomPinType.Disabled, Main.showCustomPins.Value) && map != null) {
                List<ZoneSystem.LocationInstance> locations = Enumerable.ToList<ZoneSystem.LocationInstance>(ZoneSystem.instance.GetLocationList());
                foreach (ZoneSystem.LocationInstance loc in locations.Where(l => l.m_placed == true)) {
                    Vector3 locPos = loc.m_position;
                    string prefabName = loc.m_location.m_prefabName.ToLower();
                    if (!removedPins.Contains(locPos.Round()) && usedPins.Any(p => prefabName.Contains(p.Key.name.ToLower()))) {
                        KeyValuePair<CustomPinData, Minimap.SpriteData> pair = usedPins.Where(p => prefabName.Contains(p.Key.name.ToLower())).FirstOrDefault();
                        string pinTitle = Utilities.CheckInEnum(pair.Key.type, Main.showPinsTitles.Value) ? pair.Key.title : "";
                        Minimap.PinData locPin = map.GetClosestPin(locPos, 1f);
                        if (locPin == null) locPin = map.AddPin(locPos, pair.Value.m_name, Localization.instance.Localize(pinTitle), true, false);
                        locPin.m_doubleSize = Utilities.CheckInEnum(pair.Key.type, Main.biggerPins.Value); 
                        locPins.Add(locPos, locPin);
                    }
                }
                if (locPins.Count > 0) Debug.Log($"Loaded {locPins.Count} locations pins");
            }
        }

        private static void LoadTombs() {
            if (Main.enableTombStatus.Value && tombObj != null) {
                TombStone[] tombs = UnityEngine.Object.FindObjectsOfType<TombStone>();
                foreach (TombStone tomb in tombs) {
                    ZDO zdo = tomb.GetComponent<ZNetView>().GetZDO();
                    if (zdo.IsValid()) tombsFound.Add(zdo);
                }
            }
        }

        private static void LoadZDOs() {
            Minimap map = Minimap.instance;
            if ((!Utilities.CheckInEnum(CustomPinType.Disabled, Main.showCustomPins.Value) || Main.enablePortalStatus.Value 
                || Main.enableShipStatus.Value) && map != null) {
                foreach (ZDO zdo in ZDOMan.instance.m_objectsByID.Values.ToList()) {
                    int prefabHash = zdo.GetPrefab();
                    Vector3 zdoPos = zdo.GetPosition().Round();
                    if (zdo.IsValid() && !removedPins.Contains(zdoPos) && usedPins.Any(p => p.Key.hash == prefabHash)) SetElementPin(map, zdo);
                    if (zdo.IsValid() && portalObj != null && prefabHash == "portal_wood".GetStableHashCode()) {
                        portalsFound.Add(zdo);
                    } else if (zdo.IsValid() && shipObj != null && shipsHashes.Contains(prefabHash) && !shipsFound.Contains(zdo)) {
                        shipsFound.Add(zdo);
                    }
                }
                if (zdoPins.Count > 0) Debug.Log($"Loaded {zdoPins.Count} zdos pins");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNet), nameof(ZNet.LoadWorld))]
        static void PatchLoadWorld(ZNet __instance) {
            isOnBoat = false;
            zdoPins.Clear();
            locPins.Clear();
            locsFound.Clear();
            shipsFound.Clear();
            portalsFound.Clear();
            tombsFound.Clear();
            usedPins = pinsDict.Keys.ToDictionary(k => k, v => pinsDict[v]);
            if (Chainloader.PluginInfos.ContainsKey("marlthon.OdinShip") && Utilities.CheckInEnum(CustomPinType.Ship, Main.showCustomPins.Value)) {
                usedPins.Add(new CustomPinData { 
                    name = "Cargo Ship",
                    title = "Cargo Ship",
                    hash = "CargoShip".GetStableHashCode(),
                    type = CustomPinType.Ship
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + pinsDict.Count, 
                    m_icon = Utilities.GetSprite("CargoShip".GetStableHashCode(), true) 
                });
                usedPins.Add(new CustomPinData { 
                    name = "Big Cargo Ship", 
                    title = "Big Cargo Ship",
                    hash = "BigCargoShip".GetStableHashCode(),
                    type = CustomPinType.Ship
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + pinsDict.Count + 1, 
                    m_icon = Utilities.GetSprite("BigCargoShip".GetStableHashCode(), true) 
                });
                usedPins.Add(new CustomPinData { 
                    name = "War Ship", 
                    title = "War Ship",
                    hash = "WarShip".GetStableHashCode(),
                    type = CustomPinType.Ship
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + pinsDict.Count + 2, 
                    m_icon = Utilities.GetSprite("WarShip".GetStableHashCode(), true) 
                });
                usedPins.Add(key: new CustomPinData { 
                    name = "Transporter Ship", 
                    title = "Transporter Ship",
                    hash = "TransporterShip".GetStableHashCode(),
                    type = CustomPinType.Ship
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(enumType: typeof(Minimap.PinType)).Length + pinsDict.Count + 3, 
                    m_icon = Utilities.GetSprite("TransporterShip".GetStableHashCode(), true) 
                });
                usedPins.Add(new CustomPinData { 
                    name = "Little Boat", 
                    title = "Little Boat",
                    hash = "LittleBoat".GetStableHashCode(),
                    type = CustomPinType.Ship
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + pinsDict.Count + 4, 
                    m_icon = Utilities.GetSprite("LittleBoat".GetStableHashCode(), true) 
                });
                usedPins.Add(new CustomPinData { 
                    name = "Fishing Boat", 
                    title = "Fishing Boat",
                    hash = "FishingBoat".GetStableHashCode(),
                    type = CustomPinType.Ship
                }, 
                new Minimap.SpriteData { 
                    m_name = (Minimap.PinType) Enum.GetValues(typeof(Minimap.PinType)).Length + pinsDict.Count + 5, 
                    m_icon = Utilities.GetSprite("FishingBoat".GetStableHashCode(), true) 
                });
            }
            if (!Utilities.CheckInEnum(CustomPinType.TrollCave, Main.showCustomPins.Value)) usedPins.Remove(usedPins.First(p => p.Key.name == "TrollCave").Key);
            if (!Utilities.CheckInEnum(CustomPinType.MountainCave, Main.showCustomPins.Value)) usedPins.Remove(usedPins.First(p => p.Key.name == "MountainCave").Key);
            if (!Utilities.CheckInEnum(CustomPinType.InfestedMine, Main.showCustomPins.Value)) usedPins.Remove(usedPins.First(p => p.Key.name == "Mistlands_DvergrTownEntrance").Key);
            if (!Utilities.CheckInEnum(CustomPinType.Crypt, Main.showCustomPins.Value)) {
                usedPins.Remove(usedPins.First(p => p.Key.name == "SunkenCrypt").Key);
                usedPins.Remove(usedPins.First(p => p.Key.name == "Crypt").Key);
            }
            if (!Utilities.CheckInEnum(CustomPinType.FireHole, Main.showCustomPins.Value)) usedPins.Remove(usedPins.First(p => p.Key.name == "FireHole").Key);
            if (!Utilities.CheckInEnum(CustomPinType.Portal, Main.showCustomPins.Value)) usedPins.Remove(usedPins.First(p => p.Key.name == "Portal").Key);
            if (!Utilities.CheckInEnum(CustomPinType.Ship, Main.showCustomPins.Value)) {
                usedPins.Remove(usedPins.First(p => p.Key.name == "Raft").Key);
                usedPins.Remove(usedPins.First(p => p.Key.name == "Karve").Key);
                usedPins.Remove(usedPins.First(p => p.Key.name == "Viking Ship").Key);
            }
            if (!Utilities.CheckInEnum(CustomPinType.Cart, Main.showCustomPins.Value)) usedPins.Remove(usedPins.First(p => p.Key.name == "Cart").Key);
            /*if (!Utilities.CheckInEnum(CustomPinType.TamedAnimal, Main.showCustomPins.Value)) {
                usedPins.Remove(usedPins.First(p => p.Key.name == "Boar").Key);
                usedPins.Remove(usedPins.First(p => p.Key.name == "Wolf").Key);
                usedPins.Remove(usedPins.First(p => p.Key.name == "Lox").Key);
            }*/
            List<WorldInfo> worldInfos = Utilities.GetWorldInfos();
            if (worldInfos != null) {
                WorldInfo worldInfo = worldInfos.FirstOrDefault(wi => wi.worldName == ZNet.instance.GetWorldName());
                if (worldInfo != null) {
                    removedPins = worldInfo.removedPins;
                    if (removedPins.Count > 0) Debug.Log($"Removed {removedPins.Count} pins");
                }
            }
        }
    }
}
