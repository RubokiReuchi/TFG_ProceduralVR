using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGateBehaviour : MonoBehaviour
{
    [SerializeField] GATE_STATE type;
    [HideInInspector] public Animator animator;
    public TutorialRoomBehaviour roomBehaviour;
    public TutorialGateBehaviour otherGateBehaviour;
    [HideInInspector] public bool opened;
    [HideInInspector] public GameObject gateInMap;
    [HideInInspector] public Bounds roomBounds;
    Bounds hallwayBounds;
    Transform player;
    bool close;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        opened = false;
        close = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        hallwayBounds = new Bounds(new Vector3(transform.position.x - (transform.position.x - otherGateBehaviour.transform.position.x) / 2.0f, 2.5f, transform.position.z - (transform.position.z - otherGateBehaviour.transform.position.z) / 2.0f), new Vector3(3.001f, 6, 3.001f));
        roomBounds = roomBehaviour.GetComponent<BoxCollider>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        if (opened && close && !hallwayBounds.Contains(player.position))
        {
            close = false;
            animator.SetTrigger("Close");
        }
        else if (!opened && hallwayBounds.Contains(player.position))
        {
            OpenGate();
        }
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
                    otherGateBehaviour.OpenGate();
                }
                break;
            case GATE_STATE.BLUE:
                if (collisionGO.CompareTag("BlueProjectile"))
                {
                    OpenGate();
                    otherGateBehaviour.OpenGate();
                }
                break;
            case GATE_STATE.RED:
                if (collisionGO.CompareTag("RedProjectile"))
                {
                    OpenGate();
                    otherGateBehaviour.OpenGate();
                }
                break;
            case GATE_STATE.PURPLE:
                if (collisionGO.CompareTag("PurpleProjectile"))
                {
                    OpenGate();
                    otherGateBehaviour.OpenGate();
                }
                break;
            case GATE_STATE.GREEN:
                if (collisionGO.CompareTag("GreenProjectile"))
                {
                    OpenGate();
                    otherGateBehaviour.OpenGate();
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
        close = true;
    }

    public void Closed()
    {
        opened = false;
    }
}
