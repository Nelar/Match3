using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Match3
{
    public static class SystemsHelper
    {
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
    }
}