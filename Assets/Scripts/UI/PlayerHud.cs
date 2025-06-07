using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerHud : SingletonMonoBehaviour<PlayerHud>
{
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI AmmoText;

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
        if (!PlayerModel.Instance)
        {
            return;
        }

        UpdateHealth();
        UpdateScore();
        UpdateAmmoText();
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

    async void FlickerDamageScreen()
    {
        damageScreen.Show();
        await Task.Delay((int)(damageScreenFlickerWaitTime * 1000));
        damageScreen.Hide();
    }
}
