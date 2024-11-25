using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IContainer
{
    [SerializeField] Grid grid;
    [SerializeField] ContainerSlot containerSlotPrefab;
    const int X_NUMSLOTS = 4, Y_NUMSLOTS = 2;

    [SerializeField] List<ItemStack> storedItems = new List<ItemStack>();

    [Header("Animation")]
    [SerializeField] Transform lidTransform;
    [SerializeField] Vector3 openRot, closedRot;
    [SerializeField] float openDuration;

    // Start is called before the first frame update
    public void InitContainer()
    {
        GenerateSlots();
    }

    void GenerateSlots()
    {
        int index = 0;
        for (int i = 0; i < X_NUMSLOTS; i++)
        {
            for (int j = 0; j < Y_NUMSLOTS; j++)
            {
                ContainerSlot clone = Instantiate(containerSlotPrefab, grid.GetCellCenterWorld(new Vector3Int(i, j)), Quaternion.identity, grid.transform);
                if (storedItems.Count - 1 >= index)
                    if (storedItems[index] != null)
                        clone.InitSlot(storedItems[index]);

                index++;
            }
        }
    }

    public void AddNewStoredItem(ItemStack itemStackToAdd)
    {
        storedItems.Add(itemStackToAdd);
    }

    public void OpenContainer()
    {
        lidTransform.DORotate(openRot, openDuration);
    }

    public void CloseContainer()
    {
        lidTransform.DORotate(closedRot, openDuration);
    }
}
