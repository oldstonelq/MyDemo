// ---------------------------------------------------------------------------------
// File: CSVFileTool.cs
// Description: CSV文件工具类
// Author: [刘晴]
// Create Date: 2025-11-07
// Last Modified: 2025-11-07
// Vison 1.0
// ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tools.FileHelp
{
    /// <summary>
    /// CSV文件工具类
    /// </summary>
    public  class CSVFileTool
    {
        /// <summary>
        /// CSV文件写入单行
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Mess">需要写入的信息内容</param>
        /// <param name="Filemode">文件打开模式</param>
        /// <returns>成功时返回ok,失败时返回错误内容</returns>
        public static string WriteCSV(string FilePath, string Mess, FileMode Filemode)
        {
            try
            {
                FileStream fs = new FileStream(FilePath, Filemode);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                sw.WriteLine(Mess);
                sw.Close();
                fs.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// CSV文件写入多行
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="Mess">写入信息内容</param>
        /// <param name="Filemode">文件打开模式</param>
        /// <returns>成功时返回ok,失败时返回错误内容</returns>
        public static string WriteCSV(string FilePath, string[] Mess, FileMode Filemode)
        {
            try
            {
                FileStream fs = new FileStream(FilePath, Filemode);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
                for (int i = 0; i < Mess.Length; i++)
                {
                    sw.WriteLine(Mess[i]);
                }
                sw.Close();
                fs.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 读取CSV文件内容
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <returns>返回字符串数组</returns>
        public static string[] ReadCSV(string FilePath)
        {
            try
            {
                StreamReader sr = new StreamReader(FilePath, Encoding.GetEncoding("gb2312"));
                List<string> mAllLine = new List<string>();
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    mAllLine.Add(line);
                }
                sr.Close();
                sr.Dispose();
                return mAllLine.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// DGV表格导出csv文件
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="fileName"></param>
        /// <param name="appendTexts"></param>
        /// <returns></returns>
        public static bool ExportCsv(DataGridView dgv, string fileName, string[] appendTexts = null)
        {
            List<string> mList = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible) sb.Append(dgv.Columns[i].HeaderText + "\t,");
            }
            mList.Add(sb.ToString());
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                sb.Clear();
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    if (!dgv.Columns[j].Visible) continue;
                    if (dgv.Rows[i].Cells[j].Value != null) sb.Append(dgv.Rows[i].Cells[j].Value.ToString() + "\t");
                    sb.Append(",");
                }
                mList.Add(sb.ToString());
            }
            if (appendTexts != null && appendTexts.Length > 0)
            {
                mList.AddRange(appendTexts);
            }
            return WriteCSV(fileName, mList.ToArray(), System.IO.FileMode.Create) == "ok";
        }
    }
}
