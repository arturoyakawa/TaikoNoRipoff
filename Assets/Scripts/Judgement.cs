using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Judgement : MonoBehaviour
{
    [Header("Note Sounds")]
    [SerializeField] private AudioClip donSound;
    [SerializeField] private AudioClip katsuSound;
    [SerializeField] private AudioClip largeDonSound;

    [Header("Note Judgement Text")]
    [SerializeField] GameObject ryoText;
    [SerializeField] GameObject kaText;
    [SerializeField] GameObject fukaText;

    private float ogTextHeight;

    Renderer ryoSprite;
    Renderer kaSprite;
    Renderer fukaSprite;

    ScriptAnimation ryoAnim = new ScriptAnimation(.2f);
    ScriptAnimation kaAnim = new ScriptAnimation(.2f);
    ScriptAnimation fukaAnim = new ScriptAnimation(.2f);

    //[SerializeField]

    [SerializeField] private bool AutoPlay = true;

    private AudioSource audioSource;

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
            audioSource.PlayOneShot(katsuSound);
        }
        else
        {
            audioSource.PlayOneShot(largeDonSound);
        }
    }

    private void CastRegular(NoteStructure note)
    {
        if (note.IsKatsu())
        {
            audioSource.PlayOneShot(katsuSound);
        }
        else
        {
            audioSource.PlayOneShot(donSound); 
        }
    }

    private void HideAllText()
    {
        ryoText.SetActive(false);
        kaText.SetActive(false);
        fukaText.SetActive(false);
    }

    void HandleNoteStructure(NoteStructure note)
    {
        int currentMS = Lane.GetCurrentMS();

        if (note.IsLarge())
            CastLarge(note);
        else
            CastRegular(note);

        if (Note.GetMatching(note, FukaWindowMS, currentMS, out Note result))
        {
            switch (result.GetDistance(currentMS))
            {
                case <= RyoWindowMS:
                    Debug.Log("Ryo!");
                    ryoAnim.Play();
                    HideAllText();
                    ryoText.SetActive(true);
                    break;
                case <= KaWindowMS:
                    Debug.Log("Ok.");
                    kaAnim.Play();
                    HideAllText();
                    kaText.SetActive(true);
                    break;
                default:
                    Debug.Log("Fuka...");
                    fukaAnim.Play();
                    HideAllText();
                    fukaText.SetActive(true);
                    break;
            }
            

            result.Hit();
        }
    }


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        ryoSprite = ryoText.GetComponent<MeshRenderer>();
        fukaSprite = fukaText.GetComponent<MeshRenderer>();
        kaSprite = kaText.GetComponent<MeshRenderer>();
    }

    float judgementUpwardsOffset = .4f;
    float maxAlpha = .4f;

    void Start()
    {
        ogTextHeight = ryoText.transform.position.y;
        ryoAnim.OnStep += (float alpha) =>
        {
            float y = Mathf.Lerp(ogTextHeight, ogTextHeight + judgementUpwardsOffset, Math.Clamp(alpha, 0f, maxAlpha));
            ryoText.transform.position = new Vector2(ryoText.transform.position.x,y);
            Debug.Log(y);
        };
        ryoAnim.OnEnd += () =>
        {
            ryoText.SetActive(false);
        };

        kaAnim.OnStep += (float alpha) =>
        {
            float y = Mathf.Lerp(ogTextHeight, ogTextHeight + judgementUpwardsOffset, Math.Clamp(alpha, 0f, maxAlpha));
            kaText.transform.position = new Vector2(kaText.transform.position.x, y);
        };
        kaAnim.OnEnd += () =>
        {
            kaText.SetActive(false);
        };

        fukaAnim.OnStep += (float alpha) =>
        {
            float y = Mathf.Lerp(ogTextHeight, ogTextHeight + judgementUpwardsOffset, Math.Clamp(alpha,0f,maxAlpha));
            fukaText.transform.position = new Vector2(fukaText.transform.position.x, y);
        };
        fukaAnim.OnEnd += () =>
        {
            fukaText.SetActive(false);
        };
    }

    void Update()
    {
        ryoAnim.Step();
        fukaAnim.Step();
        kaAnim.Step();
        
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
                //result.Hit();
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