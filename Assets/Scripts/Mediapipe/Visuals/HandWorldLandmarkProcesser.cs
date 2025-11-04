using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using mptcc = Mediapipe.Tasks.Components.Containers;
using UnityEngine;

public class HandWorldLandmarkProcesser : MonoBehaviour
{
    public static HandWorldLandmarkProcesser instance;

    private const int _AngleCount = 21;
    private const int _LandmarkCount = 21;

    private readonly List<(int, int)> _connections = new List<(int, int)>
    {
        (0, 1),
        (1, 2),
        (2, 3),
        (3, 4),
        (0, 5),
        (5, 9),
        (9, 13),
        (13, 17),
        (0, 17),
        (5, 6),
        (6, 7),
        (7, 8),
        (9, 10),
        (10, 11),
        (11, 12),
        (13, 14),
        (14, 15),
        (15, 16),
        (17, 18),
        (18, 19),
        (19, 20),
    };

    private readonly object _currentTargetLock = new object();
    private HandLandmarkerResult _currentTarget;

    private bool isStale = false;

    [SerializeField] private GameObject landmarkPrefab;
    [SerializeField] private GameObject connectionVisualPrefab;
    private GameObject[] landmarkVisuals = new GameObject[_LandmarkCount];
    private GameObject[] connectionVisuals = new GameObject[_LandmarkCount];

    private HandLandmarkConnectionVisualController[] connectionVisualControllers =
        new HandLandmarkConnectionVisualController[_LandmarkCount];

    private Vector3[] landmarkTargetPositions = new Vector3[_LandmarkCount];

    private float landmarkScale = 20f;
    [SerializeField] private float visualsPositionSmoothTime = 0.2f;

    public float[] HandAngles { get; private set; } = new float[_AngleCount];

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < _LandmarkCount; i++)
        {
            var landmark = Instantiate(landmarkPrefab, transform);
            landmark.transform.SetParent(gameObject.transform);
            landmarkVisuals[i] = landmark;

            var connection = Instantiate(connectionVisualPrefab, transform);
            connection.transform.SetParent(gameObject.transform);
            connectionVisuals[i] = connection;

            connectionVisualControllers[i] = connection.GetComponent<HandLandmarkConnectionVisualController>();
        }

        DeactivateVisuals();
    }

    private void Update()
    {
        if (_currentTarget.handWorldLandmarks != null)
        {
            if (_currentTarget.handWorldLandmarks.Count == 0)
            {
                DeactivateVisuals();
            }
            else
            {
                ActivateVisuals();
            }
        }
        else
        {
            DeactivateVisuals();
        }

        MoveVisuals();
        DrawConnections();
    }

    private void LateUpdate()
    {
        if (isStale)
        {
            SyncNow();
        }
    }

    public void DrawLater(HandLandmarkerResult target) => UpdateCurrentTarget(target);

    private void UpdateCurrentTarget(HandLandmarkerResult newTarget)
    {
        lock (_currentTargetLock)
        {
            newTarget.CloneTo(ref _currentTarget);
            isStale = true;
        }
    }

    private void SyncNow()
    {
        lock (_currentTargetLock)
        {
            isStale = false;

            if (_currentTarget.handWorldLandmarks != null && _currentTarget.handWorldLandmarks.Count > 0)
            {
                UpdateVisualsTargetPosition(_currentTarget.handWorldLandmarks);
                UpdateHandAngles_ST(_currentTarget.handWorldLandmarks);
            }
        }
    }

    private void UpdateVisualsTargetPosition(IReadOnlyList<mptcc.Landmarks> targets)
    {
        for (int i = 0; i < _LandmarkCount; i++)
        {
            landmarkTargetPositions[i].x = targets[0].landmarks[i].x * landmarkScale;
            landmarkTargetPositions[i].y = -targets[0].landmarks[i].y * landmarkScale;
            landmarkTargetPositions[i].z = -targets[0].landmarks[i].z * landmarkScale;
        }
    }

    // function without Job System & BurstCompiler
    // ---------------------------------------------------------------------------------------------------------------------------------
    private void UpdateHandAngles_ST(IReadOnlyList<mptcc.Landmarks> targets)
    {
        if (targets == null || targets.Count == 0) return;

        var target = targets[0].landmarks;


        // --- 1. 엄지 (Thumb) (Angles 0-2) ---
        Vector3 v_thumb_mcp = GetNormalizedVector(target[0], target[1]);
        Vector3 v_thumb_pip = GetNormalizedVector(target[1], target[2]);
        Vector3 v_thumb_dip = GetNormalizedVector(target[2], target[3]);
        Vector3 v_thumb_tip = GetNormalizedVector(target[3], target[4]);
        HandAngles[0] = Vector3.Angle(v_thumb_mcp, v_thumb_pip); // 엄지 MCP
        HandAngles[1] = Vector3.Angle(v_thumb_pip, v_thumb_dip); // 엄지 PIP
        HandAngles[2] = Vector3.Angle(v_thumb_dip, v_thumb_tip); // 엄지 DIP

        // --- 2. 검지 (Index) (Angles 3-5) ---
        Vector3 v_index_mcp = GetNormalizedVector(target[0], target[5]);
        Vector3 v_index_pip = GetNormalizedVector(target[5], target[6]);
        Vector3 v_index_dip = GetNormalizedVector(target[6], target[7]);
        Vector3 v_index_tip = GetNormalizedVector(target[7], target[8]);
        HandAngles[3] = (Vector3.Angle(v_index_mcp, v_index_pip)); // 검지 MCP
        HandAngles[4] = (Vector3.Angle(v_index_pip, v_index_dip)); // 검지 PIP 
        HandAngles[5] = (Vector3.Angle(v_index_dip, v_index_tip)); // 검지 DIP

        // --- 3. 중지 (Middle) (Angles 6-8) ---
        Vector3 v_mid_mcp = GetNormalizedVector(target[0], target[9]);
        Vector3 v_mid_pip = GetNormalizedVector(target[9], target[10]);
        Vector3 v_mid_dip = GetNormalizedVector(target[10], target[11]);
        Vector3 v_mid_tip = GetNormalizedVector(target[11], target[12]);
        HandAngles[6] = (Vector3.Angle(v_mid_mcp, v_mid_pip)); // 중지 MCP
        HandAngles[7] = (Vector3.Angle(v_mid_pip, v_mid_dip)); // 중지 PIP
        HandAngles[8] = (Vector3.Angle(v_mid_dip, v_mid_tip)); // 중지 DIP

        // --- 4. 약지 (Ring) (Angles 9-11) ---
        Vector3 v_ring_mcp = GetNormalizedVector(target[0], target[13]);
        Vector3 v_ring_pip = GetNormalizedVector(target[13], target[14]);
        Vector3 v_ring_dip = GetNormalizedVector(target[14], target[15]);
        Vector3 v_ring_tip = GetNormalizedVector(target[15], target[16]);
        HandAngles[9] = (Vector3.Angle(v_ring_mcp, v_ring_pip)); // 약지 MCP
        HandAngles[10] = (Vector3.Angle(v_ring_pip, v_ring_dip)); // 약지 PIP
        HandAngles[11] = (Vector3.Angle(v_ring_dip, v_ring_tip)); // 약지 DIP

        // --- 5. 새끼 (Pinky) (Angles 12-14) ---
        Vector3 v_pinky_mcp = GetNormalizedVector(target[0], target[17]);
        Vector3 v_pinky_pip = GetNormalizedVector(target[17], target[18]);
        Vector3 v_pinky_dip = GetNormalizedVector(target[18], target[19]);
        Vector3 v_pinky_tip = GetNormalizedVector(target[19], target[20]);
        HandAngles[12] = (Vector3.Angle(v_pinky_mcp, v_pinky_pip)); // 새끼 MCP
        HandAngles[13] = (Vector3.Angle(v_pinky_pip, v_pinky_dip)); // 새끼 PIP
        HandAngles[14] = (Vector3.Angle(v_pinky_dip, v_pinky_tip)); // 새끼 DIP
    }

    private Vector3 GetNormalizedVector(mptcc.Landmark p1, mptcc.Landmark p2)
    {
        // (p2 - p1) 벡터를 계산하고 정규화(길이 1)합니다.
        return new Vector3(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z).normalized;
    }
    // ---------------------------------------------------------------------------------------------------------------------------------

    // function with Job System & BurstCompiler
    //private void UpdateHandAngles_MT(IReadOnlyList<mptcc.Landmarks> targets)
    //{
    //   
    //}

    private void ActivateVisuals()
    {
        if (landmarkVisuals.Length > 0)
        {
            for (int i = 0; i < _LandmarkCount; i++)
            {
                landmarkVisuals[i].SetActive(true);
                connectionVisuals[i].SetActive(true);
            }
        }
    }

    private void DeactivateVisuals()
    {
        if (landmarkVisuals.Length > 0)
        {
            for (int i = 0; i < _LandmarkCount; i++)
            {
                landmarkVisuals[i].SetActive(false);
                connectionVisuals[i].SetActive(false);
            }
        }
    }

    private void MoveVisuals()
    {
        for (int i = 0; i < _LandmarkCount; i++)
        {
            landmarkVisuals[i].transform.localPosition = Vector3.Lerp(landmarkVisuals[i].transform.localPosition,
                landmarkTargetPositions[i], visualsPositionSmoothTime);
        }
    }

    private void DrawConnections()
    {
        for (int i = 0; i < _LandmarkCount; i++)
        {
            connectionVisualControllers[i].SetPoints(landmarkVisuals[_connections[i].Item1].transform.position,
                landmarkVisuals[_connections[i].Item2].transform.position);
        }
    }
}