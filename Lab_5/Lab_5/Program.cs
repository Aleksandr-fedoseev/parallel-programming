using System.Diagnostics;

namespace Lab_5
{
    class Program
    {
        const int N = 10;                          // размер кольцевого буфера
        static readonly string[] buffer = new string[N];
        // Состояние ячейки: 0 — пусто, 1 — пишется, 2 — заполнено, 3 — читается
        static readonly int[] cell = new int[N];
        static int iW, iR;                         // индексы записи и чтения (меняет менеджер)

        // Сигналы рабочих: «готов» (ставит рабочий) и «начинай» (ставит менеджер)
        static ManualResetEventSlim[] wReady = null!, wStart = null!;   // писатели
        static ManualResetEventSlim[] rReady = null!, rStart = null!;   // читатели
        static int[] wIndex = null!, rIndex = null!;   // ячейка, выданная менеджером
        static int[] wPrio = null!, rPrio = null!;     // приоритеты: 1 — самый высокий
        static string[][] wMsgs = null!;               // сообщения каждого писателя
        static List<string>[] rMsgs = null!;           // что прочитал каждый читатель
        static double[] wTime = null!, rTime = null!;  // время работы каждого потока, мс   

        static void Writer(int id)
        {
            var sw = Stopwatch.StartNew();
            foreach (string msg in wMsgs[id])
            {
                wReady[id].Set();                  // готов писать
                wStart[id].Wait();                 // ждём разрешения менеджера
                wStart[id].Reset();
                int k = wIndex[id];
                buffer[k] = msg;                   // пишем в выданную ячейку
                Volatile.Write(ref cell[k], 2);    // ячейка заполнена
            }
            wTime[id] = sw.Elapsed.TotalMilliseconds;
        }

        static void Reader(int id)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                rReady[id].Set();                  // готов читать
                rStart[id].Wait();                 // ждём разрешения менеджера
                rStart[id].Reset();
                int k = rIndex[id];
                if (k < 0) break;                  // менеджер сообщил о завершении
                rMsgs[id].Add(buffer[k]);          // читаем из выданной ячейки
                Volatile.Write(ref cell[k], 0);    // ячейка свободна
            }
            rTime[id] = sw.Elapsed.TotalMilliseconds;
        }

        // Готовый поток с наивысшим приоритетом; среди равных — по очереди
        static int Pick(ManualResetEventSlim[] ready, int[] prio, ref int last)
        {
            int best = -1;
            for (int j = 1; j <= ready.Length; j++)
            {
                int i = (last + j) % ready.Length;
                if (ready[i].IsSet && (best < 0 || prio[i] < prio[best])) best = i;
            }
            if (best >= 0) last = best;
            return best;
        }

        // Менеджер (главный поток): выдаёт ячейки буфера готовым писателям и читателям
        static void Manager(int total)
        {
            int granted = 0, lastW = -1, lastR = -1;
            while (granted < total)
            {
                if (Volatile.Read(ref cell[iW]) == 0)          // есть свободная ячейка
                {
                    int w = Pick(wReady, wPrio, ref lastW);
                    if (w >= 0)
                    {
                        wReady[w].Reset();
                        cell[iW] = 1;
                        wIndex[w] = iW;
                        iW = (iW + 1) % N;          // по кольцу: после последней — первая
                        wStart[w].Set();
                    }
                }
                if (Volatile.Read(ref cell[iR]) == 2)          // есть заполненная ячейка
                {
                    int r = Pick(rReady, rPrio, ref lastR);
                    if (r >= 0)
                    {
                        rReady[r].Reset();
                        cell[iR] = 3;
                        rIndex[r] = iR;
                        iR = (iR + 1) % N;
                        granted++;
                        rStart[r].Set();
                    }
                }
            }
            for (int k = 0; k < N; k++)                       // ждём, пока дочитают
                while (Volatile.Read(ref cell[k]) != 0) { }
            for (int r = 0; r < rStart.Length; r++)            // сигнал читателям о завершении
            {
                rIndex[r] = -1;
                rStart[r].Set();
            }
        }

        record Stats(double Total, int[] WCount, double[] WTime,
                     int[] RCount, double[] RTime, bool Ok);

        static ManualResetEventSlim[] Events(int n) =>
            Enumerable.Range(0, n).Select(_ => new ManualResetEventSlim(false)).ToArray();

        // Один запуск: приоритеты писателей и читателей, всего сообщений, размер сообщения
        static Stats Run(int[] writerPrio, int[] readerPrio, int total, int size)
        {
            int W = writerPrio.Length, R = readerPrio.Length;
            wPrio = writerPrio; rPrio = readerPrio;
            iW = iR = 0;
            Array.Clear(cell);
            wReady = Events(W); wStart = Events(W); rReady = Events(R); rStart = Events(R);
            wIndex = new int[W]; rIndex = new int[R];
            wTime = new double[W]; rTime = new double[R];

            string text = new string('x', size);
            int perWriter = total / W;
            wMsgs = new string[W][];
            rMsgs = new List<string>[R];
            for (int w = 0; w < W; w++)
            {
                wMsgs[w] = new string[perWriter];
                for (int i = 0; i < perWriter; i++) wMsgs[w][i] = $"{w}_{i}_{text}";
            }
            for (int r = 0; r < R; r++) rMsgs[r] = new List<string>();

            var sw = Stopwatch.StartNew();
            var threads = new List<Thread>();
            for (int w = 0; w < W; w++)
            {
                int id = w;
                threads.Add(new Thread(() => Writer(id)));
            }
            for (int r = 0; r < R; r++)
            {
                int id = r;
                threads.Add(new Thread(() => Reader(id)));
            }
            threads.ForEach(t => t.Start());
            Manager(perWriter * W);
            threads.ForEach(t => t.Join());
            sw.Stop();

            var all = rMsgs.SelectMany(m => m).ToList();
            bool ok = all.Count == perWriter * W && all.Distinct().Count() == all.Count;
            return new Stats(sw.Elapsed.TotalMilliseconds,
                             wMsgs.Select(m => m.Length).ToArray(), wTime,
                             rMsgs.Select(m => m.Count).ToArray(), rTime, ok);
        }

        static void Print(Stats s, int[] wp, int[] rp)
        {
            for (int w = 0; w < wp.Length; w++)
                Console.WriteLine($"  Писатель {w + 1} (приоритет {wp[w]}): " +
                                  $"сообщений {s.WCount[w]}, время {s.WTime[w]:F1} мс");
            for (int r = 0; r < rp.Length; r++)
                Console.WriteLine($"  Читатель {r + 1} (приоритет {rp[r]}): " +
                                  $"сообщений {s.RCount[r]}, время {s.RTime[r]:F1} мс");
            string ok = s.Ok ? "да" : "НЕТ";
            Console.WriteLine($"  Общее время {s.Total:F1} мс, " +
                              $"все сообщения прочитаны ровно один раз: {ok}");
        }

        static double Avg(int[] wp, int[] rp, int total, int size) =>
            Enumerable.Range(0, 3).Average(_ => Run(wp, rp, total, size).Total);

        static int[] Same(int n) => Enumerable.Repeat(1, n).ToArray();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Run(Same(2), Same(2), 1000, 10);   // прогрев

            Console.WriteLine("=== Задания 1–2. 2 писателя, 3 читателя, 10 000 сообщений ===");
            int[] wp = { 1, 2 }, rp = { 1, 2, 3 };
            Print(Run(wp, rp, 10_000, 10), wp, rp);

            Console.WriteLine("\n=== Задание 3. Число потоков (100 000 сообщений, " +
                              "10 символов), среднее по 3 запускам ===");
            foreach (int t in new[] { 1, 2, 4, 8 })
                Console.WriteLine($"  {t} + {t}: {Avg(Same(t), Same(t), 100_000, 10):F1} мс");

            Console.WriteLine("\n=== Задание 3. Размер сообщения (2 + 2 потока, " +
                              "10 000 сообщений), среднее по 3 запускам ===");
            foreach (int size in new[] { 10, 1_000, 10_000 })
                Console.WriteLine($"  {size} символов: " +
                                  $"{Avg(Same(2), Same(2), 10_000, size):F1} мс");

            Console.WriteLine("\n=== Задание 3. Приоритеты " +
                              "(2 писателя, 3 читателя, 10 000 сообщений) ===");
            int[][][] prios =
            {
                new[] { new[] { 1, 1 }, new[] { 1, 1, 1 } },
                new[] { new[] { 1, 2 }, new[] { 1, 2, 3 } },
                new[] { new[] { 2, 1 }, new[] { 3, 2, 1 } },
            };
            foreach (var p in prios)
            {
                Console.WriteLine($"Писатели [{string.Join(", ", p[0])}], " +
                                  $"читатели [{string.Join(", ", p[1])}]:");
                Print(Run(p[0], p[1], 10_000, 10), p[0], p[1]);
            }
        }
    }
}
