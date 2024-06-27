using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace constellations
{
    public class AirState : State
    {
        public float jumpSpeed;
        [SerializeField] private AnimationClip anim;

        public override void Enter()
        {
            // animator.Play(anim.name);
        }

        public override void Do()
        {
            float time = Helpers.Map(core.rb2d.velocity.y, jumpSpeed, -jumpSpeed, 0, 1, true);
            // animator.Play(anim.name, 0, time);
            // animator.speed = 0;

            if (core.groundSensor.grounded)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            
        }
    }
}