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
            audioSource.PlayOneShot(donSound);
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
            audioSource.PlayOneShot(largeDonSound); 
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
    }

    void Update()
    {
        for (int i = 0; i<4; i++)
        {
            if (noteBuffer.CheckCell(i, out NoteStructure result))
                HandleNoteStructure(result);
        }

        if (Keyboard.current == null) return;

        int currentMS = Lane.GetCurrentMS();

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
