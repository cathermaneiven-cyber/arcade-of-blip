using UnityEngine;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    [Tooltip("The GameObject on the Player to enable when this is picked up.")]
    public GameObject playerItemToEnable;

    public void Interact()
    {
        if (playerItemToEnable != null)
            playerItemToEnable.SetActive(true);
        else
            Debug.LogWarning($"[PickupInteractable] No player item assigned on {gameObject.name}!");

        gameObject.SetActive(false);
    }
}