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
                                em.RemoveComponent<Selected>(neighbor);
                                em.SetComponentData<Scale>(neighbor, new Scale {
                                    Value = Setting.Instance.normalScale
                                });

                                var anotherCell = em.GetComponentData<Cell>(neighbor);
                                if (cell.IsNeighbour(anotherCell)) {
                                    needSelect = false;                                    
                                    gameState.state = State.Swap;
                                    SetSingleton<GameState>(gameState);                                    
                                }
                                else {
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
    }
}