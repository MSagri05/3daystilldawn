using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField] int maxSlots = GameManager.INVENTORY_MAX_SLOTS;

    readonly List<ItemData> items = new List<ItemData>();
    int usedSlots;

    // What the player is carrying, mirrored across scene loads. The per-scene player
    // (and this component with it) is destroyed on every door transition, but ItemData
    // entries are ScriptableObject assets, so a static list keeps them alive; the next
    // scene's Inventory refills itself from it in Awake.
    static readonly List<ItemData> carried = new List<ItemData>();

    public UnityEvent<List<ItemData>> onChanged = new UnityEvent<List<ItemData>>();

    void Awake()
    {
        foreach (ItemData item in carried) {
            items.Add(item);
            usedSlots += item.slotSize;
        }
    }

    // Fresh start (new game from the title screen)
    public static void clearCarried() => carried.Clear();

    public bool addItem(ItemData item)
    {
        if (usedSlots + item.slotSize > maxSlots) {
            Debug.Log($"Inventory full — cannot add {item.itemName}");
            return false;
        }

        items.Add(item);
        usedSlots += item.slotSize;
        syncCarried();
        onChanged.Invoke(items);
        return true;
    }

    public bool removeItem(ItemData item)
    {
        if (!items.Remove(item)) return false;
        usedSlots -= item.slotSize;
        syncCarried();
        onChanged.Invoke(items);
        return true;
    }

    public bool useItem(ItemData item)
    {
        if (!items.Contains(item)) return false;

        bool used = item.use(gameObject);
        if (used && item.consumable) removeItem(item);
        return used;
    }

    void syncCarried()
    {
        carried.Clear();
        carried.AddRange(items);
    }

    public bool hasItem(ItemData item)                => items.Contains(item);
    public IReadOnlyList<ItemData> getItems()         => items;
    public int getUsedSlots()                         => usedSlots;
    public int getMaxSlots()                          => maxSlots;
}
