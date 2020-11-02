using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;


namespace ConsoleApp1
{
    class Program
    {
        static string accelPath1 = @"Accel1.csv";
        static string gyroPath1 = @"Gyro1.csv";
        static string magnoPath1 = @"Magno1.csv";
        static string AllDataPath = @"AllData.csv";

        static string accelPath2 = @"Accel2.csv";
        static string gyroPath2 = @"Gyro2.csv";
        static string magnoPath2 = @"Magno2.csv";

        static FileStream fileStreamAccel1;
        static FileStream fileStreamGyro1;
        static FileStream fileStreamMagno1;
        static FileStream fileStreamAllData;
        static StreamWriter swAccel1;
        static StreamWriter swGyro1;
        static StreamWriter swMagno1;
        static StreamWriter swAllData;

        static FileStream fileStreamAccel2;
        static FileStream fileStreamGyro2;
        static FileStream fileStreamMagno2;
        static StreamWriter swAccel2;
        static StreamWriter swGyro2;
        static StreamWriter swMagno2;

        static System.Timers.Timer timer = new System.Timers.Timer();
        static Optoel.Optoel.Ble ble = new Optoel.Optoel.Ble();
        static string Com = "COM4";

        static Thread ThreadConsolRead;



        private static void ConsolereadThread()
        {
            string st = "";

            while (ble.IsOpen)
            {
                try
                {
                    st = Console.ReadLine();

                    if (st.Trim().Substring(0, 3) == "End")
                    {
                        Console.WriteLine(ListbleSlave[0].ListAccel.Count);
                        Console.WriteLine(ListbleSlave[0].ListGyro.Count);
                        Console.WriteLine(ListbleSlave[0].ListMagno.Count);
                    }
                    else if (st.Trim().Substring(0, 5) == "Start")
                    {
                        Console.WriteLine("Accel Unit: mg \nGyro Unit:  degree/sec. \n");

                        foreach (var item in ListbleSlave)
                        {
                            item.Start(Optoel.Optoel.Enable.Enable, Optoel.Optoel.Enable.Enable, Optoel.Optoel.Enable.Enable);
                        }
 
                        timer.Enabled = true;
                        timer.Interval = 10000;
                        timer.Start();
                    }
                    else if (st.Trim().Substring(0, 7) == "Connect")
                    {
                        if (Char.IsNumber(st[8]))
                        {
                            SelectedDevice = int.Parse(st[8].ToString());

                            if (0 < SelectedDevice && SelectedDevice <= OptoelDevices.Count)
                                ble.ConnectToDevice(OptoelDevices[SelectedDevice - 1]);  
                            else
                                Console.WriteLine("You entered incorrectly!");
                        }
                    }
                    else if (st.Trim().Substring(0, 10) == "Disconnect")
                    {
                        if (char.IsNumber(st[11]))
                        {
                            int n = int.Parse(st[11].ToString());

                            if (0 < SelectedDevice && SelectedDevice <= OptoelDevices.Count)
                                ListbleSlave[n - 1].BleDisconnect();
                            else
                                Console.WriteLine("You entered incorrectly!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("You entered incorrectly!");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("You entered incorrectly!");
                }
            }
        }


        static void Settings()
        {
            File.Delete(accelPath1);
            File.Delete(gyroPath1);
            File.Delete(magnoPath1);
            File.Delete(accelPath2);
            File.Delete(gyroPath2);
            File.Delete(magnoPath2);
            File.Delete(AllDataPath);
            fileStreamAccel1 = new FileStream(accelPath1, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamGyro1 = new FileStream(gyroPath1, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamMagno1 = new FileStream(magnoPath1, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamAllData = new FileStream(AllDataPath, FileMode.OpenOrCreate, FileAccess.Write);
            swAccel1 = new StreamWriter(fileStreamAccel1);
            swGyro1 = new StreamWriter(fileStreamGyro1);
            swMagno1 = new StreamWriter(fileStreamMagno1);
            swAllData = new StreamWriter(fileStreamAllData);

            fileStreamAccel2 = new FileStream(accelPath2, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamGyro2 = new FileStream(gyroPath2, FileMode.OpenOrCreate, FileAccess.Write);
            fileStreamMagno2 = new FileStream(magnoPath2, FileMode.OpenOrCreate, FileAccess.Write);
            swAccel2 = new StreamWriter(fileStreamAccel2);
            swGyro2 = new StreamWriter(fileStreamGyro2);
            swMagno2 = new StreamWriter(fileStreamMagno2);

            ble.DisConnected += new Optoel.Optoel.Ble.DisconnectedEventHandler(DisconnectedEvent);
            ble.SlaveError += new Optoel.Optoel.Ble.SlaveErrorEventHandler(SlaveErrorEvent);

            ble.EnableResult += new Optoel.Optoel.Ble.EnableEventHandler(EnableResultEvent);
            ble.BwResult += new Optoel.Optoel.Ble.BwEventHandler(BwResultEvent);
            ble.RangeResult += new Optoel.Optoel.Ble.RangeEventHandler(RangeResultEvent);

            ble.AccelData += new Optoel.Optoel.Ble.AccelDataEventHandler(AccelDataEvent);
            ble.GyroData += new Optoel.Optoel.Ble.GyroDataEventHandler(GyroDataEvent);
            ble.MagnoData += new Optoel.Optoel.Ble.MagnoDataEventHandler(MagnoDataEvent);

            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerEvent);
            timer.Enabled = false;
            timer.Stop();

            Console.WriteLine("Commands:");
            Console.WriteLine(" Disconnect to device:   Disconnect <n>");
            Console.WriteLine(" Connect to device:      Connect <n>");
            Console.WriteLine(" Get data:               Start");
            Console.WriteLine(" End the Program:        End");

            Console.WriteLine("\n\n");
        }


        static int SelectedDevice = -1;
        static void Main(string[] args)
        {
            Settings();

            ble.BleDiscoverResult += new Optoel.Optoel.Ble.BleDiscoverResultEventHandler(BleDiscoverResultEvent);
            ble.BleConnectResult += new Optoel.Optoel.Ble.BleConnectResultEventHandler(BleConnectResultevent);
            ble.BleErrorEvent += new Optoel.Optoel.Ble.BleErrorEventHandler(BleError);

            Optoel.Optoel.result rslt = new Optoel.Optoel.result();
           
            rslt = ble.Open(Com);
            Thread.Sleep(100);

            if (rslt.Succes)
            {
                ThreadConsolRead = new Thread(ConsolereadThread);
                ThreadConsolRead.Start();

                Console.WriteLine(ble.ConnectMessage + "\n");
                Console.WriteLine("Dongle Mac Address:          " + ble.DongleAddress);
                Console.WriteLine("Max. Supported Connections:  " + ble.MaxSupportedConnections);

                ble.DiscoverStart(2);
                Thread.Sleep(2500);

                if (OptoelDevices.Count > 0)
                {
                    Console.Write("\nHarhangi bir cihaza bağlanmak için 'Connect <n>' yazın:");
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
                        Console.WriteLine("\n" + ble.ConnectMessage + "\n");
                        break;
                    }
                }
                else
                {
                    break;
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

            if (OptoelDevices.Count > 0)
            {
                Console.WriteLine("\n-- Discovered Devices --");
                int n = 1;
                foreach (var item in OptoelDevices)
                {
                    Console.WriteLine("" + n + ". Device:");
                    Console.WriteLine("  Device Name: " + item.DeviceName + "\n  MacAddress: " + item.MAC_Address);
                    n++;
                }
            }
        }


        //static Optoel.Optoel.Ble.BleSlave bleSlave;  /* alternatif kullanım */
        static List<Optoel.Optoel.Ble.BleSlave> ListbleSlave = new List<Optoel.Optoel.Ble.BleSlave>();
        static int slaveNmbr = 1;
        private static void BleConnectResultevent(Optoel.Optoel.Ble.BleSlave e)
        {
            //bleSlave = e;  /* alternatif kullanım */
            e.SlaveNumber = slaveNmbr; // Kullanıcı tarafından isteğe göre verilen cihaz numarası.
            slaveNmbr++;
            ListbleSlave.Add(e);

            Console.WriteLine("\nConnected");
            Console.WriteLine("Device Conected State:   " + e.Connected);
            Console.WriteLine("Device Name:             " + e.DeviceName);
            Console.WriteLine("Mac Address:             " + e.MacAddress);
            Console.Write("Mac AddressHex:          ");
            string st = "";
            foreach (var item in e.MacAddressHEX)
            {
                st += item.ToString("X") + ":";
            }
            st = st.Remove(st.Length - 1);
            Console.WriteLine(st + "\n\n");

            e.AccelEnable(Optoel.Optoel.Enable.Enable);
        }

       

        private static void DisconnectedEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.Ble.BleSlave.DisconnectedEventArgs e)
        {
            Console.WriteLine("\n{0}. Device disconnected!", sender.SlaveNumber);
            Console.WriteLine("Reason Code: " + e.ReasonCode);
            Console.WriteLine("Reason:  " + e.Reason);
        }

        private static void SlaveErrorEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.Ble.ErrorHandleArgs e)
        {
            Console.WriteLine("\n{0}. Device Error!", sender.SlaveNumber);
            Console.WriteLine("Error Code: " + e.ErrorCode);
            Console.WriteLine("Error:  " + e.ErrorMessage);
        }


        private static void EnableResultEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.Ble.BleSlave.EnableResultArgs e)
        {
            switch (e.Sensor)
            {
                case Optoel.Optoel.Sensor.Accel:
                    if (e.Enable == Optoel.Optoel.Enable.Enable)
                        Console.WriteLine("{0}. Device Accel Enable", sender.SlaveNumber);
                    else if (e.Enable == Optoel.Optoel.Enable.Disable)
                        Console.WriteLine("{0}. Device Accel Disable", sender.SlaveNumber);

                    sender.GyroEnable(Optoel.Optoel.Enable.Enable);
                    break;
                case Optoel.Optoel.Sensor.Gyro:
                    if (e.Enable == Optoel.Optoel.Enable.Enable)
                        Console.WriteLine("{0}. Device Gyro Enable", sender.SlaveNumber);
                    else if (e.Enable == Optoel.Optoel.Enable.Disable)
                        Console.WriteLine("{0}. Device Gyro Disable", sender.SlaveNumber);

                    sender.MagnoEnable(Optoel.Optoel.Enable.Enable);
                    break;
                case Optoel.Optoel.Sensor.Magno:
                    if (e.Enable == Optoel.Optoel.Enable.Enable)
                        Console.WriteLine("{0}. Device Magno Enable", sender.SlaveNumber);
                    else if (e.Enable == Optoel.Optoel.Enable.Disable)
                        Console.WriteLine("{0}. Device Magno Disable", sender.SlaveNumber);

                    sender.AccelSetBw(Optoel.Optoel.AccelBw.AccelBw_62f5);
                    break;
            }
        }

        private static void BwResultEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.Ble.BleSlave.BwResultArgs e)
        {
            switch (e.Sensor)
            {
                case Optoel.Optoel.Sensor.Accel:
                    Console.WriteLine(sender.SlaveNumber + ". Device Accel Bw: " + e.Bw.ToString());
                    sender.GyroSetBw(Optoel.Optoel.GyroBw.GyroBw_100Hz);

                    break;
                case Optoel.Optoel.Sensor.Gyro:
                    Console.WriteLine(sender.SlaveNumber + ". Device Gyro Bw: " + e.Bw.ToString());
                    sender.MagnoSetBw(Optoel.Optoel.MagnoBw.MagnoBw_30Hz);

                    break;
                case Optoel.Optoel.Sensor.Magno:
                    Console.WriteLine(sender.SlaveNumber + ". Device Magno Bw: " + e.Bw.ToString());
                    sender.AccelSetRange(Optoel.Optoel.AccelRange.AccelRange_4g);

                    break;
            }
        }

        private static void RangeResultEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.Ble.BleSlave.RangeResultArgs e)
        {
            switch (e.Sensor)
            {
                case Optoel.Optoel.Sensor.Accel:
                    Console.WriteLine(sender.SlaveNumber + ". Device Accel Range: " + e.Range.ToString());
                    sender.GyroSetRange(Optoel.Optoel.GyroRange.GyroRange_1000s);

                    break;
                case Optoel.Optoel.Sensor.Gyro:
                    Console.WriteLine(sender.SlaveNumber + ". Device Gyro Range: " + e.Range.ToString());

                    break;
            }
        }

        static Optoel.Optoel.BMX055.Accel accelcsv = new Optoel.Optoel.BMX055.Accel(0, 0, 0, 0);
        static Optoel.Optoel.BMX055.Gyro gyrocsv = new Optoel.Optoel.BMX055.Gyro(0, 0, 0, 0);
        static Optoel.Optoel.BMX055.Magno magnocsv = new Optoel.Optoel.BMX055.Magno(0, 0, 0, 0);

        private static void AccelDataEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.BMX055.Accel Accel)
        {
            accelcsv = Accel;
            Console.WriteLine(sender.SlaveNumber + ". Device Accel:   {0},    {1},    {2}", Accel.RawX, Accel.RawY, Accel.RawZ);
            
            if(sender.SlaveNumber == 1)
            {
                swAccel1.WriteLine(Accel.AccelDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Accel.RawX.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawY.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawZ.ToString(new CultureInfo("en-US", false)));
                swAccel1.Flush();
            }
            else if(sender.SlaveNumber == 2)
            {
                swAccel2.WriteLine(Accel.AccelDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Accel.RawX.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawY.ToString(new CultureInfo("en-US", false)) + "," + Accel.RawZ.ToString(new CultureInfo("en-US", false)));
                swAccel2.Flush();
            } 
        }

        private static void GyroDataEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.BMX055.Gyro Gyro)
        {
            gyrocsv = Gyro;
            Console.WriteLine(sender.SlaveNumber + ". Device Gyro:    {0},    {1},    {2}", Gyro.RawX, Gyro.RawY, Gyro.RawZ);

            if (sender.SlaveNumber == 1)
            {
                swGyro1.WriteLine(Gyro.GyroDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Gyro.RawX.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawY.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawZ.ToString(new CultureInfo("en-US", false)));
                swGyro1.Flush();
            }
            else if (sender.SlaveNumber == 2)
            {
                swGyro2.WriteLine(Gyro.GyroDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Gyro.RawX.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawY.ToString(new CultureInfo("en-US", false)) + "," + Gyro.RawZ.ToString(new CultureInfo("en-US", false)));
                swGyro2.Flush();
            }
           
            //swAllData.WriteLine(
            //    accelcsv.RawX.ToString(new CultureInfo("en-US", false)) + "," +
            //    accelcsv.RawY.ToString(new CultureInfo("en-US", false)) + "," +
            //    accelcsv.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

            //    gyrocsv.RawX.ToString(new CultureInfo("en-US", false)) + "," +
            //    gyrocsv.RawY.ToString(new CultureInfo("en-US", false)) + "," +
            //    gyrocsv.RawZ.ToString(new CultureInfo("en-US", false)) + "," +

            //    magnocsv.RawX.ToString(new CultureInfo("en-US", false)) + "," +
            //    magnocsv.RawY.ToString(new CultureInfo("en-US", false)) + "," +
            //    magnocsv.RawZ.ToString(new CultureInfo("en-US", false)) + ","
            //    );
            //swAllData.Flush();
        }

        private static void MagnoDataEvent(Optoel.Optoel.Ble.BleSlave sender, Optoel.Optoel.BMX055.Magno Magno)
        {
            magnocsv = Magno;
            Console.WriteLine(sender.SlaveNumber + ". Device Magno:   {0},    {1},    {2}", Magno.RawX, Magno.RawY, Magno.RawZ);

            if (sender.SlaveNumber == 1)
            {
                swMagno1.WriteLine(Magno.MagnoDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Magno.RawX.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawY.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawZ.ToString(new CultureInfo("en-US", false)));
                swMagno1.Flush();
            }
            else if (sender.SlaveNumber == 2)
            {
                swMagno2.WriteLine(Magno.MagnoDataNumber.ToString(new CultureInfo("en-US", false)) + ", " + Magno.RawX.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawY.ToString(new CultureInfo("en-US", false)) + "," + Magno.RawZ.ToString(new CultureInfo("en-US", false)));
                swMagno2.Flush();
            } 
        }



        private static void TimerEvent(object sender, ElapsedEventArgs e)
        {
            foreach (var item in ListbleSlave)
            {
                item.Stop();
            }

            timer.Stop();
        }




       


     



    }


}
