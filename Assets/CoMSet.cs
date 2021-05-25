using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CoMSet : MonoBehaviour
{
    public Vector3 CoMLocation;

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            gameObject.GetComponent<Rigidbody>().centerOfMass = CoMLocation;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(CoMLocation), 1);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(GetComponent<Rigidbody>().centerOfMass), 1);
        }
        
    }

}
