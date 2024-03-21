using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Rigidbody rb;
    [SerializeField] protected float speed;
    [SerializeField] protected float lifeTime;
}
