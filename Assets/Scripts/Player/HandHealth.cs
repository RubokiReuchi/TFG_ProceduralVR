using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HandHealth : MonoBehaviour
{
    [SerializeField] List<MeshRenderer> healthBlocks; // 50 hp per block
    [SerializeField] Material hasCellMat, noCellMat;
    int cellsAmount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void SetCells(float maxHealth)
    {
        cellsAmount = 0;
        hasCellMat.SetFloat("_FillPercentage", 50.0f);

        for (int i = 0; i < 10; i++)
        {
            if (i * 50 < maxHealth)
            {
                healthBlocks[i].GetComponent<MeshRenderer>().material = new Material(hasCellMat);
                cellsAmount++;
            }
            else healthBlocks[i].GetComponent<MeshRenderer>().material = noCellMat;
        }
    }

    public void UpdateHealthDisplay(float currentHealth)
    {
        float auxHealth = currentHealth;
        for (int i = 0; i < cellsAmount; i++)
        {
            float value;
            if (auxHealth > 50)
            {
                value = 50;
                healthBlocks[i].GetComponent<MeshRenderer>().material.SetFloat("_FillPercentage", value);
            }
            else
            {
                value = auxHealth;
                healthBlocks[i].GetComponent<MeshRenderer>().material.SetFloat("_FillPercentage", value);
            }
            auxHealth -= value;
        }
    }
}
