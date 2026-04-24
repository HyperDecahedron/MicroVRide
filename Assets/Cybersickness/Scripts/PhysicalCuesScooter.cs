using System.Collections.Generic;
using UnityEngine;
using VK.BikeLab.Segway;

public class PhysicalCuesScooter : MonoBehaviour
{
    [Header("Arrow Setup")]
    [SerializeField] private GameObject velocityArrow;
    [SerializeField] private Transform cubesParent;
    [SerializeField] private Transform firstCube; 
    [SerializeField] private GameObject prefabArrowCube;

    private float cubeLength = 0.15f;
    private float gap = 0.05f;
    private int maxCubes = 15;

    private float maxSpeed = 10f;
    private float maxArrowRotation = 20f;   // degrees

    private Segway escooter;
    private EscooterController eScooterController;
    private FanController fanController;

    private List<Transform> cubes = new List<Transform>();
    private float step;

    // rotation
    private float currentArrowYaw;

    void Start()
    {
        escooter = transform.parent.GetComponent<Segway>();
        eScooterController = transform.parent.GetComponent<EscooterController>();
        fanController = transform.parent.GetComponent<FanController>();

        // Velocity arrow
        step = cubeLength + gap;
        cubes.Add(firstCube);
    }

    void Update()
    {
        // Velocity arrow -------------------------------------------------------------
        float speed = Mathf.Max(0, escooter.getVelosity());
        float t = Mathf.Clamp01(speed / maxSpeed);
        int targetCount = Mathf.Clamp(Mathf.CeilToInt(t * maxCubes), 1, maxCubes);
        AddCubes(targetCount);
        
        Color color = GetSpeedColor(t);
        ApplyColor(color);
        SetOutline(eScooterController.throttleActive);

        // Rotate velocityArrow according to vehicle rotation ---------------------------
        float targetYaw = 0f;

        if (fanController.turning == 1)
        {
            targetYaw = maxArrowRotation;   // right
        }
        else if (fanController.turning == -1)
        {
            targetYaw = -maxArrowRotation;  // left
        }
        else
        {
            targetYaw = 0f; // center
        }

        velocityArrow.transform.localRotation = Quaternion.Euler(0f, targetYaw, 0f);
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
            Vector3 forward = velocityArrow.transform.forward;
            for (int i = 0; i < cubes.Count; i++)
            {
                Vector3 pos = transform.position + forward * step * (cubes.Count - 1 - i);

                cubes[i].position = pos;
                cubes[i].forward = forward;
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

    void SetOutline(bool enabled)
    {
        foreach (var cube in cubes)
        {
            var outline = cube.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = enabled;
        }

        Transform arrowHead = firstCube.GetChild(0);
        var headOutline = arrowHead.GetComponent<Outline>();
        if (headOutline != null)
            headOutline.enabled = enabled;

        if (enabled)
            ApplyColor(new Color(0.5f, 0.7f, 1f)); // blue
    }
}