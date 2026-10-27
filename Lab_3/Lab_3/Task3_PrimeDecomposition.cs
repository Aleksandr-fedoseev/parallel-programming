using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lab_3
{
    /// <summary>
    /// Параллельный алгоритм №2: Декомпозиция набора простых чисел.
    /// Идея: Разделяются базовые простые числа между M потоками (циклически с шагом M).
    /// Каждый поток получает свою часть простых чисел и просеивает ВЕСЬ диапазон (sqrt(N) .. N].
    /// </summary>
    public static class Task3_PrimeDecomposition
    {
        public static double Run(int N = 5_000_000, int M = 4, bool printInfo = true)
        {
            if (printInfo)
            {
                Console.WriteLine("==================================================");
                Console.WriteLine($"АЛГОРИТМ №2: ДЕКОМПОЗИЦИЯ ПРОСТЫХ ЧИСЕЛ (M = {M} потоков)");
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

            // 2. Распределение простых чисел между M потоками с шагом M
            bool[] isComposite = new bool[N + 1];

            Thread[] threads = new Thread[M];
            Stopwatch sw = Stopwatch.StartNew();

            for (int t = 0; t < M; t++)
            {
                int threadIndex = t;

                threads[t] = new Thread(() =>
                {
                    for (int pIdx = threadIndex; pIdx < basePrimes.Length; pIdx += M)
                    {
                        int p = basePrimes[pIdx];
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
