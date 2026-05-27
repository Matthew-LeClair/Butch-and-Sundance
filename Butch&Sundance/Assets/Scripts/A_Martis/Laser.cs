using UnityEngine;
using System.Collections;
using Unity.Mathematics;

// Creates a continuous laser beam that damages objects implementing I_Damage.
// Supports optional lightning visuals and hit effects.
public class Laser : MonoBehaviour, I_Damage
{
    [SerializeField] LineRenderer laserLine; // Main laser beam renderer
    [SerializeField] GameObject hitEffect; // Visual effect displayed at hit location
    [SerializeField] Lightning lightning; // Lightning effect
    [SerializeField] Transform laserStart; // Starting point of the laser

    [SerializeField] int laserMaxDist;
    [SerializeField] int laserDamage;
    [SerializeField] float damageRate;

    bool isDamaging;
    void Update()
    {
        createLaser();
    }

    void createLaser()
    {
        RaycastHit hit;

        // Fire a raycast forward from the laser start poin
        if (Physics.Raycast(laserStart.position, laserStart.forward, out hit, laserMaxDist))
        {
            // Set laser start and hit positions
            laserLine.SetPosition(0, laserStart.position);
            laserLine.SetPosition(1, hit.point);

            // Generate optional lightning effect
            if (lightning != null)
            {
                lightning.GenerateLightning(laserStart.position, hit.point);
            }

            // Enable and position hit effect
            hitEffect.SetActive(true);
            hitEffect.transform.position = hit.point;

            // Check if the hit object can take damage
            I_Damage dmg = hit.collider.GetComponent<I_Damage>();

            // Apply damage over time
            if (dmg != null && !isDamaging)
            {
                StartCoroutine(damageTime(dmg));
            }
        }
        else
        {
            // If nothing is hit, extend laser to max distance
            laserLine.SetPosition(0, laserStart.position);
            laserLine.SetPosition(1, laserStart.position + laserStart.forward * laserMaxDist);

            // Disable hit effect since nothing was hit
            hitEffect.SetActive(false);

            // Clear lightning effect if enabled
            if (lightning != null)
            {
                lightning.GetComponent<LineRenderer>().positionCount = 0;
            }
        }
    }

    // Apply damage at timed intervals
    IEnumerator damageTime(I_Damage d)
    {
        isDamaging = true;
        d.TakeDamage(laserDamage, true);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

    // Destroy laser object on recieving damage
    public void TakeDamage(int Amount, bool AlienTech)
    {
        Destroy(gameObject);
    }
}
