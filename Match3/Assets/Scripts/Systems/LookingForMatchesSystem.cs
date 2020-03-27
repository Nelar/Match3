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
        class RowComparer : IComparer<Entity>
        {
            EntityManager manager;
            public RowComparer(EntityManager nManager) => manager = nManager;
            public int Compare(Entity x, Entity y)
            {
                return manager.GetCellByEntity(x).row - manager.GetCellByEntity(y).row;
            }
        }

        class ColumnComparer : IComparer<Entity>
        {
            EntityManager manager;
            public ColumnComparer(EntityManager nManager) => manager = nManager;
            public int Compare(Entity x, Entity y)
            {
                return manager.GetCellByEntity(x).column - manager.GetCellByEntity(y).column;
            }
        }

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
                            for (int j = r - 1; countMatched > 0; countMatched--, j--)
                            {
                                em.AddComponent<Matched>(row[j]);
                            }
                        }
                        countMatched = 0;
                    }
                    color = cell.color;
                }
                if (countMatched >= 2) {
                    countMatched++;
                    for (int j = r - 2; countMatched > 0; countMatched--, j--)
                    {
                        em.AddComponent<Matched>(row[j]);
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
                            for (int j = r - 1; countMatched > 0; countMatched--, j--)
                            {
                                em.AddComponent<Matched>(column[j]);
                            }
                        }
                        countMatched = 0;
                    }
                    color = cell.color;
                }
                if (countMatched >= 2)
                {
                    countMatched++;
                    for (int j = r - 2; countMatched > 0; countMatched--, j--)
                    {
                        em.AddComponent<Matched>(column[j]);
                    }
                }
            }

            bool setIdle = true;
            Entities.WithAll<Matched>().ForEach((Entity entity) => {
                var cell = em.GetCellByEntity(entity);
                var empty = em.CreateEntity(typeof(Empty));
                em.SetComponentData<Empty>(empty, new Empty {
                    column = cell.column,
                    row = cell.row
                });
                setIdle = false;
                em.DestroyEntity(entity);
            });

            if (setIdle) gameState.state = State.Idle;
            else gameState.state = State.Falling;

            SetSingleton<GameState>(gameState);
        }
    }
}