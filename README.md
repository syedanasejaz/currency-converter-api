# Currency Converter API

This is a simple Currency Converter API that fetches the latest exchange rates, allows currency conversion, and retrieves historical exchange rates based on the Frankfurter API.

## Git Repository Information

- **Git Repository Name**: `currency-converter-api`
- **Git Branch Name**: `main`

## Solution and Project Information

- **Solution Name**: `CurrencyConverterApi`
- **API Project Name**: `CurrencyConverterApi`
- **Unit Testing Project Name**: `CurrencyServiceTests`

## Features

1. **Retrieve Latest Exchange Rates**: Fetches the latest exchange rates for a specified base currency.
2. **Currency Conversion**: Converts amounts between different currencies, excluding specific currencies (TRY, PLN, THB, MXN).
3. **Historical Rates**: Returns historical exchange rates for a given period with pagination.

## Assumptions Made

- If the **Frankfurter API** fails on the first attempt, the service retries **3 times** using **exponential backoff**.
- **Memory caching** is implemented to optimize performance and reduce API calls.
- **Pagination** is implemented for historical data requests.
- The API follows **standard RESTful principles** and handles errors properly.

## Enhancements & Future Improvements

- **Docker Support**: Containerize the API for easier deployment.
- **Database Storage**: Store historical rates in a database for faster retrieval.
- **Authentication**: Add API key authentication for secure access.
- **Logging & Monitoring**: Integrate **Serilog** and **Grafana** for better monitoring.

## Requirements

- **.NET 5.0** or higher
- **HttpClient** for making requests
- **MemoryCache** for caching API results
- **Retry Policy** for handling API retries with exponential backoff

## How to Run the Application

1. **Clone the Repository**:
   git clone -b main https://github.com/syedanasejaz/currency-converter-api.git
   cd currency-converter-api

2. **Install Dependencies**:
	dotnet restore

3. **Run the Application**:
	dotnet run --project CurrencyConverterApi

## API Endpoints

1. **Get Latest Exchange Rates**

    - Endpoint: /latest?from={baseCurrency}
    - Method: GET
    - Description: Retrieves the latest exchange rates for the provided base currency.

2. **Convert Currency**

    -Endpoint: /convert
    -Method: POST
    -Request Body:

		{
		"amount": 100,
		"fromCurrency": "USD",
		"toCurrency": "EUR"
		}

    -Description: Converts a specified amount from one currency to another. Returns an error for specific currencies (TRY, PLN, THB, MXN).

3. **Get Historical Rates**

    - Endpoint: /historical
    - Method: GET
    - Request Parameters:
        - baseCurrency: The base currency for historical data.
        - startDate: Start date of the historical range.
        - endDate: End date of the historical range.
        - page: The page number for pagination (default is 1).
        - pageSize: The number of records per page (default is 10).
		
    -Description: Retrieves historical exchange rates for the given date range.
	
## Error Handling

    - If an error occurs while fetching data from the Frankfurter API, the service will retry up to 3 times with exponential backoff.
    - Errors are properly handled and returned in the response body with a clear error message.

## Assumptions & Limitations

1. Caching is used to store the exchange rates for 30 minutes to optimize performance and reduce redundant API calls.
2. Specific currencies like TRY, PLN, THB, and MXN are excluded from conversion requests and will return a 400 Bad Request response.

## Testing

Unit tests are implemented to verify the functionality of the service, including:

- Retry policy handling
- Caching functionality
- Currency conversion logic
- Historical rates retrieval

The unit tests are located in the CurrencyServiceTests project.

## Technologies Used

- C# and ASP.NET Core
- HttpClient for making API requests
- Polly for retry policies
- MemoryCache for caching results
- xUnit for unit testing