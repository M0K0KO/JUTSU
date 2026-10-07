using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Screen = UnityEngine.Screen;

// Opt-in diagnostic mode for the built player. Normal launches do not create it.
public class JutsuSmokeTest : MonoBehaviour
{
    private Texture2D _image;
    private float _started;
    private bool _failed;
    private float _vignetteBaselineEdge;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuSmokeTest") < 0 &&
            Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuVoiceCancelSmoke") < 0 &&
            Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuVignetteProbe") < 0 &&
            Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuGameplayProbe") < 0) return;
        var go = new GameObject("JUTSU smoke test");
        DontDestroyOnLoad(go);
        go.AddComponent<JutsuSmokeTest>();
    }

    private void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuGameplayProbe") >= 0)
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        _started = Time.realtimeSinceStartup;
        _image = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        _image.SetPixels32(new Color32[64 * 64]);
        _image.Apply();
        SceneManager.sceneLoaded += PrepareScene;
        Application.logMessageReceived += CheckLog;
    }

    private void PrepareScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main Combat Scene") return;
        var runner = FindFirstObjectByType<HandGestureRecognizerRunner>();
        if (runner == null) { Fail("Combat scene has no hand recognizer."); return; }
        typeof(BaseRunner).GetMethod("FindBootstrap", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(runner, null);
        // Deterministic blank camera input; this mode does not use the user's webcam.
        ImageSourceProvider.Initialize(null, new TestImageSource(_image), null);
        ImageSourceProvider.Switch(ImageSourceType.Image);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(5);
        var manager = VoiceRecognitionManager.instance;
        if (manager == null) { Fail("Voice manager did not initialize."); yield break; }
        while (manager.whisperManager.IsLoading && !_failed) yield return null;
        if (!manager.whisperManager.IsLoaded) { Fail("Whisper model did not load."); yield break; }

        for (var cycle = 0; cycle < 2; ++cycle)
        {
            yield return SceneManager.LoadSceneAsync("Main Combat Scene");
            yield return new WaitForSecondsRealtime(8);
            if (FindObjectsByType<HandGestureRecognizerRunner>(FindObjectsSortMode.None).Length != 1 ||
                FindObjectsByType<VoiceRecognitionManager>(FindObjectsSortMode.None).Length != 1)
            { Fail("A recognizer was missing or duplicated after scene load."); yield break; }
            var runner = FindFirstObjectByType<HandGestureRecognizerRunner>();
            var timestamp = (long)typeof(HandGestureRecognizerRunner).GetField("_lastTimestamp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(runner);
            if (timestamp <= 0) { Fail("Hand recognition did not submit image frames."); yield break; }

            var whisper = manager.whisperManager;
            // Exercise the actual gameplay cancellation method without accessing a microphone.
            var jutsu = FindFirstObjectByType<PlayerJutsuManager>();
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuGameplayProbe") >= 0)
            {
                yield return ProbeGameplay(jutsu);
                Application.Quit(_failed ? 1 : 0);
                yield break;
            }
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuVignetteProbe") >= 0)
            {
                yield return ProbeVignette(jutsu);
                Application.Quit(_failed ? 1 : 0);
                yield break;
            }
            if (cycle == 0) yield return ProbeVignette(jutsu);
            if (_failed) yield break;
            if (!CheckDomainVisuals(jutsu)) yield break;
            var mic = manager.microphoneRecord;
            var wasEnabled = mic.enabled;
            mic.enabled = false;
            var recording = mic.GetType().GetField("<IsRecording>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            var recognize = typeof(PlayerJutsuManager).GetMethod("RecognizeVoiceAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            for (var attempt = 0; attempt < 20; ++attempt)
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    recording.SetValue(mic, true);
                    var voice = (Task<string>)recognize.Invoke(jutsu, new object[] { cancellation.Token });
                    recording.SetValue(mic, false);
                    cancellation.Cancel();
                    while (!voice.IsCompleted && !_failed) yield return null;
                    if (voice.IsFaulted || voice.IsCanceled || voice.Result != string.Empty)
                    { Fail("Gameplay voice cancellation failed."); yield break; }
                }
            }
            mic.enabled = wasEnabled;
            var subscriptions = mic.GetType().GetField("OnRecordStop", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mic) as Delegate;
            if (subscriptions != null && Array.Exists(subscriptions.GetInvocationList(),
                callback => callback.Method.DeclaringType?.DeclaringType == typeof(PlayerJutsuManager)))
            { Fail("A canceled voice session kept a microphone subscription."); yield break; }
            Debug.Log("JUTSU_VOICE_CANCEL_RESULT: passed (20 gameplay cancellations)");
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-jutsuVoiceCancelSmoke") >= 0)
            {
                Debug.Log("JUTSU_SMOKE_RESULT: passed (voice cancellation)");
                Application.Quit(0);
                yield break;
            }
            whisper.initialPrompt = "first request";
            var audio = new float[48000];
            for (var i = 0; i < audio.Length; ++i)
                audio[i] = 0.2f * (float)Math.Sin(i * 2 * Math.PI * 440 / 16000);
            var first = whisper.GetTextAsync(audio, 16000, 1);
            whisper.initialPrompt = "second request";
            var second = whisper.GetTextAsync(audio, 16000, 1);
            while ((!first.IsCompleted || !second.IsCompleted) && !_failed) yield return null;
            if (first.IsFaulted || second.IsFaulted || first.Result == null || second.Result == null)
            { Fail("Whisper inference failed."); yield break; }

            var domain = typeof(PlayerJutsuManager).GetMethod("MuryokushoSequence", BindingFlags.Instance | BindingFlags.NonPublic);
            var casting = jutsu.StartCoroutine((IEnumerator)domain.Invoke(jutsu, null));
            if (cycle == 0)
            {
                yield return casting;
                if (!CheckDomainVisuals(jutsu)) yield break;
                Debug.Log("JUTSU_DOMAIN_RESULT: full cast restored startup visuals");
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.4f);
                var settings = ReadField<MuryokushoSequenceData>(jutsu, "muryokushoSequenceData");
                if (!jutsu.isInMuryokusho || RenderSettings.skybox != settings.spaceSkyboxMaterial)
                { Fail("Domain expansion did not activate before the interruption check."); yield break; }
            }
            var dissolve = ReadField<Material>(jutsu, "dissolveMaterial");
            var bloom = ReadField<Material>(jutsu, "bloomQuadMaterial");
            var baseline = ReadField<MuryokushoSequenceData>(jutsu, "muryokushoSequenceData").minCutoffHeight;

            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return new WaitForSecondsRealtime(2);
            if (!Mathf.Approximately(dissolve.GetFloat("_Cutoff_Height"), baseline) ||
                !Mathf.Approximately(bloom.GetFloat("_Alpha"), 0))
            { Fail("Scene unload left domain materials active."); yield break; }
            if (FindObjectsByType<HandGestureRecognizerRunner>(FindObjectsSortMode.None).Length != 0)
            { Fail("Hand recognizer survived combat scene unload."); yield break; }
            Debug.Log($"JUTSU_SMOKE_CYCLE: {cycle + 1} passed");
        }
        if (!_failed)
        {
            yield return SceneManager.LoadSceneAsync("Main Combat Scene");
            yield return new WaitForSecondsRealtime(1);
            if (!CheckDomainVisuals(FindFirstObjectByType<PlayerJutsuManager>())) yield break;
            yield return SceneManager.LoadSceneAsync("MainMenu");
            Debug.Log("JUTSU_SMOKE_RESULT: passed (40 voice cancellations, inference, full and interrupted domain casts, scene reload)");
            Application.Quit(0);
        }
    }

    private static T ReadField<T>(PlayerJutsuManager jutsu, string name) =>
        (T)typeof(PlayerJutsuManager).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(jutsu);

    private IEnumerator ProbeGameplay(PlayerJutsuManager jutsu)
    {
        yield return new WaitForEndOfFrame();
        CaptureGameplay("before");
        var mouse = InputSystem.AddDevice<Mouse>("Jutsu diagnostic mouse");
        InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Right));
        var deadline = Time.realtimeSinceStartup + 2;
        while (!jutsu.isUsingJutsu && Time.realtimeSinceStartup < deadline) yield return null;
        InputSystem.QueueStateEvent(mouse, new MouseState());
        if (!jutsu.isUsingJutsu) { InputSystem.RemoveDevice(mouse); Fail("Right mouse input did not enter jutsu mode."); yield break; }
        yield return new WaitForSecondsRealtime(2);
        if (!jutsu.isUsingJutsu) { InputSystem.RemoveDevice(mouse); Fail("Right mouse input did not enter jutsu mode."); yield break; }
        yield return new WaitForEndOfFrame();
        CaptureGameplay("jutsu");
        CaptureGameplayReference("jutsu-effect-on", false);
        CaptureGameplayReference("jutsu-v3-strength", false, 0.3f);
        CaptureGameplayReference("jutsu-effect-off", true);
        while (jutsu.isUsingJutsu && !_failed) yield return null;
        yield return new WaitForSecondsRealtime(2);
        yield return new WaitForEndOfFrame();
        CaptureGameplay("after");
        InputSystem.RemoveDevice(mouse);
        Debug.Log("JUTSU_GAMEPLAY_PROBE: captured actual screen and right mouse input");
    }

    private static string ProbeDirectory()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-jutsuProbePath");
        var directory = index >= 0 && index + 1 < args.Length ? args[index + 1] : Application.persistentDataPath;
        Directory.CreateDirectory(directory);
        return directory;
    }

    private void CaptureGameplay(string phase)
    {
        var image = ScreenCapture.CaptureScreenshotAsTexture();
        if (image == null) { Fail("Actual screen capture unavailable."); return; }
        File.WriteAllBytes(Path.Combine(ProbeDirectory(), phase + ".png"), image.EncodeToPNG());
        Destroy(image);
        GlobalVolumeManager.instance.volume.profile.TryGet<Vignette>(out var profile);
        var camera = Camera.main.GetUniversalAdditionalCameraData();
        var effective = VolumeManager.instance.stack.GetComponent<Vignette>();
        Debug.Log($"JUTSU_GAMEPLAY_{phase}: camera={Camera.main.name}; size={Screen.width}x{Screen.height}; " +
                  $"profile={profile.intensity.value}; effective={effective.intensity.value}; post={camera.renderPostProcessing}; " +
                  $"cameraState={PlayerCameraStateHandler.instance.currentState}; timeScale={Time.timeScale}");
    }

    private void CaptureGameplayReference(string phase, bool disableEffect, float? intensity = null)
    {
        GlobalVolumeManager.instance.volume.profile.TryGet<Vignette>(out var vignette);
        var wasActive = vignette.active;
        var originalIntensity = vignette.intensity.value;
        var previous = RenderTexture.active;
        var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        var image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        try
        {
            if (disableEffect) vignette.active = false;
            if (intensity.HasValue) vignette.intensity.value = intensity.Value;
            RenderPipeline.SubmitRenderRequest(Camera.main, new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(ProbeDirectory(), phase + ".png"), image.EncodeToPNG());
        }
        finally
        {
            vignette.active = wasActive;
            vignette.intensity.value = originalIntensity;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Destroy(image);
        }
    }

    private IEnumerator ProbeVignette(PlayerJutsuManager jutsu)
    {
        var camera = Camera.main;
        var originalMask = camera.cullingMask;
        var originalFlags = camera.clearFlags;
        var originalColor = camera.backgroundColor;
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var enabledCanvases = Array.FindAll(canvases, canvas => canvas.enabled);
        camera.cullingMask = 0;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.gray;
        foreach (var canvas in enabledCanvases) canvas.enabled = false;
        yield return new WaitForSecondsRealtime(2);
        CaptureVignette("before");
        // Verify that gameplay entry restores accidentally disabled volume/camera settings.
        var volume = GlobalVolumeManager.instance.volume;
        volume.profile.TryGet<Vignette>(out var vignette);
        vignette.active = false;
        vignette.intensity.overrideState = false;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
        camera.GetUniversalAdditionalCameraData().volumeLayerMask = 0;
        camera.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.ViaScripting);
        var mode = jutsu.StartCoroutine(jutsu.JutsuMode());
        yield return new WaitForSecondsRealtime(3);
        CaptureVignette("jutsu");
        yield return mode;
        yield return new WaitForSecondsRealtime(3);
        CaptureVignette("after");
        camera.cullingMask = originalMask;
        camera.clearFlags = originalFlags;
        camera.backgroundColor = originalColor;
        foreach (var canvas in enabledCanvases) if (canvas != null) canvas.enabled = true;
        if (!_failed) Debug.Log("JUTSU_VIGNETTE_PROBE: passed (gameplay entry, rendered darkening, exit recovery)");
    }

    private void CaptureVignette(string phase)
    {
        var manager = GlobalVolumeManager.instance;
        manager.volume.profile.TryGet<Vignette>(out var profile);
        var camera = Camera.main.GetUniversalAdditionalCameraData();
        var target = RenderTexture.GetTemporary(640, 360, 24, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        var image = new Texture2D(640, 360, TextureFormat.RGBA32, false);
        try
        {
            RenderPipeline.SubmitRenderRequest(Camera.main,
                new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 640, 360), 0, 0);
            image.Apply();
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
        }
        var effective = VolumeManager.instance.stack.GetComponent<Vignette>();
        Debug.Log($"JUTSU_VIGNETTE_STATE: manager={manager != null}; profile={profile != null}; effective={effective != null}");
        Debug.Log($"JUTSU_VOLUME_STATE: enabled={manager.volume.enabled}; global={manager.volume.isGlobal}; " +
                  $"weight={manager.volume.weight}; layer={manager.gameObject.layer}; mask={camera.volumeLayerMask.value}; " +
                  $"registered={Array.Exists(VolumeManager.instance.GetVolumes(camera.volumeLayerMask), v => v == manager.volume)}");
        if (profile == null || effective == null) { Destroy(image); Fail("Missing vignette in render stack."); return; }
        var center = image.GetPixel(image.width / 2, image.height / 2).grayscale;
        var edge = image.GetPixel(image.width / 20, image.height / 20).grayscale;
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-jutsuProbePath");
        if (index >= 0 && index + 1 < args.Length)
        {
            Directory.CreateDirectory(args[index + 1]);
            File.WriteAllBytes(Path.Combine(args[index + 1], phase + ".png"), image.EncodeToPNG());
        }
        Destroy(image);
        Debug.Log($"JUTSU_VIGNETTE_{phase}: profile={profile.intensity.value}; effective={effective.intensity.value}; " +
                  $"active={effective.active}; override={profile.intensity.overrideState}; post={camera.renderPostProcessing}; " +
                  $"update={camera.requiresVolumeFrameworkUpdate}; center={center}; edge={edge}");
        if (Mathf.Abs(profile.intensity.value - effective.intensity.value) > 0.01f || !effective.active || !camera.renderPostProcessing)
            Fail("The camera did not apply the gameplay vignette.");
        if (phase == "before") _vignetteBaselineEdge = edge;
        if (phase == "jutsu" && edge >= _vignetteBaselineEdge - 0.05f)
            Fail("Jutsu mode did not darken rendered screen edges.");
        if (phase == "after" && Mathf.Abs(edge - _vignetteBaselineEdge) > 0.02f)
            Fail("Vignette did not recover after jutsu mode exit.");
    }

    private bool CheckDomainVisuals(PlayerJutsuManager jutsu)
    {
        if (jutsu == null) { Fail("Missing jutsu manager."); return false; }
        var settings = ReadField<MuryokushoSequenceData>(jutsu, "muryokushoSequenceData");
        var renderer = ReadField<ScriptableRendererData>(jutsu, "rendererData");
        if (jutsu.isInMuryokusho || RenderSettings.skybox != settings.originalSkyboxMaterial ||
            !Mathf.Approximately(ReadField<Material>(jutsu, "bloomQuadMaterial").GetFloat("_Alpha"), 0) ||
            !Mathf.Approximately(ReadField<Material>(jutsu, "dissolveMaterial").GetFloat("_Cutoff_Height"), settings.minCutoffHeight) ||
            ReadField<Transform>(jutsu, "intersectionSphereTransform").localScale != Vector3.zero ||
            renderer.rendererFeatures.Exists(feature => (feature.name == "Player" || feature.name == "Enemy") && feature.isActive))
        { Fail("Domain expansion visuals were active outside a cast."); return false; }
        return true;
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup - _started > 150) Fail("Smoke test timed out.");
    }

    private void CheckLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            Fail("Runtime error: " + message);
    }

    private void Fail(string message)
    {
        if (_failed) return;
        _failed = true;
        Debug.Log("JUTSU_SMOKE_RESULT: failed; " + message);
        Application.Quit(1);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= PrepareScene;
        Application.logMessageReceived -= CheckLog;
        if (_image != null) Destroy(_image);
    }

    private sealed class TestImageSource : StaticImageSource
    {
        private readonly Texture _texture;
        private bool _playing;
        public TestImageSource(Texture texture) : base(new[] { texture }, new[] { new ImageSource.ResolutionStruct(64, 64, 0) })
        { _texture = texture; SelectResolution(0); }
        public override bool isPrepared => _texture != null;
        public override bool isPlaying => _playing;
        public override Texture GetCurrentTexture() => _texture;
        public override IEnumerator Play() { _playing = true; yield return null; }
        public override IEnumerator Resume() { _playing = true; yield return null; }
        public override void Pause() { _playing = false; }
        public override void Stop() { _playing = false; }
    }
}
