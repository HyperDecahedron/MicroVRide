using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniquesEnabler : MonoBehaviour
{
    [SerializeField] private GameObject Helmet;

    private FanController fanController;


    void Start()
    {

        string technique = SessionState.CSTechnique;

        if (string.IsNullOrEmpty(technique))
        {
            Debug.LogWarning("CSTechnique is null or empty. Using default behavior.");
            return;
        }

        // default behaviour, no technique enabled
        fanController = this.gameObject.GetComponent<FanController>();
        fanController.fan_enabled = false;
        Helmet.SetActive(false);

        if (technique == "af")
        {
            fanController.fan_enabled = true;
        }
        else if (technique == "hel")
        {
            Helmet.SetActive(true);
        }
        else if (technique == "af_hel")
        {
            Helmet.SetActive(true);
            fanController.fan_enabled = true;
        }
    }

}
