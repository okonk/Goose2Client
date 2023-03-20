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
        private Dictionary<int, Sprite[]> sheetCache = new Dictionary<int, Sprite[]>();

        public void Load(string mapFile, string mapName)
        {
            Debug.Log($"Loading {mapFile} {mapName}");

            var label = gameObject.GetComponent<TMPro.TMP_Text>();
            label.text = $"Loading {mapName}...";

            StartCoroutine(LoadMapAsync(mapFile));
        }

        private IEnumerator LoadMapAsync(string mapFile)
        {
            var map = GetMap($"Maps/{mapFile.Replace(".map", "")}");

            var currentScene = SceneManager.GetActiveScene();
            var asyncLoad = SceneManager.LoadSceneAsync("MapScene", LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            var mapScene = SceneManager.GetSceneByName("MapScene");
            var eventSystem = FindObjectOfType<EventSystem>();
            SceneManager.MoveGameObjectToScene(eventSystem.gameObject, mapScene);

            var mapObj = ImportMap(map);
            SceneManager.MoveGameObjectToScene(mapObj, mapScene);
            SceneManager.UnloadSceneAsync(currentScene);

            var mapManager = FindObjectOfType<MapManager>();
            mapManager.OnMapLoaded(mapObj);

            Debug.Log("done loading map");

            GameManager.Instance.NetworkClient.DoneLoadingMap();

            GameManager.Instance.NetworkClient.Pause = false;
        }

        private MapFile GetMap(string path)
        {
            var mapFile = Resources.Load<TextAsset>(path);

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
            sheetCache.Clear();

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

            if (!sheetCache.TryGetValue(sheetNumber, out Sprite[] sprites))
            {
                sprites = Resources.LoadAll<Sprite>($"Spritesheets/{sheetNumber}");
                sheetCache[sheetNumber] = sprites;
            }

            var name = graphicId.ToString();
            var sprite = sprites.FirstOrDefault(s => s.name == name);
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;

            tileCache[graphicId] = tile;
            return tile;
        }
    }
}