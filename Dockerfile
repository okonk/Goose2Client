FROM unityci/editor:2022.2.11f1-linux-il2cpp-1 AS base

WORKDIR .
COPY . .

RUN BuildScripts/build_windows.sh