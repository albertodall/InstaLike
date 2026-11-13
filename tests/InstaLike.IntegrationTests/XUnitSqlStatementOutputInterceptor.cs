using System.Collections.Generic;
using NHibernate;
using NHibernate.SqlCommand;
using Xunit;

namespace InstaLike.IntegrationTests;

internal sealed class XUnitSqlStatementOutputInterceptor : EmptyInterceptor
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _statements = [];

    public XUnitSqlStatementOutputInterceptor(ITestOutputHelper output)
    {
        _output = output;
    }

    public IReadOnlyList<string> Statements => _statements;

    public override SqlString OnPrepareStatement(SqlString sql)
    {
        _statements.Add(sql.ToString());
        _output.WriteLine(sql.ToString());

        return base.OnPrepareStatement(sql);
    }
}
