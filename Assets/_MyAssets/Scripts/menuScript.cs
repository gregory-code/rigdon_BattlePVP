using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Linq;

public class menuScript : MonoBehaviour, IDataPersistence
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] _background_Sprites;
    [SerializeField] private Sprite[] _button_Sprites;

    [Header("Variables")]

    public string PlayerName;

    private string _fieldString;
    private int _fieldInt;

    private void Start()
    {
        menuStyle(Random.Range(0, _background_Sprites.Length)); // picks a random style at game start
    }

    private void menuStyle(int change)
    {
        transform.GetChild(0).GetComponent<Image>().sprite = _background_Sprites[change];

        GameObject[] buttonsToChange = GameObject.FindGameObjectsWithTag("button");
        foreach (GameObject obj in buttonsToChange)
        {
            obj.GetComponent<Image>().sprite = _button_Sprites[change];
        }
    }

    public void LoadData(GameData data)
    {
        //PlayerName = data.PlayerName;
        //transform.Find("nameField").GetComponent<TMP_InputField>().text = PlayerName;
    }

    public GameData SaveData(GameData data)
    {
        //data.PlayerName = PlayerName;

        return data;
    }

    public void ReadStringInput(string s)
    {
        _fieldString = s;

        if (!int.TryParse(s, out int example)) return;
        _fieldInt = System.Convert.ToInt32(s);
    }

    public void updateField(string field)
    {
        switch(field)
        {
            case "PlayerName":
                PlayerName = _fieldString;
                break;
        }
    }
}
