using UnityEngine;
using System.Collections;

public class PlayerController : CharacterBase
{
    [SerializeField] int Health;

    int HealthMax;

    [SerializeField] CharacterController Controller;

    [SerializeField] public float SpeedBase;
    public float Speed;

    [SerializeField] public float BaseMomentumBuildRate;
    public float MomentumBuildRate;
    float CurrMomentum = 0;
    float MaxMomentum = 50;

    int KillStreak;
    int ParkourCount;

    float TimeMove;
    [SerializeField] float TimeMoveDecayRate;
    float TimeMoveDecayTimer;

    [SerializeField] float KillStreakDecayRate;
    float KillStreakDecayTimer;

    int ParkourCombo;
    [SerializeField] float ParkourComboDecayRate;
    float ParkourComboDecayTimer;

    bool IsMoving;
    public float JumpSpeedBase;
    float JumpSpeed;
    int JumpMax;
    [SerializeField] int TrueJumpMax;
    int JumpCount;
    float Gravity = 35;

    Vector3 MoveDir;
    Vector3 PlayerVel;


    public bool Aimed;
    public bool Shot;
    public bool Reloaded;
    public bool Moved;
    public bool Jumped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        MomentumBuildRate = BaseMomentumBuildRate;
        CurrHealth = MaxHealth;

        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        IsMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f
            || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
        if (GameManager.Instance != null) { HandleMomentum(); }

        HandleMomentum(); // Moves once per frame for better smoothing

        Movement(); // Moves once per frame for better smoothing
    }

    void Movement() 
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        { Weapon_R.Reload(); Reloaded = true; }

        if (Input.GetButtonDown("Fire2")) { Aim(); Aimed = true; }
        if (Input.GetButtonUp("Fire2")) { Aim(); }

        if (Input.GetButton("Fire1") && IsAiming) // If Left Click is Pressed while Aiming...
        { Weapon_R.Shoot("Player"); Shot = true; } // Call Shoot Function

        if (Controller.isGrounded)  // Checks if the Player Character is on the ground
        { 
            JumpCount = 0;  // Resets the Jump Count
            PlayerVel.y = 0; // Resets the Player Velocity Y to help processes
        }

        MoveDir = // Move Direction Equals The Following Equation
            Input.GetAxis("Horizontal") // Get the Horizontal Axis
            * transform.right  // Multi it by the Players Right Transform
            +  // Add that to...
            Input.GetAxis("Vertical") // Get the Vertical Axis
            * transform.forward;  // Multi it by the Players Forward Transform

        // Use the Controller to Move the Player using Move Direction Var * Speed
        Controller.Move(MoveDir.normalized * Speed * Time.deltaTime); // Make it time relative by adding a Multi Delta Time

        if (MoveDir.normalized.Equals(new Vector3(0, 0, 0))) { } else { Moved = true; }

            Jump(); // Call Jump Function in Movement

        Controller.Move(PlayerVel * Time.deltaTime); // Move the Player in the Jump Direction

        PlayerVel.y -= Gravity * // Decrement the Player Velocity Y by Gravity
            Time.deltaTime; // Relative To Time
    }

    void Jump() 
    {
        if (Input.GetButtonDown("Jump") // If Jump button is pressed...
            && JumpCount < JumpMax) // AND Jump Count is NOT more than Jump Max
        {
            Jumped = true;
            JumpCount++; // Increment Jump Count
            PlayerVel.y = JumpSpeed; // Set Player Velocity Y to Jump Speed
        } 
    }

    void HandleMomentum()
    {
        HandleKillStreak();
        HandleParkourCountCombo();
        HandleTimeMove();

        float MomentumGain =
            ((TimeMove / 60f) * 0.5f +        // Slow - Movement Baseline
            (ParkourCombo / 100f) * 2f +      // Mid - Rewards Parkour
            (KillStreak / 100f) * 5f)         // Fast - Kills Spike Momentum Hard
            * Time.deltaTime * MomentumBuildRate;

        CurrMomentum = Mathf.Clamp(CurrMomentum + MomentumGain, 0, MaxMomentum);
        HandleStats();
    }

    public override void TakeDamage(int Amount, string BodyPart, bool Single) 
    {
        if (CurrMomentum > 0)
        {
            float DamageRatio = Mathf.Clamp01((float)Amount / CurrHealth);
            CurrMomentum *= (1f - DamageRatio);
        }

        base.TakeDamage(Amount, BodyPart, Single);

        StartCoroutine(FlashDamageScreen());
    }

    public override void Death()
    {
        Debug.Log(CurrHealth);

        #if UNITY_EDITOR // If in Unity Editor...
        UnityEditor.EditorApplication.isPlaying = false; // Quit Debug
        #else // If NOT in Unity Editor...      Quit Game.
                        Application.Quit(); 
        #endif
    }

    void HandleKillStreak()
    {
        if (KillStreak != GameManager.Instance.KillCount)
        {
            KillStreak = GameManager.Instance.KillCount;
            KillStreakDecayTimer = 0;
        }
        else
        {
            if (KillStreakDecayTimer < KillStreakDecayRate)
            {
                KillStreakDecayTimer += Time.deltaTime;
            }
            else
            {
                KillStreak = 0;
                GameManager.Instance.KillCount = 0;
                CurrMomentum = MaxMomentum * 0.15f;
            }
        }
    }

    void HandleParkourCountCombo()
    {
        if (ParkourCount != ParkourCombo)
        {
            ParkourCount = ParkourCombo;
            ParkourComboDecayTimer = 0;
        }
        else
        {
            if (ParkourComboDecayTimer < ParkourComboDecayRate)
            {
                ParkourComboDecayTimer += Time.deltaTime;
            }
            else
            {
                ParkourCombo = 0;
                ParkourCount = 0;
                CurrMomentum = MaxMomentum * 0.15f;
            }
        }
    }

    void HandleTimeMove()
    {
        if (IsMoving)
        {
            TimeMove = Mathf.Min(TimeMove + Time.deltaTime, 60f);
            TimeMoveDecayTimer = 0;
        }
        else
        {
            if (TimeMoveDecayTimer < TimeMoveDecayRate)
            {
                TimeMoveDecayTimer += Time.deltaTime;
            }
            else
            {
                TimeMove = 0;
                CurrMomentum = MaxMomentum * 0.15f;
            }
        }
    }

    void HandleStats()
    {
        float MomentumPercent = CurrMomentum / MaxMomentum;
        float CurvedPercent = Mathf.Pow(MomentumPercent, 0.3f); // Curves the response AGGRESSIVELY

        // Damage Reduction
        DamageReduc = DamageReducBase + (CurvedPercent * Random.Range(1, 3));
        // Speed
        Speed = SpeedBase * (1f + (CurvedPercent * Random.Range(1, 3)));
        // Jump Count
        JumpMax = Mathf.Clamp((int)((CurvedPercent * Random.Range(1, 3)) * TrueJumpMax) + 1, 1, TrueJumpMax);
        // Jump Speed
        JumpSpeed = JumpSpeedBase * (1f + (CurvedPercent * Random.Range(1, 3)));
    }
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
}
