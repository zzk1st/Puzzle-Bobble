using UnityEngine;
using System.Collections;

public class Butterfly : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pointA, 0.1f);
        Gizmos.DrawWireSphere(pointB, 0.1f);
    }

    void Start()
    {
        transform.position = pointA;
        OnMoveToNextPoint();
    }

    void OnMoveToNextPoint()
    {
        Vector3 nextPos;
        if (Vector3.Distance(transform.position, pointA) < 0.1f)
        {
            nextPos = pointB;
        }
        else
        {
            nextPos = pointA;
        }

        transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);

        Hashtable args = new Hashtable();
        args.Add("easeType", iTween.EaseType.easeInOutQuad);
        args.Add("position", nextPos);
        args.Add("speed", 1f);
        args.Add("onComplete", "OnMoveToNextPoint");
        iTween.MoveTo(gameObject, args);
    }
}
