using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    public TextMeshProUGUI HealthHud;
    public TextMeshProUGUI ScoreHud;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealth();
        UpdateScore();
    }

    void UpdateHealth()
    {
        HealthHud.text = $"Health: {PlayerModel.Instance.health.currentHealth}";
    }

    void UpdateScore()
    {
        ScoreHud.text = $"Score: {PlayerModel.Instance.score}";
    }
}
