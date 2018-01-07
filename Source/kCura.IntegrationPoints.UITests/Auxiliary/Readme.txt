Log.Information("Executor URL: {url}", GetExecutorUrl());
Log.Information("URL: {sessionId}", Driver.SessionId);

Log.Information("Sleeping...");
Thread.Sleep(TimeSpan.FromHours(3));
