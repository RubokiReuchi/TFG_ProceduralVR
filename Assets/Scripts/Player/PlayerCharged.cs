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
