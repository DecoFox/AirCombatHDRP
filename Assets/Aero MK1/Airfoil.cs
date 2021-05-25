using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Airfoil : MonoBehaviour
{
    [ExecuteInEditMode]
    // Start is called before the first frame update

    public List<ChordMarker> markers = new List<ChordMarker>();
    public int Resolution;
    public float Washout;
    public bool InvertNormals;

    public int SymmetricalAirfoil;

    public Vector3 TipPosition;
    public Vector3 RootPosition;
    public Airfoil ParentWing;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Concept--------------
    /*
     * Wing is created in sections separated by "ChordMarkers"
     * "ChordMarkers" are a line with a length and local (to the wing root) position
     * These define the shape of the wing
     * With the wing shape established, we then distribute Subsurfaces across the wing based on the resolution
     * Script figures how many segments each chord section needs to make the total wing have the proper resolution
     * Subsurfaces are planar quads with trailing and leading edge measurements at their "root" and "tip"
     * Subsurfaces use this information to compute their own area and normals
     * We can then create lift nodes preconfigured by subsurfaces to simulate wings with the parameters those subsurfaces describe.
     * Speed / angle measurement is all done locally, so sweep is supported out-of-hand
     * Area measurement is computed accurately rather than divided from the whole, so taper is supported
     * Could also be set up to simulate a float for Washout distributed over the length of the wing, slightly changing the AoI. Probably handy.

     */

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {

            foreach (ChordMarker M in markers)
            {
                M.surfaces.Clear();
                int CurrentEntry = markers.IndexOf(M);
                Vector3 CurrentLeading = transform.TransformPoint(M.Position.x, M.Position.y, M.Position.z + (M.ChordLength / 2));
                Vector3 CurrentTrailing = transform.TransformPoint(M.Position.x, M.Position.y, M.Position.z - (M.ChordLength / 2));

                if (CurrentEntry <= markers.Count - 2)
                {
                    //Draw lines between the terminal points
                    ChordMarker NM = markers[CurrentEntry + 1];

                    Vector3 NextLeading = transform.TransformPoint(NM.Position.x, NM.Position.y, NM.Position.z + (NM.ChordLength / 2));
                    Vector3 NextTrailing = transform.TransformPoint(NM.Position.x, NM.Position.y, NM.Position.z - (NM.ChordLength / 2));

                    Debug.DrawLine(CurrentLeading, NextLeading, new Color(0, 255, 0));
                    Debug.DrawLine(CurrentTrailing, NextTrailing, new Color(255, 0, 0));

                    //Divide into resolution segments
                    Resolution = Mathf.RoundToInt(Mathf.Clamp(Resolution, 1, Mathf.Infinity));
                    float SegmentDistance = Vector3.Distance(M.Position, NM.Position); //How long is this particular wing segment
                    float RelativeScale = Vector3.Distance(M.Position, NM.Position) / Vector3.Distance(markers[0].Position, markers[markers.Count - 1].Position); //What fraction of the whole wing (spanwise) is the current section?
                    int SegmentCount = (int)Mathf.RoundToInt(Resolution * RelativeScale); //How many segments we're going to split this particular wing section into (a function of resolution and section size), so that the overall wing matches the requested resolution.
                    SegmentCount = (int)Mathf.Clamp(SegmentCount, 1, Mathf.Infinity);
                    for (int i = 0; i <= SegmentCount; i++)
                    {
                        Vector3 LeadPoint = Vector3.Lerp(CurrentLeading, NextLeading, (float)i / (float)SegmentCount);
                        Vector3 TrailPoint = Vector3.Lerp(CurrentTrailing, NextTrailing, (float)i / (float)SegmentCount);
                        Debug.DrawLine(LeadPoint, TrailPoint, new Color(0, 255, 255));


                        if (i < SegmentCount) //Populate SubSurfaces from our Segments as we create them
                        {
                            Vector3 NextLeadPoint = Vector3.Lerp(CurrentLeading, NextLeading, (float)(i + 1) / (float)SegmentCount);
                            Vector3 NextTrailPoint = Vector3.Lerp(CurrentTrailing, NextTrailing, (float)(i + 1) / (float)SegmentCount);
                            float NormMult = 1;
                            if (InvertNormals)
                            {
                                NormMult = -1;
                            }
                            Vector3 WingNormal = Vector3.Normalize(Vector3.Cross(NextLeading - CurrentLeading, CurrentLeading - CurrentTrailing) * NormMult);
                            if (M.SubjectToWashout)
                            {
                                float TotalSpan = Vector3.Distance(transform.position, markers[markers.Count - 1].Position);
                                float WashoutScalar = Vector3.Distance((LeadPoint + TrailPoint + NextLeadPoint + NextTrailPoint) / 4, transform.position) / TotalSpan;
                                WingNormal = Vector3.Normalize(WingNormal + new Vector3(0, 0, Washout * WashoutScalar));
                            }
                            //NOTE: WingNormals here are providing us with Angle of Incidence for our transforms later, but not force direction.
                            //Force direction should be the cross product of the center-root to center-tip of each element and the world velocity
                            
                            SubSurface S = new SubSurface(LeadPoint, TrailPoint, NextLeadPoint, NextTrailPoint, WingNormal, InvertNormals);
                            Debug.DrawLine(S.Center + (S.Normal * 0.1f), S.Center + (S.Normal * 0.3f), new Color(255, 0, 255));
                            Handles.Label(S.Center + new Vector3(0, 0.3f, 0), "Area: " + S.Area);
                            if (!M.surfaces.Contains(S))
                            {
                                M.surfaces.Add(S);
                            }
                        }

                    }


                    //NOTE FOR LATER: WE SHOULD BE ABLE TO EXTRACT THE WING NORMALS BY TAKING THE CROSS PRODUCT OF THE DIFFERENCES BETWEEN CHORD MARKERS

                }
                //Draw chord lines
                Debug.DrawLine(CurrentLeading, CurrentTrailing, new Color(255, 255, 255));
                //Debug.DrawLine(transform.TransformPoint(M.Position - new Vector3(0,0.25f,0)), transform.TransformPoint(M.Position + new Vector3(0,0.25f,0)), new Color(0, 0, 255));

                

            }

        }
        
       
    }

    public List<LiftSurface> BuildLiftSurfaces()
    {
        List<LiftSurface> LiftingSurfaces = new List<LiftSurface>();
        if (ParentWing == null) //Use our local tip and root unless we define a parent wing (as we might for a control surface)
        {
            TipPosition = transform.TransformPoint(markers[markers.Count - 1].Position);
            RootPosition = transform.position;
        }
        else
        {
            TipPosition = ParentWing.TipPosition;
            RootPosition = ParentWing.RootPosition;
        }
        LiftingSurfaces.Clear();

        foreach(ChordMarker M in markers)
        {
            foreach (SubSurface s in M.surfaces)
            {

                //Debug.DrawLine(s.Center + (s.Normal * 0.1f), s.Center + (s.Normal * 0.3f), new Color(255, 0, 255));
                

                //Actually create lifting surfaces for each subsurf and add them to the list
                float Falloff = (Vector3.Distance(RootPosition, s.Center) / Vector3.Distance(RootPosition, TipPosition)) / 2; //Falloff approaches one as we approach the wingtip.
                Vector3 AircraftCenter = gameObject.GetComponentInParent<Rigidbody>().transform.InverseTransformPoint(s.Center);


                LiftSurface LS = new LiftSurface(AircraftCenter, s.Normal, s.Area, 100, Falloff, s.Spar, SymmetricalAirfoil);
                //Handles.Label(s.Center + new Vector3(0, 0.3f, 0), "F/O: " + Falloff);

                if (!LiftingSurfaces.Contains(LS))
                {
                    LiftingSurfaces.Add(LS);
                }
            }
        }

        
        return LiftingSurfaces;
    }

    private void OnValidate()
    {
        //Clear all the Subsurfaces in all the wings whenever we change inspector values relating to wings so we can rebuild them from scratch
        //foreach(ChordMarker m in markers)
        //{
            //m.surfaces.Clear();
        //}
        //LiftingSurfaces.Clear();
    }

    [System.Serializable]
    public struct ChordMarker //What we use to define shape through changes along the leading and trailing edge
    {
        public Vector3 Position;
        public float ChordLength;
        public List<SubSurface> surfaces;
        public bool SubjectToWashout;

        public ChordMarker(Vector3 Position, float ChordLength, bool SubjectToWashout = true)
        {
            this.Position = Position;
            this.ChordLength = ChordLength;
            this.surfaces = new List<SubSurface>();
            this.SubjectToWashout = SubjectToWashout;
        }
    }



    //The sub-components into which we split wing sections, with said sections having been created between chordmarkers. 
    //These are not lines like chordmarkers, but rather 2d planar surfaces with root and tip entries for the trailing and leading edge
    //plus an area, as determined by the two comprising triangles
    //And a normal, obtained by cross product
    //These are the things that we will use to inform our lift nodes.
    [System.Serializable]
    public struct SubSurface 
    {
        public Vector3 RootLeading;
        public Vector3 RootTrailing;
        public Vector3 TipLeading;
        public Vector3 TipTrailing;
        public Vector3 Normal;
        public Vector3 Spar; //Average of the tips - average of the roots. Used for cross product with relative wind for lift direction.
        public float Area;

        public Vector3 Center;

        public SubSurface(Vector3 RootLeading, Vector3 RootTrailing, Vector3 TipLeading, Vector3 TipTrailing, Vector3 Normal, bool InvertSpar)
        {
            this.RootLeading = RootLeading;
            this.RootTrailing = RootTrailing;
            this.TipLeading = TipLeading;
            this.TipTrailing = TipTrailing;
            this.Normal = Normal;

            //Area Calculation--------
            Vector3 LeadingEdge = (TipLeading - RootLeading);
            Vector3 TrailingEdge = (TipTrailing - RootTrailing);
            Vector3 Hypotenuse = (TipTrailing - RootLeading);
            float theta1 = Vector3.Angle(LeadingEdge, Hypotenuse) * Mathf.Deg2Rad;
            float theta2 = Vector3.Angle(TrailingEdge, Hypotenuse) * Mathf.Deg2Rad;
        
            float b1 = Vector3.Distance(TipLeading, RootLeading); //Length of the leading edge
            float b2 = Vector3.Distance(TipTrailing, RootTrailing); //length of the trailing edge
            float hyplength = Vector3.Distance(TipTrailing, RootLeading); //length of the hypotenuse

            float Tri1Area = 0.5f * hyplength * b1 * Mathf.Sin(theta1);
            float Tri2Area = 0.5f * hyplength * b2 * Mathf.Sin(theta2);
            //Area calculation finished----
            this.Area = Tri1Area + Tri2Area; //Total area is the sum of the two triangles we split it into to run the numbers
            this.Center = (RootLeading + RootTrailing + TipLeading + TipTrailing) / 4;

            //"Spar" calculation
            Vector3 AvgRoot = (RootLeading + RootTrailing) / 2;
            Vector3 AvgTip  = (TipLeading + TipTrailing) / 2;

            float SparInversion = 1;
            if (InvertSpar)
            {
                SparInversion = -1;
            }

            this.Spar = (AvgTip - AvgRoot) * SparInversion;
        }
    }


    //The object we'll use to actually preform lift calculations. Populated directly but SubSurface. One SubSurface = one LiftSurface
    //Only different because SubSurfaces are full of computational shit we don't really need in realtime once the game is playing.
    //Also because compute shaders etc
    //We'll store a list of all this wing's lift surfaces here, but we won't iterate on them. A master script on the whole airplane will fetch
    //and yeet them into a compute shader.

    //IDEA: We'll have to change surface health outside the shader because the compute shader can't do hit detection. 
    //Maybe we could cause one section going full "red" to also destroy every section outboard of it, simulating that part 
    //of the wing breaking off?
    //Maybe instead we destroy when average section health is < X. Simpler from a visuals perspective.

    //NOTE: Since we're collecting all of these nodes for the compute shader elsewhere, the aero code won't necessarily know if the wing is
    //outright missing. We'll have to nuke node health when the wing is destroyed.
    //Or we could send the code from each wing, but is that a good idea? We should probably minimzie the number of times we write to the buffer.
    
    [System.Serializable]
    public struct LiftSurface
    {
        public Vector3 Center;
        public Vector3 Normal;
        public float Area;
        public float Health;
        public float Falloff;
        public Vector3 LocalVelocity;
        public Vector3 WorldVelocity;

        public Vector3 SparVector;

        public float Lift;
        public float Drag;

        public float CL;
        public int Symmetric;

        public LiftSurface(Vector3 Center, Vector3 Normal, float Area, float Health, float Falloff, Vector3 SparVector, int Symmetric)
        {
            this.Center = Center;
            this.Normal = Normal;
            this.Area = Area;
            this.Health = Health;
            this.Falloff = Falloff;
            this.LocalVelocity = Vector3.zero;
            this.WorldVelocity = Vector3.zero;
            this.Symmetric = Symmetric;
            this.SparVector = SparVector;
            Lift = 0;
            Drag = 0;
            CL = 0;
        }
    }
}
