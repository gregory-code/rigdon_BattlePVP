using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatComparer : IComparer<monster>
{
    public int Compare(monster x, monster y)
    {
        // Compare based on speed
        return y.GetCurrentSpeed().CompareTo(x.GetCurrentSpeed()); // Compare in descending order (highest to lowest)
    }
}