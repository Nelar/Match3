using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

namespace Match3
{    
    public enum Type
    {
        Simple,
        Gravity
    }
    public enum Color
    {
        Red,
        Yellow,
        Green,
        Purple,
        Blue,
        Orange,
        None
    }
    public enum State
    {
        Idle,
        Swap,
        LoookingForMatches,
        Filling,
        Falling,
    }
    public enum Gravity
    {
        Up,
        Down
    }
}
