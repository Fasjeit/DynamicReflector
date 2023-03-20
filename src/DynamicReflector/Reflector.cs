using DynamicReflector.Extensions;
using DynamicReflector.Harmony.Tools;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;

namespace DynamicReflector
{
    public class Reflector : DynamicObject, IDisposable
    {
        private readonly object? obj = null;

        private readonly Traverse objectTraverse;

        private bool disposedValue;

        /// <summary>
        /// Creates <see cref="Reflector"/> instance for an object
        /// </summary>
        /// <param name="obj">
        /// Object to be reflected
        /// </param>
        public Reflector(object obj)
        {
            this.obj = obj;
            this.objectTraverse = Traverse.Create(obj);
        }

        /// <summary>
        /// Creates <see cref="Reflector"/> instance for an object <see cref="Traverse"/>
        /// </summary>
        /// <param name="obj">
        /// Object to be reflected
        /// </param>
        private Reflector(Traverse obj)
        {
            this.objectTraverse = obj;
        }

        /// <summary>
        /// Get the original object
        /// </summary>
        public object? OriginalObject => this.obj;

        /// <summary>
        /// Creates new object using constructor.
        /// </summary>
        /// <param name="type">
        /// Type of the object to be created.
        /// </param>
        /// <param name="args">
        /// Construtor arguments.
        /// </param>
        /// <returns>
        /// Reflector for the created object.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public static Reflector Create(Type type, params object?[]? args)
        {
            var obj = Activator.CreateInstance(
                type: type,
                bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                args: args,
                culture : null);
            if (obj == null)
            {
                throw new ArgumentException("Cannot create object");
            }
            return new Reflector(obj);
        }

        /// <summary>
        /// Creates new object using constructor.
        /// </summary>
        /// <param name="typeName">
        /// Type of the object to be created.
        /// </param>
        /// <param name="assembly">
        /// Assembly containing the type.
        /// </param>
        /// <param name="args">
        /// Construtor arguments.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Reflector Create(string typeName, Assembly assembly, params object?[]? args)
        {
            var type = assembly.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException("Type not found");
            }
            var obj = Activator.CreateInstance(
                type: type,
                bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                args: args,
                culture: null);
            if (obj == null)
            {
                throw new ArgumentException("Cannot create object");
            }
            return new Reflector(obj);
        }

        /// <summary>
        /// Creates a felector for a static class.
        /// </summary>
        /// <param name="type">
        /// Type of the static class.
        /// </param>
        /// <returns></returns>
        public static Reflector CreateStatic(Type type)
        {
            var traverse = Traverse.Create(type);
            var reflector = new Reflector(traverse);
            return reflector;
        }

        /// <summary>
        /// Creates a felector for a static class.
        /// </summary>
        /// <param name="typeName">
        /// Type of the static class.
        /// </param>
        /// <param name="assembly">
        /// Assembly containing the type.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Reflector CreateStatic(string typeName, Assembly assembly)
        {
            var type = assembly.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException("Type not found");
            }
            return Reflector.CreateStatic(type);
        }

        public override bool TryCreateInstance(
            CreateInstanceBinder binder,
            object?[]? args,
            [NotNullWhen(true)] out object? result)
        {
            return base.TryCreateInstance(binder, args, out result);
        }

        // Implement the TryGetMember method of the DynamicObject class for dynamic member calls.
        public override bool TryGetMember(
            GetMemberBinder binder,
            out object? result)
        {
            var filed = this.objectTraverse.Field(binder.Name);
            if (filed.FieldExists())
            {
                result = filed.GetValue().ToReflector();
                return true;
            }

            var property = this.objectTraverse.Property(binder.Name);
            if (property.PropertyExists())
            {
                result = property.GetValue().ToReflector();
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryInvokeMember(
            InvokeMemberBinder binder,
            object?[]? args,
            out object? result)
        {
            var method = this.objectTraverse.Method(binder.Name, args);
            if (!method.MethodExists())
            {
                result = null;
                return false;
            }
            result = method.GetValue(args)?.ToReflector();
            return true;
        }

        public override bool TryInvoke(
            InvokeBinder binder,
            object?[]? args,
            out object? result)
        {
            return base.TryInvoke(binder, args, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            var filed = this.objectTraverse.Field(binder.Name);
            if (filed.FieldExists())
            {
                filed.SetValue(value);
                return true;
            }

            var property = this.objectTraverse.Property(binder.Name);
            if (property.PropertyExists())
            {
                property.SetValue(value);
                return true;
            }

            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.obj is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
