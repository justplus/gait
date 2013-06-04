using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WF = System.Windows.Forms;
using System.IO;

namespace Gait
{
    /// <summary>
    /// SaveFrames.xaml 的交互逻辑
    /// </summary>
    public partial class SaveFrames : Window
    {
        public int fromFrame = 0;
        public int toFrame = 0;
        public int interval = 1;
        public string savePath = string.Empty;
        int totalFrame;
        public SaveFrames()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Text = fromFrame.ToString();
            textBox2.Text = toFrame.ToString();
            totalFrame = Int32.Parse(textBox2.Text);
            textBox3.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void BrowseClicked(object sender, RoutedEventArgs e)
        {
            WF.FolderBrowserDialog folderBrowserDialog = new WF.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == WF.DialogResult.OK)
            {
                savePath = folderBrowserDialog.SelectedPath;
                textBox3.Text = savePath;
            }
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            //int totalFrame = Int32.Parse(textBox2.Text);
            Int32.TryParse(textBox1.Text, out fromFrame);
            Int32.TryParse(textBox2.Text, out toFrame);
            Int32.TryParse(textBox4.Text, out interval);
            if (fromFrame < 0 || toFrame > totalFrame || fromFrame > totalFrame)
            {
                MessageBox.Show("请检查输入值的范围！");
            }
            else if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("当前路径不能为空！");
            }
            else if (interval > toFrame || interval < 1)
            {
                MessageBox.Show(string.Format("间隔值介于1和{0}之间",toFrame));
            }
            else
            {
                savePath = textBox3.Text.Trim();
                DialogResult = true;
            }
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
