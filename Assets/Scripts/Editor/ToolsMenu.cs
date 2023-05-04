using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;

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

        public const string spritesheetsDir = "Assets/Spritesheets";
        public const string animationsDir = "Assets/Animations";
        public const string mapsDir = "Assets/Maps";
        public const string streamingAssetsDir = "Assets/StreamingAssets";

        public static Dictionary<int, AdfFile> adfs;

        [MenuItem("Tools/Generate Data")]
        public static void GenerateData()
        {
            //var illutiaDir = EditorUtility.OpenFolderPanel("Choose Original Illutia Client Folder", "", "");
            var illutiaDir = "/home/hayden/code/illutiadata";
            var sourceDataDir = $"{illutiaDir}/data";
            var sourceMapsDir = $"{illutiaDir}/maps";

            var compiledEnc = new CompiledEnc($"{sourceDataDir}/compiled.enc");
            adfs = LoadAdfs(sourceDataDir);

            var adfsToLoad = adfs.Values.Where(a => a.Type == AdfType.Graphic);

            //ConvertPngs(adfsToLoad);
            ImportSpritesheets(adfsToLoad);

            // Have to do this to be able to load the SpriteAtlas
            BuildPipeline.BuildAssetBundles(streamingAssetsDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);

            ImportAnimations(compiledEnc, adfs);

            // CopyMaps(sourceMapsDir, mapsDir);

            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(streamingAssetsDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);
        }

        private static void CopyMaps(string sourceMapsDir)
        {
            foreach (var file in Directory.EnumerateFiles(sourceMapsDir, "*.map"))
            {
                File.Copy(file, $"{mapsDir}/M{Path.GetFileNameWithoutExtension(file).Substring(1)}.bytes", overwrite: true);
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

        private static void ConvertPngs(IEnumerable<AdfFile> adfs)
        {
            foreach (var adf in adfs)
            {
                ConvertToPng(adf);
            }

            AssetDatabase.Refresh();
        }

        private static void ImportSpritesheets(IEnumerable<AdfFile> adfs)
        {
            foreach (var adf in adfs)
            {
                var path = ConvertToPng(adf);

                AssetDatabase.ImportAsset(path);

                //ImportSpritesheet(adf);
            }

            //AssetDatabase.Refresh();
        }

        private static string ConvertToPng(AdfFile adf)
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

                var path = $"{spritesheetsDir}/{adf.FileNumber}.png";
                File.WriteAllBytes(path, texture.EncodeToPNG());

                UnityEngine.Object.DestroyImmediate(texture);

                return path;
            }
            catch (Exception e)
            {
                Debug.Log($"Problem converting {adf.FileNumber}.adf: {e}");
                return null;
            }
        }

        private static void ImportSpritesheet(AdfFile adf)
        {
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

        private static AnimationClip CreateAnimation(Sprite[] frames, float frameRate = 8)
        {
            var clip = new AnimationClip
            {
                frameRate = frameRate
            };

            var curveBinding = new EditorCurveBinding
            {
                propertyName = "m_Sprite",
                path = "",
                type = typeof(SpriteRenderer)
            };

            int numberOfFrames = frames.Length;

            var keyFrames = new ObjectReferenceKeyframe[numberOfFrames];
            for (int i = 0; i < numberOfFrames; i++)
            {
                var keyFrame = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = frames[i]
                };
                keyFrames[i] = keyFrame;
            }
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

            var settings = new AnimationClipSettings
            {
                loopTime = true,
                keepOriginalPositionY = true,
                stopTime = keyFrames.Length / clip.frameRate
            };
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            return clip;
        }

        private static string CreateAnimationName(int id, AnimationType type, AnimationOrder animation, AnimationDirection direction)
        {
            return $"{type}-{id}-{animation}-{direction}";
        }

        private static void ImportAnimations(CompiledEnc compiledEnc, Dictionary<int, AdfFile> adfs)
        {
            var atlas = ResourceManager.LoadFromBundle<SpriteAtlas>("spriteatlas", "SpriteAtlas");

            var animationToFrame = new Dictionary<string, Frame>();
            var animationHeights = new Dictionary<string, int>();

            var animationsInCompiled = new HashSet<int>();

            foreach (var compiledAnimation in compiledEnc.CompiledAnimations)
            {
                var assetBundleName = $"{compiledAnimation.Type}-{compiledAnimation.Id}";

                for (int animationNumber = 0; animationNumber < 11; animationNumber++)
                {
                    var sheetNumber = compiledAnimation.AnimationFiles[animationNumber];
                    if (sheetNumber == 0 || !adfs.TryGetValue(sheetNumber, out var adf)) continue;

                    // var checkName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (Direction)1);
                    // if (File.Exists($"Assets/Animations/{checkName}.anim")) continue;

                    for (int direction = 0; direction < 4; direction++)
                    {
                        var animationId = compiledAnimation.AnimationIndexes[direction * 11 + animationNumber];
                        if (animationId == 0) continue;

                        animationsInCompiled.Add(animationId);

                        List<Frame> frameDefinitions;
                        if (adf.Animations == null || !adf.Animations.TryGetValue(animationId, out var animationDefinition))
                            frameDefinitions = new List<Frame> { adf.Frames[direction] };
                        else
                            frameDefinitions = animationDefinition.Frames;

                        var frames = frameDefinitions.Select(frame => atlas.GetSprite($"{sheetNumber}-{frame.Index}")).ToArray();

                        var animationName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (AnimationDirection)direction);

                        var unityAnimation = CreateAnimation(frames);
                        var animationPath = $"{animationsDir}/{animationName}.anim";
                        AssetDatabase.CreateAsset(unityAnimation, animationPath);

                        LabelAsset(animationPath, assetBundleName);

                        if (animationNumber == (int)AnimationOrder.WalkingNoEquip && direction == (int)AnimationDirection.Down)
                            animationToFrame[assetBundleName] = frameDefinitions.First();

                        if (animationNumber is (int)AnimationOrder.WalkingNoEquip or (int)AnimationOrder.WalkingEquip or (int)AnimationOrder.Mounted)
                            GenerateIdleAnimation(frames.First(), frameDefinitions.First(), (AnimationOrder)animationNumber, (AnimationDirection)direction, compiledAnimation.Id, compiledAnimation.Type, assetBundleName, animationHeights);

                        SetAnimationHeight(frameDefinitions, animationName, animationHeights);
                    }
                }
            }

            // import other animations that aren't in CompiledAnimations.. things like spells and emotes
            foreach (var adf in adfs.Values)
            {
                if (adf.Animations == null) continue;

                foreach (var animation in adf.Animations.Values)
                {
                    if (animationsInCompiled.Contains(animation.Id)) continue;

                    var frames = animation.Frames.Select(frame => atlas.GetSprite($"{adf.FileNumber}-{frame.Index}")).ToArray();

                    var animationName = animation.Id.ToString();

                    var unityAnimation = CreateAnimation(frames);
                    var animationPath = $"{animationsDir}/{animationName}.anim";
                    AssetDatabase.CreateAsset(unityAnimation, animationPath);

                    var assetBundleName = $"Spell-{animation.Id}";
                    LabelAsset(animationPath, assetBundleName);

                    SetAnimationHeight(animation.Frames, animationName, animationHeights);
                }
            }

            File.WriteAllLines($"Assets/Resources/AnimationToFirstFrame.txt", animationToFrame.Select(a => $"{a.Key},{a.Value.Index},{a.Value.W},{a.Value.H}"));
            File.WriteAllLines($"Assets/Resources/AnimationHeights.txt", animationHeights.Select(a => $"{a.Key},{a.Value}"));
        }

        private static void GenerateIdleAnimation(Sprite frame, Frame frameDefinition, AnimationOrder animationNumber, AnimationDirection direction, int id, AnimationType type, string assetBundleName, Dictionary<string, int> animationHeights)
        {
            var frames = new[] { frame };
            var frameDefinitions = new[] { frameDefinition };

            string animationTypeName = animationNumber switch
            {
                AnimationOrder.WalkingNoEquip => "IdleNoEquip",
                AnimationOrder.WalkingEquip => "IdleEquip",
                AnimationOrder.Mounted => "MountedIdle",

                _ => throw new ArgumentException($"Unexpected AnimationOrder: {animationNumber}")
            };

            var animationName = $"{type}-{id}-{animationTypeName}-{direction}";
            var unityAnimation = CreateAnimation(frames);
            var animationPath = $"{animationsDir}/{animationName}.anim";
            AssetDatabase.CreateAsset(unityAnimation, animationPath);

            LabelAsset(animationPath, assetBundleName);

            SetAnimationHeight(frameDefinitions, animationName, animationHeights);
        }

        private static void SetAnimationHeight(IReadOnlyList<Frame> frames, string animationName, Dictionary<string, int> animationHeights)
        {
            var maxHeight = frames.Max(f => f.H);
            if (maxHeight == 64) return;

            animationHeights[animationName] = maxHeight;
        }

        private static void LabelAsset(string path, string label)
        {
            AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant(label, "");
        }
    }

    public class PostProcessImportSpritesheet : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetImporter.assetPath.StartsWith(ToolsMenu.spritesheetsDir))
                return;

            var numberString = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
            if (!int.TryParse(numberString, out var adfNumber))
                throw new ArgumentException($"Unexpected spritesheet: {assetImporter.assetPath}");

            var adf = ToolsMenu.adfs[adfNumber];

            var asset = (TextureImporter)assetImporter;

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