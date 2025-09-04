using UnityEngine;
using UnityEngine.SceneManagement;
using Esper.Freeloader;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }
        public void LoadGameScene()
    {
        LoadingScreen.Instance.Load(1);
        
        // LoadingScreen.Instance.Load(1);
    }
}
