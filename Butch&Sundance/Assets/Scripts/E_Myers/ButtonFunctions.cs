using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void Resume() 
    { GameManager.Instance.StateUnpause(); } // Unpause

    public void Restart() 
    {
        GameManager.Instance.StateUnpause(); // Unpause
        SceneManager.LoadScene(1);
    }
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Load Next Scene
        GameManager.Instance.StateUnpause();
    }
    public void Respawn()
    {         
        GameManager.Instance.PlayerScript.ChangeRespawnPos(); // Respawn Player
        GameManager.Instance.StateUnpause(); // Unpause
    }

    public void Quit()
    {
        #if UNITY_EDITOR // If in Unity Editor...
                UnityEditor.EditorApplication.isPlaying = false; // Quit Debug
        #else // If NOT in Unity Editor...      Quit Game.
                Application.Quit(); 
        #endif
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
        GameManager.Instance.StateUnpause();
    }
}
