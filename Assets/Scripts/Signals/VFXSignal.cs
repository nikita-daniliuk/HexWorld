using UnityEngine;
using System;

public class VFXSignal
{
    public Type VFXType;
    public Vector2 Position;

    public VFXSignal(Type VFXType, Vector2 Position)
    {
        this.VFXType = VFXType;
        this.Position = Position;
    }
}