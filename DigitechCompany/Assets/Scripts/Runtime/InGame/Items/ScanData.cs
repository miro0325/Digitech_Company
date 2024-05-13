using System.Collections;
using System.Collections.Generic;

public struct ScanData
{
    public float gameTime;
    public float price;
    public List<ItemBase> items;

    public class EqualityComparer : IEqualityComparer<ScanData>
    {
        public bool Equals(ScanData x, ScanData y)
        {
            return x.gameTime == y.gameTime;
        }

        public int GetHashCode(ScanData obj)
        {
            return obj.gameTime.GetHashCode();
        }
    }
}