using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class DashState : State
    {
        public override void Enter()
        {
            // animator.Play("Dash");
        }

        public override void Do()
        {
            if (time >= PlayerController.dashDecelerationIgnore)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}