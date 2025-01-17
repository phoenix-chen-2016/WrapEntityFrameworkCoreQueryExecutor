using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.Core;
using WrapEntityFrameworkCoreQueryExecutor;

var dbContextOptions = new DbContextOptionsBuilder<SandboxDbContext>()
	.UseInMemoryDatabase("Test")
	.ReplaceService<IQueryCompilationContextFactory, TestQueryCompilationContextFactory>()
	.Options;
using var dbContext = new SandboxDbContext(dbContextOptions);

var fakeCreateQueryExecutorWrapper = Substitute.For<CreateQueryExecutorWrapper>();

TestQueryCompilationContextFactory.Services
	.AddKeyedScoped(
		dbContextOptions,
		(sp, _) => fakeCreateQueryExecutorWrapper);

fakeCreateQueryExecutorWrapper.CreateQueryExecutor(
	Arg.Any<Expression>(),
	Arg.Any<Func<Expression, Func<QueryContext, IAsyncEnumerable<Table1>>>>())
	.Returns((CallInfo callInfo) =>
	{
		var queryExp = callInfo.Arg<Expression>();
		var baseFunc = callInfo.Arg<Func<Expression, Func<QueryContext, IAsyncEnumerable<Table1>>>>();

		return queryExp is MethodCallExpression methodCall
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
			: baseFunc(queryExp);
	});

var rows = await dbContext.Table1
	.FromSqlInterpolated($"SELECT * FROM Table1 WHERE Id = {1}")
	.AsNoTracking()
	.ToArrayAsync();

Console.WriteLine(rows.Count());
