using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class LiftManager : MonoBehaviour
{
    public Dictionary<Transform, Airfoil.LiftSurface> SurfaceDictionary = new Dictionary<Transform, Airfoil.LiftSurface>();
    public Dictionary<Transform, Vector3> SpeedDictionary = new Dictionary<Transform, Vector3>();
    private Rigidbody Self;

    public ComputeShader CS;
    // Start is called before the first frame update
    void Start()
    {
        Self = GetComponent<Rigidbody>();
        Airfoil[] Foils = gameObject.GetComponentsInChildren<Airfoil>();
        foreach(Airfoil F in Foils)
        {
            List<Airfoil.LiftSurface> Sfc = F.BuildLiftSurfaces();
            foreach(Airfoil.LiftSurface s in Sfc)
            {
                GameObject G = new GameObject();
                G.transform.position = transform.TransformPoint(s.Center);
                G.transform.rotation = F.transform.rotation * Quaternion.FromToRotation(F.transform.up, s.Normal);
                G.transform.parent = F.gameObject.transform;
                SurfaceDictionary.Add(G.transform, s);
                SpeedDictionary.Add(G.transform, new Vector3(0,0,0));
            }
            
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        LiftUpdate();
    }

    void LiftUpdate()
    {
        //print(GetComponent<Rigidbody>().velocity.magnitude * 1.94384f);
        List<Airfoil.LiftSurface> SV = new List<Airfoil.LiftSurface>();
        foreach (Transform t in SurfaceDictionary.Keys)
        {
            Airfoil.LiftSurface Val = SurfaceDictionary[t];

            Vector3 TotalVel = Self.GetPointVelocity(t.position);

            Vector3 LocalVel = t.InverseTransformVector(TotalVel);

            Vector3 WorkingVel = Vector3.Lerp(LocalVel, SpeedDictionary[t], 0.50f); //A weird smoothing system. Doesn't work very well. Not currently in use. Assign struct speed to WorkingVel if use is desired. Basically just forces the speed measurement through an accumulator.

            Val.LocalVelocity = LocalVel;
            Val.WorldVelocity = TotalVel;

            SpeedDictionary[t] = WorkingVel;

            SV.Add(Val);
        }

        //print(SV.Count);
        Airfoil.LiftSurface[] SurfaceValues = SV.ToArray();

        //Size Calculation
        /*
        public Vector3 Center;
        public Vector3 Normal;
        public float Area;
        public float Health;
        public float Falloff;
        public Vector3 LocalVelocity;

        public float Lift;
        public float Drag;
        */

        int VectorSize = sizeof(float) * 3;
        int FloatSize = sizeof(float);
        int IntSize = sizeof(int);

        int TotalSize = (VectorSize * 5) + (FloatSize * 6) + IntSize;


        ComputeBuffer LiftBuffer = new ComputeBuffer(SurfaceValues.Length, TotalSize);
        LiftBuffer.SetData(SurfaceValues);
        CS.SetBuffer(0, "surfaces", LiftBuffer);
        CS.Dispatch(0, SurfaceValues.Length / 10, 1, 1); //Should divide by whatever X quantity of worker threads we use


        LiftBuffer.GetData(SurfaceValues);
        foreach (Airfoil.LiftSurface s in SurfaceValues)
        {
           
            Debug.DrawRay(transform.TransformPoint(s.Center) + (s.WorldVelocity * Time.fixedDeltaTime), Vector3.Normalize((Vector3.Cross(-s.WorldVelocity, transform.TransformDirection(s.SparVector)))) * ((s.Lift * Time.deltaTime) / 100), new Color(255,255 - (s.Symmetric * 255), 255 - (s.Symmetric * 255)));
            //Debug.DrawRay(transform.TransformPoint(s.Center), Vector3.Normalize(transform.TransformVector(-s.LocalVelocity)) * (s.Drag * Time.deltaTime) / 100, new Color(255, 255, 0));
            //Debug.DrawRay(transform.TransformPoint(s.Center), transform.TransformDirection(s.SparVector) * 5);

            //Self.AddForceAtPosition(transform.TransformVector((s.Normal)) * (s.Lift * Time.deltaTime), transform.TransformPoint(s.Center), ForceMode.Impulse);
            Self.AddForceAtPosition(Vector3.Normalize((Vector3.Cross(-s.WorldVelocity, transform.TransformDirection(s.SparVector)))) * (s.Lift * 1), transform.TransformPoint(s.Center), ForceMode.Force);
            Self.AddForceAtPosition(Vector3.Normalize(-s.WorldVelocity) * (s.Drag * 1), transform.TransformPoint(s.Center), ForceMode.Force);

            //Handles.Label(transform.TransformPoint(s.Center) + new Vector3(0, 0, 0), ""+s.CL);
            if(s.CL != 0)
            {
                //print(s.CL * 1.94384f + "|" + GetComponent<Rigidbody>().velocity.magnitude * 1.94384f);
                //print(s.Normal);
            }
            
        }

        LiftBuffer.Dispose();

    }


}
