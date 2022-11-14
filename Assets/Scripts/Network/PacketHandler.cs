using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goose2Client
{
    public abstract class PacketHandler
    {
        public abstract string Prefix { get; }

        public List<Action<object>> Observers = new();

        public abstract object Parse(PacketParser p);

        public virtual void CallObservers(object obj)
        {
            for (int i = 0; i < Observers.Count; i++)
            {
                Observers[i].Invoke(obj);
            }
        }
    }
}