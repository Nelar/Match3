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
            bool hasFalling = false;
            Entities.WithAll<Falling, Cell>().ForEach((Entity e, ref Cell cell, ref Translation translation) => {
                hasFalling = true;
                if (gameState.gravity == Gravity.Down)
                {
                    if (translation.Value.y > cell.row)
                    {
                        translation.Value.y -= Setting.Instance.speed * Time.DeltaTime;
                    }
                    else
                    {
                        translation.Value.y = cell.row;
                        em.RemoveComponent<Falling>(e);
                    }
                }
                else
                {
                    if (translation.Value.y < cell.row)
                    {
                        translation.Value.y += Setting.Instance.speed * Time.DeltaTime;
                    }
                    else
                    {
                        translation.Value.y = cell.row;
                        em.RemoveComponent<Falling>(e);
                    }
                }                
            });

            if (!hasFalling) gameState.state = State.LoookingForMatches;
            SetSingleton<GameState>(gameState);
        }            
    }
}