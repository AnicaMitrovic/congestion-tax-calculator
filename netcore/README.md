## How to run

Requires the .NET 8 SDK and the ASP.NET Core 8 runtime

```bash
dotnet test                                  # run the unit tests
dotnet run --project src/CongestionTax.Api   # start the API
```

Then open `/swagger`. `GET /api/cities` lists the cities that have rules loaded.

Example request to `POST /api/tax/calculate`:

```json
{
  "city": "gothenburg",
  "vehicleType": "Car",
  "passages": ["2013-02-08T06:20:27", "2013-02-08T15:47:00", "2013-02-08T16:48:00"]
}
```

Example request to `POST /api/tax/calculate` using the sample dates for 2013-02-08:

```json
{
  "city": "gothenburg",
  "vehicleType": "Car",
  "passages": [
    "2013-02-08T06:20:27",
    "2013-02-08T06:27:00",
    "2013-02-08T14:35:00",
    "2013-02-08T15:29:00",
    "2013-02-08T15:47:00",
    "2013-02-08T16:01:00",
    "2013-02-08T16:48:00",
    "2013-02-08T17:49:00",
    "2013-02-08T18:29:00",
    "2013-02-08T18:35:00"
  ]
}
```

This gives `total: 60` and `sumOfCharges: 70`

Timestamps are Swedish local time and only 2013 is supported

## What I prioritised
 Getting the build green, fixing CongestionTaxCalculator.cs
 Making the solution flexible so it can be used for other cities, not just Gothenburg.
 Most of my time went into the calculation itself. I moved all the parameters out of the calculator into `CityTaxRules`, so the same code works for any city and the
tariffs can be edited outside the application.

## Bugs found
1. Conjuction range bug, rejects HH:00–HH:29 for hours 09–14
2. dates[0] in GetTax method are not checked for null or empty array, which can lead to null exception
3. Vehicle type Tractor is in code , but not in specification
4. GetTax assumes that input is sorted and applies for single-day, doesn´t validates any of that
5. The 60 minute check compared `date.Millisecond - intervalStart.Millisecond`. `Millisecond` is the 0-999 part of a second, not the timestamp, so the difference was always 0 and the else branch never ran
6. Bus is missing from the tax free vehicles, although the specification requires it
7. Following from 4, the 60 SEK cap was applied to the whole input instead of to each day separately. Fixed by grouping passages by calendar date and capping each day on its own
8. The tax free dates were wrapped in `if (year == 2013)`, so for any other year nothing was a holiday

## Deliberately not done
Complete test coverage. I tested the calculation logic, since that is where the bugs were and where the rules are ambiguous. 
I did not test the HTTP endpoint or the JSON loading, those would mostly check that ASP.NET and System.Text.Json work.
Validation is inline in the endpoint rather than in its own class, since there are only three checks. Rules are read once at startup, so editing a file needs a restart.
No authentication.

## With more time
I would add integration tests over the endpoint, and tests for the single-charge window, which is part that is most likely to break with input I haven't thought of
I would also add dates to the rules, so each rule set knows when it was valid from and until. Right now one file per city covers different cities, but not different time periods. 

## Time log
1-2h
After creating project structure, I spent good amount of time on the algorithm itself - going through specification, understanding the requirements and understanding what the collegue has done so far in CongestionTaxCalculator class.
Checked the single charge rule against Skatteverket. The spec can be read two ways so I wanted to be sure before building on it
First I fixed obvious things thing I found in a code.  
Fixed null reference exception, two tariff band bugs in place, replaced the tariff chain with a lookup table

3h
Moved the parameters out of the calculator into CityTaxRules (read all of them from the rules parameter instead so they can be edited for another cities)
Code moved in one method, grouped passages by day and returned a per-day breakdown to fix charging passages for several days as one passage

4+h
Added tests for the new logic and while writing them I found a bug in the single-charge window. The window was stuck on the first passage of the day and never moved forward, so every passage was compared against it. The fix was to start a new window when a passage comes more than 60 minutes after the current window started and charge the highest amount from each window.
Implemented JsonFileTaxRuleProvider , in production we expect database, but here we can use a JSON file to provide the rules. The provider reads the JSON file and deserializes it into a CityTaxRules object


## Structure

`CongestionTax.Domain` holds the rules and the calculation and has no package references at all, no ASP.NET, no JSON library. That is enforced by the compiler, so nothing from the web layer can leak into the calculation.

`CongestionTax.Api` holds the endpoint and the JSON rule provider. The dependency goes one way: Api knows about Domain, Domain knows nothing about Api.

`ITaxRuleProvider` is defined in Domain but only used by the endpoint. The calculator takes `CityTaxRules` as a plain parameter, so it never touches a file, a database or configuration