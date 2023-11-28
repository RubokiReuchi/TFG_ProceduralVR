using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerChecker : MonoBehaviour
{
    [SerializeField] Text displayedText;

    [Header("Puzzle")]
    [SerializeField] bool isPartOfPuzzle;
    [SerializeField] float damageObjective;
    [SerializeField] ParticleSystem completePs;
    [SerializeField] EnergyBarrier barrier;

    [Header("Freeze")]
    [SerializeField] protected GameObject[] caseGameObjects;
    [SerializeField] protected GameObject supportGO;
    [SerializeField] protected GameObject pilarGO;
    [SerializeField] protected Material caseOriginalMaterial;
    [SerializeField] protected Material supportOriginalMaterial;
    [SerializeField] protected Material pilarOriginalMaterial;
    Material caseMaterial;
    Material supportMaterial;
    Material pilarMaterial;
    protected float freezePercentage = 0;
    protected float freezeDuration = 10; // secs
    protected float freezedTime; // secs
    protected float recoverDelay = 2; // secs
    protected float recoverTime; // secs
    protected float invulneravilityTime = 0; // secs

    void Start()
    {
        caseMaterial = new Material(caseOriginalMaterial);
        foreach (var caseGO in caseGameObjects) caseGO.GetComponent<Renderer>().material = caseMaterial;
        supportMaterial = new Material(supportOriginalMaterial);
        supportGO.GetComponent<Renderer>().material = supportMaterial;
        pilarMaterial = new Material(pilarOriginalMaterial);
        pilarGO.GetComponent<Renderer>().material = pilarMaterial;

        if (isPartOfPuzzle)
        {
            displayedText.text = "";
            enabled = false;
        }
    }

    private void Update()
    {
        if (freezePercentage == 100)
        {
            invulneravilityTime -= Time.deltaTime;
            freezedTime -= Time.deltaTime;
            if (freezedTime < 0)
            {
                freezedTime = 0;
                freezePercentage = 0;
                caseMaterial.SetFloat("_FreezeInterpolation", 0);
                supportMaterial.SetFloat("_FreezeInterpolation", 0);
                pilarMaterial.SetFloat("_FreezeInterpolation", 0);
            }
        }
        else if (freezePercentage > 0)
        {
            if (recoverTime > 0)
            {
                recoverTime -= Time.deltaTime;
                if (recoverTime < 0) recoverTime = 0;
            }
            else if (recoverTime == 0)
            {
                freezePercentage -= Time.deltaTime * 20.0f;
                if (freezePercentage < 0) freezePercentage = 0;
                caseMaterial.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
                supportMaterial.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
                pilarMaterial.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!enabled || invulneravilityTime > 0) return;

        float finalDamage;
        if (freezePercentage == 100)
        {
            finalDamage = amount * 5.0f;
            freezePercentage = 0;
            caseMaterial.SetFloat("_FreezeInterpolation", 0);
            supportMaterial.SetFloat("_FreezeInterpolation", 0);
            pilarMaterial.SetFloat("_FreezeInterpolation", 0);
        }
        else finalDamage = amount;

        displayedText.text = finalDamage.ToString();

        if (isPartOfPuzzle && finalDamage >= damageObjective)
        {
            barrier.PuzzleCompleted();
            completePs.Play();
            enabled = false;
        }
    }

    public void TakeFreeze(float amount)
    {
        freezePercentage += amount;
        if (freezePercentage >= 100) // freeze
        {
            freezePercentage = 100;
            freezedTime = freezeDuration;
            invulneravilityTime = 1.0f;
        }
        caseMaterial.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
        supportMaterial.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
        pilarMaterial.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
        recoverTime = recoverDelay;
    }

    public void StartPuzzle()
    {
        displayedText.text = "0";
        barrier.PuzzleStarted();
    }
}
