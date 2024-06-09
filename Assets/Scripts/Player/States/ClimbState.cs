using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "ClimbState")]
    public class ClimbState : State
    {
        public override void Enter()
        {
            // animator.Play("Climb");
        }

        public override void Do()
        {
            animator.speed = Helpers.Map(PlayerController.maxClimbSpeed, 0, 1, 0, 1, true);

            if (!input.climbing || Mathf.Abs(rb2d.velocity.x) < 0.1f)
            isComplete = true;
        }

        public override void Exit()
        {
            animator.speed = 1;
        }
    }
}