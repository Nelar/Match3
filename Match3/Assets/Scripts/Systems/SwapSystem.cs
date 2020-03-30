using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class SwapSystem : ComponentSystem
    {
        private ColumnComparer columnComparer;
        private RowComparer rowComparer;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<GameState>();
            columnComparer = new ColumnComparer(EntityManager);
            rowComparer = new RowComparer(EntityManager);
        }

        protected override void OnUpdate()
        {
            var gameState = GetSingleton<GameState>();
            if (gameState.state != State.Swap) return;
            var em = EntityManager;
            bool isMoving = false;
            var countSelected = Entities.GetSelectedEntities();
            Entities.WithAll<Selected, Cell>().ForEach((Entity e, ref Cell cell, ref Translation translation) => {
                if (math.abs(translation.Value.y - cell.row) > 0.1f)
                {
                    float sign = 1.0f;
                    if (translation.Value.y > cell.row) sign = -1.0f;
                    translation.Value.y += sign *Setting.Instance.speed * Time.DeltaTime;
                    isMoving = true;
                }
                else
                {
                    translation.Value.y = cell.row;                    
                }

                if (math.abs(translation.Value.x - cell.column) > 0.1f)
                {
                    float sign = 1.0f;
                    if (translation.Value.x > cell.column) sign = -1.0f;
                    translation.Value.x += sign * Setting.Instance.speed * Time.DeltaTime;
                    isMoving = true;
                }
                else
                {
                    translation.Value.x = cell.column;
                }
            });


            if (!isMoving) gameState.state = State.LoookingForMatches;            
            SetSingleton<GameState>(gameState);
        }
    }
}