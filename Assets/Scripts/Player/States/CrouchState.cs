using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "CrouchState")]
    public class CrouchState : State
    {
        public override void Enter()
        {
            // animator.Play("Crouch");
        }

        public override void Do()
        {
            animator.speed = Helpers.Map(PlayerController.maxSpeed * PlayerController.crouchSpeedMult,
            0, 1, 0, 1, true);

            if (!input.crouching || !input.grounded || Mathf.Abs(rb2d.velocity.x) < 0.1f)
            isComplete = true;
        }

        public override void Exit()
        {
            animator.speed = 1;
        }
    }
}