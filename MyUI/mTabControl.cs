// ---------------------------------------------------------------------------------
// File: mTabControl.cs
// Description: 自定义mTabControl控件
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyUI
{
    /// <summary>
    /// 自定义TabControl,新增的功能:
    /// 查看属性自定义分组内的内容
    /// </summary>
    public partial class mTabControl : TabControl
    {
        #region 字段
        /// <summary>
        /// 粗画笔
        /// </summary>
        Pen DeepPen = new Pen(Color.Gray, 2);
        /// <summary>
        /// 细画笔
        /// </summary>
        Pen LightPen = new Pen(Color.LightGray, 1);

        Color mSelectTapColor = default(Color);
        Color mNormalTapColor = default(Color);
        #endregion

        #region 属性
        private int _IconTopValue = 2;
        [Browsable(true), Description("图标离顶端的距离"), Category("自定义分组")]
        /// <summary>
        /// 图标离顶端的距离
        /// </summary>
        public int IconTopValue
        {
            get
            {
                return this._IconTopValue;
            }
            set
            {
                this._IconTopValue = value;
                this.Refresh();
            }
        }

        private int _TextTopValue = 2;
        [Browsable(true), Description("文字离顶端的距离"), Category("自定义分组")]
        /// <summary>
        /// 文字离顶端的距离
        /// </summary>
        public int TextTopValue
        {
            get
            {
                return this._TextTopValue;
            }
            set
            {
                this._TextTopValue = value;
                this.Refresh();
            }
        }

        private int _IconLeftValue = 8;
        [Browsable(true), Description("图标离左端的距离"), Category("自定义分组")]
        /// <summary>
        /// 图标离左端的距离
        /// </summary>
        public int IconLeftValue
        {
            get
            {
                return this._IconLeftValue;
            }
            set
            {
                this._IconLeftValue = value;
                this.Refresh();
            }
        }

        private Color _ContolerBackColor = default(Color);
        [Browsable(true), Description("控件背景颜色"), Category("自定义分组")]
        /// <summary>
        /// 控件背景颜色
        /// </summary>
        public Color ControlBackColor
        {
            get
            {
                return this._ContolerBackColor;
            }
            set
            {
                this._ContolerBackColor = value;
                this.Refresh();
            }
        }

        private Color _TabPageSelectBackColor = default(Color);
        [Browsable(true), Description("标题选中的背景颜色"), Category("自定义分组")]
        /// <summary>
        /// 标题选中的背景颜色
        /// </summary>
        public Color TabPageSelectBackColor
        {
            get
            {
                return this._TabPageSelectBackColor;
            }
            set
            {
                this._TabPageSelectBackColor = value;
                mSelectTapColor = value;
                this.Refresh();
            }
        }

        private Color _TabPageNormalBackColor = default(Color);
        [Browsable(true), Description("标题未选中的背景颜色"), Category("自定义分组")]
        /// <summary>
        /// 标题选中的背景颜色
        /// </summary>
        public Color TabPageNormalBackColor
        {
            get
            {
                return this._TabPageNormalBackColor;
            }
            set
            {
                this._TabPageNormalBackColor = value;
                mNormalTapColor = value;
                this.Refresh();
            }
        }

        private Color _TabPageSelectForeColor = Color.Black;
        [Browsable(true), Description("标题选中的字体颜色"), Category("自定义分组")]
        /// <summary>
        /// 标题选中的背景颜色
        /// </summary>
        public Color TabPageSelectForeColor
        {
            get
            {
                return this._TabPageSelectForeColor;
            }
            set
            {
                this._TabPageSelectForeColor = value;
                this.Refresh();
            }
        }

        private Color _TabPageNormalForeColor = Color.FromArgb(64, 64, 64);
        [Browsable(true), Description("标题未选中的字体颜色"), Category("自定义分组")]
        /// <summary>
        /// 标题选中的背景颜色
        /// </summary>
        public Color TabPageNormalForeColor
        {
            get
            {
                return this._TabPageNormalForeColor;
            }
            set
            {
                this._TabPageNormalForeColor = value;
                this.Refresh();
            }
        }

        [Browsable(false)]
        /// <summary>
        /// 重写TabPage填充的属性,满填
        /// </summary>
        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle _DisplayRectangle = base.DisplayRectangle;
                return new Rectangle(_DisplayRectangle.Left - 4, _DisplayRectangle.Top - 4, _DisplayRectangle.Width + 8, _DisplayRectangle.Height + 8);
            }
        }

        private Font _TabPageTitleFont = new Font("宋体", 9.5F);
        [Browsable(true), Description("标题的字体设置"), Category("自定义分组")]
        /// <summary>
        /// 标题的字体设置
        /// </summary>
        public Font TabPageTitleFont
        {
            get { return this._TabPageTitleFont; }
            set
            {
                this._TabPageTitleFont = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// 排列方式
        /// </summary>
        public enum LayoutMode
        {
            /// <summary>
            /// 水平排布
            /// </summary>
            Alignment,
            /// <summary>
            /// 垂直排布
            /// </summary>
            LineAlignment,
        }
        private LayoutMode _ItemLayoutMode = LayoutMode.Alignment;
        [Browsable(true), Description("设置选项卡内图标和文字的排列方式,当SizeMode为Fixed时有效"), Category("自定义分组")]
        ///<summary>
        /// 设置选项卡内图标和文字的排列方式,当SizeMode为Fixed有效
        /// </summary>
        public LayoutMode ItemLayoutMode
        {
            get { return this._ItemLayoutMode; }
            set
            {
                this._ItemLayoutMode = value;
                this.Refresh();
            }
        }
        #endregion

        public mTabControl()
        {
            this.SetStyle(ControlStyles.UserPaint
                        | ControlStyles.ResizeRedraw
                        | ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.DoubleBuffer, true);
            this.DoubleBuffered = true;
            this.SizeMode = TabSizeMode.Normal;

        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //事件引发
            base.OnPaint(e);

            #region 控件背景颜色渲染
            SolidBrush BrushControlBackColor = new SolidBrush(ControlBackColor);
            //获取整个TabControl的工作区域
            Rectangle TabControl_Rectangle = this.ClientRectangle;
            //填充整个TabControl的工作区域
            e.Graphics.FillRectangle(BrushControlBackColor, TabControl_Rectangle);
            #endregion

            #region 对每个TabPage进行渲染
            foreach (TabPage tp in this.TabPages)
            {
                DrawTabPage(e.Graphics, this.GetTabRect(this.TabPages.IndexOf(tp)), tp);
            }
            #endregion
        }
        /// <summary>
        /// 绘制每个TabPage
        /// </summary>
        /// <param name="graphics">绘画</param>
        /// <param name="rectangle">工作区域</param>
        /// <param name="tabpage">要绘制的TabPage</param>
        private void DrawTabPage(Graphics graphics, Rectangle rectangle, TabPage tabpage)
        {
            Rectangle Icon_Retangle;  //定义icon放置的位置
            Rectangle Text_Retangle;  //定义text放置的位置

            //定义TabPage标题文字布局
            StringFormat stringFormat = new StringFormat();
            //Text在定义的矩形内居中放置
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            if (this.SelectedTab != null)
            {
                //字体颜色
                tabpage.ForeColor = this.SelectedTab.Equals(tabpage) ? _TabPageSelectForeColor : _TabPageNormalForeColor;
                //背景颜色
                SolidBrush BrushBackColor = this.SelectedTab.Equals(tabpage) ? new SolidBrush(_TabPageSelectBackColor) : new SolidBrush(_TabPageNormalBackColor);
                graphics.FillRectangle(BrushBackColor, rectangle);
            }

            #region 图标和文字绘制
            if (this.ImageList != null &&
                ((!string.IsNullOrEmpty(tabpage.ImageKey) && tabpage.ImageKey.Contains(".")) || tabpage.ImageIndex != -1))
            {//有无图标判断
                string[] IconNameElement = null;
                Image mImage = null;
                int mIndex = -1;
                if (tabpage.ImageIndex == -1)
                {
                    #region 用ImageKey绑定图标的方式
                    //判断TabPage是否被选中,加载选中或没选中的图标
                    //注意规则:被选中的图标名称后面加个_s,如果找不到该图标,则用原名图标
                    if (this.SelectedTab != null && this.SelectedTab.Equals(tabpage))
                    {
                        IconNameElement = tabpage.ImageKey.Split('.');
                        mIndex = this.ImageList.Images.IndexOfKey(IconNameElement[0] + "_s." + IconNameElement[1]);
                        if (mIndex == -1)
                        {//找不到_s图标,则用原图标
                            mIndex = this.ImageList.Images.IndexOfKey(tabpage.ImageKey);
                        }
                    }
                    else
                    {
                        mIndex = this.ImageList.Images.IndexOfKey(tabpage.ImageKey);
                    }
                    if (mIndex == -1)
                    {
                        Text_Retangle = new Rectangle(rectangle.X, rectangle.Y + _TextTopValue, rectangle.Width, rectangle.Height);
                    }
                    else
                    {
                        mImage = this.ImageList.Images[mIndex];  //获取图标
                        if (this.SizeMode == TabSizeMode.Fixed)
                        {
                            //此模式下选项卡的大小由ItemSize决定
                            if (_ItemLayoutMode == LayoutMode.LineAlignment)
                            {
                                Icon_Retangle = new Rectangle(rectangle.X + (rectangle.Width - mImage.Width) / 2 + _IconLeftValue, rectangle.Y + _IconTopValue, mImage.Width, mImage.Height);
                                Text_Retangle = new Rectangle(rectangle.X, rectangle.Y + mImage.Height + _IconTopValue + 1, rectangle.Width, rectangle.Height - mImage.Height);
                            }
                            else
                            {
                                Icon_Retangle = new Rectangle(rectangle.X + _IconLeftValue, rectangle.Y + (rectangle.Height - mImage.Height) / 2 + _IconTopValue, mImage.Width, mImage.Height);
                                Text_Retangle = new Rectangle(Icon_Retangle.Right, rectangle.Y + _TextTopValue, rectangle.Width - Icon_Retangle.Width - _IconLeftValue, rectangle.Height);
                            }
                        }
                        else
                        {
                            Icon_Retangle = new Rectangle(rectangle.X + _IconLeftValue, rectangle.Y + (rectangle.Height - mImage.Height) / 2 + _IconTopValue, mImage.Width, mImage.Height);
                            Text_Retangle = new Rectangle(Icon_Retangle.Right, rectangle.Y + _TextTopValue, rectangle.Width - mImage.Width - _IconLeftValue, rectangle.Height);
                        }
                        graphics.DrawImage(mImage, Icon_Retangle);  //绘制图片
                    }
                    #endregion
                }
                else
                {
                    #region 用ImageIndex绑定图标的方式
                    if (this.SelectedTab != null && this.SelectedTab.Equals(tabpage))
                    {
                        IconNameElement = this.ImageList.Images.Keys[tabpage.ImageIndex].Split('.');
                        mIndex = this.ImageList.Images.IndexOfKey(IconNameElement[0] + "_s." + IconNameElement[1]);
                        if (mIndex == -1)
                        {//找不到_s图标,则用原图标
                            mIndex = this.ImageList.Images.IndexOfKey(tabpage.ImageKey);
                        }
                    }
                    else
                    {
                        mIndex = tabpage.ImageIndex;
                    }
                    if (mIndex == -1)
                    {
                        Text_Retangle = new Rectangle(rectangle.X, rectangle.Y + _TextTopValue, rectangle.Width, rectangle.Height);
                    }
                    else
                    {
                        mImage = this.ImageList.Images[mIndex];  //获取图标
                        if (this.SizeMode == TabSizeMode.Fixed)
                        {
                            //此模式下选项卡的大小由ItemSize决定
                            if (_ItemLayoutMode == LayoutMode.LineAlignment)
                            {
                                Icon_Retangle = new Rectangle(rectangle.X + (rectangle.Height - mImage.Width) / 2 + _IconLeftValue, rectangle.Y + _IconTopValue, mImage.Width, mImage.Height);
                                Text_Retangle = new Rectangle(rectangle.X, rectangle.Y + mImage.Height + _IconTopValue + 1, rectangle.Width, rectangle.Height - mImage.Height);
                            }
                            else
                            {
                                Icon_Retangle = new Rectangle(rectangle.X + _IconLeftValue, rectangle.Y + (rectangle.Height - mImage.Height) / 2 + _IconTopValue, mImage.Width, mImage.Height);
                                Text_Retangle = new Rectangle(Icon_Retangle.Right, rectangle.Y + _TextTopValue, rectangle.Width - mImage.Width - Icon_Retangle.Width, rectangle.Height);
                            }
                        }
                        else
                        {
                            Icon_Retangle = new Rectangle(rectangle.X + _IconLeftValue, rectangle.Y + (rectangle.Height - mImage.Height) / 2 + _IconTopValue, mImage.Width, mImage.Height);
                            Text_Retangle = new Rectangle(Icon_Retangle.Right, rectangle.Y + _TextTopValue, rectangle.Width - mImage.Width - _IconLeftValue, rectangle.Height);
                        }
                        graphics.DrawImage(mImage, Icon_Retangle);  //绘制图片
                    }
                    #endregion
                }
            }
            else
            {
                //没有图片直接获取整个tabpage的区域
                Text_Retangle = new Rectangle(rectangle.X, rectangle.Y + _TextTopValue, rectangle.Width, rectangle.Height);
            }
            if (_TabPageTitleFont == null)
            {
                _TabPageTitleFont = this.Font;
            }
            graphics.DrawString(tabpage.Text, _TabPageTitleFont, new SolidBrush(tabpage.ForeColor), Text_Retangle, stringFormat); //文字绘制
            #endregion

            #region 边框绘制
            if (this.SelectedTab != null && this.SelectedTab.Equals(tabpage))
            {
                if (this.Appearance == TabAppearance.Normal)
                {
                    //右
                    graphics.DrawLine(LightPen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom);
                }
                else
                {
                    //上
                    graphics.DrawLine(DeepPen, rectangle.X + 1, rectangle.Y, rectangle.Right, rectangle.Y);
                    //左
                    graphics.DrawLine(DeepPen, rectangle.X + 1, rectangle.Y, rectangle.X + 1, rectangle.Bottom + 1);
                    //右
                    graphics.DrawLine(LightPen, rectangle.Right, rectangle.Y, rectangle.Right, rectangle.Bottom);
                    //下
                    graphics.DrawLine(LightPen, rectangle.X + 1, rectangle.Bottom + 1, rectangle.Right, rectangle.Bottom + 1);
                }
            }
            else
            {
                if (this.Appearance == TabAppearance.Normal)
                {
                    //右
                    graphics.DrawLine(LightPen, rectangle.Right - 1, rectangle.Y, rectangle.Right - 1, rectangle.Bottom);
                }
                else
                {
                    //右
                    graphics.DrawLine(DeepPen, rectangle.Right, rectangle.Y, rectangle.Right, rectangle.Bottom);
                    //下
                    graphics.DrawLine(DeepPen, rectangle.X + 1, rectangle.Bottom + 1, rectangle.Right, rectangle.Bottom + 1);
                    //上
                    graphics.DrawLine(LightPen, rectangle.X + 1, rectangle.Y, rectangle.Right, rectangle.Y);
                    //左
                    graphics.DrawLine(LightPen, rectangle.X + 1, rectangle.Y, rectangle.X + 1, rectangle.Bottom + 1);
                }
            }
            #endregion

            stringFormat.Dispose();


        }

        /// <summary>
        /// 可以创建一块用线围起来的区域
        /// </summary>
        /// <param name="rect">初始区域</param>
        /// <returns>返回的区域</returns>
        private GraphicsPath CreateTabPath(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            rect.Width -= 1;
            path.AddLine(rect.X + 10, rect.Y, rect.Right - 10 / 2, rect.Y);
            path.CloseFigure();
            path.CloseFigure();
            return path;
        }

    }
}
