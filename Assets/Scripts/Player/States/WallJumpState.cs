using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "WallJumpState")]
    public class WallJumpState : State
    {
        float time;

        public override void Enter()
        {
            // animator.Play("WallJump");
        }

        public override void Do()
        {
            time += Time.deltaTime;

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