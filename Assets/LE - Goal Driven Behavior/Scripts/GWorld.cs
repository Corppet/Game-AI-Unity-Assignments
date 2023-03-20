using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public sealed class GWorld
    {
        public static GWorld Instance
        {
            get { return instance; }
        }

        private static readonly GWorld instance = new();
        private static WorldStates world;

        public WorldStates GetWorld()
        {
            return world;
        }

        private GWorld()
        {
            world = new();
        }
    }
}
