using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Null.ArgsParser
{
    public enum CommandElementType
    {
        SwitchArgument,
        FieldArgument,
        PropertyArgument,
        CommandLine,
        Arguments,

        NotArgument
    }
    public abstract class ArgumentElement : ICommandElement
    {
        public abstract bool IgnoreCase { get; set; }
        public abstract bool IsTriggered(string txt);
        public abstract bool TryParse(ref string[] args, ref int index);
        public abstract bool TryAssign(Type type, object instance);
    }
    public abstract class NamedArgumentElement : ArgumentElement
    {
        public abstract string Name { get; }
        public virtual CommandElementType ArgumentType { get; } = CommandElementType.NotArgument;
    }
    public interface ICommandElement
    {
        bool IgnoreCase { get; set; }
        bool IsTriggered(string txt);
        bool TryParse(ref string[] args, ref int index);
        bool TryAssign(Type type, object instance);
    }
    public interface ICommandElementContainer : ICollection<ICommandElement>
    {
        ICommandElement[] Elements { get; }
        void Parse(string[] args);
        T ToObject<T>();
    }
    public class SwitchArgument : NamedArgumentElement
    {
        string name = string.Empty;
        bool ignoreCase = false;

        char triggerChar = '/';
        bool enabled = false;
        bool assignable = false;

        public override string Name { get => name; }
        public override bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public override CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
        public char TriggerChar { get => triggerChar; set => triggerChar = value; }

        public bool Enabled
        {
            get => enabled;
            set
            {
                assignable = true;
                enabled = value;
            }
        }

        public SwitchArgument() { }
        public SwitchArgument(string name)
        {
            this.name = name;
        }
        public SwitchArgument(string name, bool enabled)    
        {
            this.name = name;
            this.Enabled = enabled;
        }

        public override bool IsTriggered(string txt)
        {
            if (ignoreCase)
                return txt.ToUpper().Equals(triggerChar + name.ToUpper());
            else
                return txt.Equals(triggerChar + name);
        }
        public override bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                this.Enabled = !enabled;

                index++;
                return true;
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (assignable)
            {
                FieldInfo fieldInfo = type.GetField(name);
                if (fieldInfo != null)
                {
                    if (fieldInfo.FieldType == typeof(bool))
                    {
                        fieldInfo.SetValue(instance, enabled);
                        return true;
                    }
                }
            }

            return false;
        }
    }
    public class PropertyArgument : NamedArgumentElement
    {
        string name = string.Empty;
        bool ignoreCase = false;

        char triggerChar = '-';
        string value = string.Empty;
        bool assignable = false;

        public override string Name { get => name; }
        public override bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public override CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
        public char TriggerChar { get => triggerChar; set => triggerChar = value; }

        public string Value
        {
            get => value;
            set
            {
                this.assignable = true;
                this.value = value;
            }
        }

        public PropertyArgument() { }
        public PropertyArgument(string name)
        {
            this.name = name;
        }
        public PropertyArgument(string name, string value)
        {
            this.name = name;
            this.Value = value;
        }

        public override bool IsTriggered(string txt)
        {
            if (ignoreCase)
                return txt.ToUpper().Equals(triggerChar + name.ToUpper());
            else
                return txt.Equals(triggerChar + name);
        }
        public override bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                index++;
                if (index < args.Length)
                {
                    this.Value = args[index];

                    index++;
                    return true;
                }
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (assignable)
            {
                FieldInfo fieldInfo = type.GetField(name);
                if (fieldInfo != null)
                {
                    if (fieldInfo.FieldType == typeof(string))
                    {
                        fieldInfo.SetValue(instance, value);
                        return true;
                    }
                }
            }

            return false;
        }
    }
    public class FieldArgument : NamedArgumentElement
    {
        string name = string.Empty;
        bool ignoreCase = false;
        CommandElementType type = CommandElementType.FieldArgument;

        char triggerChar = '=';
        string value = string.Empty;
        bool assignable = false;

        public override string Name { get => name; }
        public override bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public override CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
        public char TriggerChar { get => triggerChar; set => triggerChar = value; }

        public string Value
        {
            get => value;
            set
            {
                this.assignable = true;
                this.value = value;
            }
        }

        public FieldArgument() { }
        public FieldArgument(string name)
        {
            this.name = name;
        }
        public FieldArgument(string name, string value)
        {
            this.name = name;
            this.Value = value;
        }

        public override bool IsTriggered(string txt)
        {
            if (ignoreCase)
                return txt.ToUpper().StartsWith(name.ToUpper() + triggerChar);
            else
                return txt.StartsWith(name + triggerChar);
        }
        public override bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                this.Value = args[index].Substring(name.Length + 1);

                index++;
                return true;
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (assignable)
            {
                FieldInfo fieldInfo = type.GetField(name);
                if (fieldInfo != null)
                {
                    if (fieldInfo.FieldType == typeof(string))
                    {
                        fieldInfo.SetValue(instance, value);
                        return true;
                    }
                }
            }

            return false;
        }
    }
    public class CommandLine : NamedArgumentElement, ICommandElementContainer
    {
        string name = string.Empty;
        bool selfIgnoreCase = false;
        bool enabled;
        bool assignable = false;

        List<ICommandElement> cmdElements = new List<ICommandElement>();
        List<string> strContent = new List<string>();

        public override string Name { get => name; }
        public bool SelfIgnoreCase { get => selfIgnoreCase; set => selfIgnoreCase = value; }
        public ICommandElement[] Elements { get => cmdElements.ToArray(); }
        public string ExContentName { get; set; } = "ExtraContent";
        public string[] ExtraContent { get => strContent.ToArray(); }

        public int Count => cmdElements.Count;
        public bool IsReadOnly => false;
        public bool ElementsIgnoreCase
        {
            get
            {
                bool ignoreCase = false;
                foreach (ICommandElement element in cmdElements)
                    ignoreCase &= element.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (ICommandElement element in cmdElements)
                    element.IgnoreCase = value;
            }
        }
        public override bool IgnoreCase
        {
            get
            {
                bool ignoreCase = selfIgnoreCase;
                foreach (ICommandElement element in cmdElements)
                    ignoreCase &= element.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (ICommandElement element in cmdElements)
                    element.IgnoreCase = value;
                selfIgnoreCase = value;
            }
        }

        public CommandLine() { }
        public CommandLine(string name)
        {
            this.name = name;
        }
        public CommandLine(string name, params ICommandElement[] args)
        {
            this.name = name;
            cmdElements.AddRange(args);
        }

        public override bool IsTriggered(string txt)
        {
            if (selfIgnoreCase)
                return txt.ToUpper().StartsWith(name.ToUpper());
            else
                return txt.StartsWith(name);
        }
        public override bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                index++;
                enabled = true;

                bool parsed;
                for (; index < args.Length; index++)
                {
                    parsed = false;
                    for (int j = 0; j < cmdElements.Count && index < args.Length; j++)
                    {
                        parsed |= cmdElements[j].TryParse(ref args, ref index);
                    }
                    if (!parsed)
                    {
                        strContent.Add(args[index]);
                    }
                }

                this.assignable = true;
                return true;
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (assignable)
            {
                FieldInfo fieldInfo = type.GetField(name);
                FieldInfo strCntntFieldInfo = type.GetField(ExContentName);
                if (fieldInfo != null)
                {
                    if (fieldInfo.FieldType == typeof(bool))
                    {
                        fieldInfo.SetValue(instance, enabled);

                        if (strCntntFieldInfo.FieldType.IsAssignableFrom(typeof(List<string>)))
                            strCntntFieldInfo.SetValue(instance, strContent);
                        else if (typeof(string[]) == strCntntFieldInfo.FieldType)
                            strCntntFieldInfo.SetValue(instance, strContent.ToArray());

                        foreach (ICommandElement element in cmdElements)
                        {
                            element.TryAssign(type, instance);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void Add(ICommandElement argv)
        {
            cmdElements.Add(argv);
        }
        public void Clear()
        {
            cmdElements.Clear();
        }
        public bool Contains(ICommandElement argv)
        {
            return cmdElements.Contains(argv);
        }
        public void CopyTo(ICommandElement[] array, int index)
        {
            cmdElements.CopyTo(array, index);
        }
        bool ICollection<ICommandElement>.Remove(ICommandElement item)
        {
            return cmdElements.Remove(item);
        }
        public IEnumerator<ICommandElement> GetEnumerator()
        {
            return cmdElements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return cmdElements.GetEnumerator();
        }




        public void Parse(string[] args)
        {
            int index = 0;
            TryParse(ref args, ref index);
        }
        public T ToObject<T>()
        {
            Type type = typeof(T);
            T result = (T)Activator.CreateInstance(type);

            TryAssign(type, result);
            return result;
        }
    }
    public class Arguments : ICommandElement, ICommandElementContainer
    {
        List<ICommandElement> cmdElements = new List<ICommandElement>();
        List<string> strContent = new List<string>();
        public ICommandElement[] Elements { get => cmdElements.ToArray(); }

        public int Count => cmdElements.Count;
        public bool IsReadOnly => false;
        public bool IgnoreCase
        {
            get
            {
                bool ignoreCase = false;
                foreach (ICommandElement element in cmdElements)
                    ignoreCase &= element.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (ICommandElement element in cmdElements)
                    element.IgnoreCase = value;
            }
        }
        public string ExContentName { get; set; } = "ExtraContent";
        public string[] StringContent { get => strContent.ToArray(); }

        public Arguments() { }
        public Arguments(params ICommandElement[] args)
        {
            cmdElements.AddRange(args);
        }

        public bool IsTriggered(string txt)
        {
            return true;
        }
        public bool TryParse(ref string[] args, ref int index)
        {
            bool parsed;
            for (; index < args.Length; index++)
            {
                parsed = false;
                for (int j = 0; j < cmdElements.Count && index < args.Length; j++)
                {
                    parsed |= cmdElements[j].TryParse(ref args, ref index);
                }
                if (!parsed)
                {
                    strContent.Add(args[index]);
                }
            }

            return true;
        }
        public bool TryAssign(Type type, object instance)
        {
            FieldInfo strCntntFieldInfo = type.GetField(ExContentName);
            if (strCntntFieldInfo != null)
            {
                if (strCntntFieldInfo.FieldType.IsAssignableFrom(typeof(List<string>)))
                    strCntntFieldInfo.SetValue(instance, strContent);
                else if (typeof(string[]) == strCntntFieldInfo.FieldType)
                    strCntntFieldInfo.SetValue(instance, strContent.ToArray());
            }

            foreach (ICommandElement element in cmdElements)
            {
                element.TryAssign(type, instance);
            }

            return true;
        }

        public void Add(ICommandElement argv)
        {
            cmdElements.Add(argv);
        }
        public void Clear()
        {
            cmdElements.Clear();
        }
        public bool Contains(ICommandElement argv)
        {
            return cmdElements.Contains(argv);
        }
        public void CopyTo(ICommandElement[] array, int index)
        {
            cmdElements.CopyTo(array, index);
        }
        bool ICollection<ICommandElement>.Remove(ICommandElement item)
        {
            return cmdElements.Remove(item);
        }
        public IEnumerator<ICommandElement> GetEnumerator()
        {
            return cmdElements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return cmdElements.GetEnumerator();
        }

        public void Parse(string[] args)
        {
            int index = 0;
            TryParse(ref args, ref index);
        }
        public T ToObject<T>()
        {
            Type type = typeof(T);
            T result = (T)Activator.CreateInstance(type);

            TryAssign(type, result);
            return result;
        }
    }
}
