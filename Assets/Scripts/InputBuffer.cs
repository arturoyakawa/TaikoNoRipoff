using System;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public struct DrumInput
{
    public readonly int timestampMS;
    public bool valid { get; private set; }

    public DrumInput(int timestampMS)
    {
        this.timestampMS = timestampMS;
        valid = true;
    }

    public void Expire()
    {
        valid = false;
    }
}

public class NoteBuffer
{
    public const int largeNoteWindowMS = 20;
    private DrumInput[] buffer = new DrumInput[4];

    private NoteStructure Process(int index, bool isLarge)
    {
        DrumInput copy = buffer[index];
        buffer[index].Expire();
        if (isLarge) 
            buffer[3 - index].Expire();
        //Debug.Log(index > 0 && index < 3 ? Note.Type.Don : Note.Type.Katsu);

        NoteStructure result = new NoteStructure(copy.timestampMS, index > 0 && index < 3 ? Note.Type.Don : Note.Type.Katsu, isLarge ? Note.Size.Large : Note.Size.Small);

        return result;
    }

    public bool Register(int index, int timestampMS, out NoteStructure note)
    {
        note = default;
        if (buffer[index].valid)
        {
            note = Process(index, false);
            buffer[index] = new DrumInput(timestampMS);
            return true;
        }
        buffer[index] = new DrumInput(timestampMS);
        return false;
    }

    public bool CheckCell(int index, out NoteStructure note)
    {
        note = new NoteStructure();
        DrumInput input = buffer[index];

        // Importante: Garanta que tem "using System.Linq;" no topo do seu arquivo script!
        //Debug.Log("Validade do Buffer: " + string.Join(", ", buffer.Select(input => input.valid)));

        if (!input.valid) return false;

        int elapse = Lane.GetCurrentMS() - input.timestampMS;

        if (elapse > largeNoteWindowMS)
        {
            DrumInput otherInput = buffer[3 - index];
            bool isLarge = otherInput.valid;

            note = Process(index, isLarge);
            return true;
        }

        return false;
    }

    //public void Register(DrumInput input)
    //{
    //    buffer[freeSlot] = input;
    //    freeSlot++;
    //}

    //public void Consume(Action<NoteStructure> onRegistered)
    //{
    //    freeSlot = 0;
    //    int currentMS = Lane.GetCurrentMS();
    //    for (int i = 0; i < buffer.Length; i++)
    //    {
    //        DrumInput input = buffer[i];
    //        int elapse = currentMS - input.timestampMS;
    //        if (!input.valid || elapse < largeNoteWindowMS) continue;

    //        bool isLarge = false;
    //        for (int j = 0; j < buffer.Length; j++)
    //        {
    //            DrumInput otherInput = buffer[j];
    //            if (!otherInput.valid) continue;

    //            if (input.side != otherInput.side && input.type == otherInput.type)
    //            {
    //                isLarge = true;
    //                buffer[j].Expire();
    //            }
    //        }

    //        buffer[i].Expire();
    //        onRegistered.Invoke(new NoteStructure(input.timestampMS, input.type, isLarge ? Note.Size.Large : Note.Size.Small));
    //    }
    //}
}