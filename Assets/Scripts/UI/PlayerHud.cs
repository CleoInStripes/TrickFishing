using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerHud : SingletonMonoBehaviour<PlayerHud>
{
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI AmmoText;
    public GameObject WaveInfo;
    public TextMeshProUGUI WaveText;
    public GameObject FishesLeftBox;
    public TextMeshProUGUI FishesLeftText;
    public MenuPage GameOverPage;
    public TextMeshProUGUI GameOverScoreText;

    [Header("Damage Screen")]
    public MenuPage damageScreen;
    public float damageScreenFlickerWaitTime = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerModel.Instance.health.OnDamageTaken.AddListener(() =>
        {
            FlickerDamageScreen();
        });
        PlayerModel.Instance.health.OnHealthDepleted.AddListener(() =>
        {
            ShowGameOverScreen();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerModel.Instance)
        {
            return;
        }

        UpdateHealth();
        UpdateScore();
        UpdateAmmoText();
        UpdateWaveInfo();
    }

    void UpdateHealth()
    {
        HealthText.text = $"{PlayerModel.Instance.health.currentHealth}";
    }

    void UpdateScore()
    {
        ScoreText.text = $"{PlayerModel.Instance.score}";
    }

    void UpdateAmmoText()
    {
        AmmoText.text = $"{PlayerModel.Instance.gun.CurrentAmmo}/{PlayerModel.Instance.gun.maxAmmo}";
    }

    void UpdateWaveInfo()
    {
        CapturePointSystem capturePointSystem = CapturePointSystem.Instance;

        WaveText.text = "Waiting for next wave...";
        FishesLeftText.text = "";
        FishesLeftBox.SetActive(false);

        if (capturePointSystem.isCountingDown)
        {
            WaveText.text = $"New wave incoming in {Mathf.Ceil(capturePointSystem.timeToStartNextWave)}";
        }
        else if (capturePointSystem.isWaveActive)
        {
            WaveText.text = $"Wave {capturePointSystem.currentWaveIndex + 1}";
            FishesLeftText.text = $"{capturePointSystem.capturePoint.activeFishCount}";
            FishesLeftBox.SetActive(true);
        }
    }

    async void FlickerDamageScreen()
    {
        damageScreen.Show();
        await Task.Delay((int)(damageScreenFlickerWaitTime * 1000));
        damageScreen.Hide();
    }

    public void Restart()
    {
        GameManager.Instance.RestartCurrentScene();
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void ShowGameOverScreen()
    {
        GameOverScoreText.text = $"Your Score: {PlayerModel.Instance.score}";
        GameOverPage.Show();
        HelperUtilities.UpdateCursorLock(false);
        Time.timeScale = 0f;
    }
}
