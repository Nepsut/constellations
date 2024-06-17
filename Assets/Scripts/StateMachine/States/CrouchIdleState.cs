using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CrouchIdleState : State
    {
        public override void Enter()
        {
            // animator.Play("CrouchIdle");
        }

        public override void Do()
        {
            if (!core.input.crouching) isComplete = true;
        }
    }
}