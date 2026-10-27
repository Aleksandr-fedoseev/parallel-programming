using System;
using System.Diagnostics;

namespace Lab_2
{
    public static class Task1_Sequential
    {
        public static void Run()
        {
            // 1. Задаём размер вектора N
            int N = 100_000;
            Console.WriteLine($"Размер вектора N = {N}");

            // 2. Инициализируем исходный массив 'a' и результирующий массив 'b'
            double[] a = new double[N];
            double[] b = new double[N];

            for (int i = 0; i < N; i++)
            {
                a[i] = (i + 1) * 0.5; // Заполняем числами: 0.5, 1.0, 1.5, ...
            }

            // 3. Замеряем время последовательной обработки
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < N; i++)
            {
                b[i] = Math.Pow(a[i], 1.789); // Возведение каждого элемента в степень
            }

            sw.Stop();

            // 4. Выводим результаты
            Console.WriteLine($"Время выполнения: {sw.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"Первый элемент b[0]   = {b[0]:F4}");
            Console.WriteLine($"Последний элемент b[N-1] = {b[N - 1]:F4}");
            Console.WriteLine("==================================================\n");
        }
    }
}
