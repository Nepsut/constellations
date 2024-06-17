using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class SwimIdleState : State
    {
        public override void Enter()
        {
            // animator.Play("SwimIdle");
        }

        public override void Do()
        {
            if (!core.input.swimming) isComplete = true;
        }
    }
}