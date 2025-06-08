using UnityEngine;

public class FishFood : MonoBehaviour
{
    public float health = 10f;
    public float timeout = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, timeout);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        SoundEffectsManager.Instance.Play("TF_Healthpickup", 0.7f);
        var player = other.gameObject.GetComponentInParent<PlayerModel>();
        if (player)
        {
            player.health.UpdateHealth(health);
            Destroy(gameObject);
        }
    }
}
