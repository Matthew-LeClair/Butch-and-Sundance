using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelLoader : MonoBehaviour
{
    public Animator Transition;
    public float TransitionTime = 1f;

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.KillCount >= 0)
        {
            LoadNextLevel();
        }
    }
    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        Transition.SetTrigger("Start");

        yield return new WaitForSeconds(10);

        SceneManager.LoadScene(levelIndex);
    }
}
