using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tutorial : MonoBehaviour
{
    public TMP_Text AimText;
    public TMP_Text ShotText;
    public TMP_Text ReloadedText;
    public TMP_Text MovedText;
    public TMP_Text JumpedText;

    public GameObject Wall;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.Player.GetComponent<PlayerController>().Aimed && GameManager.Instance.Player.GetComponent<PlayerController>().Shot && GameManager.Instance.Player.GetComponent<PlayerController>().Reloaded && GameManager.Instance.Player.GetComponent<PlayerController>().Moved && GameManager.Instance.Player.GetComponent<PlayerController>().Jumped) 
        { Destroy(Wall);  Destroy(gameObject); }

        if (GameManager.Instance.Player.GetComponent<PlayerController>().Aimed) { AimText.text = "Done."; }
        if (GameManager.Instance.Player.GetComponent<PlayerController>().Shot) { ShotText.text = "Done."; }
        if (GameManager.Instance.Player.GetComponent<PlayerController>().Reloaded) { ReloadedText.text = "Done."; }
        if (GameManager.Instance.Player.GetComponent<PlayerController>().Moved) { MovedText.text = "Done."; }
        if (GameManager.Instance.Player.GetComponent<PlayerController>().Jumped) { JumpedText.text = "Done."; }
    }
}
