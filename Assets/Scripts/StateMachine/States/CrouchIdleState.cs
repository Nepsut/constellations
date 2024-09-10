using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public class CrouchIdleState : State
    {
        [SerializeField] private AnimationClip anim;
        
        public override void Enter()
        {
            core.animator.Play(anim.name); 
        }

        public override void Do()
        {
            if (!core.crouching) isComplete = true;
        }
    }
}