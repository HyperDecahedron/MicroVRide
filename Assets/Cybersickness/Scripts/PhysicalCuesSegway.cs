using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VK.BikeLab.Segway;

public class PhysicalCuesSegway : MonoBehaviour
{
    [Header("Arrow Setup")]
    [SerializeField] private GameObject velocityArrow;
    [SerializeField] private Transform cubesParent;
    [SerializeField] private Transform firstCube;
    [SerializeField] private GameObject prefabArrowCube;
    [SerializeField] private GameObject arrowHeadBack;

    private float cubeLength = 0.15f;
    private float gap = 0.05f;
    private int maxCubes = 15;

    private float maxSpeed = 10f;
    private float maxArrowRotation = 20f;   // degrees

    private Segway segway;
    private SegwayController segwayController;
    private FanController fanController;

    private List<Transform> cubes = new List<Transform>();
    private float step;

    // rotation
    private float currentArrowYaw;

    void Start()
    {
        segway = transform.parent.GetComponent<Segway>();
        segwayController = transform.parent.GetComponent<SegwayController>();
        fanController = transform.parent.GetComponent<FanController>();

        // Velocity arrow
        step = cubeLength + gap;
        cubes.Add(firstCube);
    }

    void Update()
    {
        // Velocity arrow -------------------------------------------------------------
        float speed = segway.getVelosity();
        float abs_speed = Mathf.Abs(speed);
        float t = Mathf.Clamp01(abs_speed / maxSpeed);
        int targetCount = Mathf.Clamp(Mathf.CeilToInt(t * maxCubes), 1, maxCubes);
        AddCubes(targetCount);

        Color color = GetSpeedColor(t);
        ApplyColor(color);

        // arrange if the velocity is negative
        if(speed < 0)
        {
            // hide arrow in the head and show arrow in the back
            firstCube.GetChild(0).gameObject.SetActive(false);
            arrowHeadBack.SetActive(true);
        }
        else
        {
            firstCube.GetChild(0).gameObject.SetActive(true);
            arrowHeadBack.SetActive(false);
        }

        // Rotate velocityArrow according to vehicle rotation ---------------------------
        float targetAngle = 0f;

        if (fanController.turning == 1)
        {
            targetAngle = maxArrowRotation;   // right
        }
        else if (fanController.turning == -1)
        {
            targetAngle = -maxArrowRotation;  // left
        }
        else
        {
            targetAngle = 0f; // center
        }

        velocityArrow.transform.localRotation = Quaternion.Euler(0f, 0f, -targetAngle);
    }

    void AddCubes(int targetCount)
    {
        if (cubes.Count != targetCount)
        {
            // add
            while (cubes.Count < targetCount)
            {
                GameObject newCube = Instantiate(prefabArrowCube, cubesParent);
                cubes.Add(newCube.transform);
            }

            // remove
            while (cubes.Count > targetCount && cubes.Count > 1)
            {
                Transform last = cubes[cubes.Count - 1];
                cubes.RemoveAt(cubes.Count - 1);
                Destroy(last.gameObject);
            }

            // Position cubes
            Vector3 forward = cubesParent.forward;
            int j = cubes.Count - 1;
            for (int i = 0; i < cubes.Count; i++)
            {
                Vector3 localPos = Vector3.forward * step * i;

                cubes[j].localPosition = localPos;
                cubes[j].localRotation = Quaternion.identity;
                j--;
            }
        }
    }

    void ApplyColor(Color color)
    {
        // change colour of cubes
        foreach (var cube in cubes)
        {
            Renderer r = cube.GetComponent<Renderer>();
            if (r != null)
                r.material.color = color;
        }

        // change colour of arrow head
        Transform arrowHead = firstCube.GetChild(0).GetChild(0);
        Renderer headRenderer = arrowHead.GetComponent<Renderer>();
        if (headRenderer != null)
            headRenderer.material.color = color;

        // change colour of the arrow head pointing backwards
        Renderer headBackRenderer = arrowHeadBack.transform.GetChild(0).GetComponent<Renderer>();
        if (headBackRenderer != null)
            headBackRenderer.material.color = color;
    }

    Color GetSpeedColor(float t)
    {
        if (t < 0.2f)
            return Color.green;
        else if (t < 0.4f)
            return Color.yellow;
        else if (t < 0.6f)
            return new Color(1f, 0.5f, 0f); // orange
        else if (t < 0.8f)
            return Color.red;
        else
            return new Color(0.56f, 0f, 1f); // violet
    }

}
