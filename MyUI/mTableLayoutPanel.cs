// ---------------------------------------------------------------------------------
// File: mTableLayoutPanel.cs
// Description: 自定义TableLayoutPanel控件
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyUI
{
    /// <summary>
    /// 自定义TableLayoutPanel控件
    /// </summary>
    public partial class mTableLayoutPanel : TableLayoutPanel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public mTableLayoutPanel()
        {
            InitializeComponent();
            InitStyle();
        }
        /// <summary>
        /// 加载样式
        /// </summary>
        private void InitStyle()
        {
            this.Dock = DockStyle.Fill;


        }
    }
}
