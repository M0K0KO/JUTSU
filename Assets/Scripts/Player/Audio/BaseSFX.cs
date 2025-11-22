using UnityEngine;

public class BaseSFX : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    private AudioSource audioSource;
    [SerializeField] private float pitchRandomDeviation = 0.1f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioClip()
    {
        float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
        audioSource.pitch = randomPitch;
        audioSource.PlayOneShot(audioClip);
    }
}
