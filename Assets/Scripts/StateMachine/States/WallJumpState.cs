using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace constellations
{
    public class WallJumpState : State
    {
        public override void Enter()
        {
            // animator.Play("WallJump");
        }

        public override void Do()
        {
            if (time >= PlayerController.jumpMaxDuration)
            {
                isComplete = true;
            }
        }

        public override void Exit()
        {
            
        }
    }
}