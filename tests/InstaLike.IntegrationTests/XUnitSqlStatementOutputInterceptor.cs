using System.Collections.Generic;
using NHibernate;
using NHibernate.SqlCommand;
using Xunit;

namespace InstaLike.IntegrationTests
{
    /// <summary>
    /// Writes every SQL statement NHibernate prepares to the test output, and records it
    /// so that tests can check which statements a session sends to the database.
    /// </summary>
    internal sealed class XUnitSqlStatementOutputInterceptor : EmptyInterceptor
    {
        private readonly List<string> _statements = [];

        public IReadOnlyList<string> Statements => _statements;

        public override SqlString OnPrepareStatement(SqlString sql)
        {
            _statements.Add(sql.ToString());
            TestContext.Current.TestOutputHelper?.WriteLine(sql.ToString());

            return base.OnPrepareStatement(sql);
        }
    }
}
