using System;
using System.Diagnostics;
using System.Threading;

namespace Lab_2
{
    public static class Task2_MultiThreadedRange
    {
        public static void Run()
        {
            // 1. Задаём параметры: размер вектора N и количество потоков M
            int N = 100_000;
            int M = 4; // Будем использовать 4 потока

            Console.WriteLine($"Размер вектора N = {N}");
            Console.WriteLine($"Количество потоков M = {M}");

            // 2. Инициализируем массивы
            double[] a = new double[N];
            double[] b = new double[N];

            for (int i = 0; i < N; i++)
            {
                a[i] = (i + 1) * 0.5;
            }

            // 3. Создаём массив объектов потоков
            Thread[] threads = new Thread[M];

            // Размер одного куска (блока) для каждого потока
            int chunkSize = N / M;
            int remainder = N % M; // остаток, если N не делится на M нацело
            

            Stopwatch sw = Stopwatch.StartNew();

            // 4. Запускаем M потоков
            for (int t = 0; t < M; t++)
            {
                // сохраняем 't' в локальную переменную для корректного замыкания
                int threadIndex = t;

                // Вычисляем границы диапазона для текущего потока:
                int start = threadIndex * chunkSize + Math.Min(threadIndex, remainder);
                int end = start + chunkSize + (threadIndex < remainder ? 1 : 0);

                // Создаём поток, передав ему лямбда-выражение с нужным диапазоном
                threads[t] = new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                    {
                        b[i] = Math.Pow(a[i], 1.789);
                    }
                });

                // Запускаем поток
                threads[t].Start();
            }

            // 5. Ожидаем завершения ВСЕХ потоков
            for (int t = 0; t < M; t++)
            {
                threads[t].Join();
            }

            sw.Stop();

            // 6. Выводим результаты
            Console.WriteLine($"Время выполнения ({M} потока): {sw.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"Первый элемент b[0]   = {b[0]:F4}");
            Console.WriteLine($"Последний элемент b[N-1] = {b[N - 1]:F4}");
            Console.WriteLine("==================================================\n");
        }
    }
}
