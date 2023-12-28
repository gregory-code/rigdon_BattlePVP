using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class passivePopUp : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private float secondsActive = 4;

    private List<GameObject> passiveAllyList = new List<GameObject>();
    private List<GameObject> passiveEnemyList = new List<GameObject>();

    [SerializeField] Sprite friendBackdrop;

    public void createPopup(bool isFriendly, string monName, string passiveName, Sprite monIcon, Transform attachPoint)
    {
        GameObject popup = Instantiate(popUpPrefab, transform);
        popup.transform.localScale = new Vector3(1, 1, 1);
        popup.transform.localPosition = new Vector2(-450, 400);

        if (isFriendly)
            popup.transform.GetComponent<Image>().sprite = friendBackdrop;

        popup.transform.Find("nameTitle").GetComponent<TextMeshProUGUI>().text = monName;
        popup.transform.Find("abilityTitle").GetComponent<TextMeshProUGUI>().text = passiveName;
        popup.transform.Find("monImage").GetComponent<Image>().sprite = monIcon;

        passiveAllyList.Insert(0, popup);
        resetListeners();

        StartCoroutine(leaveAutomatically(popup));
    }

    private IEnumerator leaveAutomatically(GameObject popup)
    {
        yield return new WaitForSeconds(secondsActive);

        if (popup != null)
        {
            removeNotif(passiveAllyList.IndexOf(popup));
        }
    }

    public void removeNotif(int index)
    {
        Destroy(passiveAllyList[index]);
        passiveAllyList.RemoveAt(index);
        resetListeners();
    }

    private void resetListeners()
    {
        for (int i = 0; i < passiveAllyList.Count; i++)
        {
            passiveAllyList[i].GetComponent<Button>().onClick.RemoveAllListeners();
            int notifReset = i;
            passiveAllyList[i].GetComponent<Button>().onClick.AddListener(() => removeNotif(notifReset));
        }
    }

    private void Update()
    {
        for (int i = 0; i < passiveAllyList.Count; i++)
        {
            Vector2 notif = Vector2.Lerp(passiveAllyList[i].transform.localPosition, new Vector2(-450, (250 - 90 * passiveAllyList.IndexOf(passiveAllyList[i]))), 12 * Time.deltaTime);
            passiveAllyList[i].transform.localPosition = notif;
        }
    }
}
