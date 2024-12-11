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
        [SerializeField] private BoxCollider2D attackHitbox;
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
        private bool disableMovement = false;
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
        private bool attackStarted = false;     //this one helps with chaining attacks
        private bool canHeavyAttack = false;
        private bool heavyAttackCoolingDown = false;
        public int totalSlashAttacks { get; private set; } = 0;
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

        [Header("Other Const Variables")]
        private const int maxHealth = 100;
        private const int manaOrbHealAmount = 20;
        private const float invulnerableDuration = 0.7f;

        [Header("Other Dynamic Variables")]
        private int attackChain = 0;
        private float currentHealth = 100;
        public bool dead
        {
            get { return currentHealth < 0; }
        }

        private float invulnerableTime;
        public bool invulnerable
        {
            get { return invulnerableTime > 0; }
        }

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

            OverrideNonAttackStates();

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
            else
            {
                HandleAirDrag();
                attacking = false;
                bigAttacking = false;
            }

            //if attacking, check for enemies inside hitbox
            if (attacking || bigAttacking)
            {
                CheckForHitEnemies();
            }

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
            if (!attacking && timeSinceLastAttack < chainAttacksThreshold) TimeAttacks();

            if (invulnerable)
            invulnerableTime -= Time.deltaTime;
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
            else if (collision.gameObject.CompareTag("Mana"))
            {
                currentHealth += manaOrbHealAmount;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                Destroy(collision.gameObject);
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
            if (!attackEnabled || !groundSensor.grounded) return;
            if (!attackCooldown && !screaming)
            {
                Debug.Log("attack pressed");
                didAttack = true;
                attackTypeCheck = StartCoroutine(AttackTypeCheck());
            }
        }

        private void HandleAttackCancel()
        {
            if (!attackEnabled || !groundSensor.grounded) return;
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

        //this was for something i swear i just forgot and i'll never remember if this is removed
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
            else if (attacking)
            {
                //might be a "slightly" scuffed way to do chain attacks but hey it works
                if (ChainAttacks())
                {
                    machine.Set(attackStates[attackChain] as State);

                    if (attackStarted)
                    {
                        attackStarted = false;
                        attackChain++;
                    }
                    if (attackChain > 2) attackChain = 0;
                }
                else
                {
                    attackChain = 0;
                    machine.Set(attackStates[attackChain]);
                }
                if (attackChain > attackStates.Length) attackChain = 0;
            }
            else if (bigAttacking)
            {
                machine.Set(slashAttackState as State);
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
            if (disableMovement)
            {
                rb2d.velocity = Vector2.zero;
                return;
            }

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
            if (disableMovement) return;

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
            attackStarted = true;
            StopCoroutine(attackTypeCheck);
            Debug.Log(attackStates[attackChain].anim.length);

            //attack styles do their own things
            if (canHeavyAttack && !heavyAttackCoolingDown)
            {
                totalSlashAttacks++;
                bigAttacking = true;
                canHeavyAttack = false;
                heavyAttackCoolingDown = true;
                StartCoroutine(HeavyAttackCooldown());
                yield return new WaitForSeconds(slashAttackState.anim.length);
            }
            else
            {
                attacking = true;
                yield return new WaitForSeconds(attackStates[attackChain].anim.length);
            }
            attackCooldown = false;
            bigAttacking = false;
            attacking = false;
            timeSinceLastAttack = 0;
            Debug.Log(slashAttackState.anim.length);
        }

        private void CheckForHitEnemies()
        {
            EnemyBase enemyScript;
            Vector2 pos = (Vector2)transform.position + attackHitbox.offset;
            Debug.Log(pos);
            //check if enemies are within player's attack hitbox
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(pos, attackHitbox.size, 0, 1 << LayerMask.NameToLayer("Enemy"));

            if (hitEnemies == null || hitEnemies.Length == 0) return;

            for (int i = 0; i < hitEnemies.Length; i++)
            {
                if (hitEnemies[i].CompareTag("Skeleton"))
                {
                    enemyScript = hitEnemies[i].GetComponent<SkeletonBehavior>();
                }
                else if (hitEnemies[i].CompareTag("Ghost"))
                {
                    enemyScript = hitEnemies[i].GetComponent<GhostBehavior>();
                }
                else continue;
                
                if (attacking)
                {
                    enemyScript.TakeDamage(realDamage);
                    enemyScript.wasHeavyHit = false;
                }
                else
                {
                    enemyScript.TakeDamage(realDamage * heavyAttackMult);
                    enemyScript.wasHeavyHit = true;
                }
            }
        }

        private IEnumerator HeavyAttackCooldown()
        {
            yield return new WaitForSeconds(heavyAttackCooldown + slashAttackState.anim.length);
            heavyAttackCoolingDown = false;
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

        private void OverrideNonAttackStates()
        {
            if (attacking || bigAttacking)
            {
                disableMovement = true;
            }
            else
            {
                disableMovement = false;
            }
        }

        #endregion

        #region combat handling

        public void DamagePlayer(int _damage)
        {
            currentHealth -= _damage;
            invulnerableTime = invulnerableDuration;
            Debug.Log($"Player took {_damage} damage");
            Debug.Log($"Player is dead: {dead}");
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