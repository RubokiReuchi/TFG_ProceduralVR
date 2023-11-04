using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerCharged : Projectile
{
    public GUN_TYPE selectedGunType;
    SphereCollider col;
    bool launch;
    Vector3 initialScale;
    [SerializeField] GameObject decal;

    public void SetUp()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        launch = false;
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (!launch) return;
        rb.velocity = transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Foundations"))
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit hit);
            GameObject decalGo = GameObject.Instantiate(decal, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal) * Quaternion.AngleAxis(90, Vector3.right));
            decalGo.transform.localScale = transform.localScale * 0.2f;
        }
        Destroy(this.gameObject);
    }

    public void Increase(Vector3 newScl, Vector3 newPos)
    {
        transform.position = newPos;
        transform.localScale = initialScale + newScl;
    }

    public void Launch(Quaternion rotation)
    {
        transform.rotation = rotation;
        col.enabled = true;
        launch = true;
    }
}
