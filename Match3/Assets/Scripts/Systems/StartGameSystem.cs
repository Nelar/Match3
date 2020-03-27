using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System.Collections.Generic;

namespace Match3
{
    public class StartGameSystem : ComponentSystem
    {
        private Random random;
        protected override void OnCreate()
        {
            random = new Random(100500);
        }

        private Color RandomColor()
        {
            var v = System.Enum.GetValues(typeof(Color));
            return (Color)v.GetValue(random.NextInt(v.Length - 1));
        }

        private UnityEngine.Material TextureByCell(Cell cell)
        {
            var texturesSetting = System.Array.Find(Setting.Instance.Views, x => x.color == cell.color && x.type == cell.type);
            return new UnityEngine.Material(texturesSetting.material);
        }

        protected override void OnStartRunning()
        {
            var gameStateEntity = EntityManager.CreateEntity(typeof(GameState));
            EntityManager.SetComponentData<GameState>(gameStateEntity, new GameState {
                state = State.LoookingForMatches,
                gravity = Gravity.Down
            });

            for (var r = 0; r < Setting.Instance.rowCount; r++)
            {
                for (var c = 0; c < Setting.Instance.columnCount; c++)
                {
                    SpawnCell(r, c);
                }
            }
        }

        private void SpawnCell(int r, int c)
        {
            var em = EntityManager;
            var entity = em.Instantiate(Setting.Instance.PrefabEntity);
            em.AddComponent<Scale>(entity);
            em.AddComponent<Cell>(entity);
            em.SetComponentData<Scale>(entity, new Scale {
                Value = 1.0f
            });
            em.SetComponentData<Translation>(entity, new Translation {
                Value = new float3(r, c, 0)
            });
            var cell = new Cell {
                color = RandomColor(),
                type = Type.Simple,
                row = r,
                column = c
            };
            em.SetComponentData<Cell>(entity, cell);            
            var renderMesh = em.GetSharedComponentData<RenderMesh>(entity);
            em.SetSharedComponentData<RenderMesh>(entity, new RenderMesh {
                mesh = renderMesh.mesh,
                layer = 0,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows = false,
                needMotionVectorPass = false,
                material = TextureByCell(cell)
            });
        }

        protected override void OnUpdate()
        {            
        }
    }
}