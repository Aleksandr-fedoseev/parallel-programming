using System.Diagnostics;
using System.Text;

namespace Lab_6
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Lab_6.exe bench — анализ эффективности (п. 5), результаты в out6.txt
            if (args.Length > 0 && args[0] == "bench") { Benchmark("out6.txt"); return; }

            ApplicationConfiguration.Initialize();     // стандартная настройка WinForms
            var form = new LifeForm();
            // Lab_6.exe screenshot — 30 поколений и снимок окна для отчёта
            if (args.Length > 0 && args[0] == "screenshot")
                form.Shown += async (_, _) =>
                {
                    for (int g = 0; g < 30; g++) await form.StepOnce(CancellationToken.None);
                    using var bmp = new Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
                    bmp.Save("screenshot.png");
                    form.Close();
                };
            Application.Run(form);                     // показать окно и обрабатывать события
        }

        // Среднее время одного поколения, мс (3 запуска по gens поколений)
        static double Measure(Algorithm alg, int size, int gens, int tasks = 0)
        {
            double sum = 0;
            for (int run = 0; run < 3; run++)
            {
                var life = new Life(size, size);
                life.Randomize(0.3, 42);               // одно и то же поле для всех алгоритмов
                var sw = Stopwatch.StartNew();
                for (int g = 0; g < gens; g++) life.Step(alg, tasks);
                sum += sw.Elapsed.TotalMilliseconds / gens;
            }
            return sum / 3;
        }

        static void Benchmark(string path)
        {
            var sb = new StringBuilder();
            foreach (Algorithm a in Enum.GetValues<Algorithm>()) Measure(a, 100, 5);  // прогрев

            // Проверка: все алгоритмы дают одинаковый результат
            var results = Enum.GetValues<Algorithm>().Select(a =>
            {
                var life = new Life(300, 300);
                life.Randomize(0.3, 7);
                for (int g = 0; g < 50; g++) life.Step(a);
                return life.Table.Cast<bool>().ToArray();
            }).ToList();
            bool same = results.All(r => r.SequenceEqual(results[0]));
            sb.AppendLine($"CHECK {(same ? "ok" : "FAIL")}");

            // 1) Время поколения при разном размере поля
            foreach (int size in new[] { 100, 500, 1000, 2000 })
            {
                int gens = size <= 500 ? 20 : 5;
                var times = Enum.GetValues<Algorithm>().Select(a => Measure(a, size, gens));
                string row = string.Join(" ", times.Select(t => t.ToString("F2")));
                sb.AppendLine($"SIZE {size} {row}");
            }

            // 2) Алгоритм Tasks при разном числе задач (поле 1000 × 1000)
            foreach (int m in new[] { 1, 2, 4, 6, 8, 12, 16 })
                sb.AppendLine($"TASKS {m} {Measure(Algorithm.Tasks, 1000, 5, m):F2}");

            // 3) Отмена: расчёт 1000 поколений поля 2000 × 2000, через 300 мс — Cancel()
            foreach (Algorithm a in Enum.GetValues<Algorithm>())
            {
                var life = new Life(2000, 2000);
                life.Randomize(0.3, 42);
                using var cts = new CancellationTokenSource();
                int done = 0;
                var task = Task.Run(() =>
                {
                    for (int g = 0; g < 1000; g++) { life.Step(a, 0, cts.Token); done++; }
                });
                Thread.Sleep(300);                     // даём расчёту поработать 300 мс
                var sw = Stopwatch.StartNew();
                cts.Cancel();                          // то же, что кнопка «Стоп»
                string result;
                try { task.Wait(); result = "не отменён"; }
                catch (Exception e) when (Life.IsCancel(e)) { result = "отменён"; }
                double stopMs = sw.Elapsed.TotalMilliseconds;   // сколько шла остановка
                sb.AppendLine($"CANCEL {(int)a} {result} {done} {stopMs:F1}");
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
