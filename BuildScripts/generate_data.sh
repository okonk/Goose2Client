#!/bin/bash

# export PATH="$PATH:/home/hayden/Unity/Hub/Editor/2022.2.11f1/Editor"

assetsDir="./Assets"

rm -rf "$assetsDir/Test"

[ ! -d $assetsDir ] && mkdir $assetsDir
[ ! -d "$assetsDir/Test" ] && mkdir "$assetsDir/Test"
[ ! -d "$assetsDir/Test/Spritesheets" ] && mkdir "$assetsDir/Test/Spritesheets"
[ ! -d "$assetsDir/Test/Animations" ] && mkdir "$assetsDir/Test/Animations"

Unity -quit -batchmode -logFile "./Logs/GenerateData.log" -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.ToolsMenu.GenerateData