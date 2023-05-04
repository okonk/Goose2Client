using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Goose2Client.Assets.Scripts.Editor
{
    public class ToolsMenu
    {
        [MenuItem("Tools/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            string assetBundleDirectory = "Assets/StreamingAssets";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);
        }

        [MenuItem("Tools/Generate Data")]
        public static void GenerateData()
        {
            //var illutiaDir = EditorUtility.OpenFolderPanel("Choose Original Illutia Client Folder", "", "");
            var illutiaDir = "/home/hayden/code/illutiadata";
            var dataDir = $"{illutiaDir}/data";
            var mapDir = $"{illutiaDir}/maps";

            var spritesheetsDir = $"Assets/Test/Spritesheets";
            var animationsDir = $"Assets/Test/Animations";

            var compiledEnc = new CompiledEnc($"{dataDir}/compiled.enc");
            var adfs = LoadAdfs(dataDir);

            foreach (var adf in adfs.Values.Where(a => a.Type == AdfType.Graphic).Take(2))
            {
                ImportSpritesheet(spritesheetsDir, adf);
            }
        }

        private static Dictionary<int, AdfFile> LoadAdfs(string dataDir)
        {
            var adfs = new Dictionary<int, AdfFile>();

            foreach (var file in Directory.EnumerateFiles(dataDir, "*.adf"))
            {
                var adf = new AdfFile(file);
                adfs[adf.FileNumber] = adf;
            }

            return adfs;
        }

        private static void ConvertToPng(string spritesheetsDir, AdfFile adf)
        {
            try
            {
                var texData = GifLoader.Load(adf.FileData, out int width, out int height);
                // not sure why, but after saving the png the pixels are upside down, so flip them
                var flippedTexData = new byte[texData.Length];
                for (int i = 0; i < height; i++)
                {
                    Array.Copy(texData, i * width * 4, flippedTexData, (height - i - 1) * width * 4, width * 4);
                }

                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(flippedTexData);
                texture.Apply();

                File.WriteAllBytes($"{spritesheetsDir}/{adf.FileNumber}.png", texture.EncodeToPNG());

                UnityEngine.Object.DestroyImmediate(texture);
            }
            catch (Exception e)
            {
                Debug.Log($"Problem converting {adf.FileNumber}.adf: {e}");
            }
        }

        private static void ImportSpritesheet(string spritesheetsDir, AdfFile adf)
        {
            ConvertToPng(spritesheetsDir, adf);

            var asset = (TextureImporter)AssetImporter.GetAtPath($"{spritesheetsDir}/{adf.FileNumber}.png");

            asset.spritePixelsPerUnit = 32;
            asset.filterMode = FilterMode.Point;
            asset.textureCompression = TextureImporterCompression.Uncompressed;

            if (adf.FileNumber >= 14 && adf.FileNumber <= 52)
            {
                asset.spriteImportMode = SpriteImportMode.Single;
                asset.SaveAndReimport();
                return;
            }

            asset.spriteImportMode = SpriteImportMode.Multiple;

            asset.GetSourceTextureWidthAndHeight(out int totalWidth, out int totalHeight);

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(asset);
            dataProvider.InitSpriteEditorDataProvider();

            var spriteRects = new List<SpriteRect>();

            for (int i = 0; i < adf.Frames.Count; i++)
            {
                var frame = adf.Frames[i];

                var newSprite = new SpriteRect()
                {
                    name = $"{adf.FileNumber}-{frame.Index}",
                    spriteID = GUID.Generate(),
                    rect = new Rect(frame.X, totalHeight - frame.Y - frame.H, frame.W, frame.H),
                    alignment = SpriteAlignment.BottomCenter,
                };

                spriteRects.Add(newSprite);
            }

            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();
            asset.SaveAndReimport();
        }
    }
}