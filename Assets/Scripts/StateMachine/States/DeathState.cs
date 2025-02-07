using UnityEngine;

namespace constellations
{
    public class DeathState : State
    {
        [SerializeField] private AnimationClip anim;

        public float animLength
        {
            get { return anim.length; }
        }

        public override void Enter()
        {
            core.animator.Play(anim.name);
        }

        public override void Do() { }

        public override void Exit()
        {
            core.animator.speed = 1;
        }
    }
}