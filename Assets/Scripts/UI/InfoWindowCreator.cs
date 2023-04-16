using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class InfoWindowCreator : BaseMultipleWindowManager<InfoWindow>
    {
        public override string PrefabPath => "InfoWindow";

        public override WindowFrames WindowFrame => WindowFrames.GenericInfo;
    }
}