using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Peshitta.Data.SqlLite;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;

namespace TestProject1
{
	[TestClass]
	public class SqlLite
	{
		DbSqlContext dbSqlContext;
		[TestInitialize]
		public void Init()
		{
			
			var connectionString= ConfigurationManager.ConnectionStrings["bijbel"].ConnectionString;
			
			var _options = new DbContextOptionsBuilder<DbSqlContext>()
				.UseSqlite(connectionString)
				.Options;

			dbSqlContext = new DbSqlContext(_options);
			try
			{
				dbSqlContext.Database.EnsureDeleted();
				dbSqlContext.Database.EnsureCreated();
			}
			catch (InvalidOperationException inv)
			{

				throw inv;
			}
		}
		[TestMethod]
		public async Task TestCreate()
		{
			var book =await dbSqlContext.Books.FirstOrDefaultAsync();
		}
	}
}
