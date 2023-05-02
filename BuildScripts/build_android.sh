#!/bin/bash

# export PATH="$PATH:/home/hayden/Unity/Hub/Editor/2022.2.11f1/Editor"

clientDir="./Builds/Android/Goose2Client"
streamingAssetsDir="./Assets/StreamingAssets"
assetBundleDir="./Builds/AssetBundles/Android"

[ ! -d "./Builds" ] && mkdir "./Builds"
[ ! -d "./Builds/Android" ] && mkdir "./Builds/Android"
[ ! -d "./Builds/AssetBundles" ] && mkdir "./Builds/AssetBundles"
[ ! -d $assetBundleDir ] && mkdir $assetBundleDir

rm -rf "$clientDir/*"
rm -rf "$assetBundleDir/*"
rm -rf "$streamingAssetsDir/*"

Unity -quit -batchmode -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.BuildScript.PerformAssetBundleBuildAndroid

mv "$assetBundleDir" "$streamingAssetsDir"

Unity -quit -batchmode -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.BuildScript.PerformBuildAndroid