using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    protected EnemyAI ai;

    protected virtual void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    public abstract void Tick();
}
