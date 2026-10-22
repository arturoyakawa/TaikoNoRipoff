using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Judgement : MonoBehaviour
{
    [Header("Note Sounds")]
    [SerializeField] private AudioClip donSound;
    [SerializeField] private AudioClip katsuSound;
    [SerializeField] private AudioClip largeDonSound;
    [SerializeField] private bool AutoPlay = true;

    private AudioSource audioSource;

    private static float judgementX;

    static private float ImperfectVolume = .3f;
    static private float PerfectVolume = .3f;

    private static NoteBuffer noteBuffer = new NoteBuffer();

    public enum Score
    {
        Ryo,
        Ka,
        Fuka
    }

    // ------------------------------

    public const int RyoWindowMS = 42;
    public const int KaWindowMS = 108;
    public const int FukaWindowMS = 180;

    // ------------------------------

    private void CastLarge(NoteStructure note)
    {
        if (note.IsKatsu())
        {
            Debug.Log("KKKK");
            audioSource.PlayOneShot(katsuSound);
        }
        else
        {
            Debug.Log("DDDD");
            audioSource.PlayOneShot(largeDonSound);
        }
    }

    private void CastRegular(NoteStructure note)
    {
        if (note.IsKatsu())
        {
            Debug.Log("k");
            audioSource.PlayOneShot(katsuSound);
        }
        else
        {
            Debug.Log("d");
            audioSource.PlayOneShot(donSound); 
        }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        judgementX = transform.position.x;
    }

    void HandleNoteStructure(NoteStructure note)
    {
        Debug.Log(note.IsKatsu());
        if (note.IsLarge())
            CastLarge(note);
        else
            CastRegular(note);

        if (Note.GetMatching(note, FukaWindowMS, Lane.GetCurrentMS(), out Note result))
        {
            result.Hit();
        }
    }

    void Update()
    {
        for (int i = 0; i<4; i++)
        {
            if (noteBuffer.CheckCell(i, out NoteStructure result))
                HandleNoteStructure(result);
        }

        int currentMS = Lane.GetCurrentMS();

        if (AutoPlay)
        {
            if (Note.GetClosest(RyoWindowMS/3, currentMS, out Note result))
            {
                result.Hit();
                HandleNoteStructure(new NoteStructure(currentMS, result.type, result.size));
            }
        }

        if (Keyboard.current == null) return;

        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
           if (noteBuffer.Register(0, currentMS, out NoteStructure last))
                HandleNoteStructure(last);
        }

        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            if (noteBuffer.Register(1, currentMS, out NoteStructure last))
                HandleNoteStructure(last);
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (noteBuffer.Register(2, currentMS, out NoteStructure last))
                HandleNoteStructure(last);
        }

        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (noteBuffer.Register(3, currentMS, out NoteStructure last))
                HandleNoteStructure(last);
        }
    }
}