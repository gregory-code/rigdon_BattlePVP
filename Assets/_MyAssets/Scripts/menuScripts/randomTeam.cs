using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "RandomTeam")]
public class randomTeam : ScriptableObject
{
    [SerializeField] monsterPreferences[] randoTeam = new monsterPreferences[3];

    public monsterPreferences[] GetTeam()
    {
        return randoTeam;
    }
}
