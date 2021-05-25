using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicForcer : MonoBehaviour
{
    public float Mult;
    public Vector3 Force;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * Mult);
        //gameObject.GetComponent<Rigidbody>().AddForce(Force * Mult);
    }
}
