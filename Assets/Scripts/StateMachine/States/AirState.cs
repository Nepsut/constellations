using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace constellations
{
    public class AirState : State
    {
        public float jumpSpeed;
        private float animTime;
        [SerializeField] private AnimationClip anim;

        public override void Enter()
        {
            core.animator.Play(anim.name);
        }

        public override void Do()
        {
            animTime = Helpers.Map((core.rb2d.velocity.y), jumpSpeed, -jumpSpeed, 0, 1, true);
            core.animator.speed = 0;
            core.animator.Play(anim.name, 0, animTime);

            if (core.groundSensor.grounded)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            core.animator.Play(anim.name, 0, time);
            core.animator.speed = 1;
        }
    }
}