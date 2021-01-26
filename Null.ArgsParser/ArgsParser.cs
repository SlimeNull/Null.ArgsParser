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
    public abstract class ArgumentElement : CommandElement
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
    public interface CommandElement
    {
        bool IgnoreCase { get; set; }
        bool IsTriggered(string txt);
        bool TryParse(ref string[] args, ref int index);
        bool TryAssign(Type type, object instance);
    }
    public interface CommandElementContainer : ICollection<CommandElement>
    {
        CommandElement[] Elements { get; }
        void Parse(string[] args);
        T ToObject<T>();
    }
    public class SwitchArgument : NamedArgumentElement
    {
        string name = string.Empty;
        bool ignoreCase = false;

        char triggerChar = '/';
        bool enabled = false;
        bool parsed = false;

        public override string Name { get => name; }
        public override bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public override CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
        public char TriggerChar { get => triggerChar; set => triggerChar = value; }

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public SwitchArgument() { }
        public SwitchArgument(string name)
        {
            this.name = name;
        }
        public SwitchArgument(string name, bool enabled)
        {
            this.name = name;
            this.enabled = enabled;
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
                enabled = !enabled;

                index++;
                parsed = true;
                return true;
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (parsed)
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
        bool parsed = false;

        public override string Name { get => name; }
        public override bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public override CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
        public char TriggerChar { get => triggerChar; set => triggerChar = value; }

        public string Value
        {
            get => value;
            set => this.value = value;
        }

        public PropertyArgument() { }
        public PropertyArgument(string name)
        {
            this.name = name;
        }
        public PropertyArgument(string name, string value)
        {
            this.name = name;
            this.value = value;
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
                    value = args[index];

                    index++;
                    parsed = true;
                    return true;
                }
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (parsed)
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
        bool parsed = false;

        public override string Name { get => name; }
        public override bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public override CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
        public char TriggerChar { get => triggerChar; set => triggerChar = value; }

        public string Value
        {
            get => value;
            set => this.value = value;
        }

        public FieldArgument() { }
        public FieldArgument(string name)
        {
            this.name = name;
        }
        public FieldArgument(string name, string value)
        {
            this.name = name;
            this.value = value;
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
                value = args[index].Substring(name.Length + 1);

                index++;
                parsed = true;
                return true;
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (parsed)
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
    public class CommandLine : NamedArgumentElement, CommandElementContainer
    {
        string name = string.Empty;
        bool selfIgnoreCase = false;
        bool enabled;
        bool parsed = false;

        List<CommandElement> cmdElements = new List<CommandElement>();
        List<string> strContent = new List<string>();

        public override string Name { get => name; }
        public bool SelfIgnoreCase { get => selfIgnoreCase; set => selfIgnoreCase = value; }
        public CommandElement[] Elements { get => cmdElements.ToArray(); }
        public string ExContentName { get; set; } = "ExtraContent";
        public string[] ExtraContent { get => strContent.ToArray(); }

        public int Count => cmdElements.Count;
        public bool IsReadOnly => false;
        public bool ElementsIgnoreCase
        {
            get
            {
                bool ignoreCase = false;
                foreach (CommandElement element in cmdElements)
                    ignoreCase &= element.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (CommandElement element in cmdElements)
                    element.IgnoreCase = value;
            }
        }
        public override bool IgnoreCase
        {
            get
            {
                bool ignoreCase = selfIgnoreCase;
                foreach (CommandElement element in cmdElements)
                    ignoreCase &= element.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (CommandElement element in cmdElements)
                    element.IgnoreCase = value;
                selfIgnoreCase = value;
            }
        }

        public CommandLine() { }
        public CommandLine(string name)
        {
            this.name = name;
        }
        public CommandLine(string name, params CommandElement[] args)
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

                this.parsed = true;
                return true;
            }

            return false;
        }
        public override bool TryAssign(Type type, object instance)
        {
            if (parsed)
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

                        foreach (CommandElement element in cmdElements)
                        {
                            element.TryAssign(type, instance);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void Add(CommandElement argv)
        {
            cmdElements.Add(argv);
        }
        public void Clear()
        {
            cmdElements.Clear();
        }
        public bool Contains(CommandElement argv)
        {
            return cmdElements.Contains(argv);
        }
        public void CopyTo(CommandElement[] array, int index)
        {
            cmdElements.CopyTo(array, index);
        }
        bool ICollection<CommandElement>.Remove(CommandElement item)
        {
            return cmdElements.Remove(item);
        }
        public IEnumerator<CommandElement> GetEnumerator()
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
    public class Arguments : CommandElement, CommandElementContainer
    {
        List<CommandElement> cmdElements = new List<CommandElement>();
        List<string> strContent = new List<string>();
        public CommandElement[] Elements { get => cmdElements.ToArray(); }

        public int Count => cmdElements.Count;
        public bool IsReadOnly => false;
        public bool IgnoreCase
        {
            get
            {
                bool ignoreCase = false;
                foreach (CommandElement element in cmdElements)
                    ignoreCase &= element.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (CommandElement element in cmdElements)
                    element.IgnoreCase = value;
            }
        }
        public string ExContentName { get; set; } = "ExtraContent";
        public string[] StringContent { get => strContent.ToArray(); }

        public Arguments() { }
        public Arguments(params CommandElement[] args)
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

            foreach (CommandElement element in cmdElements)
            {
                element.TryAssign(type, instance);
            }

            return true;
        }

        public void Add(CommandElement argv)
        {
            cmdElements.Add(argv);
        }
        public void Clear()
        {
            cmdElements.Clear();
        }
        public bool Contains(CommandElement argv)
        {
            return cmdElements.Contains(argv);
        }
        public void CopyTo(CommandElement[] array, int index)
        {
            cmdElements.CopyTo(array, index);
        }
        bool ICollection<CommandElement>.Remove(CommandElement item)
        {
            return cmdElements.Remove(item);
        }
        public IEnumerator<CommandElement> GetEnumerator()
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
