using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Lab_3
{
    public class Lab3BenchmarkResult
    {
        public int RunNumber { get; set; }
        public Dictionary<int, double> SequentialMs { get; set; } = new();
        public Dictionary<int, double> ThreadPoolMs { get; set; } = new();
        public Dictionary<int, Dictionary<int, double>> Alg1DataMs { get; set; } = new();
        public Dictionary<int, Dictionary<int, double>> Alg2PrimeMs { get; set; } = new();
        public Dictionary<int, Dictionary<int, double>> Alg4DynamicMs { get; set; } = new();
    }

    public static class BenchmarkJsonExport
    {
        public static void RunExport()
        {
            int[] nValues = { 1_000_000, 5_000_000, 10_000_000 };
            int[] mValues = { 2, 4, 6, 8, 12 };

            List<Lab3BenchmarkResult> allRuns = new();

            for (int run = 1; run <= 3; run++)
            {
                Console.WriteLine($"\n=== ПРОГОН №{run} ===");
                var result = new Lab3BenchmarkResult { RunNumber = run };

                foreach (int n in nValues)
                {
                    Console.WriteLine($"  Диапазон N = {n:N0}...");

                    // Sequential
                    result.SequentialMs[n] = MeasureSequential(n);

                    // ThreadPool (Alg 3)
                    result.ThreadPoolMs[n] = Task4_ThreadPoolSieve.Run(n, printInfo: false);

                    result.Alg1DataMs[n] = new Dictionary<int, double>();
                    result.Alg2PrimeMs[n] = new Dictionary<int, double>();
                    result.Alg4DynamicMs[n] = new Dictionary<int, double>();

                    foreach (int m in mValues)
                    {
                        result.Alg1DataMs[n][m] = Task2_DataDecomposition.Run(n, m, printInfo: false);
                        result.Alg2PrimeMs[n][m] = Task3_PrimeDecomposition.Run(n, m, printInfo: false);
                        result.Alg4DynamicMs[n][m] = Task5_DynamicPrimeQueue.Run(n, m, useInterlocked: true, printInfo: false);
                    }
                }

                allRuns.Add(result);
            }

            string json = JsonSerializer.Serialize(allRuns, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("benchmark_lab3_results.json", json);
            Console.WriteLine("\n[УСПЕХ] Результаты 3 прогонов сохранены в benchmark_lab3_results.json!");
        }

        private static double MeasureSequential(int n)
        {
            int sqrtN = (int)Math.Sqrt(n);
            bool[] baseComposite = new bool[sqrtN + 1];
            List<int> basePrimesList = new List<int>();

            for (int p = 2; p <= sqrtN; p++)
            {
                if (!baseComposite[p])
                {
                    basePrimesList.Add(p);
                    for (int multiple = p * p; multiple <= sqrtN; multiple += p)
                    {
                        baseComposite[multiple] = true;
                    }
                }
            }
            int[] basePrimes = basePrimesList.ToArray();

            bool[] isComposite = new bool[n + 1];

            Stopwatch sw = Stopwatch.StartNew();

            foreach (int p in basePrimes)
            {
                int start = Math.Max(p * p, ((sqrtN / p) + 1) * p);
                for (int multiple = start; multiple <= n; multiple += p)
                {
                    isComposite[multiple] = true;
                }
            }

            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
