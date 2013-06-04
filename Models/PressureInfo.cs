using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElencySolutions.CsvHelper;

namespace Gait.Models
{
    class FrameInfo
    {
        public int frameIndex;              //帧索引
        public int rows = 40;               //行数
        public int cols = 40;               //列数
        public int[,] pressureValue;        //存储压力矩阵
        public int Area;                    //接触面积
        public int Pressure;                //总压力
        public int MaxPressure;             //最大压力
        //public int mPosX;                 //最大压力值对应的位置
        //public int mPosY;
        //public int top,bottom,left,right;   //边界

        public int backIndex;               //背景足底索引
        
        public bool twoFeet;                //两只脚 OR 一只脚
        public int t_Top,t_Bottom,t_Left,t_Right;
        public int b_Top,b_Bottom,b_Left,b_Right;
        //public int 

        public FrameInfo()
        {
            pressureValue = new int[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    pressureValue[i, j] = 0;
            //top=40;bottom=0;left=40;right=0;
            twoFeet = false;
            t_Bottom = rows;
            b_Top = 0;
        }

        //计算边界(两只脚)
    }

    class PressureInfo
    {
        string path;                            //CSV文件路径
        public List<FrameInfo> allFrames;       //存储该文件的所有帧数据
        public List<FrameInfo> backFrames;

        //构造函数
        public PressureInfo(string path)
        {
            allFrames = new List<FrameInfo>();
            backFrames = new List<FrameInfo>();
            this.path = path;
        }

        //获取文件名对应的文件夹
        public string GetFileName()
        {
            int index1 = path.LastIndexOf(@"\") + 1;
            int index2 = path.LastIndexOf(@".");
            string fileName = path.Substring(index1, index2 - index1);
            //string savePath = path.Substring(0, index1 - 1) + @"\" + fileName + @"\";
            return fileName;
        }

        //获取所有帧中最大接触面积 最大压力值
        public void GetMaxValue(out int mArea, out int mPressure, out int mMaxPressure)
        {
            mArea = 0; mPressure = 0; mMaxPressure = 0;
            foreach (FrameInfo frameInfo in allFrames)
            {
                if (frameInfo.Area > mArea)
                    mArea = frameInfo.Area;
                if (frameInfo.Pressure > mPressure)
                    mPressure = frameInfo.Pressure;
                if (frameInfo.MaxPressure > mMaxPressure)
                    mMaxPressure = frameInfo.MaxPressure;
            }
        }

        //读取压力值到内存中
        public void GetPressure()
        {
            try
            {
                using (CsvReader reader = new CsvReader(path, Encoding.Default))
                {
                    FrameInfo frameInfo = new FrameInfo();
                    int frameIndex = -1;
                    int tmpRow = 0, tmpCol = 0, tmpPressure = 0, totalPressure = 0, totalNonZero = 0, maxPressure = 0;
                    //int mPosX = 0, mPosY = 0;
                    bool isBackFrame = false;
                    while (reader.ReadNextRecord())
                    {
                        if (reader.Fields[0] == "-10")
                        {
                            if (isBackFrame)
                                backFrames.Add(frameInfo);
                            else
                                allFrames.Add(frameInfo);
                            break;
                        }
                        else if (reader.Fields[0] == "-1")
                        {
                            if (frameIndex >= 0)
                            {
                                if (isBackFrame)
                                    backFrames.Add(frameInfo);
                                else
                                    allFrames.Add(frameInfo);
                            }
                            isBackFrame = false;
                            frameInfo = new FrameInfo();
                            frameIndex = frameIndex + 1;
                            totalPressure = 0;
                            totalNonZero = 0;
                            maxPressure = 0;
                            frameInfo.frameIndex = Int32.Parse(reader.Fields[1]);
                            continue;
                        }
                        else if (reader.Fields[0] == "-2")
                        {
                            if (isBackFrame)
                                backFrames.Add(frameInfo);
                            else
                                allFrames.Add(frameInfo);
                            isBackFrame = true;
                            frameInfo = new FrameInfo();
                            tmpRow = 0;
                            totalPressure = 0;
                            totalNonZero = 0;
                            maxPressure = 0;
                            frameInfo.frameIndex = frameIndex;
                            continue;
                        }
                        else if (frameIndex >= 0 && isBackFrame == false)
                        {
                            List<string> tmpRowInfo = reader.Fields;
                            for (int i = 0; i < tmpRowInfo.Count; i++)
                            {
                                string tmp = tmpRowInfo[i];
                                if (tmp.Contains("."))
                                    tmp = tmp.Substring(0, tmp.IndexOf("."));
                                if (tmp != "-1")
                                {
                                    if (i == 0)
                                    {
                                        tmpRow = Int32.Parse(tmp);
                                    }
                                    else if (i % 2 == 1)
                                    {
                                        tmpCol = Int32.Parse(tmp);
                                        totalNonZero = totalNonZero + 1;
                                    }
                                    else
                                    {
                                        tmpPressure = Int32.Parse(tmp);
                                        totalPressure = totalPressure + tmpPressure;
                                        frameInfo.pressureValue[tmpRow, tmpCol] = tmpPressure;
                                        if (tmpPressure > maxPressure)
                                        {
                                            maxPressure = tmpPressure;
                                            //mPosX = tmpRow; mPosY = tmpCol;
                                        }
                                    }
                                }
                                else
                                    break;
                            }
                            frameInfo.Area = totalNonZero;
                            frameInfo.Pressure = totalPressure;
                            frameInfo.MaxPressure = maxPressure;
                        }
                        else if (frameIndex >= 0 && isBackFrame == true)
                        {
                            List<string> tmpRowInfo = reader.Fields;
                            for (int i = 0; i < tmpRowInfo.Count; i++)
                            {
                                string tmp = tmpRowInfo[i];
                                if (tmp.Contains("."))
                                    tmp = tmp.Substring(0, tmp.IndexOf("."));
                                //if (tmp != "-1")
                                {
                                    tmpPressure = Int32.Parse(tmp);
                                    frameInfo.pressureValue[tmpRow, i] = tmpPressure;
                                    totalPressure = totalPressure + tmpPressure;
                                    if (tmp != "0")
                                        totalNonZero = totalNonZero + 1;
                                }
                            }
                            tmpRow = tmpRow + 1;
                            //frameInfo.averagePress = totalPressure * 1.0 / totalNonZero;
                        }
                    }
                }

                //剔除孤立点
                for (int i = 0; i < backFrames.Count; i++)
                {
                    DelIsolated(ref backFrames[i].pressureValue);
                }
                for (int i = 0; i < allFrames.Count; i++)
                {
                    DelIsolated(ref allFrames[i].pressureValue);
                }

                for (int i = 0; i < allFrames.Count; i++)
                {
                    for (int j = 0; j < backFrames.Count - 1; j++)
                    {
                        if (i > backFrames[j].frameIndex && i <= backFrames[j + 1].frameIndex)
                            allFrames[i].backIndex = j + 1;
                        else if (i < backFrames[0].frameIndex)
                            allFrames[i].backIndex = 0;
                        else if (i > backFrames[backFrames.Count - 1].frameIndex)
                            allFrames[i].backIndex = backFrames.Count - 1;
                    }
                }

                //计算背景边缘
                for (int i = 0; i < backFrames.Count; i++)
                {
                    FrameInfo finfo = backFrames[i];
                    //finfo.CalBound();
                    CalBackBound(ref finfo);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        //剔除矩阵matrix中的孤立点
        //孤立点的判断标准是当前像素点的值小于15，且其邻域的8个像素的值均为0
        private void DelIsolated(ref int[,] matrix)
        {
            //去除边缘一圈的杂点 && 去除值小于5的噪音点
            for (int i = 0; i <= 39; i++)
                for (int j = 0; j <= 39; j++)
                {
                    if (i == 0 || i == 39 || j == 0 || j == 39)
                        matrix[i, j] = 0;
                    else
                        matrix[i, j] = matrix[i, j] <= 5 ? 0 : matrix[i, j];
                }
            //去除孤立点
            for (int i = 1; i <= 38; i++)
                for (int j = 1; j <= 38; j++)
                {
                    if (matrix[i, j] >= 20)
                        continue;
                    else if (matrix[i - 1, j - 1] == 0 && matrix[i - 1, j] == 0 && matrix[i - 1, j + 1] == 0 && matrix[i, j - 1] == 0 && matrix[i, j + 1] == 0 && matrix[i + 1, j - 1] == 0 && matrix[i + 1, j] == 0 && matrix[i + 1, j + 1] == 0)
                    {
                        matrix[i, j] = 0;
                    }
                }
        }

        public void CalBackBound(ref FrameInfo backFrame)
        {
            int[] rowsum=new int[40];
            //int[] t_colsum = new int[40];
            //int[] b_colsum = new int[40];
            bool t_first = true, b_first=true;
            for (int i = 0; i < backFrame.rows; i++)
            {
                for (int j = 0; j < backFrame.cols; j++)
                {
                    rowsum[i] = rowsum[i] + backFrame.pressureValue[i, j];
                }
            }
            for (int i = 0; i < rowsum.GetLength(0) - 1; i++)
            {
                if (rowsum[i] == 0 && rowsum[i + 1] != 0)
                {
                    if (t_first)
                        backFrame.t_Top = i + 1;
                    else
                    {
                        backFrame.twoFeet = true;
                        backFrame.b_Top = i + 1;
                    }
                    t_first = !t_first;
                }
                if (rowsum[i] != 0 && rowsum[i + 1] == 0)
                {
                    if (b_first)
                        backFrame.t_Bottom = i;
                    else
                        backFrame.b_Bottom = i;
                    b_first = !b_first;
                }
            }

            int t_left = 40, t_right = 0, b_left = 40, b_right = 0;
            for (int i = backFrame.t_Top; i <= backFrame.t_Bottom; i++)
                for (int j = 0; j < 40; j++)
                {
                    if (backFrame.pressureValue[i,j] != 0)
                    {
                        t_left = j < t_left ? j : t_left;
                        t_right = j > t_right ? j : t_right;
                    }
                }
            for (int i = backFrame.b_Top; i <= backFrame.b_Bottom; i++)
                for (int j = 0; j < 40; j++)
                {
                    if (backFrame.pressureValue[i, j] != 0)
                    {
                        b_left = j < b_left ? j : b_left;
                        b_right = j > b_right ? j : b_right;
                    }
                }
            backFrame.t_Left = t_left;
            backFrame.t_Right = t_right;
            backFrame.b_Left = b_left;
            backFrame.b_Right = b_right;
        }
    }
}
