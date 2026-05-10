using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour, I_Damage
{
    [SerializeField] Renderer Mat;
    [SerializeField] CharacterController Controller;

    [SerializeField] LayerMask IgnoreLayer;

    [SerializeField] int HP;

    [SerializeField] int Speed;

    [SerializeField] GameObject Bullet;
    [SerializeField] float ShootRate;
    [SerializeField] Transform GunPivot;
    [SerializeField] Transform ShootPos;
    [SerializeField] int GunRotateSpeed;
    [SerializeField] int FaceTargetSpeed;

    [SerializeField] Color OriginalColor;
    [SerializeField] Color FlashColor;

    float ShootTimer;
    float AngleToPlayer;

    Vector3 PlayerDir;


    int OriginalHP;

    Vector3 MoveDir;

    bool PlayerInRange;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Mat.material.color = OriginalColor; // Set the Material Color as the Original Color, Modular Version
        GameManager.Instance.UpdateGameGoal(1); // Init Kill Count
    }

    // Update is called once per frame
    void Update() 
    {
        PlayerDir = GameManager.Instance.Player.transform.position - transform.position;
        
        if (ShootTimer < ShootRate) 
        { ShootTimer += Time.deltaTime; }
        
        if (PlayerInRange) 
        {
            RotateGun();

            FaceTarget();

            if (ShootTimer > ShootRate)
            { Shoot(); }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        { PlayerInRange = true; }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        { PlayerInRange = false; }
    }

    public void TakeDamage(int amount) // Take Damage, Damage Interface Override
    {
        HP -= amount; // Subtract Health by Amount
        GameManager.Instance.UpdateGameGoal(-1); // Update Kill Count
        if (HP <= 0) // If Health is Less Than or Equal To 0...
        { Destroy(gameObject); } // Destroy the Object
        else { StartCoroutine(Flash()); } // Call the Flash Function, Modular Version
    }

    IEnumerator Flash() // Flash Function, Modular Version
    {
        Mat.material.color = FlashColor; // Set Mat Color is the Flash Color, Modular Version
        yield return new WaitForSeconds(0.1f); // Wait for .1 Seconds
        Mat.material.color = OriginalColor; // Reset the Color
    }

    void RotateGun() 
    {
        Quaternion Rot = Quaternion.LookRotation(PlayerDir);

        GunPivot.rotation = 
            Quaternion.Lerp(
                GunPivot.rotation,
                Rot, 
                Time.deltaTime * GunRotateSpeed);
    }

    void FaceTarget()
    {
        Quaternion Rot = Quaternion.LookRotation(new Vector3(PlayerDir.x, 0 , PlayerDir.z));

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                Rot,
                Time.deltaTime * FaceTargetSpeed);
    }

    void Shoot()
    {
        ShootTimer = 0; // Reset Shoot Timer

        // Spawn Bullet at the Shoot Pos at the Gun Pivot Rotation
        Instantiate(Bullet, ShootPos.position, GunPivot.rotation);
    }
}