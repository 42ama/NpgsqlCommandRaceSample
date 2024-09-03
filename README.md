# NpgsqlCommandRaceSample

This project reproduces a problem with **idle-connections** and **busy-connections** metrics not working correctly.

## Project structure

- Database connection context and related classes: `MyDbContext` and `SavedBookInformation`
- `NpgSqlMeter` - class for collecting metrics.
- `WeatherForecastController` - service entry point.

## Problem reproduction

**Reproduction Steps:**
1. Go to the sources of the `Npgsql.NpgsqlEventSource` class.
2. Put a breakpoint in the `DataSourceCreated` and `OnEventCommand` methods.
3. In debug mode, start the application and make a request to _GetWeatherForecast_.

**Expected result:**
The execution thread first stopped in the `DataSourceCreated` method, then moved to `OnEventCommand`, **idle-connections** and **busy-connections** metrics were registered.

**Factual result** (both with and without the fix from https://github.com/npgsql/npgsql/pull/5497):
The execution thread stops in the `OnEventCommand` method, then stops in the `DataSourceCreated` method. It does not come to `OnEventCommand` anymore, so after `_dataSources` initiation there is no re-registration of metrics collection, because `OnEventCommand` is not called again.

Also in the `NpgSqlMeter` class you can put a breakpoint in the `SaveMeasurements` method to see what metrics values are written to `_values`. There are no values related to **idle-connections** and **busy-connections**.