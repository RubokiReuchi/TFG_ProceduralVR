using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] COIN type;
    [SerializeField] int amount;
    [SerializeField] Renderer handPrintRenderer;
    [SerializeField] BoxCollider detectorCollider;
    Bounds colliderBounds;
    CapsuleCollider leftHandCollider;
    float handInTime;
    Material material;

    // Start is called before the first frame update
    void Start()
    {
        colliderBounds = detectorCollider.GetComponent<BoxCollider>().bounds;
        material = new Material(handPrintRenderer.materials[1]);
        Material[] auxArray = { handPrintRenderer.materials[0], material };
        handPrintRenderer.materials = auxArray;
        material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f, 1));

        leftHandCollider = GameObject.Find("LeftHandCollision").GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (colliderBounds.Contains(leftHandCollider.bounds.center))
        {
            handInTime += Time.deltaTime;

            if (handInTime >= 1)
            {
                handInTime = 1;
                GetComponent<Animator>().enabled = true;
                if (type == COIN.BIOMATTER) PlayerSkills.instance.AddBiomatter(amount);
                else PlayerSkills.instance.AddGear(amount);
                enabled = false;
            }
            material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f + 0.46f * handInTime, 1));
        }
        else if (handInTime > 0)
        {
            handInTime -= Time.deltaTime;
            if (handInTime < 0) handInTime = 0;
            material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f + 0.23f * handInTime, 1));
        }
    }
}
