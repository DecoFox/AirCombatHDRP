
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTexMaster;

[ExecuteInEditMode]
public class ProceduralTexServer : MonoBehaviour
{
    public bool Run = false;
    public List<TileTexture> TileTextures = new List<TileTexture>();

    private void OnValidate()
    {
        if (Run)
        {
            print("Executing");
            ProceduralTexClient[] Clients = gameObject.GetComponentsInChildren<ProceduralTexClient>();
            foreach(ProceduralTexClient C in Clients)
            {
                C.SteepnessCoefficient = 1000;
                C.InitializePainter(C.gameObject.GetComponent<Terrain>().terrainData);

            }
            Run = false;
        }
    }
}

