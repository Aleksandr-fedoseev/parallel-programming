namespace Lab_6
{
    // Алгоритмы расчёта поколения (порядок совпадает со списком в окне)
    public enum Algorithm { Sequential, ParallelRows, ParallelColumns, Tasks }

    // Клеточная модель «Игра Жизнь» Дж. Конвея
    public class Life
    {
        public int Height { get; }                     // число строк поля (клеток по вертикали)
        public int Width { get; }                      // число столбцов (клеток по горизонтали)
        public bool[,] Table { get; private set; }     // поле: true — живая, false — мёртвая

        // Правила: потенциал 3 — клетка оживает, 2 — сохраняет состояние, иначе погибает.
        // p — число живых соседей, state — жива ли клетка сейчас; результат — будет ли жива
        static readonly Func<int, bool, bool> lifeRules = (p, state) =>
        {
            if (p == 3) return true;
            if (p == 2) return state;
            return false;
        };

        // Пустое поле заданного размера (все клетки мёртвые)
        public Life(int height, int width)
        {
            Height = height;
            Width = width;
            Table = new bool[height, width];
        }

        // Случайное начальное поле: каждая клетка живая с вероятностью density (30 %).
        // Одинаковый seed даёт одинаковое поле — нужно для честного сравнения алгоритмов
        public void Randomize(double density = 0.3, int seed = 1)
        {
            var rnd = new Random(seed);
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    Table[i, j] = rnd.NextDouble() < density;
        }

        public void Clear() => Table = new bool[Height, Width];          // все клетки мёртвые
        public void Toggle(int i, int j) => Table[i, j] = !Table[i, j];   // живая ↔ мёртвая

        // Сколько живых клеток на всём поле (для строки состояния окна)
        public int Alive()
        {
            int n = 0;
            foreach (bool c in Table) if (c) n++;
            return n;
        }

        // Потенциал клетки [i, j] — число живых соседей в квадрате 3 × 3 вокруг неё
        int CalcPotential(bool[,] t, int i, int j)
        {
            int p = 0;
            for (int x = i - 1; x <= i + 1; x++)           // строка выше, своя, ниже
                for (int y = j - 1; y <= j + 1; y++)       // столбец левее, свой, правее
                {
                    if (x < 0 || y < 0 || x >= Height || y >= Width) continue;   // за краем поля
                    if (x == i && y == j) continue;                              // сама клетка
                    if (t[x, y]) p++;                                            // живой сосед
                }
            return p;
        }

        // Новое состояние одной клетки: соседей считаем по СТАРОМУ полю t,
        // ответ пишем в НОВОЕ поле tNew (старое не меняется, пока считаются остальные клетки)
        void CalcCell(bool[,] t, bool[,] tNew, int i, int j) =>
            tNew[i, j] = lifeRules(CalcPotential(t, i, j), t[i, j]);

        // Один шаг (новое поколение) выбранным алгоритмом.
        // tasks — число задач для алгоритма Tasks; token — отмена расчёта.
        // Клетки независимы: все потоки только читают t и пишут разные ячейки tNew,
        // поэтому синхронизация между потоками не нужна.
        public void Step(Algorithm alg, int tasks = 0, CancellationToken token = default)
        {
            bool[,] t = Table, tNew = new bool[Height, Width];
            // отмена для Parallel.For передаётся через ParallelOptions
            var options = new ParallelOptions { CancellationToken = token };

            switch (alg)
            {
                case Algorithm.Sequential:                      // один поток, строка за строкой
                    for (int i = 0; i < Height; i++)
                    {
                        token.ThrowIfCancellationRequested();   // нажали «Стоп» → исключение
                        for (int j = 0; j < Width; j++) CalcCell(t, tNew, i, j);
                    }
                    break;

                case Algorithm.ParallelRows:                    // строки раздаёт потокам .NET
                    Parallel.For(0, Height, options, i =>
                    {
                        for (int j = 0; j < Width; j++) CalcCell(t, tNew, i, j);
                    });
                    break;

                case Algorithm.ParallelColumns:                 // столбцы раздаёт потокам .NET
                    Parallel.For(0, Width, options, j =>
                    {
                        for (int i = 0; i < Height; i++) CalcCell(t, tNew, i, j);
                    });
                    break;

                case Algorithm.Tasks:                           // m задач, у каждой блок строк
                    // по умолчанию задач столько, сколько логических процессоров
                    int m = tasks > 0 ? tasks : Environment.ProcessorCount;
                    var list = new Task[m];
                    for (int k = 0; k < m; k++)
                    {
                        // строки задачи k: [from, to); сначала умножаем, потом делим —
                        // так последняя задача заканчивает ровно на Height
                        int from = Height * k / m, to = Height * (k + 1) / m;
                        list[k] = Task.Run(() =>
                        {
                            for (int i = from; i < to; i++)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = 0; j < Width; j++) CalcCell(t, tNew, i, j);
                            }
                        }, token);
                    }
                    Task.WaitAll(list, token);                  // ждём все задачи (как Join)
                    break;
            }
            Table = tNew;      // новое поколение сохраняется, только если расчёт не отменён
        }

        // Исключение означает отмену расчёта (в том числе внутри задач)
        public static bool IsCancel(Exception e) =>
            e is OperationCanceledException ||
            e is AggregateException ae &&
            ae.Flatten().InnerExceptions.All(x => x is OperationCanceledException);
    }
}
