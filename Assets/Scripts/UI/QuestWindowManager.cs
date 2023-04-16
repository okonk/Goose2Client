using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Goose2Client
{
    public class QuestWindowManager : BaseMultipleWindowManager<QuestWindow>
    {
        public override string PrefabPath => "QuestWindow";

        public override WindowFrames WindowFrame => WindowFrames.Quest;
    }
}