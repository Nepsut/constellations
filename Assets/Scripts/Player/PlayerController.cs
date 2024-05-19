using System.Collections;
using UnityEngine;

namespace constellations
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Engine Variables")]
        [SerializeField] private InputReader input;
        private Rigidbody2D rb2d;
        private CapsuleCollider2D capsuleCollider;
        [SerializeField] private LayerMask ground;
        [SerializeField] private LayerMask climbable;
        [SerializeField] private GameObject cameraFollowObject;

        [Header("Constant Movement Variables")]
        private const float maxSpeed = 3f;
        private const float acceleration = 6000f;
        private const float deceleration = 8f;
        private float trueAcceleration;

        //if horizontal input differs from movement direction, changingDirection = true
        private bool changingDirection => (rb2d.velocity.x > 0f && horizontal < 0f) || (rb2d.velocity.x < 0f && horizontal > 0f);
        private const float jumpForce = 50f;
        private const float fallGravMult = 2.1f;
        private const float lowJumpMult = 1.8f;
        private const float jumpMaxDuration = 0.35f;
        private const float dashSpeedMult = 3f;
        private const float runSpeedMult = 1.8f;
        private const float dashMaxDuration = 0.2f;
        private const float dashCooldown = 0.1f;
        private const float crouchSpeedMult = 0.6f;
        private const float climbSpeedMult = 1.2f;
        private const float baseColliderHeight = 1.5f;
        private const float crouchColliderHeight = 1f;
        private const float groundRaycastLength = 0.87f;
        private const float crouchRaycastLength = 0.62f;
        private const float climbRaycastLength = 1.28f;

        [Header("Dynamic Movement Variables")]
        private float horizontal = 0f;
        private float vertical = 0f;
        private float realHorizontal = 0f;
        private float realVertical = 0f;
        private float targetHorizontal;
        private float targetVertical;

        [Header("Other Variables")]
        private float fallYDampThreshold;

        public bool facingRight { get; private set; } = true;
        private bool jump = false;
        private bool longJump = false;
        private bool wallJumped = false;
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
            capsuleCollider = GetComponent<CapsuleCollider2D>();

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
            calcAccel();
            //check if player is currently next to a climbable wall and is moving horizontally
            if (canClimb() && horizontal != 0f) climbing = true;
            else climbing = false;

            if (climbing) rb2d.gravityScale = 0;
            else rb2d.gravityScale = 1;

            moveAction();
            climbAction();
            if (jump) jumpAction();
            fallAdjuster();
            handleDrag();

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

        #region movement calcs

        private void calcAccel()
        {
            if (!wallJumped)
            {
                trueAcceleration = acceleration * horizontal * Time.deltaTime;
            }
            else
            {
                trueAcceleration = (acceleration * horizontal * Time.deltaTime) / 2;
            }
        }

        private void fallAdjuster()
        {
            if (rb2d.velocity.y < 0f)
            {
                rb2d.velocity += Vector2.up * Physics2D.gravity.y * (fallGravMult - 1) * Time.deltaTime;
            }
            else if (rb2d.velocity.y < 0f && !jump)
            {
                rb2d.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMult - 1) * Time.deltaTime;
            }
        }

        #endregion

        #region input handlers

        private void HandleMove(Vector2 dir)
        {
            horizontal = dir.x;
            vertical = dir.y;
            //Debug.Log(message: $"y {dir.y} x {dir.x} hz {horizontal} vert {vertical} spd {speed}");
        }

        private void HandleJump()
        {
            if (isGrounded() || canClimb())
            {
                if (canClimb()) wallJumped = true;
                jump = true;
                longJump = true;
                crouching = false;
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, baseColliderHeight);
                dashing = false;
            }
        }
        private void HandleJumpCancel()
        {
            jump = false;
        }

        //this thing will end jump after specified duration
        private IEnumerator JumpCap()
        {
            longJump = false;
            yield return new WaitForSeconds(jumpMaxDuration);
            jump = false;
            wallJumped = false;
        }

        private void HandleDash()
        {
            if (!climbing)
            {
                if (crouching)
                {
                    crouching = false;
                    capsuleCollider.size = new Vector2(capsuleCollider.size.x, baseColliderHeight);
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

        //this thing will end dash after specified duration
        private IEnumerator DashCap()
        {
            dashHelp = false;
            dashOnCooldown = true;
            yield return new WaitForSeconds(dashMaxDuration);
            dashing = false;
            yield return new WaitForSeconds(dashCooldown);
            dashOnCooldown = false;
        }

        private void HandleDashCancel()
        {
            running = false;
            //GO BACK TO WALK ANIMATION HERE
        }

        private void HandleCrouch()
        {
            if (isGrounded())
            {
                crouching = true;
                dashing = false;
                running = false;
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, crouchColliderHeight);
                StartCoroutine(CameraManager.instance.CrouchOffset(true));
                //PLAY CROUCH ANIMATION HERE
            }
        }

        private void HandleCrouchCancel()
        {
            crouching = false;
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, baseColliderHeight);
            StartCoroutine(CameraManager.instance.CrouchOffset(false));
            //EXIT CROUCH ANIMATION HERE
        }

        #endregion

        #region checks

        //check if player is currently on the ground using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        private bool isGrounded()
        {
            RaycastHit2D hit;
            if (!crouching)
            {
                hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastLength, ground);
            }
            else
            {
                hit = Physics2D.Raycast(transform.position, Vector2.down, crouchRaycastLength, ground);
            }
            return hit.collider != null;
        }

        //check if player is on climbable wall using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        private bool canClimb()
        {
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, climbRaycastLength, climbable);
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, -Vector2.right, climbRaycastLength, climbable);
            if ((hitRight || hitLeft)) return true;
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
            if (!dashing && !crouching && !running && !wallJumped)       //normal movement, accelerate until maxSpeed
            {
                rb2d.AddForce(trueAcceleration * Vector2.right);
                if (Mathf.Abs(rb2d.velocity.x) > maxSpeed)
                {
                    rb2d.velocity = new Vector2(Mathf.Sign(rb2d.velocity.x) * maxSpeed, rb2d.velocity.y);
                }
            }
            else if (!dashing && crouching && !running && !wallJumped)   //crouch movement, accelerate slower until maxSpeed*crouchSpeedMult
            {
                rb2d.AddForce(trueAcceleration * crouchSpeedMult * Vector2.right);
                if (Mathf.Abs(rb2d.velocity.x) > maxSpeed * crouchSpeedMult)
                {
                    rb2d.velocity = new Vector2(Mathf.Sign(rb2d.velocity.x) * maxSpeed * crouchSpeedMult, rb2d.velocity.y);
                }
            }
            if (wallJumped)        //if walljumped, add some sideways force as well based on player direction and slow player acceleration
            {
                rb2d.velocity = Vector2.Lerp(rb2d.velocity, (new Vector2(targetHorizontal, rb2d.velocity.y)), 0.5f * Time.deltaTime);
            }

            //flip sprite according to movement direction
            if (horizontal > 0f && !facingRight) CatFlip();
            else if (horizontal < 0f && facingRight) CatFlip();

            //DASH AND RUN HANDLED HERE
            if (dashing && !crouching)      //this is dash, the initial burst of speed on pressing shift
            {
                if (dashHelp && !dashOnCooldown)
                {
                    StartCoroutine(DashCap());
                }
                if (facingRight)
                {
                    //rb2d.velocity = new Vector2(speed * Time.fixedDeltaTime * dashSpeedMult, rb2d.velocity.y);
                }
                else if (!facingRight)
                {
                    //rb2d.velocity = new Vector2(-speed * Time.fixedDeltaTime * dashSpeedMult, rb2d.velocity.y);
                }
            }
            else if (running && !crouching) //this is run, the remaining extra speed after dash ends
            {
                if (!dashing && facingRight && running)
                {
                    rb2d.velocity = new Vector2(targetHorizontal * runSpeedMult, rb2d.velocity.y);
                }
                else if (!dashing && !facingRight && running)
                {
                    rb2d.velocity = new Vector2(targetHorizontal * runSpeedMult, rb2d.velocity.y);
                }
            }
        }

        private void handleDrag()
        {
            if (Mathf.Abs(horizontal) < 0.4f || changingDirection)
            {
                rb2d.drag = deceleration;
            }
            else
            {
                rb2d.drag = 0;
            }
        }

        private void jumpAction()
        {
            //executing jump
            if (!canClimb())
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
            else if (canClimb())
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                if (facingRight)
                {
                    rb2d.AddForce(new Vector2(-jumpForce, jumpForce), ForceMode2D.Impulse);
                }
                else
                {
                    rb2d.AddForce(new Vector2(jumpForce, jumpForce), ForceMode2D.Impulse);
                }
                if (longJump) StartCoroutine(JumpCap());
            }
        }

        private void climbAction()
        {
            //executing climb
            if (climbing)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, targetVertical * climbSpeedMult);
            }
        }

        #endregion

        #region TEMPORARY

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastLength);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * crouchRaycastLength);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * climbRaycastLength);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position - Vector3.right * climbRaycastLength);
        }

        #endregion
    }
}