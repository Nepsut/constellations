using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class ClimbState : State
    {
        [SerializeField] private AnimationClip anim;
        
        public override void Enter()
        {
            animator.Play(anim.name);
        }

        public override void Do()
        {
            animator.speed = Helpers.Map(core.rb2d.velocity.y, 0, StateMachineCore.maxClimbSpeed, 0, 1, true);
            if (core.rb2d.velocity.y >= 0)
            {
                animator.SetFloat("Direction", 1);
            }
            else
            {
                animator.SetFloat("Direction", -1);
            }
            if (!core.climbing || Mathf.Abs(core.rb2d.velocity.x) > 0.1f)
            isComplete = true;
        }

        public override void Exit()
        {
            animator.speed = 1;
        }
    }
}