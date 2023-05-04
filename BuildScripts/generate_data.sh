#!/bin/bash

# export PATH="$PATH:/home/hayden/Unity/Hub/Editor/2022.2.11f1/Editor"

assetsDir="./Assets"

rm -rf "$assetsDir/Spritesheets"
rm -rf "$assetsDir/Animations"

[ ! -d $assetsDir ] && mkdir $assetsDir
[ ! -d "$assetsDir/Spritesheets" ] && mkdir "$assetsDir/Spritesheets"
[ ! -d "$assetsDir/Animations" ] && mkdir "$assetsDir/Animations"

Unity -quit -batchmode -logFile "./Logs/GenerateData.log" -projectPath . -executeMethod Goose2Client.Assets.Scripts.Editor.ToolsMenu.GenerateData