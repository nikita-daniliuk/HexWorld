using System;
using UnityEngine;

public class Message
{
    public readonly Enum Signal;
    public readonly GameObject Sender;
    public readonly object Data;

    public Message(Enum Signal, GameObject Sender, object Data)
    {
        this.Signal = Signal;
        this.Sender = Sender;
        this.Data = Data;
    }
}