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
        private static GameObject cursorObj = null;

        public static Block Start() {
            if (Main.enableMapStats.Value) {
                mapBlock = new Block(
                    "MapStats",
                    Main.mapStatsColor.Value,
                    Main.mapStatsSize.Value,
                    Main.mapStatsPosition.Value,
                    Main.mapStatsMargin.Value/*,
                    Main.mapStatsAlign.Value*/
                );
            }
            if (Main.showCursorCoordinates.Value) {
                GameObject original = Minimap.instance.m_biomeNameLarge.gameObject;
                cursorObj = UnityEngine.Object.Instantiate(original, original.transform);
                cursorObj.name = "CursorCoordinates";
                cursorObj.transform.SetParent(original.transform.parent);
                cursorObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                RectTransform cursorRect = cursorObj.GetComponent<RectTransform>();
                cursorRect.anchorMin = cursorRect.anchorMax = cursorRect.pivot = new Vector2(0f, 0f);
                cursorRect.anchoredPosition = new Vector2(15f, 5f);
                cursorObj.SetActive(true);
            }
            /*if (Main.showCustomMinimap.Value) {
                RawImage original =  Minimap.instance.m_mapImageSmall;
                GameObject mapSmall = new GameObject("SmallMapMask");
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                string name = Enumerable.Single<string>(executingAssembly.GetManifestResourceNames(), (string str) => str.EndsWith("CircleMask.png"));
                Stream stream = executingAssembly.GetManifestResourceStream(name);
                byte[] byteArray;
                using (Stream resFilestream = executingAssembly.GetManifestResourceStream(name)) {
                    byteArray = new byte[resFilestream.Length];
                    resFilestream.Read(byteArray, 0, byteArray.Length);
                }
                Texture2D bmp = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
                bmp.name = "CircleTexture";
                bmp.LoadImage(byteArray);
                Sprite newSprite = Sprite.Create(bmp, new Rect(0, 0, Minimap.instance.m_textureSize, Minimap.instance.m_textureSize), Vector2.zero);
                newSprite.name = "CircleSprite";
                mapSmall.AddComponent<Image>().sprite = newSprite;
                Mask mask = original.gameObject.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                RectTransform prt = mapSmall.AddComponent<RectTransform>(); 
                prt.localScale = Vector3.one;
                prt.anchoredPosition = Vector2.zero;
                prt.sizeDelta = new Vector2(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize);
                mapSmall.transform.SetParent(original.transform.parent);
                original.transform.SetParent(mapSmall.transform);
                mapSmall.SetActive(true);
            }*/
            return mapBlock;
        }

        public static void Update() {
            Minimap map = Minimap.instance;
            Player localPlayer = Player.m_localPlayer;
            Vector2 mousePos = Input.mousePosition;
            RectTransform mapRect = map.m_largeRoot.GetComponent<RectTransform>();
            if (Main.showPlayerCoordinates.Value) {
                if (localPlayer == null || mapBlock == null) return;
                Vector3 player = localPlayer.transform.position;
                mapBlock.SetText(String.Format(
                    Main.playerCoordinatesStringFormat.Value,
                    $"{player.x:0.#}", $"{player.y:0.#}", $"{player.z:0.#}"
                ));
            }
            if (Main.showCursorCoordinates.Value && RectTransformUtility.RectangleContainsScreenPoint(mapRect, mousePos)) {
                if (cursorObj == null) return;
                Vector3 cursor = map.ScreenToWorldPoint(mousePos);
                cursorObj.GetComponent<Text>().text = String.Format(
                    Main.cursorCoordinatesStringFormat.Value,
                    $"{cursor.x:0.#}", $"{cursor.z:0.#}"
                );
            }
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Update")]
        public static void UpdateCustomMinimap(Minimap __instance) {
            if (!Main.showCustomMinimap.Value) return;
            Quaternion cameraRotation = Utils.GetMainCamera().transform.rotation;
            __instance.m_mapSmall.transform.rotation = Quaternion.Euler(0, 0, cameraRotation.eulerAngles.y);
            __instance.m_smallMarker.transform.rotation = Quaternion.Euler(0, 0, 0);
            Quaternion shipRotation = __instance.m_smallShipMarker.rotation;
            __instance.m_smallShipMarker.rotation = Quaternion.Euler(0, 0, cameraRotation.eulerAngles.y) * shipRotation;
            Quaternion windRotation = __instance.m_windMarker.rotation;
            __instance.m_windMarker.rotation = Quaternion.Euler(0, 0, cameraRotation.eulerAngles.y) * windRotation;
        }*/
    }
}
