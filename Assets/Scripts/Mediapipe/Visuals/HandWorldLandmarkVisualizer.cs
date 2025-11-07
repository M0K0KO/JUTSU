using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mediapipe;
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
    
    [Header("SmoothTime Options")]
    [SerializeField] private float visualsPositionSmoothTime = 0.2f;
    [SerializeField] private float rootPositionSmoothTime = 0.2f;
    
    [Header("Position Control (X/Y)")]
    public float minUnityX = -2.0f;
    public float maxUnityX = 2.0f;
    public float maxUnityY = 2.0f;
    public float minUnityY = -2.0f;
    private Vector3 initialPosition;
    

    [Header("Depth Options")]
    [SerializeField] private float minHandScale = 0.08f; // 가장 가까운 손의 Z값
    [SerializeField] private float maxHandScale = 0.28f; // 가장 먼 손의 Z값
    [SerializeField] private float maxUnityZ = 2f; // 가장 가까울 때 Unity Z 위치
    [SerializeField] private float minUnityZ = -5f; // 가장 멀 때 Unity Z 위치
    
    public Vector3 rootTargetPosition = Vector3.zero;
    
    public float[] HandAngles { get; private set; } = new float[_AngleCount];

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        rootTargetPosition = transform.localPosition;
        initialPosition = transform.localPosition;
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
        
        ///////////////////////////////////////////////////
        /*
         string modelPath = Path.Combine(Application.streamingAssetsPath, "gesture_recognizer.bytes");
        Debug.Log($"모델 경로: {modelPath}");
        Debug.Log($"파일 존재: {File.Exists(modelPath)}");
    
        if (File.Exists(modelPath))
        {
            FileInfo fileInfo = new FileInfo(modelPath);
            Debug.Log($"파일 크기: {fileInfo.Length} bytes");
        }
        */
        ///////////////////////////////////////////////////
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
                UpdateRootTransform(_currentTarget.handLandmarks);
                UpdateVisualsTargetPosition(_currentTarget.handWorldLandmarks);
                GestureIndicator.instance.ChangeGestureIndicatorText(_currentTarget.gestures[0].categories[0].categoryName);
            }
        }
    }

    private void UpdateRootTransform(IReadOnlyList<mptcc.NormalizedLandmarks> landmarks2D)
    {
        Vector2 lm9 = new Vector2(landmarks2D[0].landmarks[1].x, landmarks2D[0].landmarks[1].y);
        Vector2 lm10 = new Vector2(landmarks2D[0].landmarks[2].x, landmarks2D[0].landmarks[2].y);
        Vector2 lm11 = new Vector2(landmarks2D[0].landmarks[3].x, landmarks2D[0].landmarks[3].y);
        Vector2 lm12 = new Vector2(landmarks2D[0].landmarks[4].x, landmarks2D[0].landmarks[4].y);
        float dist1 = Vector2.Distance(lm9, lm10);
        float dist2 = Vector2.Distance(lm10, lm11);
        float dist3 = Vector2.Distance(lm11, lm12);
        float thumbLengthSum = Mathf.Floor((dist1 + dist2 + dist3) * 100f) / 100f;
        float normalizedDepthFactor = Mathf.InverseLerp(minHandScale, maxHandScale, thumbLengthSum);
        float targetZPosition = Mathf.Lerp(minUnityZ, maxUnityZ, normalizedDepthFactor);

        
        float normalizedX = landmarks2D[0].landmarks[0].x;
        float normalizedY = landmarks2D[0].landmarks[0].y;
        float targetXPosition = Mathf.Lerp(minUnityX, maxUnityX, normalizedX);
        float targetYPosition = Mathf.Lerp(maxUnityY, minUnityY, normalizedY);

        rootTargetPosition = initialPosition + new Vector3(
            targetXPosition,
            targetYPosition,
            targetZPosition
        );
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
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            rootTargetPosition,
            rootPositionSmoothTime);
        
        for (int i = 0; i < _LandmarkCount; i++)
        {
            landmarkVisuals[i].transform.localPosition = Vector3.Lerp(
                landmarkVisuals[i].transform.localPosition,
                landmarkTargetPositions[i], 
                visualsPositionSmoothTime);
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