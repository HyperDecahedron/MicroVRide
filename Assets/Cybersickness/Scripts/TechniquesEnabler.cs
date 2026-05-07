using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniquesEnabler : MonoBehaviour
{
    //[SerializeField] private GameObject RestingFrame;
    [SerializeField] private GameObject Helmet;
    [SerializeField] private GameObject PhysicalCues;

    void Start()
    {
        string technique = SessionState.CSTechnique;

        if (string.IsNullOrEmpty(technique))
        {
            Debug.LogWarning("CSTechnique is null or empty. Using default behavior.");
            return;
        }

        //RestingFrame.SetActive(false);
        Helmet.SetActive(false);
        PhysicalCues.SetActive(false);

        if (technique == "af")
        {
            // only air flow
        }
        /*else if (technique == "rf")
        {
            RestingFrame.SetActive(true);
        }*/
        else if (technique == "hel")
        {
            Helmet.SetActive(true);
        }
        else if (technique == "pc")
        {
            PhysicalCues.SetActive(true);
        }
    }

}
