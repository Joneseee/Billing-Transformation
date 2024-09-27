namespace IntegrationLogic;

public static class Retry
{
    private static readonly List<string> ExceptionList = new(){"IDNOTSET"};

    private const int MaxAttempts = 5;

    private static readonly List<TimeSpan> IntervalList= new()
    {
        new(0, 0, 30),
        new(0, 0, 60),
        new(0, 0, 180),
        new(0, 0, 300)
    };

    // Asynchronous Retry
    public static async Task DoAsync(Func<Task> action)
    {
        await DoAsync<object>(async() =>
        {
            await action();
            return null!;
        });
    }

    public static async Task<T> DoAsync<T>(Func<Task<T>> action)
    {
        Exception exception = new();
        List<TimeSpan> intervals = IntervalList.OrderBy(t => t.Seconds).ToList();

        for (int attempted = 0; attempted < MaxAttempts; attempted++)
        {
            try
            {
                if (attempted > 0)
                {
                    await Task.Delay(intervals[attempted - 1]);
                }
                return await action();
            }
            catch (Exception ex)
            {
                exception = ex;

                if (ExceptionList.Any(exceptionString => ex.Message.Contains(exceptionString))) continue;
                break;
            }
        }
        throw exception;
    }

    // Synchronous Retry
    public static void Do(Action action)
    {
        Do<object>(() =>
        {
            action();
            return null!;
        });
    }

    public static T Do<T>(Func<T> action)
    {
        Exception exception = new();
        List<TimeSpan> intervals = IntervalList.OrderBy(t => t.Seconds).ToList();

        for (int attempted = 0; attempted < MaxAttempts; attempted++)
        {
            try
            {
                if (attempted > 0)
                {
                    Thread.Sleep(intervals[attempted - 1]);
                }
                return action();
            }
            catch (Exception ex)
            {
                exception = ex;

                if (ExceptionList.Any(exceptionString => exception.Message.Contains(exceptionString))) continue;
                break;
            }
        }
        throw exception;
    }
}