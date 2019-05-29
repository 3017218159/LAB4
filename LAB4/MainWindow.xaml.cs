using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using ZedGraph;
using System.Windows.Forms;

namespace LAB4
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Data data = new Data();
        SerialPort port = new SerialPort();
        private double red_value, green_value, yellow_value, blue_value, white_value;
        List<byte> temp = new List<byte>{ 0, 0, 0 };
        List<byte> light = new List<byte> { 0, 0, 0 };
        PointPairList temp_list = new PointPairList();
        PointPairList light_list = new PointPairList();
        private StreamWriter sw = null;
        private bool isRecord = false;
        public MainWindow()
        {
            InitializeComponent();

            Init();//初始化graph

            System.Windows.Data.Binding temp_binding = new System.Windows.Data.Binding();
            System.Windows.Data.Binding light_binding = new System.Windows.Data.Binding();
            temp_binding.Source = data;
            light_binding.Source = data;
            temp_binding.Path = new PropertyPath("Temperature");
            light_binding.Path = new PropertyPath("Intensity");
            textBox1.SetBinding(System.Windows.Controls.TextBox.TextProperty, temp_binding);
            textBox2.SetBinding(System.Windows.Controls.TextBox.TextProperty, light_binding);

            red_value = red.Value;
            green_value = green.Value;
            yellow_value = yellow.Value;
            blue_value = blue.Value;
            white_value = white.Value;
            color.Fill= new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));

            //填入波特率
            cbb2.Items.Clear();
            cbb2.Items.Add("9600");
            cbb2.Items.Add("19200");
            cbb2.Items.Add("38400");
            cbb2.Items.Add("57600");
            cbb2.Items.Add("115200");
            cbb2.Items.Add("921600");
            cbb2.SelectedItem = cbb2.Items[0];
        }

        private void Init()
        {
            wfh1.Visibility = Visibility.Visible;
            wfh2.Visibility = Visibility.Hidden;
            graph1.GraphPane.Title = "温度实时动态图";
            graph1.GraphPane.XAxis.Title = "时间";
            graph1.GraphPane.YAxis.Title = "温度";
            graph1.GraphPane.FontSpec.Size = 30;
            graph1.GraphPane.XAxis.TitleFontSpec.Size = 25;
            graph1.GraphPane.XAxis.ScaleFontSpec.Size = 20;
            graph1.GraphPane.YAxis.TitleFontSpec.Size = 25;
            graph1.GraphPane.YAxis.ScaleFontSpec.Size = 20;
            graph1.Refresh();
            graph2.GraphPane.Title = "光强实时动态图";
            graph2.GraphPane.XAxis.Title = "时间";
            graph2.GraphPane.YAxis.Title = "光强";
            graph2.GraphPane.FontSpec.Size = 30;
            graph2.GraphPane.XAxis.TitleFontSpec.Size = 25;
            graph2.GraphPane.XAxis.ScaleFontSpec.Size = 20;
            graph2.GraphPane.YAxis.TitleFontSpec.Size = 25;
            graph2.GraphPane.YAxis.ScaleFontSpec.Size = 20;
            graph2.Refresh();
        }

        private void Receive_Data(object sender, SerialDataReceivedEventArgs e)
        {
            if (!port.IsOpen)
            {
                return;
            }
            int len = port.BytesToRead;
            for(int i=0; i<len; i++)
            {
                try
                {
                    int data1 = port.ReadByte();
                    int data2 = port.ReadByte();
                    int data3 = port.ReadByte();
                    string message;
                    message = data1.ToString("x2") + "-";
                    message = message + data2.ToString("x2") + "-";
                    message = message + data3.ToString("x2");
                    i += 2;
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        listView2.Items.Add(message);
                        listView2.ScrollIntoView(message);
                    }));
                    if (isRecord)
                    {
                        sw.WriteLine("接收:" + message + "\n");
                        sw.Flush();
                    }
                    if (data1 == (byte)0xE0)//温度
                    {
                        temp.Clear();
                        temp.Add((byte)data1);
                        temp.Add((byte)data2);
                        temp.Add((byte)data3);
                        if (temp[0] != 0 && temp.Count == 3)
                            data.Temperature = (temp[1] | (temp[2] << 7)).ToString();
                        temp_list.Add(temp_list.Count, data.Get_Temp());
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            graph1.GraphPane.AddCurve("", temp_list, System.Drawing.Color.Red);
                            graph1.AxisChange();
                            graph1.Refresh();
                        }));
                    }
                    if (data1 == (byte)0xE1)
                    {
                        light.Clear();
                        light.Add((byte)data1);
                        light.Add((byte)data2);
                        light.Add((byte)data3);
                        if (light[0] != 0 && light.Count == 3)
                            data.Intensity = (light[1] | (light[2] << 7)).ToString();
                        light_list.Add(light_list.Count, data.Get_Inte());
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            graph2.GraphPane.AddCurve("", light_list, System.Drawing.Color.Blue);
                            graph2.AxisChange();
                            graph2.Refresh();
                        }));
                    }
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    //return;
                }
                
            }
        }

        private void Cbb1_DropDownOpened(object sender, EventArgs e)//下拉事件，填入端口号
        {
            string[] str = SerialPort.GetPortNames();
            int count = str.Length;
            cbb1.Items.Clear();
            for (int i=0; i<count; i++)
            {
                if (!cbb1.Items.Contains(str[i]))
                {
                    cbb1.Items.Add(str[i]);
                }
            }
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)//串口连接按钮点击事件
        {
            if (cbb1.SelectedItem != null)
            {
                if (!port.IsOpen)
                {
                    try
                    {
                        port = new SerialPort(cbb1.SelectedItem.ToString());//连接串口
                        port.BaudRate = int.Parse(cbb2.SelectedItem.ToString());//设置波特率
                        port.ReceivedBytesThreshold = 1;
                        port.DataReceived += new SerialDataReceivedEventHandler(Receive_Data);
                        port.Open();

                        System.Windows.MessageBox.Show("串口已连接");
                    }
                    catch(Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("请选择一个端口");
            }
        }

        private void Btn3_Click(object sender, RoutedEventArgs e)//发送按钮
        {
            if (port == null || !port.IsOpen)
            {
                System.Windows.MessageBox.Show("串口未连接");
                return;
            }
            //红灯
            byte cmd = (byte)0xD5;
            byte first = (byte)((int)(red.Value * 255 / 10) & 0x7f);
            byte second = (byte)((int)(red.Value * 255 / 10) >> 7);
            byte[] b = { cmd, first, second };
            string message = cmd.ToString("x2") + "-" + first.ToString("x2") + "-" + second.ToString("x2");
            port.Write(b, 0, 3);
            listView1.Items.Add(message);
            if (isRecord)
            {
                sw.WriteLine("发送:" + message + "\n");
                sw.Flush();
            }
            //绿灯
            cmd = (byte)0xD6;
            first = (byte)((int)(green.Value * 255 / 10) & 0x7f);
            second = (byte)((int)(green.Value * 255 / 10) >> 7);
            b = new byte[] { cmd, first, second };
            message = cmd.ToString("x2") + "-" + first.ToString("x2") + "-" + second.ToString("x2");
            port.Write(b, 0, 3);
            listView1.Items.Add(message);
            if (isRecord)
            {
                sw.WriteLine("发送:" + message + "\n");
                sw.Flush();
            }
            //黄灯
            cmd = (byte)0xD3;
            first = (byte)((int)(yellow.Value * 255 / 10) & 0x7f);
            second = (byte)((int)(yellow.Value * 255 / 10) >> 7);
            b = new byte[] { cmd, first, second };
            message = cmd.ToString("x2") + "-" + first.ToString("x2") + "-" + second.ToString("x2");
            port.Write(b, 0, 3);
            listView1.Items.Add(message);
            if (isRecord)
            {
                sw.WriteLine("发送:" + message + "\n");
                sw.Flush();
            }
            //蓝灯
            cmd = (byte)0xD9;
            first = (byte)((int)(blue.Value * 255 / 10) & 0x7f);
            second = (byte)((int)(blue.Value * 255 / 10) >> 7);
            b = new byte[] { cmd, first, second };
            message = cmd.ToString("x2") + "-" + first.ToString("x2") + "-" + second.ToString("x2");
            port.Write(b, 0, 3);
            listView1.Items.Add(message);
            if (isRecord)
            {
                sw.WriteLine("发送:" + message + "\n");
                sw.Flush();
            }
            //白灯
            cmd = (byte)0xDA;
            first = (byte)((int)(white.Value * 255 / 10) & 0x7f);
            second = (byte)((int)(white.Value * 255 / 10) >> 7);
            b = new byte[] { cmd, first, second };
            message = cmd.ToString("x2") + "-" + first.ToString("x2") + "-" + second.ToString("x2");
            port.Write(b, 0, 3);
            listView1.Items.Add(message);
            if (isRecord)
            {
                sw.WriteLine("发送:" + message + "\n");
                sw.Flush();
            }
        }

        private void Btn4_Click(object sender, RoutedEventArgs e)//log开始
        {
            if (port == null || !port.IsOpen)
            {
                System.Windows.MessageBox.Show("串口未连接");
            }
            if (isRecord)
            {
                return;
            }
            SaveFileDialog logFile = new SaveFileDialog();
            logFile.Filter = "txt文件(*.txt)|*.txt";
            logFile.FileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            if(logFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                isRecord = true;
                sw = File.CreateText(logFile.FileName);
                sw.WriteLine("串口号:" + cbb1.SelectedItem.ToString());
                sw.WriteLine("波特率:" + cbb2.SelectedItem.ToString() + "\n");
                sw.WriteLine("温度:" + textBox1.Text + "\n");
                sw.WriteLine("光强:" + textBox2.Text + "\n");
                sw.Flush();
            }
        }

        private void Btn5_Click(object sender, RoutedEventArgs e)//log结束
        {
            if (isRecord)
            {
                sw.Flush();
                sw.Close();
                isRecord = false;
            }
        }

        private void Rdb2_Checked(object sender, RoutedEventArgs e)//切换温度光强图
        {
            wfh1.Visibility = Visibility.Hidden;
            wfh2.Visibility = Visibility.Visible;
        }

        private void Rdb1_Checked(object sender, RoutedEventArgs e)//切换温度光强图
        {
            wfh1.Visibility = Visibility.Visible;
            wfh2.Visibility = Visibility.Hidden;
        }

        //改变色块颜色
        private void Color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //各颜色RGB值
            //red(255, 0, 0)
            //green(0, 128, 0)
            //yellow(255, 255, 0)
            //blue(0, 0, 255)
            //white(255, 255, 255)
            red_value = red.Value * 255 / 10;
            green_value = green.Value * 255 / 10;
            yellow_value = yellow.Value * 255 / 10;
            blue_value = blue.Value * 255 / 10;
            white_value = white.Value * 255 / 10;
            double r = 255 * red.Value / 10 + 0 * green.Value / 10 + 255 * yellow.Value / 10 + 0 * blue.Value / 10 + 255 * white.Value / 10;
            double g = 0 * red.Value / 10 + 128 * green.Value / 10 + 255 * yellow.Value / 10 + 0 * blue.Value / 10 + 255 * white.Value / 10;
            double b = 0 * red.Value / 10 + 0 * green.Value / 10 + 0 * yellow.Value / 10 + 255 * blue.Value / 10 + 255 * white.Value / 10;
            color.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b));
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)//串口关闭
        {
            if (port != null || port.IsOpen)
            {
                port.Close();
                port.DataReceived -= new SerialDataReceivedEventHandler(Receive_Data);
                //关闭后清屏
                listView1.Items.Clear();
                listView2.Items.Clear();
                graph1.GraphPane.CurveList.Clear();
                graph2.GraphPane.CurveList.Clear();
                temp_list.Clear();
                light_list.Clear();
                graph1.Refresh();
                graph2.Refresh();

                textBox1.Text = "0.00℃";
                textBox2.Text = "0℉";
                System.Windows.MessageBox.Show("串口已关闭");
            }
        }
    }
}
