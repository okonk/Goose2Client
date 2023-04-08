using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class InfoWindowCreator : BaseMultipleWindowManager<QuestWindow>
    {
        public override string PrefabPath => "Prefabs/UI/InfoWindow";

        public override WindowFrames WindowFrame => WindowFrames.GenericInfo;
    }
}