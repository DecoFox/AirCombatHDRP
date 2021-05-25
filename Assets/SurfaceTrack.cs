using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SurfaceTrack : MonoBehaviour
{
    public Transform TargetTransform;
    public Vector3 AxisOffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = TargetTransform.rotation;
        //transform.Rotate(AxisOffset, Space.Self); //Incorporates the offset, but we might be able to get around needing it.

    }


}
