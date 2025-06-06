using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private const string gameSettingsFileName = "gameSettings.dat";

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string mainLevelScene = "Main";

    public InputDevice lastDetectedDevice = null;

    private Vector2 lastScreenSize;

    public event Action onScreenSizeChanged;

    public LevelManager.LevelSettings selectedLevelSettings { get; set; } = null;

    new void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        InputSystem.onEvent += (ptr, device) => { lastDetectedDevice = device; };

        QualitySettings.vSyncCount = 0; // VSync must be disabled.
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (lastScreenSize != screenSize)
        {
            lastScreenSize = screenSize;
            onScreenSizeChanged?.Invoke();
        }
    }

    public void QuitGame()
    {
        ScreenFader.Instance.FadeOut(-1, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        });
    }

    public void GoToMainMenu()
    {
        GoToScene(mainMenuScene);
    }

    public void GoToMainLevel()
    {
        GoToScene(mainLevelScene);
    }

    public void RestartCurrentScene()
    {
        GoToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToScene(string sceneName)
    {
        ScreenFader.Instance.FadeOut(-1, () =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    public void DeleteAllSaveData()
    {
        string dir = Application.persistentDataPath;
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }
}
