using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Goose2Client.Assets.Scripts.Editor
{
    public class ToolsMenu
    {
        [MenuItem("Tools/Generate TemplateAnimations")]
        public static void GenerateTemplateAnimations()
        {
            var animations = new[]
            {
                "Animation-1-AttackNoEquip-Left",
                "Animation-1-AttackNoEquip-Down",
                "Animation-1-AttackNoEquip-Right",
                "Animation-1-AttackNoEquip-Up",
                "Animation-1-Attack2Hand-Left",
                "Animation-1-Attack2Hand-Down",
                "Animation-1-Attack2Hand-Right",
                "Animation-1-Attack2Hand-Up",
            };

            foreach (var animation in animations)
            {
                AssetDatabase.CopyAsset($"Assets/Resources/Animations/Blank.anim", $"Assets/Resources/Animations/{animation}.anim");
            }
        }

        [MenuItem("Tools/Generate AnimationToFrame")]
        public static void GenerateAnimationToFrame()
        {
            var illutiaDir = "/home/hayden/code/illutiadata";
            var sourceDataDir = $"{illutiaDir}/data";

            var compiledEnc = new CompiledEnc($"{sourceDataDir}/compiled.enc");
            adfs = LoadAdfs(sourceDataDir);

            ImportAnimationToFrame(compiledEnc, adfs);
        }

        private static void ImportAnimationToFrame(CompiledEnc compiledEnc, Dictionary<int, AdfFile> adfs)
        {
            var animationToFrame = new Dictionary<string, AnimationFrame>();

            foreach (var compiledAnimation in compiledEnc.CompiledAnimations)
            {
                int animationNumber = (int)AnimationOrder.WalkingNoEquip;

                var sheetNumber = compiledAnimation.AnimationFiles[animationNumber];
                if (sheetNumber == 0) continue;

                if (!adfs.TryGetValue(sheetNumber, out var adf)) continue;

                int direction = (int)AnimationDirection.Down;

                var animationId = compiledAnimation.AnimationIndexes[direction * 11 + animationNumber];
                if (animationId == 0) continue;

                List<Frame> animationFrames;
                if (adf.Animations == null || !adf.Animations.TryGetValue(animationId, out var animationDefinition))
                    animationFrames = new List<Frame> { adf.Frames[direction] };
                else
                    animationFrames = animationDefinition.Frames;

                var name = $"{compiledAnimation.Type}-{compiledAnimation.Id}";
                var frame = animationFrames.First();

                animationToFrame[name] = new AnimationFrame(sheetNumber, frame.Index, frame.W, frame.H);
            }

            File.WriteAllLines($"Assets/Resources/AnimationToFirstFrame.txt", animationToFrame.Select(a => $"{a.Key},{a.Value.FileId},{a.Value.GraphicId},{a.Value.Width},{a.Value.Height}"));
        }


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

        public const string tempSpritesheetsDir = "Assets/TempSpritesheets";
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
                // .Where(a => a.Value.Type == AdfType.Graphic)
                // .Where(a => a.Value.FileNumber < 200)
                // .ToDictionary(k => k.Key, v => v.Value);

            ImportSpritesheets(adfs.Values.Where(a => a.Type == AdfType.Graphic));
            ImportAnimations(compiledEnc, adfs);
            CopyMaps(sourceMapsDir);

            BuildPipeline.BuildAssetBundles(streamingAssetsDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);
        }

        private static void CopyMaps(string sourceMapsDir)
        {
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var file in Directory.EnumerateFiles(sourceMapsDir, "*.map"))
                {
                    var outputPath = $"{mapsDir}/M{Path.GetFileNameWithoutExtension(file).Substring(1)}.bytes";
                    File.Copy(file, outputPath, overwrite: true);

                    AssetDatabase.ImportAsset(outputPath);
                    LabelAsset(outputPath, "maps");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
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

        private static void ImportSpritesheets(IEnumerable<AdfFile> adfs)
        {
            Debug.Log("Starting importing spritesheets");

            foreach (var adf in adfs)
            {
                var path = ConvertToPng(adf);

                AssetDatabase.ImportAsset(path);
            }

            AssetDatabase.StartAssetEditing();

            try
            {
                // Something is creating the Spritesheets dir, so copying files individual instead
                foreach (var sheetPath in Directory.EnumerateFiles(tempSpritesheetsDir, "*.png"))
                {
                    AssetDatabase.MoveAsset(sheetPath, $"{spritesheetsDir}/{Path.GetFileName(sheetPath)}");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log("Finished importing spritesheets");
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

                var path = $"{tempSpritesheetsDir}/{adf.FileNumber}.png";
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

        private static Dictionary<string, Sprite> spriteCache = new();

        private static Sprite GetSprite(int fileId, int graphicId)
        {
            var key = $"{fileId}-{graphicId}";

            if (spriteCache.TryGetValue(key, out var sprite))
                return sprite;

            var sprites = AssetDatabase.LoadAllAssetsAtPath($"{spritesheetsDir}/{fileId}.png").OfType<Sprite>();
            foreach (var s in sprites)
            {
                spriteCache[s.name] = s;
            }

            spriteCache.TryGetValue(key, out sprite);
            return sprite;
        }

        private static void ImportAnimations(CompiledEnc compiledEnc, Dictionary<int, AdfFile> adfs)
        {
            var animationToFrame = new Dictionary<string, AnimationFrame>();
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

                        var frames = frameDefinitions.Select(frame => GetSprite(sheetNumber, frame.Index)).ToArray();

                        var animationName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (AnimationDirection)direction);

                        var unityAnimation = CreateAnimation(frames);
                        var animationPath = $"{animationsDir}/{animationName}.anim";
                        AssetDatabase.CreateAsset(unityAnimation, animationPath);

                        LabelAsset(animationPath, assetBundleName);

                        if (animationNumber == (int)AnimationOrder.WalkingNoEquip && direction == (int)AnimationDirection.Down)
                        {
                            var firstFrame = frameDefinitions.First();
                            animationToFrame[assetBundleName] = new AnimationFrame(sheetNumber, firstFrame.Index, firstFrame.W, firstFrame.H);
                        }

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

                    var frames = animation.Frames.Select(frame => GetSprite(adf.FileNumber, frame.Index)).ToArray();

                    var animationName = animation.Id.ToString();

                    var unityAnimation = CreateAnimation(frames);
                    var animationPath = $"{animationsDir}/{animationName}.anim";
                    AssetDatabase.CreateAsset(unityAnimation, animationPath);

                    var assetBundleName = $"Spell-{animation.Id}";
                    LabelAsset(animationPath, assetBundleName);

                    SetAnimationHeight(animation.Frames, animationName, animationHeights);
                }
            }

            File.WriteAllLines($"Assets/Resources/AnimationToFirstFrame.txt", animationToFrame.Select(a => $"{a.Key},{a.Value.FileId},{a.Value.GraphicId},{a.Value.Width},{a.Value.Height}"));
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
            if (!assetImporter.assetPath.StartsWith(ToolsMenu.tempSpritesheetsDir))
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