using System.Diagnostics;

namespace Lab_4
{
    class Program
    {
        static string? buffer;                   // одноэлементный буфер
        static volatile bool bEmpty = true;      // true — буфер пуст
        static volatile bool finish;             // писатели закончили

        static string[][] writerMsgs = null!;    // сообщения каждого писателя
        static List<string>[] readerMsgs = null!; // что прочитал каждый читатель

        static readonly object lockRead = new(), lockWrite = new();
        static AutoResetEvent evFull = null!, evEmpty = null!;
        static Semaphore kFull = null!, kEmpty = null!;          // семафор ядра ОС
        static SemaphoreSlim semFull = null!, semEmpty = null!;  // облегчённый семафор
        static int readBusy, writeBusy;          // замки Interlocked: 0 — свободно

        // ===== 1. Без синхронизации =====
        static void ReaderNoSync(int id)
        {
            while (!finish)
                if (!bEmpty) { readerMsgs[id].Add(buffer!); bEmpty = true; }
        }
        static void WriterNoSync(int id)
        {
            int i = 0;
            while (i < writerMsgs[id].Length)
                if (bEmpty) { buffer = writerMsgs[id][i++]; bEmpty = false; }
        }

        // ===== 2a. lock =====
        static void ReaderLock(int id)
        {
            while (!finish)
                lock (lockRead)
                    if (!bEmpty) { readerMsgs[id].Add(buffer!); bEmpty = true; }
        }
        static void WriterLock(int id)
        {
            int i = 0;
            while (i < writerMsgs[id].Length)
                lock (lockWrite)
                    if (bEmpty) { buffer = writerMsgs[id][i++]; bEmpty = false; }
        }   

        // ===== 2b. Сигнальные сообщения AutoResetEvent =====
        static void ReaderEvent(int id)
        {
            while (true)
            {
                evFull.WaitOne();                       // ждём, пока буфер заполнят
                if (finish) { evFull.Set(); break; }    // будим следующего и выходим
                readerMsgs[id].Add(buffer!);
                bEmpty = true;
                evEmpty.Set();                          // буфер пуст — пускаем писателя
            }
        }
        static void WriterEvent(int id)
        {
            foreach (string msg in writerMsgs[id])
            {
                evEmpty.WaitOne();                      // ждём, пока буфер освободится
                buffer = msg;
                bEmpty = false;
                evFull.Set();                           // буфер полон — пускаем читателя
            }
        }

        // ===== 2c. Семафоры Semaphore =====
        // kEmpty — свободных мест в буфере (0 или 1), kFull — сообщений в буфере (0 или 1)
        static void ReaderKernelSemaphore(int id)
        {
            while (true)
            {
                kFull.WaitOne();                        // занимаем сообщение (или ждём)
                if (finish) { kFull.Release(); break; }
                readerMsgs[id].Add(buffer!);
                bEmpty = true;
                kEmpty.Release();                       // освободилось место
            }
        }
        static void WriterKernelSemaphore(int id)
        {
            foreach (string msg in writerMsgs[id])
            {
                kEmpty.WaitOne();                       // занимаем свободное место (или ждём)
                buffer = msg;
                bEmpty = false;
                kFull.Release();                        // появилось сообщение
            }
        }

        // ===== 2c. Семафоры SemaphoreSlim =====
        static void ReaderSemaphore(int id)
        {
            while (true)
            {
                semFull.Wait();
                if (finish) { semFull.Release(); break; }
                readerMsgs[id].Add(buffer!);
                bEmpty = true;
                semEmpty.Release();
            }
        }
        static void WriterSemaphore(int id)
        {
            foreach (string msg in writerMsgs[id])
            {
                semEmpty.Wait();
                buffer = msg;
                bEmpty = false;
                semFull.Release();
            }
        }

        // ===== 2d. Атомарные операции Interlocked =====
        // CompareExchange(ref x, 1, 0): если x == 0, записать 1 и вернуть 0 — вход
        static void ReaderInterlocked(int id)
        {
            while (!finish)
                if (!bEmpty && Interlocked.CompareExchange(ref readBusy, 1, 0) == 0)
                {
                    if (!bEmpty) { readerMsgs[id].Add(buffer!); bEmpty = true; }
                    Interlocked.Exchange(ref readBusy, 0);   // выход
                }
        }
        static void WriterInterlocked(int id)
        {
            int i = 0;
            while (i < writerMsgs[id].Length)
                if (bEmpty && Interlocked.CompareExchange(ref writeBusy, 1, 0) == 0)
                {
                    if (bEmpty) { buffer = writerMsgs[id][i++]; bEmpty = false; }
                    Interlocked.Exchange(ref writeBusy, 0);
                }
        }

        static readonly string[] names =
            { "Без синхронизации", "lock", "AutoResetEvent", "Semaphore", "SemaphoreSlim",
              "Interlocked" };
        static readonly Action<int>[] readers =
            { ReaderNoSync, ReaderLock, ReaderEvent, ReaderKernelSemaphore, ReaderSemaphore,
              ReaderInterlocked };
        static readonly Action<int>[] writers =
            { WriterNoSync, WriterLock, WriterEvent, WriterKernelSemaphore, WriterSemaphore,
              WriterInterlocked };

        // ===== Один запуск: время, прочитано, повторы, потери =====
        static (double ms, int read, int dup, int lost) Run(int v, int threads,
                                                            int total, int size)
        {
            bEmpty = true; finish = false; readBusy = writeBusy = 0;
            evFull = new AutoResetEvent(false); evEmpty = new AutoResetEvent(true);
            kFull = new Semaphore(0, 1);        kEmpty = new Semaphore(1, 1);
            semFull = new SemaphoreSlim(0, 1);  semEmpty = new SemaphoreSlim(1, 1);

            string text = new string('x', size);
            int n = total / threads;
            writerMsgs = new string[threads][];
            readerMsgs = new List<string>[threads];
            for (int t = 0; t < threads; t++)
            {
                writerMsgs[t] = new string[n];
                for (int i = 0; i < n; i++) writerMsgs[t][i] = $"{t}_{i}_{text}";
                readerMsgs[t] = new List<string>();
            }

            Stopwatch sw = Stopwatch.StartNew();
            var r = new Thread[threads];
            var w = new Thread[threads];
            for (int t = 0; t < threads; t++)
            {
                int id = t;
                r[t] = new Thread(() => readers[v](id));
                w[t] = new Thread(() => writers[v](id));
            }
            foreach (var t in r) t.Start();
            foreach (var t in w) t.Start();

            foreach (var t in w) t.Join();     // ждём писателей
            while (!bEmpty) { }                // ждём, пока прочитают последнее
            finish = true;                     // сигнал читателям
            evFull.Set(); kFull.Release(); semFull.Release();   // будим спящих читателей
            foreach (var t in r) t.Join();     // ждём читателей
            sw.Stop();

            var all = readerMsgs.SelectMany(x => x).ToList();
            int unique = all.Distinct().Count();
            return (sw.Elapsed.TotalMilliseconds, all.Count,
                    all.Count - unique, n * threads - unique);
        }

        // ===== Таблица: среднее время по 3 запускам =====
        // config(c) — параметры для столбца c: (потоков, сообщений, размер)
        static void Experiment(string title, string[] columns,
                               Func<int, (int threads, int total, int size)> config)
        {
            Console.WriteLine(title);
            Console.Write($"{"Средство",-16}");
            foreach (string c in columns) Console.Write($"{c,12}");
            Console.WriteLine();
            for (int v = 1; v < names.Length; v++)
            {
                Console.Write($"{names[v],-16}");
                for (int c = 0; c < columns.Length; c++)
                {
                    var (threads, total, size) = config(c);
                    double sum = 0;
                    for (int run = 0; run < 3; run++) sum += Run(v, threads, total, size).ms;
                    double avg = sum / 3;
                    Console.Write($"{avg,12:F1}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            for (int v = 0; v < names.Length; v++) Run(v, 2, 1000, 10);   // прогрев

            Console.WriteLine("=== Задание 1. Без синхронизации (10 000 сообщений) ===");
            foreach (int t in new[] { 1, 2, 4 })
            {
                var (_, read, dup, lost) = Run(0, t, 10_000, 10);
                Console.WriteLine($"{t} + {t}: прочитано {read}, повторов {dup}, потеряно {lost}");
            }

            Console.WriteLine("\n=== Задание 2. Проверка (4 + 4 потока, 10 000 сообщений) ===");
            for (int v = 1; v < names.Length; v++)
            {
                var (_, read, dup, lost) = Run(v, 4, 10_000, 10);
                Console.WriteLine($"{names[v],-16} прочитано {read}, " +
                                  $"повторов {dup}, потеряно {lost}");
            }

            Console.WriteLine("\n=== Задание 3. Среднее время по 3 запускам, мс ===");
            int[] counts = { 1_000, 10_000, 100_000 };
            int[] sizes = { 10, 1_000, 10_000 };
            int[] threads = { 1, 4, 8 };
            Experiment("Число сообщений (2 + 2 потока, 10 символов)",
                new[] { "1 000", "10 000", "100 000" }, c => (2, counts[c], 10));
            Experiment("Размер сообщения (2 + 2 потока, 10 000 сообщений)",
                new[] { "10", "1 000", "10 000" }, c => (2, 10_000, sizes[c]));
            Experiment("Число потоков (100 000 сообщений, 10 символов)",
                new[] { "1+1", "4+4", "8+8" }, c => (threads[c], 100_000, 10));
        }
    }
}
