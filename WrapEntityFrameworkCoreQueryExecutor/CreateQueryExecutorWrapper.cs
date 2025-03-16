using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace WrapEntityFrameworkCoreQueryExecutor;

public class CreateQueryExecutorWrapper
{
	public List<(Type Type, Delegate Func)> m_SpecializedQueryExecutors = [];

	public delegate Func<QueryContext, TResult> SpecializedQueryExecutorDelegate<TResult>(
		QueryCompilationContext queryCompilationContext,
		Expression query,
		Func<Expression, Func<QueryContext, TResult>> baseFunc);

	public Func<QueryContext, TResult> CreateQueryExecutor<TResult>(
		QueryCompilationContext queryCompilationContext,
		Expression query,
		Func<Expression, Func<QueryContext, TResult>> baseFunc)
	{
		var currentBaseFunc = baseFunc;
		foreach (var t in m_SpecializedQueryExecutors.Where(t => t.Type == typeof(TResult)).Reverse())
		{
			var queryExecutorDelegate = (SpecializedQueryExecutorDelegate<TResult>)t.Func;
			currentBaseFunc = q => queryExecutorDelegate(queryCompilationContext, q, currentBaseFunc);
		}

		return currentBaseFunc(query);
	}

	public void RegisterSpecializedQueryExecutor<TResult>(
		SpecializedQueryExecutorDelegate<TResult> specializedQueryExecutor)
	{
		this.m_SpecializedQueryExecutors.Add((typeof(TResult), specializedQueryExecutor));
	}
}