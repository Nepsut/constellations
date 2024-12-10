using System;
using System.Collections;
using UnityEngine;

namespace constellations
{
    public class PlayerController : StateMachineCore, IDataPersistence
    {
        #region variables

        [Header("Management Variables")]
        private bool attackEnabled = true;
        private bool screamEnabled = true;

        [Header("Engine Variables")]
        [SerializeField] private InputReader playerInput;
        [SerializeField] private HitBoxController attackHitbox;
        private CapsuleCollider2D capsuleCollider;
        private const float colliderOffset = 0.4f;
        [SerializeField] private LayerMask ground;
        [SerializeField] private GameObject cameraFollowObject;

        //reminder that constant variables can only be referenced via the class
        //and not via an object made from the class, so PlayerController.maxSpeed
        //instead of object.maxSpeed
        [Header("Constant Movement Variables")]
        private const float acceleration = 6000f;
        private const float deceleration = 8f;
        private const float jumpForce = 50f;
        private const float dashForce = 150f;
        private const float moveSpeedTransitionTime = 0.3f;
        private const float dashCooldown = 1f;
        public const float dashDecelerationIgnore = 0.2f;
        private const float fallGravMult = 3.7f;
        private const float lowJumpMult = 1.8f;
        private const float airLinearDrag = 2.5f;
        public const float runSpeedMult = 1.8f;
        private const float baseColliderHeight = 1.5f;
        private const float crouchColliderHeight = 1f;

        [Header("Dynamic Movement Variables")]
        private bool jump = false;
        private bool longJump = false;
        public float horizontal { get; private set; } = 0f;
        public float vertical { get; private set; } = 0f;
        private float trueAcceleration;
        private float climbAcceleration;
        private float trueAllowedSpeed;
        private bool dashHappened = false;
        private bool dashOnCooldown = false;
        private bool dashDecelerating = true;
        private bool lerpingMaxSpeed = false;
        public bool runningHelper { get; private set; } = false;
        private float fallYDampThreshold;

        //if input differs from movement direction, changingDirection = true
        private bool changingXDirection => (rb2d.velocity.x > 0f && horizontal < 0f) || (rb2d.velocity.x < 0f && horizontal > 0f);
        private bool changingYDirection => (rb2d.velocity.y > 0f && vertical < 0f) || (rb2d.velocity.y < 0f && vertical > 0f);

        [Header("Constant Action Variables")]
        private const int attackDamage = 20;
        private const int attackBuffAmount = 5;
        private const float attackSpeed = 10f;      //real attackspeed ends up being 10/attackSpeed
        private const float attackChargeTime = 1f;
        public const int knockbackbBuffAmount = 5;
        private const float screamMinDuration = 1.5f;
        private const float screamBufferTime = 0.1f;
        private const float meowTime = 0.2f;

        [Header("Dynamic Action Variables")]
        private int attackBuffs = 0;                //add 1 every time player's attack gets buffed
        public int knockbackBuffs { get; private set; } = 0;      //add 1 on knockback buff
        private int realDamage = 20;
        private float heavyAttackMult = 1.5f;
        private bool attackCooldown = false;
        private bool didAttack = false;
        private bool canHeavyAttack = false;
        private bool heavyAttackCoolingDown = false;
        private bool screaming = false;
        private bool screamKeyHeld = false;
        private bool meow = false;
        private Coroutine attackTypeCheck;
        private Coroutine scream;

        [Header("Interaction Variables")]
        private bool canInteractNPC = false;
        private bool canInteractObject = false;
        [HideInInspector] public bool didInteractObject = false;
        private GameObject interactingNPC;
        private GameObject interactingObject;
        private GameObject saveObject;

        [Header("Other Dynamic Variables")]
        private int attackChain = 0;

        [Header("States")]
        [SerializeField] private State idleState;
        [SerializeField] private State walkState;
        [SerializeField] private State runState;
        [SerializeField] private State dashState;
        [SerializeField] private State crouchState;
        [SerializeField] private State crouchIdleState;
        [SerializeField] private State slideState;
        [SerializeField] private State climbState;
        [SerializeField] private State swimState;
        [SerializeField] private State swimIdleState;
        [SerializeField] private State airState;
        [SerializeField] private State wallJumpState;
        [SerializeField] private AttackState[] attackStates;
        [SerializeField] private AttackSlashState slashAttackState;


        #endregion

        #region standard methods

        private void Awake()
        {
            //fetch rigidbody and collider
            rb2d = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            animator = GetComponent<Animator>();

            //this populates all states' "core" variable as this object
            SetupInstances();

            //add methods to events in InputReader
            playerInput.MoveEvent += HandleMove;
            playerInput.JumpEvent += HandleJump;
            playerInput.JumpCanceledEvent += HandleJumpCancel;
            playerInput.DashEvent += HandleDash;
            playerInput.DashCanceledEvent += HandleDashCancel;
            playerInput.CrouchEvent += HandleCrouch;
            playerInput.CrouchCanceledEvent += HandleCrouchCancel;
            playerInput.AttackEvent += HandleAttack;
            playerInput.AttackCanceledEvent += HandleAttackCancel;
            playerInput.ScreamEvent += HandleScream;
            playerInput.ScreamCanceledEvent += HandleScreamCancel;
            playerInput.MeowEvent += HandleMeow;
            playerInput.InteractEvent += HandleInteract;
            playerInput.InteractCanceledEvent += HandleInteractCancel;
        }

        void Start()
        {
            //set YDampThreshold to value specified in CameraManager
            fallYDampThreshold = CameraManager.instance.fallSpeedDampThreshold;
            trueAllowedSpeed = maxSpeed;
            realDamage = attackDamage + attackBuffs * attackBuffAmount;

            machine.Set(idleState);
        }

        //using FixedUpdate so framerate doesn't affect functionality
        void FixedUpdate()
        {
            //MOVEMENT-RELATED METHODS BELOW
            //first calculate true acceleration for movement
            CalcAccel();

            if (facingRight)
            {
                CheckWall(transform.position + new Vector3(offset.x, offset.y, 0), size);
            }
            else
            {
                CheckWall(transform.position + new Vector3(-offset.x, offset.y, 0), size);
            }
            
            IsClimbing(horizontal);

            //while climbing, set player gravity to 0 so that climbing can be handled easier
            if (climbing) rb2d.gravityScale = 0;
            else rb2d.gravityScale = 1;

            //if moving, move, if climbing, climb, if dashing, dash, if jumping, jump
            if (horizontal != 0f) MoveAction();
            if (climbing) ClimbAction();
            if (dashing) DashAction();
            if (jump) JumpAction();

            //adjust drag (and gravity) for smoother movement
            if (!climbing) FallAdjuster();
            if (groundSensor.grounded) HandleDrag();
            else HandleAirDrag();


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
            
            SelectState();
            machine.state.Do();
        }

        private void Update()
        {
            if (timeSinceLastAttack < chainAttacksThreshold) TimeAttacks();
        }

        //when entering a 2d trigger, check if it's from an NPC or an interactable object
        //this then lets player interact with said entity with the HandleInteract method
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC") || collision.gameObject.CompareTag("SavePoint"))
            {
                canInteractNPC = true;
                //save interactable NPC so we can easily call the Talk() method from it
                interactingNPC = collision.gameObject;
                //activate indicator to show this NPC can be interacted with
                if (interactingNPC.transform.GetChild(0).gameObject != null)
                {
                    interactingNPC.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = true;
                //save interactable object so we can easily call the Interact() method from it
                interactingObject = collision.gameObject;
                if (interactingObject.transform.GetChild(0).gameObject != null)
                {
                    interactingObject.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            if (collision.gameObject.CompareTag("SavePoint"))
            {
                collision.gameObject.GetComponent<SavePoint>().usedSavepoint = true;
            }
        }

        //when leaving a 2d trigger, clear appropriate saved entity and disable indicators
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.CompareTag("NPC") || collision.gameObject.CompareTag("SavePoint"))
            {
                canInteractNPC = false;
                if (interactingNPC == null) return;
                if (interactingNPC.transform.GetChild(0).gameObject != null)
                {
                    interactingNPC.transform.GetChild(0).gameObject.SetActive(false);
                }
                interactingNPC = null;
            }
            else if (collision.gameObject.CompareTag("Interactable"))
            {
                canInteractObject = false;
                if (interactingObject == null) return;
                if (interactingObject.transform.GetChild(0).gameObject != null)
                {
                    interactingObject.transform.GetChild(0).gameObject.SetActive(false);
                }
                interactingObject = null;
            }
            if (collision.gameObject.CompareTag("SavePoint"))
            {
                collision.gameObject.GetComponent<SavePoint>().usedSavepoint = false;
            }
        }

        #endregion

        #region movement calcs

        private void CalcAccel()
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
            climbAcceleration = acceleration * vertical * Time.deltaTime;
        }

        //this thing lerps trueAllowedSpeed to maxSpeed from the player's current speed
        //called when dash force impulse is added and when run ends to make movement smooth
        private IEnumerator MoveSpeedLerp()
        {
            lerpingMaxSpeed = true;
            float startSpeed = Mathf.Abs(rb2d.velocity.x);
            float takenTime = 0f;

            if (runningHelper)
            {
                runningHelper = false;
                while (takenTime < moveSpeedTransitionTime)
                {
                    takenTime += Time.deltaTime;

                    float lerpedMaxSpeed = Mathf.Lerp(startSpeed, maxSpeed * runSpeedMult, (takenTime / moveSpeedTransitionTime));
                    trueAllowedSpeed = lerpedMaxSpeed;
                    yield return null;
                }
            }
            else
            {
                while (takenTime < moveSpeedTransitionTime)
                {
                    takenTime += Time.deltaTime;

                    float lerpedMaxSpeed = Mathf.Lerp(startSpeed, maxSpeed, (takenTime / moveSpeedTransitionTime));
                    trueAllowedSpeed = lerpedMaxSpeed;
                    yield return null;
                }
                if (running) running = false;
            }
            lerpingMaxSpeed = false;
        }

        private void FallAdjuster()
        {
            //add gravity when falling to make jumps more snappy and satisfying
            if (rb2d.velocity.y < 0f)
            {
                rb2d.velocity += (fallGravMult - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
            }
            else if (rb2d.velocity.y < 0f && !jump)
            {
                rb2d.velocity += (lowJumpMult - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
            }
        }

        private void HandleDrag()
        {
            if ((Mathf.Abs(horizontal) < 0.4f || changingXDirection) && dashDecelerating)      //if less than 0.4 input or if changing direction
            {
                rb2d.drag = deceleration;       //add drag
            }
            else
            {
                rb2d.drag = 0;                  //remove drag
            }
        }

        private void HandleAirDrag()
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

        private void TimeAttacks()
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        private bool ChainAttacks()
        {
            return timeSinceLastAttack < chainAttacksThreshold;
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
            if (groundSensor.grounded || canClimb >= 0)    //if cat on ground or can climb on wall
            {
                if (canClimb >= 0) wallJumped = true;
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
                runningHelper = true;
            }
            else dashHappened = false;
        }

        //this thing handles dash cooldown
        private IEnumerator DashCooldown()
        {
            dashOnCooldown = true;
            yield return new WaitForSeconds(dashCooldown);
            dashOnCooldown = false;
        }

        private IEnumerator DashDecelerationManager()
        {
            dashDecelerating = false;
            yield return new WaitForSeconds(dashDecelerationIgnore);
            dashDecelerating = true;
        }

        private void HandleDashCancel()
        {
            if (dashHappened) StartCoroutine(MoveSpeedLerp());    //called to smooth movement from run speed to normal speed
        }

        private void HandleCrouch()
        {
            if (groundSensor.grounded)
            {
                crouching = true;
                running = false;
                runningHelper = false;
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

        private void HandleAttack()
        {
            if (!attackEnabled) return;
            if (!attackCooldown && !screaming)
            {
                Debug.Log("attack pressed");
                didAttack = true;
                attackTypeCheck = StartCoroutine(AttackTypeCheck());
            }
        }

        private void HandleAttackCancel()
        {
            if (!attackEnabled) return;
            if (didAttack)
            {
                Debug.Log("attack released");
                didAttack = false;
                StartCoroutine(Attack());
            }
        }

        private void HandleScream()
        {
            if (!screamEnabled) return;
            scream = StartCoroutine(Scream());
            screamKeyHeld = true;
        }

        private void HandleScreamCancel()
        {
            if (!screamEnabled) return;
            screamKeyHeld = false;
        }

        private void HandleMeow()
        {
            if (!meow) StartCoroutine(Meow());
        }

        //handles interaction based on data retrieved when entering trigger
        private void HandleInteract()
        {
            if (canInteractNPC && interactingNPC != null)
            {
                //change input mode so player movement is disabled during dialogue
                playerInput.SetDialogue();
                //this calls the NPC's dialogue based in its INK story
                interactingNPC.GetComponent<NPCDialogue>().Talk();
            }
            else if (canInteractObject && interactingObject != null)
            {
                didInteractObject = true;
            }
        }

        private void HandleInteractCancel()
        {
            didInteractObject = false;
        }

        #endregion

        #region checks

        private void StatePicker()
        {
            
        }

        private void SelectState()
        {
            if (sliding)
            {
                machine.Set(slideState);
            }
            else if (swimming && horizontal == 0)
            {
                machine.Set(swimIdleState);
            }
            else if (swimming && horizontal != 0)
            {
                machine.Set(swimState);
            }
            else if (climbing)
            {
                machine.Set(climbState);
            }
            else if (!groundSensor.grounded && wallJumped)
            {
                machine.Set(wallJumpState);
            }
            else if (!groundSensor.grounded && !wallJumped)
            {
                machine.Set(airState);
            }
            else if (crouching && horizontal == 0)
            {
                machine.Set(crouchIdleState);
            }
            else if (crouching && horizontal != 0)
            {
                machine.Set(crouchState);
            }
            else if (!dashDecelerating)
            {
                machine.Set(dashState);
            }
            else if (!running && horizontal != 0)
            {
                machine.Set(walkState);
            }
            else if (running && horizontal != 0)
            {
                machine.Set(runState);
            }
            else if (attacking)
            {
                machine.Set(attackStates[attackChain] as State);
                attackChain++;
                if (attackChain > attackStates.Length) attackChain = 0;
            }
            else if (bigAttacking)
            {
                machine.Set(slashAttackState as State);
            }
            else
            {
                machine.Set(idleState);
            }
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

        private void MoveAction()
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
                if (Mathf.Abs(rb2d.velocity.x) > trueAllowedSpeed)
                {
                    rb2d.velocity = new Vector2(Mathf.Sign(rb2d.velocity.x) * trueAllowedSpeed, rb2d.velocity.y);
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

        private void JumpAction()
        {
            //executing jump
            if (canClimb < 0)         //IF CAN'T CLIMB, EXECUTE NORMAL JUMP
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
            else if (canClimb == 0)   //IF CAN CLIMB AND WALL ON LEFT, WALLJUMP TO RIGHT
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(new Vector2(jumpForce, jumpForce), ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
            else if (canClimb == 1)   //IF CAN CLIMB AND WALL ON RIGHT, WALLJUMP TO LEFT
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(new Vector2(-jumpForce, jumpForce), ForceMode2D.Impulse);
                if (longJump) StartCoroutine(JumpCap());
            }
        }

        private void DashAction()
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
            dashHappened = true;
            StartCoroutine(DashCooldown());
            StartCoroutine(DashDecelerationManager());
            if (!lerpingMaxSpeed) StartCoroutine(MoveSpeedLerp());
        }

        private void ClimbAction()
        {
            rb2d.AddForce(climbAcceleration * Vector2.up);
            if (Mathf.Abs(rb2d.velocity.y) > maxClimbSpeed)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Sign(rb2d.velocity.y) * maxClimbSpeed);
            }
        }

        #endregion

        #region action methods

        private IEnumerator AttackTypeCheck()
        {
            //play animation for charge here
            yield return new WaitForSeconds(attackChargeTime);
            canHeavyAttack = true;
        }

        private IEnumerator Attack()
        {
            attackCooldown = true;
            StopCoroutine(attackTypeCheck);
            if (canHeavyAttack && !heavyAttackCoolingDown) HeavyAttack(); 
            else NormalAttack();
            yield return new WaitForSeconds(10f / attackSpeed + attackStates[attackChain].anim.length);
            attackCooldown = false;
        }

        private void NormalAttack()
        {
            attacking = true;
            StopCoroutine(attackTypeCheck);
            Debug.Log("normal attack done");
            if (attackHitbox.canAttackEnemy && attackHitbox.targetEnemy != null)
            {
                DealDamage(realDamage, false);
                Debug.Log(message: $"did normal attack on enemy for {realDamage} damage");
            }
        }

        private void HeavyAttack()
        {
            bigAttacking = true;
            canHeavyAttack = false;
            Debug.Log("heavy attack done");
            if (attackHitbox.canAttackEnemy && attackHitbox.targetEnemy != null)
            {
                DealDamage(realDamage * heavyAttackMult, true);
                Debug.Log(message: $"did heavy attack on enemy for {realDamage * heavyAttackMult} damage");
            }
            heavyAttackCoolingDown = true;
            StartCoroutine(HeavyAttackCooldown());
        }

        private IEnumerator HeavyAttackCooldown()
        {
            yield return new WaitForSeconds(heavyAttackCooldown + slashAttackState.anim.length);
            heavyAttackCoolingDown = false;
        }

        private void DealDamage(float t_damage, bool wasHeavy)
        {
            if (attackHitbox.targetEnemy.CompareTag("Ghost"))
            {
                attackHitbox.targetEnemy.GetComponentInParent<GhostBehavior>().TakeDamage(t_damage);
                attackHitbox.targetEnemy.GetComponentInParent<GhostBehavior>().wasHeavyHit = wasHeavy;

            }
            else if (attackHitbox.targetEnemy.CompareTag("Skeleton"))
            {
                attackHitbox.targetEnemy.GetComponentInParent<SkeletonBehavior>().TakeDamage(t_damage);
                attackHitbox.targetEnemy.GetComponentInParent<SkeletonBehavior>().wasHeavyHit = wasHeavy;
            }
            Debug.Log(message: $"did hit enemy for {t_damage} damage");
        }

        private IEnumerator Scream()
        {
            screaming = true;
            Debug.Log("screaming");
            //set screaming animation and sound here
            yield return new WaitForSeconds(screamMinDuration);
            while (screamKeyHeld)
            {
                yield return new WaitForSeconds(screamBufferTime);
            }
            Debug.Log("stopped screaming");
            //end screaming animation and sound here
            screaming = false;
        }

        private IEnumerator Meow()
        {
            meow = true;
            //meow sound go here
            yield return new WaitForSeconds(meowTime);
            Debug.Log("meow");
            //and they end here
            meow = false;
        }

        #endregion

        #region data persistence
        
        public void LoadData(GameData data)
        {
            gameObject.transform.position = data.savedPosition;
            this.attackEnabled = data.attackEnabled;
            this.screamEnabled = data.screamEnabled;
            this.attackBuffs = data.attackBuffs;
            this.knockbackBuffs = data.knockbackBuffs;
        }

        public void SaveData(ref GameData data)
        {
            data.savedPosition = gameObject.transform.position;
            data.attackEnabled = this.attackEnabled;
            data.screamEnabled = this.screamEnabled;
            data.attackBuffs = this.attackBuffs;
            data.knockbackBuffs = this.knockbackBuffs;
        }

        #endregion
    }
}