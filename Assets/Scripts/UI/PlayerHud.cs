using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerHud : SingletonMonoBehaviour<PlayerHud>
{
    public TextMeshProUGUI HealthHud;
    public TextMeshProUGUI ScoreHud;

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
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealth();
        UpdateScore();
    }

    void UpdateHealth()
    {
        HealthHud.text = $"{PlayerModel.Instance.health.currentHealth}";
    }

    void UpdateScore()
    {
        ScoreHud.text = $"{PlayerModel.Instance.score}";
    }

    async void FlickerDamageScreen()
    {
        damageScreen.Show();
        await Task.Delay((int)(damageScreenFlickerWaitTime * 1000));
        damageScreen.Hide();
    }
}
