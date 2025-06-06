using UnityEngine;

public class PlayerModel : SingletonMonoBehaviour<PlayerModel>
{
    public Health health;
    public gun gun;
    [HideInInspector] public Rigidbody rb;

    [ReadOnly]
    public int score = 0;

    public new void Awake()
    {
        base.Awake();
        health = GetComponent<Health>();
        gun = GetComponent<gun>();
        rb = GetComponent<Rigidbody>();
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
