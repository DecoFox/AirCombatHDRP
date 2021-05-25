using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[ExecuteInEditMode]
public class AirfoilMk2 : MonoBehaviour
{
    public bool Vaporwave;
    public Material PanelMat;
    public AnimationCurve CLc;
    public AnimationCurve CDc;
    public float Washout;
    public int Resolution;
    public List<WingSection> Sections = new List<WingSection>();
    //public List<Element> Elements = new List<Element>(); //Could store these globally in the wing or individually in sections and do a nested foreach

    public Material LineMaterial;
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isPlaying && Vaporwave)
        {
            
            foreach (WingSection S in Sections)
            {
                foreach (Element E in S.Elements)
                {
                    Transform RT = new GameObject().transform;
                    RT.transform.rotation = transform.rotation;
                    RT.transform.position = E.T.position;
                    RT.transform.parent = E.T;

                    Vector3 LocLeadRoot = RT.InverseTransformPoint(transform.TransformPoint(E.LeadRoot)); //The points are in "wing" space, so we shift them into world space and then down into node space
                    Vector3 LocTrailRoot = RT.InverseTransformPoint(transform.TransformPoint(E.TrailRoot));
                    Vector3 LocLeadTip = RT.InverseTransformPoint(transform.TransformPoint(E.LeadTip));
                    Vector3 LocTrailTip = RT.InverseTransformPoint(transform.TransformPoint(E.TrailTip));

                    Vector3 IntLeadRoot = LocLeadRoot / 1.2f;
                    Vector3 IntTrailRoot = LocTrailRoot / 1.2f;
                    Vector3 IntLeadTip = LocLeadTip / 1.2f;
                    Vector3 IntTrailTip = LocTrailTip / 1.2f;
                    //Vector3[] MV = new Vector3[4] { E.LeadRoot - transform.InverseTransformPoint(RT.position), E.LeadTip - transform.InverseTransformPoint(RT.position), E.TrailTip - transform.InverseTransformPoint(RT.position), E.TrailRoot - transform.InverseTransformPoint(RT.position) };

                    List<int> LTris = new List<int>();
                    Vector3[] MV = new Vector3[16]
                    {
                        LocLeadRoot,
                        LocLeadTip,
                        IntLeadTip,
                        IntLeadRoot,

                        LocTrailRoot,
                        LocLeadRoot,
                        IntLeadRoot,
                        IntTrailRoot,

                        LocTrailRoot,
                        LocTrailTip,
                        IntTrailTip,
                        IntTrailRoot,

                        LocTrailTip,
                        LocLeadTip,
                        IntLeadTip,
                        IntTrailTip




                        //LocTrailTip,
                        //LocTrailRoot
                    };

                    for(int i = 0; i < 4; i++)
                    {
                        LTris.Add(0 + (i * 4));
                        LTris.Add(1 + (i * 4));
                        LTris.Add(2 + (i * 4));
                        LTris.Add(3 + (i * 4));
                        LTris.Add(0 + (i * 4));
                        LTris.Add(2 + (i * 4));
                    }


                    //int[] Tris = new int[6] { 0, 1, 2, 3, 0, 2 };
                    int[] Tris = LTris.ToArray();


                    Vector3[] normals = new Vector3[16];
                    for(int i = 0; i < normals.Length; i++)
                    {
                        normals[i] = -Vector3.forward * E.NormalInversion;
                    }
                    /*
                    {
                        -Vector3.forward * E.NormalInversion,
                        -Vector3.forward * E.NormalInversion,
                        -Vector3.forward * E.NormalInversion,
                        -Vector3.forward * E.NormalInversion
                    };
                    */
                    

                    MeshFilter F = RT.gameObject.AddComponent<MeshFilter>();
                    MeshRenderer R = RT.gameObject.AddComponent<MeshRenderer>();
                    R.material = PanelMat;

                    Mesh M = F.mesh;

                    M.vertices = MV;
                    M.triangles = Tris;
                    M.normals = normals;
                    F.mesh = M;

                }
            }
            




            //TRIG TUBES VERSION------------------------
            /*
            List<Vector3> AllPoints = new List<Vector3>();
            foreach(WingSection S in Sections)
            {


                AllPoints.Add(S.TipChord.LeadPoint);
                AllPoints.Add(S.RootChord.LeadPoint);
                AllPoints.Add(S.RootChord.TrailPoint);
                AllPoints.Add(S.TipChord.TrailPoint);




            }


            List<Vector3> Verts = new List<Vector3>(); //A list to populate with actual verticies derived from AllPoints (four+ per point)
            List<int> Tris = new List<int>();
            foreach (Vector3 P in AllPoints)
            {
                if (AllPoints.IndexOf(P) < AllPoints.Count - 1)
                {


                    
                    for (int i = 1; i < 5; i++)
                    {
                        int CI = AllPoints.IndexOf(P);

                        Vector3 P1 = P + new Vector3(0, -Mathf.Cos(6.28f * ((float)i / 4)), Mathf.Sin(6.28f * ((float)i / 4))) * 0.15f;
                        Vector3 P2 = P + new Vector3(0, Mathf.Cos(6.28f * ((float)(i + 1) / 4)), -Mathf.Sin(6.28f * ((float)(i + 1) / 4))) * 0.15f;
                        Vector3 P3 = AllPoints[CI + 1] + new Vector3(0, Mathf.Cos(6.28f * ((float)(i + 1) / 4)), -Mathf.Sin(6.28f * ((float)(i + 1) / 4))) * 0.15f;
                        Vector3 P4 = AllPoints[CI + 1] + new Vector3(0, -Mathf.Cos(6.28f * ((float)i / 4)), Mathf.Sin(6.28f * ((float)i / 4))) * 0.15f;

                        Verts.Add(P4);
                        Verts.Add(P3);
                        Verts.Add(P2);
                        Verts.Add(P1);

                        Tris.Add((CI * 16) + ((i - 1) * 4) + 0);
                        Tris.Add((CI * 16) + ((i - 1) * 4) + 3);
                        Tris.Add((CI * 16) + ((i - 1) * 4) + 2);
                        Tris.Add((CI * 16) + ((i - 1) * 4) + 2);
                        Tris.Add((CI * 16) + ((i - 1) * 4) + 1);
                        Tris.Add((CI * 16) + ((i - 1) * 4) + 0);


                    }
                    

                }

            }

            MeshFilter F = transform.gameObject.AddComponent<MeshFilter>();
            MeshRenderer R = transform.gameObject.AddComponent<MeshRenderer>();
            R.material = PanelMat;

            Mesh M = F.mesh;
            M.vertices = Verts.ToArray();
            M.triangles = Tris.ToArray();
            F.mesh = M;
            */
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnRenderObject()
    {
        /*
        foreach(WingSection S in Sections)
        {
            foreach(Element E in S.Elements)
            {
                GL.PushMatrix();
                PanelMat.SetPass(0);
                GL.Begin(GL.QUADS);
                GL.Color(Color.white);
                GL.MultMatrix(transform.localToWorldMatrix);

                GL.Vertex(E.LeadRoot);
                GL.Vertex(E.LeadTip);
                GL.Vertex(E.TrailTip);
                GL.Vertex(E.TrailRoot);
                GL.End();
                GL.PopMatrix();

            }
        }
        */

    }


    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            float Span = 0;
            foreach (WingSection S in Sections)
            {
                float DistR = S.RootChord.CenterPoint.magnitude;
                float DistT = S.TipChord.CenterPoint.magnitude;

                //Find the extent of the wingspan by looking for the most distant chord line
                if (DistR > Span)
                {
                    Span = DistR;
                }

                if (DistT > Span)
                {
                    Span = DistT;
                }
            }


            foreach (WingSection S in Sections)
            {
                WingSection PrevSection = null;
                if (Sections.IndexOf(S) > 0)
                {
                    PrevSection = Sections[Sections.IndexOf(S) - 1];
                }

                if (S.InheritRoot && PrevSection != null)
                {
                    S.RootChord = PrevSection.TipChord;
                }

                //Distribute Washout
                float RootStationPercent = Vector3.Distance(Sections[Sections.IndexOf(S)].RootChord.CenterPoint, Sections[0].RootChord.CenterPoint) / Span;
                float TipStationPercent = Vector3.Distance(Sections[Sections.IndexOf(S)].TipChord.CenterPoint, Sections[0].RootChord.CenterPoint) / Span; ;
                S.RootChord.WashRot = Mathf.Lerp(0, Washout, RootStationPercent);
                S.TipChord.WashRot = Mathf.Lerp(0, Washout, TipStationPercent);

                //Match Dihedral to previous wing, if desired
                if (S.MatchDihedral && PrevSection != null)
                {
                    float PrevSectionDistance = Mathf.Abs(PrevSection.TipChord.CenterPoint.x - PrevSection.RootChord.CenterPoint.x);
                    float CurSectionDistance = Mathf.Abs(S.TipChord.CenterPoint.x - S.RootChord.CenterPoint.x);
                    float Slope = (PrevSection.TipChord.CenterPoint.y - PrevSection.RootChord.CenterPoint.y) / PrevSectionDistance;
                    S.TipChord.Position.y = PrevSection.TipChord.Position.y + (Slope * CurSectionDistance);
                }

                //Match taper to the previous wing, if desired
                if (S.MatchTaper && PrevSection != null)
                {
                    float PrevSectionDistance = Mathf.Abs(PrevSection.TipChord.CenterPoint.x - PrevSection.RootChord.CenterPoint.x);
                    float CurSectionDistance = Mathf.Abs(S.TipChord.CenterPoint.x - S.RootChord.CenterPoint.x);
                    float PrevDeltaChord = PrevSection.TipChord.ChordLength - PrevSection.RootChord.ChordLength;

                    float Slope = PrevDeltaChord / PrevSectionDistance;
                    S.TipChord.ChordLength = PrevSection.TipChord.ChordLength + (Slope * CurSectionDistance);
                }

                //Match sweep to previous wing, if desired
                if (S.MatchSweep && PrevSection != null)
                {
                    float PrevSectionDistance = Mathf.Abs(PrevSection.TipChord.CenterPoint.x - PrevSection.RootChord.CenterPoint.x);
                    float CurSectionDistance = Mathf.Abs(S.TipChord.CenterPoint.x - S.RootChord.CenterPoint.x);
                    float Slope = (PrevSection.TipChord.Position.z - PrevSection.RootChord.Position.z) / PrevSectionDistance;
                    S.TipChord.Position.z = PrevSection.TipChord.Position.z + (Slope * CurSectionDistance);
                }

                S.RecalculateChords(); //Update all the structs to reflect the new, script-driven information

                //Carve out control surfaces
                if (S.HasControlSurface)
                {
                    float ChordMargin = 1 - S.ControlSurfaceChord;
                    Vector3 RootVector = Vector3.Lerp(S.RootChord.LeadPoint, S.RootChord.TrailPoint, ChordMargin);
                    Vector3 TipVector = Vector3.Lerp(S.TipChord.LeadPoint, S.TipChord.TrailPoint, ChordMargin);
                    Vector3 CSRoot = transform.TransformPoint(RootVector);
                    Vector3 CSTip = transform.TransformPoint(TipVector);

                    Debug.DrawLine(CSRoot, CSTip, new Color(255, 255, 0));

                    //S.RootChord.TrailPoint = RootVector;
                    //S.TipChord.TrailPoint = TipVector;

                    Vector3 CSTEr = transform.TransformPoint(RootVector + new Vector3(0, (Mathf.Cos(S.ControlSurfaceInput - 1.57f) * (S.ControlSurfaceChord * S.RootChord.ChordLength)), (Mathf.Sin(S.ControlSurfaceInput - 1.57f) * (S.ControlSurfaceChord * S.RootChord.ChordLength))));
                    Vector3 CSTEt = transform.TransformPoint(TipVector + new Vector3(0, (Mathf.Cos(S.ControlSurfaceInput - 1.57f) * (S.ControlSurfaceChord * S.TipChord.ChordLength)), (Mathf.Sin(S.ControlSurfaceInput - 1.57f) * (S.ControlSurfaceChord * S.TipChord.ChordLength))));
                    //S.RootChord.TrailPoint = RootVector + new Vector3(0, Mathf.Cos(S.ControlSurfaceInput - 1.57f), Mathf.Sin(S.ControlSurfaceInput - 1.57f));
                    //S.TipChord.TrailPoint = TipVector + new Vector3(0, Mathf.Cos(S.ControlSurfaceInput - 1.57f), Mathf.Sin(S.ControlSurfaceInput - 1.57f));

                    Debug.DrawLine(CSRoot, CSTEr, new Color(255, 255, 0));
                    Debug.DrawLine(CSTip, CSTEt, new Color(255, 255, 0));
                    Debug.DrawLine(CSTEr, CSTEt, new Color(255, 255, 0));

                }
                else
                {
                    S.ControlSurfaceChord = 0;
                }

                //S.RecalculateChords();


                //Divide Section into elements
                float SegmentDistance = Vector3.Distance(S.RootChord.Position, S.TipChord.Position); //How long is this particular wing segment
                float RelativeScale = SegmentDistance / Span; //What fraction of the whole wing (spanwise) is the current section?
                int SegmentCount = (int)Mathf.RoundToInt(Resolution * RelativeScale); //How many segments we're going to split this particular wing section into (a function of resolution and section size), so that the overall wing matches the requested resolution.
                SegmentCount = (int)Mathf.Clamp(SegmentCount, 1, Mathf.Infinity);

                for (int i = 0; i <= SegmentCount; i++)
                {
                    Vector3 LocalLP = Vector3.Lerp(S.RootChord.LeadPoint, S.TipChord.LeadPoint, (float)i / (float)SegmentCount);
                    Vector3 LocalTP = Vector3.Lerp(S.RootChord.TrailPoint, S.TipChord.TrailPoint, (float)i / (float)SegmentCount);
                    Vector3 LeadPoint = transform.TransformPoint(LocalLP);
                    Vector3 TrailPoint = transform.TransformPoint(LocalTP);
                    Debug.DrawLine(LeadPoint, TrailPoint, new Color(0, 255, 255));
                    if (i < SegmentCount)
                    {
                        Vector3 LocalNL = Vector3.Lerp(S.RootChord.LeadPoint, S.TipChord.LeadPoint, ((float)i + 1) / (float)SegmentCount);
                        Vector3 LocalNT = Vector3.Lerp(S.RootChord.TrailPoint, S.TipChord.TrailPoint, ((float)i + 1) / (float)SegmentCount);
                        Vector3 NextLead = transform.TransformPoint(LocalNL);
                        Vector3 NextTrail = transform.TransformPoint(LocalNT);
                        Debug.DrawLine((LeadPoint + TrailPoint) / 2, (NextLead + NextTrail) / 2, new Color(255, 0, 255));

                        //Create elements
                        if (S.Elements.Count < SegmentCount)
                        {
                            GameObject G = new GameObject();
                            Transform OT = G.transform;
                            G.name = "Velocity Tracker";
                            float NormalInvert = 1;
                            if (S.InvertNormal)
                            {
                                NormalInvert = -1;
                            }

                            Element E = new Element(LocalLP, LocalNL, LocalTP, LocalNT, S.ControlSurfaceChord, OT, CLc, CDc, NormalInvert);
                            E.T.position = transform.TransformPoint(E.Center);
                            E.T.parent = transform;
                            E.T.LookAt((LeadPoint + NextLead) / 2, Vector3.Cross(E.SparVec, (E.LeadRoot - E.TrailRoot) * NormalInvert));
                            E.Falloff = Vector3.Magnitude(E.Center) / Span;
                            

                            if (S.HasControlSurface && S.Link != null)
                            {
                                E.CLink = S.Link;

                            }

                            S.Elements.Add(E);
                        }

                    }



                }




                Vector3 RL = transform.TransformPoint(S.RootChord.LeadPoint);
                Vector3 RT = transform.TransformPoint(S.RootChord.TrailPoint);
                Vector3 TL = transform.TransformPoint(S.TipChord.LeadPoint);
                Vector3 TT = transform.TransformPoint(S.TipChord.TrailPoint);

                Debug.DrawLine(RL, RT);
                Debug.DrawLine(TL, TT);
                Debug.DrawLine(TL, RL, Color.green);
                Debug.DrawLine(TT, RT, Color.red);

                foreach(Element EM in S.Elements)
                {
                    Handles.Label(EM.T.TransformPoint(0, 1, 0), "" + EM.Area);
                }

            }
        }

        


    }

    void OnValidate()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            RefreshWing();
        }
        
        
    }

    void RefreshWing()
    {
        if (!Application.isPlaying)
        {
            //Destroy all the elements any time we change a parameter so they repopulate
            foreach (WingSection S in Sections)
            {
                foreach (Element E in S.Elements)
                {
                    if (E.T != null)
                    {


                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            DestroyImmediate(E.T.gameObject);

                        };
                    }

                }
                S.Elements.Clear();
            }
        }
    }

    void FixedUpdate()
    {
        ExecuteLift();
    }

    void ExecuteLift()
    {
        foreach(WingSection S in Sections)
        {
            foreach(Element E in S.Elements)
            {
                Rigidbody R = gameObject.GetComponentInParent<Rigidbody>();
                Vector3 GlobalSpeed = R.GetPointVelocity(E.T.position);
                Vector3 LocalSpeed = E.T.InverseTransformVector(GlobalSpeed);
                float AoA = Mathf.Atan2(-LocalSpeed.y, LocalSpeed.z) * Mathf.Rad2Deg;
                float CL = 0;
                float CD = E.CDcurve.Evaluate(AoA);
                float UCL = E.CLcurve.Evaluate(AoA);
                float DeflCL = UCL; //Default DeflCL to UCL so there's no delta unless we change it later.
                float DeflCD = CD;

                Vector3 CenterRoot = Vector3.Lerp(E.CenterLeading, E.CenterTrailing, 1);
                if (E.CSChordFraction > 0) //CL code for present control surface
                {
                    //Modifying the CL relationship
                    //Based on https://aviation.stackexchange.com/questions/8645/how-should-control-surfaces-be-modeled-in-simulations
                    //Not quite good enough on its own because a wing starting with 0 CL (symmetrical and straight) will never gain any lift from deflection.
                    //So here we're modifying the AoA to measure a chord line from the leading edge to the trailing edge of the control surface

                    float ChordMargin = 1 - E.CSChordFraction;
                    CenterRoot = Vector3.Lerp(E.CenterLeading, E.CenterTrailing, ChordMargin);
                    Vector3 CSTE = CenterRoot + new Vector3(0, (Mathf.Cos(S.Link.Input - 1.57f) * (S.ControlSurfaceChord * E.CenterChordDistance)), (Mathf.Sin(S.Link.Input - 1.57f) * (S.ControlSurfaceChord * E.CenterChordDistance)));
                    Debug.DrawLine(transform.TransformPoint(CenterRoot) + (GlobalSpeed * Time.fixedDeltaTime), transform.TransformPoint(CSTE) + (GlobalSpeed * Time.fixedDeltaTime), Color.yellow);

                    float AdAoA = Mathf.Atan2(-CenterRoot.y + CSTE.y, E.CenterChordDistance) * Mathf.Rad2Deg;
                    DeflCL = E.CLcurve.Evaluate(AoA + (-AdAoA / 1)); //Control surface deflection changes camber, which is expressed here identically to a shift in AoA since it effectively is. AoA is increased by the angle drawn between the normal chord line and a line drawn from the leading edge to the trailing edge of the surface 
                    DeflCD = E.CDcurve.Evaluate(AoA + (-AdAoA / 1));
                    //print(-CenterRoot.y + CSTE.y);

                    AoA = AoA + (-AdAoA); //Update AoA so we can refer to resultant AoA outside this conditional


                    //float DCL = Mathf.Sqrt(E.CSChordFraction) * UCL * Mathf.Sin(-S.Link.Input); //This expresses delta coefficient lift from inputs
                    //^^Something is weird with this figure. Reversed on the ailerons but not elevator. Very strange.
                    //CL = UCL + DCL;
                    CL = DeflCL;
                    CD = DeflCD;

                }
                else //CL code for absent control surface
                {
                    CL = UCL;
                }

                Vector3 CenterOfPressure = Vector3.Lerp(E.Center + new Vector3(0, 0, E.CenterChordDistance / 4), E.Center + new Vector3(0, 0, -E.CenterChordDistance / 6), Mathf.Clamp(1 - Mathf.Abs(AoA) / 15, 0, 1)); //It would probaby be best to find the max of the cl curve, because we should reach the quarter-chord point at max lift. Better than using a fixed 15, but additional overhead to find max of the curve
          
                //float Falloff = E.Falloff;
                //print(Falloff);
                float Falloff = 0;
                float density = 1.2754f;
                float Lift = 0.5f * density * Mathf.Pow(GlobalSpeed.magnitude, 2) * E.Area * (CL) * (1 - Falloff);
                float ULift = 0.5f * density * Mathf.Pow(GlobalSpeed.magnitude, 2) * E.Area * (UCL) * (1 - Falloff); //Lift untouched by control surface shape changes
                float DeltaLift = Lift - ULift;

                float Drag = 0.5f * density * Mathf.Pow(GlobalSpeed.magnitude, 2) * E.Area * (CD);

                Vector3 EffectiveAoA = (Vector3.Normalize((Vector3.Cross(-GlobalSpeed, transform.TransformVector(E.SparVec))) * E.NormalInversion) + E.T.up) / 2;
                //Debug.DrawRay(transform.TransformPoint(CenterOfPressure) + (GlobalSpeed * Time.fixedDeltaTime), EffectiveAoA * 5, Color.yellow);
                //Debug.DrawRay(transform.TransformPoint(CenterOfPressure) + (GlobalSpeed * Time.fixedDeltaTime), E.T.up * 5, Color.red);

                Debug.DrawRay(transform.TransformPoint(CenterOfPressure) + (GlobalSpeed * Time.fixedDeltaTime), Vector3.Normalize(EffectiveAoA) * (1 * Lift / 4000), Color.cyan);
                R.AddForceAtPosition(Vector3.Normalize(EffectiveAoA) * Lift * 1, transform.TransformPoint(CenterOfPressure));

                Debug.DrawRay(transform.TransformPoint(CenterOfPressure) + (GlobalSpeed * Time.fixedDeltaTime), Vector3.Normalize(-GlobalSpeed) * (Drag / 4000), Color.red);
                R.AddForceAtPosition(Vector3.Normalize(-GlobalSpeed) * Drag, transform.TransformPoint(CenterOfPressure));

                R.AddRelativeTorque(new Vector3(DeltaLift, 0, 0) * Vector3.Distance(E.Center, CenterRoot));
                //print(AoA);
                //print(GlobalSpeed.magnitude);
            }
        }

    }

}



[System.Serializable]
public class Element
{
    //Input variables
    public Vector3 LeadRoot;
    public Vector3 LeadTip;
    public Vector3 TrailRoot;
    public Vector3 TrailTip;
    public float CSChordFraction;
    public Transform T;
    public AnimationCurve CLcurve;
    public AnimationCurve CDcurve;
    public ControlLink CLink;
    public float NormalInversion;
    public float Falloff;

    //Driven (but accessible) variables
    
    public Vector3 Center;
    public Vector3 LeVec; //Leading edge as defined by a vector
    public Vector3 TeVec; //Trailing edge as defined by a vector
    public Vector3 SparVec; //Average of Leading and Trailing vectors
    public Vector3 CenterLeading; //Middle of the leading edge
    public Vector3 CenterTrailing; //Middle of the Trailing edge
    public float CenterChordDistance; //Distance from the middle of the leading to middle of the trailing edge
    public float Area;

    public Element(Vector3 LeadRoot, Vector3 LeadTip, Vector3 TrailRoot, Vector3 TrailTip, float CSChordFraction, Transform T, AnimationCurve CLcurve, AnimationCurve CDcurve, float NormalInversion)
    {
        //Basic attribution stuff
        this.LeadRoot = LeadRoot;
        this.LeadTip = LeadTip;
        this.TrailRoot = TrailRoot;
        this.TrailTip = TrailTip;
        this.CLcurve = CLcurve;
        this.CDcurve = CDcurve;
        this.CSChordFraction = CSChordFraction;
        this.T = T;
        this.NormalInversion = NormalInversion;

        //Driven variables
        Center = (LeadRoot + LeadTip + TrailRoot + TrailTip) / 4;
        LeVec = LeadTip - LeadRoot;
        TeVec = TrailTip - TrailRoot;
        SparVec = ((LeadTip + TrailTip) / 2) - ((LeadRoot + TrailRoot) / 2);
        CenterLeading = (LeadRoot + LeadTip) / 2;
        CenterTrailing = (TrailRoot + TrailTip) / 2;
        CenterChordDistance = Vector3.Distance(CenterLeading, CenterTrailing);

        //Area Calculation

        Vector3 Hypotenuse = (TrailTip - LeadRoot);
        float theta1 = Vector3.Angle(LeVec, Hypotenuse) * Mathf.Deg2Rad;
        float theta2 = Vector3.Angle(TeVec, Hypotenuse) * Mathf.Deg2Rad;

        float b1 = Vector3.Distance(LeadTip, LeadRoot); //Length of the leading edge
        float b2 = Vector3.Distance(TrailTip, TrailRoot); //length of the trailing edge
        float hyplength = Vector3.Distance(TrailTip, LeadRoot); //length of the hypotenuse

        float Tri1Area = 0.5f * hyplength * b1 * Mathf.Sin(theta1);
        float Tri2Area = 0.5f * hyplength * b2 * Mathf.Sin(theta2);

        Area = Tri1Area + Tri2Area; //Total area is the sum of the two triangles we split it into to run the numbers

        //Area calculation finished----

    }
}

[System.Serializable]
public class WingSection
{
    [HideInInspector]
    public Vector3 LeadingEdgeVector;
    [HideInInspector]
    public Vector3 TrailingEdgeVector;
    [HideInInspector]
    public Vector3 SparVector;

    public ChordLine RootChord;
    public ChordLine TipChord;
    public bool InheritRoot; //Whether or not we should use the tip of the last segment as the root of this one.
    public bool MatchDihedral;
    public bool MatchTaper;
    public bool MatchSweep;

    public bool HasControlSurface;
    public float ControlSurfaceChord;
    public float ControlSurfaceInput;
    public ControlLink Link;

    public bool InvertNormal;
    //public bool NegateInheritance;

    [HideInInspector]
    public Vector3 Normal;
    [HideInInspector]
    public Vector3 Center;
    [HideInInspector]
    public Transform Origin;

    public List<Element> Elements;

    [HideInInspector]
    public WingSection PreviousSection;

    public WingSection()
    {
        RootChord = new ChordLine();
        TipChord = new ChordLine();
        Elements = new List<Element>();

    }

    public void RecalculateChords()
    {
        ChordLine PrevRoot = RootChord;
        ChordLine PrevTip = TipChord;

        RootChord = new ChordLine(PrevRoot.Position, PrevRoot.ChordLength, PrevRoot.WashRot);
        TipChord = new ChordLine(PrevTip.Position, PrevTip.ChordLength, PrevTip.WashRot);
    }

    [System.Serializable]
    public struct ChordLine
    {
        [HideInInspector]
        public Vector3 LeadPoint;
        public Vector3 Position;
        public float ChordLength;
        [HideInInspector]
        public float WashRot;

        [HideInInspector]
        public Vector3 TrailPoint;
        [HideInInspector]
        public Vector3 CenterPoint;

        public ChordLine(Vector3 Position, float ChordLength, float WashRot)
        {
            this.Position = Position;
            float WashRad = ((WashRot + 0) * Mathf.Deg2Rad);
            Vector3 LP = Position;
            this.ChordLength = ChordLength;
            this.WashRot = WashRot;
            Vector3 TP = Position - new Vector3(0,0,ChordLength);

            //Rotate for Washout
            CenterPoint = (LP + TP) / 2;
            this.LeadPoint = CenterPoint + new Vector3(0, (ChordLength / 2) * Mathf.Sin(WashRad), (ChordLength / 2) * Mathf.Cos(WashRad));
            this.TrailPoint = CenterPoint - new Vector3(0, (ChordLength / 2) * Mathf.Sin(WashRad), (ChordLength / 2) * Mathf.Cos(WashRad));
            //TrailPoint = ChordCenter;

        }


    } 
}
