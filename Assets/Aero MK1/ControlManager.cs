using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    public float PitchTrim;
    public List<ControlGovernor> Elevators = new List<ControlGovernor>();
    public List<ControlGovernor> Ailerons = new List<ControlGovernor>();

    public List<ControlLink> ElevatorLinks = new List<ControlLink>();
    public List<ControlLink> AileronLinks = new List<ControlLink>();

    public List<PropEngine> Engines = new List<PropEngine>();

    private float PitchEntry;
    private float RollEntry;

    public float SpeedAileron;
    public float SpeedElevator;
    public float ElevGain;
    public float AilGain;
    public float ThrotGain;
    public float Throttle;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StickAnimator[] Sticks = GetComponentsInChildren<StickAnimator>();

        float wd = Input.GetKey(KeyCode.W) ? 0 : 1;
        float sd = Input.GetKey(KeyCode.S) ? 0 : 1;
        float ad = Input.GetKey(KeyCode.A) ? 0 : 1;
        float dd = Input.GetKey(KeyCode.D) ? 0 : 1;
        float shiftd = Input.GetKey(KeyCode.LeftShift) ? 0 : 1;
        float cntrlD = Input.GetKey(KeyCode.LeftControl) ? 0 : 1;

        float RequestedPitch = (wd - sd) * ElevGain;
        float RequestedRoll = (ad - dd) * AilGain;
        float ThrottleCommand = (cntrlD - shiftd) * ThrotGain;

        PitchEntry = Mathf.Lerp(PitchEntry, RequestedPitch + (PitchTrim / 10), SpeedElevator);
        RollEntry = Mathf.Lerp(RollEntry, RequestedRoll, SpeedAileron);
        Throttle += ThrottleCommand;
        Throttle = Mathf.Clamp(Throttle, 0, 100);

        foreach(ControlGovernor G in Elevators)
        {
            G.ControlInput = PitchEntry;
        }

        foreach (ControlGovernor G in Ailerons)
        {
            G.ControlInput = RollEntry;
        }

        foreach (ControlLink G in ElevatorLinks)
        {
            G.mIn = PitchEntry;
        }

        foreach (ControlLink G in AileronLinks)
        {
            G.mIn = RollEntry;
        }

        foreach (PropEngine P in Engines)
        {
            P.Throttle = Throttle;
        }

        foreach (StickAnimator S in Sticks)
        {
            S.PitchAxis = PitchEntry;
            S.RollAxis = RollEntry;
        }


    }
}
