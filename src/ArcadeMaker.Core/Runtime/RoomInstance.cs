using ArcadeMaker.Core.Models;
using Exp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Runtime
{
    public class RoomInstance
    {
        public RoomModel Model { get; }
        public List<RoomBackground> Backgrounds { get; } = [];
        private readonly List<Instance> instances = [];
        public List<Instance> Instances => instances;
        public IEnumerable<Instance> SortedInstances
        {
            get
            {
                if (!isSorted)
                {
                    instances.Sort((a, b) => b.Depth.Value!.Number.CompareTo(a.Depth.Value!.Number));
                    isSorted = true;
                }
                return instances;
            }
        }

        private bool isSorted = false;
        public RoomInstance(RoomModel model)
        {
            this.Model = model;

            // add all instances from the init map
            foreach (var item in model.InitMap.Items)
            {
                var instance = new Instance(item.Object);
                instance.X.Value = item.X.ToExp();
                instance.Y.Value = item.Y.ToExp();
                instance.DepthChanged += MarkDepthChanged;
                AddInstance(instance);
            }

            // copy all backgrounds from the model
            foreach (var background in model.Backgrounds)
            {
                Backgrounds.Add(background with { });
            }
        }

        public void AddInstance(Instance instance)
        {
            instances.Add(instance);

            // --- LinkedList approach: ------------------
            // find the instance that after it all instances have higher depth
            //LinkedListNode<Instance>? addBefore = null;

            //if (instances.Count >= 1)
            //{
            //    var next = instances.First;
            //    while (next != null)
            //    {
            //        if (next.Value.Depth.Value!.Number < instance.Depth.Value!.Number)
            //        {
            //            addBefore = next;
            //            break;
            //        }
            //        next = next.Next;
            //    }
            //}

            //if (addBefore == null)
            //{
            //    if (instances.Count == 0)
            //        instances.AddFirst(instance);
            //    else
            //        instances.AddLast(instance);
            //}
            //else
            //    instances.AddBefore(addBefore, instance);
            // ------------------------------------------------

            isSorted = false;
        }

        public void RemoveInstance(Instance instance)
        {
            instances.Remove(instance);
            instance.DepthChanged -= MarkDepthChanged;
        }

        public void MarkDepthChanged(object? sender, double depth)
        {
            isSorted = false;
        }
    }
}