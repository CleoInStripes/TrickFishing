using UnityEngine;

public class BouncePad : MonoBehaviour
{

    public float jumpForce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        //make sure youre only bouncing player
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            //Get player rigidbody
            Rigidbody rb = other.transform.parent.GetComponent<Rigidbody>();

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
