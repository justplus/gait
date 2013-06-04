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

namespace Gait
{
    /// <summary>
    /// GoToWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GoToWindow : Window
    {
        public int currentFrameIndex = 0;
        public int totalFrames = 0;
        public int GoToFrame = 0;
        public GoToWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Text = currentFrameIndex.ToString();
            textBox2.Text = totalFrames.ToString();
            textBox3.Text = "0";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Int32.TryParse(textBox3.Text, out GoToFrame);
            if (GoToFrame < 0 || GoToFrame > totalFrames)
            {
                MessageBox.Show("请检查输入值的范围！");
            }
            else
                DialogResult = true;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
