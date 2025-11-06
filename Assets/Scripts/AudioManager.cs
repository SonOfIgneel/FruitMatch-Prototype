using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip flipSFX;
    public AudioClip matchSFX;
    public AudioClip gameOverSFX;

    private AudioSource audioSource;
    private Coroutine flipRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlayFlip()
    {
        if (flipSFX == null) return;

        if (flipRoutine != null)
            StopCoroutine(flipRoutine);

        flipRoutine = StartCoroutine(PlayFlipFastRoutine());
    }

    private IEnumerator PlayFlipFastRoutine()
    {
        audioSource.pitch = 2f;
        audioSource.PlayOneShot(flipSFX);

        yield return new WaitForSeconds(flipSFX.length / 2f);

        audioSource.pitch = 1f;      
        flipRoutine = null;
    }

    public void PlayMatch() => PlayNormal(matchSFX);
    public void PlayGameOver() => PlayNormal(gameOverSFX);

    private void PlayNormal(AudioClip clip)
    {
        if (clip == null) return;

        if (flipRoutine != null)
        {
            StopCoroutine(flipRoutine);
            flipRoutine = null;
        }

        audioSource.pitch = 1f;      
        audioSource.PlayOneShot(clip);
    }
}
