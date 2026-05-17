using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour, I_Damage
{
    [Header("Config")]
    [SerializeField] public CharacterController Controller;

    [SerializeField] Renderer Render;
    Color OriginalColor;

    float Gravity = 35;

    Vector3 MoveDir;
    Vector3 PlayerVel;

    [Header("Health & Damage")]
    [SerializeField] public float Health;
    public float HealthMax;

    [SerializeField] public float Shield;
    public float ShieldMax;

    [SerializeField] public float AlienEnergy;
    public float AlienEnergyMax;

    [Header("Weapon")]
    public PlayerGun pGun;

    [Header("Gear")]


    [Header("Movement")]
    [SerializeField] public float SpeedBase;
    public float Speed;

    bool IsMoving;
    public float JumpSpeedBase;
    float JumpSpeed;
    int JumpCount;

    [Header("Parkour")]
    // Wallrun
    public LayerMask WhatIsWall;
    public LayerMask WhatIsGround;
    public float WallRunForce;
    public float MaxWallRunTime;
    public float UpJumpForce;
    public float OutJumpForce;
    private float WallRunTimer;
    private bool IsWallRunning;

    public float WallCheckDistance;
    public float MinJumpHeight;
    private RaycastHit LeftWall;
    private bool IsLeftWall;
    private RaycastHit RightWall;
    private bool IsRightWall;
    private bool UseGravity;





    [Header("Momemtum")]
    [SerializeField] public float BaseMomentumBuildRate;
    [SerializeField] float MomentumDecayRate = 0.4f;

    public float MomentumBuildRate;
    public float CurrMomentum = 0;
    float MaxMomentum = 50;

    Vector3 MomentumVelocity = Vector3.zero;


    [Header("Parkour")]

    [Header("Tutorial")]
    public bool Aimed;
    public bool Shot;
    public bool Reloaded;
    public bool Moved;
    public bool Jumped;


    //===[Basic]===\\

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {

        // Set the Material Color as the Original Color, Modular Version
        Render.material.color = OriginalColor;

        MomentumBuildRate = BaseMomentumBuildRate;
        Health = HealthMax;

        UpdatePlayerUI();
    }

    // Update is called once per frame
    public void Update()
    {
        IsMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f
            || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
        
        Movement(); // Moves once per frame for better smoothing
    }



    //===[Movement]===\\

    void Movement()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        { pGun.Reload(); Reloaded = true; }

        if (Input.GetButtonDown("Fire2")) { pGun.Aim(); Aimed = true; }
        if (Input.GetButtonUp("Fire2")) { pGun.Aim(); }

        if (Input.GetButton("Fire1") && pGun.IsAiming)
        { pGun.Shoot(); Shot = true; }

        if (Controller.isGrounded)
        {
            PlayerVel = new Vector3(0, 0, 0); // Reset all axes on landing
            JumpCount = 0; // Reset Jump Count on Landing
        }

        Vector3 InputDir =
            Input.GetAxis("Horizontal") * transform.right
            + Input.GetAxis("Vertical") * transform.forward;

        HandleWallRun();
        
        if (!IsWallRunning)
        {
            if (InputDir.magnitude > 0.1f) // If there is Input...
            {
                Moved = true;
                MoveDir = InputDir; // Only overwrite MoveDir when actively moving
                MomentumVelocity = MoveDir.normalized * (Speed + CurrMomentum * 0.1f); // Snap directly to Input Direction
            }
            else // If there is No Input...
            {
                MomentumVelocity = Vector3.MoveTowards(
                    MomentumVelocity,
                    Vector3.zero,
                    Speed * 10f * Time.deltaTime); // Quickly bleed to zero on no input
            }
        }

        HandleMomentum(InputDir); // Pass Input Direction to Momentum System

        Controller.Move(MomentumVelocity * Time.deltaTime); // Move Player using Momentum Velocity

        Jump();

        Controller.Move(PlayerVel * Time.deltaTime);
        if (UseGravity) { PlayerVel.y -= Gravity * Time.deltaTime; }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && JumpCount < 2) // Allow Double Jump
        {
            if (!IsWallRunning)
            {
                Jumped = true;
                JumpCount++;

                float MomentumBoost = CurrMomentum / MaxMomentum; // Momentum as a Percentage
                PlayerVel.y = JumpSpeedBase * (1f + MomentumBoost * 0.4f); // Scale Jump Height with Momentum

                MomentumVelocity = new Vector3(
                    MomentumVelocity.x, 0, MomentumVelocity.z) // Strip Y from Momentum Velocity
                    * (1f + MomentumBoost * 0.3f); // Boost Horizontal on Jump
            } else { WallJump(); }
        }
    }



    //===[Momentum]===\\
    void HandleMomentum(Vector3 InputDir)
    {
        if (InputDir.magnitude > 0.1f) // If the Player is Moving...
        {
            CurrMomentum = Mathf.MoveTowards(
                CurrMomentum, MaxMomentum,
                MomentumBuildRate * Time.deltaTime); // Build Momentum toward Max
        }
        else // If the Player is NOT Moving...
        {
            CurrMomentum = Mathf.MoveTowards(
                CurrMomentum, 0,
                (MomentumBuildRate * MomentumDecayRate) * Time.deltaTime); // Decay Momentum slower than Build
        }

        HandleStats();
    }

    void HandleStats()
    {
        float MomentumPercent = CurrMomentum / MaxMomentum; // Momentum as a Percentage
        Speed = Mathf.Lerp(SpeedBase, SpeedBase * 2f, MomentumPercent); // Scale Speed between Base and 2x
        JumpSpeed = Mathf.Lerp(JumpSpeedBase, JumpSpeedBase * 1.5f, MomentumPercent); // Scale Jump between Base and 1.5x
    }



    //===[Damage]===\\
    public void TakeDamage(int Amount, bool AlienTech) 
    {
        Shield -= Amount;
        if (Shield <= 0)
        {
            if (CurrMomentum > 0)
            {
                float DamageRatio = Mathf.Clamp01((float)Amount / Health);
                CurrMomentum *= (1f - DamageRatio);
            }

            Health -= Amount; // Subtract Health by Amount
            if (Health <= 0) // If Health is Less Than or Equal To 0...
            { Death(); } // Destroy the Object
            else { StartCoroutine(Flash()); } // Call the Flash Function, Modular Version
            StartCoroutine(FlashDamageScreen());
        } else { }


    }
    IEnumerator Flash()
    {
        Transform tPart = gameObject.transform;

        if (tPart == null) { yield break; }

        Renderer rPart = tPart.GetComponent<Renderer>();

        if (rPart == null) { yield break; }

        // Flash Body Part
        rPart.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        // Reset Body Part
        rPart.material.color = OriginalColor;
    }

    public void Death()
    {
        Debug.Log(Health);

        #if UNITY_EDITOR // If in Unity Editor...
        UnityEditor.EditorApplication.isPlaying = false; // Quit Debug
        #else // If NOT in Unity Editor...      Quit Game.
                        Application.Quit(); 
        #endif
    }


    //===[UI]===\\
    public void UpdatePlayerUI()
    {
        GameManager.Instance.PlayerHP_Bar.fillAmount = (float)Health / HealthMax;
    }

    IEnumerator FlashDamageScreen()
    {
        GameManager.Instance.PlayerDamage_Screen.SetActive(true); // Activate the damage screen effect to indicate that the player has taken damage

        yield return new WaitForSeconds(0.5f); // Wait for a short duration (0.5 seconds) before deactivating the damage screen effect

        GameManager.Instance.PlayerDamage_Screen.SetActive(false); // Deactivate the damage screen effect after the wait time has elapsed
    }

    IEnumerator FlashShieldScreen()
    {
        GameManager.Instance.PlayerDamage_Screen.SetActive(true); // Activate the damage screen effect to indicate that the player has taken damage

        yield return new WaitForSeconds(0.5f); // Wait for a short duration (0.5 seconds) before deactivating the damage screen effect

        GameManager.Instance.PlayerDamage_Screen.SetActive(false); // Deactivate the damage screen effect after the wait time has elapsed
    }



    void CheckForWall()
    {
        IsRightWall = Physics.Raycast(transform.position, transform.right, out RightWall, WallCheckDistance, WhatIsWall); // Check for Wall on Right
        IsLeftWall = Physics.Raycast(transform.position, -transform.right, out LeftWall, WallCheckDistance, WhatIsWall); // Check for Wall on Left
    }

    bool AboveGround()
    { return !Physics.Raycast(transform.position, Vector3.down, MinJumpHeight, WhatIsGround); } // Check if Above Ground

    void HandleWallRun()
    {
        // Step 1 -- Wall Run Timer
        if (WallRunTimer >= MaxWallRunTime) { IsWallRunning = false; WallRunTimer = 0; }
        {
            WallRunTimer += Time.deltaTime;
            
            // Step 2 -- Wall Check
            CheckForWall();

            // Step 3 -- Wall Run Check
            if ((IsRightWall || IsLeftWall) && MoveDir.magnitude > 0.1f && AboveGround() && !Controller.isGrounded)
            { IsWallRunning = true; UseGravity = false; }
            else { IsWallRunning = false; UseGravity = true; }

            // Step 4 -- Wall Run
            if (IsWallRunning) { WallRunMovement(); }
        }
    }

    void WallRunMovement()
    {
        UseGravity = false; // Cancel Gravity while Wall Running

        Vector3 Normal = IsRightWall ? RightWall.normal : LeftWall.normal; // Get Wall Normal
        Vector3 WallForward = Vector3.Cross(Normal, transform.up); // Calculate Wall Forward Direction

        if ((transform.forward - WallForward).magnitude > (transform.forward - -WallForward).magnitude)
        { WallForward = -WallForward; }

        if (Vector3.Dot(WallForward, transform.forward) < 0) // If Wall Forward is behind the Player...
        { WallForward = -WallForward; } // Flip Wall Forward Direction

        float VerticalDot = Vector3.Dot(Camera.main.transform.forward, Vector3.up); // How much Camera is pointing Up or Down (-1 to 1)
        PlayerVel.y = VerticalDot * Speed;
    }

    void WallJump()
    {
        Vector3 Normal = IsRightWall ? RightWall.normal : LeftWall.normal; // Get Wall Normal
        Vector3 ForceToApply = transform.up * UpJumpForce + Normal * OutJumpForce; // Calculate Jump Force

        PlayerVel.y = 0f; // Reset Y Velocity before Jump
        PlayerVel = ForceToApply; // Apply Jump Force to Player Velocity
        MomentumVelocity = Vector3.zero;
        IsWallRunning = false; // Stop Wall Running
    }
}
