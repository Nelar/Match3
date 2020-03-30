using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Rendering;

namespace Match3
{
    public class RowComparer : IComparer<Entity>
    {
        EntityManager manager;        
        public RowComparer(EntityManager nManager) => manager = nManager;
        public Gravity gravity = Gravity.Down;
        public int Compare(Entity x, Entity y)
        {
            if (gravity == Gravity.Down)
                return manager.GetCellByEntity(x).row - manager.GetCellByEntity(y).row;
            else
                return manager.GetCellByEntity(y).row - manager.GetCellByEntity(x).row;
        }
    }

    public class ColumnComparer : IComparer<Entity>
    {
        EntityManager manager;
        public ColumnComparer(EntityManager nManager) => manager = nManager;
        public int Compare(Entity x, Entity y)
        {
            return manager.GetCellByEntity(x).column - manager.GetCellByEntity(y).column;
        }
    }

    public class Randomizer
    {
        private Unity.Mathematics.Random random;

        public Randomizer(uint seed)
        {
            random = new Unity.Mathematics.Random(seed);
        }

        public Color RandomColor()
        {
            var v = System.Enum.GetValues(typeof(Color));
            return (Color)v.GetValue(random.NextInt(v.Length - 1));
        }
    }

    public static class SystemsHelper
    {
        public static Randomizer Random;

        public static NativeList<Entity> GetSelectedEntities(this EntityQueryBuilder builder)
        {
            var result = new NativeList<Entity>(Allocator.Temp);
            builder.WithAll<Selected>().ForEach((Entity e) => result.Add(e));
            return result;
        }

        public static Cell GetCellByEntity(this EntityManager manager, Entity entity)
        {
            return manager.GetComponentData<Cell>(entity);
        }

        public static NativeList<Entity> GetCellEntities(this EntityQueryBuilder builder)
        {
            var result = new NativeList<Entity>(Allocator.Temp);
            builder.WithAll<Cell>().ForEach((Entity e) => result.Add(e));
            return result;
        }

        public static NativeList<Empty> GetEmptyEntities(this EntityQueryBuilder builder)
        {
            var result = new NativeList<Empty>(Allocator.Temp);
            builder.WithAll<Empty>().ForEach((Entity e, ref Empty empty) => result.Add(empty));
            return result;
        }      

        public static NativeList<Entity> GetCellEntitiesByRow(this EntityQueryBuilder builder, int row)
        {
            var result = new NativeList<Entity>(Allocator.Temp);
            builder.WithAll<Cell>().ForEach((Entity e, ref Cell c) => {
                if (c.row == row) result.Add(e);
            });
            return result;
        }

        public static NativeList<Entity> GetCellEntitiesByColumn(this EntityQueryBuilder builder, int column)
        {
            var result = new NativeList<Entity>(Allocator.Temp);
            builder.WithAll<Cell>().ForEach((Entity e, ref Cell c) => {
                if (c.column == column) result.Add(e);
            });
            return result;
        }

        public static bool IsNeighbour(this Cell cell, Cell anotherCell)
        {
            if (cell.row == anotherCell.row && math.abs(cell.column - anotherCell.column) == 1) return true;
            if (cell.column == anotherCell.column && math.abs(cell.row - anotherCell.row) == 1) return true;
            return false;
        }

        public static Entity SpawnCell(this EntityManager em, int r, int c, Color color, Type type, float yOffset = 0.0f)
        {
            var entity = em.Instantiate(Setting.Instance.PrefabEntity);
            em.AddComponent<Scale>(entity);
            em.AddComponent<Cell>(entity);
            em.SetComponentData<Scale>(entity, new Scale {
                Value = Setting.Instance.normalScale
            });
            em.SetComponentData<Translation>(entity, new Translation {
                Value = new float3(c, r + yOffset, 0)
            });
            var cell = new Cell {
                color = color,
                type = type,
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
            return entity;
        }

        public static UnityEngine.Material TextureByCell(Cell cell)
        {
            var texturesSetting = System.Array.Find(Setting.Instance.Views, x => x.color == cell.color && x.type == cell.type);
            return new UnityEngine.Material(texturesSetting.material);
        }
    }
}