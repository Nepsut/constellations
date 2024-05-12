using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace constellations
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReader input;

        private Rigidbody2D rb2d;
        private CircleCollider2D circleCollider;
        [SerializeField] private LayerMask ground;
        private float speed = 100f;
        [Range(1f, 3f)][SerializeField] private float dashSpeedMult = 1.5f;
        [Range(0, 1f)][SerializeField] private float crouchSpeedMult = 0.6f;

        private float horizontal = 0f;
        private float lastJumpY = 0f;
        private bool facingRight = true;
        private bool jump = false, longJump = false;
        private bool dashing = false;
        private bool crouching = false;

        [Range(0, 5f)][SerializeField] private float fallLongMultiplier = 0.8f;
        [Range(0, 5f)][SerializeField] private float fallShortMultiplier = 1.6f;

        // Start is called before the first frame update
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            circleCollider = GetComponent<CircleCollider2D>();

            //add methods to events in InputReader
            input.MoveEvent += HandleMove;
            input.JumpEvent += HandleJump;
            input.JumpCanceledEvent += HandleJumpCancel;
            input.DashEvent += HandleDash;
            input.DashCanceledEvent += HandleDashCancel;
            input.CrouchEvent += HandleCrouch;
            input.CrouchCanceledEvent += HandleCrouchCancel;
        }

        //actual movement is handled here
        void FixedUpdate()
        {
            float moveFactor = horizontal * Time.fixedDeltaTime;

            if (!dashing && !crouching)       //if not dashing move at speed float
            {
                rb2d.velocity = new Vector2(moveFactor, rb2d.velocity.y);
                //Debug.Log(message:$"Horizontal {horizontal}, moveFactor {moveFactor}, time {Time.fixedDeltaTime}");
            }
            else if (dashing && !crouching)   //if dashing move at dashSpeedMult x speed
            {
                rb2d.velocity = new Vector2(moveFactor * dashSpeedMult, rb2d.velocity.y);
            }
            else if (!dashing && crouching)   //if crouching move at crouchSpeedMult x speed
            {
                rb2d.velocity = new Vector2(moveFactor * crouchSpeedMult, rb2d.velocity.y);
            }
            else                              //if somehow you end up in an unforeseen combo, move at normal speed
            {
                rb2d.velocity = new Vector2(moveFactor, rb2d.velocity.y);
            }

            //flip sprite according to movement direction
            if (moveFactor > 0f && !facingRight) SpriteFlip();
            else if (moveFactor < 0f && facingRight) SpriteFlip();


            //executing different jump
            if (jump)
            {
                float jumpVelo = 4f;
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpVelo);
                if (longJump) StartCoroutine(JumpCap());
            }
        }

        #region input handlers

        private void HandleMove(Vector2 dir)
        {
            horizontal = dir.x * speed;
            Debug.Log(message:$"y {dir.y} x {dir.x} hz {horizontal} spd {speed}");
        }

        private void HandleJump()
        {
            if (isGrounded())
            {
                jump = true;
                longJump = true;
            }
            //PLAY JUMP ANIMATION HERE
        }

        //this thing will end jump after 0.35 seconds
        private IEnumerator JumpCap()
        {
            longJump = false;
            yield return new WaitForSeconds(0.35f);
            jump = false;
        }

        private void HandleJumpCancel()
        {
            jump = false;
            //EXIT JUMP ANIMATION HERE
        }

        private void HandleDash()
        {
            dashing = true;
            crouching = false;
            //PLAY DASH ANIMATION HERE
        }

        private void HandleDashCancel()
        {
            dashing = false;
            //GO BACK TO WALK ANIMATION HERE
        }

        private void HandleCrouch()
        {
            crouching = true;
            dashing = false;
            //PLAY CROUCH ANIMATION HERE
        }
        private void HandleCrouchCancel()
        {
            crouching = false;
            //EXIT CROUCH ANIMATION HERE
        }

        #endregion

        //THIS THING FLIPS CAT, LITERALLY INSANE
        private void SpriteFlip()
        {
            facingRight = !facingRight;
            Vector3 transformScale = transform.localScale;
            transformScale.x *= -1;
            transform.localScale = transformScale;
        }

        //check if player is currently on the ground using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        private bool isGrounded()
        {
            RaycastHit2D hit = Physics2D.CircleCast(circleCollider.bounds.center, circleCollider.radius, Vector2.down, 0.1f, ground);
            if (hit && !lastJumpY.Equals(0)) lastJumpY = 0;
            return hit.collider != null;
        }
    }
}