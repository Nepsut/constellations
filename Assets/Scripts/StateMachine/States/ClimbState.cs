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
            // animator.Play(anim.name);
        }

        public override void Do()
        {
            core.animator.speed = Helpers.Map(PlayerController.maxClimbSpeed, 0, 1, 0, 1, true);

            if (!core.climbing || Mathf.Abs(core.rb2d.velocity.x) < 0.1f)
            isComplete = true;
        }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}