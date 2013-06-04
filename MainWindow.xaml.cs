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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WF=System.Windows.Forms;
using System.Windows.Threading;
using System.IO;
using Gait.Models;

namespace Gait
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int currentFrame = 0;//记录当前帧索引
        private DispatcherTimer timer = new DispatcherTimer();
        PressureInfo pressureInfo;
        int axisWidth = 750;
        int type = 0;//曲线类型，默认显示压力曲线
        public int mArea, mPressure, mMaxPressure;
        int[,] subPressureTop,subPressureBottom;
        int backIndex = 0;//背景图索引
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick+=new EventHandler(timer_Tick);
            DrawAxis(750);
            DrawPixelGrid();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            WF.OpenFileDialog openFile = new WF.OpenFileDialog();
            openFile.Filter = @"CSV文件(*.csv)|*.csv";
            if (openFile.ShowDialog() == WF.DialogResult.OK)
            {
                pressureInfo = new PressureInfo(openFile.FileName);
                pressureInfo.GetPressure();
                pressureInfo.GetMaxValue(out mArea, out mPressure, out mMaxPressure);
                label1.Content = "帧数：" + pressureInfo.allFrames.Count.ToString() + "，步数：" + pressureInfo.backFrames.Count.ToString();
                canvas1.Children.Clear();
                canvas3.Children.Clear();
                canvas4.Children.Clear();
                UnFillPixelGrid();
                timer.Stop();
                currentFrame = 0;
                int frameCount = pressureInfo.allFrames.Count - 1;
                axisWidth = ((frameCount % 5 == 0) ? frameCount : ((frameCount / 5 + 1) * 5)) * 6;
                DrawAxis(axisWidth);
                //DrawPixelGrid();
                FillPixelGrid(0);
                DrawPressureCurve(type);
            }
        }

        //画坐标轴
        private void DrawAxis(int axisWidth)
        {
            //int axisWidth = pressureInfo.allFrames.Count * 6 + pressureInfo.backFrames.Count * 30 + 30;
            canvas1.Width = axisWidth + 50;
            for (int i = 0; i <= 5; i++)
            {
                int tmpY = 5 + 30 * i;
                Line axisX = new Line();
                axisX.Stroke = Brushes.Blue;
                axisX.StrokeThickness = 1;

                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(2);
                dashes.Add(2);
                axisX.StrokeDashArray = dashes;

                axisX.X1 = 30; axisX.Y1 = tmpY; axisX.X2 = axisX.X1 + axisWidth; axisX.Y2 = axisX.Y1;
                canvas1.Children.Add(axisX);

                TextBlock textBlock = new TextBlock();
                int yValue = 0;
                if (type == 0)
                    yValue = mPressure - (int)(mPressure*1.0/5 * i);
                else if (type == 1)
                    yValue = mArea - (int)(mArea * 1.0 / 5 * i);
                else if (type == 2)
                    yValue = mMaxPressure - (int)(mMaxPressure * 1.0 / 5 * i);
                textBlock.Text = yValue.ToString();
                Canvas.SetLeft(textBlock, 0);
                Canvas.SetTop(textBlock, 30 * i - 2.5);
                canvas1.Children.Add(textBlock);
            }

            for (int i = 0; i <= axisWidth / 30; i++)
            {
                int tmpX = 30 + 30 * i;
                Line axisX = new Line();
                axisX.Stroke = Brushes.Blue;
                axisX.StrokeThickness = 1;

                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(2);
                dashes.Add(2);
                axisX.StrokeDashArray = dashes;

                axisX.X1 = tmpX; axisX.Y1 = 5; axisX.X2 = axisX.X1; axisX.Y2 = 155;
                canvas1.Children.Add(axisX);

                TextBlock textBlock = new TextBlock();
                int xValue = 5 * i;
                textBlock.Text = xValue.ToString();
                Canvas.SetLeft(textBlock, tmpX - 7.5);
                Canvas.SetTop(textBlock, 160);
                canvas1.Children.Add(textBlock);
            }
        }

        //画像素网格
        private void DrawPixelGrid()
        {
            for (int i = 0; i <= 40; i++)
            {
                int axisWidth = 400;
                int tmpY = 10 * i;
                Line axisX = new Line();
                axisX.Stroke = Brushes.Gray;
                axisX.StrokeThickness = 0.5;

                axisX.X1 = 0; axisX.Y1 = tmpY; axisX.X2 = axisX.X1 + axisWidth; axisX.Y2 = axisX.Y1;
                canvas2.Children.Add(axisX);
            }
            for (int i = 0; i <= 40; i++)
            {
                int axisHeight = 400;
                int tmpY = 10 * i;
                Line axisX = new Line();
                axisX.Stroke = Brushes.Gray;
                axisX.StrokeThickness = 0.5;

                int tmpX = 10 * i;
                axisX.X1 = tmpX; axisX.Y1 = 0; axisX.X2 = axisX.X1; axisX.Y2 = axisHeight;
                canvas2.Children.Add(axisX);
            }
        }

        //画细节网格
        private void DrawSubGrid(int[,] subPressure,Canvas canvas)
        {
            //canvas3.Children.Clear();
            int rows = subPressure.GetLength(0);
            int cols = subPressure.GetLength(1);
            int mMaxPre = 0;//最大压力
            //int mPosX = 0, mPosY = 0;
            for (int i = 0; i <= rows; i++)
            {
                int tmpY = 20 * i;
                Line axisX = new Line();
                axisX.Stroke = Brushes.Gray;
                axisX.StrokeThickness = 1;
                axisX.X1 = 0; axisX.Y1 = tmpY; axisX.X2 = axisX.X1 + 20 * cols; axisX.Y2 = axisX.Y1;
                canvas.Children.Add(axisX);
            }
            for (int i = 0; i <= cols; i++)
            {
                int tmpX = 20 * i;
                Line axisX = new Line();
                axisX.Stroke = Brushes.Gray;
                axisX.StrokeThickness = 1;
                axisX.X1 = tmpX; axisX.Y1 = 0; axisX.X2 = axisX.X1; axisX.Y2 = 20 * rows;
                canvas.Children.Add(axisX);
            }
            for(int i = 0;i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int tmp = subPressure[i, j];
                    if (mMaxPre < tmp)
                    {
                        mMaxPre = tmp;
                        //mPosX = i;mPosY = j;
                    }
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.Width = 20; textBlock.Height = 20;
                    textBlock.Text = tmp.ToString();
                    Canvas.SetLeft(textBlock, j * 20);
                    Canvas.SetTop(textBlock, i * 20);
                    canvas.Children.Add(textBlock);
                }

            //标记最大压力值
            foreach (Object obj in canvas.Children)
            {
                if (obj.GetType().ToString() == "System.Windows.Controls.TextBlock")
                {
                    TextBlock textBlock = obj as TextBlock;
                    if (textBlock.Text == mMaxPre.ToString())
                        textBlock.Background = Brushes.Red;
                }
            }
        }

        //填充像素网格
        private void FillPixelGrid(int index)
        {
            if (pressureInfo.allFrames.Count <= 0)
                return;
            if (pressureInfo.backFrames.Count > 0)
            {
                backIndex = pressureInfo.allFrames[currentFrame].backIndex;         //背景索引
                FrameInfo backFrame = pressureInfo.backFrames[backIndex];
                int[,] backPressureValue = backFrame.pressureValue;
                int t_Top = backFrame.t_Top;
                int t_Bottom = backFrame.t_Bottom;
                int t_Left = backFrame.t_Left;
                int t_Right = backFrame.t_Right;
                int b_Top = backFrame.b_Top;
                int b_Bottom = backFrame.b_Bottom;
                int b_Left = backFrame.b_Left;
                int b_Right = backFrame.b_Right;
                if (backFrame.twoFeet)
                {
                    //绘制左边区域的足底背景区域[上面的那个足部信息]
                    //for (int i = t_Top; i <= t_Bottom; i++)
                    //    for (int j = t_Left; j <= t_Right; j++)
                    //    {
                    //        Rectangle backpixel = new Rectangle();
                    //        backpixel.Width = 9;
                    //        backpixel.Height = 9;
                    //        int backtmp = backPressureValue[i, j];
                    //        if (backtmp != 0)
                    //        {
                    //            backpixel.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    //            Canvas.SetLeft(backpixel, 10 * j + 0.5);
                    //            Canvas.SetTop(backpixel, 10 * i + 0.5);
                    //            canvas2.Children.Add(backpixel);
                    //        }
                    //    }
                    //绘制上面那个足部信息的轮廓
                    Rectangle topOuter = new Rectangle();
                    topOuter.Width = 10 * (t_Right - t_Left + 1);
                    topOuter.Height = 10 * (t_Bottom - t_Top + 1);
                    Canvas.SetLeft(topOuter, 10 * t_Left + 0.5);
                    Canvas.SetTop(topOuter, 10 * t_Top + 0.5);
                    topOuter.Stroke = Brushes.Red;
                    canvas2.Children.Add(topOuter);
                    //绘制上面那个足底信息的矩阵信息
                    canvas3.Children.Clear();
                    canvas3.Width = (t_Right - t_Left + 1) * 20;
                    canvas3.Height = (t_Bottom - t_Top + 1) * 20;
                    subPressureTop = new int[t_Bottom - t_Top + 1, t_Right - t_Left + 1];
                    for (int i = 0; i < t_Bottom - t_Top + 1; i++)
                        for (int j = 0; j < t_Right - t_Left + 1; j++)
                        {
                            subPressureTop[i, j] = pressureInfo.allFrames[currentFrame].pressureValue[t_Top + i, t_Left + j];
                        }
                    DrawSubGrid(subPressureTop, canvas3);

                    //绘制左边区域的足底背景区域[下面的那个足部信息]
                    //for (int i = b_Top; i <= b_Bottom; i++)
                    //    for (int j = b_Left; j <= b_Right; j++)
                    //    {
                    //        Rectangle backpixel = new Rectangle();
                    //        backpixel.Width = 9;
                    //        backpixel.Height = 9;
                    //        int backtmp = backPressureValue[i, j];
                    //        if (backtmp != 0)
                    //        {
                    //            backpixel.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    //            Canvas.SetLeft(backpixel, 10 * j + 0.5);
                    //            Canvas.SetTop(backpixel, 10 * i + 0.5);
                    //            canvas2.Children.Add(backpixel);
                    //        }
                    //    }
                    //绘制下面那个足部信息的轮廓
                    Rectangle bottomOuter = new Rectangle();
                    bottomOuter.Width = 10 * (b_Right - b_Left + 1);
                    bottomOuter.Height = 10 * (b_Bottom - b_Top + 1);
                    Canvas.SetLeft(bottomOuter, 10 * b_Left + 0.5);
                    Canvas.SetTop(bottomOuter, 10 * b_Top + 0.5);
                    bottomOuter.Stroke = Brushes.Red;
                    canvas2.Children.Add(bottomOuter);
                    //绘制上面那个足底信息的矩阵信息
                    canvas4.Children.Clear();
                    canvas4.Width = (b_Right - b_Left + 1) * 20;
                    canvas4.Height = (b_Bottom - b_Top + 1) * 20;
                    subPressureBottom = new int[b_Bottom - b_Top + 1, b_Right - b_Left + 1];
                    for (int i = 0; i < b_Bottom - b_Top + 1; i++)
                        for (int j = 0; j < b_Right - b_Left + 1; j++)
                        {
                            subPressureBottom[i, j] = pressureInfo.allFrames[currentFrame].pressureValue[b_Top + i, b_Left + j];
                        }
                    DrawSubGrid(subPressureBottom, canvas4);
                }
                else
                {
                    //绘制左边区域的足底背景区域[上面的那个足部信息]
                    //for (int i = t_Top; i <= t_Bottom; i++)
                    //    for (int j = t_Left; j <= t_Right; j++)
                    //    {
                    //        Rectangle backpixel = new Rectangle();
                    //        backpixel.Width = 9;
                    //        backpixel.Height = 9;
                    //        int backtmp = backPressureValue[i, j];
                    //        if (backtmp != 0)
                    //        {
                    //            backpixel.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    //            Canvas.SetLeft(backpixel, 10 * j + 0.5);
                    //            Canvas.SetTop(backpixel, 10 * i + 0.5);
                    //            canvas2.Children.Add(backpixel);
                    //        }
                    //    }
                    //绘制上面那个足部信息的轮廓
                    Rectangle topOuter = new Rectangle();
                    topOuter.Width = 10 * (t_Right - t_Left + 1);
                    topOuter.Height = 10 * (t_Bottom - t_Top + 1);
                    Canvas.SetLeft(topOuter, 10 * t_Left + 0.5);
                    Canvas.SetTop(topOuter, 10 * t_Top + 0.5);
                    topOuter.Stroke = Brushes.Red;
                    canvas2.Children.Add(topOuter);
                    //绘制上面那个足底信息的矩阵信息
                    canvas3.Children.Clear();
                    canvas3.Width = (t_Right - t_Left + 1) * 20;
                    canvas3.Height = (t_Bottom - t_Top + 1) * 20;
                    subPressureTop = new int[t_Bottom - t_Top + 1, t_Right - t_Left + 1];
                    for (int i = 0; i < t_Bottom - t_Top + 1; i++)
                        for (int j = 0; j < t_Right - t_Left + 1; j++)
                        {
                            subPressureTop[i, j] = pressureInfo.allFrames[currentFrame].pressureValue[t_Top + i, t_Left + j];
                        }
                    DrawSubGrid(subPressureTop, canvas3);
                }
            }

            int[,] pressureValue = pressureInfo.allFrames[index].pressureValue;
            for (int i = 0; i < pressureValue.GetLength(0); i++)
                for (int j = 0; j < pressureValue.GetLength(1); j++)
                {
                    Rectangle pixel = new Rectangle();
                    pixel.Width = 9;
                    pixel.Height = 9;
                    //实际足底区域
                    int tmp = pressureValue[i, j];
                    //tmp = (int)(tmp*1.0 / frameInfo.averagePress * 128);
                    if (tmp != 0)
                    {
                        int R, G, B;
                        Idx2RGB(tmp, out R, out G, out B);
                        pixel.Fill = new SolidColorBrush(Color.FromRgb((byte)R, (byte)G, (byte)B));
                        Canvas.SetLeft(pixel, 10 * j + 0.5);
                        Canvas.SetTop(pixel, 10 * i + 0.5);
                        canvas2.Children.Add(pixel);
                        pixel.ToolTip = "行:" + i.ToString() + "  列:" + j.ToString() + "  压力值:" + tmp.ToString();
                    }
                }

            if (type == 0)
                label3.Content = "总压力值：" + pressureInfo.allFrames[currentFrame].Pressure.ToString();
            else if (type == 1)
                label3.Content = "接触面积：" + pressureInfo.allFrames[currentFrame].Area.ToString();
            else if(type ==2)
                label3.Content = "最大压力值：" + pressureInfo.allFrames[currentFrame].MaxPressure.ToString();
        }

        //去除canvas2种除了网格的全部内容
        private void UnFillPixelGrid()
        {
            List<UIElement> rIndex = new List<UIElement>();
            for (int i = 0; i < canvas2.Children.Count; i++)
            {
                UIElement ui = canvas2.Children[i];
                if (ui.GetType().ToString() == "System.Windows.Shapes.Rectangle")
                    rIndex.Add(ui);
            }
            for (int i = 0; i < rIndex.Count; i++)
            {
                canvas2.Children.Remove(rIndex[i]);
            }
        }

        //更新进度条
        private void UpdateProcessBar()
        {
            UIElement processBar = new UIElement();
            foreach (UIElement ui in canvas1.Children)
            {
                if (ui.GetType().ToString() == "System.Windows.Shapes.Line")
                {
                    Line line = ui as Line;
                    if (line.Stroke == Brushes.Green)
                    {
                        processBar = line;
                        break;
                    }
                }
            }
            if (processBar != null)
                canvas1.Children.Remove(processBar);
            int tmpX = 30 + 6 * currentFrame;
            Line axisX = new Line();
            axisX.Stroke = Brushes.Green;
            axisX.StrokeThickness = 1;
            axisX.X1 = tmpX; axisX.Y1 = 5; axisX.X2 = axisX.X1; axisX.Y2 = 155;
            canvas1.Children.Add(axisX);
        }

        /// <summary>
        /// 绘制压力曲线
        /// </summary>
        /// <param name="type">曲线类型：0-总压力值，1-接触面积，2-最大压力值</param>
        private void DrawPressureCurve(int type)
        {
            if (pressureInfo == null)
                return;
            List<double> leftMargin = new List<double>();
            List<double> topMargin = new List<double>();
            //List<int> pressureValue = new List<int>();
            double tmpX = 30, tmpY = 155;
            //bool isBackFrame = false;
            List<int> frameindexs = new List<int>();
            foreach (FrameInfo backFrameInfo in pressureInfo.backFrames)
            {
                frameindexs.Add(backFrameInfo.frameIndex);
            }
            foreach (FrameInfo frameInfo in pressureInfo.allFrames)
            {
                if(type == 0)
                    tmpY = tmpY - frameInfo.Pressure * 150.0 / mPressure;
                else if(type == 1)
                    tmpY = tmpY - frameInfo.Area * 150.0 / mArea;
                else if(type ==2)
                    tmpY = tmpY - frameInfo.MaxPressure * 150.0 / mMaxPressure;
                //tmpY = tmpY - frameInfo.MaxPressure * 150.0 / 200;
                leftMargin.Add(tmpX);
                topMargin.Add(tmpY);
                tmpX = tmpX + 6;
                tmpY = 155;
            }
            for (int j = 0; j < leftMargin.Count - 1; j++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Red;
                line.StrokeThickness = 1;

                line.X1 = leftMargin[j];
                line.Y1 = topMargin[j];
                line.X2 = leftMargin[j + 1];
                line.Y2 = topMargin[j + 1];
                canvas1.Children.Add(line);
            }
        }

        private void Idx2RGB(int index, out int R, out int G, out int B)
        {
            if (index < 0)
                index = 0;
            else if (index > 127)
                index = 127;
            R = 0; G = 0; B = 127;
            if (index < 16)
            {
                R = 0;
                G = 0;
                B = B + (index + 1) * 8;
            }
            else if (index < 48)
            {
                R = 0; G = 0; B = 255;
                G = G + 8 * (index - 15) - 1;
            }
            else if (index < 80)
            {
                R = 0; G = 255; B = 255;
                R = R + 8 * (index - 47) - 1;
                B = B - 8 * (index - 47) + 1;
            }
            else if (index < 112)
            {
                R = 255; G = 255; B = 0;
                G = G - 8 * (index - 79) + 1;
            }
            else if (index <= 127)
            {
                R = 255; G = 0; B = 0;
                R = R - 8 * (index - 111) + 1;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //if (currentFrame < 0 || currentFrame >= pressureInfo.allFrames.Count - 1)
            //    return;
            if (currentFrame >= pressureInfo.allFrames.Count - 1)
            {
                timer.Stop();
                playButton.Tag = "Play";
                playButton.Source = new BitmapImage(new Uri("/Gait;component/Images/Play.png", UriKind.Relative));
                playButton.ToolTip = "播放";
                currentFrame = 0;
            }
            else
                currentFrame = currentFrame + 1;
            UnFillPixelGrid();
            UpdateProcessBar();
            FillPixelGrid(currentFrame);
            label2.Content = "当前帧：" + currentFrame.ToString();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            if (playButton.Tag.ToString() == "Play")
            {
                timer.Start();
                playButton.Tag = "Pause";
                playButton.Source = new BitmapImage(new Uri("/Gait;component/Images/Pause.png", UriKind.Relative));
                playButton.ToolTip = "暂停";
            }
            else if (playButton.Tag.ToString() == "Pause")
            {
                timer.Stop();
                playButton.Tag = "Play";
                playButton.Source = new BitmapImage(new Uri("/Gait;component/Images/Play.png", UriKind.Relative));
                playButton.ToolTip = "播放";
            }
        }

        private void PlayMenu_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            timer.Start();
            playButton.Tag = "Pause";
            playButton.Source = new BitmapImage(new Uri("/Gait;component/Images/Pause.png", UriKind.Relative));
            playButton.ToolTip = "暂停";
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            timer.Stop();
            playButton.Tag = "Play";
            playButton.Source = new BitmapImage(new Uri("/Gait;component/Images/Play.png", UriKind.Relative));
            playButton.ToolTip = "播放";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            timer.Stop();
            currentFrame = 0;
            playButton.Tag = "Play";
            playButton.Source = new BitmapImage(new Uri("/Gait;component/Images/Play.png", UriKind.Relative));
            playButton.ToolTip = "播放";
            UnFillPixelGrid();
            FillPixelGrid(currentFrame);
            UpdateProcessBar();
            label2.Content = "当前帧：" + currentFrame.ToString();
        }

        private void Pre_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            timer.Stop();
            if (currentFrame == 0)
                currentFrame = 0;
            else
                currentFrame = currentFrame - 1;
            UnFillPixelGrid();
            FillPixelGrid(currentFrame);
            UpdateProcessBar();
            label2.Content = "当前帧：" + currentFrame.ToString();
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            timer.Stop();
            if (currentFrame >= pressureInfo.allFrames.Count - 1)
                currentFrame = pressureInfo.allFrames.Count - 1;
            else
                currentFrame = currentFrame + 1;
            UnFillPixelGrid();
            FillPixelGrid(currentFrame);
            UpdateProcessBar();
            label2.Content = "当前帧：" + currentFrame.ToString();
        }

        private void Goto_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            GoToWindow goToWindow = new GoToWindow();
            goToWindow.currentFrameIndex = currentFrame;
            goToWindow.totalFrames = pressureInfo.allFrames.Count;
            if (goToWindow.ShowDialog() == true)
            {
                currentFrame = goToWindow.GoToFrame;
                UnFillPixelGrid();
                FillPixelGrid(currentFrame);
                UpdateProcessBar();
                label2.Content = "当前帧：" + currentFrame.ToString();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            //this.Close();
        }

        //保存有效值
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            WF.SaveFileDialog saveFileDialog = new WF.SaveFileDialog();
            saveFileDialog.Filter = "文本文件(*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == WF.DialogResult.OK)
            {
                string saveName = saveFileDialog.FileName;
                //backIndex = pressureInfo.allFrames[currentFrame].backIndex;         //背景索引
                FrameInfo backFrame = pressureInfo.backFrames[backIndex];
                if (backFrame.twoFeet)
                {
                    int dotIndex = saveName.LastIndexOf(".txt");
                    string topSaveName = saveName.Substring(0, dotIndex) + "_t.txt";
                    string bottomSaveName = saveName.Substring(0, dotIndex) + "_b.txt";
                    using (StreamWriter writer = File.CreateText(topSaveName))
                    {
                        for (int i = 0; i < subPressureTop.GetLength(0); i++)
                        {
                            string tmp = string.Empty;
                            for (int j = 0; j < subPressureTop.GetLength(1); j++)
                                tmp = tmp + subPressureTop[i, j].ToString() + " ";
                            writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                        }
                    }
                    using (StreamWriter writer = File.CreateText(bottomSaveName))
                    {
                        for (int i = 0; i < subPressureBottom.GetLength(0); i++)
                        {
                            string tmp = string.Empty;
                            for (int j = 0; j < subPressureBottom.GetLength(1); j++)
                                tmp = tmp + subPressureBottom[i, j].ToString() + " ";
                            writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                        }
                    }
                }
                else
                {
                    using (StreamWriter writer = File.CreateText(saveFileDialog.FileName))
                    {
                        for (int i = 0; i < subPressureTop.GetLength(0); i++)
                        {
                            string tmp = string.Empty;
                            for (int j = 0; j < subPressureTop.GetLength(1); j++)
                                tmp = tmp + subPressureTop[i, j].ToString() + " ";
                            writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                        }
                    }
                }
                MessageBox.Show(string.Format("第{0}帧已保存",currentFrame));
            }
        }

        //保存当前帧
        private void SaveOne_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            WF.SaveFileDialog saveFileDialog = new WF.SaveFileDialog();
            saveFileDialog.Filter = "文本文件(*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == WF.DialogResult.OK)
            {
                using (StreamWriter writer = File.CreateText(saveFileDialog.FileName))
                {
                    for (int i = 0; i < 40; i++)
                    {
                        string tmp = string.Empty;
                        for (int j = 0; j < 40; j++)
                            tmp = tmp + pressureInfo.allFrames[currentFrame].pressureValue[i, j].ToString() + " ";
                        writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                    }
                    string temp = "第" + currentFrame.ToString() + "帧已保存！";
                    MessageBox.Show(temp);
                }
            }
        }

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            SaveFrames saveFrames = new SaveFrames();
            saveFrames.toFrame = pressureInfo.allFrames.Count - 1;
            if (saveFrames.ShowDialog() == true)
            {
                string savePath = saveFrames.savePath + "\\ " + pressureInfo.GetFileName();
                string topSavePath = savePath + "_top";
                string bottomSavePath = savePath + "_bottom";
                
                int backindex;
                int[,] subPressureTop, subPressureBottom;
                for (int index = saveFrames.fromFrame; index <= saveFrames.toFrame; index = index + saveFrames.interval)
                {
                    backindex = pressureInfo.allFrames[index].backIndex;         //背景索引
                    FrameInfo backFrame = pressureInfo.backFrames[backindex];
                    //默认帧数小于1000帧
                    //int tmpIndex = frameInfo.frameIndex - saveFrames.fromFrame;           //保存的文件名从0开始
                    int tmpIndex = index;                                    //保存的文件名以真实的帧数
                    int baiwei = tmpIndex / 100;
                    int shiwei = (tmpIndex - baiwei * 100) / 10;
                    int gewei = tmpIndex % 10;

                    int t_Top = backFrame.t_Top;
                    int t_Bottom = backFrame.t_Bottom;
                    int t_Left = backFrame.t_Left;
                    int t_Right = backFrame.t_Right;
                    int b_Top = backFrame.b_Top;
                    int b_Bottom = backFrame.b_Bottom;
                    int b_Left = backFrame.b_Left;
                    int b_Right = backFrame.b_Right;

                    if (backFrame.twoFeet)
                    {
                        if (!Directory.Exists(topSavePath))
                            Directory.CreateDirectory(topSavePath);
                        if (!Directory.Exists(bottomSavePath))
                            Directory.CreateDirectory(bottomSavePath);
                        subPressureTop = new int[t_Bottom - t_Top + 1, t_Right - t_Left + 1];
                        for (int i = 0; i < t_Bottom - t_Top + 1; i++)
                            for (int j = 0; j < t_Right - t_Left + 1; j++)
                            {
                                subPressureTop[i, j] = pressureInfo.allFrames[index].pressureValue[t_Top + i, t_Left + j];
                            }
                        subPressureBottom = new int[b_Bottom - b_Top + 1, b_Right - b_Left + 1];
                        for (int i = 0; i < b_Bottom - b_Top + 1; i++)
                            for (int j = 0; j < b_Right - b_Left + 1; j++)
                            {
                                subPressureBottom[i, j] = pressureInfo.allFrames[index].pressureValue[b_Top + i, b_Left + j];
                            }

                        string topSP = topSavePath + "\\" + string.Format("{0}{1}{2}", baiwei, shiwei, gewei) + ".txt";
                        string bottomSP = bottomSavePath + "\\" + string.Format("{0}{1}{2}", baiwei, shiwei, gewei) + ".txt";
                        using (StreamWriter writer = File.CreateText(topSP))
                        {
                            for (int i = 0; i < subPressureTop.GetLength(0); i++)
                            {
                                string tmp = string.Empty;
                                for (int j = 0; j < subPressureTop.GetLength(1); j++)
                                    tmp = tmp + subPressureTop[i, j].ToString() + " ";
                                writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                            }
                        }
                        using (StreamWriter writer = File.CreateText(bottomSP))
                        {
                            for (int i = 0; i < subPressureBottom.GetLength(0); i++)
                            {
                                string tmp = string.Empty;
                                for (int j = 0; j < subPressureBottom.GetLength(1); j++)
                                    tmp = tmp + subPressureBottom[i, j].ToString() + " ";
                                writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                            }
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(savePath))
                            Directory.CreateDirectory(savePath);
                        subPressureTop = new int[t_Bottom - t_Top + 1, t_Right - t_Left + 1];
                        for (int i = 0; i < t_Bottom - t_Top + 1; i++)
                            for (int j = 0; j < t_Right - t_Left + 1; j++)
                            {
                                subPressureTop[i, j] = pressureInfo.allFrames[index].pressureValue[t_Top + i, t_Left + j];
                            }
                        string SavePath = savePath + "\\" + string.Format("{0}{1}{2}", baiwei, shiwei, gewei) + ".txt";
                        using (StreamWriter writer = File.CreateText(SavePath))
                        {
                            for (int i = 0; i < subPressureTop.GetLength(0); i++)
                            {
                                string tmp = string.Empty;
                                for (int j = 0; j < subPressureTop.GetLength(1); j++)
                                    tmp = tmp + subPressureTop[i, j].ToString() + " ";
                                writer.WriteLine(tmp.Substring(0, tmp.Length - 1));
                            }
                        }
                    } 
                }
                MessageBox.Show("所有帧数据保存完毕！");
            }
        }

        private void Pressure_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            type = 0;
            canvas1.Children.Clear();
            DrawAxis(axisWidth);
            DrawPressureCurve(type);
            UpdateProcessBar();
            label3.Content = "总压力值：" + pressureInfo.allFrames[currentFrame].Pressure.ToString();
        }

        private void Area_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            type = 1;
            canvas1.Children.Clear();
            DrawAxis(axisWidth);
            DrawPressureCurve(type);
            UpdateProcessBar();
            label3.Content = "接触面积：" + pressureInfo.allFrames[currentFrame].Area.ToString();
        }

        private void MaxPressure_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            type = 2;
            canvas1.Children.Clear();
            DrawAxis(axisWidth);
            DrawPressureCurve(type);
            UpdateProcessBar();
            label3.Content = "最大压力值：" + pressureInfo.allFrames[currentFrame].MaxPressure.ToString();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }

        //选取区域事件
        int regionLeft = 0, regionRight = 0, regionTop = 0, regionBottom = 0;
        Rectangle rect = new Rectangle();
        bool isDrawing = false;
        bool isSelectedClicked = false;
        private void SelectRegion_Click(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null)
                return;
            if (!isSelectedClicked)
            {
                isSelectedClicked = true;
                this.Cursor = Cursors.Cross;
                canvas2.MouseDown += new MouseButtonEventHandler(canvas2_MouseDown);
                canvas2.MouseUp += new MouseButtonEventHandler(canvas2_MouseUp);
                canvas2.MouseMove += new MouseEventHandler(canvas2_MouseMove);
            }
            else if (isSelectedClicked)
            {
                isSelectedClicked = false;
                this.Cursor = Cursors.Arrow;
                canvas2.MouseDown -= new MouseButtonEventHandler(canvas2_MouseDown);
                canvas2.MouseUp -= new MouseButtonEventHandler(canvas2_MouseUp);
                canvas2.MouseMove -= new MouseEventHandler(canvas2_MouseMove);

                UIElement uiElement = new UIElement();
                foreach (UIElement ui in canvas2.Children)
                {
                    if (ui.GetType().ToString() == "System.Windows.Shapes.Rectangle")
                    {
                        Rectangle rectangle = ui as Rectangle;
                        if (rectangle.Tag != null && rectangle.Tag.ToString() == "only")
                        {
                            uiElement = rectangle;
                            break;
                        }
                    }
                }
                if (uiElement != null)
                    canvas2.Children.Remove(uiElement);

                UnFillPixelGrid();
                FillPixelGrid(currentFrame);
            }
        } 

        private void canvas2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            regionLeft = ((int)e.GetPosition(canvas2).X + 5)/10;
            regionTop = ((int)e.GetPosition(canvas2).Y + 5) / 10;
        }

        private void canvas2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            regionRight = ((int)e.GetPosition(canvas2).X + 0) / 10;
            regionBottom = ((int)e.GetPosition(canvas2).Y + 0) / 10;

            if (regionRight < regionLeft)
                regionRight = regionLeft;
            if (regionBottom < regionTop)
                regionBottom = regionTop;
            if (regionRight > 39)
                regionRight = 39;
            if (regionBottom > 39)
                regionBottom = 39;

            canvas3.Children.Clear();
            canvas4.Children.Clear();
            canvas3.Width = (regionRight - regionLeft + 1) * 20;
            canvas3.Height = (regionBottom - regionTop + 1) * 20;
            subPressureTop = new int[regionBottom - regionTop + 1, regionRight - regionLeft + 1];
            for (int i = 0; i < regionBottom - regionTop + 1; i++)
                for (int j = 0; j < regionRight - regionLeft + 1; j++)
                {
                    subPressureTop[i, j] = pressureInfo.allFrames[currentFrame].pressureValue[regionTop + i, regionLeft + j];
                }
            DrawSubGrid(subPressureTop,canvas3);
        }

        private void canvas2_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing)
                return;
            UIElement uiElement = new UIElement();
            foreach (UIElement ui in canvas2.Children)
            {
                if (ui.GetType().ToString() == "System.Windows.Shapes.Rectangle")
                {
                    Rectangle rectangle = ui as Rectangle;
                    if (rectangle.Tag!=null && rectangle.Tag.ToString() == "only")
                    {
                        uiElement = rectangle;
                        break;
                    }
                }
            }
            if (uiElement != null)
                canvas2.Children.Remove(uiElement);
            regionRight = ((int)e.GetPosition(canvas2).X + 0) / 10;
            regionBottom = ((int)e.GetPosition(canvas2).Y + 0) / 10;
            //防止用户往起点左上方画！！
            if (regionRight < regionLeft)
                regionRight = regionLeft;
            if (regionBottom < regionTop)
                regionBottom = regionTop;
            if (regionRight > 39)
                regionRight = 39;
            if (regionBottom > 39)
                regionBottom = 39;

            rect.Stroke = Brushes.Blue;
            rect.Tag = "only";
            rect.Width = (regionRight - regionLeft +1) * 10;
            rect.Height = (regionBottom - regionTop +1) * 10;
            Canvas.SetLeft(rect, regionLeft * 10);
            Canvas.SetTop(rect, regionTop * 10);
            canvas2.Children.Add(rect);
        }

        //鼠标滚轮控制帧的变化
        private void window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                Right_Click(sender, e);
            else if (e.Delta > 0)
                Pre_Click(sender,e);
        }

        private void Help_Clicked(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.Show();
        }

        //上下翻转
        private void UpDownFlip(ref int[,] matrix)
        { 
            int rows=matrix.GetLength(0);
            int cols=matrix.GetLength(1);
            int[,] tmp=new int[rows,cols];
            for(int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    tmp[i, j] = matrix[rows - i - 1, j];
                }
            matrix = tmp;
        }

        //左右翻转
        private void LeftRightFlip(ref int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[,] tmp = new int[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    tmp[i, j] = matrix[i , cols - j - 1];
                }
            matrix = tmp;
        }

        private void UpDown_Clicked(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null || isSelectedClicked)
                return;
            //改变一组就要改变一个连续的区间！
            int startIndex, finishIndex;
            if (backIndex == 0)
                startIndex = 0;
            else
                startIndex = pressureInfo.backFrames[backIndex - 1].frameIndex + 1;
            finishIndex = pressureInfo.backFrames[backIndex].frameIndex;
            for (int i = startIndex; i <= finishIndex; i++)
            {
                UpDownFlip(ref pressureInfo.allFrames[i].pressureValue);
            }
            UpDownFlip(ref pressureInfo.backFrames[backIndex].pressureValue);
            //UpDownFlip(ref pressureInfo.allFrames[currentFrame].pressureValue);
            UnFillPixelGrid();
            for (int i = 0; i < pressureInfo.backFrames.Count; i++)
            {
                FrameInfo fi = pressureInfo.backFrames[i];
                pressureInfo.CalBackBound(ref fi);
            }
            FillPixelGrid(currentFrame);
        }

        private void LeftRight_Clicked(object sender, RoutedEventArgs e)
        {
            if (pressureInfo == null || isSelectedClicked)
                return;

            /*int startIndex, finishIndex;
            if (backIndex == 0)
                startIndex = 0;
            else
                startIndex = pressureInfo.backFrames[backIndex - 1].frameIndex + 1;
            finishIndex = pressureInfo.backFrames[backIndex].frameIndex;
            for (int i = startIndex; i <= finishIndex; i++)
            {
                LeftRightFlip(ref pressureInfo.allFrames[i].pressureValue);
            }

            LeftRightFlip(ref pressureInfo.backFrames[backIndex].pressureValue);*/
            //LeftRightFlip(ref pressureInfo.allFrames[currentFrame].pressureValue);
            for (int i = 0; i < pressureInfo.allFrames.Count;i++ )
                LeftRightFlip(ref pressureInfo.allFrames[i].pressureValue);
            for (int i = 0; i < pressureInfo.backFrames.Count; i++)
            {
                LeftRightFlip(ref pressureInfo.backFrames[i].pressureValue);
                //pressureInfo.backFrames[i].CalBound();
            }
            for (int i = 0; i < pressureInfo.backFrames.Count; i++)
            {
                FrameInfo fi = pressureInfo.backFrames[i];
                pressureInfo.CalBackBound(ref fi);
            }
            UnFillPixelGrid();
            FillPixelGrid(currentFrame);
        }

        private void Setting_Clicked(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
            setting.ShowDialog();
        }

        private void StateChange(object sender, MouseButtonEventArgs e)
        {
            type++;
            switch (type % 3)
            {
                case 0:
                    Pressure_Click(null, null);
                    break;
                case 1:
                    Area_Click(null,null);
                    break;
                case 2:
                    MaxPressure_Click(null,null);
                    break;
            }
        }

        /*private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Up)
                Pre_Click(sender, e);
            else if(e.Key==Key.Down)
                Right_Click(sender, e);
        }*/
    }
}
