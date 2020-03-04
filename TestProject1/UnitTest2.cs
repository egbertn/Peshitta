using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Peshitta.Data.Models;

namespace TestProject1
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            var hk = new HistoryKey(4, DateTime.Now);
            var hash = hk.GetHashCode();
        }
    }
}
