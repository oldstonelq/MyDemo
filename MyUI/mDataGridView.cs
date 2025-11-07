// ---------------------------------------------------------------------------------
// File: mDataGridView.cs
// Description: 自定义DataGridView控件
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
    /// 自定义DataGridView控件
    /// </summary>
    public partial class mDataGridView : DataGridView
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public mDataGridView()
        {
            InitializeComponent();
            InitStyle();
        }
        /// <summary>
        /// 加载样式
        /// </summary>
        private void InitStyle()
        {
            this.Dock = DockStyle.Fill;//占满容器
            this.AllowDrop = false ;//不允许拖拽数据
            this.AllowUserToAddRows=false;//不允许用户添加行
            this.AllowUserToDeleteRows=false;//不允许用户删除行
            this.AllowUserToOrderColumns=false;//不允许重新放置列
            this.AllowUserToResizeColumns=false;//不允许调整列的大小
            this.AllowUserToResizeRows=false;//不允许调整行的大小
            this.BackgroundColor = System.Drawing.Color.White;//背景色为白色
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;//单线边框
            this.RowHeadersVisible=false;//行标题列不可见
            this.ScrollBars = ScrollBars.Both;//底部和右侧都有滚动条

        }
    }
}
