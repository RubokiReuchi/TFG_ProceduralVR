using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public bool firstEnable = true;
    protected NavMeshAgent agent;
    Rigidbody rb;
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] protected LayerMask foundationsLayers;

    // crawl control
    [HideInInspector] public bool horizontalPushed = false;
    [HideInInspector] public bool verticalPushed = false;
    [HideInInspector] public bool bouncyPushed = false;
    bool giantPushed = false;
    [HideInInspector] public bool onCC = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!bouncyPushed && giantPushed && collision.gameObject.CompareTag("FoundationsW"))
        {
            bouncyPushed = true;
            StartCoroutine(AirbourneBounceCo(collision.contacts[0].point));
        }
    }

    public virtual void StartCheckOptions()
    {

    }

    public virtual void TakeDamage(float amount)
    {

    }

    public virtual void Die()
    {

    }

    public void Airbourne(float force, bool giant)
    {
        StartCoroutine(AirbourneCo(force, giant));
    }

    IEnumerator AirbourneCo(float force, bool giant)
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (giant) onCC = true;
        agent.enabled = false;
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        yield return new WaitForSeconds(0.1f); // wait 0.1 secs
        bool onGround = false;
        while (!onGround)
        {
            onGround = Physics.Raycast(transform.position + Vector3.up * 0.001f, Vector3.down, 0.005f, foundationsLayers);
            yield return null;
        }
        if (giant) onCC = false;
        agent.enabled = true;
    }

    public void Pushed(Vector3 force, bool giant)
    {
        StartCoroutine(PushedCo(force, giant));
    }

    IEnumerator PushedCo(Vector3 force, bool giant)
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (giant)
        {
            if (!agent) agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;
        }
        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.VelocityChange);
        if (giant)
        {
            giantPushed = true;
            yield return new WaitForSeconds(0.25f);
            if (!bouncyPushed) agent.enabled = true;
            giantPushed = false;
        }
        else yield return null;
    }

    IEnumerator AirbourneBounceCo(Vector3 impactPoint)
    {
        onCC = true;
        float freeze = 0.1f;
        while (freeze > 0)
        {
            freeze -= Time.deltaTime;
            rb.velocity = Vector3.zero;
            yield return null;
        }
        rb.velocity = Vector3.zero;
        Vector3 direction = (transform.position - impactPoint).normalized;
        rb.AddForce(new Vector3(direction.x, 0.7f, direction.z) * 10, ForceMode.VelocityChange);
        yield return null; // wait 1 frame
        bool onGround = false;
        while (!onGround)
        {
            onGround = Physics.Raycast(transform.position + Vector3.up * 0.001f, Vector3.down, 0.005f, foundationsLayers);
            yield return null;
        }
        onCC = false;
        agent.enabled = true;
    }
}
