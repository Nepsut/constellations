using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class IdleState : State
    {
        [SerializeField] private AnimationClip anim;

        public override void Enter()
        {
            core.animator.Play(anim.name);
        }

        public override void Do()
        {
            if (core.groundSensor != null)
            {
                if (!core.groundSensor.grounded) isComplete = true;
            }
        }
    }
}