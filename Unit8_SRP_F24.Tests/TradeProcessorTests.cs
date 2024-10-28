using Microsoft.VisualStudio.TestTools.UnitTesting;
using SingleResponsibilityPrinciple; // Ensure this namespace is correct for your main project
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Unit8_SRP_F24.Tests
{
    [TestClass]
    public class TradeProcessorTests
    {
        [TestMethod]
        public void Test_ReadTradeData_ValidData()
        {
            // Arrange
            var tradeData = "GBPUSD,1000,1.51\nUSDJPY,2000,109.34";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(tradeData);
            writer.Flush();
            stream.Position = 0;

            var tradeProcessor = new TradeProcessor();

            // Act
            var lines = tradeProcessor.ReadTradeData(stream);

            // Assert
            Assert.AreEqual(2, lines.Count(), "The number of lines read should be 2.");
        }

        [TestMethod]
        public void Test_ReadTradeData_EmptyData()
        {
            // Arrange
            var tradeData = "";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(tradeData);
            writer.Flush();
            stream.Position = 0;

            var tradeProcessor = new TradeProcessor();

            // Act
            var lines = tradeProcessor.ReadTradeData(stream);

            // Assert
            Assert.AreEqual(0, lines.Count(), "The number of lines read should be 0.");
        }

        [TestMethod]
        public void Test_ValidateTradeData_ValidFields()
        {
            // Arrange
            var fields = new string[] { "GBPUSD", "1000", "1.51" };
            var tradeProcessor = new TradeProcessor();

            // Act
            var isValid = tradeProcessor.ValidateTradeData(fields, 1);

            // Assert
            Assert.IsTrue(isValid, "The trade data should be valid.");
        }

        [TestMethod]
        public void Test_ValidateTradeData_InvalidFields()
        {
            // Arrange
            var fields = new string[] { "GBPUSD", "invalidAmount", "1.51" };
            var tradeProcessor = new TradeProcessor();

            // Act
            var isValid = tradeProcessor.ValidateTradeData(fields, 1);

            // Assert
            Assert.IsFalse(isValid, "The trade data should be invalid due to incorrect amount format.");
        }

        [TestMethod]
        public void Test_ParseTrades_ValidLines()
        {
            // Arrange
            var tradeData = new List<string> { "GBPUSD,1000,1.51", "USDJPY,2000,109.34" };
            var tradeProcessor = new TradeProcessor();

            // Act
            var trades = tradeProcessor.ParseTrades(tradeData);

            // Assert
            Assert.AreEqual(2, trades.Count(), "The number of trades parsed should be 2.");
        }

        [TestMethod]
        public void Test_StoreTrades_ValidTrades()
        {
            // Arrange
            var trades = new List<TradeRecord>
            {
                new TradeRecord { SourceCurrency = "GBP", DestinationCurrency = "USD", Lots = 1, Price = 1.51M },
                new TradeRecord { SourceCurrency = "USD", DestinationCurrency = "JPY", Lots = 2, Price = 109.34M }
            };
            var tradeProcessor = new TradeProcessor();

            // Act & Assert
            try
            {
                tradeProcessor.StoreTrades(trades);
                Assert.IsTrue(true, "The trades were successfully stored in the database.");
            }
            catch
            {
                Assert.Fail("Storing trades failed.");
            }
        }
    }
}
