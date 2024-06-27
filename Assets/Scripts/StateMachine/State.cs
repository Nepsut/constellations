using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public abstract class State : MonoBehaviour
    {
        //state handlers
        public bool isComplete { get; protected set; } = false;
        protected float startTime;
        public float time => Time.time - startTime;

        //define these in inheriting classes
        protected bool grounded;
        protected float horizontal;
        protected float vertical;

        protected StateMachineCore core;

        public virtual void Enter() { }
        public virtual void Do() { }
        public virtual void FixedDo() { }
        public virtual void Exit() { }

        public void SetCore(StateMachineCore _core)
        {
            core = _core;
        }

        public void Initialize()
        {
            isComplete = false;
            startTime = Time.time;
        }
    }
}