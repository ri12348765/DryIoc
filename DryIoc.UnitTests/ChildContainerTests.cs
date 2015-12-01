﻿using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ChildContainerTests
    {
        [Test]
        public void Can_resolve_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Orange));

            var child = parent.CreateFacade();
            child.Register(typeof(IJuice), typeof(FruitJuice));
            
            var juice = child.Resolve<IJuice>();

            Assert.IsInstanceOf<FruitJuice>(juice);
        }

        [Test]
        public void Can_resolve_service_wrapper_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Orange));

            var child = parent.CreateFacade();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var juice = child.Resolve<Func<IJuice>>();

            Assert.IsInstanceOf<Func<IJuice>>(juice);
        }

        [Test]
        public void Can_inject_singleton_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Mango), Reuse.Singleton);

            var child = parent.CreateFacade();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var fst = child.Resolve<IJuice>();
            var snd = child.Resolve<IJuice>();

            Assert.AreSame(fst.Fruit, snd.Fruit);
        }

        [Test]
        public void Can_inject_current_scope_service_from_fallback_container()
        {
            var parent = new Container();
            parent.Register<IFruit, Mango>(Reuse.InCurrentScope);

            var child = parent.CreateFacade();
            child.Register<IJuice, FruitJuice>();

            using (var childScope = child.OpenScope())
            {
                var juice = childScope.Resolve<IJuice>();
                Assert.IsInstanceOf<Mango>(juice.Fruit);
            }

            Assert.Throws<ContainerException>(() => 
            child.Resolve<IJuice>());
        }

        [Test]
        public void Can_fallback_to_parent_and_return_back_to_child()
        {
            var container = new Container();
            container.Register<FruitJuice>();
            container.Register<IFruit, Melon>();

            var childContainer = container.CreateFacade();
            childContainer.Register<IFruit, Orange>();

            Assert.IsInstanceOf<Melon>(container.Resolve<FruitJuice>().Fruit);
            Assert.IsInstanceOf<Orange>(childContainer.Resolve<FruitJuice>().Fruit);
        }

        [Test]
        public void Child_may_throw_if_parent_disposed()
        {
            var container = new Container();
            container.Register<FruitJuice>();
            container.Register<IFruit, Melon>();

            var childContainer = container.CreateFacade();
            childContainer.Register<IFruit, Orange>();

            container.Dispose();

            var ex = Assert.Throws<ContainerException>(() =>
            childContainer.Resolve<FruitJuice>());

            Assert.AreEqual(Error.ContainerIsDisposed, ex.Error);
        }

        [Test]
        public void Attach_multiple_parents_with_single_rule()
        {
            IContainer parent = new Container();
            parent.Register<FruitJuice>();

            IContainer anotherParent = new Container();
            anotherParent.Register<IFruit, Melon>();

            var container = new Container();

            var childContainer = container.With(rules => rules
                .WithFallbackContainer(parent)
                .WithFallbackContainer(anotherParent));

            Assert.IsInstanceOf<Melon>(childContainer.Resolve<FruitJuice>().Fruit);
        }

        [Test]
        public void Parent_attachment_should_not_affect_original_container()
        {
            IContainer parent = new Container();
            parent.Register<FruitJuice>();
            parent.Register<IFruit, Melon>();

            var container = new Container();
            container.Register<IFruit, Orange>();

            var childContainer = container.With(rules => rules.WithFallbackContainer(parent));

            Assert.IsInstanceOf<Melon>(parent.Resolve<FruitJuice>().Fruit);
            Assert.IsInstanceOf<Orange>(childContainer.Resolve<FruitJuice>().Fruit);

            Assert.Throws<ContainerException>(() => 
                container.Resolve<FruitJuice>());
        }

        [Test]
        public void Can_detach_parent()
        {
            IContainer parent = new Container();
            parent.Register<FruitJuice>();
            parent.Register<IFruit, Melon>();

            var container = new Container();
            container.Register<IFruit, Orange>();

            var childContainer = container.With(rules => rules.WithFallbackContainer(parent));

            Assert.IsInstanceOf<Melon>(parent.Resolve<FruitJuice>().Fruit);
            Assert.IsInstanceOf<Orange>(childContainer.Resolve<FruitJuice>().Fruit);

            var detachedChild = childContainer.With(rules => rules.WithoutFallbackContainer(parent));

            Assert.Throws<ContainerException>(() =>
                detachedChild.Resolve<FruitJuice>());
        }

        [Test]
        public void Without_singletons_should_work_with()
        {
            var container = new Container();
            container.Register<Melon>(Reuse.Singleton);
            var melon = container.Resolve<Melon>();

            var withoutSingletons = container.WithoutSingletonsAndCache(); // automatically drop cache
            var melonAgain = withoutSingletons.Resolve<Melon>();

            Assert.AreNotSame(melon, melonAgain);
        }

        [Test]
        public void With_registration_copy()
        {
            var container = new Container();
            container.Register<IFruit, Melon>();
            var melon = container.Resolve<IFruit>();
            Assert.IsInstanceOf<Melon>(melon);

            var withRegistrationsCopy = container.WithRegistrationsCopy().WithoutCache();
            withRegistrationsCopy.Register<IFruit, Orange>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var orange = withRegistrationsCopy.Resolve<IFruit>();
            Assert.IsInstanceOf<Orange>(orange);
        }

        [Test] // #107
        public void Reusing_singletons_from_parent_and_not_disposing_them_in_scoped_container()
        {
            var container = new Container();

            var parent = container.OpenScope("parent");
            parent.Register<Foo>(Reuse.InCurrentNamedScope("parent"));

            var firstChild = parent.OpenScope();
            var firstFoo = firstChild.Resolve<Foo>();

            firstChild.Register<Blah>(Reuse.InCurrentScope);
            var firstBlah = firstChild.Resolve<Blah>();

            firstChild.Dispose();

            Assert.IsFalse(firstFoo.Disposed); // firstFoo shouldn't be disposed
            Assert.IsTrue(firstBlah.Disposed); // firstBlah should be disposed

            var secondChild = parent.OpenScope();
            secondChild.Resolve<Foo>(); // Resolve<Foo>() shouldn't throw

            parent.Dispose();   // Parent scope is disposed.
            Assert.IsTrue(firstFoo.Disposed); 

            container.Dispose(); // singletons, registry, cache, is gone
        }

        [Test]
        public void Can_resolve_instance_from_fallback_container_If_instance_registered_as_delegate()
        {
            var container = new Container();

            container.RegisterDelegate(_ => "a");
            container.RegisterDelegate(_ => "a2", ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var facade = container.CreateFacade();

            var str = facade.Resolve<string>();
            Assert.AreEqual("a2", str);
        }

        [Test]
        public void Can_produce_container_which_throws_on_further_registrations()
        {
            var c = new Container();
            c.Register<Foo>(Reuse.Singleton);

            var frozen = c.WithNoMoreRegisrationAllowed();

            var ex = Assert.Throws<ContainerException>(() => 
                frozen.Register<Blah>(Reuse.Singleton));

            Assert.AreEqual(Error.NoMoreRegistrationsAllowed, ex.Error);
        }

        [Test]
        public void Can_produce_container_which_throws_on_further_unregistrations()
        {
            var c = new Container();
            c.Register<Foo>(Reuse.Singleton);

            var frozen = c.WithNoMoreRegisrationAllowed();

            var ex = Assert.Throws<ContainerException>(() =>
                frozen.Unregister<Blah>());

            Assert.AreEqual(Error.NoMoreUnregistrationsAllowed, ex.Error);
        }
        
        #region CUT

        internal class Foo : IDisposable
        {
            public bool Disposed { get; set; }
            public void Dispose()
            {
                Disposed = true;
            }
        }

        internal class Blah : IDisposable
        {
            public bool Disposed { get; set; }
            public void Dispose()
            {
                Disposed = true;
            }
        }


        public interface IFruit { }
        public class Orange : IFruit { }
        public class Mango : IFruit { }
        public class Melon : IFruit { }

        public interface IJuice
        {
            IFruit Fruit { get; }
        }

        public class FruitJuice : IJuice
        {
            public IFruit Fruit { get; set; }

            public FruitJuice(IFruit fruit)
            {
                Fruit = fruit;
            }
        }

        #endregion
    }
}