using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Lane : MonoBehaviour
{
    struct ParsedNote
    {
        public float targetMS;
        public Note.Type type;
        public Note.Size size;
    }

    [SerializeField] private GameObject notePrefab;
    public static AudioSource musicAudioSource;

    public static float currentScrollSpeed { get; private set; } = 15f;
    public static float distanceX { get; private set; } = 21000;
    public static float judgementX { get; private set; } = -7.9552f;
    public static float MSInbetween { get; private set; } = distanceX / currentScrollSpeed * 1000; 

    float dt = 0;
    bool playing = false;

    private void Awake()
    {
        musicAudioSource = GetComponent<AudioSource>();
    }

    Queue<ParsedNote> ParseFile(string fileName)
    {
        Queue<ParsedNote> parseResult = new Queue<ParsedNote>();

        string path = Path.Combine(Application.dataPath, "Maps", fileName);

        if (!File.Exists(path))
        {
            Debug.LogError("You gave me a file that doesnt exist i hate u i hate u: " + path);
        }

        string[] lines = File.ReadAllLines(path);
        bool HitObjects = false;

        string regexPattern = @"^\d+,\d+,(\d+),(\d+),(\d+),";

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!HitObjects)
            {
                if (trimmed == "[HitObjects]")
                {
                    HitObjects = true;
                }
                continue;
            }

            Match match = Regex.Match(trimmed,regexPattern);
            if (match.Success)
            {

            }
        }

        return parseResult;
    }

    

    void Start()
    {

    }
   
    void Update()
    {
        dt += Time.deltaTime;
        if (dt < 2) return;

        if (!playing)
        {
            musicAudioSource.Play();
            playing = true;
        }
    }
}
