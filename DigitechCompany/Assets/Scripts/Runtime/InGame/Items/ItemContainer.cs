using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ItemContainer
{
    private int index;
    private float wholeWeight;
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

    public float WholeWeight => wholeWeight;

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
            wholeWeight += item.Weight;
            return true;
        }
        else
        {
            if(TryGetEmptySlotIndex(out int index))
            {
                slots[index] = item;
                wholeWeight += item.Weight;
                Index = index;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void PopCurrentItem()
    {
        if(slots[index] == null) return;
        wholeWeight -= slots[index].Weight;
        slots[index] = null;
    }

    private bool TryGetEmptySlotIndex(out int index)
    {
        for(index = 0; index < slots.Length; index++)
            if(slots[index] == null) return true;
        return false;
    }
}