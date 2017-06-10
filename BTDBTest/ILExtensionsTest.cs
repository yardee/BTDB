using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;
using BTDB.IL;
using BTDB.KVDBLayer;

namespace BTDBTest
{
    public class ILExtensionsTest
    {
        public class Nested
        {
            public string PassedParam { get; private set; }

            public void Fun(string a)
            {
                PassedParam = a;
            }

            public void Fun(int noFun)
            {
                Assert.True(false, "Fail");
            }

            int PrivateProperty { get; set; }
        }

        [Fact]
        public void NoILWay()
        {
            var n = new Nested();
            n.Fun("Test");
            Assert.Equal("Test", n.PassedParam);
        }

        [Fact]
        public void ILOldWay()
        {
            var method = new DynamicMethod("SampleCall", typeof(Nested), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.DeclareLocal(typeof(Nested));
            il.Emit(OpCodes.Newobj, typeof(Nested).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldstr, "Test");
            il.Emit(OpCodes.Call, typeof(Nested).GetMethod("Fun", new[] { typeof(string) }));
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            var action = (Func<Nested>)method.CreateDelegate(typeof(Func<Nested>));
            var n = action();
            Assert.Equal("Test", n.PassedParam);
        }

        [Fact]
        public void ILNewestWayRelease()
        {
            var method = new ILBuilderRelease().NewMethod<Func<Nested>>("SampleCall");
            var il = method.Generator;
            var local = il.DeclareLocal(typeof(Nested), "n");
            il
                .Newobj(() => new Nested())
                .Dup()
                .Stloc(local)
                .Ldstr("Test")
#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                .Call(() => default(Nested).Fun(""))
#pragma warning restore CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                .Ldloc(local)
                .Ret();
            var action = method.Create();
            var n = action();
            Assert.Equal("Test", n.PassedParam);
        }

        [Fact]
        public void CanFixFirstParameterRelease()
        {
            var method = new ILBuilderRelease().NewMethod("SampleCall", typeof(Func<Nested>), typeof(string));
            var il = method.Generator;
            var local = il.DeclareLocal(typeof(Nested), "n");
            il
                .Newobj(() => new Nested())
                .Dup()
                .Stloc(local)
                .Ldarg(0)
#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                .Call(() => default(Nested).Fun(""))
#pragma warning restore CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                .Ldloc(local)
                .Ret();
            var action = (Func<Nested>)method.Create("Test");
            var n = action();
            Assert.Equal("Test", n.PassedParam);
        }

        public class PrivateConstructor
        {
            readonly int _a;

            PrivateConstructor(int a)
            {
                _a = a;
            }

            public int A => _a;
        }

        public int Factorial(int n)
        {
            var ret = n;
            while (n > 2)
                ret *= --n;
            return ret;
        }
    }
}
