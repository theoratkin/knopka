using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAttach : MonoBehaviour
{
    public GameObject player;

    private Transform oldParent;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player)
            return;
        oldParent = player.transform.parent;
        player.transform.parent = transform;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject != player)
            return;
        player.transform.parent = oldParent;
    }
}
