using TMPro;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] public string Text;
    [SerializeField] public GameObject TutorialTxt;
    [SerializeField] public TMP_Text UI_Text;


    private void Start()
    {
        TutorialTxt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        TutorialTxt.gameObject.SetActive(true);
        UI_Text.text = Text;
    }

    private void OnTriggerExit(Collider other)
    {
        TutorialTxt.gameObject.SetActive(false);
    }
}
