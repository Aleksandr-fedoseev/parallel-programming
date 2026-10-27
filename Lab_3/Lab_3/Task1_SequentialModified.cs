using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lab_3
{
    /// <summary>
    /// Задание 1: Модифицированный последовательный алгоритм поиска простых чисел.
    /// Эталон (базовая линия) для сравнения с параллельными алгоритмами.
    /// </summary>
    public static class Task1_SequentialModified
    {
        public static void Run(int N = 5_000_000)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("ЛАБОРАТОРНАЯ 3 - ЭТАП 1: ПОСЛЕДОВАТЕЛЬНЫЙ АЛГОРИТМ");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Поиск простых чисел в диапазоне от 2 до N = {N:N0}");

            Stopwatch sw = Stopwatch.StartNew();

            // 1. Этап 1: Поиск базовых простых чисел до sqrt(N) классическим решетом
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
            Console.WriteLine($"Базовых простых чисел до sqrt({N}) = {sqrtN}: {basePrimes.Length} шт.");

            // 2. Этап 2: Проверка диапазона (sqrt(N) .. N] последовательно
            bool[] isComposite = new bool[N + 1];

            foreach (int p in basePrimes)
            {
                int start = Math.Max(p * p, ((sqrtN / p) + 1) * p);

                for (int multiple = start; multiple <= N; multiple += p)
                {
                    isComposite[multiple] = true;
                }
            }

            sw.Stop();

            // 3. Подсчёт общего количества простых чисел для проверки корректности
            int primeCount = basePrimes.Length;
            for (int i = sqrtN + 1; i <= N; i++)
            {
                if (!isComposite[i]) primeCount++;
            }

            Console.WriteLine($"Время работы: {sw.Elapsed.TotalMilliseconds:F2} мс");
            Console.WriteLine($"Всего найдено простых чисел: {primeCount:N0}");
            Console.WriteLine("==================================================\n");
        }
    }
}
