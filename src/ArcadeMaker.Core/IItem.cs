using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core
{
    public interface IItem
    {
        string Name { get; }
    }


    public interface ISetsID : IItem
    {
        int ID { get; }
    }

    public static class ID
    {
        private static int currentId = 0;
        internal static int Generate() => currentId++;

        public static T GetById<T>(this IEnumerable<T> list, int id) where T : ISetsID
        {
            return list.FirstOrDefault(item => item.ID == id) ?? throw new Exceptions.ResourceNotFoundException(id);
        }
    }
}