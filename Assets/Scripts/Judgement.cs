using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
    List<Note> noteQueue = new List<Note>();
    private NoteInput?[] inputsBuffer = new NoteInput?[2];
    private int freeSlolt = 0;
    private float largeNoteWindow = .03f;
    private static float judgementX;
    [SerializeField] private bool AutoPlay = true;

    static private float ImperfectVolume = .3f;
    static private float PerfectVolume = .3f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        judgementX = transform.position.x;
    }

    bool FetchFromQueueByType(Note.Type type, out Note result)
    {
        return (result = noteQueue.FirstOrDefault(n => n.type == type)) != null;
    }

    void Update()
    {
        
    }
}
