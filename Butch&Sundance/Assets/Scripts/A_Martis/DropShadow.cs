using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropShadow : MonoBehaviour
{
    public GameObject shadow;
    public RaycastHit hit;
    public float offset;

    private void FixedUpdate()
    {
        Vector3 origin = transform.position + Vector3.up * offset;
        Ray downRay = new Ray(origin, Vector3.down);

        if(Physics.Raycast(downRay, out hit))
        {
            shadow.transform.position = hit.point;
            print(hit.transform);
        }
    }
}
