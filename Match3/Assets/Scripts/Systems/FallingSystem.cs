using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class FallingSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<GameState>();            
        }

        protected override void OnUpdate()
        {
            var gameState = GetSingleton<GameState>();
            if (gameState.state != State.Falling) return;
            var em = EntityManager;

            var empties = Entities.GetEmptyEntities();            
            foreach (var empty in empties)
            {                
            }
        }
    }
}