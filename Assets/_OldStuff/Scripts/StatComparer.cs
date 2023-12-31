using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatComparer : IComparer<monster>
{
    public int Compare(monster x, monster y)
    {
        if (x.GetMonsterID() == y.GetMonsterID())
        {
            if(x.isPlayer1() == true)
            {
                x.ChangeCurrentSpeed(-1);
                x.ChangeCurrentStrength(1);
                x.ChangeCurrentMagic(1);
            }
            else
            {
                y.ChangeCurrentSpeed(-1);
                y.ChangeCurrentStrength(1);
                y.ChangeCurrentMagic(1);
            }
        }

        int speedComparison = y.GetCurrentSpeed().CompareTo(x.GetCurrentSpeed());

        // Check if there's a speed tie
        if (speedComparison == 0)
        {
            // Custom logic for speed tie (you can adjust this part as needed)
            // For example, if tied, compare based on another stat like health

            int healthComparison = y.GetCurrentHealth().CompareTo(x.GetCurrentHealth());

            // If health is also tied, maintain the original speed-based order
            if (healthComparison == 0)
            {
                return speedComparison; // Maintain the original speed-based order
            }
            else
            {
                return healthComparison; // Compare by health if there's a tie in speed
            }
        }
        else
        {
            // If there's no tie, return the speed-based comparison result
            return speedComparison;
        }
    }
}