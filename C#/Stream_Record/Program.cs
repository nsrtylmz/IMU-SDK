using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Optoel;


namespace ConsoleApp1
{
    class Program
    {
        static string accelPath = @"Accel.csv";
        static string gyroPath = @"Gyro.csv";
        static string magnoPath = @"Magno.csv";
        static string AllDataPath = @"AllData.csv";

        static FileStream fileStreamAccel;
        static FileStream fileStreamGyro;
        static FileStream fileStreamMagno;
        static FileStream fileStreamAllData;
        static StreamWriter swAccel;
        static StreamWriter swGyro;
        static StreamWriter swMagno;
        static StreamWriter swAllData;

        static System.Timers.Timer timer = new System.Timers.Timer();
        static Optoel.Optoel.Ble ble = new Optoel.Optoel.Ble();
        static string Com = "COM4";


        static void Settings()
        {
            File.Delete(accelPath);
            File.Delete(gyroPath);
            File.Delete(magnoPath);
            File.Delete(AllDataPath);
            fileStreamAccel = new FileStream(accelPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamGyro = new FileStream(gyroPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamMagno = new FileStream(magnoPath, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamAllData = new FileStream(AllDataPath, FileMode.OpenOrCreate, FileAccess.Write);
            swAccel = new StreamWriter(fileStreamAccel);
            swGyro = new StreamWriter(fileStreamGyro);
            swMagno = new StreamWriter(fileStreamMagno);
            swAllData = new StreamWriter(fileStreamAllData);

            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerEvent);
            timer.Enabled = false;
            timer.Stop();
        }


        static async Task Main(string[] args)
        {
            Settings();

            ble.BleDiscoverResult += new Optoel.Optoel.Ble.BleDiscoverResultEventHandler(BleDiscoverResultEvent);
            ble.BleConnectResult += new Optoel.Optoel.Ble.BleConnectResultEventHandler(BleConnectResultevent);
            ble.BleErrorEvent += new Optoel.Optoel.Ble.BleErrorEventHandler(BleError);

            Optoel.Optoel.result rslt = new Optoel.Optoel.result();

            rslt = await ble.Open(Com);
            Thread.Sleep(100);

            if (rslt.Succes)
            {
                Console.WriteLine(ble.ConnectMessage + "\n");
                Console.WriteLine("Dongle Mac Address:          " + ble.DongleAddress);
                Console.WriteLine("Max. Supported Connections:  " + ble.MaxSupportedConnections);

                ble.DiscoverStart(2);
                Thread.Sleep(2500);

                if (OptoelDevices.Count > 0)
                {
                    int SelectedDevice = 0;

                    do
                    {
                        Console.Write("\nEnter the device number you want to connect to: ");
                        string getReadLine = Console.ReadLine();

                        SelectedDevice = Convert.ToInt32(getReadLine);
                        if (SelectedDevice > OptoelDevices.Count)
                            Console.WriteLine("You entered incorrectly!");
                        else
                            break;

                    } while (true);

                    ble.ConnectToDevice(OptoelDevices[SelectedDevice - 1]);
                }
                else
                {
                    Console.WriteLine("\nNo Optoel device found!");
                }
            }
            else
            {
                Console.WriteLine(rslt.Message);
            }


            while (true)
            {
                if (rslt.Succes)
                {
                    if (!ble.IsOpen)
                    {
                        Console.WriteLine("\n" + ble.ConnectMessage);
                    }
                }
            }

            Console.WriteLine("Exiting...");
            Console.ReadKey();
        }




        static private void BleError(Optoel.Optoel.Ble.ErrorHandleArgs Error)
        {
            Console.WriteLine("\n\nError!");
            Console.WriteLine("Error Code:      " + Error.ErrorCode);
            Console.WriteLine("Error Message:   " + Error.ErrorMessage);
        }


        static List<Optoel.Optoel.Ble.Device> OptoelDevices = new List<Optoel.Optoel.Ble.Device>();
        static private void BleDiscoverResultEvent(List<Optoel.Optoel.Ble.Device> e)
        {
            OptoelDevices = e;
            
            if(OptoelDevices.Count > 0)
            {
                Console.WriteLine("\n-- Discovered Devices --");
                int n = 1;
                foreach (var item in OptoelDevices)
                {
                    Console.WriteLine("" + n + ". Device:");
                    Console.WriteLine("  Device Name: " + item.DeviceName + "\n  MacAddress: " + item.MAC_Address);
                }
            }
        }


        static Optoel.Optoel.Ble.BleSlave bleSlave;
        //static List<Optoel.Optoel.Ble.BleSlave> ListbleSlave = new List<Optoel.Optoel.Ble.BleSlave>(); //alternatif kullanım
        private static void BleConnectResultevent(Optoel.Optoel.Ble.BleSlave e)
        {
            bleSlave = e;
            //ListbleSlave.Add(e);  // alternatif kullanım

            Console.WriteLine("\nConnected");
            Console.WriteLine("Device Conected State:   " + bleSlave.Connected);
            Console.WriteLine("Device Name:             " + bleSlave.DeviceName);
            Console.WriteLine("Mac Address:             " + bleSlave.MacAddress);
            Console.Write("Mac AddressHex:          ");
            string st = "";
            foreach (var item in bleSlave.MacAddressHEX)
            {
                st += item.ToString("X") + ":";    
            }
            st=st.Remove(st.Length - 1);
            Console.WriteLine(st + "\n\n");

            bleSlave.SlaveDisconnected += new Optoel.Optoel.Ble.BleSlave.SlaveDisconnectedEventHandler(SlaveDisconnectedEvent);
            bleSlave.SlaveEnableResult += new Optoel.Optoel.Ble.BleSlave.EnableEventHandler(SlaveEnableResultEvent);
            bleSlave.SlaveBwResult += new Optoel.Optoel.Ble.BleSlave.BwEventHandler(SlaveBwResultEvent);
            bleSlave.SlaveRangeResult += new Optoel.Optoel.Ble.BleSlave.RangeEventHandler(SlaveRangeResultEvent);

            bleSlave.AccelData += new Optoel.Optoel.Ble.BleSlave.AccelDataEventHandler(AccelDataEvent);
            bleSlave.GyroData += new Optoel.Optoel.Ble.BleSlave.GyroDataEventHandler(GyroDataEvent);
            bleSlave.MagnoData += new Optoel.Optoel.Ble.BleSlave.MagnoDataEventHandler(MagnoDataEvent);
               

            bleSlave.AccelEnable(Optoel.Optoel.Enable.Enable);
        }

        private static void SlaveDisconnectedEvent(Optoel.Optoel.Ble.BleSlave.DisconnectedEventArgs e)
        {
            Console.WriteLine("\nDevice disconnected!");
            Console.WriteLine("Reason Code: " + e.ReasonCode);
            Console.WriteLine("Reason:  " + e.Reason);
        }


        private static void SlaveEnableResultEvent(Optoel.Optoel.Ble.BleSlave.EnableResultArgs e)
        {
            switch (e.Sensor)
            {
                case Optoel.Optoel.Sensor.Accel:
                    if (e.Enable == Optoel.Optoel.Enable.Enable)
                        Console.WriteLine("Accel Enable");
                    else if (e.Enable == Optoel.Optoel.Enable.Disable)
                        Console.WriteLine("Accel Disable");

                    bleSlave.GyroEnable(Optoel.Optoel.Enable.Enable);
                    break;
                case Optoel.Optoel.Sensor.Gyro:
                    if (e.Enable == Optoel.Optoel.Enable.Enable)
                        Console.WriteLine("Gyro Enable");
                    else if (e.Enable == Optoel.Optoel.Enable.Disable)
                        Console.WriteLine("Gyro Disable");

                    bleSlave.MagnoEnable(Optoel.Optoel.Enable.Enable);
                    break;
                case Optoel.Optoel.Sensor.Magno:
                    if (e.Enable == Optoel.Optoel.Enable.Enable)
                        Console.WriteLine("Magno Enable");
                    else if (e.Enable == Optoel.Optoel.Enable.Disable)
                        Console.WriteLine("Magno Disable");

                    bleSlave.AccelSetBw(Optoel.Optoel.AccelBw.AccelBw_62f5);
                    break;
            }
        }

        private static void SlaveBwResultEvent(Optoel.Optoel.Ble.BleSlave.BwResultArgs e)
        {
            switch (e.Sensor)
            {
                case Optoel.Optoel.Sensor.Accel:
                    Console.WriteLine("Accel Bw: " + e.Bw.ToString());
                    bleSlave.GyroSetBw(Optoel.Optoel.GyroBw.GyroBw_100Hz);

                    break;
                case Optoel.Optoel.Sensor.Gyro:
                    Console.WriteLine("Gyro Bw: " + e.Bw.ToString());
                    bleSlave.MagnoSetBw(Optoel.Optoel.MagnoBw.MagnoBw_30Hz);

                    break;
                case Optoel.Optoel.Sensor.Magno:
                    Console.WriteLine("Magno Bw: " + e.Bw.ToString());
                    bleSlave.AccelSetRange(Optoel.Optoel.AccelRange.AccelRange_4g);

                    break;
            }
        }

        private static void SlaveRangeResultEvent(Optoel.Optoel.Ble.BleSlave.RangeResultArgs e)
        {
            switch (e.Sensor)
            {
                case Optoel.Optoel.Sensor.Accel:
                    Console.WriteLine("Accel Range: " + e.Range.ToString());

                    bleSlave.GyroSetRange(Optoel.Optoel.GyroRange.GyroRange_1000s);
                    break;
                case Optoel.Optoel.Sensor.Gyro:
                    Console.WriteLine("Gyro Range: " + e.Range.ToString());

                    Console.WriteLine("\nPress any key to start.\n\n");
                    string st = Console.ReadLine();

                    Console.WriteLine("Accel Unit: mg \nGyro Unit:  degree/sec. \n");
                    bleSlave.Start(Optoel.Optoel.Enable.Enable, Optoel.Optoel.Enable.Enable, Optoel.Optoel.Enable.Enable);
                    timer.Enabled = true;
                    timer.Interval = 10000;
                    timer.Start();
                    break;
            }
        }

        private static void TimerEvent(object sender, ElapsedEventArgs e)
        {
            bleSlave.Stop();
            timer.Stop();
        }

       
        

       

        
        static Optoel.Optoel.BMX055.Accel accelcsv = new Optoel.Optoel.BMX055.Accel(0, 0, 0, 0);
        static Optoel.Optoel.BMX055.Gyro gyrocsv = new Optoel.Optoel.BMX055.Gyro(0, 0, 0, 0);
        static Optoel.Optoel.BMX055.Magno magnocsv = new Optoel.Optoel.BMX055.Magno(0, 0, 0, 0);


        // Accel Data Event
        private static void AccelDataEvent(Optoel.Optoel.BMX055.Accel Accel)
        {
            accelcsv = Accel;
            Console.WriteLine("Accel:   {0},    {1},    {2}", Accel.RawX, Accel.RawY, Accel.RawZ);
            
            swAccel.WriteLine(Accel.AccelDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Accel.RawX.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawY.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawZ.ToString(new CultureInfo("en-US", false)));
            swAccel.Flush();
        }
        
        // Gyro Data Event
        private static void GyroDataEvent(Optoel.Optoel.BMX055.Gyro Gyro)
        {
            gyrocsv = Gyro;
            Console.WriteLine("Gyro:    {0},    {1},    {2}", Gyro.RawX, Gyro.RawY, Gyro.RawZ);

            swGyro.WriteLine(Gyro.GyroDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Gyro.RawX.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawY.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawZ.ToString(new CultureInfo("en-US", false)));
            swGyro.Flush();

            swAllData.WriteLine(
                accelcsv.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                accelcsv.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                accelcsv.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

                gyrocsv.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                gyrocsv.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                gyrocsv.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

                magnocsv.RawX.ToString(new CultureInfo("en-US", false)) + "," +
                magnocsv.RawY.ToString(new CultureInfo("en-US", false)) + "," +
                magnocsv.RawZ.ToString(new CultureInfo("en-US", false)) + ","
                );
            swAllData.Flush();
        }
        
        // Magno Data Event
        private static void MagnoDataEvent(Optoel.Optoel.BMX055.Magno Magno)
        {
            magnocsv = Magno;
            Console.WriteLine("Magno:   {0},    {1},    {2}", Magno.RawX, Magno.RawY, Magno.RawZ);

            swMagno.WriteLine(Magno.MagnoDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Magno.RawX.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawY.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawZ.ToString(new CultureInfo("en-US", false)));
            swMagno.Flush();
        }



    }

    
}
