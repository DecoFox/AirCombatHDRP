using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickAnimator : MonoBehaviour
{
    public float PitchAxis;
    public float RollAxis;
    public bool InvertPitch;
    public bool InvertRoll;
    private GameObject Ref;
    public float speed;

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
        Quaternion Target = Ref.transform.rotation * new Quaternion((PitchAxis / 4) * (InvertPitch ? -1 : 1), 0, (RollAxis / 4) * (InvertRoll ? -1 : 1), 1);
        transform.rotation = Quaternion.Lerp(transform.rotation, Target, speed);

    }
}