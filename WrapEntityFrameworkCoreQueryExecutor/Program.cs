using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WrapEntityFrameworkCoreQueryExecutor;

var createQueryExecutorWrapper = new CreateQueryExecutorWrapper();

var dbContextOptionsBuilder = new DbContextOptionsBuilder<SandboxDbContext>()
	.UseInMemoryDatabase("Test");

var serviceProvider = new ServiceCollection()
	.AddEntityFrameworkInMemoryDatabase()
	.Replace(ServiceDescriptor.Scoped<IQueryCompilationContextFactory, TestQueryCompilationContextFactory>())
	.AddKeyedScoped(dbContextOptionsBuilder.Options, (_, _) => createQueryExecutorWrapper)
	.BuildServiceProvider();

dbContextOptionsBuilder.UseInternalServiceProvider(serviceProvider);

using var dbContext = new SandboxDbContext(dbContextOptionsBuilder.Options);

createQueryExecutorWrapper.RegisterSpecializedQueryExecutor<IAsyncEnumerable<Table1>>(
	(queryExp, baseFunc) => queryExp is MethodCallExpression methodCall
		&& methodCall.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking)
		&& methodCall.Arguments[0] is FromSqlQueryRootExpression sqlQueryExp
		? new Func<QueryContext, IAsyncEnumerable<Table1>>(
			queryContext => new[] {
				new Table1
				{
					Id = 2,
					Name = "Test"
				}
			}.ToAsyncEnumerable())
		: baseFunc(queryExp));

createQueryExecutorWrapper.RegisterSpecializedQueryExecutor<IAsyncEnumerable<Table1>>(
	(queryExp, baseFunc) => queryExp is MethodCallExpression methodCall
		&& methodCall.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking)
		&& methodCall.Arguments[0] is FromSqlQueryRootExpression sqlQueryExp
		? new Func<QueryContext, IAsyncEnumerable<Table1>>(
			queryContext => new[] {
				new Table1
				{
					Id = 1,
					Name = "Test"
				}
			}.ToAsyncEnumerable())
		: baseFunc(queryExp));


var rows = await dbContext.Table1
	.FromSqlInterpolated($"SELECT * FROM Table1 WHERE Id = {1}")
	.AsNoTracking()
	.ToArrayAsync();

Console.WriteLine(rows.Count());
Console.WriteLine(rows.First().Id);
