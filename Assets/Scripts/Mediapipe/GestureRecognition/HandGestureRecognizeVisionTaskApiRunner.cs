using System.Collections;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using UnityEngine;

namespace Mediapipe.Tasks.Vision.GestureRecognizer
{
    /// <summary>
    /// 코루틴 관리용 abstract class
    /// main loop가 될 _coroutine 변수를 알아서 관리해주고, taskApi를 만들어 줘야한다는 사실을 기억!
    /// </summary>
    /// <typeparam name="TTask"></typeparam>
    public abstract class HandGestureRecognizeVisionTaskApiRunner<TTask> : BaseRunner
        where TTask : Tasks.Vision.Core.BaseVisionTaskApi
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
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            // Detach first so repeated Stop calls cannot close the same task twice.
            var taskToClose = taskApi;
            taskApi = null;
            try
            {
                (taskToClose as System.IDisposable)?.Dispose();
            }
            finally
            {
                ImageSourceProvider.ImageSource?.Stop();
            }
        }
        
        protected abstract IEnumerator Run();
    }
}
