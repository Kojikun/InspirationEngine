using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InspirationEngine.WPF.Utilities
{
	public static class GenericOperations<T1, T2>
    {
		static GenericOperations()
		{
			TParam1 = Expression.Parameter(typeof(T1));
			TParam2 = Expression.Parameter(typeof(T2));

			try
			{
				Equal = Expression.Lambda<Func<T1, T2, bool>>(Expression.Equal(TParam1, TParam2), TParam1, TParam2).Compile();
			}
			catch (Exception) { }
		}

		private static ParameterExpression TParam1;
		private static ParameterExpression TParam2;

		public readonly static Func<T1, T2, bool> Equal;
	}
	/// <summary>S
	/// Provides methods to invoke expressions on generic objects
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class GenericOperations<T1, T2, TReturn>
	{
		static GenericOperations()
		{
            TParam1 = Expression.Parameter(typeof(T1));
			TParam2 = Expression.Parameter(typeof(T2));

			try
			{
				Add = Expression.Lambda<Func<T1, T2, TReturn>>(Expression.Add(TParam1, TParam2), TParam1, TParam2).Compile();
			}
			catch (Exception) { }
			try
			{
				Subtract = Expression.Lambda<Func<T1, T2, TReturn>>(Expression.Subtract(TParam1, TParam2), TParam1, TParam2).Compile();
			}
			catch (Exception) { }
			try
			{
				Multiply = Expression.Lambda<Func<T1, T2, TReturn>>(Expression.Multiply(TParam1, TParam2), TParam1, TParam2).Compile();
			}
			catch (Exception) { }
		}

		private static ParameterExpression TParam1;
		private static ParameterExpression TParam2;

		public readonly static Func<T1, T2, TReturn> Add;

		public readonly static Func<T1, T2, TReturn> Subtract;

		public readonly static Func<T1, T2, TReturn> Multiply;
	}
}
