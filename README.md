# 实验4 《C# .NET 综合应用程序开发》实验报告
学院：软件学院  班级：软工四班   学号：3017218159   姓名：李琛
日期：2019年5月29日
# 一、功能概述：
  1.可以与Arduino进行通信，接收Arduino的AD数据（光强、温度）  
  2.可以通过PWM控制与Arduino相连的5个LED灯的亮度  
  3.数据交换的格式采用midi协议  
# 二、项目特色
  1.ComboBox可以实时监测PC串口  
  2.可以选择连接、断开串口  
  3.ListView中实时显示发送和接收的数据  
  4.实时显示温度和光强随时间变化的曲线图，并用RadioButton来切换温度曲线和光强曲线  
  5.用Slider控制各灯亮度，通过点击发送按钮发送信息  
  6.在色块中显示各颜色的RGB混合色  
  7.生成log文件，记录发送和接收信息  
# 三、代码总量
  500+行
# 四、工作时间
  一周左右
# 五、知识点总结
  1.WPF界面设计  
  2.使用serialPort类连接串口  
  3.使用数据绑定实时显示温度光强  
  4.使用ZedGraph绘制温度光强曲线图  
  5.实现PC与Arduino之间的数据交换  
  6.了解了MIDI协议的编码方式  
  7.用txt文件保存了数据交换信息  
# 六、结论
## 实现过程
 1.在MainWindow.xaml中设计界面布局  
 2.在MainWindow.xaml.cs中写界面控制事件的代码  
   ①ComboBox的下拉事件实现实时监测PC串口  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/1.png)  
   ②点击按钮实现连接、断开串口  
    连接时设置串口信息，断开时清空界面信息  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/2.png)  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/3.png)  
   ③PC端接收数据时的操作：  
    在ListView中显示接收数据  
    显示实时温度光强  
    绘制温度光强曲线图  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/4.png)  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/5.png)  
   ④对曲线图的初始化  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/6.png)  
   ⑤RGB色块的绘制  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/7.png)  
   ⑥log文件的创建  
    ![image](https://github.com/3017218159/LAB4/blob/master/picture/14.png)  
## 实现结果
  1.界面  
  ![image](https://github.com/3017218159/LAB4/blob/master/picture/8.png)  
  2.接收发送数据  
  ![image](https://github.com/3017218159/LAB4/blob/master/picture/9.png)  
  ![image](https://github.com/3017218159/LAB4/blob/master/picture/10.png)  
  3.log文件  
  ![image](https://github.com/3017218159/LAB4/blob/master/picture/11.png)  
  4.电路连接  
  ![image](https://github.com/3017218159/LAB4/blob/master/picture/13.png)  
