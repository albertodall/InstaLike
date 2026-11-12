using NHibernate;
using NHibernate.SqlCommand;
using Xunit;

namespace InstaLike.IntegrationTests;

internal sealed class XUnitSqlStatementOutputInterceptor : EmptyInterceptor
{
    private readonly ITestOutputHelper _output;

    public XUnitSqlStatementOutputInterceptor(ITestOutputHelper output)
    {
        _output = output;
    }

    public override SqlString OnPrepareStatement(SqlString sql)
    {
        _output.WriteLine(sql.ToString());

        return base.OnPrepareStatement(sql);
    }
}
