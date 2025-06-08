using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1;
        HelperUtilities.UpdateCursorLock(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        GameManager.Instance.GoToMainLevel();
    }

    public void Exit()
    {
        GameManager.Instance.QuitGame();
    }
}
