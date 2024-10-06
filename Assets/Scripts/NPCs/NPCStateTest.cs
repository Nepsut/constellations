using System.Collections;
using System.Collections.Generic;
using constellations;
using UnityEngine;

public class NPCStateTest : StateMachineCore
{
    // Start is called before the first frame update
    void Start()
    {
        SetupInstances();
    }

    // Update is called once per frame
    void Update()
    {
        if (machine.state.isComplete)
        {

        }

        machine.state.Do();
    }
}
