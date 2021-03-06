#region License
// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Selection.Heuristics;
#endregion

namespace Ninject.Selection
{
	/// <summary>
	/// Selects members for injection.
	/// </summary>
	public class Selector : NinjectComponent, ISelector
	{
		private const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Instance;

		private BindingFlags Flags
		{
			get
			{
				#if !NO_LCG && !SILVERLIGHT
				return Settings.InjectNonPublic ? (DefaultFlags | BindingFlags.NonPublic) : DefaultFlags;
				#else
				return DefaultFlags;
				#endif
			}
		}

		/// <summary>
		/// Gets or sets the constructor scorer.
		/// </summary>
		public IConstructorScorer ConstructorScorer { get; set; }

		/// <summary>
		/// Gets the property injection heuristics.
		/// </summary>
		public ICollection<IInjectionHeuristic> InjectionHeuristics { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Selector"/> class.
		/// </summary>
		/// <param name="constructorScorer">The constructor scorer.</param>
		/// <param name="injectionHeuristics">The injection heuristics.</param>
		public Selector(IConstructorScorer constructorScorer, IEnumerable<IInjectionHeuristic> injectionHeuristics)
		{
			Ensure.ArgumentNotNull(constructorScorer, "constructorScorer");
			Ensure.ArgumentNotNull(injectionHeuristics, "injectionHeuristics");

			ConstructorScorer = constructorScorer;
			InjectionHeuristics = injectionHeuristics.ToList();
		}

		/// <summary>
		/// Selects the constructor to call on the specified type, by using the constructor scorer.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The selected constructor, or <see langword="null"/> if none were available.</returns>
		public IEnumerable<ConstructorInfo> SelectConstructorsForInjection(Type type)
		{
			Ensure.ArgumentNotNull(type, "type");

			var constructors = type.GetConstructors( Flags );
			return constructors.Length == 0 ? null : constructors;
		}

		/// <summary>
		/// Selects properties that should be injected.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>A series of the selected properties.</returns>
		public IEnumerable<PropertyInfo> SelectPropertiesForInjection(Type type)
		{
			Ensure.ArgumentNotNull(type, "type");
			return type.GetProperties(Flags).Where(p => InjectionHeuristics.Any(h => h.ShouldInject(p)));
		}

		/// <summary>
		/// Selects methods that should be injected.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>A series of the selected methods.</returns>
		public IEnumerable<MethodInfo> SelectMethodsForInjection(Type type)
		{
			Ensure.ArgumentNotNull(type, "type");
			return type.GetMethods(Flags).Where(m => InjectionHeuristics.Any(h => h.ShouldInject(m)));
		}
	}
}