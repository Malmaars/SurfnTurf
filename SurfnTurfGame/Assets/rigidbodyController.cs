using System.Collections.Generic;
using UnityEngine;

public class rigidbodyController : MonoBehaviour
{
    //Experiment:
    //This script uses the velocity of the rigidbody, and keeps track of every source that input to the velocity,
    //this way it keeps full control of the rigidbody

    //keep track of all sources of velocity
    Dictionary<string, Vector3> velocitySources = new Dictionary<string, Vector3>(); 

    public void addVelocity(string keyName, Vector3 newVelocity)
    {

    }
}
