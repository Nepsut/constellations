using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "AirState")]
    public class AirState : State
    {
        public float jumpSpeed;

        public override void Enter()
        {
            // animator.Play("Jump");
        }

        public override void Do()
        {
            float time = Helpers.Map(rb2d.velocity.y, jumpSpeed, -jumpSpeed, 0, 1, true);
            // animator.Play("Jump", 0, time);
            // animator.speed = 0;

            if (input.grounded)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            
        }
    }
}