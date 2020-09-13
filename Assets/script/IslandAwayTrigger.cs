using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandAwayTrigger : MonoBehaviour
{
    public delegate void OnTrigger();

    public event OnTrigger OnTriggerEvent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        //Destroy(gameObject);
        OnTriggerEvent?.Invoke();
    }
}
