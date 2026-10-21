using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Judgement : MonoBehaviour
{
    struct NoteInput
    {
        public enum Side
        {
            Left,
            Right
        }
        public Side side;
        public Note.Type type;
        public float timestamp;
        public NoteInput(Side side, Note.Type type, float timestamp)
        {
            this.side = side;
            this.type = type;
            this.timestamp = timestamp;
        }
    }


    [Header("Note Sounds")]
    [SerializeField] private AudioClip donSound;
    [SerializeField] private AudioClip katsuSound;
    [SerializeField] private AudioClip largeDonSound;

    private AudioSource audioSource;
    List<GameObject> noteQueue = new List<GameObject>();
    private NoteInput?[] inputsBuffer = new NoteInput?[2];
    private int freeSlolt = 0;
    private float largeNoteWindow = .03f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject note = collision.gameObject;
        if (!note.CompareTag("Note")) return;
        noteQueue.Add(note);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject note = collision.gameObject;
        if (!note.CompareTag("Note")) return;
        noteQueue.Remove(note);
    }

    void CastSingle(Note.Type type)
    {
        if (type == Note.Type.Katsu)
        {
            audioSource.PlayOneShot(katsuSound, .3f);
        }
        else
        {
            audioSource.PlayOneShot(donSound, .3f);
        }
    }
    void CastLarge(Note.Type type)
    {
        if (type == Note.Type.Katsu)
        {
            audioSource.PlayOneShot(katsuSound);
        }
        else
        {
            audioSource.PlayOneShot(largeDonSound);
        }
    }

    void AddToBuffer(NoteInput input)
    {
        if (inputsBuffer[freeSlolt] != null)
        {
            ProcessBufferElement(freeSlolt);
        }
        inputsBuffer[freeSlolt] = input;
        freeSlolt = 1 - freeSlolt;
    }
   
    void RemoveBufferElement(int index)
    {
        inputsBuffer[index] = null;
    }

    void ProcessBufferElement(int index)
    {
        if (inputsBuffer[index] == null) return;
        NoteInput input = (NoteInput)inputsBuffer[index];
        if (Time.time - input.timestamp <= largeNoteWindow)
        {
            int otherIndex = 1 - index;
            if (inputsBuffer[otherIndex] == null) return;
            NoteInput otherInput = (NoteInput)inputsBuffer[otherIndex];
            if (otherInput.type == input.type && otherInput.side != input.side)
            {
                RemoveBufferElement(otherIndex);
                RemoveBufferElement(index);
                CastLarge(input.type);
            }
            else
            {
                CastSingle(input.type);
            }
        }
        else
        {
            RemoveBufferElement(index);
            CastSingle(input.type);
        }
    }

    void Update()
    {
        ProcessBufferElement(0);
        ProcessBufferElement(1);

        if (Keyboard.current == null) return;

        bool leftKatsu = Keyboard.current.zKey.wasPressedThisFrame; 
        bool rightKatsu = Keyboard.current.vKey.wasPressedThisFrame;

        bool leftDon = Keyboard.current.xKey.wasPressedThisFrame;
        bool rightDon = Keyboard.current.cKey.wasPressedThisFrame;

        if (leftKatsu)
        {
            AddToBuffer(new NoteInput(NoteInput.Side.Left, Note.Type.Katsu, Time.time));
        }
        if (rightKatsu)
        {
            AddToBuffer(new NoteInput(NoteInput.Side.Right, Note.Type.Katsu, Time.time));
        }
        if (leftDon)
        {
            AddToBuffer(new NoteInput(NoteInput.Side.Left, Note.Type.Don, Time.time));
        }
        if (rightDon)
        {
            AddToBuffer(new NoteInput(NoteInput.Side.Right, Note.Type.Don, Time.time));
        }
    }
}
