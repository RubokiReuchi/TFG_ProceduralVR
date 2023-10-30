using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateBehaviour : MonoBehaviour
{
    [SerializeField] GATE_STATE type;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Gate other;
    bool opened;
    [HideInInspector] public GameObject gateInMap;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        opened = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (opened) return;
        if (!other.CompareTag("YellowProjectile") && !other.CompareTag("BlueProjectile") && !other.CompareTag("RedProjectile") && !other.CompareTag("PurpleProjectile") && !other.CompareTag("GreenProjectile")) return;
        switch (type)
        {
            case GATE_STATE.YELLOW:
                if (other.CompareTag("YellowProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.BLUE:
                if (other.CompareTag("BlueProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.RED:
                if (other.CompareTag("RedProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.PURPLE:
                if (other.CompareTag("PurpleProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.GREEN:
                if (other.CompareTag("GreenProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.BOSS:
                OpenGate();
                this.other.self.GetComponent<GateBehaviour>().OpenGate();
                break;
            case GATE_STATE.DESTROYED:
            case GATE_STATE.NULL:
            default:
                break;
        }
    }

    public void OpenGate()
    {
        animator.SetTrigger("Open");
        opened = true;
        gateInMap.SetActive(false);
    }

    public void Dye()
    {
        Destroy(this.gameObject);
    }
}
