using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
//using System.Collections;

public class VaporSynth : MonoBehaviour
{
    private MeshFilter MF;
    private MeshRenderer MR;
    public float Size;
    public int Res;
    public float PerlinScale;
    public float PerlinHeight = 50;
    public float IslandRadius;
    public float IslandShelf;
    // Start is called before the first frame update
    void Start()
    {
        MF = GetComponent<MeshFilter>();
        MR = GetComponent<MeshRenderer>();

        CreateMesh(MF);

    }

    void CreateMesh(MeshFilter TargetFilter)
    {
        Mesh Target = TargetFilter.mesh;

        List<Vector3> Points = new List<Vector3>();
        List<int> Tris = new List<int>();
        List<Vector2> UVs = new List<Vector2>();

        float TileSize = (float)Size / (float)Res;
        print(TileSize);

        float CenterOffset = -Size / 2;
        for(int x = 0; x < Res; x++)
        {
            float OffsetX = (float)x * TileSize;
            for (int y = 0; y < Res; y++)
            {

                float OffsetY = (float)y * TileSize;
                Points.Add(new Vector3(CenterOffset + OffsetX, FetchNoise(CenterOffset + OffsetX, CenterOffset + OffsetY), CenterOffset + OffsetY));
                Points.Add(new Vector3(CenterOffset + OffsetX, FetchNoise(CenterOffset + OffsetX, CenterOffset + TileSize + OffsetY), CenterOffset + TileSize + OffsetY));
                Points.Add(new Vector3(CenterOffset + TileSize + OffsetX, FetchNoise(CenterOffset + TileSize + OffsetX, CenterOffset + TileSize + OffsetY), CenterOffset + TileSize + OffsetY));
                Points.Add(new Vector3(CenterOffset + TileSize + OffsetX, FetchNoise(CenterOffset + TileSize + OffsetX, CenterOffset + OffsetY), CenterOffset + OffsetY));

                UVs.Add(new Vector2(0, 0));
                UVs.Add(new Vector2(0, 1));
                UVs.Add(new Vector2(1, 1));
                UVs.Add(new Vector2(1, 0));
                //int[] Tris = new int[6];
                /*
                Tris[0] = 0;
                Tris[1] = 1;
                Tris[2] = 2;

                Tris[3] = 0;
                Tris[4] = 2;
                Tris[5] = 3;
                */
                int dtx = (4 * x);
                int dty = (4 * Res * y);

                Tris.Add(0 + dtx + dty);
                Tris.Add(1 + dtx + dty);
                Tris.Add(2 + dtx + dty);
                Tris.Add(0 + dtx + dty);
                Tris.Add(2 + dtx + dty);
                Tris.Add(3 + dtx + dty);

            }

        }

        Target.vertices = Points.ToArray();
        Target.triangles = Tris.ToArray();
        Target.uv = UVs.ToArray();
        print(Target.vertices.Length);

        Target.RecalculateNormals();

        //Target.uv = 

        TargetFilter.mesh = Target;


    }

    float FetchNoise(float x, float y)
    {
        float h1 = Mathf.PerlinNoise((x + 10000) / PerlinScale, (y + 10000) / PerlinScale);
        float h2 = Mathf.PerlinNoise((x + 5000) / (PerlinScale / 2), (y + 1000) / (PerlinScale / 2)) * 0.5f;
        float h3 = Mathf.PerlinNoise((x + -5000) / (PerlinScale / 4), (y + 10000) / (PerlinScale / 4)) * 0.25f;
        float h4 = Mathf.PerlinNoise((x + 8000) / (PerlinScale / 4), (y - 10000) / (PerlinScale / 8)) * 0.1f;

        float Highlands = Mathf.PerlinNoise((x + 90000) / (PerlinScale * 2), (y - 40000) / (PerlinScale * 2)) * 1f;
        float Waterways = Mathf.PerlinNoise((x + 90000) / (PerlinScale / 1), (y - 40000) / (PerlinScale / 1)) * 0.35f;

        float height = Mathf.Clamp(((h1 + h2 + h3 + h4) * Highlands) - (Waterways * 1), 0, Mathf.Infinity);
        float IslandMask = Mathf.Clamp((IslandRadius - Vector3.Magnitude(new Vector3((float)x, 0, (float)y))) / IslandShelf, 0, 1);
        return height * PerlinHeight * IslandMask;
    }

    private void OnDrawGizmos()
    {
        /*
        if (Application.isPlaying)
        {
            Mesh M = MF.mesh;
            Vector3[] V = M.vertices;
            int[] t = M.triangles;

            foreach (Vector3 vt in V)
            {
                Handles.Label(vt + new Vector3(0, 0.2f, 0), "" + System.Array.IndexOf(V, vt));
            }
        }
        */

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
