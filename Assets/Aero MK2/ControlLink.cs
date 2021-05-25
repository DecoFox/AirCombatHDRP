using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlLink : MonoBehaviour
{

    /*
     * PURPOSE: 
     * Control surfaces are attributed on a wing section basis. We have no way of assigning values to specfic wing sections
     * other than navigating the wing's list of them by their indicies, which sucks to set up
     * We could, however, assign a governing script from the inspector which could be fed any value and use it to 
     * forward the relevant information since the Element object will have access to it
     */
    public float mIn;   
    public float Input; //In radians
    public string Name;
    public bool Invert;

    void Update()
    {
        if (Invert)
        {
            Input = -mIn;
        }
        else
        {
            Input = mIn;
        }
    }

    /*
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    */
}
