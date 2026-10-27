using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lab_3
{
    /// <summary>
    /// Сводный модуль сравнительного анализа всех 5 алгоритмов Лабораторной работы №3:
    /// 1. Последовательный эталон (Sequential Modified)
    /// 2. Алгоритм №1: Декомпозиция по данным (Data Decomposition)
    /// 3. Алгоритм №2: Декомпозиция простых чисел (Prime Decomposition)
    /// 4. Алгоритм №3: Пул потоков (ThreadPool)
    /// 5. Алгоритм №4: Динамический перебор с синхронизацией (Dynamic Queue)
    /// </summary>
    public static class BenchmarkCompareAll
    {
        public static void Run()
        {
            Console.WriteLine("=========================================================================================================");
            Console.WriteLine("ЛАБОРАТОРНАЯ РАБОТА №3: СВОДНОЕ ТЕСТИРОВАНИЕ И СРАВНЕНИЕ ВСЕХ 5 АЛГОРИТМОВ");
            Console.WriteLine("=========================================================================================================\n");

            int[] nValues = { 1_000_000, 5_000_000, 10_000_000 };
            int[] mValues = { 2, 4, 6, 8, 12 };

            foreach (int n in nValues)
            {
                Console.WriteLine($"\n>>>>>>>>>> ТЕСТИРОВАНИЕ ДЛЯ ДИАПАЗОНА N = {n:N0} <<<<<<<<<<\n");

                // 1. Замер последовательного эталона
                double tSeq = MeasureSequential(n);
                Console.WriteLine($"[Эталон] Модифицированный последовательный: {tSeq:F2} мс\n");

                // 2. Замер параллельного алгоритма №3 (ThreadPool - пул потоков)
                double tPool = Task4_ThreadPoolSieve.Run(n, printInfo: false);
                double speedupPool = tSeq / tPool;
                Console.WriteLine($"[Алгоритм 3] Пул потоков (ThreadPool): {tPool:F2} мс | Ускорение S = {speedupPool:F2}x\n");

                // 3. Таблица для алгоритмов, зависящих от M
                Console.WriteLine("---------------------------------------------------------------------------------------------------------");
                Console.WriteLine($"{"Потоков M",-10} | {"Алг.1 Данные (мс)",-18} | {"Алг.2 Простые (мс)",-18} | {"Алг.4 Динамика (мс)",-19} | {"Лучшее ускорение S",-18}");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------");

                foreach (int m in mValues)
                {
                    double tAlg1 = Task2_DataDecomposition.Run(n, m, printInfo: false);
                    double tAlg2 = Task3_PrimeDecomposition.Run(n, m, printInfo: false);
                    double tAlg4 = Task5_DynamicPrimeQueue.Run(n, m, useInterlocked: true, printInfo: false);

                    double bestTime = Math.Min(tAlg1, Math.Min(tAlg2, tAlg4));
                    double bestSpeedup = tSeq / bestTime;

                    Console.WriteLine($"{m,-10} | {tAlg1,18:F2} | {tAlg2,18:F2} | {tAlg4,19:F2} | {bestSpeedup,15:F2}x");
                }
                Console.WriteLine("---------------------------------------------------------------------------------------------------------\n");
            }
        }

        private static double MeasureSequential(int n)
        {
            int sqrtN = (int)Math.Sqrt(n);
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

            bool[] isComposite = new bool[n + 1];

            Stopwatch sw = Stopwatch.StartNew();

            foreach (int p in basePrimes)
            {
                int start = Math.Max(p * p, ((sqrtN / p) + 1) * p);
                for (int multiple = start; multiple <= n; multiple += p)
                {
                    isComposite[multiple] = true;
                }
            }

            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
