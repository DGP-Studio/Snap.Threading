using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Snap.Threading
{
    public static class ParalelForEachExtensions
    {
        private static readonly int processorsLimit = Environment.ProcessorCount * 4;

        //配置节流器
        private static readonly SemaphoreSlim throttler = new(processorsLimit, processorsLimit);
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> asyncAction)
        {
            IEnumerable<Task> tasks = source.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    await asyncAction(item).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}
