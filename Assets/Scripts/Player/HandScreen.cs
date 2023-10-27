using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GUN_TYPE // move it later to swap guns, or something like that
{
    YELLOW,
    BLUE,
    RED,
    PURPLE,
    GREEN
}

public class HandScreen : MonoBehaviour
{
    [SerializeField] List<MeshRenderer> screenBlocks;
    [SerializeField] Material yellowMat, blueMat, redMat, purpleMat, greenMat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScreen(GUN_TYPE type)
    {
        for (int i = 0; i < 10; i++)
        {
            switch (type)
            {
                case GUN_TYPE.YELLOW:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = yellowMat;
                    break;
                case GUN_TYPE.BLUE:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = blueMat;
                    break;
                case GUN_TYPE.RED:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = redMat;
                    break;
                case GUN_TYPE.PURPLE:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = purpleMat;
                    break;
                case GUN_TYPE.GREEN:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = greenMat;
                    break;
                default:
                    break;
            }
        }
    }
}
