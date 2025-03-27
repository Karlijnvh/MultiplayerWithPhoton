using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Inventory Object")]
public class InventoryObject : ScriptableObject
{
    public GameObject objectPrefab;
    public Sprite objectImage;
    public int quantity = 0;
    public InventoryItemLogic itemLogic;
    public string objectTooltip;
}
