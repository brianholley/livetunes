using System;
using System.Data.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LiveTunes;

namespace LiveTunesTest
{
    [TestClass]
    public class TagItemTests
    {
        private static readonly string DBConnectionString = "Data Source=isostore:/ConcertTestDB.sdf";
        private static DataContext _context;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _context = new DataContext(DBConnectionString);
            _context.CreateDatabase();
            _context.SubmitChanges();
        }

        [TestMethod]
        public void TestCreateTag()
        {
            Table<TagItem> tagTable = _context.GetTable<TagItem>();

            TagItem tag = new TagItem() { Tag = "Rock" };
            tagTable.InsertOnSubmit(tag);

            _context.SubmitChanges();
        }
    }
}
