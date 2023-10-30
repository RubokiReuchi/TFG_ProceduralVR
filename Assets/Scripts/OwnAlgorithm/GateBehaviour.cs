using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateBehaviour : MonoBehaviour
{
    [SerializeField] GATE_STATE type;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Gate other;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("YellowProjectile") && !other.CompareTag("BlueProjectile") && !other.CompareTag("RedProjectile") && !other.CompareTag("PurpleProjectile") && !other.CompareTag("GreenProjectile")) return;
        switch (type)
        {
            case GATE_STATE.YELLOW:
                if (other.CompareTag("YellowProjectile"))
                {
                    animator.SetTrigger("Open");
                    this.other.self.GetComponent<GateBehaviour>().animator.SetTrigger("Open");
                }
                break;
            case GATE_STATE.BLUE:
                if (other.CompareTag("BlueProjectile"))
                {
                    animator.SetTrigger("Open");
                    this.other.self.GetComponent<GateBehaviour>().animator.SetTrigger("Open");
                }
                break;
            case GATE_STATE.RED:
                if (other.CompareTag("RedProjectile"))
                {
                    animator.SetTrigger("Open");
                    this.other.self.GetComponent<GateBehaviour>().animator.SetTrigger("Open");
                }
                break;
            case GATE_STATE.PURPLE:
                if (other.CompareTag("PurpleProjectile"))
                {
                    animator.SetTrigger("Open");
                    this.other.self.GetComponent<GateBehaviour>().animator.SetTrigger("Open");
                }
                break;
            case GATE_STATE.GREEN:
                if (other.CompareTag("GreenProjectile"))
                {
                    animator.SetTrigger("Open");
                    this.other.self.GetComponent<GateBehaviour>().animator.SetTrigger("Open");
                }
                break;
            case GATE_STATE.BOSS:
                animator.SetTrigger("Open");
                this.other.self.GetComponent<GateBehaviour>().animator.SetTrigger("Open");
                break;
            case GATE_STATE.DESTROYED:
            case GATE_STATE.NULL:
            default:
                break;
        }
    }

    public void Dye()
    {
        Destroy(this.gameObject);
    }
}
