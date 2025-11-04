using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.GestureRecognizer;
using Mediapipe.Tasks.Vision.HandLandmarker;
using mptcc = Mediapipe.Tasks.Components.Containers;
using UnityEngine;

public class HandWorldLandmarkVisualizer : MonoBehaviour
{
    public static HandWorldLandmarkVisualizer instance;

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
    private GestureRecognizerResult _currentTarget;

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

    public void DrawLater(GestureRecognizerResult target) => UpdateCurrentTarget(target);

    private void UpdateCurrentTarget(GestureRecognizerResult newTarget)
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