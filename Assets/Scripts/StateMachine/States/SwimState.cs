using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class SwimState : State
    {
        public override void Enter()
        {
            // animator.Play("Swim");
        }

        public override void Do()
        {
            if (!core.input.swimming) isComplete = true;
        }
    }
}