using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHealth : MonoBehaviour
{
    [SerializeField] float maxHealth; // start 250, max upgraded 500
    [SerializeField] List<MeshRenderer> healthBlocks; // 50 hp per block
    [SerializeField] Material hasCellMat, noCellMat;
    int cellsAmount;
    
    // Start is called before the first frame update
    void Start()
    {
        SetCells();
        UpdateHealthDisplay(maxHealth);
    }

    private void Update()
    {
        
    }

    void SetCells()
    {
        cellsAmount = 0;

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
                value = 1;
                healthBlocks[i].GetComponent<MeshRenderer>().material.SetFloat("_FillPercentage", value);
            }
            else
            {
                value = auxHealth / 50.0f;
                healthBlocks[i].GetComponent<MeshRenderer>().material.SetFloat("_FillPercentage", value);
            }
            auxHealth -= value * 50.0f;
        }
    }

    public void RecalculateCells()
    {

    }
}
