﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Query;
using LinqToDB.Expressions;

namespace LinqToDB.EntityFrameworkCore.Internal
{
	using Async;
	using Linq;

	/// <summary>
	///     Adapter for <see cref="IAsyncQueryProvider" />
	///		This is internal API and is not intended for use by Linq To DB applications.
	///		It may change or be removed without further notice.
	/// </summary>
	/// <typeparam name="T">Type of query element.</typeparam>
	public class LinqToDBForEFQueryProvider<T> : IAsyncQueryProvider, IQueryProviderAsync, IQueryable<T>, IAsyncEnumerable<T>
	{
		/// <summary>
		/// Creates instance of adapter.
		/// </summary>
		/// <param name="dataContext">Data context instance.</param>
		/// <param name="expression">Query expression.</param>
		public LinqToDBForEFQueryProvider(IDataContext dataContext, Expression expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));
			var dataContext1 = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
			QueryProvider = (IQueryProviderAsync) Internals.CreateExpressionQueryInstance<T>(dataContext1, expression);
			QueryProviderAsQueryable = (IQueryable<T>) QueryProvider;
		}

		IQueryProviderAsync QueryProvider { get; }
		IQueryable<T> QueryProviderAsQueryable { get; }

		/// <summary>
		/// Creates <see cref="IQueryable"/> instance from query expression.
		/// </summary>
		/// <param name="expression">Query expression.</param>
		/// <returns><see cref="IQueryable"/> instance.</returns>
		public IQueryable CreateQuery(Expression expression)
		{
			return QueryProvider.CreateQuery(expression);
		}

		/// <summary>
		/// Creates <see cref="IQueryable{T}"/> instance from query expression.
		/// </summary>
		/// <typeparam name="TElement">Query element type.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <returns><see cref="IQueryable{T}"/> instance.</returns>
		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return QueryProvider.CreateQuery<TElement>(expression);
		}

		/// <summary>
		/// Executes query expression.
		/// </summary>
		/// <param name="expression">Query expression.</param>
		/// <returns>Query result.</returns>
		public object? Execute(Expression expression)
		{
			return QueryProvider.Execute(expression);
		}

		/// <summary>
		/// Executes query expression and returns typed result.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <returns>Query result.</returns>
		public TResult Execute<TResult>(Expression expression)
		{
			return QueryProvider.Execute<TResult>(expression);
		}

		private static readonly MethodInfo _executeAsyncMethodInfo =
			MemberHelper.MethodOf((IQueryProviderAsync p) => p.ExecuteAsync<int>(null!, default)).GetGenericMethodDefinition();

		/// <summary>
		/// Executes query expression and returns result as <see cref="IAsyncEnumerable{T}"/> value.
		/// </summary>
		/// <typeparam name="TResult">Type of result element.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result as <see cref="IAsyncEnumerable{T}"/>.</returns>
		public Task<IAsyncEnumerable<TResult>> ExecuteAsyncEnumerable<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			return QueryProvider.ExecuteAsyncEnumerable<TResult>(expression, cancellationToken);
		}

		/// <summary>
		/// Executes query expression and returns typed result.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result.</returns>
		TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			var item = typeof(TResult).GetGenericArguments()[0];
			var method = _executeAsyncMethodInfo.MakeGenericMethod(item);
			return (TResult) method.Invoke(QueryProvider, new object[] { expression, cancellationToken })!;
		}

		/// <summary>
		/// Executes query expression and returns typed result.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">Query expression.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result.</returns>
		public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
		{
			return QueryProvider.ExecuteAsync<TResult>(expression, cancellationToken);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return QueryProviderAsQueryable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return QueryProviderAsQueryable.GetEnumerator();
		}

		#region IQueryable

		/// <summary>
		/// Type of query element.
		/// </summary>
		public Type ElementType => typeof(T);

		/// <summary>
		/// Query expression.
		/// </summary>
		public Expression Expression => QueryProviderAsQueryable.Expression;

		/// <summary>
		/// Query provider.
		/// </summary>
		public IQueryProvider Provider => this;

		#endregion

		/// <summary>
		/// Gets <see cref="IAsyncEnumerable{T}"/> for current query.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Query result as <see cref="IAsyncEnumerable{T}"/>.</returns>
		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			return Task.Run(() => QueryProvider.ExecuteAsyncEnumerable<T>(Expression, cancellationToken),
				cancellationToken).Result.GetAsyncEnumerator(cancellationToken);
		}

		/// <summary>
		/// Returns generated SQL for specific LINQ query.
		/// </summary>
		/// <returns>Generated SQL.</returns>
		public override string? ToString()
		{
			return QueryProvider.ToString();
		}
	}
}
