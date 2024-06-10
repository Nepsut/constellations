using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "RunState")]
    public class RunState : State
    {
        public override void Enter()
        {
            // animator.Play("Run");
        }

        public override void Do()
        {
            animator.speed = Helpers.Map(PlayerController.maxSpeed, 0, 1, 0, 1, true);

            if (!input.running || !input.grounded || Mathf.Abs(rb2d.velocity.x) <= PlayerController.maxSpeed)
            isComplete = true;
        }

        public override void Exit()
        {
            animator.speed = 1;
        }
    }
}