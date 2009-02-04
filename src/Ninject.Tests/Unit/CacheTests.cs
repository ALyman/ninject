﻿using System;
using Moq;
using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Planning.Bindings;
using Ninject.Tests.Fakes;
using Xunit;

namespace Ninject.Tests.Unit.CacheTests
{
	public class CacheContext
	{
		protected Mock<IPipeline> activatorMock;
		protected Mock<ICachePruner> prunerMock;
		protected Mock<IContext> contextMock;
		protected Mock<IBinding> bindingMock;
		protected Cache cache;

		public CacheContext()
		{
			activatorMock = new Mock<IPipeline>();
			prunerMock = new Mock<ICachePruner>();
			contextMock = new Mock<IContext>();
			bindingMock = new Mock<IBinding>();
			cache = new Cache(activatorMock.Object, prunerMock.Object);
		}
	}

	public class WhenCacheIsCreated
	{
		[Fact]
		public void AsksPrunerToStartPruning()
		{
			var activatorMock = new Mock<IPipeline>();
			var prunerMock = new Mock<ICachePruner>();

			prunerMock.Setup(x => x.StartPruning(It.IsAny<Cache>())).AtMostOnce();

			var cache = new Cache(activatorMock.Object, prunerMock.Object);

			prunerMock.Verify(x => x.StartPruning(cache));
		}
	}

	public class WhenCacheIsDisposed : CacheContext
	{
		[Fact]
		public void AsksPrunerToStopPruning()
		{
			prunerMock.Setup(x => x.StopPruning()).AtMostOnce();
			cache.Dispose();
			prunerMock.Verify(x => x.StopPruning());
		}
	}

	public class WhenTryGetInstanceIsCalled : CacheContext
	{
		[Fact]
		public void ReturnsNullIfNoInstancesHaveBeenAddedToCache()
		{
			var scope = new object();
			object instance = cache.TryGet(bindingMock.Object, scope);
			Assert.Null(instance);
		}

		[Fact]
		public void ReturnsCachedInstanceIfOneHasBeenAddedWithinSpecifiedScope()
		{
			var scope = new object();
			var sword = new Sword();

			contextMock.SetupGet(x => x.Binding).Returns(bindingMock.Object);
			contextMock.SetupGet(x => x.Instance).Returns(sword);
			contextMock.Setup(x => x.GetScope()).Returns(scope);

			cache.Remember(contextMock.Object);

			object instance = cache.TryGet(bindingMock.Object, scope);
			Assert.Same(sword, instance);
		}

		[Fact]
		public void ReturnsNullIfNoInstancesHaveBeenAddedWithinSpecifiedScope()
		{
			var scope = new object();
			var sword = new Sword();

			contextMock.SetupGet(x => x.Binding).Returns(bindingMock.Object);
			contextMock.SetupGet(x => x.Instance).Returns(sword);
			contextMock.Setup(x => x.GetScope()).Returns(scope);

			cache.Remember(contextMock.Object);

			object instance = cache.TryGet(bindingMock.Object, new object());
			Assert.Null(instance);
		}

		[Fact]
		public void ReturnsNullIfScopeHasBeenGarbageCollected()
		{
			var scope = new object();
			var sword = new Sword();

			contextMock.SetupGet(x => x.Binding).Returns(bindingMock.Object);
			contextMock.SetupGet(x => x.Instance).Returns(sword);
			contextMock.Setup(x => x.GetScope()).Returns(scope);

			cache.Remember(contextMock.Object);

			object instance = cache.TryGet(bindingMock.Object, scope);
			Assert.Same(sword, instance);

			scope = null;
			instance = null;

			GC.Collect();

			instance = cache.TryGet(bindingMock.Object, scope);
			Assert.Null(instance);
		}
	}
}