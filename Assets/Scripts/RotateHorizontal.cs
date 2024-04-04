using UnityEngine;

public class RotateHorizontal : MonoBehaviour
{
    [SerializeField] float speed;

    void Update()
    {
        transform.Rotate(Vector3.up, speed);
    }
}
