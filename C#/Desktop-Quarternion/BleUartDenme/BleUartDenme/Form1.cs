using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using ZedGraph;
using System.IO;
using Optoel;
using System.Globalization;

namespace BleUartDenme
{
    public partial class Form1 : Form
    {

        #region Objects

        string accelPath = @"Accel.csv";
        string gyroPath = @"Gyro.csv";
        string magnoPath = @"Magno.csv";
        string AllDataPath = @"AllData.csv";
        string QuaternionPath = @"Quternion.csv";
        string EulerPath = @"Euler.csv";

        FileStream fileStreamAccel;
        FileStream fileStreamGyro;
        FileStream fileStreamMagno;
        FileStream fileStreamAllData;
        FileStream fileStreamQuternion;
        FileStream fileStreamEuler;
        StreamWriter swAccel;
        StreamWriter swGyro;
        StreamWriter swMagno;
        StreamWriter swAllData;
        StreamWriter swQuaternion;
        StreamWriter swEuler;

        public Thread thread;

        //Serialport
        public GraphPane accelPaneSP = new GraphPane();
        public PointPairList accelXSP = new PointPairList();
        public PointPairList accelYSP = new PointPairList();
        public PointPairList accelZSP = new PointPairList();
        public LineItem accelCurveSP;

        public GraphPane gyroPaneSP = new GraphPane();
        public PointPairList gyroXSP = new PointPairList();
        public PointPairList gyroYSP = new PointPairList();
        public PointPairList gyroZSP = new PointPairList();
        public LineItem gyroCurveSP;

        public GraphPane magnoPaneSP = new GraphPane();
        public PointPairList magnoXSP = new PointPairList();
        public PointPairList magnoYSP = new PointPairList();
        public PointPairList magnoZSP = new PointPairList();
        public LineItem magnoCurveSP;


        //Ble1
        public GraphPane accelPaneBle1 = new GraphPane();
        public PointPairList accelXBle1 = new PointPairList();
        public PointPairList accelYBle1 = new PointPairList();
        public PointPairList accelZBle1 = new PointPairList();
        public LineItem accelCurveBle1;

        public GraphPane gyroPaneBle1 = new GraphPane();
        public PointPairList gyroXBle1 = new PointPairList();
        public PointPairList gyroYBle1 = new PointPairList();
        public PointPairList gyroZBle1 = new PointPairList();
        public LineItem gyroCurveBle1;

        public GraphPane magnoPaneBle1 = new GraphPane();
        public PointPairList magnoXBle1 = new PointPairList();
        public PointPairList magnoYBle1 = new PointPairList();
        public PointPairList magnoZBle1 = new PointPairList();
        public LineItem magnoCurveBle1;

        //Ble2
        public GraphPane accelPaneBle2 = new GraphPane();
        public PointPairList accelXBle2 = new PointPairList();
        public PointPairList accelYBle2 = new PointPairList();
        public PointPairList accelZBle2 = new PointPairList();
        public LineItem accelCurveBle2;

        public GraphPane gyroPaneBle2 = new GraphPane();
        public PointPairList gyroXBle2 = new PointPairList();
        public PointPairList gyroYBle2 = new PointPairList();
        public PointPairList gyroZBle2 = new PointPairList();
        public LineItem gyroCurveBle2;

        public GraphPane magnoPaneBle2 = new GraphPane();
        public PointPairList magnoXBle2 = new PointPairList();
        public PointPairList magnoYBle2 = new PointPairList();
        public PointPairList magnoZBle2 = new PointPairList();
        public LineItem magnoCurveBle2;


        public GraphPane gyroPane = new GraphPane();
        public PointPairList Yaw = new PointPairList();
        public PointPairList Pitch = new PointPairList();
        public PointPairList Roll = new PointPairList();
        public LineItem gyroCurve;

        //Quaternion
        public GraphPane quaternionPane = new GraphPane();
        public PointPairList quaternion0 = new PointPairList();
        public PointPairList quaternion1 = new PointPairList();
        public PointPairList quaternion2 = new PointPairList();
        public PointPairList quaternion3 = new PointPairList();
        public PointPairList quaternion4 = new PointPairList();
        public PointPairList quaternion5 = new PointPairList();
        public LineItem quaternionCurve;

        //MAG axiss
        public GraphPane MagaxisPane = new GraphPane();
        public PointPairList MagXY = new PointPairList();
        public PointPairList MagYZ = new PointPairList();
        public PointPairList MagXZ = new PointPairList();
        public LineItem MagAxisCurve;


        //MAG Compaszasyon
        public GraphPane MagCompasPane = new GraphPane();
        public PointPairList MagCompasXY = new PointPairList();
        public PointPairList MagCompasYZ = new PointPairList();
        public PointPairList MagCompasXZ = new PointPairList();
        public LineItem MagCompasCurve;

        Optoel.SerialPort serialPort;
        Optoel.Ble Ble;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        #endregion Objects

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            
            File.Delete(accelPath);
            File.Delete(gyroPath);
            File.Delete(magnoPath);
            File.Delete(AllDataPath);
            File.Delete(QuaternionPath);
            File.Delete(EulerPath);
            fileStreamAccel = new FileStream(accelPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamGyro = new FileStream(gyroPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamMagno = new FileStream(magnoPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamAllData = new FileStream(AllDataPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamQuternion = new FileStream(QuaternionPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamEuler = new FileStream(EulerPath, FileMode.OpenOrCreate, FileAccess.Write);
            swAccel = new StreamWriter(fileStreamAccel);
            swGyro = new StreamWriter(fileStreamGyro);
            swMagno = new StreamWriter(fileStreamMagno);
            swAllData = new StreamWriter(fileStreamAllData);
            swQuaternion = new StreamWriter(fileStreamQuternion);
            swEuler = new StreamWriter(fileStreamEuler);


            GettingMagCompan = Settings.GetMagCompan();

            serialPort = new Optoel.SerialPort();

            serialPort.SeriportConnectResult += new Optoel.SerialPort.ConnectResultEventHandler(ConnectResultResponse);
            serialPort.EnableResult += new Optoel.SerialPort.EnableEventHandler(serialPortEnableResultEvent);
            serialPort.BwResult += new Optoel.SerialPort.BwEventHandler(serialPortBwResultEvent);
            serialPort.RangeResult += new Optoel.SerialPort.RangeEventHandler(serialPortRangeResultEvent);
            serialPort.ReadEnableResult += new Optoel.SerialPort.ReadEnableEventHandler(serialPortReadEnableResultEvent);

            serialPort.SeriportAccelData += new Optoel.SerialPort.AccelDataEventHandler(AccelDataEvent);
            serialPort.SeriportGyroData += new Optoel.SerialPort.GyroDataEventHandler(GyroDataEvent);
            serialPort.SeriportMagnoData += new Optoel.SerialPort.MagnoDataEventHandler(MagnoDataEvent);


            Ble = new Optoel.Ble();

            Ble.BleDiscoverResult += new Optoel.Ble.BleDiscoverResultEventHandler(BleDiscoverReslutEvent);
            Ble.BleDiscoverDeivce += new Optoel.Ble.BleDiscoverDeviceEventHandler(BleDiscoverDeviceEvent);
            Ble.BleConnectResult += new Optoel.Ble.BleConnectResultEventHandler(BleConnectResultEvent);
            Ble.BleErrorEvent += new Optoel.Ble.BleErrorEventHandler(BleErrorEvent);

            Ble.EnableResult += new Ble.EnableEventHandler(EnableResultEvent);
            Ble.BwResult += new Ble.BwEventHandler(BwResultEvent);
            Ble.RangeResult += new Ble.RangeEventHandler(RangeResultEvent);
            Ble.ReadEnableResult += new Ble.ReadEnableEventHandler(ReadEnableResultEvent);

            Ble.AccelData += new Optoel.Ble.AccelDataEventHandler(BleAccelData);
            Ble.GyroData += new Optoel.Ble.GyroDataEventHandler(BleGyroData);
            Ble.MagnoData += new Optoel.Ble.MagnoDataEventHandler(BleMagnoData);

            Ble.DisConnected += new Optoel.Ble.DisconnectedEventHandler(BleDisconnectedEvent);
            Ble.GetRssi += new Optoel.Ble.GetRSSIEventHandler(BleGetRssiEvent);
            Ble.SlaveError += new Optoel.Ble.SlaveErrorEventHandler(BleSlaveErrorEvent);

            timer.Enabled = true;
            timer.Tick += new System.EventHandler(this.timer_Tick);
            timer.Interval = 1000;
            
        }

        #region Ble Event Functions
        private void EnableResultEvent(Ble.BleSlave sender, Ble.BleSlave.EnableResultArgs e)
        {
            switch (sender.SlaveNumber)
            {
                case 0:
                    switch (e.Sensor)
                    {
                        case Optoel.BMX055.Sensor.Accel:
                            switch (e.Enable)
                            {
                                case Optoel.BMX055.Enable.Enable:
                                    AccelEnableBle1 = true;
                                    lbl_AccelEnableBle1.Text = "Enable";
                                    lbl_AccelEnableBle1.BackColor = Color.Lime;

                                    break;
                                case Optoel.BMX055.Enable.Disable:
                                    AccelEnableBle1 = false;
                                    lbl_AccelEnableBle1.Text = "Disable";
                                    lbl_AccelEnableBle1.BackColor = Color.Red;

                                    break;
                            }
                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            switch (e.Enable)
                            {
                                case Optoel.BMX055.Enable.Enable:
                                    GyroEnableBle1 = true;
                                    lbl_GyroEnableBle1.Text = "Enable";
                                    lbl_GyroEnableBle1.BackColor = Color.Lime;

                                    break;
                                case Optoel.BMX055.Enable.Disable:
                                    GyroEnableBle1 = false;
                                    lbl_GyroEnableBle1.Text = "Disable";
                                    lbl_GyroEnableBle1.BackColor = Color.Red;

                                    break;
                            }
                            break;
                        case Optoel.BMX055.Sensor.Magno:
                            switch (e.Enable)
                            {
                                case Optoel.BMX055.Enable.Enable:
                                    MagnoEnableBle1 = true;
                                    lbl_MagnoEnableBle1.Text = "Enable";
                                    lbl_MagnoEnableBle1.BackColor = Color.Lime;

                                    break;
                                case Optoel.BMX055.Enable.Disable:
                                    MagnoEnableBle1 = false;
                                    lbl_MagnoEnableBle1.Text = "Disable";
                                    lbl_MagnoEnableBle1.BackColor = Color.Red;

                                    break;
                            }
                            break;
                    }
                    break;
                case 1:
                    switch (e.Sensor)
                    {
                        case Optoel.BMX055.Sensor.Accel:
                            switch (e.Enable)
                            {
                                case Optoel.BMX055.Enable.Enable:
                                    AccelEnableBle2 = true;
                                    lbl_AccelEnableBle2.Text = "Enable";
                                    lbl_AccelEnableBle2.BackColor = Color.Lime;

                                    break;
                                case Optoel.BMX055.Enable.Disable:
                                    AccelEnableBle2 = false;
                                    lbl_AccelEnableBle2.Text = "Disable";
                                    lbl_AccelEnableBle2.BackColor = Color.Red;

                                    break;
                            }
                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            switch (e.Enable)
                            {
                                case Optoel.BMX055.Enable.Enable:
                                    GyroEnableBle2 = true;
                                    lbl_GyroEnableBle2.Text = "Enable";
                                    lbl_GyroEnableBle2.BackColor = Color.Lime;

                                    break;
                                case Optoel.BMX055.Enable.Disable:
                                    GyroEnableBle2 = false;
                                    lbl_GyroEnableBle2.Text = "Disable";
                                    lbl_GyroEnableBle2.BackColor = Color.Red;

                                    break;
                            }
                            break;
                        case Optoel.BMX055.Sensor.Magno:
                            switch (e.Enable)
                            {
                                case Optoel.BMX055.Enable.Enable:
                                    MagnoEnableBle2 = true;
                                    lbl_MagnoEnableBle2.Text = "Enable";
                                    lbl_MagnoEnableBle2.BackColor = Color.Lime;

                                    break;
                                case Optoel.BMX055.Enable.Disable:
                                    MagnoEnableBle2 = false;
                                    lbl_MagnoEnableBle2.Text = "Disable";
                                    lbl_MagnoEnableBle2.BackColor = Color.Red;

                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        private void BwResultEvent(Ble.BleSlave sender, Ble.BleSlave.BwResultArgs e)
        {
            switch (sender.SlaveNumber)
            {
                case 0:
                    switch (e.Sensor)
                    {
                        case BMX055.Sensor.Accel:
                            if (e.Success == true)
                                AccelBwBle1 = trackBarAccelBwBle1.Value;
                            else
                                trackBarAccelBwBle1.Value = AccelBwBle1;

                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            if (e.Success == true)
                                GyroBwBle1 = trackBarGyroBwBle1.Value;
                            else
                                trackBarGyroBwBle1.Value = GyroBwBle1;

                            break;
                        case Optoel.BMX055.Sensor.Magno:
                            if (e.Success == true)
                                MagnoBwBle1 = trackBarMagnoBwBle1.Value;
                            else
                                trackBarMagnoBwBle1.Value = MagnoBwBle1;

                            break;
                    }
                    break;
                case 1:
                    switch (e.Sensor)
                    {
                        case Optoel.BMX055.Sensor.Accel:
                            if (e.Success == true)
                                AccelBwBle2 = trackBarAccelBwBle2.Value;
                            else
                                trackBarAccelBwBle2.Value = AccelBwBle2;
                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            if (e.Success == true)
                                GyroBwBle2 = trackBarGyroBwBle2.Value;
                            else
                                trackBarGyroBwBle2.Value = GyroBwBle2;
                            break;
                        case Optoel.BMX055.Sensor.Magno:
                            if (e.Success == true)
                                MagnoBwBle2 = trackBarMagnoBwBle2.Value;
                            else
                                trackBarMagnoBwBle2.Value = MagnoBwBle2;

                            break;
                    }
                    break;
            }
        }

        private void RangeResultEvent(Ble.BleSlave sender, Ble.BleSlave.RangeResultArgs e)
        {
            switch (sender.SlaveNumber)
            {
                case 0:
                    switch (e.Sensor)
                    {
                        case Optoel.BMX055.Sensor.Accel:
                            if (e.Success == true)
                                AccelRangeBle1 = trackBarAccelRangeBle1.Value;
                            else
                                trackBarAccelRangeBle1.Value = AccelRangeBle1;

                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            if (e.Success == true)
                                GyroRangeBle1 = trackBarGyroRangeBle1.Value;
                            else
                                trackBarGyroRangeBle1.Value = GyroRangeBle1;

                            break;
                    }
                    break;
                case 1:
                    switch (e.Sensor)
                    {
                        case Optoel.BMX055.Sensor.Accel:
                            if (e.Success == true)
                                AccelRangeBle2 = trackBarAccelRangeBle2.Value;
                            else
                                trackBarAccelRangeBle2.Value = AccelRangeBle2;

                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            if (e.Success == true)
                                GyroRangeBle2 = trackBarGyroRangeBle2.Value;
                            else
                                trackBarGyroRangeBle2.Value = GyroRangeBle2;

                            break;
                    }
                    break;
            }
        }

        private void ReadEnableResultEvent(Ble.BleSlave sender, Ble.BleSlave.ReadEnableResultArgs e)
        {
            switch (sender.SlaveNumber)
            {
                case 0:
                    switch (e.Sensor)
                    {
                        case BMX055.Sensor.Accel:
                            if (e.Enable == BMX055.Enable.Enable )
                            {
                                lbl_AccelReadEnableBle1.Text = "Enable";
                                lbl_AccelReadEnableBle1.BackColor = Color.Lime;
                                AccelReadEnableBle1 = true;
                            }
                            else if (e.Enable == BMX055.Enable.Disable)
                            {
                                lbl_AccelReadEnableBle1.Text = "Disable";
                                lbl_AccelReadEnableBle1.BackColor = Color.Red;
                                AccelReadEnableBle1 = false;
                            }
                            break;
                        case BMX055.Sensor.Gyro:
                            if (e.Enable == BMX055.Enable.Enable)
                            {
                                lbl_GyroReadEnableBle1.Text = "Enable";
                                lbl_GyroReadEnableBle1.BackColor = Color.Lime;
                                GyroReadEnableBle1 = true;
                            }
                            else if (e.Enable == BMX055.Enable.Disable)
                            {
                                lbl_GyroReadEnableBle1.Text = "Disable";
                                lbl_GyroReadEnableBle1.BackColor = Color.Red;
                                GyroReadEnableBle1 = false;
                            }
                            break;
                        case BMX055.Sensor.Magno:
                            if (e.Enable == BMX055.Enable.Enable)
                            {
                                lbl_MagnoReadEnableBle1.Text = "Enable";
                                lbl_MagnoReadEnableBle1.BackColor = Color.Lime;
                                MagnoReadEnableBle1 = true;
                            }
                            else if (e.Enable == BMX055.Enable.Disable)
                            {
                                lbl_MagnoReadEnableBle1.Text = "Disable";
                                lbl_MagnoReadEnableBle1.BackColor = Color.Red;
                                MagnoReadEnableBle1 = false;
                            }
                            break;
                    }
                    break;
                case 1:
                    switch (e.Sensor)
                    {
                        case BMX055.Sensor.Accel:
                            if (e.Enable == BMX055.Enable.Enable)
                            {
                                lbl_AccelReadEnableBle2.Text = "Enable";
                                lbl_AccelReadEnableBle2.BackColor = Color.Lime;
                                AccelReadEnableBle2 = true;
                            }
                            else if (e.Enable == BMX055.Enable.Disable)
                            {
                                lbl_AccelReadEnableBle2.Text = "Disable";
                                lbl_AccelReadEnableBle2.BackColor = Color.Red;
                                AccelReadEnableBle2 = false;
                            }
                            break;
                        case Optoel.BMX055.Sensor.Gyro:
                            if (e.Enable == BMX055.Enable.Enable)
                            {
                                lbl_GyroReadEnableBle2.Text = "Enable";
                                lbl_GyroReadEnableBle2.BackColor = Color.Lime;
                                GyroReadEnableBle2 = true;
                            }
                            else if (e.Enable == BMX055.Enable.Disable)
                            {
                                lbl_GyroReadEnableBle2.Text = "Disable";
                                lbl_GyroReadEnableBle2.BackColor = Color.Red;
                                GyroReadEnableBle2 = false;
                            }
                            break;
                        case Optoel.BMX055.Sensor.Magno:
                            if (e.Enable == BMX055.Enable.Enable)
                            {
                                lbl_MagnoReadEnableBle2.Text = "Enable";
                                lbl_MagnoReadEnableBle2.BackColor = Color.Lime;
                                MagnoReadEnableBle2 = true;
                            }
                            else if (e.Enable == BMX055.Enable.Disable)
                            {
                                lbl_MagnoReadEnableBle2.Text = "Disable";
                                lbl_MagnoReadEnableBle2.BackColor = Color.Red;
                                MagnoReadEnableBle2 = false;
                            }
                            break;
                    }
                    break;
            }
        }



        private void timer_Tick(object sender, EventArgs e)
        {
            if (ListBleSlave.Count > 0)
            {
                for (int i = 0; i < ListBleSlave.Count; i++)
                {
                    ListBleSlave[i].GetRssi();
                }
            }
        }


        private void BleSlaveErrorEvent(Ble.BleSlave sender, Ble.ErrorHandleArgs e)
        {
            if (sender.SlaveNumber == 0)
            {
                lbl_ErrorMsgBle1.Text = e.ErrorMessage;
            }
            else if (sender.SlaveNumber == 1)
            {

            }
        }


        private void BleGetRssiEvent(Ble.BleSlave sender, Ble.BleSlave.GetRSSIArgs e)
        {
            if (sender.SlaveNumber == 0)
            {
                lbl_RssiBle1.Text = e.Rssi.ToString();
            }
            else if (sender.SlaveNumber == 1)
            {
                lbl_RssiBle2.Text = e.Rssi.ToString();
            }
        }

        #endregion Ble Event Functions


        public long minaccelBle1 = 0, maxaccelBle1 = 250;
        public long minaccelBle2 = 0, maxaccelBle2 = 250;
        public long mingyroBle1 = 0, maxgyroBle1 = 250;
        public long mingyroBle2 = 0, maxgyroBle2 = 250;
        public long minmagnoBle1 = 0, maxmagnoBle1 = 250;
        public long minmagnoBle2 = 0, maxmagnoBle2 = 250;
        public long minYawPitchRoll = 0, maxYawPitchRoll = 250;

        //Madgwick madgwick = new Madgwick(0.008f, 0.95f);
        MadgwickAHRS madgwick = new MadgwickAHRS(0.008f, .001f);

        long tick1 = 0;
        long tick2 = 0;
        float saniye = 0;
        List<float> listSaniye = new List<float>();
        BMX055.Accel accel = new BMX055.Accel(0, 0, 0, 0);
        
        private void BleAccelData(Ble.BleSlave sender, Optoel.BMX055.Accel Accel)
        {
            try
            {
                if (sender.SlaveNumber == 0)
                {
                   
                    label73.Text = sender.AccelLostDataCount.ToString();
                    accel = Accel;
                    //yaw_pitc_roll();
                    //tick1 = DateTime.Now.Ticks;

                    //swAccel.WriteLine(Accel.AccelDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Accel.RawX.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawY.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawZ.ToString(new CultureInfo("en-US", false)));
                    //swAccel.Flush();

                    //swAllData.WriteLine(
                    //    accel.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                    //    accel.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                    //    accel.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

                    //    gyro.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                    //    gyro.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                    //    gyro.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

                    //    magno.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                    //    magno.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                    //    magno.RawZ.ToString(new CultureInfo("en-US", false))
                    //);
                    //swAllData.Flush();

                    //float accelX = accel.RawX / 1000.0f;
                    //float accelY = accel.RawY / 1000.0f;
                    //float accelZ = accel.RawZ / 1000.0f;
                    //roll = (float)(Math.Atan2(accelY, Math.Sqrt(Math.Pow(accelX, 2) + Math.Pow(accelZ, 2))) * 180 / Math.PI);
                    //pitch = (float)(Math.Atan2(-1 * accelX, Math.Sqrt(Math.Pow(accelY, 2) + Math.Pow(accelZ, 2))) * 180 / Math.PI);

                    madgwick.Update(Degree2Radian(gyro.RawX), Degree2Radian(gyro.RawY), Degree2Radian(gyro.RawZ), accel.RawX, accel.RawY, accel.RawZ, magno.RawX, magno.RawY, magno.RawZ);
                    //madgwick.Update(Degree2Radian(gyro.RawX), Degree2Radian(gyro.RawY), Degree2Radian(gyro.RawZ), accel.RawX, accel.RawY, accel.RawZ, corrected[0], corrected[1], corrected[2]);
                    //madgwick.Update(Degree2Radian(gyro.RawX), Degree2Radian(gyro.RawY), Degree2Radian(gyro.RawZ), -accel.RawX, -accel.RawY, accel.RawZ, magno.RawX, -magno.RawY, magno.RawZ);

                    //swQuaternion.WriteLine(
                    //    madgwick.Quaternion[0].ToString(new CultureInfo("en-US", false)) + "," +
                    //    madgwick.Quaternion[1].ToString(new CultureInfo("en-US", false)) + "," +
                    //    madgwick.Quaternion[2].ToString(new CultureInfo("en-US", false)) + "," +
                    //    madgwick.Quaternion[3].ToString(new CultureInfo("en-US", false))
                    //);
                    //swQuaternion.Flush();

                    Quaternion2Euler();

                    //swEuler.WriteLine(
                    //    Radian2Degree(euler[0]).ToString(new CultureInfo("en-US", false)) + "," +
                    //    Radian2Degree(euler[1]).ToString(new CultureInfo("en-US", false)) + "," +
                    //    Radian2Degree(euler[2]).ToString(new CultureInfo("en-US", false))
                    //);
                    //swEuler.Flush();

                    //if (filterState == true)
                    //{
                    //    madgwick.Update(Degree2Radian(gyro.RawX), Degree2Radian(gyro.RawY), Degree2Radian(gyro.RawZ), accel.RawX, accel.RawY, accel.RawZ, corrected[1], corrected[0], corrected[2]);
                    //}
                    //else
                    //{
                    //    madgwick.Update(Degree2Radian(gyro.RawX), Degree2Radian(gyro.RawY), Degree2Radian(gyro.RawZ), accel.RawX, accel.RawY, accel.RawZ, magno.RawY, -magno.RawX, magno.RawZ);
                    //}
                    //Quaternion2Euler();
                    //tick2 = DateTime.Now.Ticks;
                    //saniye = (float)((tick2 - tick1) / 10000.0);
                    //label67.Text = saniye.ToString();
                    //listSaniye.Add(saniye);
                    //if (Accel.AccelDataNumber % 5 == 0)
                    //{
                    //    accelXBle1.Add(new ZedGraph.PointPair(maxaccelBle1, Accel.RawX));
                    //    accelYBle1.Add(new ZedGraph.PointPair(maxaccelBle1, Accel.RawY));
                    //    accelZBle1.Add(new ZedGraph.PointPair(maxaccelBle1, Accel.RawZ));
                    //    accelPaneBle1.XAxis.Scale.Max = maxaccelBle1;
                    //    accelPaneBle1.XAxis.Scale.Min = minaccelBle1;

                    //    accelZedGraphBle1.Invalidate();
                    //    accelPaneBle1.AxisChange();
                    //    //accelZedGraf.Refresh();
                    //    maxaccelBle1++;
                    //    minaccelBle1++;
                    //}

                }
                else if (sender.SlaveNumber == 1)
                {
                    label70.Text = sender.AccelLostDataCount.ToString();

                    if (Accel.AccelDataNumber % 5 == 0)
                    {
                        accelXBle2.Add(new ZedGraph.PointPair(maxaccelBle2, Accel.RawX));
                        accelYBle2.Add(new ZedGraph.PointPair(maxaccelBle2, Accel.RawY));
                        accelZBle2.Add(new ZedGraph.PointPair(maxaccelBle2, Accel.RawZ));
                        accelPaneBle2.XAxis.Scale.Max = maxaccelBle2;
                        accelPaneBle2.XAxis.Scale.Min = minaccelBle2;

                        accelZedGraphBle2.Invalidate();
                        accelPaneBle2.AxisChange();
                        //accelZedGraf.Refresh();
                        maxaccelBle2++;
                        minaccelBle2++;
                    }
                }

            }
            catch (Exception ex)
            {
            }
            
        }


        BMX055.Gyro gyro = new BMX055.Gyro(0, 0, 0, 0);
        private void BleGyroData(Ble.BleSlave sender, Optoel.BMX055.Gyro Gyro)
        {
            try
            {
                if (sender.SlaveNumber == 0)
                {
                   
                    label72.Text = sender.GyroLostDataCount.ToString();
                    gyro = Gyro;

                    //swGyro.WriteLine(Gyro.GyroDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Gyro.RawX.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawY.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawZ.ToString(new CultureInfo("en-US", false)));
                    //swGyro.Flush();

                    //ComplementaryFilter();
                    //if (Gyro.GyroDataNumber % 5 == 0)
                    //{
                    //    gyroXBle1.Add(new ZedGraph.PointPair(maxgyroBle1, Gyro.RawX));
                    //    gyroYBle1.Add(new ZedGraph.PointPair(maxgyroBle1, Gyro.RawY));
                    //    gyroZBle1.Add(new ZedGraph.PointPair(maxgyroBle1, Gyro.RawZ));
                    //    gyroPaneBle1.XAxis.Scale.Max = maxgyroBle1;
                    //    gyroPaneBle1.XAxis.Scale.Min = mingyroBle1;

                    //    gyroZedGraphBle1.Invalidate();
                    //    gyroPaneBle1.AxisChange();
                    //    gyroZedGraf.Refresh();
                    //    maxgyroBle1++;
                    //    mingyroBle1++;
                    //}
                    
                }
                else if (sender.SlaveNumber == 1)
                {
                    label69.Text = sender.GyroLostDataCount.ToString();

                    if (Gyro.GyroDataNumber % 5 == 0)
                    {
                        gyroXBle2.Add(new ZedGraph.PointPair(maxgyroBle2, Gyro.RawX));
                        gyroYBle2.Add(new ZedGraph.PointPair(maxgyroBle2, Gyro.RawY));
                        gyroZBle2.Add(new ZedGraph.PointPair(maxgyroBle2, Gyro.RawZ));
                        gyroPaneBle2.XAxis.Scale.Max = maxgyroBle2;
                        gyroPaneBle2.XAxis.Scale.Min = mingyroBle2;

                        gyroZedGraphBle2.Invalidate();
                        gyroPaneBle2.AxisChange();
                        //gyroZedGraf.Refresh();
                        maxgyroBle2++;
                        mingyroBle2++;
                    }
                }

            }
            catch (Exception ex)
            {
            } 
        }


        List<float> listMagRawX = new List<float>();
        List<float> listMagRawY = new List<float>();
        List<float> listMagRawZ = new List<float>();

        Settings.MagCompan GettingMagCompan;

        BMX055.Magno magno = new BMX055.Magno(0, 0, 0, 0);
        private void BleMagnoData(Ble.BleSlave sender, Optoel.BMX055.Magno Magno)
        {
            try
            {
                if (sender.SlaveNumber == 0)
                {
                   
                    label71.Text = sender.MagnoLostDataCount.ToString();
                    magno = Magno;
                    //listMagRawX.Add(Magno.RawX);
                    //listMagRawY.Add(Magno.RawY);
                    //listMagRawZ.Add(Magno.RawZ);

                    //ManuelMagCal();
                    //swMagno.WriteLine(/*Magno.MagnoDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + */Magno.RawX.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawY.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawZ.ToString(new CultureInfo("en-US", false)));
                    //swMagno.Flush();

                    if (CompasazyonState)
                    {
                        offset[0] = -15.0f;
                        offset[1] = -33.0f;
                        offset[2] = -13.0f;

                        scale[0] = 0.8f;
                        scale[1] = 0.7f;
                        scale[2] = 2.03f;


                        m[0] = 0.1f;
                        m[1] = -1.2f;
                        m[2] = -1.1f;

                        corrected[0] = (magno.RawX - offset[0]) * scale[0] + (magno.RawY - offset[1]) * m[0] + (magno.RawZ - offset[2]) * m[2];
                        corrected[1] = (magno.RawX - offset[0]) * m[0] + (magno.RawY - offset[1]) * scale[1] + (magno.RawZ - offset[2]) * m[1];
                        corrected[2] = (magno.RawX - offset[0]) * m[2] + (magno.RawY - offset[1]) * m[1] + (magno.RawZ - offset[2]) * scale[2];

                        //MagCompasXY.Add(new PointPair(corrected[0], corrected[1]));
                        //MagCompasYZ.Add(new PointPair(corrected[1], corrected[2]));
                        //MagCompasXZ.Add(new PointPair(corrected[0], corrected[2]));
                        //zedGraphCompaszasyon.Invalidate();
                        //zedGraphCompaszasyon.AxisChange();

                        magnoXBle1.Add(new ZedGraph.PointPair(maxmagnoBle1, Magno.RawX));
                        magnoYBle1.Add(new ZedGraph.PointPair(maxmagnoBle1, Magno.RawY));
                        magnoZBle1.Add(new ZedGraph.PointPair(maxmagnoBle1, Magno.RawZ));
                        magnoPaneBle1.XAxis.Scale.Max = maxmagnoBle1;
                        magnoPaneBle1.XAxis.Scale.Min = minmagnoBle1;

                        magnoZedGraphBle1.Invalidate();
                        magnoPaneBle1.AxisChange();
                        //magnoZedGraf.Refresh();
                        maxmagnoBle1++;
                        minmagnoBle1++;
                    }
                    else
                    {
                        //MagXY.Add(new PointPair(Magno.RawX, Magno.RawY));
                        //MagYZ.Add(new PointPair(Magno.RawY, Magno.RawZ));
                        //MagXZ.Add(new PointPair(Magno.RawX, Magno.RawZ));
                        //zedGraphMag.Invalidate();
                        //zedGraphMag.AxisChange();
                    }


                    //MagXY.Add(new PointPair(corrected[0], corrected[1]));
                    //MagYZ.Add(new PointPair(corrected[1], corrected[2]));
                    //MagXZ.Add(new PointPair(corrected[0], corrected[2]));
                    //zedGraphMag.Invalidate();
                    //zedGraphMag.AxisChange();

                    //yaw_pitc_roll();
                    //magnoXBle1.Add(new ZedGraph.PointPair(maxmagnoBle1, Magno.RawX));
                    //magnoYBle1.Add(new ZedGraph.PointPair(maxmagnoBle1, Magno.RawY));
                    //magnoZBle1.Add(new ZedGraph.PointPair(maxmagnoBle1, Magno.RawZ));
                    //magnoPaneBle1.XAxis.Scale.Max = maxmagnoBle1;
                    //magnoPaneBle1.XAxis.Scale.Min = minmagnoBle1;

                    //magnoZedGraphBle1.Invalidate();
                    //magnoPaneBle1.AxisChange();
                    ////magnoZedGraf.Refresh();
                    //maxmagnoBle1++;
                    //minmagnoBle1++;  

                }
                else if (sender.SlaveNumber == 1)
                {
                    label68.Text = sender.MagnoLostDataCount.ToString();

                    magnoXBle2.Add(new ZedGraph.PointPair(maxmagnoBle2, Magno.RawX));
                    magnoYBle2.Add(new ZedGraph.PointPair(maxmagnoBle2, Magno.RawY));
                    magnoZBle2.Add(new ZedGraph.PointPair(maxmagnoBle2, Magno.RawZ));
                    magnoPaneBle2.XAxis.Scale.Max = maxmagnoBle2;
                    magnoPaneBle2.XAxis.Scale.Min = minmagnoBle2;

                    magnoZedGraphBle2.Invalidate();
                    magnoPaneBle2.AxisChange();
                    //magnoZedGraf.Refresh();
                    maxmagnoBle2++;
                    minmagnoBle2++;
                }

            }
            catch (Exception ex)
            {
            }
            
        }


        private void QuaternionDataEvent(float[] Quaternion)
        {
            //quaternion0.Add(new ZedGraph.PointPair(maxYawPitchRoll, Quaternion[0]));
            //quaternion1.Add(new ZedGraph.PointPair(maxYawPitchRoll, Quaternion[1]));
            //quaternion2.Add(new ZedGraph.PointPair(maxYawPitchRoll, Quaternion[2]));
            //quaternion2.Add(new ZedGraph.PointPair(maxYawPitchRoll, Quaternion[3]));
            //gyroPane.XAxis.Scale.Max = maxYawPitchRoll;
            //gyroPane.XAxis.Scale.Min = minYawPitchRoll;

            //GyrozedGraph.Invalidate();
            //gyroPane.AxisChange();
            ////gyroZedGraf.Refresh();
            //maxYawPitchRoll++;
            //minYawPitchRoll++;
        }

        private void BleDisconnectedEvent(Ble.BleSlave sender, Ble.BleSlave.DisconnectedEventArgs e)
        {
            if (sender.SlaveNumber == 0)
            {
                btn_ConnectBle1.BackColor = Color.Red;
                Ble1Connected = false;
                EnabledComponentBle1();

                lbl_ErrorMsgBle1.Text = e.Reason;
            }
            else if (sender.SlaveNumber == 1)
            {
                btn_ConnectBle2.BackColor = Color.Red;
                Ble2Connected = false;
                EnabledComponentBle2();
            }
        }


        static float yaw = 0;
        static float pitch = 0;
        static float roll = 0;
        static float dt = 0.01f;
        double[] euler = new double[3];

        void yaw_pitc_roll()
        {
            float accelX = accel.RawX / 1000.0f;
            float accelY = accel.RawY / 1000.0f;
            float accelZ = accel.RawZ / 1000.0f;
            roll = (float)(Math.Atan2(accelY, Math.Sqrt(Math.Pow(accelX, 2) + Math.Pow(accelZ, 2))) * 180 / Math.PI);
            pitch = (float)(Math.Atan2(-1 * accelX, Math.Sqrt(Math.Pow(accelY, 2) + Math.Pow(accelZ, 2))) * 180 / Math.PI);

            float norm, m11, m12, m13;

            norm = (float)Math.Sqrt(Math.Pow(magno.RawX, 2) + Math.Pow(magno.RawY, 2) + Math.Pow(magno.RawZ, 2));
            m11 = magno.RawX / norm;
            m12 = magno.RawY / norm;
            m13 = magno.RawZ / norm;

            float Mx, My;
            //Mx = (float)(magno.RawX * Math.Cos(pitch) + magno.RawZ * Math.Sin(pitch));
            //My = (float)(magno.RawX * Math.Sin(roll) * Math.Sin(pitch) + magno.RawY * Math.Cos(roll) - magno.RawZ * Math.Sin(roll) * Math.Cos(pitch));

            Mx = (float)(m11 * Math.Cos(pitch) + m12 * Math.Sin(roll) * Math.Sin(pitch) + m13 * Math.Cos(roll) * Math.Sin(pitch));
            My = (float)(m12 * Math.Cos(roll) - m13 * Math.Sin(roll));

            yaw = (float)(Math.Atan2(-My, Mx) * 180 / Math.PI);

            Yaw.Add(new ZedGraph.PointPair(maxYawPitchRoll, yaw));
            Pitch.Add(new ZedGraph.PointPair(maxYawPitchRoll, pitch));
            Roll.Add(new ZedGraph.PointPair(maxYawPitchRoll, roll));
            gyroPane.XAxis.Scale.Max = maxYawPitchRoll;
            gyroPane.XAxis.Scale.Min = minYawPitchRoll;

            GyrozedGraph.Invalidate();
            gyroPane.AxisChange();
            //gyroZedGraf.Refresh();
            maxYawPitchRoll++;
            minYawPitchRoll++;

        }

        long count = 0;
        void Quaternion2Euler()
        {
            try
            {
                double[] q = new double[4];
                
                q[0] = madgwick.Quaternion[0];
                q[1] = -1 * madgwick.Quaternion[1];
                q[2] = -1 * madgwick.Quaternion[2];
                q[3] = -1 * madgwick.Quaternion[3];

                #region formül1
                double R1 = (double)((2 * Math.Pow(q[0], 2) - 1) + (2 * Math.Pow(q[1], 2)));
                double R2 = (2 * q[1] * q[2]) - (q[0] * q[3]);
                double R3 = (2 * q[1] * q[3]) + (q[0] * q[2]);
                double R4 = (2 * q[2] * q[3]) - (q[0] * q[1]);
                double R5 = (double)((2 * Math.Pow(q[0], 2) - 1) + (2 * Math.Pow(q[3], 2)));

                euler[0] = (double)Math.Atan2(R4, R5);
                euler[1] = -1 * (double)Math.Atan(R3 / Math.Sqrt(1 - Math.Pow(R3, 2)));
                euler[2] = (double)Math.Atan2(R2, R1);

                //lbl_quat0.Text = Radian2Degree(euler[0]).ToString();
                //lbl_quat1.Text = Radian2Degree(euler[1]).ToString();
                //lbl_quat2.Text = Radian2Degree(euler[2]).ToString();

                //euler[0] = (float)Math.Asin(((R4 / R5) / Math.Sqrt(1 + Math.Pow(R4 / R5, 2))));
                //euler[1] = -1 * (float)Math.Atan(R3 / Math.Sqrt(1 - Math.Pow(R3, 2)));
                //euler[2] = (float)Math.Asin((R2 / R1) / Math.Sqrt(1 + Math.Pow(R2 / R1, 2)));

                #endregion formül1

                #region formül2
                //euler[0] = (float)Math.Asin(2 * (q[0] * q[1] + q[2] * q[3])/*, 1 - 2 * (Math.Pow(q[1], 2) + Math.Pow(q[2], 2))*/);
                //euler[1] = -(float)Math.Asin(2 * (q[0] * q[2] - q[3] * q[1]));
                //euler[2] = (float)Math.Asin(2 * (q[0] * q[3] + q[1] * q[2])/*, 1 - 2 * (Math.Pow(q[2], 2) + Math.Pow(q[3], 2))*/);

                //euler[0] = (float)Math.Asin(R4 / Math.Sqrt(R4 * R4 + R5 * R5));
                //euler[1] = -(float)Math.Asin(2 * (q[0] * q[2] - q[3] * q[1]));
                //euler[2] = (float)Math.Asin(R2 / Math.Sqrt(R1 * R1 + R2 * R2));
                #endregion formül2

                #region formül3
                //float sqw = q[0] * q[0];
                //float sqx = q[1] * q[1];
                //float sqy = q[2] * q[2];
                //float sqz = q[3] * q[3];

                //euler[2] = (float)Math.Atan2(2.0 * (q[1] * q[2] + q[3] * q[0]), (sqx - sqy - sqz + sqw));
                //euler[1] = (float)Math.Atan2(2.0 * (q[2] * q[3] + q[1] * q[0]), (-sqx - sqy + sqz + sqw));
                //euler[0] = (float)Math.Asin(-2.0 * ((q[1] * q[3]) - q[2] * q[0]) / (sqx + sqy + sqz + sqw));
                #endregion formül3



                if (count%2 == 0)
                {
                    quaternion0.Add(new ZedGraph.PointPair(maxYawPitchRoll, Radian2Degree(euler[0])));
                    quaternion1.Add(new ZedGraph.PointPair(maxYawPitchRoll, Radian2Degree(euler[1])));
                    quaternion2.Add(new ZedGraph.PointPair(maxYawPitchRoll, Radian2Degree(euler[2])));

                    //quaternion0.Add(new ZedGraph.PointPair(maxYawPitchRoll, q[0]));
                    //quaternion1.Add(new ZedGraph.PointPair(maxYawPitchRoll, q[1]));
                    //quaternion2.Add(new ZedGraph.PointPair(maxYawPitchRoll, q[2]));
                    //quaternion3.Add(new ZedGraph.PointPair(maxYawPitchRoll, q[3]));

                    gyroPane.XAxis.Scale.Max = maxYawPitchRoll;
                    gyroPane.XAxis.Scale.Min = minYawPitchRoll;

                    //lbl_quat0.Text = q[0].ToString();
                    //lbl_quat1.Text = q[1].ToString();
                    //lbl_quat2.Text = q[2].ToString();
                    //lbl_quat3.Text = q[3].ToString();

                    GyrozedGraph.Invalidate();
                    gyroPane.AxisChange();
                    //gyroZedGraf.Refresh();
                    maxYawPitchRoll++;
                    minYawPitchRoll++;
                }
                count++;
                
            }
            catch (Exception ex)
            {

            }
        }

        void set(float[] q)
        {
            float test = q[1] * q[2] + q[3] * q[0];
            if (test > 0.499f)
            {
                euler[2] = (float)(2 * Math.Atan2(q[1], q[0]));
                euler[1] = (float)(Math.PI / 2);
                euler[0] = 0;
                return;
            }
            if (test < -0.499f)
            {
                euler[2] = (float)(-2 * Math.Atan2(q[1], q[0]));
                euler[1] = (float)(-Math.PI / 2);
                euler[0] = 0;
                return;
            }

            float sqx = q[1] * q[1];
            float sqy = q[2] * q[2];
            float sqz = q[3] * q[3];

            euler[2] = (float)Math.Atan2(2.0 * (q[0] * q[1] - q[1] * q[2]), (1-2*sqy-2*sqz));
            euler[1] = (float)Math.Asin(2 * test);
            euler[0] = (float)Math.Atan2(2 * q[1] * q[0] - 2 * q[2] * q[3], 1 - 2 * sqx - 2 * sqz);
        }


        float[] offset = new float[3];
        float[] avg_delta = new float[3];
        float[] scale = new float[3];
        float[] m = new float[3];
        float[] corrected = new float[3] {0,0,0};

        Settings.MagCompan MagCompan = new Settings.MagCompan();

        void ManuelMagCal()
        {
            MagCompan.offset[0] = offset[0] = (float)((listMagRawX.Min() + listMagRawX.Max()) / 2.0);
            MagCompan.offset[1] = offset[1] = (float)((listMagRawY.Min() + listMagRawY.Max()) / 2.0);
            MagCompan.offset[2] = offset[2] = (float)((listMagRawZ.Min() + listMagRawZ.Max()) / 2.0);

            avg_delta[0] = (float)((listMagRawX.Min() - listMagRawX.Max()) / 2.0); if (avg_delta[0] == 0) return;
            avg_delta[1] = (float)((listMagRawY.Min() - listMagRawY.Max()) / 2.0); if (avg_delta[1] == 0) return;
            avg_delta[2] = (float)((listMagRawZ.Min() - listMagRawZ.Max()) / 2.0); if (avg_delta[2] == 0) return;

            float avg_deltat = (float)((avg_delta[0] + avg_delta[1] + avg_delta[2]) / 3.0);

            MagCompan.scale[0] = scale[0] = avg_deltat / avg_delta[0];
            MagCompan.scale[0] = scale[1] = avg_deltat / avg_delta[1];
            MagCompan.scale[0] = scale[2] = avg_deltat / avg_delta[2];


            MagCompan.m[0] = (float)((scale[0] - scale[1]));
            MagCompan.m[1] = (float)((scale[1] - scale[2]));
            MagCompan.m[2] = (float)((scale[0] - scale[2]));

            Settings.SaveMagCompan(MagCompan);

            MessageBox.Show("Offse: " + offset[0] + offset[1] + offset[2] + "\nScale: " + scale[0] + scale[1] + scale[2] + "\nm: " + m[0] + m[1] + m[2]);

            label45.Text = offset[0] + ", " + offset[1] + ", " + offset[2];
            label46.Text = scale[0] + ", " + scale[1] + ", " + scale[2];


            //corrected[0] = (magno.RawX-offset[0]) * scale[0] + (magno.RawY - offset[1]) * m[0] + (magno.RawZ - offset[2]) * m[2];
            //corrected[1] = (magno.RawX - offset[0]) * m[0] + (magno.RawY - offset[1]) * scale[1] + (magno.RawZ - offset[2]) * m[1];
            //corrected[2] = (magno.RawX - offset[0]) * m[2] + (magno.RawY - offset[1]) * m[1] + (magno.RawZ - offset[2]) * scale[2];

        }

        

        float acc_pitch = 0, gyro_ptich = 0;
        float acc_roll = 0, gyro_roll = 0;
        float acc_yaw = 0, gyro_yaw = 0;
        float Cal_GyroX = 0;
        float Cal_GyroY = 0;
        float Cal_GyroZ = 0;
        float alpha = 0.1f;
        
        void ComplementaryFilter()
        {
            //acc_yaw = (float)(Math.Atan(Math.Sqrt(Math.Pow(accel.RawX, 2) + Math.Pow(accel.RawZ, 2)) / accel.RawZ)*180/Math.PI);
            acc_pitch = (float)(Math.Atan(accel.RawY / Math.Sqrt(Math.Pow(accel.RawX, 2) + Math.Pow(accel.RawZ, 2))) * 180 / Math.PI); if (Double.IsNaN( acc_pitch)) return;
            acc_roll = (float)-(Math.Atan(accel.RawX / Math.Sqrt(Math.Pow(accel.RawY, 2) + Math.Pow(accel.RawZ, 2))) * 180 / Math.PI); if (Double.IsNaN(acc_pitch)) return;

            gyro_ptich += gyro.RawY * 0.01f;
            gyro_roll += gyro.RawX * 0.01f;
            gyro_yaw += gyro.RawZ * 0.01f;


            pitch = alpha * (pitch + gyro.RawY * 0.01f) + (1 - alpha) * acc_pitch;
            roll = alpha * (roll + gyro.RawX * 0.01f) + (1 - alpha) * acc_roll;
            

            quaternion0.Add(new ZedGraph.PointPair(maxYawPitchRoll, pitch));
            quaternion1.Add(new ZedGraph.PointPair(maxYawPitchRoll, roll));
            //quaternion2.Add(new ZedGraph.PointPair(maxYawPitchRoll, gyro_ptich));
            //quaternion3.Add(new ZedGraph.PointPair(maxYawPitchRoll, gyro_roll));
            //quaternion4.Add(new ZedGraph.PointPair(maxYawPitchRoll, gyro_yaw));
            gyroPane.XAxis.Scale.Max = maxYawPitchRoll;
            gyroPane.XAxis.Scale.Min = minYawPitchRoll;

            GyrozedGraph.Invalidate();
            gyroPane.AxisChange();
            //gyroZedGraf.Refresh();
            maxYawPitchRoll++;
            minYawPitchRoll++;
        }


        float Radian2Degree(float rad)
        {
            return (float)((180.0f / Math.PI) * rad);
        }

        double Radian2Degree(double rad)
        {
            return (double)((180.0f / Math.PI) * rad);
        }


        float Degree2Radian(float deg)
        {
            return (float)(Math.PI / 180) * deg;
        }

        bool filterState = false;

        private void btn_filterOnOff_Click_1(object sender, EventArgs e)
        {
            if (filterState == true)
            {
                btn_filterOnOff.Text = "Off";
                btn_filterOnOff.BackColor = Color.Red;
                filterState = false;
            }
            else
            {
                btn_filterOnOff.Text = "On";
                btn_filterOnOff.BackColor = Color.Lime;
                filterState = true;
            }
        }

        bool CompasazyonState = false;
        private void btn_CompasOnOff_Click(object sender, EventArgs e)
        {
            if (CompasazyonState == true)
            {
                btn_CompasOnOff.Text = "Off";
                btn_CompasOnOff.BackColor = Color.Red;
                CompasazyonState = false; 
            }
            else
            {
                btn_CompasOnOff.Text = "On";
                btn_CompasOnOff.BackColor = Color.Lime;
                CompasazyonState = true;

                ManuelMagCal();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Settings.GetStaticSettings();
            
            FirstPortSetting();
            Grafikhazirla();

        }

        

        void FirstPortSetting()
        {
            lbl_SPPort.Text = Settings.StaticSetting.SP_PortSetting.ComName;
            lbl_BlePort.Text = Settings.StaticSetting.Ble_PortSetting.ComName;
        }

        void Grafikhazirla()
        {
            //Serial ZedGraph
            accelPaneSP = accelZedGraf.GraphPane;
            gyroPaneSP = gyroZedGraf.GraphPane;
            magnoPaneSP = magnoZedGraf.GraphPane;

            accelPaneSP.YAxis.Scale.Max = 1000;
            accelPaneSP.YAxis.Scale.Min = -1000;
            gyroPaneSP.YAxis.Scale.Max = 2000;
            gyroPaneSP.YAxis.Scale.Min = -2000;
            magnoPaneSP.YAxis.Scale.Max = 500;
            magnoPaneSP.YAxis.Scale.Min = -500;

            accelCurveSP = accelPaneSP.AddCurve(null, accelXSP, Color.Red, SymbolType.None);
            accelCurveSP = accelPaneSP.AddCurve(null, accelYSP, Color.Blue, SymbolType.None);
            accelCurveSP = accelPaneSP.AddCurve(null, accelZSP, Color.Green, SymbolType.None);
            gyroCurve = gyroPaneSP.AddCurve(null, gyroXSP, Color.Red, SymbolType.None);
            gyroCurve = gyroPaneSP.AddCurve(null, gyroYSP, Color.Blue, SymbolType.None);
            gyroCurve = gyroPaneSP.AddCurve(null, gyroZSP, Color.Green, SymbolType.None);
            magnoCurveSP = magnoPaneSP.AddCurve(null, magnoXSP, Color.Red, SymbolType.None);
            magnoCurveSP = magnoPaneSP.AddCurve(null, magnoYSP, Color.Blue, SymbolType.None);
            magnoCurveSP = magnoPaneSP.AddCurve(null, magnoZSP, Color.Green, SymbolType.None);

            //Ble1 ZedGraph
            accelPaneBle1 = accelZedGraphBle1.GraphPane;
            gyroPaneBle1 = gyroZedGraphBle1.GraphPane;
            magnoPaneBle1 = magnoZedGraphBle1.GraphPane;

            accelPaneBle1.YAxis.Scale.Max = 1000;
            accelPaneBle1.YAxis.Scale.Min = -1000;
            gyroPaneBle1.YAxis.Scale.Max = 2000;
            gyroPaneBle1.YAxis.Scale.Min = -2000;
            magnoPaneBle1.YAxis.Scale.Max = 500;
            magnoPaneBle1.YAxis.Scale.Min = -500;

            accelCurveBle1 = accelPaneBle1.AddCurve(null, accelXBle1, Color.Red, SymbolType.None);
            accelCurveBle1 = accelPaneBle1.AddCurve(null, accelYBle1, Color.Blue, SymbolType.None);
            accelCurveBle1 = accelPaneBle1.AddCurve(null, accelZBle1, Color.Green, SymbolType.None);
            gyroCurveBle1 = gyroPaneBle1.AddCurve(null, gyroXBle1, Color.Red, SymbolType.None);
            gyroCurveBle1 = gyroPaneBle1.AddCurve(null, gyroYBle1, Color.Blue, SymbolType.None);
            gyroCurveBle1 = gyroPaneBle1.AddCurve(null, gyroZBle1, Color.Green, SymbolType.None);
            magnoCurveBle1 = magnoPaneBle1.AddCurve(null, magnoXBle1, Color.Red, SymbolType.None);
            magnoCurveBle1 = magnoPaneBle1.AddCurve(null, magnoYBle1, Color.Blue, SymbolType.None);
            magnoCurveBle1 = magnoPaneBle1.AddCurve(null, magnoZBle1, Color.Green, SymbolType.None);

            //Ble2 ZedGraph
            accelPaneBle2 = accelZedGraphBle2.GraphPane;
            gyroPaneBle2 = gyroZedGraphBle2.GraphPane;
            magnoPaneBle2 = magnoZedGraphBle2.GraphPane;

            accelPaneBle2.YAxis.Scale.Max = 1000;
            accelPaneBle2.YAxis.Scale.Min = -1000;
            gyroPaneBle2.YAxis.Scale.Max = 2000;
            gyroPaneBle2.YAxis.Scale.Min = -2000;
            magnoPaneBle2.YAxis.Scale.Max = 500;
            magnoPaneBle2.YAxis.Scale.Min = -500;

            accelCurveBle2 = accelPaneBle2.AddCurve(null, accelXBle2, Color.Red, SymbolType.None);
            accelCurveBle2 = accelPaneBle2.AddCurve(null, accelYBle2, Color.Blue, SymbolType.None);
            accelCurveBle2 = accelPaneBle2.AddCurve(null, accelZBle2, Color.Green, SymbolType.None);
            gyroCurveBle2 = gyroPaneBle2.AddCurve(null, gyroXBle2, Color.Red, SymbolType.None);
            gyroCurveBle2 = gyroPaneBle2.AddCurve(null, gyroYBle2, Color.Blue, SymbolType.None);
            gyroCurveBle2 = gyroPaneBle2.AddCurve(null, gyroZBle2, Color.Green, SymbolType.None);
            magnoCurveBle2 = magnoPaneBle2.AddCurve(null, magnoXBle2, Color.Red, SymbolType.None);
            magnoCurveBle2 = magnoPaneBle2.AddCurve(null, magnoYBle2, Color.Blue, SymbolType.None);
            magnoCurveBle2 = magnoPaneBle2.AddCurve(null, magnoZBle2, Color.Green, SymbolType.None);

            //Ble
            gyroPane = GyrozedGraph.GraphPane;

            gyroPane.YAxis.Scale.Max = 2000;
            gyroPane.YAxis.Scale.Min = -2000;

            gyroPane.Legend.Position = LegendPos.Top;
            gyroPane.Legend.IsVisible = true;

            gyroCurve = gyroPane.AddCurve("X", quaternion0, Color.Red, SymbolType.None);
            gyroCurve = gyroPane.AddCurve("Y", quaternion1, Color.Blue, SymbolType.None);
            gyroCurve = gyroPane.AddCurve("Z", quaternion2, Color.Green, SymbolType.None);
            gyroCurve = gyroPane.AddCurve("x", quaternion3, Color.Cyan, SymbolType.None);
            gyroCurve = gyroPane.AddCurve("y", quaternion4, Color.Yellow, SymbolType.None);
            gyroCurve = gyroPane.AddCurve(null, quaternion5, Color.Brown, SymbolType.None);

            //MagAxiss
            MagaxisPane = zedGraphMag.GraphPane;
            MagaxisPane.Title.IsVisible = false;
            gyroPane.YAxis.Scale.Max = 2000;
            gyroPane.YAxis.Scale.Min = -2000;

            MagaxisPane.XAxis.Title.FontSpec = new FontSpec("fdesf", 5, Color.Red, false, false, false);
            MagaxisPane.XAxis.Scale.FontSpec = new FontSpec("", 5, Color.Black, false, false, false);
            //MagaxisPane.XAxis.Scale.MajorStepAuto = true;

            MagaxisPane.YAxis.Title.FontSpec = new FontSpec("fdesf", 5, Color.Red, false, false, false);
            MagaxisPane.YAxis.Scale.FontSpec = new FontSpec("", 5, Color.Black, false, false, false);

            MagAxisCurve = MagaxisPane.AddCurve(null, MagXY, Color.Red, SymbolType.Circle);
            MagAxisCurve = MagaxisPane.AddCurve(null, MagYZ, Color.Blue, SymbolType.Diamond);
            MagAxisCurve = MagaxisPane.AddCurve(null, MagXZ, Color.Green, SymbolType.Plus);

            //MagCompas
            MagCompasPane = zedGraphCompaszasyon.GraphPane;
            MagCompasPane.Title.IsVisible = false;
            MagCompasPane.XAxis.Title.FontSpec = new FontSpec("fdesf", 5, Color.Red, false, false, false);
            MagCompasPane.XAxis.Scale.FontSpec = new FontSpec("", 5, Color.Black, false, false, false);
            MagCompasPane.YAxis.Title.FontSpec = new FontSpec("fdesf", 5, Color.Red, false, false, false);
            MagCompasPane.YAxis.Scale.FontSpec = new FontSpec("", 5, Color.Black, false, false, false);

            MagCompasCurve = MagCompasPane.AddCurve(null, MagCompasXY, Color.Red, SymbolType.Circle);
            MagCompasCurve = MagCompasPane.AddCurve(null, MagCompasYZ, Color.Blue, SymbolType.Diamond);
            MagCompasCurve = MagCompasPane.AddCurve(null, MagCompasXZ, Color.Green, SymbolType.Plus);
        }


        private void cb_SP_Port_Click(object sender, EventArgs e)
        {
            string st = cb_SP_Port.Text;
            cb_SP_Port.Items.Clear();
            cb_SP_Port.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            cb_SP_Port.Text = st;
        }

        private void cb_BlePort_Click(object sender, EventArgs e)
        {
            string st = cb_BlePort.Text;
            cb_BlePort.Items.Clear();
            cb_BlePort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            cb_BlePort.Text = st;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                if (0 <= cb_SP_Port.SelectedIndex)
                {
                    Settings.StaticSetting.SP_PortSetting.ComName = cb_SP_Port.Text;
                }

                if (0 <= cb_BlePort.SelectedIndex)
                {
                    Settings.StaticSetting.Ble_PortSetting.ComName = cb_BlePort.Text;
                }

                var res = Settings.SaveStaticSettings();
                if (res.Succes)
                {
                    MessageBox.Show("Ayarlar Kaydedildi.");
                    lbl_SPPort.Text = cb_SP_Port.Text;
                    lbl_BlePort.Text = cb_BlePort.Text;
                }
                else
                {
                    MessageBox.Show("Ayarlar Kaydedilemdi!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ayarlar Kaydedilemdi!\n" + ex.Message);
            }
        }

        //Modüle Seriport üzerinden bağlanır.
        private async void lbl_SPConnect_DoubleClick(object sender, EventArgs e)
        {
            Optoel.result rst = new Optoel.result();
            string com = Settings.StaticSetting.SP_PortSetting.ComName;

            if (serialPort.IsOpen)
            {
                rst = serialPort.Close();
                if (rst.Succes)
                {
                    lbl_SPConnect.Text = "Disconnect";
                    lbl_SPConnect.BackColor = Color.Red;
                }
            }
            else
            {
                rst = await serialPort.OpenAsync(com);
                if (rst.Succes == true)
                {
                    lbl_SPConnect.Text = "Connecting...";
                    lbl_SPConnect.BackColor = Color.DarkOrange;

                    serialPort.Connect();
                }
            }

            lbl_SPMessage.Text = serialPort.ConnectMessage;
        }

        //Dongle'ın seriportuna bağlanma
        private async void lbl_BleSPConnect_DoubleClick(object sender, EventArgs e)
        {
            Optoel.result rst = new Optoel.result();
            string com = Settings.StaticSetting.Ble_PortSetting.ComName;

            if (Ble.IsOpen)
            {
                rst = Ble.Close();
                if (rst.Succes == true)
                {
                    lbl_BleSPConnect.Text = "Disconnect";
                    lbl_BleSPConnect.BackColor = Color.Red;
                }
            }
            else
            {
                rst = await Ble.OpenAsync(com);
                if (rst.Succes == true)
                {
                    lbl_BleSPConnect.Text = "Connect";
                    lbl_BleSPConnect.BackColor = Color.Lime;
                }
            }

            lbl_DongleMessage.Text = Ble.ConnectMessage;
        }



        #region Ble Event Functions


        private void BleErrorEvent(Optoel.Ble.ErrorHandleArgs Error)
        {
            lbl_ErrorMsgBle1.Text = Error.ErrorMessage;
        }

        List<Optoel.Ble.Device> ListDevice;
        //Scan işlemi sonunda Optoel Cihazlarını döndüren event fonksiyonu
        private void BleDiscoverReslutEvent(List<Optoel.Ble.Device> e)
        {
            ListDevice = e;
            cb_BleDevices1.Items.Clear();
            cb_BleDevices2.Items.Clear();
            foreach (var item in e)
            {
                cb_BleDevices1.Items.Add(item.DeviceName + " (" + item.MAC_Address + ")");
                cb_BleDevices2.Items.Add(item.DeviceName + " (" + item.MAC_Address + ")");
                
            }
        }

        //Scan işlemi sırasında, her Optoel cihazı keşfedildiğinde çalışan event fonksiyonu
        private void BleDiscoverDeviceEvent(Optoel.Ble.Device e)
        {
            
        }

        
        List<Optoel.Ble.BleSlave> ListBleSlave = new List<Optoel.Ble.BleSlave>();
        Optoel.Ble.BleSlave bleSlave1;
        Optoel.Ble.BleSlave bleSlave2;

        //Bir Optoel cihazına bağlanıldığında çalışan event fonksiyonu.
        //'bleSlave' nesnesi, o cihazı kontrol eden bütün methodlara sahiptir.
        private void BleConnectResultEvent(Optoel.Ble.BleSlave bleSlave)
        {
            ListBleSlave.Add(bleSlave);
            
            if(ComboBox == 1)
            {
                bleSlave1 = bleSlave;
                bleSlave1.SlaveNumber = 0;
                Ble1Connected = true;
                btn_ConnectBle1.BackColor = Color.Lime;
                bleSlave1.QuaternionCalculateEnable = false;
                bleSlave1.QuaternionData += new Ble.BleSlave.QuaternionEventHandler(QuaternionDataEvent);

                EnabledComponentBle1();
            }
            else if( ComboBox == 2)
            {
                bleSlave2 = bleSlave;
                bleSlave2.SlaveNumber = 1;
                Ble2Connected = true;
                btn_ConnectBle2.BackColor = Color.Lime;

                EnabledComponentBle2();
                
            }
        }


        void EnabledComponentBle1()
        {
            if(Ble1Connected == true)
            {
                lbl_AccelEnableBle1.Enabled = true;
                trackBarAccelBwBle1.Enabled = true;
                trackBarAccelRangeBle1.Enabled = true;
                lbl_AccelReadEnableBle1.Enabled = true;
                btn_AccelApplyBle1.Enabled = true;

                lbl_GyroEnableBle1.Enabled = true;
                trackBarGyroBwBle1.Enabled = true;
                trackBarGyroRangeBle1.Enabled = true;
                lbl_GyroReadEnableBle1.Enabled = true;
                btn_GyroApplyBle1.Enabled = true;

                lbl_MagnoEnableBle1.Enabled = true;
                trackBarMagnoBwBle1.Enabled = true;
                lbl_MagnoReadEnableBle1.Enabled = true;
                btn_MagnoApplyBle1.Enabled = true;
            }
            else
            {
                lbl_AccelEnableBle1.Enabled = false;
                trackBarAccelBwBle1.Enabled = false;
                trackBarAccelRangeBle1.Enabled = false;
                lbl_AccelReadEnableBle1.Enabled = false;
                btn_AccelApplyBle1.Enabled = false;

                lbl_GyroEnableBle1.Enabled = false;
                trackBarGyroBwBle1.Enabled = false;
                trackBarGyroRangeBle1.Enabled = false;
                lbl_GyroReadEnableBle1.Enabled = false;
                btn_GyroApplyBle1.Enabled = false;

                lbl_MagnoEnableBle1.Enabled = false;
                trackBarMagnoBwBle1.Enabled = false;
                lbl_MagnoReadEnableBle1.Enabled = false;
                btn_MagnoApplyBle1.Enabled = false;
            } 
        }

        void EnabledComponentBle2()
        {
            if(Ble2Connected == true)
            {
                lbl_AccelEnableBle2.Enabled = true;
                trackBarAccelBwBle2.Enabled = true;
                trackBarAccelRangeBle2.Enabled = true;
                lbl_AccelReadEnableBle2.Enabled = true;
                btn_AccelApplyBle2.Enabled = true;

                lbl_GyroEnableBle2.Enabled = true;
                trackBarGyroBwBle2.Enabled = true;
                trackBarGyroRangeBle2.Enabled = true;
                lbl_GyroReadEnableBle2.Enabled = true;
                btn_GyroApplyBle2.Enabled = true;

                lbl_MagnoEnableBle2.Enabled = true;
                trackBarMagnoBwBle2.Enabled = true;
                lbl_MagnoReadEnableBle2.Enabled = true;
                btn_MagnoApplyBle2.Enabled = true;
            }
            else
            {
                lbl_AccelEnableBle2.Enabled = false;
                trackBarAccelBwBle2.Enabled = false;
                trackBarAccelRangeBle2.Enabled = false;
                lbl_AccelReadEnableBle2.Enabled = false;
                btn_AccelApplyBle2.Enabled = false;

                lbl_GyroEnableBle2.Enabled = false;
                trackBarGyroBwBle2.Enabled = false;
                trackBarGyroRangeBle2.Enabled = false;
                lbl_GyroReadEnableBle2.Enabled = false;
                btn_GyroApplyBle2.Enabled = false;

                lbl_MagnoEnableBle2.Enabled = false;
                trackBarMagnoBwBle2.Enabled = false;
                lbl_MagnoReadEnableBle2.Enabled = false;
                btn_MagnoApplyBle2.Enabled = false;
            }
            
        }



        #endregion Ble Event Functions








        #region SerialPort Event Functions
        
        bool DeviceConnectedSerial = false;

        private void ConnectResultResponse(Optoel.SerialPort.ConnecResult e)
        {
            lbl_SPConnect.Text = "Connect";
            lbl_SPConnect.BackColor = Color.Lime;
            
            if (e.AccelReady == true)
                lbl_AccelReady.BackColor = Color.Lime;

            if (e.GyroReady == true)
                lbl_GyroReady.BackColor = Color.Lime;

            if (e.MagnoReady == true)
                lbl_MagnoReady.BackColor = Color.Lime;

            DeviceConnectedSerial = true;
            EnabledComponent();
        }

        void EnabledComponent()
        {
            if (DeviceConnectedSerial == true)
            {
                lbl_AccelEnable.Enabled = true;
                trackBarAccelBw.Enabled = true;
                trackBarAccelRange.Enabled = true;
                lbl_AccelReadEnable.Enabled = true;
                btn_AccelApply.Enabled = true;

                lbl_GyroEnable.Enabled = true;
                trackBarGyroBw.Enabled = true;
                trackBarGyroRange.Enabled = true;
                lbl_GyroReadEnable.Enabled = true;
                btn_GyroApply.Enabled = true;

                lbl_MagnoEnable.Enabled = true;
                trackBarMagnoBw.Enabled = true;
                lbl_MagnoReadEnable.Enabled = true;
                btn_MagnoApply.Enabled = true;
            }
            else
            {
                lbl_AccelEnable.Enabled = false;
                trackBarAccelBw.Enabled = false;
                trackBarAccelRange.Enabled = false;
                lbl_AccelReadEnable.Enabled = false;
                btn_AccelApply.Enabled = false;

                lbl_GyroEnable.Enabled = false;
                trackBarGyroBw.Enabled = false;
                trackBarGyroRange.Enabled = false;
                lbl_GyroReadEnable.Enabled = false;
                btn_GyroApply.Enabled = false;

                lbl_MagnoEnable.Enabled = false;
                trackBarMagnoBw.Enabled = false;
                lbl_MagnoReadEnable.Enabled = false;
                btn_MagnoApply.Enabled = false;
            }
        }


        private void serialPortEnableResultEvent(Optoel.SerialPort.EnableResultArgs e)
        {
            switch (e.Sensor)
            {
                case BMX055.Sensor.Accel:
                    if (e.Enable == BMX055.Enable.Enable)
                    {
                        AccelEnable = true;
                        lbl_AccelEnable.Text = "Enable";
                        lbl_AccelEnable.BackColor = Color.Lime;
                    }
                    else if (e.Enable == BMX055.Enable.Disable)
                    {
                        AccelEnable = false;
                        lbl_AccelEnable.Text = "Disable";
                        lbl_AccelEnable.BackColor = Color.Red;
                    }
                    break;
                case BMX055.Sensor.Gyro:
                    if (e.Enable == BMX055.Enable.Enable)
                    {
                        GyroEnable = true;
                        lbl_GyroEnable.Text = "Enable";
                        lbl_GyroEnable.BackColor = Color.Lime;
                    }
                    else if (e.Enable == BMX055.Enable.Disable)
                    {
                        GyroEnable = false;
                        lbl_GyroEnable.Text = "Disable";
                        lbl_GyroEnable.BackColor = Color.Red;
                    }
                    break;
                case BMX055.Sensor.Magno:
                    if (e.Enable == BMX055.Enable.Enable)
                    {
                        MagnoEnable = true;
                        lbl_MagnoEnable.Text = "Enable";
                        lbl_MagnoEnable.BackColor = Color.Lime;
                    }
                    else if (e.Enable == BMX055.Enable.Disable)
                    {
                        MagnoEnable = false;
                        lbl_MagnoEnable.Text = "Disable";
                        lbl_MagnoEnable.BackColor = Color.Red;
                    }
                    break;
            }
        }

        private void serialPortBwResultEvent(Optoel.SerialPort.BwResultArgs e)
        {
            switch (e.Sensor)
            {
                case BMX055.Sensor.Accel:
                    if (e.Success == true)
                        AccelBw = trackBarAccelBw.Value;
                    else
                        trackBarAccelBw.Value = AccelBw;
                break;
                case BMX055.Sensor.Gyro:
                    if (e.Success == true)
                        GyroBw = trackBarGyroBw.Value;
                    else
                        trackBarGyroBw.Value = GyroBw;
                break;
                case BMX055.Sensor.Magno:
                    if (e.Success == true)
                        MagnoBw = trackBarMagnoBw.Value;
                    else
                        trackBarMagnoBw.Value = MagnoBw;
                break;
            }
        }

        private void serialPortRangeResultEvent(Optoel.SerialPort.RangeResultArgs e)
        {
            switch (e.Sensor)
            {
                case BMX055.Sensor.Accel:
                    if (e.Success == true)
                        AccelRange = trackBarAccelRange.Value;
                    else
                        trackBarAccelRange.Value = AccelRange;
                    break;
                case BMX055.Sensor.Gyro:
                    if (e.Success == true)
                        GyroRange = trackBarGyroRange.Value;
                    else
                        trackBarGyroRange.Value = GyroRange;
                    break;
            }
        }

        private void serialPortReadEnableResultEvent(Optoel.SerialPort.ReadEnableResultArgs e)
        {
            switch (e.Sensor)
            {
                case BMX055.Sensor.Accel:
                    if (e.Enable == BMX055.Enable.Enable)
                    {
                        lbl_AccelReadEnable.Text = "Enable";
                        lbl_AccelReadEnable.BackColor = Color.Lime;
                        AccelReadEnable = true;
                    }
                    else if (e.Enable == BMX055.Enable.Disable)
                    {
                        lbl_AccelReadEnable.Text = "Disable";
                        lbl_AccelReadEnable.BackColor = Color.Red;
                        AccelReadEnable = false;
                    }
                    break;
                case BMX055.Sensor.Gyro:
                    if (e.Enable == BMX055.Enable.Enable)
                    {
                        lbl_GyroReadEnable.Text = "Enable";
                        lbl_GyroReadEnable.BackColor = Color.Lime;
                        GyroReadEnable = true;
                    }
                    else if (e.Enable == BMX055.Enable.Disable)
                    {
                        lbl_GyroReadEnable.Text = "Disable";
                        lbl_GyroReadEnable.BackColor = Color.Red;
                        GyroReadEnable = false;
                    }
                    break;
                case BMX055.Sensor.Magno:
                    if (e.Enable == BMX055.Enable.Enable)
                    {
                        lbl_MagnoReadEnable.Text = "Enable";
                        lbl_MagnoReadEnable.BackColor = Color.Lime;
                        MagnoReadEnable = true;
                    }
                    else if (e.Enable == BMX055.Enable.Disable)
                    {
                        lbl_MagnoReadEnable.Text = "Disable";
                        lbl_MagnoReadEnable.BackColor = Color.Red;
                        MagnoReadEnable = false;
                    }
                    break;
            }
        }




        //private void AccelEnableResultResponse(Optoel.SerialPort.AccelEnableResult e)
        //{
        //    if (e.Enable == Optoel.BMX055.Enable.Enable)
        //    {
        //        AccelEnable = true;
        //        lbl_AccelEnable.Text = "Enable";
        //        lbl_AccelEnable.BackColor = Color.Lime;
        //    }
        //    else if (e.Enable == Optoel.BMX055.Enable.Disable)
        //    {
        //        AccelEnable = false;
        //        lbl_AccelEnable.Text = "Disable";
        //        lbl_AccelEnable.BackColor = Color.Red;
        //    }
        //}
        //private void GyroEnableResultResponse(Optoel.SerialPort.GyroEnableResult e)
        //{
        //    if (e.Enable == Optoel.BMX055.Enable.Enable)
        //    {
        //        GyroEnable = true;
        //        lbl_GyroEnable.Text = "Enable";
        //        lbl_GyroEnable.BackColor = Color.Lime;
        //    }
        //    else if (e.Enable == Optoel.BMX055.Enable.Disable)
        //    {
        //        GyroEnable = false;
        //        lbl_GyroEnable.Text = "Disable";
        //        lbl_GyroEnable.BackColor = Color.Red;
        //    }
        //}
        //private void MagnoEnableResultResponse(Optoel.SerialPort.MagnoEnableResult e)
        //{
        //    if (e.Enable == Optoel.BMX055.Enable.Enable)
        //    {
        //        MagnoEnable = true;
        //        lbl_MagnoEnable.Text = "Enable";
        //        lbl_MagnoEnable.BackColor = Color.Lime;
        //    }
        //    else if (e.Enable == Optoel.BMX055.Enable.Disable)
        //    {
        //        MagnoEnable = false;
        //        lbl_MagnoEnable.Text = "Disable";
        //        lbl_MagnoEnable.BackColor = Color.Red;
        //    }
        //}

        //private void AccelBwResultResponse(Optoel.SerialPort.AccelBwResult e)
        //{
        //    if (e.Success == true)
        //    {
        //        AccelBw = trackBarAccelBw.Value;
        //    }
        //    else
        //    {
        //        trackBarAccelBw.Value = AccelBw;
        //    }
        //}
        //private void GyroBwResultResponse(Optoel.SerialPort.GyroBwResult e)
        //{
        //    if (e.Success == true)
        //    {
        //        GyroBw = trackBarGyroBw.Value;
        //    }
        //    else
        //    {
        //        trackBarGyroBw.Value = GyroBw;
        //    }
        //}
        //private void MagnoBwResultResponse(Optoel.SerialPort.MagnoBwResult e)
        //{
        //    if (e.Success == true)
        //    {
        //        MagnoBw = trackBarMagnoBw.Value;
        //    }
        //    else
        //    {
        //        trackBarMagnoBw.Value = MagnoBw;
        //    }
        //}

        //private void AccelRangeResultResponse(Optoel.SerialPort.AccelRangeResult e)
        //{
        //    if (e.Success == true)
        //    {
        //        AccelRange = trackBarAccelRange.Value;
        //    }
        //    else
        //    {
        //        trackBarAccelRange.Value = AccelRange;
        //    }
        //}
        //private void GyroRangeResultResponse(Optoel.SerialPort.GyroRangeResult e)
        //{
        //    if (e.Success == true)
        //    {
        //        GyroRange = trackBarGyroRange.Value;
        //    }
        //    else
        //    {
        //        trackBarGyroRange.Value = GyroRange;
        //    }
        //}

        //private void AccelReadEnableResultResponse(Optoel.SerialPort.AccelReadEnableResult e)
        //{
        //    if (e.Enable == Optoel.BMX055.Enable.Enable)
        //    {
        //        lbl_AccelReadEnable.Text = "Enable";
        //        lbl_AccelReadEnable.BackColor = Color.Lime;
        //        AccelReadEnable = true;
        //    }
        //    else if (e.Enable == Optoel.BMX055.Enable.Disable)
        //    {
        //        lbl_AccelReadEnable.Text = "Disable";
        //        lbl_AccelReadEnable.BackColor = Color.Red;
        //        AccelReadEnable = false;
        //    }
        //}
        //private void GyroReadEnableResultResponse(Optoel.SerialPort.GyroReadEnableResult e)
        //{
        //    if (e.Enable == Optoel.BMX055.Enable.Enable)
        //    {
        //        lbl_GyroReadEnable.Text = "Enable";
        //        lbl_GyroReadEnable.BackColor = Color.Lime;
        //        GyroReadEnable = true;
        //    }
        //    else if (e.Enable == Optoel.BMX055.Enable.Disable)
        //    {
        //        lbl_GyroReadEnable.Text = "Disable";
        //        lbl_GyroReadEnable.BackColor = Color.Red;
        //        GyroReadEnable = false;
        //    }
        //}
        //private void MagnoReadEnableResultResponse(Optoel.SerialPort.MagnoReadEnableResult e)
        //{
        //    if (e.Enable == Optoel.BMX055.Enable.Enable)
        //    {
        //        lbl_MagnoReadEnable.Text = "Enable";
        //        lbl_MagnoReadEnable.BackColor = Color.Lime;
        //        MagnoReadEnable = true;
        //    }
        //    else if (e.Enable == Optoel.BMX055.Enable.Disable)
        //    {
        //        lbl_MagnoReadEnable.Text = "Disable";
        //        lbl_MagnoReadEnable.BackColor = Color.Red;
        //        MagnoReadEnable = false;
        //    }
        //}


        public long minaccel = 0, maxaccel = 250;
        private void AccelDataEvent(Optoel.SerialPort.Accel Accel)
        {
            label25.Text = serialPort.AccelKacanDataSayisi.ToString();
            accel = Accel;

            swAllData.WriteLine(
                    accel.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                    accel.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                    accel.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

                    gyro.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                    gyro.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                    gyro.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

                    magno.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                    magno.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                    magno.RawZ.ToString(new CultureInfo("en-US", false))
                );
            swAllData.Flush();

            if (Accel.AccelDataNumber % 5 == 0)
            {
                accelXSP.Add(new ZedGraph.PointPair(maxaccel, Accel.RawX));
                accelYSP.Add(new ZedGraph.PointPair(maxaccel, Accel.RawY));
                accelZSP.Add(new ZedGraph.PointPair(maxaccel, Accel.RawZ));
                accelPaneSP.XAxis.Scale.Max = maxaccel;
                accelPaneSP.XAxis.Scale.Min = minaccel;

                accelZedGraf.Invalidate();
                accelPaneSP.AxisChange();
                //accelZedGraf.Refresh();
                maxaccel++;
                minaccel++;

                
            }
        }
        
        public long mingyro = 0, maxgyro = 250;
        private void GyroDataEvent(Optoel.SerialPort.Gyro Gyro)
        {
            label26.Text = serialPort.GyroKacanDataSayisi.ToString();
            gyro = Gyro;

            if (Gyro.GyroDataNumber % 5 == 0)
            {
                gyroXSP.Add(new ZedGraph.PointPair(maxgyro, Gyro.RawX));
                gyroYSP.Add(new ZedGraph.PointPair(maxgyro, Gyro.RawY));
                gyroZSP.Add(new ZedGraph.PointPair(maxgyro, Gyro.RawZ));
                gyroPaneSP.XAxis.Scale.Max = maxgyro;
                gyroPaneSP.XAxis.Scale.Min = mingyro;

                gyroZedGraf.Invalidate();
                gyroPaneSP.AxisChange();
                //gyroZedGraf.Refresh();
                maxgyro++;
                mingyro++;
            }
        }

        public long minmagno = 0, maxmagno = 250;
        private void MagnoDataEvent(Optoel.SerialPort.Magno Magno)
        {
            magno = Magno;

            label28.Text = serialPort.MagnoKacanDataSayisi.ToString();
            magnoXSP.Add(new ZedGraph.PointPair(maxmagno, Magno.RawX));
            magnoYSP.Add(new ZedGraph.PointPair(maxmagno, Magno.RawY));
            magnoZSP.Add(new ZedGraph.PointPair(maxmagno, Magno.RawZ));
            magnoPaneSP.XAxis.Scale.Max = maxmagno;
            magnoPaneSP.XAxis.Scale.Min = minmagno;

            magnoZedGraf.Invalidate();
            magnoPaneSP.AxisChange();
            //magnoZedGraf.Refresh();
            maxmagno++;
            minmagno++;
        }

        #endregion Serialport Event Functions





        #region Ble

        int ComboBox = 0;


        //------------ Scan işlemleri (begin) --------------
        private void btn_BleScanStart_Click(object sender, EventArgs e)
        {
            Ble.DiscoverStart(2);
        }

        private void btn_BleScanStop_Click(object sender, EventArgs e)
        {
            Ble.DiscoverStop();
        }

        //------------ Scan işlemleri (end) ----------------



        #region Ble1

        bool Ble1Connected = false;
        private void btn_ConnectBle1_Click(object sender, EventArgs e)
        {
            if (ListDevice.Count > 0)
            {
                if (cb_BleDevices1.Items.Count > 0)
                {
                    if (Ble1Connected == false)
                    {
                        Ble.ConnectToDevice(ListDevice[cb_BleDevices1.SelectedIndex]);
                        ComboBox = 1;
                    }
                }
            }
        }

        private void btn_DisconnectBle1_Click(object sender, EventArgs e)
        {
            if(Ble1Connected == true)
            {
                bleSlave1.BleDisconnect();
                Thread.Sleep(2);
                
                if(bleSlave1.Connected == false)
                {
                    Ble1Connected = false;
                }      
            }
        }


        #region TrackBars
        public int AccelBwBle1 = 3;
        Optoel.BMX055.AccelBw accelBwBle1;
        private void trackBarAccelBwBle1_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarAccelBwBle1.Value)
            {
                case 0:
                    accelBwBle1 = Optoel.BMX055.AccelBw.AccelBw_15f625;
                    lbl_AccelBwBle1.Text = "15.625 Hz (64 ms)";
                    break;
                case 1:
                    accelBwBle1 = Optoel.BMX055.AccelBw.AccelBw_31f25;
                    lbl_AccelBwBle1.Text = "31.25 Hz (32 ms)";
                    break;
                case 2:
                    accelBwBle1 = Optoel.BMX055.AccelBw.AccelBw_62f5;
                    lbl_AccelBwBle1.Text = "62.5 Hz (16 ms)";
                    break;
                case 3:
                    accelBwBle1 = Optoel.BMX055.AccelBw.AccelBw_125;
                    lbl_AccelBwBle1.Text = "125 Hz (8 ms)";
                    break;
                case 4:
                    accelBwBle1 = Optoel.BMX055.AccelBw.AccelBw_250;
                    lbl_AccelBwBle1.Text = "250 Hz (4 ms)";
                    break;
            }
        }

        int AccelRangeBle1 = 0;
        Optoel.BMX055.AccelRange accelRangeBle1;
        private void trackBarAccelRangeBle1_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarAccelRangeBle1.Value)
            {
                case 0:
                    accelRangeBle1 = Optoel.BMX055.AccelRange.AccelRange_2g;
                    lbl_AccelRangeBle1.Text = "2g";
                    break;
                case 1:
                    accelRangeBle1 = Optoel.BMX055.AccelRange.AccelRange_4g;
                    lbl_AccelRangeBle1.Text = "4g";
                    break;
                case 2:
                    accelRangeBle1 = Optoel.BMX055.AccelRange.AccelRange_8g;
                    lbl_AccelRangeBle1.Text = "8g";
                    break;
                case 3:
                    accelRangeBle1 = Optoel.BMX055.AccelRange.AccelRange_16g;
                    lbl_AccelRangeBle1.Text = "16g";
                    break;
            }
        }


        public int GyroBwBle1 = 0;
        Optoel.BMX055.GyroBw gyroBwBle1;
        private void trackBarGyroBwBle1_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarGyroBwBle1.Value)
            {
                case 0:
                    gyroBwBle1 = Optoel.BMX055.GyroBw.GyroBw_100Hz;
                    lbl_GyroBwBle1.Text = "100 Hz";
                    break;
                case 1:
                    gyroBwBle1 = Optoel.BMX055.GyroBw.GyroBw_200Hz;
                    lbl_GyroBwBle1.Text = "200 Hz";
                    break;
            }
        }

        int GyroRangeBle1 = 4;
        Optoel.BMX055.GyroRange gyroRangeBle1;
        private void trackBarGyroRangeBle1_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarGyroRangeBle1.Value)
            {
                case 0:
                    gyroRangeBle1 = Optoel.BMX055.GyroRange.GyroRange_125s;
                    lbl_GyroRangeBle1.Text = "125 °/s";
                    break;
                case 1:
                    gyroRangeBle1 = Optoel.BMX055.GyroRange.GyroRange_250s;
                    lbl_GyroRangeBle1.Text = "250 °/s";
                    break;
                case 2:
                    gyroRangeBle1 = Optoel.BMX055.GyroRange.GyroRange_500s;
                    lbl_GyroRangeBle1.Text = "500 °/s";
                    break;
                case 3:
                    gyroRangeBle1 = Optoel.BMX055.GyroRange.GyroRange_1000s;
                    lbl_GyroRangeBle1.Text = "1000 °/s";
                    break;
                case 4:
                    gyroRangeBle1 = Optoel.BMX055.GyroRange.GyroRange_2000s;
                    lbl_GyroRangeBle1.Text = "2000 °/s";
                    break;
            }
        }


        public int MagnoBwBle1 = 7;
        Optoel.BMX055.MagnoBw magnoBwBle1;
        private void trackBarMagnoBwBle1_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarMagnoBwBle1.Value)
            {
                case 0:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_2Hz;
                    lbl_MagnoBwBle1.Text = "2 Hz";
                    break;
                case 1:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_6Hz;
                    lbl_MagnoBwBle1.Text = "6 Hz";
                    break;
                case 2:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_8Hz;
                    lbl_MagnoBwBle1.Text = "8 Hz";
                    break;
                case 3:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_10Hz;
                    lbl_MagnoBwBle1.Text = "10 Hz";
                    break;
                case 4:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_15Hz;
                    lbl_MagnoBwBle1.Text = "15 Hz";
                    break;
                case 5:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_20Hz;
                    lbl_MagnoBwBle1.Text = "20 Hz";
                    break;
                case 6:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_25Hz;
                    lbl_MagnoBwBle1.Text = "25 Hz";
                    break;
                case 7:
                    magnoBwBle1 = Optoel.BMX055.MagnoBw.MagnoBw_30Hz;
                    lbl_MagnoBwBle1.Text = "30 Hz";
                    break;
            }
        }
        #endregion TrackBars


        #region Enable/Disable
        public bool AccelEnableBle1 = false;
        private void lbl_AccelEnableBle1_DoubleClick(object sender, EventArgs e)
        {
            if (AccelEnableBle1 == false)
            {
                //ListBleSlave[0].AccelEnable(Optoel.Optoel.Enable.Enable);
                ListBleSlave[0].SetEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Enable);
            }
            else
            {
                //ListBleSlave[0].AccelEnable(Optoel.Optoel.Enable.Disable);
                ListBleSlave[0].SetEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Disable);
            }
        }


        public bool GyroEnableBle1 = false;
        private void lbl_GyroEnableBle1_DoubleClick(object sender, EventArgs e)
        {
            if (GyroEnableBle1 == false)
            {
                //ListBleSlave[0].GyroEnable(Optoel.Optoel.Enable.Enable);
                ListBleSlave[0].SetEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Enable);
            }
            else
            {
                //ListBleSlave[0].GyroEnable(Optoel.Optoel.Enable.Disable);
                ListBleSlave[0].SetEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Enable);
            }
        }


        public bool MagnoEnableBle1 = false;
        private void lbl_MagnoEnableBle1_DoubleClick(object sender, EventArgs e)
        {
            if (MagnoEnableBle1 == false)
            {
                //ListBleSlave[0].MagnoEnable(Optoel.Optoel.Enable.Enable);
                ListBleSlave[0].SetEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Enable);
            }
            else
            {
                //ListBleSlave[0].MagnoEnable(Optoel.Optoel.Enable.Disable);
                ListBleSlave[0].SetEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Enable);
            }
        }
        #endregion Enable/Disable


        #region Apply

        private void btn_AccelApplyBle1_Click(object sender, EventArgs e)
        {
            if (AccelBwBle1 != trackBarAccelBwBle1.Value)
            {
                //ListBleSlave[0].AccelSetBw(accelBwBle1);
                ListBleSlave[0].SetBw(Optoel.BMX055.Sensor.Accel, accelBwBle1);
            }

            if (AccelRangeBle1 != trackBarAccelRangeBle1.Value)
            {
                //ListBleSlave[0].AccelSetRange(accelRangeBle1);
                ListBleSlave[0].SetRange(Optoel.BMX055.Sensor.Gyro, accelRangeBle1);
            }
        }

        private void btn_GyroApplyble1_Click(object sender, EventArgs e)
        {
            if (GyroBwBle1 != trackBarGyroBwBle1.Value)
            {
                //ListBleSlave[0].GyroSetBw(gyroBwBle1);
                ListBleSlave[0].SetBw(Optoel.BMX055.Sensor.Gyro, gyroBwBle1);
            }

            if (GyroRangeBle1 != trackBarGyroRangeBle1.Value)
            {
                //ListBleSlave[0].GyroSetRange(gyroRangeBle1);
                ListBleSlave[0].SetRange(Optoel.BMX055.Sensor.Gyro, gyroRangeBle1);
            }
        }

        private void btn_MagnoApplyBle1_Click(object sender, EventArgs e)
        {
            if (MagnoBwBle1 != trackBarMagnoBwBle1.Value)
            {
                //ListBleSlave[0].MagnoSetBw(magnoBwBle1);
                ListBleSlave[0].SetRange(Optoel.BMX055.Sensor.Gyro, magnoBwBle1);
            }
        }

        #endregion Apply


        #region Read Enable

        bool StartBle1 = false;

        bool AccelReadEnableBle1 = false;
        Optoel.BMX055.Enable accelReadEnableStartingBle1 = Optoel.BMX055.Enable.Disable;
        private void lbl_AccelReadEnableBle1_DoubleClick(object sender, EventArgs e)
        {
            if (StartBle1 == false)
            {
                if (accelReadEnableStartingBle1 == Optoel.BMX055.Enable.Disable)
                {
                    accelReadEnableStartingBle1 = Optoel.BMX055.Enable.Enable;
                    lbl_AccelReadEnableBle1.Text = "Enable";
                    lbl_AccelReadEnableBle1.BackColor = Color.Lime;
                    AccelReadEnableBle1 = true;
                }
                else if (accelReadEnableStartingBle1 == Optoel.BMX055.Enable.Enable)
                {
                    accelReadEnableStartingBle1 = Optoel.BMX055.Enable.Disable;
                    lbl_AccelReadEnableBle1.Text = "Disable";
                    lbl_AccelReadEnableBle1.BackColor = Color.Red;
                    AccelReadEnableBle1 = false;
                }
            }
            else
            {
                if (AccelReadEnableBle1 == false)
                {
                    //ListBleSlave[0].AccelReadEnable(Optoel.Optoel.Enable.Enable);
                    ListBleSlave[0].SetReadEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Enable);
                }
                else if (AccelReadEnableBle1 == true)
                {
                    //ListBleSlave[0].AccelReadEnable(Optoel.Optoel.Enable.Disable);
                    ListBleSlave[0].SetReadEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Disable);
                }
            }
        }


        bool GyroReadEnableBle1 = false;
        Optoel.BMX055.Enable gyroReadEnableStartingBle1 = Optoel.BMX055.Enable.Disable;
        private void lbl_GyroReadEnableBle1_DoubleClick(object sender, EventArgs e)
        {
            if (StartBle1 == false)
            {
                if (gyroReadEnableStartingBle1 == Optoel.BMX055.Enable.Disable)
                {
                    gyroReadEnableStartingBle1 = Optoel.BMX055.Enable.Enable;
                    lbl_GyroReadEnableBle1.Text = "Enable";
                    lbl_GyroReadEnableBle1.BackColor = Color.Lime;
                    GyroReadEnableBle1 = true;
                }
                else if (gyroReadEnableStartingBle1 == Optoel.BMX055.Enable.Enable)
                {
                    gyroReadEnableStartingBle1 = Optoel.BMX055.Enable.Disable;
                    lbl_GyroReadEnableBle1.Text = "Disable";
                    lbl_GyroReadEnableBle1.BackColor = Color.Red;
                    GyroReadEnableBle1 = false;
                }
            }
            else
            {
                if (GyroReadEnableBle1 == false)
                {
                    //ListBleSlave[0].GyroReadEnable(Optoel.BMX055.Enable.Enable);
                    ListBleSlave[0].SetReadEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Enable);
                }
                else if (GyroReadEnableBle1 == true)
                {
                    //ListBleSlave[0].GyroReadEnable(Optoel.BMX055.Enable.Disable);
                    ListBleSlave[0].SetReadEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Disable);
                }
            }
        }


        bool MagnoReadEnableBle1 = false;
        Optoel.BMX055.Enable magnoReadEnableStartingBle1 = Optoel.BMX055.Enable.Disable;
        private void lbl_MagnoReadEnableBle1_DoubleClick(object sender, EventArgs e)
        {
            if (StartBle1 == false)
            {
                if (magnoReadEnableStartingBle1 == Optoel.BMX055.Enable.Disable)
                {
                    magnoReadEnableStartingBle1 = Optoel.BMX055.Enable.Enable;
                    lbl_MagnoReadEnableBle1.Text = "Enable";
                    lbl_MagnoReadEnableBle1.BackColor = Color.Lime;
                    MagnoReadEnableBle1 = true;
                }
                else if (magnoReadEnableStartingBle1 == Optoel.BMX055.Enable.Enable)
                {
                    magnoReadEnableStartingBle1 = Optoel.BMX055.Enable.Disable;
                    lbl_MagnoReadEnableBle1.Text = "Disable";
                    lbl_MagnoReadEnableBle1.BackColor = Color.Red;
                    MagnoReadEnableBle1 = false;
                }
            }
            else
            {
                if (MagnoReadEnableBle1 == false)
                {
                    ListBleSlave[0].SetReadEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Enable);
                }
                else if (MagnoReadEnableBle1 == true)
                {
                    //ListBleSlave[0].MagnoReadEnable(Optoel.BMX055.Enable.Disable);
                    ListBleSlave[0].SetReadEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Disable);
                }
            }
        }

        #endregion Read Enable


        private void btn_StartBle1_Click(object sender, EventArgs e)
        {
            ListBleSlave[0].Start(accelReadEnableStartingBle1, gyroReadEnableStartingBle1, magnoReadEnableStartingBle1);
            StartBle1 = true;
        }

        private void btn_StopBle1_Click(object sender, EventArgs e)
        {
            ListBleSlave[0].Stop();
            StartBle1 = false;
        }


        #endregion Ble1



        #region Ble2

        bool Ble2Connected = false;
        private void btn_ConnectBle2_Click(object sender, EventArgs e)
        {
            if (ListDevice.Count > 0)
            {
                if(cb_BleDevices2.Items.Count > 0)
                {
                    if(Ble2Connected == false)
                    {
                        Ble.ConnectToDevice(ListDevice[cb_BleDevices2.SelectedIndex]);
                        ComboBox = 2;
                    }
                }    
            }
        }

        private void btn_DisconnectBle2_Click(object sender, EventArgs e)
        {
            if (Ble2Connected == true)
            {
                bleSlave2.BleDisconnect();
                Thread.Sleep(2);

                if (bleSlave2.Connected == false)
                {
                    Ble2Connected = false;
                }
            }
        }



        #region TrackBars

        public int AccelBwBle2 = 3;
        Optoel.BMX055.AccelBw accelBwBle2;
        private void trackBarAccelBwBle2_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarAccelBwBle2.Value)
            {
                case 0:
                    accelBwBle2 = Optoel.BMX055.AccelBw.AccelBw_15f625;
                    lbl_AccelBwBle2.Text = "15.625 Hz (64 ms)";
                    break;
                case 1:
                    accelBwBle2 = Optoel.BMX055.AccelBw.AccelBw_31f25;
                    lbl_AccelBwBle2.Text = "31.25 Hz (32 ms)";
                    break;
                case 2:
                    accelBwBle2 = Optoel.BMX055.AccelBw.AccelBw_62f5;
                    lbl_AccelBwBle2.Text = "62.5 Hz (16 ms)";
                    break;
                case 3:
                    accelBwBle2 = Optoel.BMX055.AccelBw.AccelBw_125;
                    lbl_AccelBwBle2.Text = "125 Hz (8 ms)";
                    break;
                case 4:
                    accelBwBle2 = Optoel.BMX055.AccelBw.AccelBw_250;
                    lbl_AccelBwBle2.Text = "250 Hz (4 ms)";
                    break;
            }
        }


        int AccelRangeBle2 = 0;
        Optoel.BMX055.AccelRange accelRangeBle2;
        private void trackBarAccelRangeBle2_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarAccelRangeBle2.Value)
            {
                case 0:
                    accelRangeBle2 = Optoel.BMX055.AccelRange.AccelRange_2g;
                    lbl_AccelRangeBle2.Text = "2g";
                    break;
                case 1:
                    accelRangeBle2 = Optoel.BMX055.AccelRange.AccelRange_4g;
                    lbl_AccelRangeBle2.Text = "4g";
                    break;
                case 2:
                    accelRangeBle2 = Optoel.BMX055.AccelRange.AccelRange_8g;
                    lbl_AccelRangeBle2.Text = "8g";
                    break;
                case 3:
                    accelRangeBle2 = Optoel.BMX055.AccelRange.AccelRange_16g;
                    lbl_AccelRangeBle2.Text = "16g";
                    break;
            }
        }


        public int GyroBwBle2 = 0;
        Optoel.BMX055.GyroBw gyroBwBle2;
        private void trackBarGyroBwBle2_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarGyroBwBle2.Value)
            {
                case 0:
                    gyroBwBle2 = Optoel.BMX055.GyroBw.GyroBw_100Hz;
                    lbl_GyroBwBle2.Text = "100 Hz";
                    break;
                case 1:
                    gyroBwBle2 = Optoel.BMX055.GyroBw.GyroBw_200Hz;
                    lbl_GyroBwBle2.Text = "200 Hz";
                    break;
            }
        }


        int GyroRangeBle2 = 4;
        Optoel.BMX055.GyroRange gyroRangeBle2;
        private void trackBarGyroRangeBle2_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarGyroRangeBle2.Value)
            {
                case 0:
                    gyroRangeBle2 = Optoel.BMX055.GyroRange.GyroRange_125s;
                    lbl_GyroRangeBle2.Text = "125 °/s";
                    break;
                case 1:
                    gyroRangeBle2 = Optoel.BMX055.GyroRange.GyroRange_250s;
                    lbl_GyroRangeBle2.Text = "250 °/s";
                    break;
                case 2:
                    gyroRangeBle2 = Optoel.BMX055.GyroRange.GyroRange_500s;
                    lbl_GyroRangeBle2.Text = "500 °/s";
                    break;
                case 3:
                    gyroRangeBle2 = Optoel.BMX055.GyroRange.GyroRange_1000s;
                    lbl_GyroRangeBle2.Text = "1000 °/s";
                    break;
                case 4:
                    gyroRangeBle2 = Optoel.BMX055.GyroRange.GyroRange_2000s;
                    lbl_GyroRangeBle2.Text = "2000 °/s";
                    break;
            }
        }


        public int MagnoBwBle2 = 7;
        Optoel.BMX055.MagnoBw magnoBwBle2;
        private void trackBarMagnoBwBle2_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarMagnoBwBle2.Value)
            {
                case 0:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_2Hz;
                    lbl_MagnoBwBle2.Text = "2 Hz";
                    break;
                case 1:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_6Hz;
                    lbl_MagnoBwBle2.Text = "6 Hz";
                    break;
                case 2:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_8Hz;
                    lbl_MagnoBwBle2.Text = "8 Hz";
                    break;
                case 3:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_10Hz;
                    lbl_MagnoBwBle2.Text = "10 Hz";
                    break;
                case 4:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_15Hz;
                    lbl_MagnoBwBle2.Text = "15 Hz";
                    break;
                case 5:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_20Hz;
                    lbl_MagnoBwBle2.Text = "20 Hz";
                    break;
                case 6:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_25Hz;
                    lbl_MagnoBwBle2.Text = "25 Hz";
                    break;
                case 7:
                    magnoBwBle2 = Optoel.BMX055.MagnoBw.MagnoBw_30Hz;
                    lbl_MagnoBwBle2.Text = "30 Hz";
                    break;
            }
        }

        #endregion TrackBars


        #region Enable/Disable

        public bool AccelEnableBle2 = false;
        private void lbl_AccelEnableBle2_DoubleClick(object sender, EventArgs e)
        {
            if (AccelEnableBle2 == false)
            {
                //bleSlave2.AccelEnable(Optoel.Optoel.Enable.Enable);
                bleSlave2.SetEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Enable);
            }
            else
            {
                //bleSlave2.AccelEnable(Optoel.Optoel.Enable.Disable);
                bleSlave2.SetEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Disable);
            }
        }


        public bool GyroEnableBle2 = false;
        private void lbl_GyroEnableBle2_DoubleClick(object sender, EventArgs e)
        {
            if (GyroEnableBle2 == false)
            {
                //bleSlave2.GyroEnable(Optoel.Optoel.Enable.Enable);
                bleSlave2.SetEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Enable);
            }
            else
            {
                //bleSlave2.GyroEnable(Optoel.Optoel.Enable.Disable);
                bleSlave2.SetEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Disable);
            }
        }


        public bool MagnoEnableBle2 = false;
        private void lbl_MagnoEnableBle2_DoubleClick(object sender, EventArgs e)
        {
            if (MagnoEnableBle2 == false)
            {
                //bleSlave2.MagnoEnable(Optoel.Optoel.Enable.Enable);
                bleSlave2.SetEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Enable);
            }
            else
            {
                //bleSlave2.MagnoEnable(Optoel.Optoel.Enable.Disable);
                bleSlave2.SetEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Disable);
            }
        }

        #endregion Enable/Disable


        #region Apply

        private void btn_AccelApplyBle2_Click(object sender, EventArgs e)
        {
            if (AccelBwBle2 != trackBarAccelBwBle2.Value)
            {
                //bleSlave2.AccelSetBw(accelBwBle2);
                bleSlave2.SetBw(Optoel.BMX055.Sensor.Accel, accelBwBle2);
            }

            if (AccelRangeBle2 != trackBarAccelRangeBle2.Value)
            {
                //bleSlave2.AccelSetRange(accelRangeBle2);
                bleSlave2.SetRange(Optoel.BMX055.Sensor.Accel, accelRangeBle2);
            }
        }

        private void btn_GyroApplyble2_Click(object sender, EventArgs e)
        {
            if (GyroBwBle2 != trackBarGyroBwBle2.Value)
            {
                //bleSlave2.GyroSetBw(gyroBwBle2);
                bleSlave2.SetBw(Optoel.BMX055.Sensor.Gyro, gyroBwBle2);
            }

            if (GyroRangeBle2 != trackBarGyroRangeBle2.Value)
            {
                //bleSlave2.GyroSetRange(gyroRangeBle2);
                bleSlave2.SetRange(Optoel.BMX055.Sensor.Accel, gyroRangeBle2);
            }
        }

        private void btn_MagnoApplyBle2_Click(object sender, EventArgs e)
        {
            if (MagnoBwBle2 != trackBarMagnoBwBle2.Value)
            {
                //bleSlave2.MagnoSetBw(magnoBwBle2);
                bleSlave2.SetBw(Optoel.BMX055.Sensor.Magno, magnoBwBle2);
            }
        }

        #endregion Apply


        #region Read Enable

        bool StartBle2 = false;


        bool AccelReadEnableBle2 = false;
        Optoel.BMX055.Enable accelReadEnableStartingBle2 = Optoel.BMX055.Enable.Disable;
        private void lbl_AccelReadEnableBle2_DoubleClick(object sender, EventArgs e)
        {
            if (StartBle2 == false)
            {
                if (accelReadEnableStartingBle2 == Optoel.BMX055.Enable.Disable)
                {
                    accelReadEnableStartingBle2 = Optoel.BMX055.Enable.Enable;
                    lbl_AccelReadEnableBle2.Text = "Enable";
                    lbl_AccelReadEnableBle2.BackColor = Color.Lime;
                    AccelReadEnableBle2 = true;
                }
                else if (accelReadEnableStartingBle2 == Optoel.BMX055.Enable.Enable)
                {
                    accelReadEnableStartingBle2 = Optoel.BMX055.Enable.Disable;
                    lbl_AccelReadEnableBle2.Text = "Disable";
                    lbl_AccelReadEnableBle2.BackColor = Color.Red;
                    AccelReadEnableBle2 = false;
                }
            }
            else
            {
                if (AccelReadEnableBle2 == false)
                {
                    //bleSlave2.AccelReadEnable(Optoel.Optoel.Enable.Enable);
                    bleSlave2.SetReadEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Enable);
                }
                else if (AccelReadEnableBle2 == true)
                {
                    //bleSlave2.AccelReadEnable(Optoel.Optoel.Enable.Disable);
                    bleSlave2.SetReadEnable(Optoel.BMX055.Sensor.Accel, Optoel.BMX055.Enable.Disable);
                }
            }
        }


        bool GyroReadEnableBle2 = false;
        Optoel.BMX055.Enable gyroReadEnableStartingBle2 = Optoel.BMX055.Enable.Disable;
        private void lbl_GyroReadEnableBle2_DoubleClick(object sender, EventArgs e)
        {
            if (StartBle2 == false)
            {
                if (gyroReadEnableStartingBle2 == Optoel.BMX055.Enable.Disable)
                {
                    gyroReadEnableStartingBle2 = Optoel.BMX055.Enable.Enable;
                    lbl_GyroReadEnableBle2.Text = "Enable";
                    lbl_GyroReadEnableBle2.BackColor = Color.Lime;
                    GyroReadEnableBle2 = true;
                }
                else if (gyroReadEnableStartingBle2 == Optoel.BMX055.Enable.Enable)
                {
                    gyroReadEnableStartingBle2 = Optoel.BMX055.Enable.Disable;
                    lbl_GyroReadEnableBle2.Text = "Disable";
                    lbl_GyroReadEnableBle2.BackColor = Color.Red;
                    GyroReadEnableBle2 = false;
                }
            }
            else
            {
                if (GyroReadEnableBle2 == false)
                {
                    //bleSlave2.GyroReadEnable(Optoel.BMX055.Enable.Enable);
                    bleSlave2.SetReadEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Enable);
                }
                else if (GyroReadEnableBle2 == true)
                {
                    //bleSlave2.GyroReadEnable(Optoel.Optoel.Enable.Disable);
                    bleSlave2.SetReadEnable(Optoel.BMX055.Sensor.Gyro, Optoel.BMX055.Enable.Disable);
                }
            }
        }


        bool MagnoReadEnableBle2 = false;
        Optoel.BMX055.Enable magnoReadEnableStartingBle2 = Optoel.BMX055.Enable.Disable;
        private void lbl_MagnoReadEnableBle2_DoubleClick(object sender, EventArgs e)
        {
            if (StartBle2 == false)
            {
                if (magnoReadEnableStartingBle2 == Optoel.BMX055.Enable.Disable)
                {
                    magnoReadEnableStartingBle2 = Optoel.BMX055.Enable.Enable;
                    lbl_MagnoReadEnableBle2.Text = "Enable";
                    lbl_MagnoReadEnableBle2.BackColor = Color.Lime;
                    MagnoReadEnableBle2 = true;
                }
                else if (magnoReadEnableStartingBle2 == Optoel.BMX055.Enable.Enable)
                {
                    magnoReadEnableStartingBle2 = Optoel.BMX055.Enable.Disable;
                    lbl_MagnoReadEnableBle2.Text = "Disable";
                    lbl_MagnoReadEnableBle2.BackColor = Color.Red;
                    MagnoReadEnableBle2 = false;
                }
            }
            else
            {
                if (MagnoReadEnableBle2 == false)
                {
                    //bleSlave2.MagnoReadEnable(Optoel.Optoel.Enable.Enable);
                    bleSlave2.SetReadEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Enable);
                }
                else if (MagnoReadEnableBle2 == true)
                {
                    //bleSlave2.MagnoReadEnable(Optoel.Optoel.Enable.Disable);
                    bleSlave2.SetReadEnable(Optoel.BMX055.Sensor.Magno, Optoel.BMX055.Enable.Disable);
                }
            }
        }

        #endregion Read Enable



        private void btn_StartBle2_Click(object sender, EventArgs e)
        {
            bleSlave2.Start(accelReadEnableStartingBle2, gyroReadEnableStartingBle2, magnoReadEnableStartingBle2);

            StartBle2 = true;
        }

        private void btn_StopBle2_Click(object sender, EventArgs e)
        {
            ListBleSlave[1].Stop();
            StartBle2 = false;
        }

        #endregion Ble2



        #endregion Ble






        #region SerialPort

        #region TrackBars

        public int AccelBw = 3;
        Optoel.BMX055.AccelBw accelBw;
        private void trackBarAccelBw_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarAccelBw.Value)
            {
                case 0:
                    accelBw = Optoel.BMX055.AccelBw.AccelBw_15f625;
                    lbl_AccelBw.Text = "15.625 Hz (64 ms)";
                    break;
                case 1:
                    accelBw = Optoel.BMX055.AccelBw.AccelBw_31f25;
                    lbl_AccelBw.Text = "31.25 Hz (32 ms)";
                    break;
                case 2:
                    accelBw = Optoel.BMX055.AccelBw.AccelBw_62f5;
                    lbl_AccelBw.Text = "62.5 Hz (16 ms)";
                    break;
                case 3:
                    accelBw = Optoel.BMX055.AccelBw.AccelBw_125;
                    lbl_AccelBw.Text = "125 Hz (8 ms)";
                    break;
                case 4:
                    accelBw = Optoel.BMX055.AccelBw.AccelBw_250;
                    lbl_AccelBw.Text = "250 Hz (4 ms)";
                    break;
            }
        }

        int AccelRange = 0;
        Optoel.BMX055.AccelRange accelRange;
        private void trackBarAccelRange_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarAccelRange.Value)
            {
                case 0:
                    accelRange = Optoel.BMX055.AccelRange.AccelRange_2g;
                    lbl_AccelRange.Text = "2g";
                    break;
                case 1:
                    accelRange = Optoel.BMX055.AccelRange.AccelRange_4g;
                    lbl_AccelRange.Text = "4g";
                    break;
                case 2:
                    accelRange = Optoel.BMX055.AccelRange.AccelRange_8g;
                    lbl_AccelRange.Text = "8g";
                    break;
                case 3:
                    accelRange = Optoel.BMX055.AccelRange.AccelRange_16g;
                    lbl_AccelRange.Text = "16g";
                    break;
            }
        }

        public int GyroBw = 0;
        Optoel.BMX055.GyroBw gyroBw;
        private void trackBarGyroBw_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarGyroBw.Value)
            {
                case 0:
                    gyroBw = Optoel.BMX055.GyroBw.GyroBw_100Hz;
                    lbl_GyroBw.Text = "100 Hz";
                    break;
                case 1:
                    gyroBw = Optoel.BMX055.GyroBw.GyroBw_200Hz;
                    lbl_GyroBw.Text = "200 Hz";
                    break;
            }
        }

        int GyroRange = 4;
        Optoel.BMX055.GyroRange gyroRange;
        private void trackBarGyroRange_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarGyroRange.Value)
            {
                case 0:
                    gyroRange = Optoel.BMX055.GyroRange.GyroRange_125s;
                    lbl_GyroRange.Text = "125 °/s";
                    break;
                case 1:
                    gyroRange = Optoel.BMX055.GyroRange.GyroRange_250s;
                    lbl_GyroRange.Text = "250 °/s";
                    break;
                case 2:
                    gyroRange = Optoel.BMX055.GyroRange.GyroRange_500s;
                    lbl_GyroRange.Text = "500 °/s";
                    break;
                case 3:
                    gyroRange = Optoel.BMX055.GyroRange.GyroRange_1000s;
                    lbl_GyroRange.Text = "1000 °/s";
                    break;
                case 4:
                    gyroRange = Optoel.BMX055.GyroRange.GyroRange_2000s;
                    lbl_GyroRange.Text = "2000 °/s";
                    break;
            }
        }

        public int MagnoBw = 7;
        Optoel.BMX055.MagnoBw magnoBw;
        private void trackBarMagnoBw_ValueChanged(object sender, EventArgs e)
        {
            switch (trackBarMagnoBw.Value)
            {
                case 0:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_2Hz;
                    lbl_MagnoBw.Text = "2 Hz";
                    break;
                case 1:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_6Hz;
                    lbl_MagnoBw.Text = "6 Hz";
                    break;
                case 2:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_8Hz;
                    lbl_MagnoBw.Text = "8 Hz";
                    break;
                case 3:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_10Hz;
                    lbl_MagnoBw.Text = "10 Hz";
                    break;
                case 4:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_15Hz;
                    lbl_MagnoBw.Text = "15 Hz";
                    break;
                case 5:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_20Hz;
                    lbl_MagnoBw.Text = "20 Hz";
                    break;
                case 6:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_25Hz;
                    lbl_MagnoBw.Text = "25 Hz";
                    break;
                case 7:
                    magnoBw = Optoel.BMX055.MagnoBw.MagnoBw_30Hz;
                    lbl_MagnoBw.Text = "30 Hz";
                    break;
            }
        }

        #endregion TrackBars

        // Enable/Disable bitti
        #region Enable/Disable

        public bool AccelEnable = false;
        private void lbl_AccelEnable_DoubleClick(object sender, EventArgs e)
        {
            if (AccelEnable == false)
            {
                //serialPort.AccelEnable(Optoel.BMX055.Enable.Enable);
                serialPort.SetEnable(BMX055.Sensor.Accel, BMX055.Enable.Enable);
            }
            else
            {
                //serialPort.AccelEnable(Optoel.BMX055.Enable.Disable);
                serialPort.SetEnable(BMX055.Sensor.Accel, BMX055.Enable.Disable);
            }
        }


        bool GyroEnable = false;
        private void lbl_GyroEnable_DoubleClick(object sender, EventArgs e)
        {

            if (GyroEnable == false)
            {
                //serialPort.GyroEnable(Optoel.BMX055.Enable.Enable);
                serialPort.SetEnable(BMX055.Sensor.Gyro, BMX055.Enable.Enable);
            }
            else
            {
                //serialPort.GyroEnable(Optoel.BMX055.Enable.Disable);
                serialPort.SetEnable(BMX055.Sensor.Gyro, BMX055.Enable.Disable);
            }
        }


        bool MagnoEnable = false;
        private void lbl_MagnoEnable_DoubleClick(object sender, EventArgs e)
        {

            if (MagnoEnable == false)
            {
                //serialPort.MagnoEnable(Optoel.BMX055.Enable.Enable);
                serialPort.SetEnable(BMX055.Sensor.Magno, BMX055.Enable.Enable);
            }
            else
            {
                //serialPort.MagnoEnable(Optoel.BMX055.Enable.Disable);
                serialPort.SetEnable(BMX055.Sensor.Magno, BMX055.Enable.Disable);
            }
        }

        #endregion Enable/Disable


        // Test edilecek
        #region Apply

        private void btn_AccelApply_Click(object sender, EventArgs e)
        {
            result rslt = new result();
            if (AccelBw != trackBarAccelBw.Value)
            {
                //serialPort.AccelSetBw(accelBw);
                rslt = serialPort.SetBw(BMX055.Sensor.Accel, gyroBw);
            }

            if (AccelRange != trackBarAccelRange.Value)
            {
                //serialPort.AccelSetRange(accelRange);
                serialPort.SetRange(BMX055.Sensor.Accel, accelRange);
            }
        }

        private void btn_GyroApply_Click(object sender, EventArgs e)
        {
            if (GyroBw != trackBarGyroBw.Value)
            {
                //serialPort.GyroSetBw(gyroBw);
                serialPort.SetBw(BMX055.Sensor.Gyro, gyroBw);
            }

            if (GyroRange != trackBarGyroRange.Value)
            {
                //serialPort.GyroSetRange(gyroRange);
                serialPort.SetRange(BMX055.Sensor.Gyro, gyroRange);
            }
        }

        private void btn_MagnoApply_Click(object sender, EventArgs e)
        {
            if (MagnoBw != trackBarMagnoBw.Value)
            {
                //serialPort.MagnoSetBw(magnoBw);
                serialPort.SetBw(BMX055.Sensor.Magno, magnoBw);
            }
        }

        #endregion Apply



        #region ReadEnable
        bool Start = false;

        bool AccelReadEnable = false;



        Optoel.BMX055.Enable accelReadEnableStarting = Optoel.BMX055.Enable.Disable;
        private void lbl_AccelReadEnable_DoubleClick(object sender, EventArgs e)
        {
            if (Start == false)
            {
                if (accelReadEnableStarting == BMX055.Enable.Disable)
                {
                    accelReadEnableStarting = BMX055.Enable.Enable;
                    lbl_AccelReadEnable.Text = "Enable";
                    lbl_AccelReadEnable.BackColor = Color.Lime;
                    AccelReadEnable = true;
                }
                else if (accelReadEnableStarting == BMX055.Enable.Enable)
                {
                    accelReadEnableStarting = BMX055.Enable.Disable;
                    lbl_AccelReadEnable.Text = "Disable";
                    lbl_AccelReadEnable.BackColor = Color.Red;
                    AccelReadEnable = false;
                }
            }
            else
            {
                if (AccelReadEnable == false)
                {
                    //serialPort.AccelReadEnable(Optoel.BMX055.Enable.Enable);
                    serialPort.SetReadEnable(BMX055.Sensor.Accel, BMX055.Enable.Enable);
                }
                else if (AccelReadEnable == true)
                {
                    //serialPort.AccelReadEnable(Optoel.BMX055.Enable.Disable);
                    serialPort.SetReadEnable(BMX055.Sensor.Accel, BMX055.Enable.Disable);
                }
            }
        }


        bool GyroReadEnable = false;
        Optoel.BMX055.Enable gyroReadEnableStarting = Optoel.BMX055.Enable.Disable;
        private void lbl_GyroReadEnable_DoubleClick(object sender, EventArgs e)
        {

            if (Start == false)
            {
                if (gyroReadEnableStarting == Optoel.BMX055.Enable.Disable)
                {
                    gyroReadEnableStarting = Optoel.BMX055.Enable.Enable;
                    lbl_GyroReadEnable.Text = "Enable";
                    lbl_GyroReadEnable.BackColor = Color.Lime;
                    GyroReadEnable = true;
                }
                else if (gyroReadEnableStarting == Optoel.BMX055.Enable.Enable)
                {
                    gyroReadEnableStarting = Optoel.BMX055.Enable.Disable;
                    lbl_GyroReadEnable.Text = "Disable";
                    lbl_GyroReadEnable.BackColor = Color.Red;
                    GyroReadEnable = false;
                }
            }
            else
            {
                if (GyroReadEnable == false)
                {
                    //serialPort.GyroReadEnable(Optoel.BMX055.Enable.Enable);
                    serialPort.SetReadEnable(BMX055.Sensor.Gyro, BMX055.Enable.Enable);
                }
                else if (GyroReadEnable == true)
                {
                    //serialPort.GyroReadEnable(Optoel.BMX055.Enable.Disable);
                    serialPort.SetReadEnable(BMX055.Sensor.Gyro, BMX055.Enable.Disable);
                }
            }
        }


        bool MagnoReadEnable = false;
        Optoel.BMX055.Enable magnoReadEnableStarting = Optoel.BMX055.Enable.Disable;
        private void lbl_MagnoReadEnable_DoubleClick(object sender, EventArgs e)
        {

            if (Start == false)
            {
                if (magnoReadEnableStarting == Optoel.BMX055.Enable.Disable)
                {
                    magnoReadEnableStarting = Optoel.BMX055.Enable.Enable;
                    lbl_MagnoReadEnable.Text = "Enable";
                    lbl_MagnoReadEnable.BackColor = Color.Lime;
                    MagnoReadEnable = true;
                }
                else if (magnoReadEnableStarting == Optoel.BMX055.Enable.Enable)
                {
                    magnoReadEnableStarting = Optoel.BMX055.Enable.Disable;
                    lbl_MagnoReadEnable.Text = "Disable";
                    lbl_MagnoReadEnable.BackColor = Color.Red;
                    MagnoReadEnable = false;
                }
            }
            else
            {
                if (MagnoReadEnable == false)
                {
                    //serialPort.MagnoReadEnable(Optoel.BMX055.Enable.Enable);
                    serialPort.SetReadEnable(BMX055.Sensor.Magno, BMX055.Enable.Enable);
                }
                else if (MagnoReadEnable == true)
                {
                    //serialPort.MagnoReadEnable(Optoel.BMX055.Enable.Disable);
                    serialPort.SetReadEnable(BMX055.Sensor.Magno, BMX055.Enable.Disable);
                }
            }
        }

        #endregion ReadEnable



        private void btn_Start_Click(object sender, EventArgs e)
        {
            serialPort.Start(accelReadEnableStarting, gyroReadEnableStarting, magnoReadEnableStarting);

            Start = true;
        }

        #endregion SerialPort

    }
}
