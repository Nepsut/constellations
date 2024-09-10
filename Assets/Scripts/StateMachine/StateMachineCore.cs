using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

namespace constellations
{
    public abstract class StateMachineCore : ClimbableSensor
    {
        [Header("Engine Variables")]
        public Rigidbody2D rb2d;
        public Animator animator;
        public StateMachine machine;
        public GroundSensor groundSensor;
        [SerializeField] protected Vector2 size = Vector2.one;
        public Vector2 offset = Vector2.zero;
        
        //STATE BOOLEANS
        public bool facingRight { get; protected set; } = true;
        public bool wallJumped { get; protected set; } = false;
        public bool dashing { get; protected set; } = false;
        public bool swimming { get; protected set; } = false;
        public bool sliding { get; protected set; } = false;
        public bool running { get; protected set; } = false;
        public bool crouching { get; protected set; } = false;
        public const float dashAnimDuration = 0.5f;
        public const float maxSpeed = 3f;
        public const float crouchSpeedMult = 0.6f;
        public const float jumpMaxDuration = 0.35f;
        public const float maxClimbSpeed = 2.5f;

        public void SetupInstances()
        {
            machine = new StateMachine();
            State[] allChildStates = GetComponentsInChildren<State>();
            foreach (State state in allChildStates)
            {
                state.SetCore(this);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if (facingRight)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(offset.x, offset.y, 0), size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(-offset.x, offset.y, 0), size);
            }
        }
    }
}