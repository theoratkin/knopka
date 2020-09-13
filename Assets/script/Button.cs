using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public bool Reusable = false;

    private bool animationIsPlaying = false;

    public delegate void OnButtonPress();

    public event OnButtonPress OnButtonPressEvent;

    private Animator animator;

    private new AudioSource audio;

    void Start()
    {
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    public void SetActive(bool state)
    {
        GetComponent<Collider>().enabled = state;
    }

    public void Press()
    {
        if (animationIsPlaying)
            return;
        audio.Play();
        audio.time = .3f;
        animationIsPlaying = true;
        if (!Reusable) {
            animator.Play("press");
            SetActive(false);
        }
        else {
            animator.Play("press_release");
        }
        
        OnButtonPressEvent?.Invoke();
    }

    public void OnPressAnimEnd()
    {
        animationIsPlaying = false;
    }
}
