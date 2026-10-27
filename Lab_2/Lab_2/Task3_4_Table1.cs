using System;
using System.Diagnostics;
using System.Threading;

namespace Lab_2
{
    public static class Task3_4_Table1
    {
        private const double PowerExponent = 1.789;
        private const int ComplexityK = 1000; // Параметр усложнения из Задания 4

        public static void Run()
        {
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("ЗАДАНИЯ 3 И 4: СРАВНЕНИЕ ПРОСТОЙ И УСЛОЖНЕННОЙ ОБРАБОТКИ (ТАБЛИЦА 1)");
            Console.WriteLine("=========================================================================================");
            Console.WriteLine($"Параметр сложности K = {ComplexityK} итераций на каждый элемент\n");

            int[] nValues = { 10, 100, 1000, 100000 };
            int[] mValues = { 2, 3, 4, 5, 10 };

            // Шапка таблицы
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------");
            Console.Write($"{"N (размер)",-10} | {"Посл. прост",-12} | {"Посл. услож",-12}");
            foreach (var m in mValues)
            {
                Console.Write($" | {m + "п прост",-10} | {m + "п услож",-11}");
            }
            Console.WriteLine("\n-----------------------------------------------------------------------------------------------------------------------------");

            // Перебираем все размеры N: 10, 100, 1000, 100 000
            foreach (int n in nValues)
            {
                double[] a = new double[n];
                double[] b = new double[n];
                for (int i = 0; i < n; i++) a[i] = (i + 1) * 0.5;

                // 1. Замер последовательной простой обработки
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < n; i++) b[i] = Math.Pow(a[i], PowerExponent);
                sw.Stop();
                double seqSimpleMs = sw.Elapsed.TotalMilliseconds;

                // 2. Замер последовательной усложненной обработки (Задание 4)
                sw.Restart();
                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < ComplexityK; j++) sum += Math.Pow(a[i], PowerExponent);
                    b[i] = sum;
                }
                sw.Stop();
                double seqComplexMs = sw.Elapsed.TotalMilliseconds;

                // Выводим левую часть строки (N и последовательные замеры)
                Console.Write($"{n,-10} | {seqSimpleMs,12:F3} | {seqComplexMs,12:F3}");

                // 3. Перебираем количество потоков M (2, 3, 4, 5, 10)
                foreach (int m in mValues)
                {
                    // А. Многопоточная простая
                    sw.Restart();
                    RunThreadsRange(a, b, m, (start, end) =>
                    {
                        for (int i = start; i < end; i++) b[i] = Math.Pow(a[i], PowerExponent);
                    });
                    sw.Stop();
                    double parSimpleMs = sw.Elapsed.TotalMilliseconds;

                    // Б. Многопоточная усложненная (Задание 4)
                    sw.Restart();
                    RunThreadsRange(a, b, m, (start, end) =>
                    {
                        for (int i = start; i < end; i++)
                        {
                            double sum = 0;
                            for (int j = 0; j < ComplexityK; j++) sum += Math.Pow(a[i], PowerExponent);
                            b[i] = sum;
                        }
                    });
                    sw.Stop();
                    double parComplexMs = sw.Elapsed.TotalMilliseconds;

                    // Допечатываем в строку результаты для данного M
                    Console.Write($" | {parSimpleMs,10:F3} | {parComplexMs,11:F3}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------\n");
        }

        /// <summary>
        /// Универсальный вспомогательный метод: запускает M потоков по диапазону индексов
        /// </summary>
        private static void RunThreadsRange(double[] a, double[] b, int m, Action<int, int> processChunk)
        {
            int n = a.Length;
            int chunkSize = n / m;
            int remainder = n % m;
            Thread[] threads = new Thread[m];

            for (int t = 0; t < m; t++)
            {
                int threadIndex = t;
                int start = threadIndex * chunkSize + Math.Min(threadIndex, remainder);
                int end = start + chunkSize + (threadIndex < remainder ? 1 : 0);

                threads[t] = new Thread(() => processChunk(start, end));
                threads[t].Start();
            }

            for (int t = 0; t < m; t++)
            {
                threads[t].Join();
            }
        }
    }
}
