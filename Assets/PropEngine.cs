using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropEngine : MonoBehaviour
{
    //[ExecuteInEditMode]
    public float PropDiameter = 3.35f;
    public float PropRPM = 3000;
    public AnimationCurve PropEfficiency;
    public float AdvanceRatio;

    public float KWPower;
    //public float FreeVelocity;
    //public float TerminalVelocity;
    public float CurrentVelocity;
    public float Knots;


    public float Thrust;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        
        /*
            In calculus terms, power is the derivative of work with respect to time. ... 
            Since work is force times displacement (W=F*d), and velocity is displacement over time (v=d/t), 
            power equals force times velocity: P = F*v.Jul 15, 2019
         */

        //Force = Mass * Acceleration
        //Propeller must accelerate air from the free velocity to a final velocity by doing work to it
        //Power = Force * Velocity

        //So, Force = Power / Velocity


        /*
        So say we have an engine capable of outputting 150kw, or 150,000 joules per second
        For now we'll assume perfect efficiency and say the engine can, by way of the prop, apply 150,000 joules of work to the air
        what final velocity can we attain
        This whole concept is losing me where force and work meet
        */
        float Power = KWPower * 1000;

        Rigidbody R = gameObject.GetComponentInParent<Rigidbody>();
        CurrentVelocity = R.velocity.magnitude;

        AdvanceRatio = CurrentVelocity / (PropDiameter * (PropRPM / 60));
        float Efficiency = PropEfficiency.Evaluate(AdvanceRatio);
        //Power = Force * Velocity;
        //Force = Velocity / Power;
        Thrust = (Power / Mathf.Clamp(CurrentVelocity, 1.0f, Mathf.Infinity)) * Efficiency;


        Knots = CurrentVelocity * 1.944f;
        R.AddForce(R.transform.forward * Thrust);

    }
}
