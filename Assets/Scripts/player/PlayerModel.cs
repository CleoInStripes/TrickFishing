using UnityEngine;

public class PlayerModel : SingletonMonoBehaviour<PlayerModel>
{
    public Health health;

    [ReadOnly]
    public int score = 0;

    public new void Awake()
    {
        base.Awake();
        health = GetComponent<Health>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int _score)
    {
        score += _score;
    }
}
