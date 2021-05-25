using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorqueSim : MonoBehaviour
{
    public Vector3 Torque;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.GetComponent<Rigidbody>().AddTorque(transform.TransformVector(Torque));
    }
}
