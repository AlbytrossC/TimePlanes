using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Setup
    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }
    #endregion
    
    #region Game State Management

    public enum GameState
    {
        MainMenu,
        StageSelection,
        Playing,
        Paused,
        GameOver
    }
    public GameState CurrentGameState { get; private set; }

    #endregion

    #region Scene Management

    public void GoToScene(string sceneName)
    {
        // Implement scene transition logic here
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    #endregion
}