using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class projectileScript : MonoBehaviour
{
    [SerializeField] GameObject hitEffect;
    [SerializeField] float speed;
    [SerializeField] Transform target;

    public void Init(Transform target)
    {
        this.target = target;
        transform.localPosition = Vector3.one;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 3)
        {
            GameObject hit = Instantiate(hitEffect, transform.position, transform. rotation);
            hit.transform.SetParent(null);
            Destroy(this.gameObject);
        }
    }
}
