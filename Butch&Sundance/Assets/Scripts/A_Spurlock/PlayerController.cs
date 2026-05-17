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
    public PlayerGun pGun;

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
    [SerializeField] float MomentumDecayRate = 0.4f;
    [SerializeField] float DirectionBlendSpeed = 8f;

    public float MomentumBuildRate;
    float CurrMomentum = 0;
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
    void Update()
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
            PlayerVel.y = 0;
            JumpCount = 0; // Reset Jump Count on Landing
        }

        Vector3 InputDir =
            Input.GetAxis("Horizontal") * transform.right
            + Input.GetAxis("Vertical") * transform.forward;

        if (InputDir.magnitude > 0.1f) { Moved = true; }

        HandleMomentum(InputDir); // Pass Input Direction to Momentum System

        // Blend Momentum Velocity toward Input Direction, preserving speed through turns
        if (InputDir.magnitude > 0.1f) // If there is Input...
        {
            MomentumVelocity = Vector3.Lerp(
                MomentumVelocity,
                InputDir.normalized * (Speed + CurrMomentum * 0.1f), // Target is Input Direction at Current Speed
                DirectionBlendSpeed * Time.deltaTime); // Blend at Direction Blend Speed
        }
        else // If there is No Input...
        {
            MomentumVelocity = Vector3.Lerp(
                MomentumVelocity,
                Vector3.zero, // Bleed off toward Zero
                MomentumDecayRate * Time.deltaTime);
        }

        Controller.Move(MomentumVelocity * Time.deltaTime); // Move Player using Momentum Velocity

        Jump();

        Controller.Move(PlayerVel * Time.deltaTime);
        PlayerVel.y -= Gravity * Time.deltaTime;
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && JumpCount < 2) // Allow Double Jump
        {
            Jumped = true;
            JumpCount++;

            float MomentumBoost = CurrMomentum / MaxMomentum; // Momentum as a Percentage
            PlayerVel.y = JumpSpeedBase * (1f + MomentumBoost * 0.4f); // Scale Jump Height with Momentum

            MomentumVelocity = new Vector3(
                MomentumVelocity.x, 0, MomentumVelocity.z) // Strip Y from Momentum Velocity
                * (1f + MomentumBoost * 0.3f); // Boost Horizontal on Jump
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
}
