﻿using System;
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

    [Header("Level References")]
    public Transform initialFishSpawnSpotsHolder;
    public Transform playerRespawnPoint;

    [Header("Misc")]
    public LevelSettings settings;

    [ReadOnly] public bool isGameOver = false;

    [HideInInspector]
    public SimpleTimer timer = new SimpleTimer();

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        HelperUtilities.UpdateCursorLock(true);

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

    public void TriggerGameOver()
    {
        if (isGameOver) {
            return;
        }

        isGameOver = true;

        PlayerHud.Instance.ShowGameOverScreen();
    }

    public void PauseGame()
    {
        PlayerHud.Instance.ShowPauseScreen();
    }
}