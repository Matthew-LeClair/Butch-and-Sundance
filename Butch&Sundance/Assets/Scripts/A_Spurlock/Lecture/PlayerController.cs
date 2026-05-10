using UnityEngine;

public class PlayerController : MonoBehaviour, I_Damage
{
    [SerializeField] CharacterController Controller;

    [SerializeField] LayerMask IgnoreLayer;

    [SerializeField] int HP;

    [SerializeField] int Speed;
    [SerializeField] float SprintMod;

    [SerializeField] float JumpSpeed;
    [SerializeField] int JumpMax;

    [SerializeField] float Gravity;
    [SerializeField] int ShootDamage;
    [SerializeField] int ShootDistance;
    [SerializeField] float ShootRate;

    float ShootTimer;


    bool NeedsReset; // Bool for Sprint Handling

    int JumpCount;
    int OriginalHP;

    Vector3 MoveDir;
    Vector3 PlayerVel;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement(); // Moves once per frame for better smoothing

        Sprint(); // Call Sprint Function after Movement to prevent Bugs
    }

    void Movement() 
    {
        // Draw a Ray for Debugging Shooting
        Debug.DrawRay(Camera.main.transform.position, // Draw Ray at Camera Position...
            Camera.main.transform.forward * ShootDistance, // Draw it as long as Shoot Distance going in the Direction of Camera Forward
            Color.red); // Make the Ray red

        ShootTimer += Time.deltaTime; // Increment the Shoot Timer every Frame

        if (Input.GetButton("Fire1") // If Left Click is Pressed...
            && ShootTimer > ShootRate) // AND ShootTimer is Greater than ShootRate
        { Shoot(); } // Call Shoot Function

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

    void Sprint() 
    {
        if (Controller.isGrounded // If Character is on the ground...
            && Input.GetButtonDown("Sprint")) // AND Sprint Button Down
        {
            Speed = (int)(Speed * SprintMod); // Speed is Multiplied by SprintMod Var
            NeedsReset = true; // Set Need Reset True
        }
        else
        {
            if (NeedsReset // If NeedsReset is True...
                && Input.GetButtonUp("Sprint")) // AND Sprint Button is Released...
            { 
                Speed = (int)(Speed / SprintMod); // Speed is reset to original by Dividing it by SprintMod 
                NeedsReset = false; // Set Need Reset False
            }
        }
    }
    
    void Shoot() 
    {
        ShootTimer = 0; // Reset Shoot Timer

        RaycastHit hit; // Init the Raycast Hit

        // If the Ray Hits then it will apply Damage after Debug Printing
        if (Physics.Raycast(Camera.main.transform.position, // Ray Position is Camera Position 
            Camera.main.transform.forward, // Ray Direction is Camera Forward
            out hit, // Hit is now what the Raycast Hit
            ShootDistance, // Ray is long as Shoot Distance
            ~IgnoreLayer)) // It will now Ignore the Player in Collision
        {
            Debug.Log(hit.collider.name); // Debug Print the name of what the Ray Collides with

            I_Damage dmg = hit.collider.GetComponent<I_Damage>(); // Init and Set the Interface as the Hit GameObjects Version

            if (dmg != null) // If the GameObjects Damage Interface is NOT Null...
            { dmg.TakeDamage(ShootDamage); } // Hit GameObject takes Damage
        }
    }

    public void TakeDamage(int amount) // Take Damage, Damage Interface Override
    { 
        HP -= amount; // Subtract Health by Amount

        if (HP <= 0) // If Player is Dead
        { GameManager.Instance.YouLose(); } // Lose Screen
    }
}
