namespace MyDemo
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.mTableLayoutPanel1 = new MyUI.mTableLayoutPanel();
            this.mPanel1 = new MyUI.mPanel();
            this.mComboBox1 = new MyUI.mComboBox();
            this.mButton1 = new MyUI.mButton();
            this.mPanel2 = new MyUI.mPanel();
            this.mDataGridView1 = new MyUI.mDataGridView();
            this.mPanel3 = new MyUI.mPanel();
            this.mPanel4 = new MyUI.mPanel();
            this.mTextBox1 = new MyUI.mTextBox();
            this.mTableLayoutPanel1.SuspendLayout();
            this.mPanel1.SuspendLayout();
            this.mPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mDataGridView1)).BeginInit();
            this.mPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // mTableLayoutPanel1
            // 
            this.mTableLayoutPanel1.ColumnCount = 2;
            this.mTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mTableLayoutPanel1.Controls.Add(this.mPanel1, 0, 0);
            this.mTableLayoutPanel1.Controls.Add(this.mPanel2, 1, 0);
            this.mTableLayoutPanel1.Controls.Add(this.mPanel3, 0, 1);
            this.mTableLayoutPanel1.Controls.Add(this.mPanel4, 1, 1);
            this.mTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.mTableLayoutPanel1.Name = "mTableLayoutPanel1";
            this.mTableLayoutPanel1.RowCount = 2;
            this.mTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mTableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.mTableLayoutPanel1.TabIndex = 0;
            // 
            // mPanel1
            // 
            this.mPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mPanel1.Controls.Add(this.mComboBox1);
            this.mPanel1.Controls.Add(this.mButton1);
            this.mPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel1.Location = new System.Drawing.Point(3, 3);
            this.mPanel1.Name = "mPanel1";
            this.mPanel1.Size = new System.Drawing.Size(394, 219);
            this.mPanel1.TabIndex = 0;
            // 
            // mComboBox1
            // 
            this.mComboBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(95)))), ((int)(((byte)(147)))));
            this.mComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mComboBox1.ForeColor = System.Drawing.Color.White;
            this.mComboBox1.FormattingEnabled = true;
            this.mComboBox1.Items.AddRange(new object[] {
            "1",
            "2",
            "3"});
            this.mComboBox1.Location = new System.Drawing.Point(161, 18);
            this.mComboBox1.Name = "mComboBox1";
            this.mComboBox1.Size = new System.Drawing.Size(121, 20);
            this.mComboBox1.TabIndex = 1;
            // 
            // mButton1
            // 
            this.mButton1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.mButton1.FlatAppearance.BorderSize = 2;
            this.mButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mButton1.Location = new System.Drawing.Point(19, 8);
            this.mButton1.Name = "mButton1";
            this.mButton1.Size = new System.Drawing.Size(75, 39);
            this.mButton1.TabIndex = 0;
            this.mButton1.Text = "mButton1";
            this.mButton1.UseVisualStyleBackColor = false;
            // 
            // mPanel2
            // 
            this.mPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mPanel2.Controls.Add(this.mDataGridView1);
            this.mPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel2.Location = new System.Drawing.Point(403, 3);
            this.mPanel2.Name = "mPanel2";
            this.mPanel2.Size = new System.Drawing.Size(394, 219);
            this.mPanel2.TabIndex = 1;
            // 
            // mDataGridView1
            // 
            this.mDataGridView1.AllowUserToAddRows = false;
            this.mDataGridView1.AllowUserToDeleteRows = false;
            this.mDataGridView1.AllowUserToResizeColumns = false;
            this.mDataGridView1.AllowUserToResizeRows = false;
            this.mDataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.mDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mDataGridView1.Location = new System.Drawing.Point(0, 0);
            this.mDataGridView1.Name = "mDataGridView1";
            this.mDataGridView1.RowHeadersVisible = false;
            this.mDataGridView1.RowTemplate.Height = 23;
            this.mDataGridView1.Size = new System.Drawing.Size(392, 217);
            this.mDataGridView1.TabIndex = 0;
            // 
            // mPanel3
            // 
            this.mPanel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mPanel3.Controls.Add(this.mTextBox1);
            this.mPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel3.Location = new System.Drawing.Point(3, 228);
            this.mPanel3.Name = "mPanel3";
            this.mPanel3.Size = new System.Drawing.Size(394, 219);
            this.mPanel3.TabIndex = 2;
            // 
            // mPanel4
            // 
            this.mPanel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPanel4.Location = new System.Drawing.Point(403, 228);
            this.mPanel4.Name = "mPanel4";
            this.mPanel4.Size = new System.Drawing.Size(394, 219);
            this.mPanel4.TabIndex = 3;
            // 
            // mTextBox1
            // 
            this.mTextBox1.BackColor = System.Drawing.SystemColors.Info;
            this.mTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mTextBox1.Location = new System.Drawing.Point(0, 0);
            this.mTextBox1.Multiline = true;
            this.mTextBox1.Name = "mTextBox1";
            this.mTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.mTextBox1.Size = new System.Drawing.Size(392, 217);
            this.mTextBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mTableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.mTableLayoutPanel1.ResumeLayout(false);
            this.mPanel1.ResumeLayout(false);
            this.mPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mDataGridView1)).EndInit();
            this.mPanel3.ResumeLayout(false);
            this.mPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MyUI.mTableLayoutPanel mTableLayoutPanel1;
        private MyUI.mPanel mPanel1;
        private MyUI.mPanel mPanel2;
        private MyUI.mPanel mPanel3;
        private MyUI.mPanel mPanel4;
        private MyUI.mButton mButton1;
        private MyUI.mComboBox mComboBox1;
        private MyUI.mDataGridView mDataGridView1;
        private MyUI.mTextBox mTextBox1;
    }
}

