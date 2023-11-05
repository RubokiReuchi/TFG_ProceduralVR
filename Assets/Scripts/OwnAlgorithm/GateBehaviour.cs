using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateBehaviour : MonoBehaviour
{
    [SerializeField] GATE_STATE type;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Gate other;
    [HideInInspector] public bool opened;
    [HideInInspector] public GameObject gateInMap;
    [HideInInspector] public Bounds roomBounds;
    Transform player;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        opened = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenTriggered(GameObject collisionGO)
    {
        if (opened || !roomBounds.Contains(player.position)) return;
        switch (type)
        {
            case GATE_STATE.YELLOW:
                if (collisionGO.CompareTag("YellowProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.BLUE:
                if (collisionGO.CompareTag("BlueProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.RED:
                if (collisionGO.CompareTag("RedProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.PURPLE:
                if (collisionGO.CompareTag("PurpleProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.GREEN:
                if (collisionGO.CompareTag("GreenProjectile"))
                {
                    OpenGate();
                    this.other.self.GetComponent<GateBehaviour>().OpenGate();
                }
                break;
            case GATE_STATE.BOSS:
                OpenGate();
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
        //gateInMap.GetComponent<GateInMap>().HideGate();
    }

    public IEnumerator CloseGateDelay()
    {
        yield return new WaitForSeconds(3);
        animator.SetTrigger("Close");
        opened = false;
    }

    public void Dye()
    {
        Destroy(this.gameObject);
    }
}
