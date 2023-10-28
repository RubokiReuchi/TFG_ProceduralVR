using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFollowHand : MonoBehaviour
{
    [SerializeField] Transform anchor;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;
        transform.SetParent(anchor);
    }
}
