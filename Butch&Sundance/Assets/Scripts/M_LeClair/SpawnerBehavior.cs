using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Spawner : EnemyBehavior
{
    [SerializeField] GameObject[] objectToSpawn;
    [SerializeField] int amountToSpawn;
    [SerializeField] int spawnRate;
    [SerializeField] int spawnDist;
    [SerializeField] ParticleSystem spawnFX;

    int spawnCount;
    float spawnTimer;

    bool onCooldown = false;

    public override void Tick()
    {
        if (!ai.seePlayer)
        {
            ai.CheckMovement();
        }
        else
        {
            ai.agent.ResetPath();
        }

        if (ai.seePlayer)
        {
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
    }

    void spawn()
    {
        spawnTimer = 0;
        spawnCount++;

        Vector3 ranPos = Random.insideUnitSphere * spawnDist;
        ranPos += transform.position;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(ranPos, out hit, spawnDist, 1)) { return; }

        StartCoroutine(SpawnWithEffect(hit.position + Vector3.up * 0.5f));
    }

    IEnumerator SpawnWithEffect(Vector3 spawnPos)
    {
        if(spawnFX != null)
        {
            ParticleSystem fx = Instantiate(spawnFX, spawnPos, Quaternion.identity);
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);

            yield return new WaitForSeconds(1f);

            GameObject enemy = Instantiate(objectToSpawn[Random.Range(0, objectToSpawn.Length)], spawnPos + Vector3.up * 0.5f, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
    }
    IEnumerator spawnCooldown()
    {
        if (ai.seePlayer)
        {
            yield return new WaitForSeconds(10);
        }
        else
        {
            yield return new WaitForSeconds(30);
        }
        spawnCount = 0;
        spawnTimer = 0;
        onCooldown = false;
    }
}