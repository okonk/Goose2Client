using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Goose2Client
{
    public class ToolsMenu
    {
        [MenuItem("Tools/Remap Animations")]
        private static void RemapAnimations()
        {
            // RemapAnim("Right", "Right2");
            // RemapAnim("Left", "Right");
            // RemapAnim("Up", "Left");
            // RemapAnim("Right2", "Up");

            foreach (var animation in Directory.EnumerateFiles("Assets/Resources/Animations", $"*.anim"))
            {
                if (animation.Contains("Idle"))
                    continue;

                int lastDash = animation.LastIndexOf('-');
                if (lastDash == -1)
                    continue;

                var filename = Path.GetFileNameWithoutExtension(animation);
                var contents = File.ReadAllText(animation);
                var replaced = Regex.Replace(contents, @"m_Name: (.*)$", $"m_Name: {filename}", RegexOptions.Multiline);
                File.WriteAllText(animation, replaced);
            }
        }

        private static void RemapAnim(string dir, string replace)
        {
            foreach (var animation in Directory.EnumerateFiles("Assets/Resources/Animations", $"*-{dir}.anim"))
            {
                if (animation.Contains("Idle"))
                    continue;

                int lastDash = animation.LastIndexOf('-');
                if (lastDash == -1)
                    throw new Exception("!!!!!");

                File.Move(animation, animation.Substring(0, lastDash) + $"-{replace}.anim");
            }
        }

        [MenuItem("Tools/Generate Data")]
        private static void GenerateData()
        {
            //var illutiaDir = EditorUtility.OpenFolderPanel("Choose Original Illutia Client Folder", "", "");
            var illutiaDir = "/home/hayden/code/illutiadata";
            var dataDir = $"{illutiaDir}/data";
            var mapDir = $"{illutiaDir}/maps";

            var adfs = new Dictionary<int, ADFFile>();

            foreach (var file in Directory.EnumerateFiles(dataDir, "*.adf"))
            {
                //Debug.Log($"Loading {file}");

                var adf = new ADFFile(file);
                adfs[adf.FileNumber] = adf;

                // if (adf.Type == ADFType.Graphic)
                //     ConvertToPng(adf);
            }

            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();

            // foreach (var adf in adfs.Values)
            // {
            //     if (adf.Type != ADFType.Graphic) continue;

            //     ImportSpritesheet(adf);
            // }

            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();

            var compiledEnc = new CompiledEnc($"{dataDir}/compiled.enc");
            // ImportCompiledAnimations(compiledEnc, adfs);
            ImportIdleAnimations(compiledEnc, adfs);
            ImportOtherAnimations(compiledEnc, adfs);

            // foreach (var file in Directory.EnumerateFiles(mapDir, "*.map"))
            // {
            //     ImportMap(file, adfs);
            // }

            //ImportMap($"{mapDir}/Map2.map");
        }

        private static void ImportCompiledAnimations(CompiledEnc compiledEnc, Dictionary<int, ADFFile> adfs)
        {
            foreach (var compiledAnimation in compiledEnc.CompiledAnimations)
            {
                for (int animationNumber = 0; animationNumber < 11; animationNumber++)
                {

                    var sheetNumber = compiledAnimation.AnimationFiles[animationNumber];
                    if (sheetNumber == 0) continue;

                    // var checkName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (Direction)1);
                    // if (File.Exists($"Assets/Animations/{checkName}.anim")) continue;

                    var sprites = AssetDatabase.LoadAllAssetsAtPath($"Assets/Spritesheets/{sheetNumber}.png").OfType<Sprite>();
                    if (!adfs.TryGetValue(sheetNumber, out var adf)) continue;

                    for (int direction = 0; direction < 4; direction++)
                    {
                        var animationId = compiledAnimation.AnimationIndexes[direction * 11 + animationNumber];
                        if (animationId == 0 || adf.Animations == null || !adf.Animations.TryGetValue(animationId, out var animationDefinition)) continue;

                        var frames = animationDefinition.Frames.Select(frame => sprites.FirstOrDefault(s => s.name == frame.Index.ToString())).ToArray();

                        var animationName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (Direction)direction);

                        var unityAnimation = CreateAnimation(animationName, frames);
                        AssetDatabase.CreateAsset(unityAnimation, $"Assets/Animations/{animationName}.anim");

                        Debug.Log($"Created {animationName}");
                    }
                }
            }
        }

        private static void ImportIdleAnimations(CompiledEnc compiledEnc, Dictionary<int, ADFFile> adfs)
        {
            foreach (var compiledAnimation in compiledEnc.CompiledAnimations)
            {
                for (int animationNumber = 0; animationNumber < 2; animationNumber++)
                {
                    var sheetNumber = compiledAnimation.AnimationFiles[animationNumber];
                    if (sheetNumber == 0) continue;

                    // var checkName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (Direction)1);
                    // if (File.Exists($"Assets/Animations/{checkName}.anim")) continue;

                    var sprites = AssetDatabase.LoadAllAssetsAtPath($"Assets/Resources/Spritesheets/{sheetNumber}.png").OfType<Sprite>();
                    if (!adfs.TryGetValue(sheetNumber, out var adf)) continue;

                    for (int direction = 0; direction < 4; direction++)
                    {
                        var animationId = compiledAnimation.AnimationIndexes[direction * 11 + animationNumber];
                        if (animationId == 0 || adf.Animations == null || !adf.Animations.TryGetValue(animationId, out var animationDefinition)) continue;

                        var frames = animationDefinition.Frames.Select(frame => sprites.FirstOrDefault(s => s.name == frame.Index.ToString())).Take(1).ToArray();

                        var animationName = CreateAnimationName(compiledAnimation.Id, compiledAnimation.Type, (AnimationOrder)animationNumber, (Direction)direction);
                        animationName = animationName.Replace("Walking", "Idle");

                        var unityAnimation = CreateAnimation(animationName, frames);
                        AssetDatabase.CreateAsset(unityAnimation, $"Assets/Resources/Animations/{animationName}.anim");

                        Debug.Log($"Created {animationName}");
                    }
                }
            }
        }
        private static void ImportOtherAnimations(CompiledEnc compiledEnc, Dictionary<int, ADFFile> adfs)
        {
            var compiledAnimations = new HashSet<int>();

            foreach (var compiledAnimation in compiledEnc.CompiledAnimations)
            {
                for (int animationNumber = 0; animationNumber < 11; animationNumber++)
                {
                    var sheetNumber = compiledAnimation.AnimationFiles[animationNumber];
                    if (sheetNumber == 0) continue;

                    for (int direction = 0; direction < 4; direction++)
                    {
                        var animationId = compiledAnimation.AnimationIndexes[direction * 11 + animationNumber];
                        compiledAnimations.Add(animationId);
                    }
                }
            }

            foreach (var adf in adfs.Values)
            {
                if (adf.Animations == null) continue;

                foreach (var animation in adf.Animations.Values)
                {
                    if (compiledAnimations.Contains(animation.Id)) continue;

                    var sprites = AssetDatabase.LoadAllAssetsAtPath($"Assets/Resources/Spritesheets/{adf.FileNumber}.png").OfType<Sprite>();
                    var frames = animation.Frames.Select(frame => sprites.FirstOrDefault(s => s.name == frame.Index.ToString())).ToArray();

                    var animationName = animation.Id.ToString();

                    var unityAnimation = CreateAnimation(animationName, frames);
                    AssetDatabase.CreateAsset(unityAnimation, $"Assets/Resources/Animations/{animationName}.anim");

                    Debug.Log($"Created {animationName}");
                }
            }
        }

        private static string CreateAnimationName(int id, AnimationType type, AnimationOrder animation, Direction direction)
        {
            return $"{type}-{id}-{animation}-{direction}";
        }

        private static AnimationClip CreateAnimation(string name, Sprite[] frames)
        {
            var clip = new AnimationClip();
            clip.frameRate = 8f;

            var curveBinding = new EditorCurveBinding();
            curveBinding.propertyName = "m_Sprite";
            curveBinding.path = "";
            curveBinding.type = typeof(SpriteRenderer);

            var keyFrames = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < keyFrames.Length; i++)
            {
                var keyFrame = new ObjectReferenceKeyframe();
                keyFrame.time = i / clip.frameRate;
                keyFrame.value = frames[i];
                keyFrames[i] = keyFrame;
            }
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

            var settings = new AnimationClipSettings();
            settings.loopTime = true;
            settings.keepOriginalPositionY = true;
            settings.stopTime = keyFrames.Length / clip.frameRate;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            return clip;
        }

        private static void ImportMap(string path)
        {
            var map = new MapFile(path);

            var grid = new GameObject("Grid");
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

                    if (tile.IsBlocked())
                        layers[5].SetTile(new Vector3Int(x, map.Height - y - 1, 0), GetOrCreateTile(13, 3002));

                    for (int l = 0; l < tile.Layers.Length; l++)
                    {
                        var layer = tile.Layers[l];
                        if (layer.Graphic == 0) continue;

                        layers[l].SetTile(new Vector3Int(x, map.Height - y - 1, 0), GetOrCreateTile(layer.Sheet, layer.Graphic));
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(grid, $"Assets/Resources/Maps/M{Path.GetFileNameWithoutExtension(path).Substring(1)}.prefab");
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

        private static Dictionary<int, Tile> tileCache = new Dictionary<int, Tile>();
        private static Dictionary<int, Sprite[]> sheetCache = new Dictionary<int, Sprite[]>();

        private static Tile GetOrCreateTile(int sheetNumber, int graphicId)
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

        private static ADFFile GetOrLoadAdf(int fileNumber, Dictionary<int, ADFFile> adfs)
        {
            if (adfs.TryGetValue(fileNumber, out ADFFile adf))
                return adf;

            adf = new ADFFile($"/home/hayden/code/illutiadata/data/{fileNumber}.adf");
            // ConvertToPng(adf);

            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();

            // ImportSpritesheet(adf);

            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();

            adfs[fileNumber] = adf;

            return adf;
        }

        private static void ImportSpritesheet(ADFFile adf)
        {
            var asset = (TextureImporter)TextureImporter.GetAtPath($"Assets/Spritesheets/{adf.FileNumber}.png");

            asset.spritePixelsPerUnit = 32;
            asset.filterMode = FilterMode.Point;
            asset.textureCompression = TextureImporterCompression.Uncompressed;

            asset.spriteImportMode = SpriteImportMode.Multiple;

            asset.GetSourceTextureWidthAndHeight(out int totalWidth, out int totalHeight);

            var spritesheet = new SpriteMetaData[adf.FrameCount];
            for (int i = 0; i < adf.Frames.Count; i++)
            {
                var frame = adf.Frames[i];

                var metadata = new SpriteMetaData();
                metadata.name = frame.Index.ToString();

                // unity coordinates are bottom-up
                var y = totalHeight - frame.Y - frame.H;
                metadata.rect = new Rect(frame.X, y, frame.W, frame.H);

                if (frame.H > 32)
                {
                    metadata.alignment = (int)SpriteAlignment.Custom;
                    metadata.pivot = new Vector2(0.5f, 0.5f * 32f / (float)frame.H);
                }

                spritesheet[i] = metadata;
            }

            asset.spritesheet = spritesheet;

            asset.SaveAndReimport();
        }

        private static void ConvertToPng(ADFFile adf)
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

                File.WriteAllBytes($"Assets/Spritesheets/{adf.FileNumber}.png", texture.EncodeToPNG());

                UnityEngine.Object.DestroyImmediate(texture);
            }
            catch (Exception e)
            {
                Debug.Log($"Problem converting {adf.FileNumber}.adf: {e}");
            }
        }
    }

    public enum AnimationOrder
    {
        WalkingNoEquip = 0,
        WalkingEquip,
        AttackNoEquip,
        Attack1Hand,
        AttackStaff,
        Attack2Hand,
        AttackBow,
        SpellCast,
        Knealing,
        Death,
        Mounted,
    }

    public enum AnimationType
    {
        Body,
        Hair,
        Eyes,
        Chest,
        Helm,
        Legs,
        Feet,
        Hand
    }

    public class CompiledAnimation
    {
        public AnimationType Type { get; set; }
        public int Id { get; set; }

        public int[] AnimationIndexes { get; set; }

        public int[] AnimationFiles { get; set; }

        public CompiledAnimation(AnimationType type, int id)
        {
            this.Type = type;
            this.Id = id;
            this.AnimationIndexes = new int[4 * 11];
            this.AnimationFiles = new int[11];
        }
    }

    public class CompiledEnc
    {
        public List<CompiledAnimation> CompiledAnimations { get; private set; }
        public Dictionary<int, CompiledAnimation> SheetToAnimation { get; private set; }

        public CompiledEnc(string file)
        {
            this.CompiledAnimations = new List<CompiledAnimation>();
            this.SheetToAnimation = new Dictionary<int, CompiledAnimation>();

            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    AnimationType type = (AnimationType)Convert.ToInt32(reader.ReadInt16()) - 1;
                    int id = reader.ReadInt32();

                    var animation = new CompiledAnimation(type, id);

                    int length = 4;
                    // directions
                    for (int i = 0; i < length; i++)
                    {
                        for (int k = 0; k < 11; k++)
                        {
                            animation.AnimationIndexes[i * 11 + k] = reader.ReadInt32();
                        }
                    }
                    // files
                    for (int j = 0; j < 11; j++)
                    {
                        int fileNumber = reader.ReadInt32();
                        this.SheetToAnimation[fileNumber] = animation;
                        animation.AnimationFiles[j] = fileNumber;
                    }

                    this.CompiledAnimations.Add(animation);
                }
            }
        }

        public CompiledEnc()
        {
            this.CompiledAnimations = new List<CompiledAnimation>();
        }
    }

    public enum ADFType
    {
        Graphic = 1,
        Sound = 2,
    }

    public class Animation
    {
        public int Id { get; set; }
        public List<Frame> Frames { get; set; }

        public Animation(int id)
        {
            this.Id = id;
            this.Frames = new List<Frame>();
        }
    }

    public class Frame
    {
        public int Index { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }

        public Frame(int index, int x, int y, int w, int h)
        {
            this.Index = index;
            this.X = x;
            this.Y = y;
            this.W = w;
            this.H = h;
        }
    }

    public class ADFFile
    {
        public int FileNumber { get; set; }
        public ADFType Type { get; set; }
        public byte Version { get; set; }
        public byte Offset { get; set; }
        public int FirstFrameIndex { get; set; }
        public int EndFrameIndex { get; set; }
        public int FrameCount { get; set; }
        public int FirstAnimationIndex { get; set; }
        public int EndAnimationIndex { get; set; }
        public int AnimationCount { get; set; }
        public List<Frame> Frames { get; set; }
        public Dictionary<int, Animation> Animations { get; set; }
        public byte[] FileData { get; set; }
        public byte[] ExtraBytes { get; set; }

        public ADFFile(string file)
        {
            using (var reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                this.Type = (ADFType)Convert.ToInt32(reader.ReadByte());
                this.Version = reader.ReadByte();

                // not sure what these are.. just eat them
                int extraBytes = reader.ReadInt32();
                this.ExtraBytes = new byte[extraBytes];
                for (int i = 0; i < extraBytes; i++)
                    this.ExtraBytes[i] = reader.ReadByte();

                this.Offset = reader.ReadByte();

                this.FirstFrameIndex = this.Decode(reader.ReadInt32());
                this.FrameCount = this.Decode(reader.ReadInt32());
                this.EndFrameIndex = (this.FirstFrameIndex + this.FrameCount) - 1;
                this.FirstAnimationIndex = this.Decode(reader.ReadInt32());
                this.AnimationCount = this.Decode(reader.ReadInt32());
                this.EndAnimationIndex = (this.FirstAnimationIndex + this.AnimationCount) - 1;

                this.Frames = new List<Frame>();
                for (int i = this.FirstFrameIndex; i <= this.EndFrameIndex; i++)
                {
                    this.Frames.Add(new Frame(i,
                        this.Decode(reader.ReadInt32()),
                        this.Decode(reader.ReadInt32()),
                        this.Decode(reader.ReadInt32()),
                        this.Decode(reader.ReadInt32())));
                }

                if (this.AnimationCount > 0)
                {
                    this.Animations = new Dictionary<int, Animation>();
                    for (int i = this.FirstAnimationIndex; i <= this.EndAnimationIndex; i++)
                    {
                        var animation = new Animation(i);
                        int frameCount = this.DecodeByte(reader.ReadByte());
                        for (int j = 0; j < frameCount; j++)
                        {
                            int frameIndex = this.Decode(reader.ReadInt32());

                            var frame = this.Frames[frameIndex - this.FirstFrameIndex];
                            animation.Frames.Add(frame);
                        }

                        this.Animations.Add(i, animation);
                    }
                }

                int headerSize = this.Decode(reader.ReadInt32());
                int length = (int)(reader.BaseStream.Length - reader.BaseStream.Position);

                byte[] buffer = reader.ReadBytes(length);
                byte[] data = new byte[this.RealSize(buffer.Length)];
                for (int k = 0; k < buffer.Length; k++)
                {
                    if (k - (k / 790) >= data.Length) continue;

                    data[k - (k / 790)] = this.DecodeByte(buffer[k]);
                }

                this.FileData = data;
                this.FileNumber = Convert.ToInt32(Path.GetFileNameWithoutExtension(file));
            }
        }

        public int Decode(int data)
        {
            return (data - this.Offset);
        }

        public byte DecodeByte(byte data)
        {
            if (this.Offset > data)
            {
                data = (byte)(data + (0x100 - this.Offset));
                return data;
            }
            data = (byte)(data - this.Offset);
            return data;
        }

        public int RealSize(int datasize)
        {
            return (datasize - (datasize / 790));
        }

        public int Encode(int data)
        {
            return (data + this.Offset);
        }

        public byte EncodeByte(byte data)
        {
            if ((0x100 - data) < this.Offset)
            {
                data = (byte)(this.Offset - (0x100 - data));
                return data;
            }

            data = (byte)(data + this.Offset);
            return data;
        }
    }
}