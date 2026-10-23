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

    [SerializeField] private Sprite donMouthImage;
    [SerializeField] private Sprite katsuMouthImage;

    [SerializeField] private Sprite greendonImage;
    [SerializeField] private Sprite voitsuImage;

    [SerializeField] private AudioSource musicAudioSource;

    // ANIMATION --------

    [SerializeField] private float hitAnimAmplitude;
    [SerializeField] private float hitAnimLength;
    [SerializeField] private float hitAnimDuration = 1;

    static float hitAmp;
    static float hitLen;
    static float hitDur;

    ScriptAnimation HitAnim;
    
    // ------------------

    public Type type;
    public Size size;

    private Transform trans;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private static List<Note> activeNotes = new List<Note>();

    private bool active = false;

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

    private void Erase()
    {
        trans.position = new Vector2(Lane.spawnX, Lane.judgementY);
        Lane.ReleaseNote(gameObject);
    }

    public void Hit()
    {
        Deactivate();
        HitAnim.Play();
       // Erase();
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

    public void Deactivate()
    {
        active = false;
        activeNotes.Remove(this);
    }

    public void Activate(Type newType, Size newSize, int newHitMS, float newScrollSpeed, int sortingOrder)
    {
        hitMS = newHitMS;
        type = newType;
        size = newSize;
        scrollSpeed = newScrollSpeed;
        appearenceMS = hitMS - Lane.MSInbetween;
        spriteRenderer.sortingOrder = sortingOrder;

        if (!activeNotes.Contains(this))
            activeNotes.Add(this);

        UpdateAppearence();

        active = true;
    }

    void Awake()
    {
        trans = gameObject.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        HitAnim = new ScriptAnimation(hitAnimDuration);
        HitAnim.OnStep += (float alpha) =>
        {
            float xOffset = hitAnimLength * alpha;
            float yOffset = (float)Math.Sin(alpha * 150 * Mathf.Deg2Rad) * hitAnimAmplitude;

            rb.MovePosition(new Vector2(Lane.judgementX + xOffset, Lane.judgementY + yOffset));
        };

        HitAnim.OnEnd += () =>
        {
            Erase();
        };
    }

    private void Update()
    {
        if (!active) 
        {
            HitAnim.Step();
            return; 
        }

        hitLen = hitAnimLength;
        hitAmp = hitAnimAmplitude;
        hitDur = hitAnimDuration;

        int currentMS = Lane.GetCurrentMS();
        float alpha = MathUtils.InverseLerpUnclamped(appearenceMS, hitMS, currentMS);
        float targetX = Mathf.LerpUnclamped(Lane.spawnX, Lane.judgementX, alpha);

        if (GetDistance(currentMS, true) <= -Judgement.FukaWindowMS - 200)
        {
            Deactivate();
            Erase();
        }

        rb.MovePosition(new Vector2(targetX, Lane.judgementY));
    }
}