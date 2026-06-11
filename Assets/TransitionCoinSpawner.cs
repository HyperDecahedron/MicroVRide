using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// - Coins spawn along a straight line from waypointStart -> waypointEnd
/// - Coins are spaced uniformly
/// - Lateral offsets are derived from waypoint0/1/2/3 distances to waypointStart
/// - Coin order is randomized
/// - Keeps original CoinMeta, airHeight, CSV export, etc.
[ExecuteAlways]
public class TransitionCoinSpawner : MonoBehaviour
{
    [Header("Path References")]
    public Transform waypointStart;
    public Transform waypointEnd;

    [Header("Difficulty Waypoints")]
    [Tooltip("Distance from waypointStart defines the lateral offset.")]
    public Transform waypoint0;
    public Transform waypoint1;
    public Transform waypoint2;
    public Transform waypoint3;

    [Header("Prefab")]
    public GameObject coinPrefab;

    [Header("Sampling")]
    [Tooltip("Distance between consecutive coins (m).")]
    public float coinSpacing = 20f;

    [Header("Randomness")]
    public int randomSeed = 0;

    [Header("Edit Mode")]
    public bool generateInEditMode = true;
    public bool autoRegenerateOnChange = false;

    [Header("Play Mode")]
    public bool spawnOnStartPlay = false;

    [Header("Vertical")]
    public float airHeight = 1.5f;

    [Header("Container (created if missing)")]
    public Transform container;
    const string ContainerName = "CoinsContainer";
    const string AnchorName = "SpawnAnchor";

    [Header("XR")]
    [Tooltip("XR headset camera.")]
    public Transform xrCamera;

    // Runtime cache
    List<Vector3> _samples;
    List<Vector3> _rights;

#if UNITY_EDITOR
    bool _pendingRegen;
#endif

    void OnEnable()
    {
        EnsureContainer();

#if UNITY_EDITOR
        if (!Application.isPlaying && generateInEditMode && autoRegenerateOnChange)
            ScheduleEditorRegen();
#endif
    }

    void Start()
    {
        if (Application.isPlaying && spawnOnStartPlay)
            SpawnRuntime();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        if (!generateInEditMode || !autoRegenerateOnChange) return;
        ScheduleEditorRegen();
#endif
    }

    // ---------------- Buttons ----------------

    [ContextMenu("Bake Coins (Edit Mode)")]
    public void BakeCoins()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogWarning("Bake is for Edit Mode.");
            return;
        }

        EditorRegenerateNow();
#endif
    }

    [ContextMenu("Clear Baked Coins")]
    public void ClearBaked()
    {
        EnsureContainer();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);

            return;
        }
#endif

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    [ContextMenu("Spawn Runtime (Play)")]
    public void SpawnRuntime()
    {
        if (!Validate()) return;

        ClearBaked();

        int seed = (randomSeed > 0) ? randomSeed : Environment.TickCount;
        UnityEngine.Random.InitState(seed);

        Debug.Log($"[TransitionCoinSpawner] Using random seed = {seed}");

        BuildSamples();

        GenerateAndPlace((prefab, parent) => Instantiate(prefab, parent));
    }

    [ContextMenu("Export CSV (coins)")]
    public void ExportCSV()
    {
        var metas = container ? container.GetComponentsInChildren<CoinMeta>() : null;

        if (metas == null || metas.Length == 0)
        {
            Debug.LogWarning("[TransitionCoinSpawner] No CoinMeta found.");
            return;
        }

        string dir = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
        string path = Path.Combine(dir, $"coin_log_{name}.csv");

        using (var sw = new StreamWriter(path))
        {
            sw.WriteLine("index,s_meters,lateral_m,delta_lateral_m,label,segmentIndex,world_x,world_y,world_z");

            foreach (var m in metas)
            {
                Vector3 p = m.transform.position;

                sw.WriteLine(
                    $"{m.index},{m.s:F3},{m.lateral:F3},{m.deltaLateral:F3},{m.label},{m.segmentIndex},{p.x:F3},{p.y:F3},{p.z:F3}"
                );
            }
        }

        Debug.Log($"[TransitionCoinSpawner] CSV exported: {path}");
    }

    // ---------------- Generation ----------------

#if UNITY_EDITOR
    void ScheduleEditorRegen()
    {
        if (_pendingRegen) return;

        _pendingRegen = true;

        EditorApplication.delayCall += () =>
        {
            _pendingRegen = false;

            if (this == null) return;
            if (!generateInEditMode) return;

            EditorRegenerateNow();
        };
    }

    void EditorRegenerateNow()
    {
        if (!Validate()) return;

        ClearBaked();

        int seed = (randomSeed > 0) ? randomSeed : Environment.TickCount;
        UnityEngine.Random.InitState(seed);

        Debug.Log($"[TransitionCoinSpawner] Using random seed = {seed}");

        BuildSamples();

        GenerateAndPlace(
            (prefab, parent) =>
            {
                var prefabRef = PrefabUtility.GetCorrespondingObjectFromSource(prefab) ?? prefab;

                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef, parent);

                Undo.RegisterCreatedObjectUndo(inst, "Bake Coin");

                return inst;
            }
        );

        EditorUtility.SetDirty(gameObject);
    }
#endif

    delegate GameObject Instantiator(GameObject prefab, Transform parent);

    void BuildSamples()
    {
        _samples = new List<Vector3>();
        _rights = new List<Vector3>();

        Vector3 start = waypointStart.position;
        Vector3 end = waypointEnd.position;

        Vector3 forward = (end - start).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        float totalDistance = Vector3.Distance(start, end);

        int coinCount = Mathf.FloorToInt(totalDistance / coinSpacing);

        for (int i = 0; i < coinCount; i++)
        {
            float s = (i + 1) * coinSpacing;

            Vector3 pos = start + forward * s;

            _samples.Add(pos);
            _rights.Add(right);
        }

        Debug.Log($"[TransitionCoinSpawner] Generated {_samples.Count} samples.");
    }

    void GenerateAndPlace(Instantiator inst)
    {
        if (_samples == null || _samples.Count == 0)
        {
            Debug.LogWarning("[TransitionCoinSpawner] No samples.");
            return;
        }

        // Build lateral pool with provenance (waypoint type included)
        List<(float lat, int type)> lateralPool = new List<(float lat, int type)>();

        AddWaypointLaterals(lateralPool, waypoint0, 0);
        AddWaypointLaterals(lateralPool, waypoint1, 1);
        AddWaypointLaterals(lateralPool, waypoint2, 2);
        AddWaypointLaterals(lateralPool, waypoint3, 3);

        if (lateralPool.Count == 0)
        {
            Debug.LogWarning("[TransitionCoinSpawner] No lateral waypoints assigned.");
            return;
        }

        Shuffle(lateralPool);

        float prevLat = 0f;
        bool hasPrev = false;

        int c0 = 0, c1 = 0, c2 = 0, c3 = 0;

        int maxCoins = Mathf.Min(_samples.Count, lateralPool.Count);

        for (int i = 0; i < maxCoins; i++)
        {
            Vector3 basePos = _samples[i];
            Vector3 right = _rights[i];

            var entry = lateralPool[i];
            float lat = entry.lat;
            int type = entry.type;

            if (type == 0) c0++;
            else if (type == 1) c1++;
            else if (type == 2) c2++;
            else c3++;

            float delta = hasPrev ? Mathf.Abs(lat - prevLat) : 0f;
            DifficultyLabel label = Classify(delta);

            Vector3 pos = basePos + right * lat;
            pos.y += airHeight;

            Quaternion rot = coinPrefab.transform.rotation;

            GameObject coin = inst(coinPrefab, container);

            int coinsLayer = LayerMask.NameToLayer("Coins");
            if (coinsLayer >= 0)
                coin.layer = coinsLayer;

#if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(coin, 0);
#endif

            Vector3 localAnchor = Vector3.zero;
            var anchor = coin.transform.Find(AnchorName);
            if (anchor)
                localAnchor = anchor.localPosition;

            Vector3 worldPos = pos - (rot * localAnchor);
            coin.transform.SetPositionAndRotation(worldPos, rot);

            var meta = coin.GetComponent<CoinMeta>() ?? coin.AddComponent<CoinMeta>();
            meta.index = i;
            meta.s = (i + 1) * coinSpacing;
            meta.lateral = lat;
            meta.deltaLateral = delta;
            meta.label = label;
            meta.segmentIndex = 0;

            hasPrev = true;
            prevLat = lat;
        }

        Debug.Log(
            $"[TransitionCoinSpawner] Waypoint distribution -> WP0:{c0}, WP1:{c1}, WP2:{c2}, WP3:{c3}"
        );

        Debug.Log($"[TransitionCoinSpawner] Spawned {_samples.Count} coins.");
    }

    void AddWaypointLaterals(List<(float lat, int type)> list, Transform wp, int type)
    {
        if (!wp) return;

        Vector3 start = waypointStart.position;
        Vector3 end = waypointEnd.position;

        Vector3 forward = (end - start).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 offset = wp.position - start;

        float lateral = Vector3.Dot(offset, right);

        for (int i = 0; i < 5; i++)
            list.Add((lateral, type));
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);

            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    DifficultyLabel Classify(float delta)
    {
        if (delta <= 0.2f)
            return DifficultyLabel.Easy;

        if (delta <= 0.5f)
            return DifficultyLabel.Medium;

        return DifficultyLabel.Hard;
    }

    // ---------------- Helpers ----------------

    void EnsureContainer()
    {
        if (container) return;

        var found = transform.Find(ContainerName);

        if (found)
        {
            container = found;
            return;
        }

        var go = new GameObject(ContainerName);

        go.transform.SetParent(transform, false);

        container = go.transform;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            Undo.RegisterCreatedObjectUndo(go, "Create Coins Container");
#endif
    }

    bool Validate()
    {
        if (!waypointStart)
        {
            Debug.LogError("[TransitionCoinSpawner] waypointStart missing.");
            return false;
        }

        if (!waypointEnd)
        {
            Debug.LogError("[TransitionCoinSpawner] waypointEnd missing.");
            return false;
        }

        if (!coinPrefab)
        {
            Debug.LogError("[TransitionCoinSpawner] coinPrefab missing.");
            return false;
        }

        return true;
    }

    void OnDrawGizmos()
    {
        if (!waypointStart || !waypointEnd) return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawSphere(waypointStart.position, 0.1f);
        Gizmos.DrawSphere(waypointEnd.position, 0.1f);

        Gizmos.DrawLine(waypointStart.position, waypointEnd.position);

        DrawWaypointGizmo(waypoint0, Color.green);
        DrawWaypointGizmo(waypoint1, Color.yellow);
        DrawWaypointGizmo(waypoint2, Color.magenta);
        DrawWaypointGizmo(waypoint3, Color.red);
    }

    void DrawWaypointGizmo(Transform t, Color c)
    {
        if (!t) return;

        Gizmos.color = c;
        Gizmos.DrawSphere(t.position, 0.08f);
    }
}