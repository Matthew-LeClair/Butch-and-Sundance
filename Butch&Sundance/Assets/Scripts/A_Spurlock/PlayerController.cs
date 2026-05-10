using UnityEngine;

public class PlayerController : CharacterBase
{
    [SerializeField] CharacterController Controller;

    [SerializeField] LayerMask IgnoreLayer;

    [SerializeField] int Speed;
    float Momentum;

    [SerializeField] float JumpSpeed;
    [SerializeField] int JumpMax;
    [SerializeField] float Gravity;
    int JumpCount;

    Vector3 MoveDir;
    Vector3 PlayerVel;

    bool IsAiming;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleMomentum();

        Movement(); // Moves once per frame for better smoothing
    }

    void Movement() 
    {
        if (Input.GetButton("Fire1")) // If Left Click is Pressed...
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

    }

    public override void Death()
    {

    }
}
