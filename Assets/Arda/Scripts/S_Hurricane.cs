using System.Collections;
using UnityEngine;

public class S_Hurricane : MonoBehaviour
{
    [Header("Hurricane Settings")]
    public float lifetime = 3f;  // How long the hurricane will exist
    [Header("Audio Settings")]
    public AudioClip hurricaneSound;  // The music or sound effect to play

    private AudioSource audioSource;

    private void Start()
    {
        // Create an AudioSource and set its properties
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = hurricaneSound;
        audioSource.loop = true;        // Loop the sound continuously
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;        // Volume (0–1 range)

        // Start playing the sound
        audioSource.Play();

        // Start the hurricane's lifetime coroutine
        StartCoroutine(HurricaneLifetime());
    }

    private IEnumerator HurricaneLifetime()
    {
        // Wait for the lifetime duration
        yield return new WaitForSeconds(lifetime);

        // Stop playing the sound
        audioSource.Stop();

        // Destroy the hurricane object
        Destroy(gameObject);

        // Notify the AirCharacter that the hurricane has been destroyed
        S_AirECharacter airCharacter = FindObjectOfType<S_AirECharacter>();
        if (airCharacter != null)
        {
            airCharacter.OnHurricaneDestroyed();
        }
    }
}
