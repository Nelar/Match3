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
        protected override void OnCreate()
        {            
        }        

        protected override void OnStartRunning()
        {
            SystemsHelper.Random = new Randomizer(12313);
            var em = EntityManager;
            var gameStateEntity = em.CreateEntity(typeof(GameState));
            em.SetComponentData<GameState>(gameStateEntity, new GameState {
                state = State.LoookingForMatches,
                gravity = Setting.Instance.gravity,
                needChangeGravity = false
            });

            for (var r = 0; r < Setting.Instance.rowCount; r++)
            {
                for (var c = 0; c < Setting.Instance.columnCount; c++)
                {
                    em.SpawnCell(r, c, SystemsHelper.Random.RandomColor(), Type.Simple);
                }
            }
        }

        protected override void OnUpdate()
        {            
        }
    }
}