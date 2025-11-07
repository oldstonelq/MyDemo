// ---------------------------------------------------------------------------------
// File: mButton.cs
// Description: 自定义Button控件
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
    /// 自定义Button控件
    /// </summary>
    public partial class mButton : Button
    {
        /// <summary>
        /// 鼠标进入控件（黄色)
        /// </summary>
        public static Color FocusColor => Color.FromArgb(255, 242, 157);
        /// <summary>
        /// 鼠标离开控件（灰色）
        /// </summary>
        public static Color ButtonColor => Color.FromArgb(214, 219, 233);
        /// <summary>
        /// 构造函数
        /// </summary>
        public mButton()
        {
            InitializeComponent();
            InitStyle();
        }
        /// <summary>
        /// 加载样式
        /// </summary>
        private void InitStyle()
        {
            BackColor = SystemColors.GradientInactiveCaption;//背景色
            FlatStyle = FlatStyle.Flat;//平面样式
            FlatAppearance.BorderSize = 2;//边框大小
            MouseEnter += Button_MouseEnter;//鼠标进入控件事件
            MouseLeave += Button_MouseLeave;//鼠标离开控件事件
        }
        /// <summary>
        /// 鼠标进入控件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Button_MouseEnter(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.BackColor = FocusColor;
        }
        /// <summary>
        /// 鼠标离开控件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Button_MouseLeave(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            button.BackColor = ButtonColor;
        }
    }
}
