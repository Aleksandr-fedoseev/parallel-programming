using System.Diagnostics;

namespace Lab_6
{
    // Окно игры: элементы формы описаны в LifeForm.Designer.cs, здесь — логика
    public partial class LifeForm : Form
    {
        Life life = new Life(100, 100);                // модель: поле и алгоритмы расчёта
        CancellationTokenSource? cts;                  // источник отмены (есть во время «Старт»)
        int generation;                                // номер текущего поколения

        public LifeForm()
        {
            InitializeComponent();                     // создаёт элементы из LifeForm.Designer.cs
            algBox.SelectedIndex = 1;                  // по умолчанию Parallel.For по строкам
            life.Randomize();
            Redraw();
            SetRunning(false);
        }

        // ===== Обработчики событий формы =====
        // «Случайно» — новое поле, ~30 % живых клеток (seed — текущее время, поле всегда новое)
        private void btnRandom_Click(object sender, EventArgs e)
        {
            NewLife();
            life.Randomize(0.3, Environment.TickCount);
            Redraw();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            NewLife();
            Redraw();
        }

        private void sizeBox_ValueChanged(object sender, EventArgs e)
        {
            NewLife();
            Redraw();
        }

        // «Шаг» — одно поколение; отмена не нужна, поэтому CancellationToken.None
        private async void btnStep_Click(object sender, EventArgs e) =>
            await StepOnce(CancellationToken.None);

        private async void btnStart_Click(object sender, EventArgs e) => await RunLoop();

        private void btnStop_Click(object sender, EventArgs e) =>
            cts?.Cancel();                              // отмена через CancellationToken

        private void canvas_Resize(object sender, EventArgs e) => canvas.Invalidate();

        // ===== Логика =====
        // Новое пустое поле размера из поля «Размер»
        void NewLife()
        {
            life = new Life((int)sizeBox.Value, (int)sizeBox.Value);
            generation = 0;
        }

        // Один шаг: расчёт в фоновом потоке, чтобы окно не зависало
        public async Task StepOnce(CancellationToken token)
        {
            var alg = (Algorithm)algBox.SelectedIndex;     // выбранный в списке алгоритм
            var sw = Stopwatch.StartNew();
            await Task.Run(() => life.Step(alg, 0, token), token);   // расчёт нового поколения
            sw.Stop();
            generation++;
            Redraw($"шаг {sw.Elapsed.TotalMilliseconds:F1} мс");
        }

        // «Старт»: шаги один за другим, пока не нажата «Стоп»
        async Task RunLoop()
        {
            cts = new CancellationTokenSource();       // его токен получат все шаги
            SetRunning(true);
            try
            {
                while (true)
                {
                    await StepOnce(cts.Token);
                    await Task.Delay(50, cts.Token);    // пауза, чтобы смену поколений было видно
                }
            }
            catch (Exception e) when (Life.IsCancel(e))  // «Стоп»: Cancel() → исключение отмены
            {
                status.Text += " — расчёт остановлен";
            }
            finally
            {
                SetRunning(false);
                cts.Dispose();
                cts = null;
            }
        }

        // Во время «Старт» доступна только «Стоп», остальные элементы отключены
        void SetRunning(bool running)
        {
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            btnStep.Enabled = btnRandom.Enabled = btnClear.Enabled = !running;
            sizeBox.Enabled = algBox.Enabled = !running;
        }

        // Обновить строку состояния и перерисовать поле (вызовет canvas_Paint)
        void Redraw(string extra = "")
        {
            status.Text = $"Поколение {generation}, живых клеток {life.Alive()}" +
                          (extra == "" ? "" : ", " + extra);
            canvas.Invalidate();
        }

        // Отрисовка поля: живые клетки — зелёные
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var t = life.Table;
            float cw = (float)canvas.ClientSize.Width / life.Width;     // ширина клетки, пиксели
            float ch = (float)canvas.ClientSize.Height / life.Height;
            for (int i = 0; i < life.Height; i++)
                for (int j = 0; j < life.Width; j++)
                    if (t[i, j])
                        e.Graphics.FillRectangle(Brushes.DarkGreen, j * cw, i * ch, cw, ch);
            if (cw >= 6)                                       // сетка, если клетки крупные
            {
                float w = life.Width * cw, h = life.Height * ch;
                for (int i = 0; i <= life.Height; i++)
                    e.Graphics.DrawLine(Pens.Gainsboro, 0, i * ch, w, i * ch);
                for (int j = 0; j <= life.Width; j++)
                    e.Graphics.DrawLine(Pens.Gainsboro, j * cw, 0, j * cw, h);
            }
        }

        // Щелчок по клетке — оживить или убить её
        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (cts != null) return;                   // во время «Старт» поле не редактируем
            int j = (int)(e.X * life.Width / (float)canvas.ClientSize.Width);
            int i = (int)(e.Y * life.Height / (float)canvas.ClientSize.Height);
            if (i < life.Height && j < life.Width) { life.Toggle(i, j); Redraw(); }
        }
    }
}
