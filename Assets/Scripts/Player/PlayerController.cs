using System.Collections;
using UnityEngine;

namespace constellations
{
    public class PlayerController : MonoBehaviour
    {
        //init engine variables
        [SerializeField] private InputReader input;
        private Rigidbody2D rb2d;
        private CircleCollider2D circleCollider;
        [SerializeField] private LayerMask ground;
        [SerializeField] private LayerMask climbable;
        [SerializeField] private GameObject cameraFollowObject;

        //init constant variables
        private const float speed = 100f;
        private const float jumpVelo = 5f;
        private const float dashSpeedMult = 3f;
        private const float runSpeedMult = 1.8f;
        private const float crouchSpeedMult = 0.6f;
        private const float climbSpeedMult = 1.2f;

        //init state and timers variables
        private float horizontal = 0f;
        private float vertical = 0f;
        private float lastJumpY = 0f;
        private float fallYDampThreshold;
        private float moveFactor;
        private float climbFactor;

        public bool facingRight { get; private set; } = true;
        private bool jump = false;
        private bool longJump = false;
        private bool dashing = false;
        private bool running = false;
        private bool dashHelp = false;
        private bool dashOnCooldown = false;
        private bool crouching = false;
        private bool climbing = false;

        #region standard methods
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
            moveFactor = horizontal * Time.fixedDeltaTime;
            climbFactor = vertical * Time.fixedDeltaTime;


            //check if player is currently climbing 
            if (!isGrounded() && canClimb() && Mathf.Abs(moveFactor) > 0f) climbing = true;
            else climbing = false;

            if (climbing) rb2d.gravityScale = 0;
            else rb2d.gravityScale = 1;

            moveAction();
            climbAction();
            jumpAction();


            //CAMERA HANDLING BELOW, TAKE HEED
            //if falling faster than set threshold, lerp damping slightly
            if (rb2d.velocity.y < fallYDampThreshold && !CameraManager.instance.YDampLerping && !CameraManager.instance.PlayerFallLerped)
            {
                StartCoroutine(CameraManager.instance.LerpYAction(true));
            }

            //if y movement is >= 0, set damp to standard
            if (rb2d.velocity.y >= 0f && !CameraManager.instance.YDampLerping && CameraManager.instance.PlayerFallLerped)
            {
                //reset so this can be called again
                CameraManager.instance.PlayerFallLerped = false;

                StartCoroutine(CameraManager.instance.LerpYAction(false));
            }
        }

        #endregion

        #region input handlers

        private void HandleMove(Vector2 dir)
        {
            horizontal = dir.x * speed;
            vertical = dir.y * speed;
            Debug.Log(message: $"y {dir.y} x {dir.x} hz {horizontal} vert {vertical} spd {speed}");
        }

        private void HandleJump()
        {
            if (isGrounded() || canClimb())
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
            if (!climbing)
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
                    //PLAY DASH ANIMATION HERE
                }
            }
        }

        private void HandleDashCancel()
        {
            running = false;
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

        #region checks

        //check if player is currently on the ground using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        private bool isGrounded()
        {
            RaycastHit2D hit = Physics2D.CircleCast(circleCollider.bounds.center, circleCollider.radius, Vector2.down, 0.1f, ground);
            if (hit && !lastJumpY.Equals(0)) lastJumpY = 0;
            return hit.collider != null;
        }

        //check if player is on climbable wall using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        private bool canClimb()
        {
            RaycastHit2D hitRight = Physics2D.CircleCast(circleCollider.bounds.center, circleCollider.radius, Vector2.right, 0.1f, climbable);
            RaycastHit2D hitLeft = Physics2D.CircleCast(circleCollider.bounds.center, circleCollider.radius, Vector2.right, -0.1f, climbable);
            if (hitRight || hitLeft) return true;
            else return false;
        }

        #endregion

        #region movement actions

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

        private void moveAction()
        {
            //STANDARD MOVEMENT
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
        }

        private void jumpAction()
        {
            //executing jump
            if (jump)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpVelo);
                if (longJump) StartCoroutine(JumpCap());
            }
        }

        private void climbAction()
        {
            //executing climb
            if (climbing)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, climbFactor * climbSpeedMult);
            }
        }

        #endregion
    }
}