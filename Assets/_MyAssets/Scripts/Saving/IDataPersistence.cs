using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public interface IDataPersistence
{
    void LoadData(GameData data);

    GameData SaveData(GameData data);
}
