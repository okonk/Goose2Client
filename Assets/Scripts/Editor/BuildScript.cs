using UnityEditor;

namespace Goose2Client.Assets.Scripts.Editor
{
    public class BuildScript
    {
        [MenuItem("Tools/Build/Linux")]
        public static void PerformBuildLinux()
        {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./Builds/Linux/Goose2Client/Goose2Client.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.CompressWithLz4HC);
        }

        [MenuItem("Tools/Build/AssetBundles/Linux")]
        public static void PerformAssetBundleBuildLinux()
        {
            BuildPipeline.BuildAssetBundles("./Builds/AssetBundles/Linux/", BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);
        }

        [MenuItem("Tools/Build/Windows")]
        public static void PerformBuildWindows()
        {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./Builds/Windows/Goose2Client/Goose2Client.exe", BuildTarget.StandaloneWindows64, BuildOptions.CompressWithLz4HC);
        }

        [MenuItem("Tools/Build/AssetBundles/Windows")]
        public static void PerformAssetBundleBuildWindows()
        {
            BuildPipeline.BuildAssetBundles("./Builds/AssetBundles/Windows/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Tools/Build/Android")]
        public static void PerformBuildAndroid()
        {
            //PlayerSettings.Android.bundleVersionCode = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"./Builds/Android/Goose2Client/Goose2Client.apk", BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("Tools/Build/AssetBundles/Android")]
        public static void PerformAssetBundleBuildAndroid()
        {
            BuildPipeline.BuildAssetBundles("./Builds/AssetBundles/Android/", BuildAssetBundleOptions.None, BuildTarget.Android);
        }
    }
}