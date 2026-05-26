using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class Laser : MonoBehaviour, I_Damage
{
    [SerializeField] LineRenderer laserLine;
    [SerializeField] GameObject hitEffect;
    [SerializeField] Lightning lightning;
    [SerializeField] Transform laserStart;
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
        if (Physics.Raycast(laserStart.position, laserStart.forward, out hit, laserMaxDist))
        {
            laserLine.SetPosition(0, laserStart.position);
            laserLine.SetPosition(1, hit.point);
            if(lightning != null)
            {
                lightning.GenerateLightning(laserStart.position, hit.point);
            }
            hitEffect.SetActive(true);
            hitEffect.transform.position = hit.point;

            I_Damage dmg = hit.collider.GetComponent<I_Damage>();
            if (dmg != null && !isDamaging)
            {
                StartCoroutine(damageTime(dmg));
            }
        }
        else
        {
            laserLine.SetPosition(0, laserStart.position);
            laserLine.SetPosition(1, laserStart.position + laserStart.forward * laserMaxDist);
            hitEffect.SetActive(false);

            if(lightning != null)
            {
                lightning.GetComponent<LineRenderer>().positionCount = 0;
            }
        }
    }

    IEnumerator damageTime(I_Damage d)
    {
        isDamaging = true;
        d.TakeDamage(laserDamage, true);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

    public void TakeDamage(int Amount, bool AlienTech)
    {
        Destroy(gameObject);
    }
}
