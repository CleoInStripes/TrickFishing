using UnityEngine;

public class FishProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public float damage = 5f;

    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime); // auto-destroy after time
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerModel = other.gameObject.GetComponentInParent<PlayerModel>();
        if (playerModel)
        {
            playerModel.health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }


        var fishModel = other.gameObject.GetComponentInParent<FishAIModel>();
        if (!fishModel && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
