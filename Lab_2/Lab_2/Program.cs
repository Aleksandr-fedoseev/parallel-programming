using System;
using System.Text;

namespace Lab_2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine("ЛАБОРАТОРНАЯ РАБОТА № 2: МЕНЮ ЗАПУСКА");
            Console.WriteLine("==================================================");
            Console.WriteLine("1 - Задание 1 (Последовательная обработка)");
            Console.WriteLine("2 - Задание 2 (Многопоточная обработка по диапазону)");
            Console.WriteLine("3 - Задания 3 и 4 (Таблица 1: Простая и усложненная обработка)");
            Console.WriteLine("4 - Задания 5 и 6 (Таблица 2: Диапазон vs Круг при неравномерной нагрузке)");
            Console.Write("Введите номер задания (1-4): ");

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Task1_Sequential.Run();
                    break;
                case "2":
                    Task2_MultiThreadedRange.Run();
                    break;
                case "3":
                    Task3_4_Table1.Run();
                    break;
                case "4":
                    Task5_6_Table2.Run();
                    break;
                default:
                    Console.WriteLine("Не выбран конкретный пункт. Запускаем Таблицу 2 (Задания 5 и 6):\n");
                    Task5_6_Table2.Run();
                    break;
            }

            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}