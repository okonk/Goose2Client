using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Goose2Client
{
    public class LoadingMapScene : MonoBehaviour
    {
        private Dictionary<int, Tile> tileCache = new Dictionary<int, Tile>();

        public void Load(string mapFile, string mapName)
        {
            Debug.Log($"Loading {mapFile} {mapName}");

            var label = gameObject.GetComponent<TMPro.TMP_Text>();
            label.text = $"Loading {mapName}...";

            StartCoroutine(LoadMapAsync(mapFile));
        }

        private IEnumerator LoadMapAsync(string mapFile)
        {
            var map = GetMap($"{mapFile.Replace(".map", "")}");

            var lastScene = SceneManager.GetActiveScene();

            var ui = lastScene.GetRootGameObjects().FirstOrDefault(o => o.name == "UI");

            var asyncLoad = SceneManager.LoadSceneAsync("MapScene", LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            var mapScene = SceneManager.GetSceneByName("MapScene");
            var eventSystem = FindObjectOfType<EventSystem>();
            SceneManager.MoveGameObjectToScene(eventSystem.gameObject, mapScene);

            if (ui != null)
            {
                SceneManager.MoveGameObjectToScene(ui, mapScene);
                ui.SetActive(true);
            }

            var mapObj = ImportMap(map);
            SceneManager.MoveGameObjectToScene(mapObj, mapScene);

            SceneManager.UnloadSceneAsync(lastScene);

            if (ui == null)
            {
                var uiPrefab = ResourceManager.LoadFromBundle<GameObject>("ui-prefabs", "UI");
                ui = Instantiate(uiPrefab, null);
                ui.name = "UI";
            }

            var mapManager = FindObjectOfType<MapManager>();
            mapManager.OnMapLoaded(mapObj);

            GameManager.Instance.NetworkClient.DoneLoadingMap();

            GameManager.Instance.NetworkClient.Pause = false;
        }

        private MapFile GetMap(string path)
        {
            var mapFile = ResourceManager.LoadFromBundle<TextAsset>("maps", path);

            var map = new MapFile(mapFile.bytes);
            GameManager.Instance.CurrentMap = map;

            return map;
        }

        private GameObject ImportMap(MapFile map)
        {
            var grid = new GameObject("MapGrid");
            grid.AddComponent<Grid>();

            var layers = new Tilemap[6];
            for (int i = 0; i < layers.Length; i++)
            {
                var layerName = GetLayerName(i);
                var obj = new GameObject(layerName);
                var tilemap = obj.AddComponent<Tilemap>();
                var renderer = obj.AddComponent<TilemapRenderer>();
                renderer.sortingLayerID = SortingLayer.NameToID(layerName);
                renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;

                if (i == 0)
                {
                    obj.AddComponent<TilemapCollider2D>();
                    obj.AddComponent<MapClickHandler>();
                }

                if (i == 5)
                    renderer.enabled = false;

                obj.transform.parent = grid.transform;

                layers[i] = tilemap;
            }

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map[x, y];

                    if (tile.IsBlocked)
                        layers[5].SetTile(new Vector3Int(x, map.Height - y - 1, 0), GetOrCreateTile(13, 3002));

                    for (int l = 0; l < tile.Layers.Length; l++)
                    {
                        var layer = tile.Layers[l];
                        if (layer.Graphic == 0) continue;

                        layers[l].SetTile(new Vector3Int(x, map.Height - y - 1, 0), GetOrCreateTile(layer.Sheet, layer.Graphic));
                    }
                }
            }

            tileCache.Clear();

            return grid;
        }

        private static string GetLayerName(int i)
            => i switch {
                0 => "Ground",
                1 => "Ground 2",
                2 => "Objects 1",
                3 => "Objects 2",
                4 => "Roofs",
                5 => "Blocked",
                _ => throw new ArgumentException(nameof(i))
            };

        private Tile GetOrCreateTile(int sheetNumber, int graphicId)
        {
            if (tileCache.TryGetValue(graphicId, out Tile tile))
                return tile;

            var sprite = Helpers.GetSprite(graphicId, sheetNumber);
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;

            tileCache[graphicId] = tile;
            return tile;
        }
    }
}