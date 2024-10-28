using Microsoft.Data.SqlClient;

namespace SingleResponsibilityPrinciple
{
    public class TradeProcessor
    {
        const float LotSize = 100000f;

        internal IEnumerable<string> ReadTradeData(Stream stream)
        {
            List<string> lines = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        internal bool ValidateTradeData(String[] fields, int currentLine)
        {
            if (fields.Length != 3)
            {
                LogMessage("WARN: Line {0} malformed. Only {1} field(s) found.", currentLine, fields.Length);
                return false;
            }

            if (fields[0].Length != 6)
            {
                LogMessage("WARN: Trade currencies on line {0} malformed: '{1}'", currentLine, fields[0]);
                return false;
            }

            int tradeAmount;
            if (!int.TryParse(fields[1], out tradeAmount))
            {
                LogMessage("WARN: Trade amount on line {0} not a valid integer: '{1}'", currentLine, fields[1]);
                return false;
            }

            decimal tradePrice;
            if (!decimal.TryParse(fields[2], out tradePrice))
            {
                LogMessage("WARN: Trade price on line {0} not a valid decimal: '{1}'", currentLine, fields[2]);
                return false;
            }
            return true;
        }

        internal TradeRecord MapTradeDataToTradeRecord(String[] fields)
        {
            var sourceCurrencyCode = fields[0].Substring(0, 3);
            var destinationCurrencyCode = fields[0].Substring(3, 3);
            int tradeAmount = int.Parse(fields[1]);
            decimal tradePrice = decimal.Parse(fields[2]);

            var trade = new TradeRecord();
            trade.SourceCurrency = sourceCurrencyCode;
            trade.DestinationCurrency = destinationCurrencyCode;
            trade.Lots = tradeAmount / LotSize;
            trade.Price = tradePrice;

            return trade;
        }

        internal IEnumerable<TradeRecord> ParseTrades(IEnumerable<string> lines)
        {
            List<TradeRecord> trades = new List<TradeRecord>();

            var lineCount = 1;
            foreach (var line in lines)
            {
                String[] fields = line.Split(new char[] { ',' });

                if (ValidateTradeData(fields, lineCount) == false)
                {
                    continue;
                }

                TradeRecord trade = MapTradeDataToTradeRecord(fields);
                trades.Add(trade);

                lineCount++;
            }
            return trades;
        }

        internal void StoreTrades(IEnumerable<TradeRecord> trades)
        {
            LogMessage("INFO: Connecting to database");

            // Updated connection string to use the attached database
            string datadirConnectString = @"Server=(LocalDB)\libckout10-ngna;Database=TradeDatabase;Integrated Security=True;";

            using (var connection = new SqlConnection(datadirConnectString))
            {
                LogMessage("INFO: Going to open database connection");
                connection.Open();
                LogMessage("INFO: Database connection OPEN");

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades)
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "InsertTrade";
                        command.Parameters.AddWithValue("@sourceCurrency", trade.SourceCurrency);
                        command.Parameters.AddWithValue("@destinationCurrency", trade.DestinationCurrency);
                        command.Parameters.AddWithValue("@lots", trade.Lots);
                        command.Parameters.AddWithValue("@price", trade.Price);
                        LogMessage("INFO: Adding trade to database...");

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                connection.Close();
            }

            LogMessage("INFO: {0} trades processed", trades.Count());
        }

        private void LogMessage(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void ProcessTrades(Stream stream)
        {
            var lines = ReadTradeData(stream);
            var trades = ParseTrades(lines);
            StoreTrades(trades);
        }
    }
}
