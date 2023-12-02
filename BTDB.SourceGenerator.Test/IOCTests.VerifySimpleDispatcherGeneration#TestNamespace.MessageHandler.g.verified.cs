﻿//HintName: TestNamespace.MessageHandler.g.cs
// <auto-generated/>
using System;
using System.Runtime.CompilerServices;

namespace TestNamespace;

static file class MessageHandlerRegistration
{
    [ModuleInitializer]
    internal static unsafe void Register4BTDB()
    {
        BTDB.IOC.IContainer.RegisterFactory(typeof(global::TestNamespace.MessageHandler), (container, ctx) =>
        {
            return (container2, ctx2) =>
            {
                var res = new global::TestNamespace.MessageHandler();
                return res;
            };
        });
        global::TestNamespace.IDispatcher.ConsumeHandlers.GetOrAddValueRef(typeof(global::TestNamespace.Message).TypeHandle.Value).ExecuteFactory = (BTDB.IOC.IContainer c) => {
           var nestedFactory = c.CreateFactory(typeof(global::TestNamespace.MessageHandler));
           return (container, message) =>
           {
               var res = nestedFactory(container, null);
               Unsafe.As<global::TestNamespace.MessageHandler>(res).Consume(Unsafe.As<global::TestNamespace.Message>(message));
               return null;
           };
        };
    }
}
