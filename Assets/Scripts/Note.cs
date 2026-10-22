using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public struct NoteStructure
{
    public int targetMS;
    readonly public Note.Type type;
    readonly public Note.Size size;

    public NoteStructure(int targetMS, Note.Type type, Note.Size size)
    {
        this.targetMS = targetMS;
        this.type = type;
        this.size = size;
    }

    public bool IsKatsu()
    {
        return (type == Note.Type.Katsu);
    }

    public bool IsLarge()
    {
        return (size == Note.Size.Large);
    }

    public bool Matches(NoteStructure otherNote)
    {
        return type == otherNote.type && size == otherNote.size;
    }
}

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

    private float scrollSpeed;
    public int hitMS { get; private set; }
    private int appearenceMS;

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
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private static List<Note> activeNotes = new List<Note>();

    float InverseLerpUnclamped(float min, float max, float valor)
    {
        if (Mathf.Approximately(min, max)) return 0f;

        return (valor - min) / (max - min);
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

    public void Activate(Type newType, Size newSize, int newHitMS, float newScrollSpeed, int sortingOrder)
    {
        hitMS = newHitMS;
        type = newType;
        size = newSize;
        scrollSpeed = newScrollSpeed;
        appearenceMS = hitMS - Lane.MSInbetween;
        spriteRenderer.sortingOrder = sortingOrder;
        trans.position = new Vector2(Lane.spawnX, trans.position.y);

        if (!activeNotes.Contains(this))
            activeNotes.Add(this);

        UpdateAppearence();
    }

    private void Erase()
    {
        activeNotes.Remove(this);
        Lane.ReleaseNote(gameObject);
    }

    public void Hit()
    {
        Erase();
    }

    public int GetDistance(int currentMS, bool isSigned = false)
    {
        if (isSigned)
            return hitMS - currentMS;
        else
            return Math.Abs(currentMS - hitMS);
    }

    public static bool GetClosest(int timeoutMS, int currentMS, out Note result)
    {
        result = default;
        bool found = false;
        int lessTime = int.MaxValue;

        foreach (Note note in activeNotes)
        {
            int dist = Math.Abs(currentMS - note.hitMS);
            if (dist > timeoutMS)
                continue;

            if (note.hitMS < lessTime)
            {
                lessTime = note.hitMS;
                result = note;
                found = true;
            }
        }

        return found;
    }

    public static bool GetMatching(NoteStructure input, int timeoutMS, int currentMS, out Note result)
    {
        result = default;
        int lessTime = int.MaxValue;
        bool found = false;
        foreach (Note note in activeNotes)
        {
            int dist = note.GetDistance(currentMS);
            if (dist > timeoutMS)
                continue;

            if (note.hitMS < lessTime && note.type == input.type && note.size == input.size)
            {
                found = true;
                result = note;
                lessTime = note.hitMS;
            }
        }

        return found;
    }

    void Awake()
    {
        trans = gameObject.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        int currentMS = Lane.GetCurrentMS();
        float alpha = InverseLerpUnclamped(appearenceMS, hitMS, currentMS);
        float targetX = Mathf.LerpUnclamped(Lane.spawnX, Lane.judgementX, alpha);

        if (GetDistance(currentMS, true) <= -Judgement.FukaWindowMS - 200)
        {
            Erase();
        }

        rb.MovePosition(new Vector2(targetX, transform.position.y));
    }
}