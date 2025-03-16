using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace WrapEntityFrameworkCoreQueryExecutor;

internal class TestQueryCompilationContext(
	QueryCompilationContextDependencies dependencies,
	bool async,
	CreateQueryExecutorWrapper wrapper)
	: QueryCompilationContext(dependencies, async)
{
	public override Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
		=> wrapper.CreateQueryExecutor(this, query, base.CreateQueryExecutor<TResult>);
}