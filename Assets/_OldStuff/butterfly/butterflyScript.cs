using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class butterflyScript : MonoBehaviour
{

    private Animator butterfly_Animator;

    private bool bFacingSide;

    [SerializeField] private float speed = 1;
    private float x;
    private float y;
    private Vector2 NextPos;

    void Start()
    {
        butterfly_Animator = gameObject.GetComponent<Animator>();
        butterfly_Animator.speed = speed;
        StartCoroutine(moveAround());
    }

    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, NextPos, speed * Time.deltaTime);
    }

    public void flyAway()
    {
        bFacingSide = true;
        butterfly_Animator.SetTrigger("facingSide");
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.tag == "killBox") Destroy(this.gameObject); } // Kill Box

    private IEnumerator moveAround()
    {
        butterfly_Animator.ResetTrigger("facingSide");

        x = transform.position.x + Random.Range((speed - 3), (speed + 3));
        y = transform.position.y + Random.Range((speed - 1), (speed + 6));

        if(bFacingSide) x += Random.Range((speed + 3), (speed + 6)); 

        NextPos = new Vector2(x, y);

        if (transform.position.y >= 10 && bFacingSide == false) flyAway();

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(moveAround());
    }
}
