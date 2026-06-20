using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtonFunctions : MonoBehaviour
{
  public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Load Next Scene
    }
    public void Options()
    {

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
