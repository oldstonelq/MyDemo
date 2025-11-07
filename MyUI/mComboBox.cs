// ---------------------------------------------------------------------------------
// File: mComboBox.cs
// Description: 自定义ComboBox控件
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
    /// 自定义ComboBox控件
    /// </summary>
    public partial class mComboBox : ComboBox
    {
        /// <summary>
        /// 鼠标离开控件颜色（深蓝）
        /// </summary>
        public static Color Theme_Second => Color.FromArgb(59, 95, 147);
        /// <summary>
        /// 鼠标指针移动到控件上颜色（黄色）
        /// </summary>
        public static Color FocusColor => Color.FromArgb(255, 242, 157);
        /// <summary>
        /// 构造函数
        /// </summary>
        public mComboBox()
        {
            InitializeComponent();
            InitStyle();
        }
        /// <summary>
        /// 加载样式
        /// </summary>
        private void InitStyle()
        {
            BackColor = Theme_Second;//背景色
            ForeColor = Color.White;//前景色
            DropDownStyle = ComboBoxStyle.DropDownList;//下拉框样式
            MouseLeave += ComboBox_MouseLeave;//鼠标离开控件事件
            MouseMove += ComboBox_MouseMove;//鼠标指针移动到控件上事件
            SelectionChangeCommitted += ComboBox_SelectionChangeCommitted;//更改选项事件
        }
        /// <summary>
        /// 鼠标离开控件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ComboBox_MouseLeave(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.BackColor = Theme_Second;
            comboBox.ForeColor = Color.White;
        }
        /// <summary>
        /// 鼠标指针移动到控件上事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.BackColor = FocusColor;
            comboBox.ForeColor = Color.Black;
        }
        /// <summary>
        /// 更改选项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.BackColor = Theme_Second;
            comboBox.ForeColor = Color.White;
        }
    }
}
