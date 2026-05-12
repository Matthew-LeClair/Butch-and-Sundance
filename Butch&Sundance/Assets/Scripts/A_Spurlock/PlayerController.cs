using UnityEngine;

public class PlayerController : CharacterBase
{
    [SerializeField] CharacterController Controller;

    [SerializeField] public float SpeedBase;
    public float Speed;

    [SerializeField] public float BaseMomentumBuildRate;
    public float MomentumBuildRate;
    float CurrMomentum = 0;
    float MaxMomentum = 300;

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

    float JumpSpeed;
    int JumpMax;
    int JumpCount;
    float Gravity = 35;

    Vector3 MoveDir;
    Vector3 PlayerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        MomentumBuildRate = BaseMomentumBuildRate;
    }

    // Update is called once per frame
    void Update()
    {
        IsMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f
            || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
        if (GameManager.Instance != null) { HandleMomentum(); }

        Movement(); // Moves once per frame for better smoothing
    }

    void Movement() 
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        { Weapon.Reload(); }

        if (Input.GetButtonDown("Fire2")) { Aim(); }
        if (Input.GetButtonUp("Fire2")) { Aim(); }

        if (Input.GetButton("Fire1") && IsAiming) // If Left Click is Pressed while Aiming...
        { Weapon.Shoot(); } // Call Shoot Function

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
            ((TimeMove / 60f) + 
            (ParkourCombo / 100f) + 
            (KillStreak / 100f))
            * Time.deltaTime * MomentumBuildRate;

        CurrMomentum = Mathf.Clamp(CurrMomentum + MomentumGain, 0, MaxMomentum);

        HandleStats();
    }

    public override void TakeDamage(int Amount, string BodyPart) 
    {
        float DamageRatio = Mathf.Clamp01((float)Amount / CurrHealth);
        CurrMomentum *= (1f - DamageRatio);

        base.TakeDamage(Amount, BodyPart);
    }

    public override void Death()
    {

    }

    void HandleKillStreak()
    {
        if (KillStreak != GameManager.Instance.KillCount) { KillStreak = GameManager.Instance.KillCount; KillStreakDecayTimer = 0; }
        else
        { 
            if (KillStreakDecayTimer < KillStreakDecayRate) 
            { KillStreakDecayTimer += Time.deltaTime; }
            else
            { 
                KillStreak = 0; 
                GameManager.Instance.KillCount = 0; 
                CurrMomentum = MaxMomentum * .03f; 
            }
        }
    }

    void HandleParkourCountCombo() 
    {
        if (ParkourCount != ParkourCombo) { ParkourCount = ParkourCombo; ParkourComboDecayTimer = 0; }
        else
        {
            if (ParkourComboDecayTimer < ParkourComboDecayRate)
            { ParkourComboDecayTimer += Time.deltaTime; }
            else 
            { 
                ParkourCombo = 0; 
                ParkourCount = 0; 
                CurrMomentum = MaxMomentum * .03f; }
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
            { TimeMoveDecayTimer += Time.deltaTime; }
            else { TimeMove = 0; CurrMomentum = MaxMomentum * .03f; }
        }
    }

    void HandleStats()
    {
        int ClampedMomentum = Mathf.Clamp((int)(CurrMomentum), 1, (int)(MaxMomentum));

        float MomentumPercent = (ClampedMomentum / MaxMomentum);

        // Damage Reduction
        DamageReduc = DamageReducBase + MomentumPercent;

        // Speed
        Speed = SpeedBase * (1f + MomentumPercent); // Up to 2x Speed Bonus when at Max Momentum

        // JumpCount
        JumpMax = (int)(CurrMomentum / 100);
        JumpMax = Mathf.Clamp(JumpMax, 1, (int)(MaxMomentum / 100));

        // Jump Speed
        JumpSpeed = Speed + .6f;
    }
}
