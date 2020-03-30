using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class TouchSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<GameState>();
        }

        protected override void OnUpdate()
        {
            var gameState = GetSingleton<GameState>();
            if (gameState.state != State.Idle) return;

            if (Input.GetMouseButtonUp(0))
            {
                var em = EntityManager;
                var mousePos = Setting.Instance.Camera.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
                var touchPos = new float3(mousePos.x, mousePos.y, mousePos.z);                
                Entities.WithAll<Cell>().ForEach((Entity entity) => {
                    var translation = em.GetComponentData<Translation>(entity);
                    var distance = math.abs(math.distance(translation.Value, touchPos));
                    if (distance < 0.5f) {
                        if (em.HasComponent<Selected>(entity)) {
                            em.RemoveComponent<Selected>(entity);
                            em.SetComponentData<Scale>(entity, new Scale {
                                Value = Setting.Instance.normalScale
                            });
                        }
                        else {
                            var prevSelected = Entities.GetSelectedEntities();                                            
                            var cell = em.GetComponentData<Cell>(entity);
                            bool needSelect = true;
                            foreach (var neighbor in prevSelected) {                                
                                em.SetComponentData<Scale>(neighbor, new Scale {
                                    Value = Setting.Instance.normalScale
                                });
                                var anotherCell = em.GetComponentData<Cell>(neighbor);
                                if (cell.IsNeighbour(anotherCell)) {
                                    needSelect = false;
                                    em.SetComponentData<Scale>(entity, new Scale {
                                        Value = Setting.Instance.normalScale
                                    });
                                    em.SetComponentData<Scale>(neighbor, new Scale {
                                        Value = Setting.Instance.normalScale
                                    });

                                    if (CheckMatch(Entities, anotherCell.row, anotherCell.column, cell.color, cell.row, cell.column) ||
                                        CheckMatch(Entities, cell.row, cell.column, anotherCell.color, anotherCell.row, anotherCell.column))
                                    {
                                        em.AddComponent<Selected>(entity);
                                        em.SetComponentData<Cell>(entity, new Cell {
                                            row = anotherCell.row,
                                            column = anotherCell.column,
                                            color = cell.color,
                                            type = cell.type
                                        });
                                        em.SetComponentData<Cell>(neighbor, new Cell {
                                            row = cell.row,
                                            column = cell.column,
                                            color = anotherCell.color,
                                            type = anotherCell.type
                                        });
                                        gameState.state = State.Swap;
                                        SetSingleton<GameState>(gameState);
                                    }
                                    else
                                    {
                                        em.RemoveComponent<Selected>(neighbor);
                                        needSelect = true;
                                    }
                                }
                                else {
                                    em.RemoveComponent<Selected>(neighbor);
                                    needSelect = true;
                                }
                            }    
                            
                            if (needSelect) {
                                em.AddComponent<Selected>(entity);
                                em.SetComponentData<Scale>(entity, new Scale {
                                    Value = Setting.Instance.selectedScale
                                });
                            }
                        }
                    }
                });
            }
        }

        private bool CheckMatch(EntityQueryBuilder builder, int row, int column, Color color, 
            int previousRow, int previousColumn)
        {
            var entities = builder.GetCellEntitiesByColumn(column);
            entities.Sort(new RowComparer(EntityManager));
            var countMatched = 0;
            for (int i = row+1; i < Setting.Instance.rowCount; i++)
            {
                if (previousRow == i) break;
                if (EntityManager.GetCellByEntity(entities[i]).color == color) countMatched++;
                else break;
            }                
            for (int i = row-1; i >= 0; i--)
            {
                if (previousRow == i) break;
                if (EntityManager.GetCellByEntity(entities[i]).color == color) countMatched++;
                else break;
            }                

            if (countMatched >= 2) return true;

            entities = builder.GetCellEntitiesByRow(row);
            entities.Sort(new ColumnComparer(EntityManager));
            countMatched = 0;
            for (int i = column + 1; i < Setting.Instance.columnCount; i++)
            {
                if (previousColumn == i) break;
                if (EntityManager.GetCellByEntity(entities[i]).color == color) countMatched++;
                else break;
            }                
            for (int i = column - 1; i >= 0; i--)
            {
                if (previousColumn == i) break;
                if (EntityManager.GetCellByEntity(entities[i]).color == color) countMatched++;
                else break;
            }

            return countMatched >= 2;
        }
    }
}