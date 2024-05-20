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
        private const float colliderOffset = 0.4f;
        [SerializeField] private LayerMask ground;
        [SerializeField] private LayerMask climbable;
        [SerializeField] private GameObject cameraFollowObject;

        [Header("Constant Movement Variables")]
        private const float maxSpeed = 3f;
        private const float maxClimbSpeed = 2.5f;
        private const float acceleration = 6000f;
        private const float deceleration = 8f;
        private const float jumpForce = 65f;
        private const float dashForce = 75f;
        private const float movspeedTransitionTime = 0.3f;
        private const float dashCooldown = 0.3f;
        private const float fallGravMult = 2.1f;
        private const float lowJumpMult = 1.8f;
        private const float airLinearDrag = 2.5f;
        private const float jumpMaxDuration = 0.35f;
        private const float runSpeedMult = 1.8f;
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
        private float trueAcceleration;
        private float climbAcceleration;
        private float trueAllowedSpeed;
        public bool facingRight { get; private set; } = true;
        private bool jump = false;
        private bool longJump = false;
        private bool wallJumped = false;
        private bool dashing = false;
        private bool dashOnCooldown = false;
        private bool lerpingMaxSpeed = false;
        private bool running = false;
        private bool crouching = false;
        private bool climbing = false;

        //if horizontal input differs from movement direction, changingDirection = true
        private bool changingXDirection => (rb2d.velocity.x > 0f && horizontal < 0f) || (rb2d.velocity.x < 0f && horizontal > 0f);
        private bool changingYDirection => (rb2d.velocity.y > 0f && vertical < 0f) || (rb2d.velocity.y < 0f && vertical > 0f);

        [Header("Other Variables")]
        private float fallYDampThreshold;

        #region standard methods

        private void Awake()
        {
            //fetch rigidbody and collider
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
        }

        void Start()
        {
            //set YDampThreshold to value specified in CameraManager
            fallYDampThreshold = CameraManager.instance.fallSpeedDampThreshold;
            trueAllowedSpeed = maxSpeed;
        }

        //using FixedUpdate so framerate doesn't affect functionality
        void FixedUpdate()
        {
            //MOVEMENT-RELATED METHODS BELOW
            //first calculate true acceleration for movement
            calcAccel();

            //check if player is currently next to a climbable wall and is moving horizontally at wall
            if ((canClimb() == 1 && horizontal > 0f) || (canClimb() == 0 && horizontal < 0f)) climbing = true;
            else climbing = false;

            //while climbing, set player gravity to 0 so that climbing can be handled easier
            if (climbing) rb2d.gravityScale = 0;
            else rb2d.gravityScale = 1;

            //if moving, move, if climbing, climb, if dashing, dash, if jumping, jump
            if (horizontal != 0f) moveAction();
            if (climbing) climbAction();
            if (dashing) dashAction();
            if (jump) jumpAction();

            //adjust drag (and gravity) for smoother movement
            if (!climbing) fallAdjuster();
            if (isGrounded()) handleDrag();
            else handleAirDrag();


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
            //make acceleration much lower if player has walljumped recently to limit mobility slightly
            if (!wallJumped)
            {
                trueAcceleration = acceleration * horizontal * Time.deltaTime;
            }
            else
            {
                trueAcceleration = (acceleration * horizontal * Time.deltaTime) / 4;
            }
            climbAcceleration = acceleration * climbSpeedMult * vertical * Time.deltaTime;
        }

        //this thing lerps trueAllowedSpeed to maxSpeef from the player's current speed
        //called when dash force impulse is added and when run ends to make movement smooth
        private IEnumerator moveSpeedLerp()
        {
            lerpingMaxSpeed = true;
            float startSpeed = Mathf.Abs(rb2d.velocity.x);
            float takenTime = 0f;
            while (takenTime < movspeedTransitionTime)
            {
                takenTime += Time.deltaTime;

                float lerpedMaxSpeed = Mathf.Lerp(startSpeed, maxSpeed, (takenTime / movspeedTransitionTime));
                trueAllowedSpeed = lerpedMaxSpeed;
                yield return null;
            }
            lerpingMaxSpeed = false;
            trueAllowedSpeed = maxSpeed;
        }

        private void fallAdjuster()
        {
            //add gravity when falling to make jumps more snappy and satisfying
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
            //separate the vector2 from movement input to horizontal and vertical for easier usage
            horizontal = dir.x;
            vertical = dir.y;
        }

        private void HandleJump()
        {
            if (isGrounded() || canClimb() >= 0)    //if cat on ground or can climb on wall
            {
                if (canClimb() >= 0) wallJumped = true;
                jump = true;
                longJump = true;
                if (crouching)      //if crouching, stop crouching and return collider to normal size
                {
                    crouching = false;
                    capsuleCollider.size = new Vector2(capsuleCollider.size.x, baseColliderHeight);
                }
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

        //does the dash thing but only if we're not climbing or dash isn't on cooldown
        private void HandleDash()
        {
            if (!climbing && !dashOnCooldown)
            {
                if (crouching)
                {
                    crouching = false;
                    capsuleCollider.size = new Vector2(capsuleCollider.size.x, baseColliderHeight);
                    StartCoroutine(CameraManager.instance.CrouchOffset(false));
                }
                dashing = true;
                running = true;
            }
        }

        //this thing handles dash cooldown
        private IEnumerator DashCap()
        {
            dashOnCooldown = true;
            yield return new WaitForSeconds(dashCooldown);
            dashOnCooldown = false;
        }

        private void HandleDashCancel()
        {
            running = false;
            StartCoroutine(moveSpeedLerp());    //called to smooth movement from run speed to normal speed
        }

        private void HandleCrouch()
        {
            if (isGrounded())
            {
                crouching = true;
                running = false;
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, crouchColliderHeight);   //make collider smaller
                StartCoroutine(CameraManager.instance.CrouchOffset(true));                          //pan cam down
            }
        }

        private void HandleCrouchCancel()
        {
            crouching = false;
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, baseColliderHeight);         //make collider great again /j
            StartCoroutine(CameraManager.instance.CrouchOffset(false));                             //pan cam to normal
        }

        #endregion

        #region checks

        //check if player is currently on the ground using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        private bool isGrounded()
        {
            Vector2 lineDownRight = new Vector2(transform.position.x + colliderOffset, transform.position.y);
            Vector2 lineDownLeft = new Vector2(transform.position.x - colliderOffset, transform.position.y);
            RaycastHit2D hit1;
            RaycastHit2D hit2;
            if (!crouching)
            {
                hit1 = Physics2D.Raycast(lineDownRight, Vector2.down, groundRaycastLength, ground);
                hit2 = Physics2D.Raycast(lineDownLeft, Vector2.down, groundRaycastLength, ground);
            }
            else
            {
                hit1 = Physics2D.Raycast(lineDownRight, Vector2.down, crouchRaycastLength, ground);
                hit2 = Physics2D.Raycast(lineDownLeft, Vector2.down, crouchRaycastLength, ground);
            }
            if (hit1 || hit2) return true;
            else return false;
        }

        //check if player is on climbable wall using fancy raycasting tech
        //to avoid the jitteriness of rigidbodies
        //THIS IS AN INT SO WE KNOW IF WALL IS ON LEFT OR RIGHT,
        //1 == RIGHT, 0 == LEFT
        private int canClimb()
        {
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, climbRaycastLength, climbable);
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, -Vector2.right, climbRaycastLength, climbable);
            if (hitRight) return 1;
            else if (hitLeft) return 0;
            else return -1;
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
            if (crouching && !wallJumped)   //crouch movement, accelerate slower until trueAllowedSpeed*crouchSpeedMult
            {
                rb2d.AddForce(trueAcceleration * crouchSpeedMult * Vector2.right);
                if (Mathf.Abs(rb2d.velocity.x) > trueAllowedSpeed * crouchSpeedMult)
                {
                    rb2d.velocity = new Vector2(Mathf.Sign(rb2d.velocity.x) * trueAllowedSpeed * crouchSpeedMult, rb2d.velocity.y);
                }
            }
            else if (running && !wallJumped)  //run movement, accelerate faster until trueAllowedSpeed*runSpeedMult
            {
                rb2d.AddForce(trueAcceleration * runSpeedMult * Vector2.right);
                if (Mathf.Abs(rb2d.velocity.x) > trueAllowedSpeed * runSpeedMult)
                {
                    rb2d.velocity = new Vector2(Mathf.Sign(rb2d.velocity.x) * trueAllowedSpeed * runSpeedMult, rb2d.velocity.y);
                }
            }
            else       //normal movement, accelerate until trueAllowedSpeed
            {
                rb2d.AddForce(trueAcceleration * Vector2.right);
                if (Mathf.Abs(rb2d.velocity.x) > trueAllowedSpeed)
                {
                    rb2d.velocity = new Vector2(Mathf.Sign(rb2d.velocity.x) * trueAllowedSpeed, rb2d.velocity.y);
                }
            }

            //flip sprite according to movement direction
            if (horizontal > 0f && !facingRight) CatFlip();
            else if (horizontal < 0f && facingRight) CatFlip();
        }

        private void handleDrag()
        {
            if (Mathf.Abs(horizontal) < 0.4f || changingXDirection)      //if less than 0.4 input or if changing direction
            {
                rb2d.drag = deceleration;       //add drag
            }
            else
            {
                rb2d.drag = 0;                  //remove drag
            }
        }

        private void handleAirDrag()
        {
            if (climbing)       //if climbing, add normal deceleration drag
            {
                if (Mathf.Abs(vertical) < 0.4f || changingYDirection)      //if less than 0.4 input or if changing direction
                {
                    rb2d.drag = deceleration;       //add drag
                }
                else
                {
                    rb2d.drag = 0;                  //remove drag
                }
            }
            else                //if not climbing, add air drag instead
            {
                rb2d.drag = airLinearDrag;       //add air drag
            }
        }

        private void jumpAction()
        {
            //executing jump
            if (canClimb() < 0)         //IF CAN'T CLIMB, EXECUTE NORMAL JUMP
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
            else if (canClimb() == 0)   //IF CAN CLIMB AND WALL ON LEFT, WALLJUMP TO RIGHT
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(new Vector2(jumpForce, jumpForce), ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
            else if (canClimb() == 1)   //IF CAN CLIMB AND WALL ON RIGHT, WALLJUMP TO LEFT
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(new Vector2(-jumpForce, jumpForce), ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
        }

        private void dashAction()
        {
            if (facingRight)
            {
                rb2d.AddForce(Vector2.right * dashForce, ForceMode2D.Impulse);
            }
            else
            {
                rb2d.AddForce(-Vector2.right * dashForce, ForceMode2D.Impulse);
            }
            dashing = false;
            StartCoroutine(DashCap());
            if (!lerpingMaxSpeed) StartCoroutine(moveSpeedLerp());
        }

        private void climbAction()
        {
            rb2d.AddForce(climbAcceleration * Vector2.up);
            if (Mathf.Abs(rb2d.velocity.y) > maxClimbSpeed)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Sign(rb2d.velocity.y) * maxClimbSpeed);
            }
        }

        #endregion

        #region TEMPORARY

        private void OnDrawGizmos()
        {
            Vector2 lineDownRight = new Vector2(transform.position.x + colliderOffset, transform.position.y);
            Vector2 lineDownLeft = new Vector2(transform.position.x - colliderOffset, transform.position.y);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(lineDownRight, lineDownRight + Vector2.down * groundRaycastLength);
            Gizmos.DrawLine(lineDownLeft, lineDownLeft + Vector2.down * groundRaycastLength);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lineDownRight, lineDownRight + Vector2.down * crouchRaycastLength);
            Gizmos.DrawLine(lineDownLeft, lineDownLeft + Vector2.down * crouchRaycastLength);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * climbRaycastLength);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position - Vector3.right * climbRaycastLength);
        }

        #endregion
    }
}