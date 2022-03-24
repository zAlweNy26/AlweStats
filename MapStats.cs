using UnityEngine;
using HarmonyLib;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AlweStats {
    [HarmonyPatch]
    public static class MapStats {
        private static Block mapBlock = null;
        private static readonly Dictionary<string, int> pinsObjs = new() {
            { "SunkenCrypt", "TrophySkeletonPoison".GetStableHashCode() },
            { "TrollCave", "TrophyFrostTroll".GetStableHashCode() },
            { "FireHole", "TrophySurtling".GetStableHashCode() },
            { "Crypt", "TrophySkeleton".GetStableHashCode() },
            { "Cart", "Cart".GetStableHashCode() },
            { "Raft", "Raft".GetStableHashCode() },
            { "Karve", "Karve".GetStableHashCode() },
            { "Viking Ship", "VikingShip".GetStableHashCode() },
            { "Wood Portal", "portal_wood".GetStableHashCode() },
            { "Stone Portal", "portal".GetStableHashCode() }
        };
        private static GameObject cursorObj = null, exploredObj = null, bedObj = null, portalObj = null, shipObj = null;
        private static Dictionary<ZDO, Minimap.PinData> zdoPins = new();
        private static Dictionary<Vector3, Minimap.PinData> locPins = new();
        private static Dictionary<int, List<ZDO>> foundPrefabs = new();

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
            if (Main.showCursorCoordinatesInMap.Value) {
                GameObject original = map.m_biomeNameLarge.gameObject;
                cursorObj = UnityEngine.Object.Instantiate(original, original.transform);
                cursorObj.name = "CursorCoordinates";
                cursorObj.transform.SetParent(original.transform.parent);
                cursorObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                cursorObj.GetComponent<Text>().fontSize = Main.largeMapInfoSize.Value;
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
                exploredObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                exploredObj.GetComponent<Text>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform exploredRect = exploredObj.GetComponent<RectTransform>();
                exploredRect.anchorMin = exploredRect.anchorMax = exploredRect.pivot = Vector2.up;
                exploredRect.anchoredPosition = new Vector2(15f, -5f);
                exploredObj.SetActive(true);
            }
            if (Main.enableBedStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                bedObj = UnityEngine.Object.Instantiate(template, template.transform);
                bedObj.transform.SetParent(template.transform.parent);
                bedObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                bedObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_bed");
                bedObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                bedObj.SetActive(false);
            }
            if (Main.enableShipStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                shipObj = UnityEngine.Object.Instantiate(template, template.transform);
                shipObj.transform.SetParent(template.transform.parent);
                shipObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                shipObj.transform.Find("Name").GetComponent<Text>().text = "Ship";
                shipObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                shipObj.SetActive(false);
            }
            if (Main.enablePortalStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                portalObj = UnityEngine.Object.Instantiate(template, template.transform);
                portalObj.transform.SetParent(template.transform.parent);
                portalObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                portalObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_portal");
                portalObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                portalObj.SetActive(false);
            }
            return mapBlock;
        }

        public static void Update() {
            Minimap map = Minimap.instance;
            Player localPlayer = Player.m_localPlayer;
            Vector2 mousePos = Input.mousePosition;
            if (localPlayer == null || map == null) return;
            RectTransform mapRect = map.m_largeRoot.GetComponent<RectTransform>();
            int totEffects = Hud.instance.m_statusEffects.Count;
            Vector3 playerPos3 = localPlayer.transform.position;
            Vector2 playerPos = new(playerPos3.x, playerPos3.z);
            Transform cameraTransform = Utils.GetMainCamera().transform;
            //Vector2 cameraPos = new(cameraTransform.position.x, cameraTransform.position.z);
            Vector3 cameraAngles = cameraTransform.rotation.eulerAngles;

            if ((CheckForPin(1) || CheckForPin(2) || CheckForPin(3)) && locPins.Count == 0) {
                if (!CheckForPin(1)) pinsObjs.Remove("TrollCave");
                if (!CheckForPin(3)) pinsObjs.Remove("FireHole");
                if (!CheckForPin(2)) {
                    pinsObjs.Remove("SunkenCrypt");
                    pinsObjs.Remove("Crypt");
                }
                List<ZoneSystem.LocationInstance> locations = Enumerable.ToList<ZoneSystem.LocationInstance>(ZoneSystem.instance.GetLocationList());
                foreach (ZoneSystem.LocationInstance loc in locations.Where(l => l.m_placed == true)) {
                    //Debug.Log($"Location {loc.m_location.m_prefabName} | {loc.m_position} | {loc.m_placed}");
                    string prefabName = loc.m_location.m_prefabName.ToLower();
                    if (!locPins.ContainsKey(loc.m_position) && pinsObjs.Any(p => prefabName.Contains(p.Key.ToLower()))) {
                        KeyValuePair<string, int> pair = pinsObjs.Where(p => prefabName.Contains(p.Key.ToLower())).FirstOrDefault();
                        int dungeonType = pair.Key.Contains("Crypt") ? 2 : (pair.Key.Contains("TrollCave") ? 1 : 3);
                        string pinTitle = CheckForPinTitle(dungeonType) ? Regex.Replace(pair.Key, "([a-z])([A-Z])", "$1 $2") : "";
                        Minimap.PinData customPin = map.AddPin(loc.m_position, Minimap.PinType.Death, pinTitle, false, false);
                        customPin.m_icon = GetSprite(pair.Value, false);
                        customPin.m_doubleSize = CheckForPinSize(dungeonType);
                        locPins.Add(loc.m_position, customPin);
                    }
                }
            }

            foreach (int prefab in foundPrefabs.Keys.ToList()) {
                if (CheckForPin(6) && foundPrefabs.ContainsKey("Cart".GetStableHashCode())) {
                    foreach (ZDO zdo in foundPrefabs["Cart".GetStableHashCode()]) {
                        Minimap.PinData customPin;
                        bool pinWasFound = zdoPins.TryGetValue(zdo, out customPin);
                        if (!pinWasFound) {
                            string pinTitle = CheckForPinTitle(6) ? "Cart" : "";
                            customPin = map.AddPin(zdo.m_position, Minimap.PinType.Death, pinTitle, false, false);
                            customPin.m_icon = GetSprite(zdo.m_prefab, true);
                            customPin.m_doubleSize = CheckForPinSize(6);
                            zdoPins.Add(zdo, customPin);
                        } else customPin.m_pos = zdo.m_position;
                    }
                }
                if (Main.enableShipStatus.Value || CheckForPin(5)) {
                    List<ZDO> shipsFound = new(), raftsList = new(), karvesList = new(), vikingsList = new();
                    foundPrefabs.TryGetValue("Raft".GetStableHashCode(), out raftsList);
                    foundPrefabs.TryGetValue("Karve".GetStableHashCode(), out karvesList);
                    foundPrefabs.TryGetValue("VikingShip".GetStableHashCode(), out vikingsList);
                    if (raftsList != null) shipsFound = shipsFound.Concat(raftsList).ToList();
                    if (karvesList != null) shipsFound = shipsFound.Concat(karvesList).ToList();
                    if (vikingsList != null) shipsFound = shipsFound.Concat(vikingsList).ToList();
                    if (Main.enableShipStatus.Value && shipObj != null) {
                        int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0);
                        if (shipsFound.Count > 0) {
                            List<float> distances = shipsFound.Select(p => {
                                Vector3 p3 = p.GetPosition();
                                return Vector2.Distance(playerPos, new Vector2(p3.x, p3.z));
                            }).ToList();
                            float closer = distances.Min();
                            ZDO closerShip = shipsFound[distances.IndexOf(closer)];
                            string shipName = pinsObjs.Where(p => p.Value == closerShip.GetPrefab()).FirstOrDefault().Key;
                            Vector3 shipPos3 = closerShip.GetPosition();
                            Vector2 shipPos = new(shipPos3.x, shipPos3.z);
                            shipObj.transform.Find("Name").GetComponent<Text>().text = shipName;
                            SetElementStatus(shipObj, playerPos, shipPos, cameraTransform, space);
                        } else shipObj.SetActive(false);
                    }
                    if (CheckForPin(5)) {
                        foreach (ZDO zdo in shipsFound) {
                            Minimap.PinData customPin;
                            bool pinWasFound = zdoPins.TryGetValue(zdo, out customPin);
                            if (!pinWasFound) {
                                string shipName = pinsObjs.Where(p => p.Value == zdo.GetPrefab()).FirstOrDefault().Key;
                                string pinTitle = CheckForPinTitle(5) ? shipName : "";
                                customPin = map.AddPin(zdo.m_position, Minimap.PinType.Death, pinTitle, false, false);
                                customPin.m_icon = GetSprite(zdo.GetPrefab(), true);
                                customPin.m_doubleSize = CheckForPinSize(5);
                                zdoPins.Add(zdo, customPin);
                            } else customPin.m_pos = zdo.m_position;
                        }
                    }
                }
                if (Main.enablePortalStatus.Value || CheckForPin(4)) {
                    List<ZDO> portalsFound = new(), woodsList = new(), stonesList = new();
                    foundPrefabs.TryGetValue("portal_wood".GetStableHashCode(), out woodsList);
                    foundPrefabs.TryGetValue("portal".GetStableHashCode(), out stonesList);
                    if (woodsList != null) portalsFound = portalsFound.Concat(woodsList).ToList();
                    if (stonesList != null) portalsFound = portalsFound.Concat(stonesList).ToList();
                    if (Main.enablePortalStatus.Value && portalObj != null) {
                        int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0) + (shipObj != null ? (shipObj.activeSelf ? 1 : 0) : 0);
                        if (portalsFound.Count > 0) {
                            List<float> distances = portalsFound.Select(p => {
                                Vector3 p3 = p.GetPosition();
                                return Vector2.Distance(playerPos, new Vector2(p3.x, p3.z));
                            }).ToList();
                            float closer = distances.Min();
                            Vector3 portalPos3 = portalsFound[distances.IndexOf(closer)].GetPosition();
                            Vector2 portalPos = new(portalPos3.x, portalPos3.z);
                            SetElementStatus(portalObj, playerPos, portalPos, cameraTransform, space);
                        } else portalObj.SetActive(false);
                    }
                    if (CheckForPin(4)) {
                        foreach (ZDO zdo in portalsFound) {
                            Minimap.PinData customPin;
                            bool pinWasFound = zdoPins.TryGetValue(zdo, out customPin);
                            if (!pinWasFound) {
                                string portalTag = CheckForPinTitle(4) ? zdo.GetString("tag", "") : "";
                                customPin = map.AddPin(zdo.m_position, Minimap.PinType.Death, portalTag, false, false);
                                customPin.m_icon = GetSprite(zdo.m_prefab, true);
                                customPin.m_doubleSize = CheckForPinSize(4);
                                zdoPins.Add(zdo, customPin);
                            } else {
                                string portalTag = CheckForPinTitle(4) ? zdo.GetString("tag", "") : "";
                                customPin.m_name = portalTag;
                                customPin.m_pos = zdo.m_position;
                            }
                        }
                    }
                }
            }

            /*timeInterval += Time.deltaTime;
            if ((Main.enableCustomPins.Value || Main.enablePortalStatus.Value || Main.enableShipStatus.Value) && timeInterval >= updateTime) {
                Dictionary<ZDO, Minimap.PinData> tempDict = zdoPins.Keys.ToDictionary(k => k, k => zdoPins[k]);
                foreach (KeyValuePair<ZDO, Minimap.PinData> pin in tempDict) {
                    if (!pin.Key.IsValid()) {
                        map.RemovePin(pin.Value);
                        zdoPins.Remove(pin.Key);
                    }
                }
                shipsFound.Clear();
                portalsFound.Clear();
                foundPrefabs.Clear();
                foreach (ZDO zdo in ZDOMan.instance.m_objectsByID.Values.ToList()) {
                    if (pinsObjs.Any(p => p.Value == zdo.GetPrefab())) {
                        if (foundPrefabs.ContainsKey(zdo.GetPrefab())) foundPrefabs[zdo.GetPrefab()].Add(zdo);
                        else foundPrefabs.Add(zdo.GetPrefab(), new List<ZDO>(){ zdo });
                    }
                }
                List<ZDO> raftsList = new(), karvesList = new(), vikingsList = new(), woodsList = new(), stonesList = new();
                foundPrefabs.TryGetValue("Raft".GetStableHashCode(), out raftsList);
                foundPrefabs.TryGetValue("Karve".GetStableHashCode(), out karvesList);
                foundPrefabs.TryGetValue("VikingShip".GetStableHashCode(), out vikingsList);
                foundPrefabs.TryGetValue("portal_wood".GetStableHashCode(), out woodsList);
                foundPrefabs.TryGetValue("portal".GetStableHashCode(), out stonesList);
                if (raftsList != null) shipsFound = shipsFound.Concat(raftsList).ToList();
                if (karvesList != null) shipsFound = shipsFound.Concat(karvesList).ToList();
                if (vikingsList != null) shipsFound = shipsFound.Concat(vikingsList).ToList();
                if (woodsList != null) portalsFound = portalsFound.Concat(woodsList).ToList();
                if (stonesList != null) portalsFound = portalsFound.Concat(stonesList).ToList();
                timeInterval -= updateTime;
            }

            foreach (int prefab in foundPrefabs.Keys.ToList()) {
                if (Main.enableCartsPins.Value && foundPrefabs.ContainsKey("Cart".GetStableHashCode())) {
                    foreach (ZDO zdo in foundPrefabs["Cart".GetStableHashCode()]) {
                        Minimap.PinData customPin;
                        bool pinWasFound = zdoPins.TryGetValue(zdo, out customPin);
                        if (!pinWasFound) {
                            string pinTitle = CheckForPinTitle(6) ? "Cart" : "";
                            customPin = map.AddPin(zdo.m_position, Minimap.PinType.Death, pinTitle, false, false);
                            customPin.m_icon = GetSprite(zdo.m_prefab, true);
                            customPin.m_doubleSize = CheckForPinSize(6);
                            zdoPins.Add(zdo, customPin);
                        }
                    }
                }
                if (Main.enableShipStatus.Value || Main.enableShipsPins.Value) {
                    if (Main.enableShipStatus.Value && shipObj != null) {
                        int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0);
                        if (shipsFound.Count > 0) {
                            List<float> distances = shipsFound.Select(p => {
                                Vector3 p3 = p.GetPosition();
                                return Vector2.Distance(playerPos, new Vector2(p3.x, p3.z));
                            }).ToList();
                            float closer = distances.Min();
                            ZDO closerShip = shipsFound[distances.IndexOf(closer)];
                            string shipName = pinsObjs.Where(p => p.Value == closerShip.GetPrefab()).FirstOrDefault().Key;
                            Vector3 shipPos3 = closerShip.GetPosition();
                            Vector2 shipPos = new(shipPos3.x, shipPos3.z);
                            shipObj.transform.Find("Name").GetComponent<Text>().text = shipName;
                            SetElementStatus(shipObj, playerPos, shipPos, cameraTransform, space);
                        } else shipObj.SetActive(false);
                    }
                    if (Main.enableShipsPins.Value) {
                        foreach (ZDO zdo in shipsFound) {
                            Minimap.PinData customPin;
                            bool pinWasFound = zdoPins.TryGetValue(zdo, out customPin);
                            if (!pinWasFound) {
                                string shipName = pinsObjs.Where(p => p.Value == zdo.GetPrefab()).FirstOrDefault().Key;
                                string pinTitle = CheckForPinTitle(5) ? shipName : "";
                                customPin = map.AddPin(zdo.m_position, Minimap.PinType.Death, pinTitle, false, false);
                                customPin.m_icon = GetSprite(zdo.GetPrefab(), true);
                                customPin.m_doubleSize = CheckForPinSize(5);
                                zdoPins.Add(zdo, customPin);
                            }
                        }
                    }
                }
                if (Main.enablePortalStatus.Value || Main.enablePortalsPins.Value) {
                    if (Main.enablePortalStatus.Value && portalObj != null) {
                        int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0) + (shipObj != null ? (shipObj.activeSelf ? 1 : 0) : 0);
                        if (portalsFound.Count > 0) {
                            List<float> distances = portalsFound.Select(p => {
                                Vector3 p3 = p.GetPosition();
                                return Vector2.Distance(playerPos, new Vector2(p3.x, p3.z));
                            }).ToList();
                            float closer = distances.Min();
                            Vector3 portalPos3 = portalsFound[distances.IndexOf(closer)].GetPosition();
                            Vector2 portalPos = new(portalPos3.x, portalPos3.z);
                            SetElementStatus(portalObj, playerPos, portalPos, cameraTransform, space);
                        } else portalObj.SetActive(false);
                    }
                    if (Main.enablePortalsPins.Value) {
                        foreach (ZDO zdo in portalsFound) {
                            Minimap.PinData customPin;
                            bool pinWasFound = zdoPins.TryGetValue(zdo, out customPin);
                            if (!pinWasFound) {
                                string portalTag = CheckForPinTitle(4) ? zdo.GetString("tag", "") : "";
                                customPin = map.AddPin(zdo.m_position, Minimap.PinType.Death, portalTag, false, false);
                                customPin.m_icon = GetSprite(zdo.m_prefab, true);
                                customPin.m_doubleSize = CheckForPinSize(4);
                                zdoPins.Add(zdo, customPin);
                            }
                        }
                    }
                }
            }*/

            if (Main.enableBedStatus.Value && bedObj != null) {
                PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                if (playerProfile.HaveCustomSpawnPoint()) {
                    Vector3 spawnPos3 = playerProfile.GetCustomSpawnPoint();
                    Vector2 spawnPos = new(spawnPos3.x, spawnPos3.z);
                    SetElementStatus(bedObj, playerPos, spawnPos, cameraTransform, totEffects);
                } else bedObj.SetActive(false);
            }
            if (Main.enableMapStats.Value && mapBlock != null) {
                RaycastHit hit;
                Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit);
                mapBlock.SetText(String.Format(
                    Main.worldCoordinatesFormat.Value,
                    $"{playerPos3.x:0.#}", $"{playerPos3.y:0.#}", $"{playerPos3.z:0.#}",
                    $"{hit.point.x:0.#}", $"{hit.point.y:0.#}", $"{hit.point.z:0.#}"
                ));
            }
            if (Main.showCursorCoordinatesInMap.Value && cursorObj != null && RectTransformUtility.RectangleContainsScreenPoint(mapRect, mousePos)) {
                Vector3 cursor = map.ScreenToWorldPoint(mousePos);
                cursorObj.GetComponent<Text>().text = String.Format(
                    Main.mapCoordinatesFormat.Value,
                    $"{cursor.x:0.#}", $"{cursor.z:0.#}"
                );
            }
            if (Main.showExploredPercentage.Value && exploredObj != null) {
                float exploredYouPercentage = map.m_explored.Where(b => b).Count() * 100f / map.m_explored.Length;
                //float exploredOthersPercentage = map.m_exploredOthers.Where(b => b).Count() * 100f / map.m_exploredOthers.Length;
                exploredObj.GetComponent<Text>().text = $"Explored : {exploredYouPercentage:0.##} %";
            }
            if (Main.enableRotatingMinimap.Value) {
                map.m_mapImageSmall.transform.rotation = Quaternion.Euler(0f, 0f, cameraAngles.y);
                map.m_pinRootSmall.transform.rotation = Quaternion.Euler(0f, 0f, cameraAngles.y);
                for (int i = 0; i < map.m_pinRootSmall.childCount; i++)
                    map.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
                if (map.m_mode == Minimap.MapMode.Small) {
                    map.m_smallMarker.rotation = Quaternion.identity;
                    Vector3 windAngles = Quaternion.LookRotation(EnvMan.instance.GetWindDir()).eulerAngles;
                    map.m_windMarker.rotation = Quaternion.Euler(0f, 0f, cameraAngles.y - windAngles.y);
                    Ship controlledShip = localPlayer.GetControlledShip();
                    if (controlledShip) {
                        map.m_smallShipMarker.gameObject.SetActive(true);
                        map.m_smallShipMarker.transform.rotation = Quaternion.Euler(0f, 0f, controlledShip.GetShipYawAngle());
                    } else map.m_smallShipMarker.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Awake")]
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
                bmp.LoadImage(byteArray);
                Sprite spt = Sprite.Create(bmp, new Rect(0, 0, __instance.m_textureSize, __instance.m_textureSize), Vector2.zero);
                spt.name = "CircleSprite";

                GameObject newSmall = new("MaskedMap");
                newSmall.transform.SetParent(__instance.m_smallRoot.transform);
                RectTransform newRect = newSmall.AddComponent<RectTransform>();
                RectTransform oldRect = __instance.m_mapSmall.GetComponent<RectTransform>();
                newRect.anchorMin = oldRect.anchorMin;
                newRect.anchorMax = oldRect.anchorMax;
                newRect.pivot = oldRect.pivot;
                newRect.anchoredPosition = oldRect.anchoredPosition;
                newRect.position = oldRect.position;
                newRect.sizeDelta = oldRect.sizeDelta;
                __instance.m_mapSmall.transform.SetParent(newSmall.transform);
                __instance.m_mapSmall.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                UnityEngine.Object.Destroy(__instance.m_mapSmall.GetComponent<RectMask2D>());
                __instance.m_mapImageSmall.maskable = true;
                newSmall.AddComponent<Image>().sprite = spt;
                newSmall.AddComponent<Mask>().showMaskGraphic = false;
                newSmall.SetActive(true);

                //__instance.m_mapSmall.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                //__instance.m_mapImageSmall.maskable = true;
                //__instance.m_smallRoot.GetComponent<Image>().sprite = spt;
                //__instance.m_smallRoot.GetComponent<Image>().preserveAspect = true;
                //__instance.m_smallRoot.AddComponent<Mask>().showMaskGraphic = false;
            }*/
            if (Main.enableRotatingMinimap.Value) {
                __instance.m_pinRootSmall.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_smallMarker.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_smallShipMarker.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_windMarker.transform.SetParent(__instance.m_smallRoot.transform);
            }
            float newSmallSize = __instance.m_smallMarker.sizeDelta.x * Main.playerMarkerScale.Value;
            float newLargeSize = __instance.m_largeMarker.sizeDelta.x * Main.playerMarkerScale.Value;
            __instance.m_smallMarker.sizeDelta = new(newSmallSize, newSmallSize);
            __instance.m_largeMarker.sizeDelta = new(newLargeSize, newLargeSize);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Location), "Awake")]
        static void ShowDungeonsPins(Location __instance) {
            Location location = __instance.GetComponent<Location>();
            Minimap map = Minimap.instance;
            if (location && map) {
                Vector3 locPos = location.transform.position;
                string locName = location.name.ToLower();
                if (!locPins.ContainsKey(locPos) && pinsObjs.Any(p => locName.Contains(p.Key.ToLower()))) {
                    KeyValuePair<string, int> pair = pinsObjs.Where(p => locName.Contains(p.Key.ToLower())).FirstOrDefault();
                    string pinTitle = CheckForPinTitle(pair.Key.Contains("Crypt") ? 2 : 1) ? pair.Key : "";
                    Minimap.PinData customPin = map.AddPin(locPos, Minimap.PinType.Death, pinTitle, false, false);
                    customPin.m_icon = GetSprite(pair.Value, false);
                    customPin.m_doubleSize = CheckForPinSize(pair.Key.Contains("Crypt") ? 2 : 1);
                    locPins.Add(locPos, customPin);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNetScene), "AddInstance")]
        static void PatchCreateZDO(ZNetScene __instance, ZDO zdo) {
            if ((CheckForPin(0) || Main.enablePortalStatus.Value || Main.enableShipStatus.Value) && foundPrefabs.Count == 0) {
                if (!CheckForPin(6)) pinsObjs.Remove("Cart");
                if (!CheckForPin(5) && !Main.enableShipStatus.Value) {
                    pinsObjs.Remove("Raft");
                    pinsObjs.Remove("Karve");
                    pinsObjs.Remove("Viking Ship");
                }
                if (!CheckForPin(4) && !Main.enablePortalStatus.Value) {
                    pinsObjs.Remove("Wood Portal");
                    pinsObjs.Remove("Stone Portal");
                }
                foreach (ZDO zdoByID in ZDOMan.instance.m_objectsByID.Values.ToList()) {
                    if (pinsObjs.Any(p => p.Value == zdoByID.GetPrefab())) {
                        if (foundPrefabs.ContainsKey(zdoByID.GetPrefab())) foundPrefabs[zdoByID.GetPrefab()].Add(zdoByID);
                        else foundPrefabs.Add(zdoByID.GetPrefab(), new List<ZDO>(){ zdoByID });
                    }
                }
            }
            if (zdo.IsValid() && pinsObjs.Any(p => zdo.GetPrefab() == p.Value) &&
                CheckForPin(0) || Main.enablePortalStatus.Value || Main.enableShipStatus.Value) {
                //Debug.Log($"ZDO aggiunto a foundPrefabs !");
                if (foundPrefabs.ContainsKey(zdo.GetPrefab())) foundPrefabs[zdo.GetPrefab()].Add(zdo);
                else foundPrefabs.Add(zdo.GetPrefab(), new List<ZDO>(){ zdo });
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZNetScene), "Destroy")]
        static bool PatchDestroyZDO(ZNetScene __instance, GameObject go) {
            if (CheckForPin(0) || !Main.enablePortalStatus.Value || !Main.enableShipStatus.Value) return true;
            Minimap map = Minimap.instance;
            ZNetView component = go.GetComponent<ZNetView>();
            if (component && component.GetZDO() != null) {
                ZDO zdo = component.GetZDO();
                if (map && pinsObjs.Any(p => zdo.GetPrefab() == p.Value)) {
                    Minimap.PinData customPin;
                    List<ZDO> zdoList;
                    if (zdoPins.TryGetValue(zdo, out customPin)) {
                        //Debug.Log($"ZDO rimosso da zdoPins !");
                        map.RemovePin(customPin);
                        zdoPins.Remove(zdo);
                    }
                    if (foundPrefabs.TryGetValue(zdo.GetPrefab(), out zdoList)) {
                        if (zdoList.Contains(zdo)) {
                            //Debug.Log($"ZDO rimosso da foundPrefabs !");
                            foundPrefabs[zdo.GetPrefab()].Remove(zdo);
                        }
                    }
                }
                component.ResetZDO();
                __instance.m_instances.Remove(zdo);
                if (zdo.IsOwner()) ZDOMan.instance.DestroyZDO(zdo);
            }
            UnityEngine.Object.Destroy(go);
            return false;
        }

        private static void SetElementStatus(GameObject element, Vector2 playerPos, Vector2 targetPos, Transform camera, int space) {
            float distance = Vector2.Distance(playerPos, targetPos);
            string pointDistance = distance < 1000f ? $"{distance:0.#} m" : $"{(distance / 1000f):0.#} km";
            Vector2 cameraForward = new(camera.forward.x, camera.forward.z);
            Vector2 cameraRight = new(camera.right.x, camera.right.z);
            float forwardAngle = Vector2.Angle(targetPos - playerPos, cameraForward);
            float rightAngle = Vector2.Angle(targetPos - playerPos, cameraRight);
            Quaternion objRotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
            element.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - space * Hud.instance.m_statusEffectSpacing, 0f);
            element.transform.Find("Icon").rotation = objRotation;
            element.transform.Find("TimeText").GetComponent<Text>().text = pointDistance;
            element.SetActive(true);
        }
        
        private static Sprite GetSprite(int nameHash, bool isPiece) {
            if (isPiece) {
                GameObject hammerObj = ObjectDB.instance.m_itemByHash["Hammer".GetStableHashCode()];
                if (!hammerObj) return null;
                ItemDrop hammerDrop = hammerObj.GetComponent<ItemDrop>();
                if (!hammerDrop) return null;
                PieceTable hammerPieceTable = hammerDrop.m_itemData.m_shared.m_buildPieces;
                foreach (GameObject piece in hammerPieceTable.m_pieces) {
                    Piece p = piece.GetComponent<Piece>();
                    if (p.name.GetStableHashCode() == nameHash) return p.m_icon;
                }
            } else {
                GameObject itemObj;
                ObjectDB.instance.m_itemByHash.TryGetValue(nameHash, out itemObj);
                if (!itemObj) return null;
                ItemDrop itemDrop = itemObj.GetComponent<ItemDrop>();
                if (!itemDrop) return null;
                return itemDrop.m_itemData.GetIcon();
            }
            return null;
        }

        private static bool CheckForPin(int num) {
            string numString = Convert.ToString(num);
            string[] values = Regex.Replace(Main.showCustomPins.Value, @"\s+", "").Split(',');
            if (values.Contains("0")) return false;
            else return values.Contains(numString);
        }

        private static bool CheckForPinTitle(int num) {
            string numString = Convert.ToString(num);
            string[] values = Regex.Replace(Main.showPinsTitles.Value, @"\s+", "").Split(',');
            if (values.Contains("0")) return false;
            else return values.Contains(numString);
        }

        private static bool CheckForPinSize(int num) {
            string numString = Convert.ToString(num);
            string[] values = Regex.Replace(Main.biggerPins.Value, @"\s+", "").Split(',');
            if (values.Contains("0")) return false;
            else return values.Contains(numString);
        }
    }
}
