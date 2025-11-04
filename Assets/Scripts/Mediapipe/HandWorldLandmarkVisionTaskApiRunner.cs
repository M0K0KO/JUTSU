using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample;
using UnityEngine;

namespace Mediapipe.Unity.HandWorldLandmarkDetection
{
    /// <summary>
    /// 코루틴 관리용 abstract class
    /// main loop가 될 _coroutine 변수를 알아서 관리해주고, taskApi를 만들어 줘야한다는 사실을 기억!
    /// </summary>
    /// <typeparam name="TTask"></typeparam>
    public abstract class HandWorldLandmarkVisionTaskApiRunner<TTask> : BaseRunner where TTask : Tasks.Vision.Core.BaseVisionTaskApi
    {
        private Coroutine _coroutine;
        protected TTask taskApi;

        public RunningMode runningMode;
        
        public override void Play()
        {
            if (_coroutine != null)
            {
                Stop();
            }
            base.Play();
            _coroutine = StartCoroutine(Run());
        }

        public override void Pause()
        {
            base.Pause();
            ImageSourceProvider.ImageSource.Pause();
        }

        public override void Resume()
        {
            base.Resume();
            var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
        }

        public override void Stop()
        {
            base.Stop();
            StopCoroutine(_coroutine);
            ImageSourceProvider.ImageSource.Stop();
            taskApi?.Close();
            taskApi = null;
        }
        
        protected abstract IEnumerator Run();
    }
}
