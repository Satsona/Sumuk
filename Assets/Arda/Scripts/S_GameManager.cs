using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class S_GameManager : MonoBehaviour
{
    public GameObject airCharacterPrefab;
    public GameObject fireCharacterPrefab;

    public CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera

    private GameObject currentCharacter;

    void Start()
    {
        // Get reference to Cinemachine camera if not set in Inspector
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        // Spawn the air character by default
        currentCharacter = Instantiate(airCharacterPrefab, Vector3.zero, Quaternion.identity);

        // Set the virtual camera to follow the newly spawned character immediately
        if (virtualCamera != null)
        {
            virtualCamera.Follow = currentCharacter.transform; // Make the camera follow the first character
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("1 Pressed");
            SwitchCharacter(airCharacterPrefab);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("2 Pressed");
            SwitchCharacter(fireCharacterPrefab);
        }
    }

    void SwitchCharacter(GameObject newCharacterPrefab)
    {

        if (currentCharacter != null)
        {
            // Save the current movement state (e.g., velocity, position, etc.)
            Rigidbody2D currentRb = currentCharacter.GetComponent<Rigidbody2D>();
            Vector2 currentVelocity = currentRb != null ? currentRb.velocity : Vector2.zero;
            Vector3 oldPosition = currentCharacter.transform.position;


            // Destroy the old character
            Destroy(currentCharacter);

            // Instantiate the new character
            currentCharacter = Instantiate(newCharacterPrefab, oldPosition, Quaternion.identity);

            // Set the camera to follow the new character
            if (virtualCamera != null)
            {
                virtualCamera.Follow = currentCharacter.transform;
            }



            // Apply the saved velocity or movement state to the new character
            Rigidbody2D newRb = currentCharacter.GetComponent<Rigidbody2D>();
            if (newRb != null)
            {
                newRb.velocity = currentVelocity; // Apply the previous character's velocity
            }
        }
    }
}