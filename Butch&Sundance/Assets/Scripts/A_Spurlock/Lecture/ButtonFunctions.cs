using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void Resume() 
    { GameManager.Instance.StatePause(false); } // Unpause

    public void Restart() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload Active Scene
        GameManager.Instance.StatePause(false); // Unpause
    }

    public void Quit()
    {
        #if UNITY_EDITOR // If in Unity Editor...
                UnityEditor.EditorApplication.isPlaying = false; // Quit Debug
        #else // If NOT in Unity Editor...      Quit Game.
                Application.Quit(); 
        #endif
    }
}
