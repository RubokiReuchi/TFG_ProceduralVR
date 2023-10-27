using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public enum GUN_TYPE
{
    YELLOW,
    BLUE,
    RED,
    PURPLE,
    GREEN
}

public class PlayerProjectile : MonoBehaviour
{
    GUN_TYPE selectedGunType = GUN_TYPE.YELLOW;
    Rigidbody rb;
    [SerializeField] float speed;
    float lifeTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void Update()
    {
        rb.velocity = transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }
}
