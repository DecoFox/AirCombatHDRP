using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotCam : MonoBehaviour
{
    private GameObject Cam;
    private float AccX;
    private float AccY;
    private float LX;
    private float LY;
    private bool IsHolding;
    private Vector3 InitialPos;
    public float InertiaStrength;
    private Vector3 LastVelocity;
    private Rigidbody R;
    private Vector3 InertiaAcceleration;

    public Vector3 UpVector;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        Cam = GetComponentInChildren<Camera>().gameObject;
        UpVector = transform.up;
        R = GetComponentInParent<Rigidbody>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        bool RightClick = Input.GetMouseButton(1);
        if (RightClick)
        {
            if (!IsHolding)
            {
                InitialPos = Input.mousePosition;
                IsHolding = true;
            }

            InitialPos = Vector3.Lerp(InitialPos, Input.mousePosition, 0.8f);

            float DX = (Input.mousePosition - InitialPos).x * 15;
            float DY = (Input.mousePosition - InitialPos).y * 15;

            AccX += (DX * speed) / 10000;
            AccY += (DY * speed) / 10000;
            //print(DX + "|" + DY);

        }
        else
        {
            IsHolding = false;
        }

        AccX = Mathf.Clamp(AccX, -2.5f, 2.5f);
        AccY = Mathf.Clamp(AccY, -1, 1);

        LX = Mathf.Lerp(LX, AccX, 0.1f);
        LY = Mathf.Lerp(LY, AccY, 0.1f);

        UpVector = Vector3.Lerp(UpVector, transform.up, 1 - (InertiaStrength / 1.1f));

        Vector3 CurrentVelocity = R.GetPointVelocity(transform.position); //Maybe use the camera rather than the cam master. Probably more stable from the master though.
        Vector3 Acceleration = (LastVelocity - CurrentVelocity) * Time.fixedDeltaTime;
        LastVelocity = CurrentVelocity;

        Acceleration = new Vector3(Mathf.Clamp(Acceleration.x, -100, 100), Mathf.Clamp(Acceleration.y, -100, 100), Mathf.Clamp(Acceleration.z, -100, 100));

        if(InertiaAcceleration.magnitude < Acceleration.magnitude)
        {
            InertiaAcceleration = Vector3.Lerp(InertiaAcceleration, Acceleration * 10, 0.2f);
        }
        else
        {
            InertiaAcceleration = Vector3.Lerp(InertiaAcceleration, Acceleration * 10, 0.05f);
        }
        

        Cam.transform.LookAt(transform.TransformPoint(Mathf.Sin(LX) * 5, Mathf.Sin(LY) * 5, Mathf.Cos(LX) * 5), UpVector);
        Cam.transform.position = transform.TransformPoint(InertiaAcceleration * InertiaStrength);


    }
}
