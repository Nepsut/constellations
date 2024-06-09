using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace constellations
{
    public abstract class State : ScriptableObject
    {
        //state handlers
        public bool isComplete { get; protected set; } = false;

        //define these in inheriting classes
        protected Rigidbody2D rb2d;
        protected Animator animator;
        protected bool grounded;
        protected float horizontal;
        protected float vertical;
        protected PlayerController input;

        public virtual void Enter() { }
        public virtual void Do() { }
        public virtual void FixedDo() { }
        public virtual void Exit() { }

        public void Setup(Rigidbody2D _rb2d, Animator _animator, PlayerController _input)
        {
            rb2d = _rb2d;
            animator = _animator;
            input = _input;
        }
    }
}