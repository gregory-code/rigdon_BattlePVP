using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatComparer : IComparer<critter>
{
    public int Compare(critter x, critter y)
    {
        // Compare based on speed
        return y.getCurrentSpeed().CompareTo(x.getCurrentSpeed()); // Compare in descending order (highest to lowest)
    }
}