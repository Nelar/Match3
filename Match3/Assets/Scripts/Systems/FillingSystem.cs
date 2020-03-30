using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class FillingSystem : ComponentSystem
    {
        private RowComparer rowComparer;
        private float cellOffset = 0.0f;
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<GameState>();
            rowComparer = new RowComparer(EntityManager);
        }

        protected override void OnUpdate()
        {
            var gameState = GetSingleton<GameState>();
            if (gameState.state != State.Filling) return;
            var em = EntityManager;
            
            for (var c = 0; c < Setting.Instance.columnCount; c++)
            {                
                if (gameState.gravity == Gravity.Down)
                {
                    cellOffset = 0.0f;
                    for (var r = 0; r < Setting.Instance.rowCount; r++) FillLogic(r, c, gameState.gravity);
                }
                else
                {
                    cellOffset = -1.0f;
                    for (var r = Setting.Instance.rowCount-1; r >= 0; r--) FillLogic(r, c, gameState.gravity);
                }
            }

            bool setFalling = true;
            Entities.WithAll<Empty>().ForEach((Entity entity) => {
                setFalling = false;
            });

            if (setFalling) gameState.state = State.Falling;
            SetSingleton<GameState>(gameState);
        }

        private void FillLogic(int r, int c, Gravity gravity)
        {
            var em = EntityManager;
            Entities.WithAll<Empty>().ForEach((Entity e, ref Empty empty) => {
                if (empty.row == r && empty.column == c)
                {
                    var entities = Entities.GetCellEntitiesByColumn(c);
                    rowComparer.gravity = gravity;
                    entities.Sort(rowComparer);
                    bool needAddCell = true;
                    foreach (var nearestCell in entities)
                    {
                        var cell = em.GetCellByEntity(nearestCell);
                        if (gravity == Gravity.Down && cell.row > r || gravity == Gravity.Up && cell.row < r)
                        {
                            int newEmptyColumn = cell.column;
                            int newEmptyRow = cell.row;
                            em.SetComponentData<Cell>(nearestCell, new Cell {
                                color = cell.color,
                                type = cell.type,
                                row = r,
                                column = c
                            });
                            em.AddComponent<Falling>(nearestCell);
                            needAddCell = false;
                            em.SetComponentData<Empty>(e, new Empty {
                                column = newEmptyColumn,
                                row = newEmptyRow
                            });
                            break;
                        }
                    }
                    if (needAddCell)
                    {
                        float offset = Setting.Instance.rowCount - r + cellOffset;
                        if (gravity == Gravity.Up)
                            offset = -r + cellOffset;
                        var newCell = em.SpawnCell(r, c, SystemsHelper.Random.RandomColor(), Type.Simple,
                            offset);
                        em.AddComponent<Falling>(newCell);
                        em.DestroyEntity(e);
                        cellOffset += (gravity == Gravity.Down)?1.0f:-1.0f;
                    }
                }
            });
        }
    }
}