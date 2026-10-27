using System;
using System.Threading;
using System.Diagnostics;

class Program
{
    static int N = Environment.ProcessorCount;
    
    static void CodeFragment()
    {
        double x = 12345.6789;
        for (int i = 0; i < 10000; i++)
        {
            for (int j = 0; j < 10000; j++)
            {
                x = Math.Sqrt(x);
                x = x + 0.000000001;
                x = Math.Pow(x, 2);
            }
        }
    }

    static void Main()
    {
        Console.WriteLine($"Количество логических ядер (N): {N}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("ИССЛЕДОВАНИЕ ВЛИЯНИЯ ПРИОРИТЕТОВ ПОТОКОВ");
        Console.WriteLine(new string('=', 60));

        // Многократный запуск (5 итераций) для получения статистически 
        // достоверных данных и выявления недетерминированности порядка выполнения
        for (int run = 1; run <= 5; run++)
        {
            Console.WriteLine($"\n--- Запуск №{run} ---");
            
            // Массив для хранения ссылок на создаваемые объекты потоков
            Thread[] threads = new Thread[3];
            
            // Массив для фиксации хронологического порядка завершения потоков
            string[] completionOrder = new string[3];
            
            // Индекс текущей свободной ячейки в массиве порядка завершения
            int currentIndex = 0;
            
            // Объект синхронизации (монитор) для защиты критической секции 
            // от состояния гонки (race condition) при одновременной записи
            object lockObj = new object();

            // Набор исследуемых уровней приоритета: минимальный, стандартный и максимальный
            ThreadPriority[] testPriorities = { 
                ThreadPriority.Lowest,    // Присваивается потоку с индексом 0
                ThreadPriority.Normal,    // Присваивается потоку с индексом 1
                ThreadPriority.Highest    // Присваивается потоку с индексом 2
            };
            
            Stopwatch sw = Stopwatch.StartNew();

            // Цикл создания и конфигурации рабочих потоков
            for (int i = 0; i < 3; i++)
            {
                int threadId = i; // Локальная копия индекса для корректного замыкания в лямбда-выражении
                ThreadPriority priority = testPriorities[i];
                
                // Инициализация потока с использованием лямбда-выражения
                threads[i] = new Thread(() =>
                {
                    // 1. Выполнение вычислительной нагрузки
                    CodeFragment();
                    
                    // Только один поток может выполнить этот блок кода в данный момент времени.
                    lock (lockObj)
                    {
                        completionOrder[currentIndex] = $"Поток {threadId} ({priority})";
                        currentIndex++; // Переход к записи следующего завершившегося потока
                    }
                });

                // Явное назначение приоритета потоку перед его запуском
                threads[i].Priority = priority;
                
                // Передача потока под управление планировщика операционной системы
                threads[i].Start();
            }

            // Синхронизация: блокировка главного потока методом Join() до полного 
            // завершения всех рабочих потоков для корректного замера общего времени
            for (int i = 0; i < 3; i++)
            {
                threads[i].Join();
            }

            // Остановка секундомера после завершения всех потоков
            sw.Stop();

            // Вывод результатов текущего запуска
            Console.WriteLine($"Общее время выполнения: {sw.ElapsedMilliseconds} мс");
            Console.WriteLine($"Хронология завершения: {string.Join("  ->  ", completionOrder)}");
        }

        Console.WriteLine("\n============================================================");
        Console.WriteLine("Эксперимент завершён. Результаты готовы для анализа.");
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}