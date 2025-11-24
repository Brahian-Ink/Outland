using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    
    public virtual void OnPlayerEnterRange(PlayerController player) { }

    public virtual void OnPlayerExitRange(PlayerController player) { }

    
    public abstract void Interact(PlayerController player);
}
