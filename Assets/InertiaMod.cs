using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertiaMod : MonoBehaviour
{
    public Vector3 SetTensor = new Vector3(5000,5000,5000);
    // Start is called before the first frame update
    void Start()
    {
        //gameObject.GetComponent<Rigidbody>().inertiaTensor = gameObject.GetComponent<Rigidbody>().inertiaTensor / 24;
        SetTensor = GetComponent<Rigidbody>().inertiaTensor;


    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Rigidbody>().inertiaTensor = SetTensor;
    }
}
