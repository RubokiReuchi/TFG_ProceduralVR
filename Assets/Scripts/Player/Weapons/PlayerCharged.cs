using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCharged : Projectile
{
    [SerializeField] float minDamage;
    [SerializeField] float maxDamage;
    float damage;
    SphereCollider col;
    bool launch;
    Vector3 initialScale;
    [SerializeField] GameObject decal;
    [SerializeField] GameObject hitMark;

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
        if (collision.gameObject.CompareTag("FoundationsF") || collision.gameObject.CompareTag("FoundationsW"))
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit hit);
            GameObject decalGo = GameObject.Instantiate(decal, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal) * Quaternion.AngleAxis(90, Vector3.right));
            decalGo.transform.localScale = transform.localScale * 0.2f;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            GameObject hit = GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            collision.gameObject.GetComponent<Enemy>().TakeDamage(damage);
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

    public void SetDamage(float current, float min, float max)
    {
        float normal = Mathf.InverseLerp(min, max, current);
        damage = Mathf.Lerp(minDamage, maxDamage, normal);
    }
}
