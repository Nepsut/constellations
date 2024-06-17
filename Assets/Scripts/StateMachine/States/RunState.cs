using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class RunState : State
    {
        public override void Enter()
        {
            // animator.Play("Run");
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(PlayerController.maxSpeed, 0, 1, 0, 1, true);

            if (!core.input.running)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}