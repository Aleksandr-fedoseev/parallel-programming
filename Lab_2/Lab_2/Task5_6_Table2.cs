using System;
using System.Diagnostics;
using System.Threading;

namespace Lab_2
{
    public static class Task5_6_Table2
    {
        private const double PowerExponent = 1.789;

        public static void Run()
        {

            int[] nValues = { 10, 100, 1000, 10000 };
            int[] mValues = { 2, 3, 4, 5, 10 };

            // Шапка таблицы 2
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
            Console.Write($"{"N (размер)",-10}");
            foreach (var m in mValues)
            {
                Console.Write($" | {m + "п диап",-9} | {m + "п круг",-9}");
            }
            Console.WriteLine("\n------------------------------------------------------------------------------------------------------------------");

            foreach (int n in nValues)
            {
                double[] a = new double[n];
                double[] bRange = new double[n];
                double[] bCirc = new double[n];

                for (int i = 0; i < n; i++) a[i] = (i + 1) * 0.5;

                Console.Write($"{n,-10}");

                foreach (int m in mValues)
                {
                    // 1. Задание 5: Разделение по диапазону (блочное)
                    Stopwatch sw = Stopwatch.StartNew();
                    RunThreadsRange(a, bRange, m, (start, end) =>
                    {
                        for (int i = start; i < end; i++)
                        {
                            double sum = 0;
                            for (int j = 0; j < i; j++) sum += Math.Pow(a[i], PowerExponent);
                            bRange[i] = sum;
                        }
                    });
                    sw.Stop();
                    double rangeMs = sw.Elapsed.TotalMilliseconds;

                    // 2. Задание 6: Разделение по кругу (циклическое)
                    sw.Restart();
                    RunThreadsCircular(a, bCirc, m, (threadIndex, step, len) =>
                    {
                        // Поток идёт с шагом M, начиная со своего номера:
                        for (int i = threadIndex; i < len; i += step)
                        {
                            double sum = 0;
                            for (int j = 0; j < i; j++) sum += Math.Pow(a[i], PowerExponent);
                            bCirc[i] = sum;
                        }
                    });
                    sw.Stop();
                    double circMs = sw.Elapsed.TotalMilliseconds;

                    Console.Write($" | {rangeMs,9:F3} | {circMs,9:F3}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("------------------------------------------------------------------------------------------------------------------\n");
        }

        /// <summary>
        /// Задание 5: Блочная декомпозиция (по диапазону)
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

        /// <summary>
        /// Задание 6: Циклическая (круговая) декомпозиция
        /// </summary>
        private static void RunThreadsCircular(double[] a, double[] b, int m, Action<int, int, int> processCircular)
        {
            int n = a.Length;
            Thread[] threads = new Thread[m];

            for (int t = 0; t < m; t++)
            {
                int threadIndex = t;
                // Потоку передаётся: его номер (смещение), шаг (m) и общая длина массива (n)
                threads[t] = new Thread(() => processCircular(threadIndex, m, n));
                threads[t].Start();
            }

            for (int t = 0; t < m; t++)
            {
                threads[t].Join();
            }
        }
    }
}
