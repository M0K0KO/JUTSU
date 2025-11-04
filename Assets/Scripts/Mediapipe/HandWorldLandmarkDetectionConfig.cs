using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.Unity.HandWorldLandmarkDetection
{
    public class HandWorldLandmarkDetectionConfig
    {
        public Tasks.Core.BaseOptions.Delegate Delegate { get; set; } = Tasks.Core.BaseOptions.Delegate.CPU;

        public ImageReadMode ImageReadMode { get; set; } = ImageReadMode.CPUAsync;

        public Tasks.Vision.Core.RunningMode RunningMode { get; set; } = Tasks.Vision.Core.RunningMode.LIVE_STREAM;

        public int NumHands { get; set; } = 1;
        public float MinHandDetectionConfidence { get; set; } = 0.5f;
        public float MinHandPresenceConfidence { get; set; } = 0.5f;
        public float MinTrackingConfidence { get; set; } = 0.5f;
        public string ModelPath => "hand_landmarker.bytes";

        public HandLandmarkerOptions GetHandLandmarkerOptions(
            HandLandmarkerOptions.ResultCallback resultCallback = null)
        {
            return new HandLandmarkerOptions(
                new Tasks.Core.BaseOptions(Delegate, modelAssetPath: ModelPath),
                runningMode: RunningMode,
                numHands: NumHands,
                minHandDetectionConfidence: MinHandDetectionConfidence,
                minHandPresenceConfidence: MinHandPresenceConfidence,
                minTrackingConfidence: MinTrackingConfidence,
                resultCallback: resultCallback
            );
        }
    }
}
