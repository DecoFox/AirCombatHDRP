using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTexMaster;
using System.Linq;
using System.Threading;
using System;

public class ProceduralTexClient : MonoBehaviour
{
    public float SteepnessCoefficient = 1; //1 works well at 513x513 heightmap sizes. Smaller sizes will require smaller coefficients.
    public List<TileTexture> TexturesToTile = new List<TileTexture>();
    private TerrainData Data;
    private Texture2D[] MaskTextures;
    private LayerAlias[] Aliases; //An array of layer aliases, one for each TileTexture. Index should correspond with respective TileTexture index. Stores parameter data for use in the thread.

    //Set up some hard measurements that we'll need in the thread later. We COULD pass these into the thread as args, but this is ultimately cleaner since we're just looking for properties of the current terrain.
    private float[,] MeasuredHeights;
    private int AlphaResolution;
    private int DetailResolution;
    private int HeightmapResolution;
    private TerrainLayer[] LayerList;

    private float[,,] SplatStack; //3d float array of resultant mask values as indexed by x,y location, and layer index (z);
    private List<Action> CuedActions = new List<Action>();


    void Start()
    {
        InitializePainter(gameObject.GetComponentInChildren<Terrain>().terrainData);
    }

    public void InitializePainter(TerrainData TDataIn)
    {
        Data = TDataIn;
        TexturesToTile = gameObject.transform.parent.GetComponent<ProceduralTexServer>().TileTextures;
        ConfigureSplats(Data);

        HeightmapResolution = Data.heightmapResolution;
        AlphaResolution = Data.alphamapResolution;
        DetailResolution = Data.detailResolution;
        //print(AlphaResolution);
        MeasuredHeights = new float[AlphaResolution, AlphaResolution];
        //MeasuredHeights = Data.GetHeights(0, 0, AlphaResolution, AlphaResolution);
        MeasuredHeights = Data.GetInterpolatedHeights(0, 0, AlphaResolution, AlphaResolution, 1.0f / (float)AlphaResolution, 1.0f / (float)AlphaResolution);
        /*
        for(int x = 0; x < AlphaResolution; x++)
        {
            for(int y = 0; y < AlphaResolution; y++)
            {
                MeasuredHeights[y, x] = Data.GetInterpolatedHeight(x, y);
            }
        }
        */

        Thread PaintThread = new Thread(() => Paint(MappedLayerCallback =>
        {

            ApplyPaint(MappedLayerCallback);

        }
        )
        );

        PaintThread.Start();


        //Paint();
    }

    private void Update()
    {
        while (CuedActions.Count > 0)
        {
            Action Act = CuedActions[0];
            CuedActions.RemoveAt(0);
            Act();
        }
    }

    void ConfigureSplats(TerrainData Dat)
    {
        LayerList = new TerrainLayer[TexturesToTile.Count];
        Aliases = new LayerAlias[TexturesToTile.Count];
        MaskTextures = new Texture2D[TexturesToTile.Count];
        for (int i = 0; i < TexturesToTile.Count; i++)
        {
            //MaskTextures[i] = new Texture2D(Data.alphamapResolution, Data.alphamapResolution);

            LayerList[i] = new TerrainLayer();
            LayerList[i].tileOffset = new Vector2(TexturesToTile[i].u, TexturesToTile[i].v);
            LayerList[i].tileSize = new Vector2(TexturesToTile[i].scale, TexturesToTile[i].scale);
            LayerList[i].name = TexturesToTile[i].Name;
            LayerList[i].diffuseTexture = TexturesToTile[i].Texture;
            LayerList[i].normalMapTexture = TexturesToTile[i].Normal;
            //LayerList[i].maskMapTexture = MaskTextures[i];
            //LayerList[i].maskMapTexture = new Texture2D(Data.heightmapResolution, Data.heightmapResolution);

            //Populate the values in the layer alias we're feeding to the thread so they match the originating TileTexture object
            LayerAlias LA = new LayerAlias();
            LA.MaximumHeight = TexturesToTile[i].MaximumHeight;
            LA.MinimumHeight = TexturesToTile[i].MinimumHeight;
            LA.MaximumSteepness = TexturesToTile[i].MaximumSteepness;
            LA.MinimumSteepness = TexturesToTile[i].MinimumSteepness;
            LA.HeightWeight = TexturesToTile[i].HeightWeight;
            LA.SteepWeight = TexturesToTile[i].SteepWeight;
            Aliases[i] = LA;
        }
        Dat.terrainLayers = LayerList;
        print(LayerList.Length);
    }

    void Paint(System.Action<float[,,]> MappedLayerCallback)
    {
        SplatStack = new float[AlphaResolution, AlphaResolution, LayerList.Length];
        float[] SplatWeights = new float[LayerList.Length];
        for(int y = 0; y < AlphaResolution; y++)
        {
            for (int x = 0; x < AlphaResolution; x++)
            {
                //Normalize the texture coordinates so we can work in percentages
                float ynorm = (float)y / (float)AlphaResolution;
                float xnorm = (float)x / (float)AlphaResolution;
                float ynorm2 = ((float)y + 1) / (float)AlphaResolution;
                float xnorm2 = ((float)x + 1) / (float)AlphaResolution;

                float ynorm3 = ((float)y - 1) / (float)AlphaResolution;
                float xnorm3 = ((float)x - 1) / (float)AlphaResolution;

                //Find the height value at this coordinate, and at the adjacent coordinates on X and Y (for measuring slope).
                float height = MeasuredHeights[Mathf.RoundToInt(ynorm * AlphaResolution), Mathf.RoundToInt(xnorm * AlphaResolution)] * 1000;
                float height2 = MeasuredHeights[Mathf.RoundToInt(ynorm * AlphaResolution), Mathf.Clamp(Mathf.RoundToInt(xnorm2 * AlphaResolution), 0, AlphaResolution - 1)] * 1000;
                float height3 = MeasuredHeights[Mathf.Clamp(Mathf.RoundToInt(ynorm2 * AlphaResolution), 0, AlphaResolution - 1), Mathf.RoundToInt(xnorm * AlphaResolution)] * 1000; ;

                float height4 = MeasuredHeights[Mathf.RoundToInt(ynorm * AlphaResolution), Mathf.Clamp(Mathf.RoundToInt(xnorm3 * AlphaResolution), 0, AlphaResolution - 1)] * 1000;
                float height5 = MeasuredHeights[Mathf.Clamp(Mathf.RoundToInt(ynorm3 * AlphaResolution), 0, AlphaResolution - 1), Mathf.RoundToInt(xnorm * AlphaResolution)] * 1000; ;

                //print(height);
                //How different is our main sample from its adjacent values?
                float DX = height - height2; 
                float DY = height - height3;
                float DX2 = height - height4;
                float DY2 = height - height5;

                //Pythagorean theorum to get steepness.
                float s1 = Mathf.Abs(((Mathf.Sqrt((DX * DX) + (DY * DY)))) / 60); //The multiplier here depends on resolution. Lower res = lower multiplier. To match 7 at 512, we would need 0.89 at 1. Calculated via proportion.
                float s2 = Mathf.Abs(((Mathf.Sqrt((DX2 * DX2) + (DY2 * DY2)))) / 60); //The multiplier here depends on resolution. Lower res = lower multiplier. To match 7 at 512, we would need 0.89 at 1. Calculated via proportion.


                float steep = (s1 + s2) / 2;
                steep = (steep / 1.5f) * SteepnessCoefficient;
                //print(steep);
                //float steep = height - ((height2 + height3 + height4 + height5) / 4);

                //NOTE: The big problem here is we're going to apply alphamap values based on height and steepness and maybe whatever else, 
                //but we need a way to know which parameters we selected in the Server script.
                //The indicies of the layers should be consistent, so what if we create a series of dictionaries (or maybe a class?) that relate the indicie of a layer to its paramters...
                //We would four dictionaries: one for min heights, one for max heights, one for min steepness, one for max steepness.
                //Let's try the class first:
                foreach (LayerAlias A in Aliases)
                {
                    int index = System.Array.IndexOf(Aliases, A);

                    float SteepMinStrength = (steep - A.MinimumSteepness) / 65;
                    float SteepMaxStrength = (A.MaximumSteepness - steep) / 65;
                    float SteepStrength = Mathf.Clamp((SteepMaxStrength * SteepMinStrength), 0.001f, 1) * A.SteepWeight;
                    //print(SteepStrength);


                    float HeightMinStrength = (height - A.MinimumHeight) / 100;
                    float HeightMaxStrength = (A.MaximumHeight - height) / 100;
                    float HeightStrength = Mathf.Clamp((HeightMaxStrength * HeightMinStrength) , 0.001f, 1) * A.HeightWeight;
                    //float HeightStrength = 0;

                    //SplatStack[x, y, index] = SteepStrength + HeightStrength;

                    SplatWeights[index] = Mathf.Clamp(HeightStrength * SteepStrength,0, Mathf.Infinity);
                    //SplatWeights[index] = Mathf.Clamp( SteepStrength, 0, Mathf.Infinity);
                    //SplatWeights[index] = HeightMaxStrength;

                    //print(HeightMaxStrength);
                }
                
                float SplatTotal = SplatWeights.Sum();
                //print(SplatTotal);
                for(int i = 0; i < LayerList.Length; i++)
                {
                    SplatWeights[i] = (SplatWeights[i] / SplatTotal);
                    SplatStack[y, x, i] = SplatWeights[i];
                }
            }
        }

        MappedLayerCallback(SplatStack);

    }

    void ApplyPaint(float[,,] SplatStack)
    {
        Action CueTerrainPaint = () =>
        {
            Data.SetAlphamaps(0, 0, SplatStack);
        };
        CuedActions.Add(CueTerrainPaint);

        
    }

}