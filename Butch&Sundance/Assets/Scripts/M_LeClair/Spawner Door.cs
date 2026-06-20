using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Rendering;

public class SpawnerDoor : MonoBehaviour
{
    [SerializeField] GameObject[] objectToSpawn;
    [SerializeField] int amountToSpawn;
    [SerializeField] int spawnRate;
    public LayerMask masks;
    [SerializeField] ParticleSystem spawnFX;
    public Transform player;

    int spawnCount;
    float spawnTimer;

    bool onCooldown = false;
    bool canSpawn;

    public void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        Vector3 rayTarget = player.position + Vector3.up * 1f;
        Vector3 rayDir = (rayTarget - rayOrigin).normalized;
        float dist = Vector3.Distance(rayOrigin, rayTarget);
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, rayDir, out hit, dist, masks))
        {
            canSpawn = hit.transform.root.CompareTag("Player");
        }
        if (!onCooldown)
        {
            spawnTimer += Time.deltaTime;

            if (spawnCount < amountToSpawn && spawnTimer >= spawnRate)
            {
                spawn();
            }

            if (spawnCount >= amountToSpawn && !onCooldown)
            {
                onCooldown = true;
                StartCoroutine(spawnCooldown());
            }
        }
    }

    void spawn()
    {
        spawnTimer = 0;
        spawnCount++;

        StartCoroutine(SpawnWithEffect(transform.position + transform.forward * 1.5f));
    }

    IEnumerator SpawnWithEffect(Vector3 spawnPos)
    {
        if (spawnFX != null)
        {
            ParticleSystem fx = Instantiate(spawnFX, spawnPos, Quaternion.identity);
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);

            yield return new WaitForSeconds(1f);

            GameObject enemy = Instantiate(objectToSpawn[Random.Range(0, objectToSpawn.Length)], spawnPos + Vector3.up * 0.5f, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }
    IEnumerator spawnCooldown()
    {
        yield return new WaitForSeconds(30);
        spawnCount = 0;
        spawnTimer = 0;
        onCooldown = false;
    }
}