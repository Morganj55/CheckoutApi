# Instructions for running

You can download the source code for the project here: https://github.com/Morganj55/CheckoutApi

## Solution

To run the solution from the command line:

1. Run command "dotnet build" path/to/downloaded/repository/MySolution.sln
2. Run command "dotnet run" --project path/to/downloaded/repository/src/PaymentGateway.Api.csproj

Or you can run from your IDE of choice.

## Fake Bank Implementation

Ensure you have docker downloaded, you can find the download link here: https://www.docker.com/

To start the fake aquiring bank implementation, run the container from docker or:

1. Navigate to path/to/downloaded/repository/imposters and run the command "docker-compose up"

## Sending requests to the API

If running the application from your IDE of choice, in debug mode the swagger web UI will appear with an interface to send request to the api.
Otherwise, you can use whaterver tool you like to interact with the api e.g curl.

To run the api sucesfully end to end, ensure the fake aquiring bank is up and then start the checkout api.

## Valid ISO Currency codes

1. USD
2. EUR
3. GBP

## Example of requests

Success-Unauthorised
{
"cardNumber": "1234567890123456",
"expiryMonth": 12,
"expiryYear": 2026,
"currency": "USD",
"amount": 1000,
"cvv": "1234"
}

Success-Authorised
{
"cardNumber": "1234567890123457",
"expiryMonth": 12,
"expiryYear": 2026,
"currency": "USD",
"amount": 1000,
"cvv": "1234"
}

Fail
{
"cardNumber": "1234567890123450",
"expiryMonth": 12,
"expiryYear": 2026,
"currency": "USD",
"amount": 1000,
"cvv": "1234"
}

# Assumptions

- There is only one aquiring bank.
- Not considered logging as you would normally have a distributed logging system.
- Not considering idempotency e.g no idempotency key in client headers.
- Not considered resiliance in terms of retries, circuit breakers etc.
- We do not have and actual concrete implementation of the database, I am just using an in memory concurrent dictionary to simulate this.
- Not performing any authorization of the client e.g no api keys in client headers
- No contract tests
- No E2E environment for tests (except manually doing them!)
