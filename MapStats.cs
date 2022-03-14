using UnityEngine;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

namespace AlweStats {
    [HarmonyPatch]
    public static class MapStats {
        private static Block mapBlock = null;
        private static GameObject cursorObj = null, exploredObj = null;

        public static Block Start() {
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
                GameObject original = Minimap.instance.m_biomeNameLarge.gameObject;
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
                GameObject original = Minimap.instance.m_biomeNameLarge.gameObject;
                exploredObj = UnityEngine.Object.Instantiate(original, original.transform);
                exploredObj.name = "ExploredPercentage";
                exploredObj.transform.SetParent(original.transform.parent);
                exploredObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                exploredObj.GetComponent<Text>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform exploredRect = exploredObj.GetComponent<RectTransform>();
                exploredRect.anchorMin = exploredRect.anchorMax = exploredRect.pivot = new Vector2(0f, 1f);
                exploredRect.anchoredPosition = new Vector2(15f, -5f);
                exploredObj.SetActive(true);
            }
            if (Main.enableRotatingMinimap.Value) {
                Minimap map = Minimap.instance;
                map.m_pinRootSmall.transform.SetParent(map.m_mapImageSmall.transform.parent);
                map.m_smallMarker.transform.SetParent(map.m_mapImageSmall.transform.parent);
                map.m_smallShipMarker.transform.SetParent(map.m_mapImageSmall.transform.parent);
                map.m_windMarker.transform.SetParent(map.m_mapImageSmall.transform.parent);
            }
            /*if (Main.showCustomMinimap.Value) {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string name = Enumerable.Single<string>(assembly.GetManifestResourceNames(), (string str) => str.EndsWith("CircleMask.png"));
                Stream stream = assembly.GetManifestResourceStream(name);
                byte[] byteArray;
                using (Stream resFilestream = assembly.GetManifestResourceStream(name)) {
                    byteArray = new byte[resFilestream.Length];
                    resFilestream.Read(byteArray, 0, byteArray.Length);
                }
                int textureSize = Minimap.instance.m_textureSize;
                Texture2D bmp = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
                bmp.name = "CircleTexture";
                bmp.LoadImage(byteArray);
                Sprite spt = Sprite.Create(bmp, new Rect(0, 0, textureSize, textureSize), Vector2.zero);
                spt.name = "CircleSprite";

                GameObject original = Minimap.instance.m_smallRoot;
                Minimap.instance.m_mapSmall.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                Minimap.instance.m_mapImageSmall.maskable = true;
                original.GetComponent<Image>().sprite = spt;
                original.AddComponent<Mask>();
            }*/
            return mapBlock;
        }

        public static void Update() {
            Minimap map = Minimap.instance;
            Player localPlayer = Player.m_localPlayer;
            Vector2 mousePos = Input.mousePosition;
            RectTransform mapRect = map.m_largeRoot.GetComponent<RectTransform>();
            if (Main.enableMapStats.Value && localPlayer != null && mapBlock != null) {
                Vector3 player = localPlayer.transform.position;
                RaycastHit hit;
                Physics.Raycast(Utils.GetMainCamera().transform.position, Utils.GetMainCamera().transform.forward, out hit);
                mapBlock.SetText(String.Format(
                    Main.worldCoordinatesFormat.Value,
                    $"{player.x:0.#}", $"{player.y:0.#}", $"{player.z:0.#}",
                    $"{hit.point.x:0.#}", $"{hit.point.y:0.#}", $"{hit.point.z:0.#}"
                ));
            }
            if (Main.showCursorCoordinatesInMap.Value && cursorObj != null &&
                RectTransformUtility.RectangleContainsScreenPoint(mapRect, mousePos)) {
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
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Update")]
        private static void RotateMinimap(Minimap __instance) {
            if (!Main.enableRotatingMinimap.Value) return;
            Player player = Player.m_localPlayer;
            if (player == null) return;
            Vector3 playerAngles = player.m_eye.transform.rotation.eulerAngles;
            __instance.m_mapImageSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
            __instance.m_pinRootSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
            for (int i = 0; i < __instance.m_pinRootSmall.childCount; i++)
                __instance.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
            if (__instance.m_mode == Minimap.MapMode.Small) {
				__instance.m_smallMarker.rotation = Quaternion.identity;
                Vector3 windAngles = Quaternion.LookRotation(EnvMan.instance.GetWindDir()).eulerAngles;
                __instance.m_windMarker.rotation = Quaternion.Euler(0f, 0f, playerAngles.y - windAngles.y);
				Ship controlledShip = player.GetControlledShip();
				if (controlledShip) {
					__instance.m_smallShipMarker.gameObject.SetActive(true);
					__instance.m_smallShipMarker.transform.rotation = Quaternion.Euler(0f, 0f, controlledShip.GetShipYawAngle());
				} else __instance.m_smallShipMarker.gameObject.SetActive(false);
			}
        }
    }
}
