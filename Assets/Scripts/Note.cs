using UnityEngine;

public struct NoteStructure
{
    public int targetMS;
    readonly public Note.Type type;
    readonly public Note.Size size;

    public NoteStructure(int targetMS, Note.Type type, Note.Size size)
    {
        this.targetMS = targetMS;
        this.type = type;
        this.size = size;
    }

    public bool IsKatsu()
    {
        return (type == Note.Type.Katsu);
    }

    public bool IsLarge()
    {
        return (size == Note.Size.Large);
    }
}

public class Note : MonoBehaviour
{
    public enum Type
    {
        Don,
        Katsu,
        Greendon,
        Voitsu,
    }

    public enum Size
    {
        Large,
        Small
    }

    private float scrollSpeed;
    private int hitMS;
    private int appearenceMS;

    private static float regularNoteSize = 1.2875f;
    private static float largeNoteSizeFactor = 2f;

    [Header("Note Sprites")]
    [SerializeField] private Sprite donImage;
    [SerializeField] private Sprite katsuImage;
    [SerializeField] private Sprite greendonImage;
    [SerializeField] private Sprite voitsuImage;
    [SerializeField] private AudioSource musicAudioSource;

    public Type type;
    public Size size;

    private Transform trans;
    private SpriteRenderer spriteRenderer;

    float InverseLerpUnclamped(float min, float max, float valor)
    {
        if (Mathf.Approximately(min, max)) return 0f;

        return (valor - min) / (max - min);
    }

    void Awake()
    {
        trans = gameObject.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void UpdateAppearence()
    {
        switch (type)
        {
            case Type.Katsu:
                spriteRenderer.sprite = katsuImage;
                break;
            case Type.Don:
                spriteRenderer.sprite = donImage;
                break;
            case Type.Greendon:
                spriteRenderer.sprite = greendonImage;
                break;
            case Type.Voitsu:
                spriteRenderer.sprite = voitsuImage;
                break;
        }

        trans.localScale = Vector3.one * regularNoteSize * (size == Size.Large ? largeNoteSizeFactor : 1f);
    }

    public void Hit()
    {
        Lane.ReleaseNote(gameObject);
    }

    public void Initialize(Type newType, Size newSize, int newHitMS, float newScrollSpeed, int sortingOrder)
    {
        hitMS = newHitMS;
        type = newType;
        size = newSize;
        scrollSpeed = newScrollSpeed;
        appearenceMS = hitMS - Lane.MSInbetween;
        spriteRenderer.sortingOrder = sortingOrder;
        UpdateAppearence();
    }

    private void Update()
    {
        float alpha = InverseLerpUnclamped(appearenceMS, hitMS, Lane.GetCurrentMS());
        float targetX = Mathf.LerpUnclamped(Lane.spawnX, Lane.judgementX, alpha);

        if (alpha > 1.5f)
        {
            Lane.ReleaseNote(gameObject);
        }

        transform.position = new Vector2(targetX, transform.position.y);
    }
}