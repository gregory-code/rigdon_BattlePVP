using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public interface IDataPersistence
{
    void LoadData(Dictionary<string, object> dataDictionary);

    void LoadOtherPlayersData(string key, object data);
}
