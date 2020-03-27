using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Match3
{
    public struct Empty : IComponentData
    {
        public int column;
        public int row;
    }
}
