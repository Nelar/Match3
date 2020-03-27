using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Match3
{
    public struct Cell : IComponentData
    {
        public Color color;
        public Type type;
        public int column;
        public int row;
    }
}
