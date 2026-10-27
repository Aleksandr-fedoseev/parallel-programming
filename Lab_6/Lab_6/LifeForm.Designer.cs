namespace Lab_6
{
    partial class LifeForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelTop = new FlowLayoutPanel();
            lblSize = new Label();
            sizeBox = new NumericUpDown();
            btnRandom = new Button();
            btnClear = new Button();
            btnStep = new Button();
            btnStart = new Button();
            btnStop = new Button();
            algBox = new ComboBox();
            canvas = new PictureBox();
            statusStrip = new StatusStrip();
            status = new ToolStripStatusLabel();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sizeBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // panelTop
            //
            panelTop.AutoSize = true;
            panelTop.Controls.Add(lblSize);
            panelTop.Controls.Add(sizeBox);
            panelTop.Controls.Add(btnRandom);
            panelTop.Controls.Add(btnClear);
            panelTop.Controls.Add(btnStep);
            panelTop.Controls.Add(btnStart);
            panelTop.Controls.Add(btnStop);
            panelTop.Controls.Add(algBox);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(4);
            panelTop.Size = new Size(760, 39);
            panelTop.TabIndex = 0;
            //
            // lblSize
            //
            lblSize.AutoSize = true;
            lblSize.Location = new Point(7, 11);
            lblSize.Margin = new Padding(3, 7, 3, 0);
            lblSize.Name = "lblSize";
            lblSize.Size = new Size(50, 15);
            lblSize.TabIndex = 0;
            lblSize.Text = "Размер:";
            //
            // sizeBox
            //
            sizeBox.Location = new Point(63, 7);
            sizeBox.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            sizeBox.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            sizeBox.Name = "sizeBox";
            sizeBox.Size = new Size(60, 23);
            sizeBox.TabIndex = 1;
            sizeBox.Value = new decimal(new int[] { 100, 0, 0, 0 });
            sizeBox.ValueChanged += sizeBox_ValueChanged;
            //
            // btnRandom
            //
            btnRandom.AutoSize = true;
            btnRandom.Location = new Point(129, 7);
            btnRandom.Name = "btnRandom";
            btnRandom.Size = new Size(75, 25);
            btnRandom.TabIndex = 2;
            btnRandom.Text = "Случайно";
            btnRandom.UseVisualStyleBackColor = true;
            btnRandom.Click += btnRandom_Click;
            //
            // btnClear
            //
            btnClear.AutoSize = true;
            btnClear.Location = new Point(210, 7);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 25);
            btnClear.TabIndex = 3;
            btnClear.Text = "Очистить";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            //
            // btnStep
            //
            btnStep.AutoSize = true;
            btnStep.Location = new Point(291, 7);
            btnStep.Name = "btnStep";
            btnStep.Size = new Size(75, 25);
            btnStep.TabIndex = 4;
            btnStep.Text = "Шаг";
            btnStep.UseVisualStyleBackColor = true;
            btnStep.Click += btnStep_Click;
            //
            // btnStart
            //
            btnStart.AutoSize = true;
            btnStart.Location = new Point(372, 7);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 25);
            btnStart.TabIndex = 5;
            btnStart.Text = "Старт";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            //
            // btnStop
            //
            btnStop.AutoSize = true;
            btnStop.Enabled = false;
            btnStop.Location = new Point(453, 7);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(75, 25);
            btnStop.TabIndex = 6;
            btnStop.Text = "Стоп";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            //
            // algBox
            //
            algBox.DropDownStyle = ComboBoxStyle.DropDownList;
            algBox.FormattingEnabled = true;
            algBox.Items.AddRange(new object[] {
                "Последовательный",
                "Parallel.For по строкам",
                "Parallel.For по столбцам",
                "Задачи (Tasks)" });
            algBox.Location = new Point(534, 7);
            algBox.Name = "algBox";
            algBox.Size = new Size(190, 23);
            algBox.TabIndex = 7;
            //
            // canvas
            //
            canvas.BackColor = Color.White;
            canvas.Dock = DockStyle.Fill;
            canvas.Location = new Point(0, 39);
            canvas.Name = "canvas";
            canvas.Size = new Size(760, 659);
            canvas.TabIndex = 1;
            canvas.TabStop = false;
            canvas.Paint += canvas_Paint;
            canvas.MouseClick += canvas_MouseClick;
            canvas.Resize += canvas_Resize;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { status });
            statusStrip.Location = new Point(0, 698);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(760, 22);
            statusStrip.TabIndex = 2;
            //
            // status
            //
            status.Name = "status";
            status.Size = new Size(84, 17);
            status.Text = "Поколение 0";
            //
            // LifeForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 720);
            Controls.Add(canvas);
            Controls.Add(statusStrip);
            Controls.Add(panelTop);
            Name = "LifeForm";
            Text = "Игра «Жизнь»";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sizeBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel panelTop;
        private Label lblSize;
        private NumericUpDown sizeBox;
        private Button btnRandom;
        private Button btnClear;
        private Button btnStep;
        private Button btnStart;
        private Button btnStop;
        private ComboBox algBox;
        private PictureBox canvas;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel status;
    }
}
