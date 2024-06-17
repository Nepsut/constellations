using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class SlideState : State
    {
        public override void Enter()
        {
            // animator.Play("Slide");
        }

        public override void Do()
        {
            if (!core.input.sliding) isComplete = true;
        }
    }
}