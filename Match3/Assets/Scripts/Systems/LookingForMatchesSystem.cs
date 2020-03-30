using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class LookingForMatchesSystem : ComponentSystem
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
            if (gameState.state != State.LoookingForMatches) return;

            if (gameState.needChangeGravity)
            {
                gameState.needChangeGravity = false;
                if (gameState.gravity == Gravity.Down) gameState.gravity = Gravity.Up;
                else gameState.gravity = Gravity.Down;
            }

            var em = EntityManager;
            for (int i = 0; i < Setting.Instance.rowCount; i++) {
                var row = Entities.GetCellEntitiesByRow(i);
                row.Sort(columnComparer);

                Color color = Color.None;
                int countMatched = 0;
                int r = 0;
                for (r = 0; r < row.Length; r++) {
                    var cell = em.GetCellByEntity(row[r]);                    
                    if (cell.color == color) countMatched++;
                    else if (cell.color != color) {
                        if (countMatched >= 2) {
                            countMatched++;
                            int matched = countMatched;
                            for (int j = r - 1; countMatched > 0; countMatched--, j--)
                            {
                                var entity = row[j];
                                SetMatched(ref entity, matched);                                
                            }
                        }
                        countMatched = 0;
                    }
                    color = cell.color;
                }
                if (countMatched >= 2) {
                    countMatched++;
                    int matched = countMatched;
                    for (int j = r - 2; countMatched > 0; countMatched--, j--)
                    {
                        var entity = row[j];
                        SetMatched(ref entity, matched);
                    }
                }
            }

            for (int i = 0; i < Setting.Instance.columnCount; i++)
            {
                var column = Entities.GetCellEntitiesByColumn(i);
                column.Sort(rowComparer);
                Color color = Color.None;
                int countMatched = 0;
                int r = 0;
                for (r = 0; r < column.Length; r++)
                {
                    var cell = em.GetCellByEntity(column[r]);
                    if (cell.color == color) countMatched++;
                    else if (cell.color != color)
                    {
                        if (countMatched >= 2)
                        {
                            countMatched++;
                            int matched = countMatched;
                            for (int j = r - 1; countMatched > 0; countMatched--, j--)
                            {
                                var entity = column[j];
                                SetMatched(ref entity, matched);
                            }
                        }
                        countMatched = 0;
                    }
                    color = cell.color;
                }
                if (countMatched >= 2)
                {
                    countMatched++;
                    int matched = countMatched;
                    for (int j = r - 2; countMatched > 0; countMatched--, j--)
                    {
                        var entity = column[j];
                        SetMatched(ref entity, matched);
                    }
                }
            }

            bool setIdle = true;

            Entities.WithAll<Matched>().ForEach((Entity entity, ref Matched matched, ref Cell cell) => {
                if (matched.count >= 4 && em.HasComponent<Selected>(entity)) {
                    cell.type = Type.Gravity;
                    em.RemoveComponent<Matched>(entity);
                    var renderMesh = em.GetSharedComponentData<RenderMesh>(entity);
                    em.SetSharedComponentData<RenderMesh>(entity, new RenderMesh {
                        mesh = renderMesh.mesh,
                        layer = 0,
                        castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                        receiveShadows = false,
                        needMotionVectorPass = false,
                        material = SystemsHelper.TextureByCell(cell)
                    });
                }
                else
                {
                    if (cell.type == Type.Gravity) gameState.needChangeGravity = !gameState.needChangeGravity;
                    var empty = em.CreateEntity(typeof(Empty));
                    em.SetComponentData<Empty>(empty, new Empty {
                        column = cell.column,
                        row = cell.row
                    });
                    setIdle = false;
                    em.DestroyEntity(entity);
                }                
            });

            Entities.WithAll<Selected>().ForEach((Entity entity) => {
                em.RemoveComponent<Selected>(entity);
            });

            if (setIdle)
            {
                gameState.state = State.Idle;                
            }
            else gameState.state = State.Filling;

            SetSingleton<GameState>(gameState);
        }

        private void SetMatched(ref Entity entity, int countMatched)
        {
            var em = EntityManager;
            if (em.HasComponent<Matched>(entity)){
                if (em.GetComponentData<Matched>(entity).count < countMatched)
                {
                    em.SetComponentData<Matched>(entity, new Matched {
                        count = countMatched
                    });
                }
            }
            else
            {
                em.AddComponent<Matched>(entity);
                em.SetComponentData<Matched>(entity, new Matched {
                    count = countMatched
                });
            }            
        }
    }
}