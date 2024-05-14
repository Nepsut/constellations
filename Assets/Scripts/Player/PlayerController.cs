using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace constellations
{
    public class PlayerController : MonoBehaviour
    {
        //init engine variables
        [SerializeField] private InputReader input;
        private Rigidbody2D rb2d;
        private CircleCollider2D circleCollider;
        [SerializeField] private LayerMask ground;
        [SerializeField] private GameObject cameraFollowObject;

        //init constant variables
        private const float speed = 100f;
        private const float jumpVelo = 5f;
        private const float dashSpeedMult = 3f;
        private const float runSpeedMult = 1.8f;
        private const float crouchSpeedMult = 0.6f;

        //init state and timers variables
        private float horizontal = 0f, lastJumpY = 0f, dashTimer = 0f, jumpTimer = 0f, fallYDampThreshold;
        public bool facingRight { get; private set; } = true;
        private bool jump = false, longJump = false, dashing = false, running = false, dashHelp = false, dashOnCooldown = false, crouching = false;

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

            fallYDampThreshold = CameraManager.instance.fallSpeedDampThreshold;
            Debug.Log(fallYDampThreshold);
        }

        //actual movement is handled here
        void FixedUpdate()
        {
            float moveFactor = horizontal * Time.fixedDeltaTime;

            if (!dashing && !crouching && !running)       //if not dashing move at speed float
            {
                rb2d.velocity = new Vector2(moveFactor, rb2d.velocity.y);
                //Debug.Log(message:$"Horizontal {horizontal}, moveFactor {moveFactor}, time {Time.fixedDeltaTime}");
            }
            else if (!dashing && crouching && !running)   //if crouching move at crouchSpeedMult x speed
            {
                rb2d.velocity = new Vector2(moveFactor * crouchSpeedMult, rb2d.velocity.y);
            }

            //flip sprite according to movement direction
            if (moveFactor > 0f && !facingRight) CatFlip();
            else if (moveFactor < 0f && facingRight) CatFlip();

            //DASH AND RUN HANDLED HERE
            if (dashing && !crouching)      //this is dash, the initial burst of speed on pressing shift
            {
                if (dashHelp && !dashOnCooldown)
                {
                    StartCoroutine(DashCap());
                }
                if (facingRight)
                {
                    rb2d.velocity = new Vector2(speed * Time.fixedDeltaTime * dashSpeedMult, rb2d.velocity.y);
                }
                else if (!facingRight)
                {
                    rb2d.velocity = new Vector2(-speed * Time.fixedDeltaTime * dashSpeedMult, rb2d.velocity.y);
                }
            }
            else if (running && !crouching) //this is run, the remaining extra speed after dash ends
            {
                if (!dashing && facingRight && running)
                {
                    rb2d.velocity = new Vector2(moveFactor * runSpeedMult, rb2d.velocity.y);
                }
                else if (!dashing && !facingRight && running)
                {
                    rb2d.velocity = new Vector2(moveFactor * runSpeedMult, rb2d.velocity.y);
                }
            }

            //executing jump
            if (jump)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpVelo);
                if (longJump) StartCoroutine(JumpCap());
            }

            //CAMERA HANDLING BELOW, TAKE HEED
            //if falling faster than set threshold
            if (rb2d.velocity.y < fallYDampThreshold && !CameraManager.instance.YDampLerping && !CameraManager.instance.PlayerFallLerped)
            {
                StartCoroutine(CameraManager.instance.LerpYAction(true));
            }

            //if y movement is >= 0
            if (rb2d.velocity.y >= 0f && !CameraManager.instance.YDampLerping && CameraManager.instance.PlayerFallLerped)
            {
                //reset so this can be called again
                CameraManager.instance.PlayerFallLerped = false;

                StartCoroutine(CameraManager.instance.LerpYAction(false));
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

        private IEnumerator DashCap()
        {
            dashHelp = false;
            dashOnCooldown = true;
            yield return new WaitForSeconds(0.2f);
            dashing = false;
            yield return new WaitForSeconds(0.1f);
            dashOnCooldown = false;
        }

        private void HandleJumpCancel()
        {
            jump = false;
            //EXIT JUMP ANIMATION HERE
        }

        private void HandleDash()
        {
            if (crouching)
            {
                crouching = false;
                StartCoroutine(CameraManager.instance.CrouchOffset(false));
            }
            if (!dashing)
            {
                dashing = true;
                running = true;
                dashHelp = true;
                crouching = false;
            }
            //PLAY DASH ANIMATION HERE
        }

        private void HandleDashCancel()
        {
            running = false;
            dashTimer = 0f;
            //GO BACK TO WALK ANIMATION HERE
        }

        private void HandleCrouch()
        {
            crouching = true;
            dashing = false;
            running = false;
            StartCoroutine(CameraManager.instance.CrouchOffset(true));
            //PLAY CROUCH ANIMATION HERE
        }
        private void HandleCrouchCancel()
        {
            crouching = false;
            StartCoroutine(CameraManager.instance.CrouchOffset(false));
            //EXIT CROUCH ANIMATION HERE
        }

        #endregion

        //THIS THING FLIPS CAT, LITERALLY INSANE
        private void CatFlip()
        {
            if (facingRight)
            {
                Vector3 newRotation = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(newRotation);
                facingRight = !facingRight;

                //turn camera to follow object with small delay, handled in different script
                StartCoroutine(cameraFollowObject.GetComponent<CameraFollowObject>().FlipYLerp());
            }
            else
            {
                Vector3 newRotation = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(newRotation);
                facingRight = !facingRight;

                //turn camera to follow object with small delay, handled in different script
                StartCoroutine(cameraFollowObject.GetComponent<CameraFollowObject>().FlipYLerp());
            }
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