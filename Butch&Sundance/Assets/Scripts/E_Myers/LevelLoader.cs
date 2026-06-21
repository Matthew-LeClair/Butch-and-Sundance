using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelLoader : MonoBehaviour
{
    public Animator Transition;
    public float TransitionTime = 1f;
    bool isLoading = false;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GoalCompleted)
        {
            LoadNextLevel();
        }
    }
    public void LoadNextLevel()
    {
        if (isLoading)
        {
            return;
        }
        isLoading = true;
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        Transition.SetTrigger("Start");

        yield return new WaitForSeconds(TransitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}
