using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class StateMachine
    {
        public State state;
        
        public void Set(State newState, bool forceReset = false)
        {
            if (state != newState || forceReset)
            {
                state?.Exit();
                state = newState;
                state.Initialize();
                state.Enter();
            }
        }
    }
}