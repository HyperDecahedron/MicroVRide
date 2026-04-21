using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniquesEnabler : MonoBehaviour
{
    [SerializeField] private GameObject RestingFrame;
    [SerializeField] private GameObject PhysicalCues;

    void Start()
    {
        string technique = SessionState.CSTechnique;

        if (string.IsNullOrEmpty(technique))
        {
            Debug.LogWarning("CSTechnique is null or empty. Using default behavior.");
            RestingFrame.SetActive(true);
            PhysicalCues.SetActive(true);
            return;
        }

        if (technique == "af")
        {
            RestingFrame.SetActive(false);
            PhysicalCues.SetActive(false);
        }
        else if (technique == "rf")
        {
            RestingFrame.SetActive(true);
            PhysicalCues.SetActive(false);
        }
        else if (technique == "pc")
        {
            RestingFrame.SetActive(false);
            PhysicalCues.SetActive(true);
        }
        else
        {
            RestingFrame.SetActive(true);
            PhysicalCues.SetActive(true);
        }
    }

}
