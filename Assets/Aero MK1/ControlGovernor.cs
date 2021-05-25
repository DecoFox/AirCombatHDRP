using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControlGovernor : MonoBehaviour
{
    public float ControlInput;
    public float speed;
    public bool invert;
    private GameObject Ref;
    
    // Start is called before the first frame update
    void Start()
    {
        Ref = new GameObject();
        Ref.transform.position = transform.position;
        Ref.transform.rotation = transform.rotation;
        Ref.transform.parent = transform.parent;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion Target = Ref.transform.rotation * new Quaternion((ControlInput / 2) * (invert ? -1: 1), 0, 0, 1);
        transform.rotation = Quaternion.Lerp(transform.rotation, Target, speed);
        
    }
}
