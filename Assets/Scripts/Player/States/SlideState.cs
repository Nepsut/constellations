using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    [CreateAssetMenu(menuName = "SlideState")]
    public class SlideState : State
    {
        public override void Enter()
        {
            // animator.Play("Slide");
        }

        public override void Do()
        {
            if (!input.sliding) isComplete = true;
        }
    }
}