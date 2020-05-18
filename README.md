Vontebel Coding test

2 branches:
- master has simple implementation
- AzureServiceBus branch is more advanced with azure Queue and circut breaker (https://github.com/suposk/CodingExercisePDE/tree/AzureServiceBus)

To run application, select CodingExercisePDE.Api as Starp project and run with F5.

StandardNumbersHostedService generates number, saves to DB and post to local endpoint https://localhost:5001/api/numbers.

Get on https://localhost:5001/api/numbers returns all posted numbers, stored in MemoryCache.
