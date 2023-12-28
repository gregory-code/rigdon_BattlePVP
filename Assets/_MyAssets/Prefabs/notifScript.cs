using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class notifScript : MonoBehaviour
{
    private List<GameObject> notifList = new List<GameObject>();

    [Header("Prefab")]
    [SerializeField] private GameObject notifPrefab;

    [Header("Timer Destroy")]
    [SerializeField] private bool bLeaveAutomatically = true;
    [SerializeField] private float secondsToLeave = 5;

    public void createNotif(string text, Color color)
    {
        GameObject notif = Instantiate(notifPrefab);
        notif.transform.SetParent(transform);
        notif.transform.localScale = new Vector3(1, 1, 1);
        notif.transform.localPosition = new Vector2(-450, 400);

        notif.transform.Find("notifText").GetComponent<TextMeshProUGUI>().text = text;
        notif.transform.Find("notifText").GetComponent<TextMeshProUGUI>().color = color;

        notifList.Insert(0, notif);

        resetListeners();

        if(bLeaveAutomatically) 
            StartCoroutine(leaveAutomatically(notif));
    }

    public void removeNotif(int index)
    {
        Destroy(notifList[index]);
        notifList.RemoveAt(index);
        resetListeners();
    }

    private void resetListeners()
    {
        for (int i = 0; i < notifList.Count; i++)
        {
            notifList[i].GetComponent<Button>().onClick.RemoveAllListeners();
            int notifReset = i;
            notifList[i].GetComponent<Button>().onClick.AddListener(() => removeNotif(notifReset));
        }
    }

    private IEnumerator leaveAutomatically(GameObject notif)
    {
        yield return new WaitForSeconds(secondsToLeave);

        if (notif != null)
        {
            removeNotif(notifList.IndexOf(notif));
        }
    }

    private void Update()
    {
        for (int i = 0; i < notifList.Count; i++)
        {
            Vector2 notif = Vector2.Lerp(notifList[i].transform.localPosition, new Vector2(-450, (250 - 90 * notifList.IndexOf(notifList[i]))), 12 * Time.deltaTime);
            notifList[i].transform.localPosition = notif;
        }
    }
}
