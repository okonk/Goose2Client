#!/bin/bash

clientDir="./Builds/Windows/Goose2Client"
streamingAssetsDir="$clientDir/Goose2Client_Data/StreamingAssets"
assetBundleDir="./Builds/AssetBundles/Windows"
outputZipPath="./Builds/Goose2Client.zip"

[ ! -d "./Builds" ] && mkdir "./Builds"
[ ! -d "./Builds/Windows" ] && mkdir "./Builds/Windows"
[ ! -d "./Builds/AssetBundles" ] && mkdir "./Builds/AssetBundles"
[ ! -d $assetBundleDir ] && mkdir $assetBundleDir

rm "$outputZipPath"
rm -rf "$clientDir/*"
rm -rf "$assetBundleDir/*"

Unity -quit -batchmode -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.BuildScript.PerformBuildWindows

Unity -quit -batchmode -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.BuildScript.PerformAssetBundleBuildWindows

rm -rf "$clientDir/Goose2Client_BurstDebugInformation_DoNotShip"

rm -rf "$streamingAssetsDir"

mv "$assetBundleDir" "$streamingAssetsDir"

zip -r "$outputZipPath" "$clientDir/"