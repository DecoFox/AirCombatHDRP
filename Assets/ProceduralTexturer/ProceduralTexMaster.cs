using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTexMaster
{
    [System.Serializable]
    public class TileTexture
    {
        public string Name;
        public Texture2D Texture;
        public Texture2D Normal;
        public float MinimumHeight;
        public float MaximumHeight;
        public float MinimumSteepness;
        public float MaximumSteepness;
        public float scale;
        public float u;
        public float v;

        public float SteepWeight = 1;
        public float HeightWeight = 1;
    }

    [System.Serializable]
    public class LayerAlias //We can't work with actual TileTexture objects in threads, and we don't need all of their information anyway. This exists to store data from these objects for use in a thread according to a corresonding index.
    {
        public float MinimumHeight;
        public float MaximumHeight;
        public float MinimumSteepness;
        public float MaximumSteepness;

        public float SteepWeight;
        public float HeightWeight;
    }
}