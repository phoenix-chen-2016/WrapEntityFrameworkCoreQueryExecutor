using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace WrapEntityFrameworkCoreQueryExecutor;

internal class TestQueryCompilationContextFactory(
	QueryCompilationContextDependencies dependencies,
	IServiceProvider serviceProvider)
	: IQueryCompilationContextFactory
{
	public QueryCompilationContext Create(bool async)
		=> new TestQueryCompilationContext(
			dependencies,
			async,
			serviceProvider.GetRequiredKeyedService<CreateQueryExecutorWrapper>(dependencies.ContextOptions));
}
