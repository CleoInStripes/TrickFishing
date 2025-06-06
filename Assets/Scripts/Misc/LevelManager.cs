using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    [Serializable]
    public class LevelSettings
    {
        public int maxTime = 60;
        public float difficultyLevel = 0.5f;
    }

    public LevelSettings settings;

    public List<Transform> playerSpawnPoints;

    Randomizer<Transform> playerSpawnRandomizer;

    public SimpleTimer timer = new SimpleTimer();

    [ReadOnly] public bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        HelperUtilities.UpdateCursorLock(true);

        playerSpawnRandomizer = new Randomizer<Transform>(playerSpawnPoints);

        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver)
        {
            if (!timer.expired)
            {
                timer.Update();
                if (timer.expired)
                {
                    // Out of time
                    TriggerGameOver();
                }
            }
        }
    }

    void Initialize()
    {
        if (GameManager.Instance.selectedLevelSettings != null)
        {
            settings = GameManager.Instance.selectedLevelSettings;
        }

        timer = new SimpleTimer(settings.maxTime);
    }

    void TriggerGameOver()
    {
        if (isGameOver) {
            return;
        }

        isGameOver = true;

        // TODO: Handle Game Over Logic
    }
}