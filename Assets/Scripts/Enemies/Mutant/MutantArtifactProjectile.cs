using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantArtifactProjectile : Projectile
{
    Transform playerHead;
    [SerializeField] float damage;
    [SerializeField] float homingStrenght;
    [SerializeField] GameObject trailPrefab;

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
            collision.transform.root.GetComponent<PlayerState>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
