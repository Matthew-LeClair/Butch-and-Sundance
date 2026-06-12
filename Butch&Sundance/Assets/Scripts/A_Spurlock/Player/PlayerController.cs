using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour, I_Damage
{

    [Header("Config")]
    [SerializeField] public CharacterController Controller;
    float Gravity = 35; // Gravity Force
    Vector3 MoveDir; // Move Direction
    Vector3 PlayerVel; // Player Velocity

    [Header("Health & Damage")]
    [SerializeField] public float Health; // Current Health
    [SerializeField] public float HealthMax; // Max Health
    [SerializeField] public float Shield; // Current Shield
    [SerializeField] public float ShieldMax; // Max Shield

    [Header("Alien Energy")]
    [SerializeField] public float AlienEnergy; // Current Alien Energy
    [SerializeField] public float AlienEnergyMax; // Max Alien Energy
    [SerializeField] public float AlienEnergyRegenRate = 5f; // How fast Alien Energy regens per second
    [SerializeField] public float AlienEnergyRegenCap = 25f; // Max regen cap (upgradable)
    [SerializeField] public float WallRunEnergyCost = 5f; // Energy cost per second of wall running
    [SerializeField] public float GrappleEnergyCost = 10f; // Energy cost per grapple
    [SerializeField] public float AimEnergyCost = 3f; // Energy cost per second of aiming
    [SerializeField] public float AbsorbRatio = 0.3f; // How much incoming damage is absorbed as energy

    [Header("Dodge")]
    [SerializeField] public float DodgeEnergyCost = 15f; // Energy cost per dodge
    [SerializeField] public float DodgeDistance = 5f; // How far the dodge travels
    [SerializeField] public float DodgeDuration = 0.2f; // How long the dodge takes
    [SerializeField] public float DodgeCooldown = 1f; // Cooldown between dodges
    private float DodgeCooldownTimer; // Current Dodge Cooldown Timer
    private bool IsDodging; // Is the Player currently Dodging

    [Header("Aim")]
    [SerializeField] public float AimFOV = 40f; // FOV when aiming
    [SerializeField] public float AimDamageMultiplier = 1.5f; // Damage multiplier when aiming

    [Header("Weapon")]
    [SerializeField] public PlayerGun pGun; // Player Gun Reference

    [Header("Gear")]
    [SerializeField] public bool HasPhaseBoots; // Unlocks Parkour and Dodge
    [SerializeField] public bool HasEnergyConverter; // Unlocks Damage Absorption

    [Header("Movement")]
    [SerializeField] public float SpeedBase; // Base Speed
    [SerializeField] public float Speed; // Current Speed
    bool IsMoving; // Is the Player currently Moving
    [SerializeField] public float JumpSpeedBase; // Base Jump Speed
    float JumpSpeed; // Current Jump Speed
    int JumpCount; // Current Jump Count

    [Header("Wall Run")]
    [SerializeField] public LayerMask WhatIsWall; // Wall Layer Mask
    [SerializeField] public LayerMask WhatIsGround; // Ground Layer Mask
    [SerializeField] public float WallRunForce; // Wall Run Force
    [SerializeField] public float MaxWallRunTime; // Max Wall Run Time
    [SerializeField] public float UpJumpForce; // Up Jump Force
    [SerializeField] public float OutJumpForce; // Out Jump Force
    private float WallRunTimer; // Current Wall Run Timer
    [SerializeField] public bool IsWallRunning; // Is the Player currently Wall Running
    private bool IsExitWallRun; // Is the Player exiting Wall Run
    [SerializeField] public float ExitTime; // Exit Wall Run Time
    private float ExitTimer; // Current Exit Timer
    [SerializeField] public float WallCheckDistance; // Wall Check Distance
    [SerializeField] public float MinJumpHeight; // Minimum Jump Height for Wall Run
    private RaycastHit LeftWall; // Left Wall Raycast Hit
    private bool IsLeftWall; // Is there a Wall on the Left
    private RaycastHit RightWall; // Right Wall Raycast Hit
    [SerializeField] public bool IsRightWall; // Is there a Wall on the Right
    [SerializeField] public float GravityCounterForce; // Counter Force against Gravity during Wall Run

    public enum RopeModeType { None, Grapple, Swing, Pull } // Available Rope Modes
    [Header("Rope Mode")]
    public RopeModeType RopeMode = RopeModeType.None; // Currently active rope mode - for reference/UI only

    [Header("Rope Input")]
    [SerializeField] public float SwingHoldThreshold = 0.2f; // Seconds Tab must be held before Swing fires instead of Grapple
    private float TabHoldTimer; // Tracks how long Tab has been held this press
    private bool IsTabHeld; // True while Tab is physically held down
    private bool SwingStartedThisHold; // Prevents Swing from firing more than once per hold

    [Header("Prediction")]
    [SerializeField] public RaycastHit PredictionHit; // Predicted Swing Point Hit
    [SerializeField] public float PredictionSphereCastRadius; // Radius of the Prediction Sphere Cast
    [SerializeField] public Transform PredictionPoint; // Visual Indicator for Predicted Swing Point
    [SerializeField] GameObject PredictionPrefab; // Prefab to spawn at Prediction Point
    private GameObject SpawnedPrediction; // Current Spawned Prediction Prefab Instance

    [Header("Hook Shot")]
    [SerializeField] GameObject Cam; // Camera GameObject Reference
    [SerializeField] public Transform CamTransform; // Camera Transform Reference
    [SerializeField] public LineRenderer GrappleLine; // Grapple Line Renderer Reference
    [SerializeField] int RopePoints = 20; // Number of points in the rope
    [SerializeField] float RopeSag = 2f; // How much the rope sags
    [SerializeField] public float MaxGrappleDistance; // Max Grapple Distance
    [SerializeField] public float GrappleDelayTime; // Delay before Grapple launches
    private Vector3 GrapplePoint; // Grapple Point in World Space
    [SerializeField] public bool IsGrapple; // Is the Player currently Grappling
    [SerializeField] public float GrappleForceMultiplier = 0.1f; // Grapple Force Multiplier
    private Rigidbody GrappleRB; // Rigidbody added dynamically during grapple - destroyed on arrival or cancel

    [Header("Swinging")]
    [SerializeField] public float MaxSwingDistance; // Max Swing Distance
    [SerializeField] public float SwingSpeed = 10f; // How fast the player swings
    [SerializeField] public float SwingLengthMin = 5f; // Minimum rope length
    [SerializeField] public float SwingLengthMax = 25f; // Maximum rope length
    [SerializeField] public float SwingLengthChangeSpeed = 5f; // How fast rope length changes
    private bool IsSwinging; // Is the Player currently Swinging
    private SpringJoint SwingJoint; // Spring Joint for Swinging
    private Rigidbody SwingRB; // Rigidbody for Swinging

    [SerializeField] public float HorizontalThrustForce; // Horizontal Thrust Force during Swing
    [SerializeField] public float ForwardThrustForce; // Forward Thrust Force during Swing
    [SerializeField] public float ExtendCableSpeed; // How fast the cable extends during Swing
    [SerializeField] public float SwingPassiveForwardForce = 8f; // Constant forward push applied when no ODM input is held

    [Header("Momentum")]
    [SerializeField] public float BaseMomentumBuildRate; // Base Momentum Build Rate
    [SerializeField] float MomentumDecayRate = 0.4f; // Momentum Decay Rate
    [SerializeField] public float MomentumBuildRate; // Current Momentum Build Rate
    [SerializeField] public float CurrMomentum = 0; // Current Momentum
    float MaxMomentum = 50; // Max Momentum
    Vector3 MomentumVelocity = Vector3.zero; // Momentum Velocity

    [Header("Object Grapple")]
    [SerializeField] float PullObjectForce;
    private PullableObject CurrentPullable;
    private Coroutine PullCoroutine;

    [Header("Tutorial")]
    public bool Aimed; // Has the Player Aimed
    public bool Shot; // Has the Player Shot
    public bool Reloaded; // Has the Player Reloaded
    public bool Moved; // Has the Player Moved
    public bool Jumped; // Has the Player Jumped

    [Header("Audio")]
    [SerializeField] AudioSource AudioPlayer;
    [SerializeField] AudioClip[] JumpSound;
    [SerializeField] float JumpSoundVol;
    [SerializeField] AudioClip[] HurtSound;
    [SerializeField] float HurtSoundVol;
    [SerializeField] AudioClip[] StepSounds;
    [SerializeField] float StepSoundVol;



    //===[Basic]===\\

    // Called once before the first frame by Unity.
    // Sets up all initial values for health, shield, momentum and the grapple line renderer.
    // This is the entry point for the player's state - everything starts here.
    public void Start()
    {
        GrappleLine = Cam.GetComponent<LineRenderer>(); // Get LineRenderer from Camera on Start
        MomentumBuildRate = BaseMomentumBuildRate; // Set Momentum Build Rate to Base on Start
        Health = HealthMax; // Set Health to Max on Start
        Shield = ShieldMax; // Set Shield to Max on Start
        SetStartPosition();
    }

    // Called every frame by Unity.
    // Acts as the master controller - handles energy regen, dodge cooldown, rope visuals, and final velocity application.
    // Rope input (Tab tap/hold, Q cancel) is handled via HandleRopeInput() which runs every frame unconditionally.
    void Update()
    {
        if (HasPhaseBoots && AlienEnergy < AlienEnergyRegenCap && !pGun.IsAiming) // Only regen if Phase Boots equipped and not Aiming
        { AlienEnergy = Mathf.Min(AlienEnergy + AlienEnergyRegenRate * Time.deltaTime, AlienEnergyRegenCap); } // Regen Alien Energy up to Cap

        if (DodgeCooldownTimer > 0) { DodgeCooldownTimer -= Time.deltaTime; } // Count Down Dodge Cooldown Timer

        if (IsSwinging && SwingJoint != null) // If the Player is Swinging and the Joint exists...
        {
            UpdateRopePoints(new Vector3(CamTransform.position.x, CamTransform.position.y - .5f, CamTransform.position.z), SwingJoint.connectedAnchor); // Update Rope Points with Sag to Swing Anchor
            OdmGearMovement(); // Handle ODM Gear Movement while Swinging
        }

        CheckForSwingPoints(); // Always check for swing points regardless of swing state

        if (IsGrapple && !IsSwinging) // Rigidbody drives physics during grapple - CharacterController is disabled
        {
            if (GrappleRB != null) // Update rope visual to follow player position
            { UpdateRopePoints(new Vector3(CamTransform.position.x, CamTransform.position.y - .5f, CamTransform.position.z), GrapplePoint); }

            if (Vector3.Distance(transform.position, GrapplePoint) < 2f) // Close enough - hard stop and clean up
            {
                GrappleLine.enabled = false; // Disable Grapple Line
                IsGrapple = false; // Stop Grappling
                MomentumVelocity = Vector3.zero; // Zero horizontal momentum
                PlayerVel = Vector3.zero; // Zero all player velocity
                if (GrappleRB != null) { Destroy(GrappleRB); GrappleRB = null; } // Destroy Rigidbody
                Controller.enabled = true; // Re-enable CharacterController
            }
        }
        else
        {
            Debug.DrawRay(CamTransform.position, CamTransform.forward * pGun.ShootDistance, Color.red); // Debug Draw Shoot Ray
            IsMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f; // Check if Player is Moving
            Movement(); // Call Movement Function
        }

        HandleRopeInput(); // Rope input runs every frame - not gated by grapple/movement state

        if (!IsSwinging && !IsGrapple) // CharacterController is disabled during both Swing and Grapple - Rigidbody drives physics in both cases
        {
            Controller.enabled = true; // Activate Controller
            Controller.Move(MomentumVelocity * Time.deltaTime); // Move Player using Momentum Velocity
            Controller.Move(PlayerVel * Time.deltaTime); // Move Player using Player Velocity
            PlayerVel.y -= Gravity * Time.deltaTime; // Apply Gravity to Player Velocity
        }
    }


    //===[Input]===\\

    // Called every frame from Movement().
    // Handles reload, aim, shoot, and dodge.
    // Rope input is no longer here - it lives in HandleRopeInput() called directly from Update().
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R)) // If R is pressed...
        { pGun.SwitchWeapons(); } // Reload the Gun

        if (Input.GetButtonDown("Fire2") && (!HasPhaseBoots || AlienEnergy > 0)) // If Mouse2 pressed and has energy or no boots...
        {
            pGun.Aim(); // Toggle Aim
            Aimed = true; // Set Aimed Flag
            if (HasPhaseBoots && !IsGrapple && !IsSwinging) // Only slow time if boots equipped and not in rope action
            {
                Time.timeScale = 0.5f; // Slow Global Time
                Time.fixedDeltaTime = 0.02f * Time.timeScale; // Keep Physics in sync
            }
        }

        if (Input.GetButtonUp("Fire2")) // If Mouse2 released...
        {
            pGun.Aim(); // Toggle Aim off
            Time.timeScale = 1f; // Restore Global Time
            Time.fixedDeltaTime = 0.02f; // Restore Physics
        }

        if (HasPhaseBoots && pGun.IsAiming && AlienEnergy > 0) // If boots equipped, aiming, and has energy...
        {
            AlienEnergy = Mathf.Max(AlienEnergy - AimEnergyCost * Time.deltaTime, 0); // Drain Alien Energy while Aiming
            if (AlienEnergy <= 0) // If out of energy...
            {
                pGun.IsAiming = false; // Force stop Aim
                Time.timeScale = 1f; // Restore Global Time
                Time.fixedDeltaTime = 0.02f; // Restore Physics
            }
        }

        if (Input.GetButton("Fire1")) { pGun.Shoot(); Shot = true; } // Mouse1 = Shoot

        if (HasPhaseBoots && Input.GetKeyDown(KeyCode.LeftShift) // If boots equipped and Shift pressed...
            && !IsDodging && DodgeCooldownTimer <= 0 && AlienEnergy >= DodgeEnergyCost) // And not dodging and has energy...
        { StartCoroutine(Dodge()); } // Start Dodge Coroutine
    }

    // Called every frame from Update() - runs unconditionally, even during a grapple launch.
    // Owns all Tab and Q rope logic.
    //
    //   Tab down             -> arm the timer, reset hold state
    //   Tab held < threshold -> waiting, nothing fires yet
    //   Tab held >= threshold -> StartSwing() fires immediately; SwingStartedThisHold blocks re-fire
    //   Tab up, was swinging -> StopSwing() - release is the natural stop
    //   Tab up, not swinging, tap (< threshold) -> StartGrapple()
    //   Tab up, hold-fired swing already resolved -> nothing (SwingStartedThisHold guards this)
    //   Q                    -> cancel active grapple or swing; does nothing when rope is idle
    void HandleRopeInput()
    {
        // --- Tab DOWN: arm the timer ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TabHoldTimer = 0f; // Reset timer on each new press
            IsTabHeld = true; // Mark Tab as physically held
            SwingStartedThisHold = false; // Clear the hold-fire guard
        }

        // --- Tab HELD: accumulate time; fire Swing the instant threshold is crossed ---
        if (IsTabHeld)
        {
            TabHoldTimer += Time.deltaTime; // Count up every frame Tab is down

            if (!SwingStartedThisHold && TabHoldTimer >= SwingHoldThreshold && PredictionHit.collider != null && PredictionHit.collider.CompareTag("Grapple"))
            {
                SwingStartedThisHold = true; // Guard - Swing only fires once per hold
                StartSwing(); // Swing launches immediately; player releases Tab later to stop it
            }
        }

        // --- Tab UP: resolve the press ---
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            IsTabHeld = false; // Tab is no longer physically held

            if (IsSwinging) // Released while swinging -> natural stop
            {
                StopSwing();
            }
            else if (!SwingStartedThisHold && !IsGrapple) // Tap (released before threshold, swing never fired) -> Grapple
            {
                StartGrapple();
            }
            // If SwingStartedThisHold is true and IsSwinging is false, swing already resolved - do nothing

            TabHoldTimer = 0f; // Always reset timer on release
            SwingStartedThisHold = false; // Always clear guard on release
        }

        // --- Q: pure cancel - kills whatever rope action is active, does nothing when idle ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsGrapple) { CancelGrapple(); } // Cancel active Grapple launch
            else if (IsSwinging) { StopSwing(); } // Hard-stop active Swing
        }
    }


    //===[Movement]===\\

    // Called every frame from Update() when not in a grapple launch.
    // Handles all grounded logic, input, jump, wall run, and momentum - the core movement loop.
    // Everything the player does physically on the ground or in the air routes through here.
    void Movement()
    {
        HandleInput(); // Handle Input

        if (Controller.isGrounded && !IsGrapple && !IsSwinging) // If Grounded and not in a rope action...
        {
            PlayerVel = new Vector3(0, 0, 0); // Reset Player Velocity on Landing
            JumpCount = 0; // Reset Jump Count on Landing
        }

        Vector3 InputDir = // Calculate Input Direction
            Input.GetAxis("Horizontal") * transform.right // Horizontal Input * Player Right
            + Input.GetAxis("Vertical") * transform.forward; // Vertical Input * Player Forward

        Jump(); // Call Jump Function
        HandleWallRun(); // Call Wall Run Function

        if (!IsWallRunning && !IsSwinging) // Only handle normal movement if not Wall Running or Swinging
        {
            if (InputDir.magnitude > 0.1f) // If there is Input...
            {
                Moved = true; // Set Moved Flag
                MoveDir = InputDir; // Only overwrite MoveDir when actively moving
                MomentumVelocity = MoveDir.normalized * (Speed + CurrMomentum * 0.1f); // Snap directly to Input Direction
            }
            else // If there is No Input...
            {
                MomentumVelocity = Vector3.MoveTowards(MomentumVelocity, Vector3.zero, Speed * 10f * Time.deltaTime); // Quickly bleed to zero on no input
            }
        }

        HandleMomentum(InputDir); // Pass Input Direction to Momentum System
    }

    // Called every frame from Movement().
    // Handles double jump and wall jump - checks jump count and wall run state to decide which jump to perform.
    // Scales jump height with current momentum so faster players jump higher.
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && JumpCount < 2) // If Jump pressed and under Jump Limit...
        {
            AudioPlayer.PlayOneShot(JumpSound[Random.Range(0, JumpSound.Length)], JumpSoundVol);
            if (!IsWallRunning) // If not Wall Running...
            {
                Jumped = true; // Set Jumped Flag
                JumpCount++; // Increment Jump Count

                AudioPlayer.PlayOneShot(JumpSound[Random.Range(0, JumpSound.Length)], JumpSoundVol);

                float MomentumBoost = CurrMomentum / MaxMomentum; // Momentum as a Percentage
                PlayerVel.y = JumpSpeedBase * (1f + MomentumBoost * 0.4f); // Scale Jump Height with Momentum

                MomentumVelocity = new Vector3(MomentumVelocity.x, 0, MomentumVelocity.z) // Strip Y from Momentum Velocity
                    * (1f + MomentumBoost * 0.3f); // Boost Horizontal on Jump
            }
            else { WallJump(); } // Call Wall Jump if Wall Running
        }
    }

    // Called from HandleInput() when Shift is pressed and conditions are met.
    // Disables the CharacterController and moves the player directly through geometry for the dodge duration.
    // Re-enables collision after the dodge and starts the cooldown timer.
    IEnumerator Dodge()
    {
        IsDodging = true; // Set Dodging State
        AlienEnergy -= DodgeEnergyCost; // Consume Energy

        Vector3 DodgeDir = MoveDir.magnitude > 0.1f ? MoveDir.normalized : transform.forward; // Dodge in move direction or forward
        float Timer = 0f; // Init Timer

        float originalYVel = PlayerVel.y;

        while (Timer < DodgeDuration) // While Dodge is active...
        {
            Timer += Time.deltaTime; // Increment Timer

            float stepSpeed = DodgeDistance / DodgeDuration;
            Vector3 DodgeMove = DodgeDir * stepSpeed * Time.deltaTime; // Calculate Dodge Move

            DodgeMove.y = originalYVel * Time.deltaTime; // Lock Y to prevent phasing through floor
            Controller.Move(DodgeMove);
            yield return null; // Wait for next frame
        }

        DodgeCooldownTimer = DodgeCooldown; // Start Cooldown
        IsDodging = false; // End Dodge
    }


    //===[Momentum]===\\

    // Called every frame from Movement().
    // Builds momentum while moving and decays it slower when stopped, then updates stats.
    // The momentum system feeds into speed and jump height scaling via HandleStats().
    void HandleMomentum(Vector3 InputDir)
    {
        if (InputDir.magnitude > 0.1f) // If the Player is Moving...
        { CurrMomentum = Mathf.MoveTowards(CurrMomentum, MaxMomentum, MomentumBuildRate * Time.deltaTime); } // Build Momentum toward Max
        else // If the Player is NOT Moving...
        { CurrMomentum = Mathf.MoveTowards(CurrMomentum, 0, (MomentumBuildRate * MomentumDecayRate) * Time.deltaTime); } // Decay Momentum slower than Build

        HandleStats(); // Update Stats based on Current Momentum
    }

    // Called every frame from HandleMomentum().
    // Scales Speed and JumpSpeed based on current momentum percentage.
    // This is what makes the player feel faster the longer they move.
    void HandleStats()
    {
        float MomentumPercent = CurrMomentum / MaxMomentum; // Momentum as a Percentage
        Speed = Mathf.Lerp(SpeedBase, 10f, MomentumPercent); // Scale Speed between Base and 2x
        JumpSpeed = Mathf.Lerp(JumpSpeedBase, JumpSpeedBase * 1.5f, MomentumPercent); // Scale Jump between Base and 1.5x
    }


    //===[Damage]===\\

    // Called by enemies or hazards via the I_Damage interface.
    // Handles shield absorption first, then health damage with momentum reduction and optional energy absorption.
    // Triggers death if health hits zero, and flashes the appropriate screen effect.
    public void TakeDamage(int Amount, bool AlienTech)
    {
        if (HasEnergyConverter) // Only absorb damage if Energy Converter is equipped
        {
            float AbsorbedAmount = Amount * AbsorbRatio; // Calculate Absorbed Amount
            AlienEnergy = Mathf.Min(AlienEnergy + AbsorbedAmount, AlienEnergyMax); // Add to Alien Energy
            Amount = (int)(Amount - AbsorbedAmount); // Reduce actual damage by absorbed amount
        }

        if (Shield > 0) { Shield -= Amount; } // Absorb damage with Shield first

        if (Shield <= 0) // If Shield is depleted...
        {
            if (CurrMomentum > 0) // If Player has Momentum...
            {
                float DamageRatio = Mathf.Clamp01((float)Amount / Health); // Calculate Damage Ratio
                CurrMomentum *= (1f - DamageRatio); // Reduce Momentum by Damage Ratio
            }

            Health -= Amount; // Subtract Health by Amount

            AudioPlayer.PlayOneShot(HurtSound[Random.Range(0, HurtSound.Length)], HurtSoundVol);

            if (Health <= 0) // If Health is Zero or less...
            {
                GameManager.Instance.YouLose(); // Trigger Lose State
                Death(); // Call Death Function
            }
            StartCoroutine(FlashDamageScreen()); // Flash Damage Screen
        }
        else { StartCoroutine(FlashShieldScreen());
        } // Flash Shield Screen if Shield absorbed damage

        UpdatePlayerUI(); // Update Player UI
    }

    // Called from TakeDamage() when Health reaches zero.
    // Stops play in editor or quits the application in a build.
    // Wrapped in preprocessor directives so it works correctly in both environments.
    public void Death()
    {
        Debug.Log(Health); // Debug Print Health
        GameManager.Instance.YouLose(); // Lose UI
        ChangeRespawnPos();
    }


    //===[UI]===\\

    // Called from Start(), TakeDamage(), and any stat change.
    // Updates the HP and Shield bar fill amounts on the HUD.
    // Should be called any time Health or Shield values change so the UI stays in sync.
    public void UpdatePlayerUI()
    {
        if (GameManager.Instance.PlayerHP_Bar != null && GameManager.Instance.PlayerShieldHP_Bar != null
            && GameManager.Instance.AlienEnergy_Bar != null)
        {
            GameManager.Instance.PlayerHP_Bar.fillAmount = (float)Health / HealthMax; // Update HP Bar
            GameManager.Instance.PlayerShieldHP_Bar.fillAmount = (float)Shield / ShieldMax; // Update Shield Bar
            GameManager.Instance.AlienEnergy_Bar.fillAmount = AlienEnergy / AlienEnergyMax; // Update Alien Energy Bar
        }

    }

    // Called from TakeDamage() when health damage is taken.
    // Briefly activates the damage screen overlay then deactivates it after half a second.
    // Gives the player visual feedback that they took damage to their health.
    IEnumerator FlashDamageScreen()
    {
        GameManager.Instance.PlayerDamage_Screen.SetActive(true); // Activate Damage Screen
        yield return new WaitForSeconds(0.5f); // Wait half a second
        GameManager.Instance.PlayerDamage_Screen.SetActive(false); // Deactivate Damage Screen
    }

    // Called from TakeDamage() when shield damage is taken.
    // Briefly activates the shield screen overlay then deactivates it after half a second.
    // Gives the player visual feedback that their shield absorbed the hit.
    IEnumerator FlashShieldScreen()
    {
        GameManager.Instance.PlayerShield_Screen.SetActive(true); // Activate Shield Screen
        yield return new WaitForSeconds(0.5f); // Wait half a second
        GameManager.Instance.PlayerShield_Screen.SetActive(false); // Deactivate Shield Screen
    }


    //===[Wall Run]===\\

    // Called every frame from HandleWallRun().
    // Fires two raycasts left and right from the player to detect nearby walls.
    // Stores the results in IsRightWall and IsLeftWall for the wall run check to use.
    void CheckForWall()
    {
        IsRightWall = Physics.Raycast(transform.position, transform.right, out RightWall, WallCheckDistance, WhatIsWall); // Check for Wall on Right
        IsLeftWall = Physics.Raycast(transform.position, -transform.right, out LeftWall, WallCheckDistance, WhatIsWall); // Check for Wall on Left
    }

    // Called every frame from HandleWallRun().
    // Fires a downward raycast to check if the player is high enough off the ground to wall run.
    // Returns true if no ground is detected within MinJumpHeight.
    bool AboveGround()
    { return !Physics.Raycast(transform.position, Vector3.down, MinJumpHeight, WhatIsGround); } // Check if Above Ground

    // Called every frame from Movement().
    // Manages the full wall run lifecycle - timer, wall detection, state toggling, and movement.
    // Also handles the exit timer that prevents immediately re-attaching to a wall after jumping off.
    void HandleWallRun()
    {
        if (!HasPhaseBoots) return; // Wall Run requires Phase Boots

        if(IsExitWallRun)
        {
            ExitTimer += Time.deltaTime;

            if(ExitTimer >= ExitTime)
            {
                IsExitWallRun = false;
                ExitTimer = 0f;
            }

            return;
        }

        CheckForWall();

        bool wallDetected = IsRightWall || IsLeftWall;

        if (wallDetected && !Controller.isGrounded && AboveGround() && MoveDir.magnitude > 0.1f)
        {
            IsWallRunning = true;
            WallRunTimer += Time.deltaTime;

            if(WallRunTimer >= MaxWallRunTime)
            {
                IsWallRunning = false;
                WallRunTimer = 0f;
                return;
            }

            WallRunMovement();
            JumpCount = 0;
        }
        else
        {
            IsWallRunning = false;
            WallRunTimer = 0f;
        }
    }

    // Called every frame from HandleWallRun() while IsWallRunning is true.
    // Applies counter gravity, calculates the wall forward direction, and uses camera vertical tilt for diagonal movement.
    // Drains Alien Energy each frame and stops the wall run if energy runs out.
    void WallRunMovement()
    {
        // Stop Wall Run if no Energy
        if (AlienEnergy <= 0) 
        { 
            IsWallRunning = false; 
            return;
        }
        AlienEnergy = Mathf.Max(AlienEnergy - WallRunEnergyCost * Time.deltaTime, 0); // Drain Energy while Wall Running

        Vector3 wallNormal = IsRightWall ? RightWall.normal : LeftWall.normal; // Get Wall Normal
        wallNormal.Normalize();

        Vector3 wallTangent = Vector3.Cross(wallNormal, Vector3.up);

        if(Vector3.Dot(wallTangent, transform.forward) < 0)
        {
            wallTangent = -wallTangent;
        }

        float inputVertical = Input.GetAxis("Vertical");
        Vector3 alongWallMove = wallTangent * inputVertical * Speed;

        float targetFallSpeed = -2.5f;

        PlayerVel.y = Mathf.MoveTowards(PlayerVel.y, targetFallSpeed, Gravity * 0.6f * Time.deltaTime);

        MomentumVelocity = new Vector3(alongWallMove.x, 0f, alongWallMove.z);
    }

    // Called from Jump() when the player jumps while wall running.
    // Launches the player away from the wall using UpJumpForce and OutJumpForce relative to the wall normal.
    // Resets momentum velocity and starts the exit timer to prevent immediately re-entering the wall run.
    void WallJump()
    {
        IsWallRunning = false; // Stop Wall Running
        WallRunTimer = 0; // Reset Wall Run Timer

        IsExitWallRun = true; // Enter Exit State
        ExitTimer = 0f;

        Vector3 Normal = IsRightWall ? RightWall.normal : LeftWall.normal; // Get Wall Normal
        Vector3 ForceToApply = transform.up * UpJumpForce + Normal * OutJumpForce; // Calculate Jump Force

        PlayerVel = ForceToApply; // Apply Jump Force to Player Velocity
        MomentumVelocity = Vector3.zero; // Zero out Momentum Velocity on Wall Jump
    }


    //===[Hook Shot]===\\

    // Called from HandleRopeInput() on a Tab tap (released before SwingHoldThreshold).
    // Uses the pre-calculated PredictionHit from CheckForSwingPoints() instead of firing a new raycast.
    // This means the player always grapples to exactly where the prediction indicator is showing.
    void StartGrapple()
    {
        if (!HasPhaseBoots) return; // Grapple requires Phase Boots
        if (PredictionHit.collider == null) return;
        
        if (PredictionHit.collider.CompareTag("Pullable"))
        {
            RopeMode = RopeModeType.Pull;
            PullableObject pullable = PredictionHit.collider.GetComponent<PullableObject>();

            if (pullable != null)
            {
                CurrentPullable = pullable;
                PullCoroutine = StartCoroutine(PullObject(pullable));
                return;
            }
        }

        if (!PredictionHit.collider.CompareTag("Grapple")) return; 
        
        RopeMode = RopeModeType.Grapple;

        IsGrapple = true; // Set Grapple State
        AlienEnergy -= GrappleEnergyCost; // Consume Energy on Grapple
        GrapplePoint = PredictionHit.point; // Set Grapple Point from Prediction

        Controller.enabled = false; // Disable CharacterController - GrappleRB drives physics during flight
        GrappleRB = gameObject.AddComponent<Rigidbody>(); // Add Rigidbody dynamically
        GrappleRB.freezeRotation = true; // Prevent Rotation
        GrappleRB.linearVelocity = MomentumVelocity; // Carry existing momentum into grapple

        GrappleLine.enabled = true; // Enable Grapple Line
        GrappleLine.positionCount = RopePoints; // Set Rope Points
        StartCoroutine(ExtendGrappleLine(GrapplePoint)); // Extend Rope to Grapple Point

        Debug.Log("Grapple Started at: " + GrapplePoint); // Debug Log
    }

    // Called from HandleRopeInput() when Q is pressed and IsGrapple is true.
    // Zeros velocity, destroys the GrappleRB, re-enables the CharacterController, and clears the line.
    // Used as an emergency cancel so the player is never locked into a grapple.
    void CancelGrapple()
    {
        if (PullCoroutine != null)
        {
            StopCoroutine(PullCoroutine);
            PullCoroutine = null;
        }
        CurrentPullable = null;
        
        GrappleLine.enabled = false; // Disable Grapple Line
        IsGrapple = false; // Stop Grappling
        MomentumVelocity = Vector3.zero; // Zero out Momentum Velocity
        PlayerVel = Vector3.zero; // Zero all player velocity
        if (GrappleRB != null) { Destroy(GrappleRB); GrappleRB = null; } // Destroy Rigidbody
        Controller.enabled = true; // Re-enable CharacterController

        RopeMode = RopeModeType.None;
    }

    // Called from StartGrapple() as a coroutine.
    // Lerps the line renderer endpoint from the camera to the grapple point over GrappleDelayTime.
    // Once the line is fully extended, launches the player toward the grapple point.
    IEnumerator ExtendGrappleLine(Vector3 Target)
    {
        float Timer = 0f; // Init Timer
        GrappleLine.positionCount = RopePoints; // Set number of rope points

        while (Timer < GrappleDelayTime) // While line is extending...
        {
            Timer += Time.deltaTime; // Increment Timer
            float Percent = Timer / GrappleDelayTime; // 0 to 1 over Delay Time
            UpdateRopePoints(CamTransform.position, Vector3.Lerp(CamTransform.position, Target, Percent)); // Extend Rope to Target
            yield return null; // Wait for next frame
        }

        Vector3 GrappleDir = (GrapplePoint - transform.position).normalized; // Direction to Grapple Point
        float GrappleDist = Vector3.Distance(transform.position, GrapplePoint); // Distance to Grapple Point
        float LaunchForce = GrappleDist * GrappleForceMultiplier; // Force purely based on distance
        if (GrappleRB != null) { GrappleRB.linearVelocity = GrappleDir * LaunchForce; } // Launch via Rigidbody - covers all angles uniformly
        Debug.Log("Grapple Launch Force: " + LaunchForce); // Debug Log
    }

    // Called from Update() and ExtendGrappleLine() every frame while a rope is active.
    // Distributes RopePoints evenly between Start and End with a sine-based sag offset applied on Y.
    // Used by both grapple and swing to keep the rope visual consistent.
    void UpdateRopePoints(Vector3 Start, Vector3 End)
    {
        for (int i = 0; i < RopePoints; i++) // Loop through each Rope Point
        {
            float t = i / (float)(RopePoints - 1); // 0 to 1 along rope
            Vector3 Point = Vector3.Lerp(Start, End, t); // Linear point along rope
            float Sag = Mathf.Sin(t * Mathf.PI) * RopeSag * (1f - t * 0.5f); // Sag peaks in middle
            Point.y -= Sag; // Apply sag downward
            GrappleLine.SetPosition(i, Point); // Set rope point position
        }
    }

    bool IsValidGrappleTarget(RaycastHit hit)
    {
        if(hit.collider == null) return false;

        return hit.collider.CompareTag("Grapple") || hit.collider.CompareTag("Pullable");
    }


    //===[Swing]===\\

    // Called from HandleRopeInput() when Tab is held past SwingHoldThreshold.
    // Uses the pre-calculated PredictionHit from CheckForSwingPoints() instead of firing a new raycast.
    // Swing launches immediately at the moment the threshold is crossed - no waiting for Tab release.
    void StartSwing()
    {
        if (!HasPhaseBoots) return; // Swing requires Phase Boots

        if (PredictionHit.point == Vector3.zero) return; // No valid swing point found

        if (SpawnedPrediction != null) { Destroy(SpawnedPrediction); SpawnedPrediction = null; } // Destroy Prediction Prefab on Swing Start

        IsSwinging = true; // Set Swinging State
        IsGrapple = false; // Ensure Grapple is not active during Swing

        Controller.enabled = false; // Disable CharacterController - let SpringJoint drive physics

        SwingRB = gameObject.AddComponent<Rigidbody>(); // Add Rigidbody dynamically
        SwingRB.freezeRotation = true; // Prevent Rotation
        SwingRB.linearVelocity = MomentumVelocity; // Carry existing momentum into swing

        SwingJoint = gameObject.AddComponent<SpringJoint>(); // Add SpringJoint dynamically
        SwingJoint.autoConfigureConnectedAnchor = false; // Manual anchor control
        SwingJoint.connectedAnchor = PredictionHit.point; // Set Swing Point as anchor

        float DistFromPoint = Vector3.Distance(transform.position, PredictionHit.point); // Distance to Swing Point
        SwingJoint.maxDistance = DistFromPoint * 0.8f; // Set Max Distance
        SwingJoint.minDistance = DistFromPoint * 0.25f; // Set Min Distance
        SwingJoint.spring = 4.5f; // Set Spring Force
        SwingJoint.damper = 7f; // Set Damper Force
        SwingJoint.massScale = 4.5f; // Set Mass Scale

        GrappleLine.enabled = true; // Enable Grapple Line
        GrappleLine.positionCount = RopePoints; // Set Rope Points
        Debug.Log("Swing Started at: " + PredictionHit.point); // Debug Log
    }

    // Called from HandleRopeInput() on Tab release while swinging, or on Q press while swinging.
    // Destroys the SpringJoint and Rigidbody, re-enables the CharacterController, and carries swing velocity back into the momentum system.
    // This ensures the player doesn't lose their speed when transitioning out of a swing.
    void StopSwing()
    {
        IsSwinging = false; // Stop Swinging

        if (SwingRB != null) // If Rigidbody exists...
        {
            MomentumVelocity = new Vector3(SwingRB.linearVelocity.x, 0, SwingRB.linearVelocity.z); // Carry horizontal swing velocity into Momentum
            PlayerVel.y = SwingRB.linearVelocity.y; // Carry vertical swing velocity into Player Velocity
        }

        if (SwingJoint != null) { Destroy(SwingJoint); SwingJoint = null; } // Remove SpringJoint
        if (SwingRB != null) { Destroy(SwingRB); SwingRB = null; } // Remove Rigidbody

        Controller.enabled = true; // Re-enable CharacterController

        GrappleLine.enabled = false; // Disable Grapple Line
        RopeMode = RopeModeType.None;

        Debug.Log("Swing Stopped"); // Debug Log
    }

    // Called every frame from Update() while IsSwinging is true and SwingJoint exists.
    // Applies directional thrust forces to the Rigidbody based on WASD input, and handles cable shortening and extending via Space and Ctrl.
    // This is what gives the swing its active movement feel - without it the player just hangs passively.
    void OdmGearMovement()
    {
        // When no ODM input is held, push constantly in the camera's horizontal forward direction.
        // This carries the player over gaps naturally without any input required.
        bool NoOdmInput = !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A)
                       && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D)
                       && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftControl);

        if (NoOdmInput)
        {
            Vector3 CamFlat = new Vector3(CamTransform.forward.x, 0f, CamTransform.forward.z).normalized; // Strip Y so force is purely horizontal
            SwingRB.AddForce(CamFlat * SwingPassiveForwardForce * Time.deltaTime); // Push forward at constant rate
        }

        if (Input.GetKey(KeyCode.D)) // Right thrust
        { SwingRB.AddForce(transform.right * HorizontalThrustForce * Time.deltaTime); } // Add Right Force to Rigidbody

        if (Input.GetKey(KeyCode.A)) // Left thrust
        { SwingRB.AddForce(-transform.right * HorizontalThrustForce * Time.deltaTime); } // Add Left Force to Rigidbody

        if (Input.GetKey(KeyCode.W)) // Forward thrust
        { SwingRB.AddForce(transform.forward * ForwardThrustForce * Time.deltaTime); } // Add Forward Force to Rigidbody

        if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.Space)) // Backward thrust - only if not shortening cable
        { SwingRB.AddForce(-transform.forward * ForwardThrustForce * Time.deltaTime); } // Add Backward Force to Rigidbody

        // Mouse look influence - swing in the direction the camera is facing
        float MouseX = Input.GetAxisRaw("Mouse X"); // Get Mouse X Input
        float MouseY = Input.GetAxisRaw("Mouse Y"); // Get Mouse Y Input
        Vector3 CameraInfluence = CamTransform.right * MouseX + CamTransform.up * MouseY; // Combine into Camera Influence
        SwingRB.AddForce(CameraInfluence * HorizontalThrustForce * 0.5f * Time.deltaTime); // Apply Camera Influence Force

        if (Input.GetKey(KeyCode.Space)) // Shorten cable
        {
            Vector3 DirToPoint = SwingJoint.connectedAnchor - transform.position; // Direction to Swing Point
            SwingRB.AddForce(DirToPoint.normalized * ForwardThrustForce * Time.deltaTime); // Pull toward Swing Point
            float DistFromPoint = Vector3.Distance(transform.position, SwingJoint.connectedAnchor); // Current Distance to Swing Point
            SwingJoint.maxDistance = DistFromPoint * 0.8f; // Shorten Max Distance
            SwingJoint.minDistance = DistFromPoint * 0.25f; // Shorten Min Distance
        }

        if (Input.GetKey(KeyCode.LeftControl)) // Extend cable
        {
            float ExtendedDist = Vector3.Distance(transform.position, SwingJoint.connectedAnchor) + ExtendCableSpeed; // Extended Distance
            SwingJoint.maxDistance = ExtendedDist * 0.8f; // Extend Max Distance
            SwingJoint.minDistance = ExtendedDist * 0.25f; // Extend Min Distance
        }
    }

    // Called every frame from Update() regardless of swing state.
    // Fires both a raycast and a spherecast from the camera forward to detect potential swing and grapple points.
    // Hides the prediction indicator during an active grapple launch so it only shows when the player can actually fire.
    void CheckForSwingPoints()
    {
        if (SwingJoint != null) return;
        if (IsGrapple) return;

        RaycastHit rayHit;
        RaycastHit sphereHit;

        bool hasRay = Physics.Raycast(CamTransform.position, CamTransform.forward, out rayHit, MaxSwingDistance);

        bool hasSphere = Physics.SphereCast(CamTransform.position, PredictionSphereCastRadius, CamTransform.forward, out sphereHit, MaxSwingDistance);

        RaycastHit bestHit = hasRay ? rayHit : sphereHit;

        bool hasValidHit = (hasRay && IsValidGrappleTarget(rayHit)) || (hasSphere && IsValidGrappleTarget(sphereHit));

        if (!hasValidHit)
        {
            ClearPrediction();
            PredictionHit = default;
            return;
        }

        PredictionHit = hasRay && IsValidGrappleTarget(rayHit ) ? rayHit : sphereHit;

        UpdatePredictionVisual(PredictionHit.point);
    }

    IEnumerator PullObject(PullableObject pullable)
    {
        Debug.Log("PULL STARTED");

        IsGrapple = true;

        GrappleLine.enabled = true;
        GrappleLine.positionCount = RopePoints;

        while (pullable != null)
        {
            pullable.PullTowards(transform.position, PullObjectForce);

            UpdateRopePoints(new Vector3(CamTransform.position.x, CamTransform.position.y - .5f, CamTransform.position.z), pullable.transform.position);


            if (Vector3.Distance(transform.position, pullable.transform.position) < 2f)
            {
                break;
            }

            yield return null;
        }
        GrappleLine.enabled = false;

        CurrentPullable = null;
        PullCoroutine = null;
        IsGrapple = false;
        RopeMode = RopeModeType.None;

    }

    void UpdatePredictionVisual(Vector3 point)
    {
        if(SpawnedPrediction == null && PredictionPrefab != null)
        {
            SpawnedPrediction = Instantiate(PredictionPrefab, point, Quaternion.identity);
            PredictionPoint = SpawnedPrediction.transform;
        }
        else if (SpawnedPrediction != null)
        {
            SpawnedPrediction.transform.position = point;
        }
    }

    void ClearPrediction()
    {
        if (SpawnedPrediction != null)
        {
            Destroy(SpawnedPrediction);
            SpawnedPrediction = null;
            PredictionPoint = null;
        }
    }

    public void SetStartPosition()
    {
        Controller.enabled = false;
        transform.position = GameManager.Instance.PlayerStartPos.transform.position;
        Controller.enabled = true;
        Physics.SyncTransforms();
        Health = HealthMax;
        UpdatePlayerUI();
    }

    public void ChangeRespawnPos()
    {
        Controller.enabled = false;
        transform.position = GameManager.Instance.RespawnPosition;
        Controller.enabled = true;
        Physics.SyncTransforms(); // Ensure CharacterController is in sync with new position
        Health = HealthMax; // Restore Health on Respawn
        UpdatePlayerUI(); // Update UI to reflect restored health
    }
}