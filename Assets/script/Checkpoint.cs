using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform Position;

    void Start()
    {
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!enabled)
            return;
        Player player = collider.transform.GetComponent<Player>();
        if (player && player.Checkpoint != Position) {
            player.Checkpoint = Position;
            Debug.LogFormat("{0}: checkpoint set", transform.parent.name);
        }
    }

    void Update()
    {
    }
}
