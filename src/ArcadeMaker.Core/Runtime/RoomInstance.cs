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
                    instances.Sort((a, b) => a.Depth.Value!.Number.CompareTo(b.Depth.Value!.Number));
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