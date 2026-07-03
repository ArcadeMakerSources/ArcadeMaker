using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using ArcadeMaker.Core.ExpSrc.Controls;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.Core.Common;
using Exp;
using Exp.Spans;

namespace ArcadeMaker.Core.Models
{
    public class ObjectModel : IModel
    {
        public string Name { get; }
        public Sprite? Sprite { get; }
        public ClassDefSpan Class { get; }
        public List<ObjectEvent> Events { get; }
        public (int Depth, bool Visible, bool Solid) InitValues { get; set; }
        public ObjectProperty[] ExtraProperties { get; }

        internal ObjectEvent? CreateEvent { get; set; } // setter is required for the properties initializer generator
        internal ObjectEvent? StepEvent { get; private set; }
        internal ObjectEvent? DrawEvent { get; private set; }
        internal ObjectEvent? DestroyEvent { get; private set; }
        internal ParameterizedObjectEvent<Keys>[] KeyDownEvents { get; private set; }
        internal ParameterizedObjectEvent<Keys>[] KeyPressEvents { get; private set; }
        internal ParameterizedObjectEvent<Keys>[] KeyReleaseEvents { get; private set; }
        internal ParameterizedObjectEvent<MouseButton>[] MouseDownEvents { get; private set; }
        internal ParameterizedObjectEvent<MouseButton>[] MousePressEvents { get; private set; }
        internal ParameterizedObjectEvent<MouseButton>[] MouseReleaseEvents { get; private set; }
        internal CollisionEvent[] CollisionEvents { get; private set; }
        internal ParameterizedObjectEvent<int>[] AlarmEvents { get; private set; }
        internal ObjectEvent? OutsideRoomEvent { get; private set; }

        public ObjectModel(string name, Sprite? sprite, ObjectEvent[] events, ObjectProperty[] extraProperties)
        {
            this.Name = name;
            this.Sprite = sprite;
            this.ExtraProperties = extraProperties;

            this.Class = CreateClass(name, extraProperties); // create a class for this object model
            events.ForEach(e => e.CreateDocs(Class));
            
            this.Events = [..events];

            // AssignEvents() is called by RemoveEmptyEvents() which is called by GameRunner's ctor
        }

        private static ClassDefSpan CreateClass(string name, ObjectProperty[] extraProps)
        {
            // create properties
            List<Property> props = [];
            foreach (var property in Runtime.Instance.csProperties)
            {
                props.Add(new(def: null /*(assigned at ClassDefSpan's ctor)*/, false, property.Key.Name.StartWithLowerCase(), false, false));
            }
            foreach (var property in extraProps)
            {
                props.Add(new(def: null /*(assigned at ClassDefSpan's ctor)*/, property.Constant, property.Name, property.Private, false));
            }

            return new ClassDefSpan(name, [..props], []);
        }

        /// <summary>
        /// Gets the first <see cref="ObjectEvent"/> with <see cref="ObjectEvent.Type"/> equals to the given <see cref="ObjectEvent.EventType"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The first <see cref="ObjectEvent"/> with <see cref="ObjectEvent.Type"/> equals to the given <see cref="ObjectEvent.EventType"/>, or <c>null</c>.</returns>
        internal ObjectEvent? GetEvent(ObjectEvent.EventType type)
        {
            return Events.FirstOrDefault(e => e.Type == type);
        }

        internal ObjectEvent? GetEvent<T>(ObjectEvent.EventType type, T param)
        {
            return Events.OfType<ParameterizedObjectEvent<T>>().FirstOrDefault(e => e.Type == type && EqualityComparer<T>.Default.Equals(e.Param, param));
        }

        internal IEnumerable<ParameterizedObjectEvent<T>> GetEvents<T>(ObjectEvent.EventType type)
        {
            return Events.OfType<ParameterizedObjectEvent<T>>().Where(e => e.Type == type);
        }

        internal void RemoveEmptyEvents()
        {
            // this method is being called after TryPrepare(...) is called on all docs, so their ContainsCode property can be trusted
            for (int i = 0; i < Events.Count; i++)
            {
                var ev = Events[i];
                ev.Docs!.RemoveAll(doc => !doc.ContainsCode);
                if (ev.Docs.Count == 0)
                {
                    Events.Remove(ev);
                    i--;
                }
            }

            AssignEvents();
        }

        private void AssignEvents()
        {
            CreateEvent = GetEvent(ObjectEvent.EventType.Create);
            StepEvent = GetEvent(ObjectEvent.EventType.Step);
            DrawEvent = GetEvent(ObjectEvent.EventType.Draw);
            DestroyEvent = GetEvent(ObjectEvent.EventType.Destroy);
            OutsideRoomEvent = GetEvent(ObjectEvent.EventType.OutsideRoom);
            KeyDownEvents = [.. GetEvents<Keys>(ObjectEvent.EventType.KeyDown)];
            KeyPressEvents = [.. GetEvents<Keys>(ObjectEvent.EventType.KeyPress)];
            KeyReleaseEvents = [.. GetEvents<Keys>(ObjectEvent.EventType.KeyRelease)];
            MouseDownEvents = [.. GetEvents<MouseButton>(ObjectEvent.EventType.MouseDown)];
            MousePressEvents = [.. GetEvents<MouseButton>(ObjectEvent.EventType.MousePress)];
            MouseReleaseEvents = [.. GetEvents<MouseButton>(ObjectEvent.EventType.MouseRelease)];
            CollisionEvents = [.. Events.OfType<CollisionEvent>()];
            AlarmEvents = [.. GetEvents<int>(ObjectEvent.EventType.Alarm)];
        }
    }

    public class ObjectEvent
    {
        public ObjectEvent() { } // for the serializer
        public ObjectEvent(EventType type, IEnumerable<string> scripts, params string[] scriptArgs)
        {
            this.Type = type;
            this.Scripts = [..scripts];
            this.ScriptArgs = [..scriptArgs];
        }

        public enum EventType
        {
            Create,
            Step,
            Draw,
            Destroy,
            Collision,
            KeyDown,
            KeyPress,
            KeyRelease,
            MouseDown,
            MousePress,
            MouseRelease,
            MouseWheel,
            Alarm,
            OutsideRoom
        }

        public EventType Type { get; set; }
        public List<Wrapper<string>> Scripts { get; set; }

        [XmlIgnore]
        public List<InstanceScriptDocument> Docs { get; } = [];

        public string[] ScriptArgs { get; set; }

        internal void CreateDocs(ClassDefSpan def)
        {
            for (int i = 0; i < Scripts.Count; i++)
            {
                Docs.Add(ExpSrc.ExpSrc.CreateInstanceScriptDocument($"{def.Name}.Events.{Type}{(GetParam(out var param) ? $"<{param}>" : "")}.{i}", def, Scripts[i] ?? throw new NullReferenceException($"{nameof(Scripts)}[{i}]"), ScriptArgs));
            }
        }

        internal void InsertDoc(int index, InstanceScriptDocument doc, bool insertScript)
        {
            if (Docs == null)
                throw new InvalidOperationException("Documents were not created.");

            if (insertScript)
                Scripts.Insert(index, doc.Script);
            Docs.Insert(index, doc);
        }

        public virtual bool GetParam(out object? value)
        {
            value = null;
            return false;
        }

        public virtual bool Equals(ObjectEvent? other)
        {
            return other != null && Type == other.Type && Scripts.SequenceEqual(other.Scripts);
        }

        public override string ToString() => Type.ToString();
    }

    public class ParameterizedObjectEvent<T> : ObjectEvent
    {
        public ParameterizedObjectEvent() { } // for the serializer
        public ParameterizedObjectEvent(ObjectEvent.EventType type, IEnumerable<string> scripts, T param, params string[] scriptArgs) : base(type, scripts, scriptArgs)
        {
            this.Param = param;
        }

        public T Param { get; set; }

        public sealed override bool GetParam(out object? value)
        {
            value = Param;
            return true;
        }

        public override bool Equals(ObjectEvent? other)
        {
            return base.Equals(other) && other is ParameterizedObjectEvent<T> parameterizedOther && EqualityComparer<T>.Default.Equals(Param, parameterizedOther.Param);
        }

        public override string ToString() => $"{base.ToString()} <{Param}>";
    }

    public class CollisionEvent : ParameterizedObjectEvent<string>
    {
        public CollisionEvent() : base() { }
        public CollisionEvent(string obj, IEnumerable<string> scripts) : base(EventType.Collision, scripts, obj, "other") { }

        [XmlIgnore]
        internal ObjectModel Model { get; private set; } = null!;

        internal void Resolve(IGame game)
        {
            Model =
                game.Objects.FirstOrDefault(o => o.Name == Param) ??
                throw new KeyNotFoundException($"The given game doesn't contain an object model called '{Param}'.");
        }
    }
}