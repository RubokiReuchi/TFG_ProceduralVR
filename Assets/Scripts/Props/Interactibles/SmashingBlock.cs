using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SmashingBlock : Puzzle
{
    [SerializeField] Transform spawnTransform;
    [SerializeField] Animator animator;
    [SerializeField] Material fadeMat;
    [SerializeField] bool startActivated;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (startActivated) animator.SetTrigger("Active");
    }

    public override void StartPuzzle()
    {
        animator.SetTrigger("Active");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnCoroutine(other.gameObject));
        }
    }

    IEnumerator RespawnCoroutine(GameObject player)
    {
        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 5;
            if (opacity > 1) opacity = 1;
            fadeMat.SetFloat("_Opacity", opacity);
            yield return null;
        }

        player.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

        yield return new WaitForSeconds(0.3f);
        opacity = 1.0f;
        while (opacity > 0)
        {
            opacity -= Time.deltaTime * 2;
            if (opacity < 0) opacity = 0;
            fadeMat.SetFloat("_Opacity", opacity);
            yield return null;
        }
    }
}
