using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lab_3
{
    /// <summary>
    /// Параллельный алгоритм №4: Динамический последовательный перебор простых чисел.
    /// Идея: M долгоживущих потоков динамически «выхватывают» очередное простое число из общей очереди
    /// с помощью потокобезопасного счетчика (Interlocked.Increment / lock), просеивают массив и берут следующее.
    /// </summary>
    public static class Task5_DynamicPrimeQueue
    {
        public static double Run(int N = 5_000_000, int M = 4, bool useInterlocked = true, bool printInfo = true)
        {
            if (printInfo)
            {
                string syncType = useInterlocked ? "Атомарный Interlocked" : "Критическая секция lock";
                Console.WriteLine("==================================================");
                Console.WriteLine($"АЛГОРИТМ №4: ДИНАМИЧЕСКИЙ ПЕРЕБОР (M = {M}, {syncType})");
                Console.WriteLine("==================================================");
            }

            // 1. Поиск базовых простых чисел до sqrt(N)
            int sqrtN = (int)Math.Sqrt(N);
            bool[] baseComposite = new bool[sqrtN + 1];
            List<int> basePrimesList = new List<int>();

            for (int p = 2; p <= sqrtN; p++)
            {
                if (!baseComposite[p])
                {
                    basePrimesList.Add(p);
                    for (int multiple = p * p; multiple <= sqrtN; multiple += p)
                    {
                        baseComposite[multiple] = true;
                    }
                }
            }
            int[] basePrimes = basePrimesList.ToArray();

            // 2. Динамический разбор задач через общую очередь
            bool[] isComposite = new bool[N + 1];

            int globalIndex = 0;
            object syncObj = new object();

            Thread[] threads = new Thread[M];
            Stopwatch sw = Stopwatch.StartNew();

            for (int t = 0; t < M; t++)
            {
                threads[t] = new Thread(() =>
                {
                    while (true)
                    {
                        int currentIdx;

                        if (useInterlocked)
                        {
                            currentIdx = Interlocked.Increment(ref globalIndex) - 1;
                        }
                        else
                        {
                            lock (syncObj)
                            {
                                currentIdx = globalIndex;
                                globalIndex++;
                            }
                        }

                        if (currentIdx >= basePrimes.Length)
                        {
                            break;
                        }

                        int p = basePrimes[currentIdx];
                        int start = Math.Max(p * p, ((sqrtN / p) + 1) * p);

                        for (int multiple = start; multiple <= N; multiple += p)
                        {
                            isComposite[multiple] = true;
                        }
                    }
                });
                threads[t].Start();
            }

            for (int t = 0; t < M; t++)
            {
                threads[t].Join();
            }

            sw.Stop();
            double timeMs = sw.Elapsed.TotalMilliseconds;

            if (printInfo)
            {
                int primeCount = basePrimes.Length;
                for (int i = sqrtN + 1; i <= N; i++)
                {
                    if (!isComposite[i]) primeCount++;
                }

                Console.WriteLine($"Время работы: {timeMs:F2} мс");
                Console.WriteLine($"Всего найдено простых чисел: {primeCount:N0}");
                Console.WriteLine("==================================================\n");
            }

            return timeMs;
        }
    }
}
