CREATE TABLE Trades (
    TradeID INT IDENTITY(1,1) PRIMARY KEY,
    SourceCurrency NVARCHAR(10) NOT NULL,
    DestinationCurrency NVARCHAR(10) NOT NULL,
    Lots FLOAT NOT NULL,
    Price FLOAT NOT NULL
);
