using UnityEngine;
using UnityEngine.Events;

public class TreasureChest : MonoBehaviour
{
    public UnityEvent OnLanded;
    public float terminalVelocity = 1f;

    private Rigidbody rb;
    private bool startedFalling = false;
    private bool landed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!startedFalling)
        {
            if (rb.linearVelocity.magnitude > terminalVelocity)
            {
                startedFalling = true;
            }
        }

        if (startedFalling && !landed && rb.linearVelocity.magnitude <= terminalVelocity)
        {
            Landed();
        }
    }

    void Landed()
    {
        if (!landed)
        {
            landed = true;
            OnLanded.Invoke();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!landed)
        {
            var terrain = collision.gameObject.GetComponent<Terrain>();
            if (terrain)
            {
                Landed();
            }
        }
    }
}
