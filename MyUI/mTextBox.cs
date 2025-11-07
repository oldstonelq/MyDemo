// ---------------------------------------------------------------------------------
// File: mTextBox.cs
// Description: 自定义TextBox控件
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyUI
{
    /// <summary>
    /// 自定义TextBox控件
    /// </summary>
    public partial class mTextBox : TextBox 
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public mTextBox()
        {
            InitializeComponent();
            InitStyle();
        }
        /// <summary>
        /// 加载样式
        /// </summary>
        private void InitStyle()
        {
            this.Dock = DockStyle.Fill;//填满整个窗体
            this.Multiline = true;//多行
            this.ScrollBars = ScrollBars.Both;//底部和右侧滚动条
            this.BorderStyle = BorderStyle.FixedSingle;//单线框
            this.BackColor = SystemColors.Info;//设置背景色（黄色）

        }

    }
}
