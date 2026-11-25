using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    [SerializeField] GameObject comboDisplay;

    private float ogTextHeight;

    Renderer ryoSprite;
    Renderer kaSprite;
    Renderer fukaSprite;

    ScriptAnimation ryoAnim = new ScriptAnimation(.2f);
    ScriptAnimation kaAnim = new ScriptAnimation(.2f);
    ScriptAnimation fukaAnim = new ScriptAnimation(.2f);

    TMP_Text comboText;

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

    private void ThrowRyo()
    {
        ryoAnim.Play();
        HideAllText();
        ryoText.SetActive(true);
        comboText.text = (int.Parse(comboText.text) + 1).ToString();
    }

    private void ThrowKa()
    {
        kaAnim.Play();
        HideAllText();
        kaText.SetActive(true);
        comboText.text = (int.Parse(comboText.text) + 1).ToString();
    }

    private void ThrowFuka()
    {
        fukaAnim.Play();
        HideAllText();
        fukaText.SetActive(true);
        comboText.text = "0";
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
                    ThrowRyo();
                    break;
                case <= KaWindowMS:
                    ThrowKa();
                    break;
                default:
                    ThrowFuka();
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

        comboText = comboDisplay.GetComponent<TMP_Text>();
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
            if (noteBuffer.CheckCell(i, out NoteStructure structureresult))
                HandleNoteStructure(structureresult);
        }

        int currentMS = Lane.GetCurrentMS();

        if (Note.GetClosest(FukaWindowMS, currentMS, out Note closest))
        {
            //result.Hit();
            if (closest.GetDistance(currentMS, true) <= -KaWindowMS)
            {
                ThrowFuka();
            }
           // HandleNoteStructure(new NoteStructure(currentMS, result.type, result.size));
        }

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