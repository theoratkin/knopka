using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island : MonoBehaviour
{
    public Transform Checkpoint;

    void Start()
    {
    }

    void OnCollisionEnter(Collision hit)
    {
        if (!enabled)
            return;
        Player player = hit.transform.GetComponent<Player>();
        if (player && player.Checkpoint != Checkpoint) {
            player.Checkpoint = Checkpoint;
            Debug.LogFormat("{0}: checkpoint set", Checkpoint.parent.name);
        }
    }

    void Update()
    {
    }
}
