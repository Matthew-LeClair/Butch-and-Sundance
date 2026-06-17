using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SimpleKeyInputJump : MonoBehaviour
{
    public Animator characterAnimator;

    private void Start()
    {
        characterAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            characterAnimator.SetTrigger("JumpTrigger");
        }
    }
}
