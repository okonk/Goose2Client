using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public interface IWindow
    {
        int WindowId { get; }

        WindowFrames WindowFrame { get; }
    }
}