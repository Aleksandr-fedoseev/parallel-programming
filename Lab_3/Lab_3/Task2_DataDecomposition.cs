using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lab_3
{
    /// <summary>
    /// Параллельный алгоритм №1: Декомпозиция по данным.
    /// Идея: Диапазон (sqrt(N) .. N] делится на M равных частей (блоков).
    /// Каждый поток обрабатывает свой диапазон, проверяя его на делимость по ВСЕМ базовым простым числам.
    /// </summary>
    public static class Task2_DataDecomposition
    {
        public static double Run(int N = 5_000_000, int M = 4, bool printInfo = true)
        {
            if (printInfo)
            {
                Console.WriteLine("==================================================");
                Console.WriteLine($"АЛГОРИТМ №1: ДЕКОМПОЗИЦИЯ ПО ДАННЫМ (M = {M} потоков)");
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

            // 2. Распределение диапазона (sqrt(N) .. N] по M потокам
            bool[] isComposite = new bool[N + 1];

            int totalRange = N - sqrtN;
            int chunkSize = totalRange / M;
            int remainder = totalRange % M;

            Thread[] threads = new Thread[M];
            Stopwatch sw = Stopwatch.StartNew();

            for (int t = 0; t < M; t++)
            {
                int threadIndex = t;
                int start = sqrtN + 1 + threadIndex * chunkSize + Math.Min(threadIndex, remainder);
                int end = start + chunkSize + (threadIndex < remainder ? 1 : 0);

                threads[t] = new Thread(() =>
                {
                    foreach (int p in basePrimes)
                    {
                        int firstMultiple = Math.Max(p * p, ((start + p - 1) / p) * p);

                        for (int multiple = firstMultiple; multiple < end; multiple += p)
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
