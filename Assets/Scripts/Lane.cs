using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class Chart
{
    public string name { get; private set; }

    public string audioPath { get; private set; }

    string[] lines;
    string path;
    string parentPath;

    bool playing = false;

    public int fullCombo { get; private set; } = 0;

    Queue<NoteStructure> notesQueue = new Queue<NoteStructure>();
    List<NoteStructure> abstractNotesQueue = new List<NoteStructure>();

    private int noteScrollPointer = 0;

    // --------------------------------------------------------------

    private static readonly Regex noteRegex = new(@"^\d+,\d+,(\d+),(\d+),(\d+),", RegexOptions.Compiled);
    private static readonly Regex trackRegex = new(@"^AudioFilename: (.+)", RegexOptions.Compiled);

    // --------------------------------------------------------------

    public Chart(string name, string fileName)
    {
        this.name = name;
        parentPath = Path.Combine(Application.dataPath, "Maps", name);
        path = Path.Combine(parentPath, fileName+".osu");

        if (!File.Exists(path))
        {
            Debug.LogError("You gave me a file that doesnt exist i hate u i hate u: " + path);
        }
    }

    public void Play()
    {
        playing = true;
    }

    public bool RenderNotes(int currentMS, out NoteStructure note)
    {
        Debug.Log("request");
        note = default;
        if (!playing) return false;

        if (notesQueue.TryPeek(out NoteStructure incoming))
        {
            if (incoming.targetMS - Lane.MSInbetween - 200 <= currentMS)
            {
                note = incoming;
                notesQueue.Dequeue();
                return true;
            }
        }
        else
        {
            playing = false;
        }

        return false;
    }

    public void GetClosest(NoteStructure input)
    {
        for (; noteScrollPointer < fullCombo; noteScrollPointer++)
        {
            NoteStructure closestNote = abstractNotesQueue[noteScrollPointer];
            int distance = Math.Abs(input.targetMS - closestNote.targetMS);
            switch (distance)
            {
                case <= Judgement.RyoWindowMS:
                    Debug.Log("RYO");
                    break;
                case <= Judgement.KaWindowMS:
                    Debug.Log("KA");
                    break;
                default:
                    Debug.Log("FUKA");
                    break;
            }
        }
    }

    public void Parse()
    {
        string Header = null;

        lines = File.ReadAllLines(path);
        foreach (string untrimmed in lines)
        {
            string line = untrimmed.Trim();
            if (line == "[General]" || line == "[HitObjects]" || string.IsNullOrWhiteSpace(line))
            {
                Header = line;
                continue;
            }

            switch (Header)
            {
                case "[General]":
                    Match trackMatch = trackRegex.Match(line);
                    if (trackMatch.Success)
                    {
                        audioPath = "MapAudio/" + name + "/" + trackMatch.Groups[1].Value.Replace(".mp3", "");

                        Header = null;
                    }
                    break;
                case "[HitObjects]":
                    Match objMatch = noteRegex.Match(line);
                    if (objMatch.Success)
                    {
                        int targetMS = int.Parse(objMatch.Groups[1].Value);
                        Note.Size noteSize = int.Parse(objMatch.Groups[2].Value) == 5 ? Note.Size.Large : Note.Size.Small;
                        Note.Type noteType = int.Parse(objMatch.Groups[3].Value) == 8 ? Note.Type.Katsu : Note.Type.Don;

                        fullCombo++;
                        notesQueue.Enqueue(new NoteStructure(targetMS, noteType, noteSize));
                        abstractNotesQueue.Add(new NoteStructure(targetMS, noteType, noteSize));
                    }
                    break;
            }
        }
    }
}

public class Lane : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    public static AudioSource musicAudioSource;

    public static float currentScrollSpeed { get; private set; } = 15f;
    public static float spawnX { get; private set; } = 16;
    public static float judgementX { get; private set; } = -7.9552f;
    public static float distanceX { get; private set; } = spawnX - judgementX;
    public static int MSInbetween { get; private set; } = (int)Math.Round(distanceX / currentScrollSpeed * 1000);

    private static ObjectPool<GameObject> NotePool;

    public static Chart chart = new Chart("LagTrain", "exnoiz - No Lag Train (2Gbps) (woosungko) [Futsuu]");

    // --------------------------------------------------------------
        
    float dt = 0;
    bool playing = false;
    int sortingOrder;

    // --------------------------------------------------------------

    private void Awake()
    {
        musicAudioSource = GetComponent<AudioSource>();
    }

    public static GameObject GetNote()
    {
        return NotePool.Get();
    }

    public static void ReleaseNote(GameObject note)
    {
        NotePool.Release(note);
    }

    public static int GetCurrentMS()
    {
        return !musicAudioSource.isPlaying ? (int)Math.Round(Time.time*1000) : (int)Math.Round(((double)musicAudioSource.timeSamples / musicAudioSource.clip.frequency) * 1000);
    }

    public static void TryScore()
    {
        
    }

    void Start()
    {
        NotePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(notePrefab, Camera.main.transform),
            actionOnGet: (note) => note.SetActive(true),
            actionOnRelease: (note) => note.SetActive(false),
            actionOnDestroy: (note) => Destroy(note),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 10
        );

        chart.Parse();
        sortingOrder = chart.fullCombo;
        Debug.Log(chart.audioPath);
    }

    void Update()
    {
        dt += Time.deltaTime;
        if (dt < 3) return;

        if (!playing)
        {
            playing = true;
            AudioClip song = Resources.Load<AudioClip>(chart.audioPath);

            musicAudioSource.clip = song;
            musicAudioSource.Play();
            chart.Play();
        }

        if (chart.RenderNotes(GetCurrentMS(), out NoteStructure note))
        {
            Debug.Log("RENDER");
            GameObject noteObj = GetNote();
            Note noteScript = noteObj.GetComponent<Note>();

            noteScript.Initialize(note.type, note.size, note.targetMS, currentScrollSpeed, sortingOrder);
            sortingOrder--;
        }
    }
}