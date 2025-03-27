using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public List<itemType> inventoryList;
    public int playerReach;
    [SerializeField] Camera cam;
    [SerializeField] GameObject pressToPickup_gameobject;
    [SerializeField] Image[] inventorySlotImage = new Image[9];
    [SerializeField] Image[] inventoryBackgroundImage = new Image[9];
    [SerializeField] Sprite prazdnySlotImage;
    [SerializeField] GameObject throwObject_gameobject;
    [SerializeField] KeyCode throwItemKey;
    [SerializeField] KeyCode pickUpItemKey;

    public int selectedItem = 0;

    [Space(10)]
    [Header("Zbrane gameobjects")]
    [SerializeField] GameObject bow_item;
    [SerializeField] GameObject sword_item;
    [SerializeField] GameObject hammer_item;
    [SerializeField] GameObject blade_item;
    [SerializeField] GameObject axe_item;
    [SerializeField] GameObject maul_item;

    [Header("weapon prefabs")]
    [SerializeField] GameObject bow_prefab;
    [SerializeField] GameObject sword_prefab;
    [SerializeField] GameObject hammer_prefab;
    [SerializeField] GameObject blade_prefab;
    [SerializeField] GameObject axe_prefab;
    [SerializeField] GameObject maul_prefab;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>();
    private Dictionary<itemType, GameObject> itemInstantiate = new Dictionary<itemType, GameObject>();

    void Start()
    {
        itemSetActive.Add(itemType.Bow, bow_item);
        itemSetActive.Add(itemType.Sword, sword_item);
        itemSetActive.Add(itemType.Hammer, hammer_item);
        itemSetActive.Add(itemType.Blade, blade_item);
        itemSetActive.Add(itemType.Axe, axe_item);
        itemSetActive.Add(itemType.Maul, maul_item);

        itemInstantiate.Add(itemType.Bow, bow_prefab);
        itemInstantiate.Add(itemType.Sword, sword_prefab);
        itemInstantiate.Add(itemType.Hammer, hammer_prefab);
        itemInstantiate.Add(itemType.Blade, blade_prefab);
        itemInstantiate.Add(itemType.Axe, axe_prefab);
        itemInstantiate.Add(itemType.Maul, maul_prefab);

        NewItemSelected();
    }

    void Update()
    {
        if (Input.GetKeyDown(throwItemKey) && inventoryList.Count > 0)
        {
            Instantiate(itemInstantiate[inventoryList[selectedItem]], position: throwObject_gameobject.transform.position, new Quaternion());
            inventoryList.RemoveAt(selectedItem);

            if (selectedItem >= inventoryList.Count) // Check if selectedItem is out of bounds
            {
                selectedItem = Mathf.Max(0, inventoryList.Count - 1); // Adjust selectedItem
            }
            NewItemSelected();
        }

        for (int i = 0; i < 5; i++)
        {
            if (i < inventoryList.Count)
            {
                GameObject itemGO = itemSetActive[inventoryList[i]];
                if (itemGO != null) // Check if the GameObject still exists
                {
                    Item itemComponent = itemGO.GetComponent<Item>();
                    if (itemComponent != null) // Check if the component is accessible
                    {
                        inventorySlotImage[i].sprite = itemComponent.itemScriptableObject.item_sprite;
                    }
                }
            }
            else
            {
                inventorySlotImage[i].sprite = prazdnySlotImage;
            }
        }

        int a = 0;
        foreach (Image image in inventoryBackgroundImage)
        {
            if (a == selectedItem)
            {
                image.color = new Color32(145, 255, 126, 255);
            }
            else
            {
                image.color = new Color32(219, 219, 219, 255);
            }
            a++;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach) && Input.GetKey(pickUpItemKey))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();
            if (item != null)
            {
                // Controleer of de inventory vol is
                if (inventoryList.Count < 6)
                {
                    pressToPickup_gameobject.SetActive(true);
                    inventoryList.Add(hitInfo.collider.GetComponent<WeaponPickable>().weaponScriprableObject.item_type);
                    item.PickItem();
                }
                else
                {
                    pressToPickup_gameobject.SetActive(true);
                    // Pas de UI aan om aan te geven dat de inventory vol is
                    pressToPickup_gameobject.GetComponentInChildren<Text>().text = "Inventory Full - Throw an item to pick up";
                }
            }
            else
            {
                pressToPickup_gameobject.SetActive(false);
            }
        }
        else
        {
            pressToPickup_gameobject.SetActive(false);
        }

        HandleHotkeys();
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0)
        {
            selectedItem = 0;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1)
        {
            selectedItem = 1;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2)
        {
            selectedItem = 2;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && inventoryList.Count > 3)
        {
            selectedItem = 3;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) && inventoryList.Count > 4)
        {
            selectedItem = 4;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) && inventoryList.Count > 5)
        {
            selectedItem = 5;
            NewItemSelected();
        }
    }

    private void NewItemSelected()
    {
        bow_item.SetActive(false);
        sword_item.SetActive(false);
        hammer_item.SetActive(false);
        blade_item.SetActive(false);
        axe_item.SetActive(false);
        maul_item.SetActive(false);

        if (inventoryList.Count > 0)
        {
            GameObject selectedItemGameObject = itemSetActive[inventoryList[selectedItem]];
            selectedItemGameObject.SetActive(true);
        }
    }
}

public interface IPickable
{
    void PickItem();
}
