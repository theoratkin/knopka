using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    private const float PopDistance = 20f;

    public delegate void OnButtonPress();

    public event OnButtonPress OnButtonPressEvent;

    public Color HighlightColor;
    public Color PressColor;
    public Color SelectColor;


    AudioSource clickSound;

    public bool PopOut = false;

    Color normalColor;

    new BoxCollider2D collider;
    RectTransform rectt;
    Image image;

    Vector2 normalSize;


    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        rectt = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        collider.size = rectt.rect.size;
        collider.offset = rectt.rect.center;

        normalColor = image.color;

        normalSize = rectt.sizeDelta;

        clickSound = GameObject.Find("ui_click").GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (rectt)
            rectt.sizeDelta = normalSize;
    }

    void OnMouseEnter()
    {
        image.color = HighlightColor;
        Vector2 size = rectt.sizeDelta;
        if (!PopOut)
            size.x = size.x - PopDistance;
        else
            size.x = size.x + PopDistance;
        rectt.sizeDelta = size;
    }

    void OnMouseExit()
    {
        image.color = normalColor;
        rectt.sizeDelta = normalSize;
    }

    void OnMouseDown()
    {
        image.color = PressColor;
    }

    void OnMouseUp()
    {
        image.color = HighlightColor;
    }

    void OnMouseUpAsButton()
    {
        clickSound.Play();
        OnButtonPressEvent?.Invoke();
    }
}
