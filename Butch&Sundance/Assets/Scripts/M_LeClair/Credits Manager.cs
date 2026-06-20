using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditsManager : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] RectTransform creditsContent;
    [SerializeField] float scrollSpeed = 50f;
    [SerializeField] float endPause = 2f;

    [Header("Navigation")]
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string returnScene;

    bool isScrolling = true;

    void Start()
    {
        Vector2 startPos = creditsContent.anchoredPosition;
        startPos.y = -Screen.height * 0.7f;
        creditsContent.anchoredPosition = startPos;

        StartCoroutine(ScrollCredits());
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            StopAllCoroutines();
            GoBack();
        }
    }

    IEnumerator ScrollCredits()
    {
        isScrolling = true;

        float contentHeight = creditsContent.rect.height;
        float targetY = contentHeight + Screen.height;

        while (creditsContent.anchoredPosition.y < targetY)
        {
            Vector2 pos = creditsContent.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;
            creditsContent.anchoredPosition = pos;
            yield return null;
        }

        isScrolling = false;
        yield return new WaitForSeconds(endPause);
        GoBack();
    }

    void GoBack()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(returnScene))
            SceneManager.LoadScene(returnScene);
        else
            SceneManager.LoadScene(mainMenuScene);
    }
}
