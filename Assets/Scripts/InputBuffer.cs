using UnityEngine;

public struct DrumInput
{
    public enum Side
    {
        Left,
        Right
    }

    public readonly Note.Type type;
    public readonly Side side;
    public readonly int timestampMS;
    public bool expired { get; private set; }

    public DrumInput(Note.Type type, Side side, int timestampMS)
    {
        this.type = type;
        this.timestampMS = timestampMS;
        this.side = side;
        this.expired = false;
    }

    public void Expire()
    {
        expired = true;
    }
}

public class DrummingBuffer
{
    public const int largeNoteWindowMS = 3;
    private DrumInput[] buffer = new DrumInput[4];
    private int freeSlot = 0;

    void Register(DrumInput input)
    {
        buffer[freeSlot] = input;
        freeSlot++;
    }

    NoteStructure[] Consume()
    {
        NoteStructure[] result = new NoteStructure[4];
        int k = 0;

        for (int i = 0; i < buffer.Length; i++)
        {
            DrumInput input = buffer[i];
            if (input.expired || Time.time - input.timestampMS < largeNoteWindowMS) continue;

            bool isLarge = false;
            for (int j = i; j<buffer.Length; j++)
            {
                DrumInput otherInput = buffer[j];
                if (input.expired) continue;

                if (input.side != otherInput.side && input.type == otherInput.type)
                {
                    isLarge = true;
                    otherInput.Expire();
                }
            }

            if (isLarge)
            {
                result[k] = new NoteStructure(input.timestampMS, input.type, isLarge ? Note.Size.Large : Note.Size.Small);
            }

            input.Expire();
        }

        return result;
    }
}