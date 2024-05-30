using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusHomingProjectile : Projectile
{
    Transform playerHead;
    [SerializeField] float damage;
    [SerializeField] float homingStrenght;
    [SerializeField] float slowPercentage;
    [SerializeField] float slowDuration;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerHead = GameObject.FindGameObjectWithTag("PlayerHead").transform;
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * speed;
        Vector3 direction = playerHead.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        float homingSpeed = (lifeTime > 4) ? 1000.0f : homingStrenght;
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, homingSpeed * Time.deltaTime));
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead") || collision.gameObject.CompareTag("NormalHand"))
        {
            if (damage > 0) collision.transform.root.GetComponent<PlayerState>().TakeDamage(damage);
            if (slowDuration > 0) collision.transform.root.GetComponentInChildren<PlayerMovement>().TakeSlow(slowPercentage, slowDuration);
        }
        Destroy(gameObject);
    }
}
