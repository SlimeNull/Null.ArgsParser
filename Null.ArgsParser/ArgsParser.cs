﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Null.ArgsParser
{
    public enum CommandElementType
    {
        SwitchArgument,
        FieldArgument,
        PropertyArgument,
        StringArgument,
        CommandLine,
        Arguments,

        NotArgument
    }

    public interface INamedArgument : ICommandElement
    {
        string Name { get; }
        CommandElementType ArgumentType { get; }
    }
    public interface ICaseIgnorableArgument : INamedArgument
    {
        bool IgnoreCase { get; set; }
    }
    public interface ICommandElement
    {
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
    public class SwitchArgument : ICaseIgnorableArgument
    {
        string name = string.Empty;
        bool ignoreCase = false;

        char triggerChar = '/';
        bool enabled = false;
        bool assignable = false;

        public  string Name { get => name; }
        public  bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public  CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
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

        public  bool IsTriggered(string txt)
        {
            if (ignoreCase)
                return txt.ToUpper().Equals(triggerChar + name.ToUpper());
            else
                return txt.Equals(triggerChar + name);
        }
        public  bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                this.Enabled = !enabled;

                index++;
                return true;
            }

            return false;
        }
        public  bool TryAssign(Type type, object instance)
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
    public class PropertyArgument : ICaseIgnorableArgument
    {
        string name = string.Empty;
        bool ignoreCase = false;

        char triggerChar = '-';
        string value = string.Empty;
        bool assignable = false;

        public  string Name { get => name; }
        public  bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public  CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
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

        public  bool IsTriggered(string txt)
        {
            if (ignoreCase)
                return txt.ToUpper().Equals(triggerChar + name.ToUpper());
            else
                return txt.Equals(triggerChar + name);
        }
        public  bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                if (index + 1 < args.Length)
                {
                    this.Value = args[++index];      // 包含 index + 1 的操作

                    index++;
                    return true;
                }
            }

            return false;
        }
        public  bool TryAssign(Type type, object instance)
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
    public class FieldArgument : ICaseIgnorableArgument
    {
        string name = string.Empty;
        bool ignoreCase = false;
        CommandElementType type = CommandElementType.FieldArgument;

        char triggerChar = '=';
        string value = string.Empty;
        bool assignable = false;

        public  string Name { get => name; }
        public  bool IgnoreCase { get => ignoreCase; set => ignoreCase = value; }
        public  CommandElementType ArgumentType { get; } = CommandElementType.SwitchArgument;
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

        public  bool IsTriggered(string txt)
        {
            if (ignoreCase)
                return txt.ToUpper().StartsWith(name.ToUpper() + triggerChar);
            else
                return txt.StartsWith(name + triggerChar);
        }
        public  bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                this.Value = args[index].Substring(name.Length + 1);

                index++;
                return true;
            }

            return false;
        }
        public  bool TryAssign(Type type, object instance)
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
    public class StringArgument : INamedArgument
    {
        bool parsed = false;
        bool assignable = false;
        string name;
        string value;
        public string Name => name;
        public string Value
        {
            get => value; 
            set
            {
                this.assignable = true;
                this.value = value; 
            } 
        }

        public CommandElementType ArgumentType => CommandElementType.StringArgument;

        public StringArgument() { }
        public StringArgument(string name)
        {
            this.name = name;
        }
        public StringArgument(string name, string value)
        {
            this.name = name;
            this.Value = value;
        }
        public  bool IsTriggered(string txt)
        {
            return true;
        }

        public  bool TryAssign(Type type, object instance)
        {
            if (assignable)
            {
                FieldInfo fieldInfo = type.GetField(Name);
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

        public  bool TryParse(ref string[] args, ref int index)
        {
            if (assignable && parsed)
                return false;

            if (index < args.Length)
                Value = args[index];
            else
                return false;

            index++;
            return parsed = true;
        }
    }
    public class CommandLine : INamedArgument, ICaseIgnorableArgument, ICommandElementContainer
    {
        string name = string.Empty;
        bool selfIgnoreCase = false;
        bool enabled;
        bool assignable = false;

        List<ICommandElement> cmdElements = new List<ICommandElement>();
        List<string> strContent = new List<string>();

        public  string Name { get => name; }
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
                    if (typeof(ICaseIgnorableArgument).IsAssignableFrom(element.GetType()))
                        ignoreCase &= (element as ICaseIgnorableArgument).IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (ICommandElement element in cmdElements)
                    if (typeof(ICaseIgnorableArgument).IsAssignableFrom(element.GetType()))
                        (element as ICaseIgnorableArgument).IgnoreCase = value;
            }
        }
        public bool IgnoreCase
        {
            get
            {
                bool ignoreCase = true;
                foreach (ICaseIgnorableArgument argv in cmdElements.OfType<ICaseIgnorableArgument>())
                    ignoreCase &= argv.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (ICaseIgnorableArgument argv in cmdElements.OfType<ICaseIgnorableArgument>())
                    argv.IgnoreCase = value;
            }
        }

        public CommandElementType ArgumentType => CommandElementType.CommandLine;

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

        public  bool IsTriggered(string txt)
        {
            if (selfIgnoreCase)
                return txt.ToUpper().StartsWith(name.ToUpper());
            else
                return txt.StartsWith(name);
        }
        public  bool TryParse(ref string[] args, ref int index)
        {
            if (IsTriggered(args[index]))
            {
                index++;
                enabled = true;

                bool parsed;
                for (; index < args.Length;)
                {
                    parsed = false;
                    for (int j = 0; j < cmdElements.Count && index < args.Length; j++)
                    {
                        bool thisParsed = cmdElements[j].TryParse(ref args, ref index);
                        parsed |= thisParsed;

                        if (thisParsed)
                            break;
                    }
                    if (!parsed)
                    {
                        strContent.Add(args[index]);
                        index++;
                    }
                }

                this.assignable = true;
                return true;
            }

            return false;
        }
        public  bool TryAssign(Type type, object instance)
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

                        foreach (ICommandElement element in cmdElements)
                        {
                            element.TryAssign(type, instance);
                        }

                        return true;
                    }
                }
                if (strCntntFieldInfo != null)
                {
                    if (strCntntFieldInfo.FieldType.IsAssignableFrom(typeof(List<string>)))
                        strCntntFieldInfo.SetValue(instance, strContent);
                    else if (typeof(string[]) == strCntntFieldInfo.FieldType)
                        strCntntFieldInfo.SetValue(instance, strContent.ToArray());
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
                bool ignoreCase = true;
                foreach (ICaseIgnorableArgument argv in cmdElements.OfType<ICaseIgnorableArgument>())
                    ignoreCase &= argv.IgnoreCase;
                return ignoreCase;
            }
            set
            {
                foreach (ICaseIgnorableArgument argv in cmdElements.OfType<ICaseIgnorableArgument>())
                    argv.IgnoreCase = value;
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
            for (; index < args.Length;)
            {
                parsed = false;
                for (int j = 0; j < cmdElements.Count && index < args.Length; j++)
                {
                    bool thisParsed = cmdElements[j].TryParse(ref args, ref index);
                    parsed |= thisParsed;

                    if (thisParsed)
                        break;
                }
                if (!parsed)
                {
                    strContent.Add(args[index]);
                    index++;
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
