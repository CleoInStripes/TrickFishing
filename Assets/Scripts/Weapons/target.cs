
using UnityEngine;
using UnityEngine.Events;

public class target : MonoBehaviour
{
    public UnityEvent<float> OnHit;

    public void Hit(float damage)
    {
        OnHit.Invoke(damage);
    }    
}
