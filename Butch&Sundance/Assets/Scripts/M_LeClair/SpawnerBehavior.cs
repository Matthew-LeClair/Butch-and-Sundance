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
    bool onCooldown = false;

    Vector3 startingPos;

    private void Start()
    {
        startingPos = transform.position;
    }

    public override void Tick()
    {
        checkRoam();
        if (startSpawning && !onCooldown)
        {
            spawnTimer += Time.deltaTime;

            if (spawnCount < amountToSpawn && spawnTimer >= spawnRate)
            {
                spawn();
            }

            if(spawnCount >= amountToSpawn)
            {
                StartCoroutine(spawnCooldown());
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
        onCooldown = true;
        yield return new WaitForSeconds(10);
        spawnCount = 0;
        spawnTimer = 0;
        onCooldown = false;
    }
}