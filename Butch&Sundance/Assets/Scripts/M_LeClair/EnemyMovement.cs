using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    public void Move(Vector3 direction, int speed, bool CanFly)
    {
        if (!CanFly)
        {
            direction.y = 0f;
        }
        transform.position += direction.normalized * Time.deltaTime * speed;
    }
}
