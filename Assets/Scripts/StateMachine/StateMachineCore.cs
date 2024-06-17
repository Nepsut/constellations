using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace constellations
{
    public abstract class StateMachineCore : MonoBehaviour
    {
        public Rigidbody2D rb2d;
        public Animator animator;
        public StateMachine machine;
        public PlayerController input;

        public void SetupInstances()
        {
            machine = new StateMachine();
            State[] allChildStates = GetComponentsInChildren<State>();
            foreach (State state in allChildStates)
            {
                state.SetCore(this);
            }
        }
    }
}