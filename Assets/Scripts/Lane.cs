using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
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

    public bool RenderNote(int currentMS, out NoteStructure note)
    {
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

    public NoteStructure GetFirst()
    {
        return new NoteStructure();
    }

    public void Parse(int offsetMS)
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
                        notesQueue.Enqueue(new NoteStructure(targetMS+offsetMS, noteType, noteSize));
                    }
                    break;
            }
        }
    }
}

public class Lane : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private int Offset;
    [SerializeField] private string ChartName;
    [SerializeField] private bool PlayChartAutomatically = true;

    public static AudioSource musicAudioSource;

    public static float currentScrollSpeed { get; private set; } = 15f;
    public static float spawnX { get; private set; } = 16;
    public static float judgementX { get; private set; } = -7.9552f;
    public static float distanceX { get; private set; } = spawnX - judgementX;
    public static int MSInbetween { get; private set; } = (int)Math.Round(distanceX / currentScrollSpeed * 1000);

    private static ObjectPool<GameObject> NotePool;

    public static Chart chart;

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
        if (!musicAudioSource.isPlaying)
            return (int)Math.Round(Time.time * 1000);

        return (int)Math.Round(
            (double)musicAudioSource.timeSamples
            / musicAudioSource.clip.frequency * 1000
        );
    }

    void Start()
    {
        chart = new Chart(ChartName, "chart");

        Transform child = transform.GetChild(0);
        judgementX = child.position.x;

        NotePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(notePrefab, Camera.main.transform),
            actionOnGet: (note) => note.SetActive(true),
            actionOnRelease: (note) => note.SetActive(false),
            actionOnDestroy: (note) => { if (note) Destroy(note); },
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 10
        );

        chart.Parse(Offset);
        sortingOrder = chart.fullCombo;
    }

    void Update()
    {
        dt += Time.deltaTime;
        if (dt < .7f || !PlayChartAutomatically) return;

        if (!playing)
        {
            playing = true;
            AudioClip song = Resources.Load<AudioClip>(chart.audioPath);

            musicAudioSource.clip = song;
            musicAudioSource.Play();
            chart.Play();
        }

        if (chart.RenderNote(GetCurrentMS(), out NoteStructure note))
        {
            GameObject noteObj = GetNote();
            Note noteScript = noteObj.GetComponent<Note>();

            noteScript.Activate(note.type, note.size, note.targetMS, currentScrollSpeed, sortingOrder);
            sortingOrder--;
        }
    }
}