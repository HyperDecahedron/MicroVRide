using System.Collections.Generic;
using UnityEngine;
using VK.BikeLab.Segway;

public class PhysicalCuesManager : MonoBehaviour
{
    private Segway escooter;
    private EscooterController eScooterController;

    [Header("Arrow Setup")]
    [SerializeField] private Transform firstCube; 
    [SerializeField] private GameObject prefabArrowCube;

    [SerializeField] private float cubeLength = 0.15f;
    [SerializeField] private float gap = 0.3f;
    [SerializeField] private int maxCubes = 15;

    [SerializeField] private float maxSpeed = 10f;

    private List<Transform> cubes = new List<Transform>();
    private float step;

    void Start()
    {
        escooter = transform.parent.GetComponent<Segway>();
        eScooterController = transform.parent.GetComponent<EscooterController>();

        // Velocity arrow
        step = cubeLength + gap;
        cubes.Add(firstCube); 
    }

    void Update()
    {
        float speed = Mathf.Max(0, escooter.getVelosity());
        Vector3 forward = transform.parent.forward;

        float t = Mathf.Clamp01(speed / maxSpeed);
        int targetCount = Mathf.Clamp(Mathf.CeilToInt(t * maxCubes), 1, maxCubes);

        // Velocity arrow -------------------------------------------------------------
        // Create cubes
        AdjustCubeCount(targetCount);

        // Position cubes
        for (int i = 0; i < cubes.Count; i++)
        {
            Vector3 pos = transform.position + forward * step * (cubes.Count - 1 - i);

            cubes[i].position = pos;
            cubes[i].forward = forward;
        }

        Color color = GetSpeedColor(t);
        ApplyColor(color);
        SetOutline(eScooterController.throttleActive);
        // ------------------------------------------------------------------------------
    }

    void AdjustCubeCount(int targetCount)
    {
        // add
        while (cubes.Count < targetCount)
        {
            GameObject newCube = Instantiate(prefabArrowCube, transform);
            cubes.Add(newCube.transform); 
        }

        // remove
        while (cubes.Count > targetCount && cubes.Count > 1)
        {
            Transform last = cubes[cubes.Count - 1];
            cubes.RemoveAt(cubes.Count - 1);
            Destroy(last.gameObject);
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
    }
}