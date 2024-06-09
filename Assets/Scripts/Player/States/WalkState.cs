using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "WalkState")]
    public class WalkState : State
    {
        public override void Enter()
        {
            animator.Play("Walk");
        }

        public override void Do()
        {
            animator.speed = Helpers.Map(PlayerController.maxSpeed, 0, 1, 0, 1, true);

            if (!input.grounded || Mathf.Abs(rb2d.velocity.x) < 0.1f)
            isComplete = true;
        }

        public override void Exit()
        {
            animator.speed = 1;
        }
    }
}