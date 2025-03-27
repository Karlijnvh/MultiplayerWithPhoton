using UnityEngine;

public class RuntimeInventoryLogic : MonoBehaviour
{
    public virtual void Use(GameObject player)
    {
        Debug.Log("Using item: " + gameObject.name);
    }
}
