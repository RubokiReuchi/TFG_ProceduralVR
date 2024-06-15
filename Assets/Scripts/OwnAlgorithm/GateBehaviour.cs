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
    Bounds hallwayBounds;
    Transform player;
    bool close;
    AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        opened = false;
        close = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (other != null) hallwayBounds = new Bounds(new Vector3(transform.position.x - (transform.position.x - other.position.x) / 2.0f, 2.5f, transform.position.z - (transform.position.z - other.position.z) / 2.0f), new Vector3(3.001f, 6, 3.001f));

        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (other == null) return;
        if (opened && close && !hallwayBounds.Contains(player.position))
        {
            close = false;
            animator.SetTrigger("Close");
            source.Play();
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
                if (collisionGO.CompareTag("YellowProjectile") || collisionGO.CompareTag("GreenProjectile"))
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
        source.Play();
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
