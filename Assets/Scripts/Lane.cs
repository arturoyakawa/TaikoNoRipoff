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

    string path;
    string parentPath;
    string audioPath;

    string[] lines;
    Queue<NoteStructure> notesQueue = new Queue<NoteStructure>();

    // --------------------------------------------------------------

    private static readonly Regex noteRegex = new(@"^\d+,\d+,(\d+),(\d+),(\d+),", RegexOptions.Compiled);
    private static readonly Regex trackRegex = new(@"^AudioFilename: (.+)", RegexOptions.Compiled);

    // --------------------------------------------------------------

    public Chart(string name, string fileName)
    {
        this.name = name;
        this.parentPath = Path.Combine(Application.dataPath, "Maps", name);
        this.path = Path.Combine(parentPath, fileName+".osu");

        if (!File.Exists(path))
        {
            Debug.LogError("You gave me a file that doesnt exist i hate u i hate u: " + path);
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
                        audioPath = Path.Combine(parentPath, trackMatch.Groups[1].Value);
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

                        notesQueue.Enqueue(new NoteStructure(targetMS, noteType, noteSize));
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

    private static Chart chart = new Chart("LagTrain", "exnoiz - No Lag Train (2Gbps) (woosungko) [Futsuu]");

    // --------------------------------------------------------------

    static private int FullCombo;
    static private int NoteID = 0;

    // --------------------------------------------------------------
        
    float dt = 0;
    bool playing = false;

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
    }

    void Update()
    {

    }
}