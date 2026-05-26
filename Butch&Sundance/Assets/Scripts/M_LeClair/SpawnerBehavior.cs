using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Spawner : EnemyBehavior
{
    [SerializeField] GameObject[] objectToSpawn;
    [SerializeField] int amountToSpawn;
    [SerializeField] int spawnRate;
    [SerializeField] int spawnDist;
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;

    int spawnCount;
    float spawnTimer;
    float roamTimer;

    bool startSpawning;

    Vector3 startingPos;

    public override void Tick()
    {
        checkRoam();
        if (startSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnCount < amountToSpawn && spawnTimer >= spawnRate)
            {
                spawn();
            }
            if(spawnCount >= amountToSpawn)
            {
                spawnCooldown();
            }
        }
    }

    void checkRoam()
    {
        if (ai.agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;

            if (roamTimer >= roamPauseTime)
            {
                roam();
            }
        }
    }

    void roam()
    {
        roamTimer = 0;
        Vector3 randPos = Random.insideUnitSphere * roamDistance;
        randPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(randPos, out hit, roamDistance, 1);
        ai.agent.SetDestination(hit.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            startSpawning = true;
        }
    }

    void spawn()
    {
        spawnTimer = 0;
        spawnCount++;

        Vector3 ranPos = Random.insideUnitSphere * spawnDist;
        ranPos += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, spawnDist, 1);

        Instantiate(objectToSpawn[Random.Range(0, objectToSpawn.Length)], hit.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
    }

    IEnumerator spawnCooldown()
    {
        yield return new WaitForSeconds(30);
    }
}