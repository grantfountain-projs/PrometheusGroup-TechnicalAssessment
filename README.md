# Stock Data API

A C# ASP.NET Core Web API that consumes the Yahoo Finance API to fetch intraday stock data for a given symbol, groups it by day, and returns the daily averages and total volume for the last month.

## How to Run
```bash
dotnet run
```

## Usage
```bash
curl http://localhost:5196/api/stock/TSLA
```

Returns daily stock data with average low/high prices and total volume.
