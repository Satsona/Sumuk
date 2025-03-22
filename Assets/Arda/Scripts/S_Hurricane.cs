using System.Collections;
using UnityEngine;

public class S_Hurricane : MonoBehaviour
{
    [Header("Hurricane Settings")]
    public float lifetime = 3f;  // How long the hurricane will last before disappearing

    private void Start()
    {
        // Start the countdown for the hurricane's lifetime
        StartCoroutine(HurricaneLifetime());
    }

    private IEnumerator HurricaneLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        // Destroy the hurricane after the lifetime ends
        Destroy(gameObject);

        // Notify the AirCharacter that the hurricane has been destroyed
        // This assumes that the AirCharacter script is attached to the player and we can access it
        S_AirECharacter airCharacter = FindObjectOfType<S_AirECharacter>();  // Find the player in the scene
        if (airCharacter != null)
        {
            airCharacter.OnHurricaneDestroyed();  // Call a method in the AirCharacter to reset the flag
        }
    }
}
