#!/bin/bash

clientDir="./Builds/Linux/Goose2Client"
streamingAssetsDir="$clientDir/Goose2Client_Data/StreamingAssets"
assetBundleDir="./Builds/AssetBundles/Linux"
outputZipPath="./Builds/Goose2Client_Linux.zip"

[ ! -d "./Builds" ] && mkdir "./Builds"
[ ! -d "./Builds/Linux" ] && mkdir "./Builds/Linux"
[ ! -d "./Builds/AssetBundles" ] && mkdir "./Builds/AssetBundles"
[ ! -d $assetBundleDir ] && mkdir $assetBundleDir

rm -rf "$clientDir/*"
rm -rf "$assetBundleDir/*"

unity-editor -quit -batchmode -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.BuildScript.PerformBuildLinux

unity-editor -quit -batchmode -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.BuildScript.PerformAssetBundleBuildLinux

rm -rf "$streamingAssetsDir"

mv "$assetBundleDir" "$streamingAssetsDir"

zip -r "$outputZipPath" "$clientDir"