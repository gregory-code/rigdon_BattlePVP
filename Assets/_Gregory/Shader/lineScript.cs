using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// UnityEngine.InputSystem;

public class lineScript : MonoBehaviour
{
    private LineRenderer lr;

    [SerializeField] private Vector2 startPoint;
    [SerializeField] private GameObject reticle;

    [SerializeField] private Material dottedMAT;

    private Vector3 previousScale;

    private bool bFocusEnemy;
    private Transform enemyTransform;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        enable(false, new Vector2(0, 0));
    }

    private void Start()
    {
        lr.material = dottedMAT;

        previousScale = reticle.transform.localScale;

        //lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
    }

    public void enable(bool state, Vector2 start)
    {
        lr.sortingOrder = -5;
        reticle.SetActive(state);
        lr.enabled = state;

        if (state)
        {
            StartCoroutine(showLineRender());
            startPoint = start;
            lr.SetPosition(0, startPoint);
            lr.SetPosition(1, reticle.transform.position);
        }
    }

    public void focusEnemy(bool state, Transform enemy)
    {
        bFocusEnemy = state;
        enemyTransform = enemy;
    }

    private IEnumerator showLineRender()
    {
        yield return new WaitForSeconds(0.1f);
        lr.sortingOrder = 0;
    }

    private void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Mouse0) && lr.enabled)
        {
            enable(false, new Vector2(0, 0));
        }*/

        if (lr.enabled == false)
            return;

        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, reticle.transform.position);


        Vector3 lerpScale = (bFocusEnemy) ? Vector3.Lerp(reticle.transform.localScale, (enemyTransform.localScale * 2), 6 * Time.deltaTime) 
                                          : Vector3.Lerp(reticle.transform.localScale, previousScale, 6 * Time.deltaTime);
        reticle.transform.localScale = lerpScale;

        if (bFocusEnemy == true)
        {
            Vector2 lerpPos = Vector2.Lerp(reticle.transform.position, enemyTransform.position, 6 * Time.deltaTime);
            reticle.transform.position = lerpPos;
            return;
        }

        Vector2 mouseScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        reticle.transform.position = mouseScreenPosition;
    }

}
