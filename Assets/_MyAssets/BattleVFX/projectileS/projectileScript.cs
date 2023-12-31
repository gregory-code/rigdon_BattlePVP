using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class projectileScript : MonoBehaviour
{
    [SerializeField] GameObject hitEffect;
    [SerializeField] float speed;
    [SerializeField] bool followOwner;
    [SerializeField] float lifeTime;
    [SerializeField] bool shouldDestroy;
    Transform target;
    Transform followTarget;

    public void Init(Transform target, Transform owner)
    {
        this.target = target;
        followTarget = target;
        transform.localPosition = Vector3.one;

        if (followOwner)
        {
            followTarget = owner;
        }

        if(shouldDestroy)
        {
            StartCoroutine(DestroyTimer());
        }
    }

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(this.gameObject);
    }

    private void LateUpdate()
    {
        if(followOwner)
        {
            transform.position = followTarget.position;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, followTarget.position, speed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 3)
        {
            GameObject hit = Instantiate(hitEffect, transform.position, transform. rotation);
            hit.transform.SetParent(null);
            StopAllCoroutines();
            Destroy(this.gameObject);
        }
    }
}
