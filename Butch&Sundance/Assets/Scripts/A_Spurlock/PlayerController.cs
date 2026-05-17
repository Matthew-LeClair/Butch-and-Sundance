using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] CharacterController Controller;

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
    PlayerGun pGun;

    [Header("Gear")]


    [Header("Movement")]
    [SerializeField] public float SpeedBase;
    public float Speed;

    bool IsMoving;
    public float JumpSpeedBase;
    float JumpSpeed;
    int JumpCount;


    [Header("Momemtum")]
    [SerializeField] public float BaseMomentumBuildRate;
    public float MomentumBuildRate;
    float CurrMomentum = 0;
    float MaxMomentum = 50;

    [Header("Parkour")]
    float TimeMove;
    [SerializeField] float TimeMoveDecayRate;
    float TimeMoveDecayTimer;

    [Header("Tutorial")]
    public bool Aimed;
    public bool Shot;
    public bool Reloaded;
    public bool Moved;
    public bool Jumped;

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
        { pGun.Reload(); Reloaded = true; }

        if (Input.GetButtonDown("Fire2")) { pGun.Aim(); Aimed = true; }
        if (Input.GetButtonUp("Fire2")) { pGun.Aim(); }

        if (Input.GetButton("Fire1") && pGun.IsAiming) // If Left Click is Pressed while Aiming...
        { pGun.Shoot(); Shot = true; } // Call Shoot Function

        if (Controller.isGrounded)  // Checks if the Player Character is on the ground
        { PlayerVel.y = 0; } // Resets the Player Velocity Y to help processes

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
            && JumpCount < 1) // AND Jump Count is NOT more than 1
        {
            Jumped = true;
            JumpCount++; // Increment Jump Count
            PlayerVel.y = JumpSpeed; // Set Player Velocity Y to Jump Speed
        } 
    }

    void HandleMomentum()
    {
        HandleTimeMove();

        float MomentumGain =
            ((TimeMove / 60f)
            * Time.deltaTime) * 
            MomentumBuildRate;

        CurrMomentum = Mathf.Clamp(CurrMomentum + MomentumGain, 0, MaxMomentum);
        HandleStats();
    }

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

        // Speed
        Speed = SpeedBase * (1f + (CurvedPercent * Random.Range(1, 3)));
        
        // Jump Speed
        JumpSpeed = (float)((JumpSpeedBase * (1f + (CurvedPercent * Random.Range(1, 3)))) + (Speed * .6));
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

    IEnumerator FlashShieldScreen()
    {
        GameManager.Instance.PlayerDamage_Screen.SetActive(true); // Activate the damage screen effect to indicate that the player has taken damage

        yield return new WaitForSeconds(0.5f); // Wait for a short duration (0.5 seconds) before deactivating the damage screen effect

        GameManager.Instance.PlayerDamage_Screen.SetActive(false); // Deactivate the damage screen effect after the wait time has elapsed
    }
}
