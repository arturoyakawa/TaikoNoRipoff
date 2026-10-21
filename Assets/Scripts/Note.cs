using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Note : MonoBehaviour
{
    public enum Type
    {
        Don,
        Katsu,
        Greendon,
        Voitsu,
    }
    public enum Size
    {
        Large,
        Small
    }

    [Header("Note Settings")]
    private float scrollSpeed;
    private static float regularNoteSize = 1.2875f;
    private static float largeNoteSizeFactor = 2f;

    [Header("Note Sprites")]
    [SerializeField] private Sprite donImage;
    [SerializeField] private Sprite katsuImage;
    [SerializeField] private Sprite greendonImage;
    [SerializeField] private Sprite voitsuImage;
    [SerializeField] private AudioSource musicAudioSource;

    public Type type;
    public Size size;

    private Transform trans;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Awake()
    {
        trans = gameObject.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void UpdateAppearence()
    {
        switch (type)
        {
            case Type.Katsu:
                spriteRenderer.sprite = katsuImage;
                break;
            case Type.Don:
                spriteRenderer.sprite = donImage;
                break;
            case Type.Greendon:
                spriteRenderer.sprite = greendonImage;
                break;
            case Type.Voitsu:
                spriteRenderer.sprite = voitsuImage;
                break;
        }

        trans.localScale = Vector3.one * regularNoteSize * (size == Size.Large ? largeNoteSizeFactor : 1f);
    }

    public void ChangeScrollSpeed(float newScrollSpeed)
    {
        scrollSpeed = newScrollSpeed;
    }

    public void ChangeAppearence(Type newType, Size newSize)
    {
        type = newType;
        size = newSize;
        UpdateAppearence();
    } 

    private void Update()
    {
        float currentMS = Lane.musicAudioSource.time*1000;
        float hitMS = 5000f;
        float appearenceMS = hitMS - Lane.MSInbetween;

        float alpha = (currentMS - appearenceMS) / (hitMS - appearenceMS);
        
        rb.MovePosition(new Vector2(Mathf.LerpUnclamped(Lane.distanceX, Lane.judgementX, alpha), transform.position.y));
    }
}
