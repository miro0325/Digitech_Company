using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ItemContainer
{
    private int index;
    private ItemBase[] slots;

    public int Index
    {
        get => index;
        set
        {
            if(value == index) return;

            var pre = index;
            if(value < 0) value = slots.Length - 1;
            if(value > slots.Length - 1) value = 0;
            index = value;
            OnIndexChanged?.Invoke(pre, index);
        }
    }

    //old cur
    public event Action<int, int> OnIndexChanged;

    public ItemBase this[int index]
    {
        get => slots[index];
        set => slots[index] = value;
    }

    public ItemContainer(int size)
    {
        slots = new ItemBase[size];
    }

    public ItemBase GetCurrentSlotItem()
    {
        return slots[Index];
    }

    public bool TryInsertItem(ItemBase item)
    {
        if(slots[index] == null)
        {
            slots[index] = item;
            return true;   
        }
        else
        {
            if(TryGetEmptySlotIndex(out int index))
            {
                this.index = index;
                slots[index] = item;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private bool TryGetEmptySlotIndex(out int index)
    {
        index = -1;
        for(int i = 0; i < slots.Length; i++)
            if(slots[i] == null)
            {
                index = i;
                return true;
            }
        return false;
    }
}