using System.Collections;
using UnityEngine;

public class TreasureChest : PickUp_Interact
{
    [Header("Contents")]
    [SerializeField] GameObject[] possibleWeapons;

    [Header("Animation")]
    [SerializeField] Transform lid;
    [SerializeField] float openAngle = 110f;
    [SerializeField] float openSpeed = 3f;

    [Header("Spawn")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] float spawnDelay = 0.5f;

    bool isOpen = false;

    public override void EventPickUp()
    {
        if (isOpen) return;
        isOpen = true;

        StartCoroutine(OpenChest());
    }

    IEnumerator OpenChest()
    {
        Quaternion startRot = lid.localRotation;
        Quaternion targetRot = Quaternion.Euler(-openAngle, 0f, 0f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            lid.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        lid.localRotation = targetRot;

        yield return new WaitForSeconds(spawnDelay);

        if (possibleWeapons != null && possibleWeapons.Length > 0)
        {
            int index = Random.Range(0, possibleWeapons.Length);
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
            Instantiate(possibleWeapons[index], pos, Quaternion.identity);
        }
    }
}