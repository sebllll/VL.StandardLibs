﻿using MessagePack;
using NUnit.Framework;
using VL.Core;
using VL.Core.CompilerServices;
using VL.TestFramework;

namespace VL.Serialization.MessagePack.Tests
{
    [TestFixture]
    public class MessagePackTests
    {
        private TestAppHost appHost;

        [SetUp]
        public void Setup()
        {
            appHost = new TestAppHost();
            appHost.MakeCurrent().DisposeBy(appHost);
        }

        [TearDown]
        public void TearDown()
        {
            appHost.Dispose();
        }

        [Test]
        public void TypelessSerialization()
        {
            var myClass = new MyClass();
            var content = MessagePackSerialization.Serialize<object>(myClass);
            Assert.IsInstanceOf<MyClass>(MessagePackSerialization.Deserialize<object>((IEnumerable<byte>)content));
        }

        [Test]
        public void TypelessNestedSerialization()
        {
            var myClass = new MyClass2() { NestedProperty = new MyClass3() };
            var content = MessagePackSerialization.Serialize<object>(myClass);
            var result = (MyClass2)MessagePackSerialization.Deserialize<object>((IEnumerable<byte>)content);
            Assert.IsInstanceOf<MyClass3>(result.NestedProperty);
        }

        [Test]
        public void TypelessNestedPatchedInterfaceTypeSerialization()
        {
            var myClass = new MyClass5() { NestedProperty = new MyClass4() };
            var content = MessagePackSerialization.Serialize<object>(myClass);
            var result = (MyClass5)MessagePackSerialization.Deserialize<object>((IEnumerable<byte>)content);
            Assert.IsInstanceOf<MyClass4>(result.NestedProperty);
        }

        [MessagePackObject]
        public class MyClass
        {

        }

        [MessagePackObject]
        public class MyClass2
        {
            [Key(0)]
            public IMyInterface? NestedProperty { get; set; }
        }

        [MessagePackObject]
        public class MyClass3 : IMyInterface
        {

        }

        public class MyClass4 : IMyInterface2
        {
            [CreateNew]
            public static MyClass4 CreateNew(NodeContext ctx) => new MyClass4();

            public AppHost AppHost => throw new NotImplementedException();

            public NodeContext Context => throw new NotImplementedException();

            public uint Identity => throw new NotImplementedException();

            public IVLObject With(IReadOnlyDictionary<string, object> values)
            {
                return this;
            }
        }

        [MessagePackObject]
        public class MyClass5
        {
            [Key(0)]
            public IMyInterface2? NestedProperty { get; set; }
        }

        public interface IMyInterface { }

        public interface IMyInterface2 : IVLObject { }
    }
}
