using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SphereRobotRay : Projectile
{
    [SerializeField] float damage;
    Transform psTransform;
    BoxCollider bc;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        psTransform = transform.GetChild(0).GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0.5f)
        {
            bc.enabled = false;
            psTransform.localScale = Vector3.Lerp(psTransform.localScale, Vector3.zero, Time.deltaTime * 10.0f);
        }
        if (lifeTime < 0) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead") || collision.gameObject.CompareTag("NormalHand"))
        {
            collision.transform.root.GetComponent<PlayerState>().TakeDamage(damage);
        }
        Destroy(this.gameObject);
    }
}
