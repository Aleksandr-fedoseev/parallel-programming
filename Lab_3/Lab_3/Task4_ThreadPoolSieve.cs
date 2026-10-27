using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lab_3
{
    /// <summary>
    /// Параллельный алгоритм №3: Применение пула потоков (ThreadPool).
    /// Идея: Каждое базовое простое число отправляется как отдельный рабочий элемент в ThreadPool.
    /// Синхронизация завершения выполняется через CountdownEvent.
    /// </summary>
    public static class Task4_ThreadPoolSieve
    {
        public static double Run(int N = 5_000_000, bool printInfo = true)
        {
            if (printInfo)
            {
                Console.WriteLine("==================================================");
                Console.WriteLine("АЛГОРИТМ №3: ПРИМЕНЕНИЕ ПУЛА ПОТОКОВ (ThreadPool)");
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

            // 2. Обработка через системный пул потоков
            bool[] isComposite = new bool[N + 1];

            using (CountdownEvent countdown = new CountdownEvent(basePrimes.Length))
            {
                Stopwatch sw = Stopwatch.StartNew();

                for (int i = 0; i < basePrimes.Length; i++)
                {
                    int p = basePrimes[i];

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            int start = Math.Max(p * p, ((sqrtN / p) + 1) * p);

                            for (int multiple = start; multiple <= N; multiple += p)
                            {
                                isComposite[multiple] = true;
                            }
                        }
                        finally
                        {
                            countdown.Signal();
                        }
                    });
                }

                countdown.Wait();

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
}
