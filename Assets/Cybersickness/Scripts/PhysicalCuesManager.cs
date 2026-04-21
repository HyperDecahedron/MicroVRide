using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VK.BikeLab.Segway;

public class PhysicalCuesManager : MonoBehaviour
{
    private Segway escooter;
    private EscooterController eScooterController;

    void Start()
    {
        segway = this.transform.parent.gameObject.GetComponent<Segway>();
        eScooterController = this.transform.parent.gameObject.GetComponent<EscooterController>();

        // the throttle can be checked if it is active by checking eScooterController.throttleActive. if active, make the arrow with blue outline
        // if inactive, remove outline

        // the velocity of the vehicle can be checked with escooter.getVelosity()
    }

    void Update()
    {
        Debug.Log(escooter.getVelosity());
    }
}
