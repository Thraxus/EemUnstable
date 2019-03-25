using System;
using System.Collections.Generic;
using System.Linq;

namespace Eem.Thraxus.Helpers
{
	/// <summary>
	/// Provides a set of methods to fix some of the LINQ idiocy.
	/// <para/>
	/// Enjoy your allocations.
	/// </summary>
	public static class GenericHelpers
	{
		//public static List<T> Except<T>(this List<T> source, Func<T, bool> sorter)
		//{
		//	return source.Where(x => !sorter(x)).ToList();
		//}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			HashSet<T> hashset = new HashSet<T>();
			foreach (T item in source)
				hashset.Add(item);
			return hashset;
		}

		/// <summary>
		/// Returns a list with one item excluded.
		/// </summary>
		public static List<T> Except<T>(this List<T> source, T exclude)
		{
			return source.Where(x => !x.Equals(exclude)).ToList();
		}

		//public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> sorter, out IEnumerable<T> any)
		//{
		//	any = source.Where(sorter);
		//	return any.Any();
		//}

		/// <summary>
		/// Determines if the sequence has no elements matching a given predicate.
		/// <para />
		/// Basically, it's an inverted Any().
		/// </summary>
		//public static bool None<T>(this IEnumerable<T> source, Func<T, bool> sorter)
		//{
		//	return !source.Any(sorter);
		//}

		//public static IEnumerable<T> Unfitting<T>(this IEnumerable<T> source, Func<T, bool> sorter)
		//{
		//	return source.Where(x => sorter(x) == false);
		//}

		//public static List<T> Unfitting<T>(this List<T> source, Func<T, bool> sorter)
		//{
		//	return source.Where(x => sorter(x) == false).ToList();
		//}

		public static bool Any<T>(this List<T> source, Func<T, bool> sorter, out List<T> any)
		{
			any = source.Where(sorter).ToList();
			return any.Count > 0;
		}

		public static bool Empty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}
	}
}