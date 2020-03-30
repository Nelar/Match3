using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Match3
{
    public struct GameState : IComponentData
    {
        public State state;
        public Gravity gravity;
        public bool needChangeGravity;
    }
}
