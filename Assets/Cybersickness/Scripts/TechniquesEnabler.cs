using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TechniquesEnabler : MonoBehaviour
{
    [SerializeField] private GameObject Helmet;
    [SerializeField] private TMP_Text debug_text2;
    public bool debug2 = false;

    private FanController fanController;


    void Start()
    {
        string technique = SessionState.CSTechnique;

        if (string.IsNullOrEmpty(technique))
        {
            Debug.LogWarning("CSTechnique is null or empty. Using default behavior.");
            if (debug2 && debug_text2 != null)
                debug_text2.text = "CSTechnique is null or empty";
            return;
        }

        if (debug2 && debug_text2 != null)
            debug_text2.text = technique;

        // default behaviour, no technique enabled
        fanController = this.gameObject.GetComponent<FanController>();
        if ( fanController == null)
        {
            if (debug2 && debug_text2 != null)
                debug_text2.text = "Null Fan";
        }
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
