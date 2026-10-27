using System;

namespace Lab_3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args.Length > 0)
            {
                ExecuteChoice(args[0]);
                return;
            }

            while (true)
            {
                Console.WriteLine("==========================================================================");
                Console.WriteLine("ЛАБОРАТОРНАЯ РАБОТА №3: ПОИСК ПРОСТЫХ ЧИСЕЛ (РЕШЕТО ЭРАТОСФЕНА)");
                Console.WriteLine("==========================================================================");
                Console.WriteLine("1 - Модифицированный последовательный алгоритм (Эталон)");
                Console.WriteLine("2 - Параллельный алгоритм №1: Декомпозиция по данным (диапазонам)");
                Console.WriteLine("3 - Параллельный алгоритм №2: Декомпозиция набора простых чисел");
                Console.WriteLine("4 - Параллельный алгоритм №3: Применение пула потоков (ThreadPool)");
                Console.WriteLine("5 - Параллельный алгоритм №4: Динамический перебор (Interlocked / lock)");
                Console.WriteLine("6 - Сводное сравнительное тестирование всех алгоритмов (N до 10 млн)");
                Console.WriteLine("0 - Выход");
                Console.WriteLine("==========================================================================");
                Console.Write("Выберите пункт меню (0-6): ");

                string? choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                if (choice == "0")
                {
                    Console.WriteLine("Выход из программы.");
                    return;
                }

                ExecuteChoice(choice ?? "");

                Console.WriteLine("Нажмите Enter для продолжения...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private static void ExecuteChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    Task1_SequentialModified.Run();
                    break;
                case "2":
                    Task2_DataDecomposition.Run();
                    break;
                case "3":
                    Task3_PrimeDecomposition.Run();
                    break;
                case "4":
                    Task4_ThreadPoolSieve.Run();
                    break;
                case "5":
                    Task5_DynamicPrimeQueue.Run();
                    break;
                case "6":
                    BenchmarkCompareAll.Run();
                    break;
                case "7":
                    BenchmarkJsonExport.RunExport();
                    break;
                default:
                    Console.WriteLine("Неверный ввод. Пожалуйста, укажите цифру от 1 до 6.\n");
                    break;
            }
        }
    }
}
