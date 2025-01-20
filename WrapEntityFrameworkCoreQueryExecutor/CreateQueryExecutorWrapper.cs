using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace WrapEntityFrameworkCoreQueryExecutor;

public class CreateQueryExecutorWrapper
{
	public Dictionary<Type, Delegate> m_SpecializedQueryExecutors = new();

	public delegate Func<QueryContext, TResult> SpecializedQueryExecutorDelegate<TResult>(
		Expression query,
		Func<Expression, Func<QueryContext, TResult>> baseFunc);

	public Func<QueryContext, TResult> CreateQueryExecutor<TResult>(
		Expression query,
		Func<Expression, Func<QueryContext, TResult>> baseFunc)
	{
		return this.m_SpecializedQueryExecutors.TryGetValue(typeof(TResult), out var specializedQueryExecutor)
			? ((SpecializedQueryExecutorDelegate<TResult>)specializedQueryExecutor)(query, baseFunc)
			: baseFunc(query);
	}

	public void RegisterSpecializedQueryExecutor<TResult>(
		SpecializedQueryExecutorDelegate<TResult> specializedQueryExecutor)
	{
		this.m_SpecializedQueryExecutors[typeof(TResult)] = specializedQueryExecutor;
	}
}