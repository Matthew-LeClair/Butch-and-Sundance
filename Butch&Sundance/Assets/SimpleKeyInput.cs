using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SimpleKeyInput : MonoBehaviour
{
    public Animator characterAnimator;

    private void Start()
    {
        characterAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        bool wKey = Input.GetKey(KeyCode.W);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            characterAnimator.SetTrigger("JumpTrigger");
        }

        bool isMoving = wKey || Input.GetKey(KeyCode.A)
                      || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        characterAnimator.SetBool("IsMoving", isMoving);
    }
}
