using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CrouchState : State
    {
        public override void Enter()
        {
            // animator.Play("Crouch");
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(PlayerController.maxSpeed * PlayerController.crouchSpeedMult,
            0, 1, 0, 1, true);

            if (!core.input.crouching)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}