using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Bluegiga.BLE.Events.ATTClient;
using Bluegiga.BLE.Events.Connection;
using Bluegiga.BLE.Events.GAP;
using Bluegiga.BLE.Responses.ATTClient;
using Bluegiga.BLE.Responses.Connection;
using Bluegiga.BLE.Responses.GAP;
using Bluegiga.BLE.Responses.System;


namespace Optoel
{
    public class Optoel
    {

        bool SeriportConnected = false;
        bool BleConnected = false;

        public class SerialPort : BMX055
        {





            #region Seriport

            System.IO.Ports.SerialPort SP = new System.IO.Ports.SerialPort();

            /// <summary>
            /// Seriport açık mı?
            /// </summary>
            public bool IsOpen { get { return SP.IsOpen; } }
            bool DeviceConnected = false;

            string _ConnectMessage = "";
            public string ConnectMessage { get { return _ConnectMessage; } }



            /// <summary>
            /// Seriporu açar. (BaudRade: 921600)
            /// </summary>
            /// <param name="Com"></param>
            /// <returns></returns>
            public async Task<result> Open(string Com)
            {
                result rslt = new result();

                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (SP.IsOpen)
                        {
                            rslt.Succes = false;
                            rslt.Message = _ConnectMessage = "Cihaz zaten bağlı!";
                        }
                        else
                        {
                            SP = new System.IO.Ports.SerialPort();

                            SP.PortName = Com;
                            SP.BaudRate = 921600;
                            SP.Parity = Parity.None;
                            SP.StopBits = StopBits.One;
                            SP.DataBits = 8;
                            SP.Handshake = Handshake.None;

                            SP.DataReceived += SP_DataReceived;

                            SP.Open();
                            SP.DiscardInBuffer();

                            rslt.Succes = true;
                            rslt.Message = _ConnectMessage = "Bağlandı.";
                        }
                    }
                    catch (Exception ex)
                    {
                        rslt.Succes = false;
                        rslt.Message = _ConnectMessage = "Bağlanamadı!\n" + ex.Message;
                    }
                });
                await task;

                return rslt;
            }

            /// <summary>
            /// Seriportu kapatır.
            /// </summary>
            /// <returns></returns>
            public result Close()
            {
                result rslt = new result();

                try
                {
                    if (SP.IsOpen)
                    {
                        SP.Close();
                        rslt.Succes = true;
                        rslt.Message = _ConnectMessage = "Bağlantı sonlandırıldı!";
                    }
                    else
                    {
                        rslt.Succes = true;
                    }
                }
                catch (Exception ex)
                {

                    rslt.Succes = false;
                    rslt.Message = _ConnectMessage = ex.Message;
                }

                return rslt;
            }

            result DataSend(string data)
            {
                result rslt = new result();

                if (SP.IsOpen)
                {
                    try
                    {
                        SP.Write(data);
                        rslt.Succes = true;
                    }
                    catch (Exception ex)
                    {
                        rslt.Succes = false;
                        rslt.Message = "Veri gönderilemedi!\n" + ex.Message;
                    }
                }
                else
                {
                    rslt.Succes = false;
                    rslt.Message = "Veri gönderilemedi! ComPort kapalı.";
                }

                return rslt;
            }

            result DataSend(byte[] data)
            {
                result rslt = new result();

                if (SP.IsOpen)
                {
                    try
                    {
                        SP.Write(data, 0, data.Length);
                        rslt.Succes = true;
                    }
                    catch (Exception ex)
                    {

                        rslt.Succes = false;
                        rslt.Message = "Veri gönderilemedi!\n" + ex.Message;
                    }
                }
                else
                {
                    rslt.Succes = false;
                    rslt.Message = "Veri gönderilemedi! ComPort kapalı.";
                }

                return rslt;
            }



            List<byte[]> listByte = new List<byte[]>();
            List<byte[]> listBuffer = new List<byte[]>();
            List<double> ListTimediff = new List<double>();
            List<double> ListEventtime = new List<double>();

            /*  #Gelen Data
             *  [0] Data Length
             *  [1] Response / Event (0x00: Response, 0x08: Event)
             *  [2 - n] Data 
             */

            /*  #Response
             *  [1] Response (0x00) 
             *  [2] Isready (0x00) / Enable-Disable (0x01) / Bw (0x02) / Range (0x03) / Read Enable-Disable (0x04)
             *  [3] Accel (0x01) / Gyro (0x02) / Magno (0x04)
             *  []
             * 
             * 
             * 
             */

            List<byte> receivedData = new List<byte>();
            byte[] kalanbuffer;
            byte[] kalanbuffer1;
            byte[] buffer;

            double timediff = 0;
            double eventimediff = 0;
            long tick1 = 0;
            long tick2 = 0;

            private void SP_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                eventimediff = (DateTime.Now.Ticks - tick1) / 10000.0;
                tick1 = DateTime.Now.Ticks;
                ListEventtime.Add(eventimediff);


                Thread.Sleep(1);

                try
                {
                    buffer = new Byte[SP.BytesToRead];
                    SP.Read(buffer, 0, buffer.Length);

                    receivedData.AddRange(buffer);

                    listBuffer.Add(buffer);

                    if (buffer.Length > 0)
                    {
                        try
                        {
                            if ((buffer.Length == 2) && (buffer[0] == 0x48 && buffer[1] == 0x69))   // "Hi"
                            {
                                DeviceConnected = true;
                                Thread.Sleep(5);
                                DataSend("IsReady");
                                receivedData.Clear();
                            }
                            else if (DeviceConnected == true)
                            {

                                if (buffer.Length <= 1)
                                {
                                    while (!(SP.BytesToRead > 1)) ;

                                    int fark = SP.BytesToRead;
                                    kalanbuffer = new byte[fark];
                                    SP.Read(kalanbuffer, 0, fark);

                                    receivedData.AddRange(kalanbuffer);

                                }

                                if (receivedData.Count != receivedData[0])
                                {
                                    listByte.Add(receivedData.ToArray());

                                    if (receivedData.Count < receivedData[0])
                                    {
                                        int fark1 = receivedData[0] - receivedData.Count;
                                        while (!(SP.BytesToRead >= fark1)) ;

                                        fark1 = SP.BytesToRead;
                                        kalanbuffer1 = new byte[fark1];
                                        SP.Read(kalanbuffer1, 0, fark1);

                                        receivedData.AddRange(kalanbuffer1);
                                    }
                                }

                                while (true)
                                {
                                    try
                                    {
                                        byte[] _buffer = new byte[receivedData[0] - 1];
                                        Array.Copy(receivedData.ToArray(), 1, _buffer, 0, receivedData[0] - 1);

                                        bool SendOkEnable = false;
                                        if (_buffer[0] == 0x08)
                                            SendOkEnable = true;

                                        Parse(_buffer);

                                        receivedData.RemoveRange(0, receivedData[0]);

                                        if (receivedData.Count > 0)
                                        {
                                            if (receivedData[0] <= receivedData.Count)
                                                continue;
                                            else
                                            {
                                                DataSend(ReGetDataCommand());
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (SendOkEnable)
                                                DataSend("ok");

                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        break;
                                    }

                                }

                                #region a

                                //if (receivedData.Count > receivedData[0])
                                //{
                                //    while (true)
                                //    {
                                //        byte[] _buffer = new byte[receivedData[0]-1];
                                //        Array.Copy(receivedData.ToArray(), 1, _buffer, 0, receivedData[0]-1);

                                //        Parse(_buffer);

                                //        receivedData.RemoveRange(0, receivedData[0]);

                                //        if (receivedData.Count > 0)
                                //        {
                                //            if (receivedData[0] <= receivedData.Count)
                                //                continue;
                                //            else
                                //            {
                                //                DataSend(bmx.ReGetDataCommand());
                                //                break;
                                //            }
                                //        }
                                //        else
                                //        {
                                //            break;
                                //        }    
                                //    }

                                //    DataSend("ok");
                                //}
                                //else
                                //{


                                //byte[] _buffer = new byte[receivedData[2] - 4];
                                //Array.Copy(receivedData.ToArray(), 4, _buffer, 0, receivedData.Count - 4);
                                //DataSend("ok");

                                //byte[] data = new byte[8];
                                //for (int i = 0; i < _buffer.Length; i = i + 8)
                                //{
                                //    try
                                //    {
                                //        Array.Copy(_buffer, i, data, 0, 8);
                                //        //bmx.Handle(data);
                                //        Parse(_buffer);
                                //    }
                                //    catch (Exception ex)
                                //    {

                                //    }
                                //}

                                //DataSend(bmx.ReGetDataCommand());

                                //}
                                //}
                                //else
                                //{
                                //    listByte.Add(receivedData.ToArray());

                                //    byte[] _buffer = new byte[receivedData[0] - 1];
                                //    Array.Copy(receivedData.ToArray(), 1, _buffer, 0, receivedData.Count - 1);

                                //    Parse(_buffer);

                                //    if (_buffer[0] == 0x08)
                                //        DataSend("ok");

                                //}
                                #endregion a
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    SP.DiscardInBuffer();
                    DataSend(ReGetDataCommand());
                }

                receivedData.Clear();

                //tick2 = DateTime.Now.Ticks;
                //timediff = (tick2 - tick1) / 10000.0;
                //timediff = (DateTime.Now.Ticks - tick1) / 10000.0;
                //ListTimediff.Add(timediff);
                //form1.label29.Text = timediff.ToString();
            }


            private void Parse(byte[] data)
            {
                if (data[0] == 0x00)   // Response: 0x00
                {
                    if (data[1] == 0x00)   // Ready
                    {
                        bool accelReady = false;
                        bool accelSelfTest = false;
                        bool gyroReady = false;
                        bool gyroSelfTest = false;
                        bool magnoReady = false;
                        bool magnoSelfTest = false;

                        if (data[2] == 0x01)   // Accel
                        {
                            if (data[3] == 0x01)  // Accel Ready
                            {
                                accelReady = true;

                                if (data[4] == 0x00)  // Accel Self-test success
                                {
                                    accelSelfTest = true;
                                }
                                else                    // Accel Self-test failed
                                {
                                    accelSelfTest = false;
                                }
                            }
                            else if (data[3] == 0x00) // Accel not ready
                            {
                                accelReady = false;
                            }
                        }
                        if (data[5] == 0x02)  // Gyro
                        {
                            if (data[6] == 0x01) // Gyro Ready
                            {
                                gyroReady = true;

                                if (data[7] == 0x00)  // Gyro Self-test Success
                                {
                                    gyroSelfTest = true;
                                }
                                else                   // Gyro Self-test failed
                                {
                                    gyroSelfTest = false;
                                }
                            }
                            else if (data[6] == 0x00) // Gyro not ready
                            {
                                gyroReady = false;
                            }
                        }
                        if (data[8] == 0x04)  // Magno
                        {
                            if (data[9] == 0x01)  // Magno Ready
                            {
                                magnoReady = true;

                                if (data[10] == 0x00) // Magno Selft-test Success   
                                {
                                    magnoSelfTest = true;
                                }
                                else                   // Magno Selft-test failed
                                {
                                    magnoSelfTest = false;
                                }
                            }
                            else if (data[9] == 0x00)
                            {
                                magnoReady = false;
                            }
                        }

                        ConnecResult connecResult = new ConnecResult(true, accelReady, accelSelfTest, gyroReady, gyroSelfTest, magnoReady, magnoSelfTest);
                        SeriportConnectResult(connecResult);
                    }
                    else if (data[1] == 0x01)  // Enable/Disable
                    {
                        if (data[2] == 0x01)   // Accel
                        {
                            bool AccelEnableSucces = false;
                            Enable enable = Enable.Failed;

                            if (data[3] == 0x01)        // Enable
                            {
                                AccelEnableSucces = true;
                                enable = Enable.Enable;
                            }
                            else if (data[3] == 0x02)   // Disable
                            {
                                AccelEnableSucces = true;
                                enable = Enable.Disable;
                            }
                            else if (data[3] == 0x0)     // Failed
                            {
                                AccelEnableSucces = false;
                                enable = Enable.Failed;
                            }

                            SeriportAccelEnableResult(new AccelEnableResult(AccelEnableSucces, enable));
                        }
                        else if (data[2] == 0x02)  // Gyro
                        {
                            bool GyroEnableSuccess = false;
                            Enable enable = Enable.Failed;

                            if (data[3] == 0x01)        // Enable
                            {
                                GyroEnableSuccess = true;
                                enable = Enable.Enable;
                            }
                            else if (data[3] == 0x02)   // Disable
                            {
                                GyroEnableSuccess = true;
                                enable = Enable.Disable;
                            }
                            else if (data[3] == 0x00)    // Failed
                            {
                                GyroEnableSuccess = false;
                                enable = Enable.Failed;
                            }

                            SeriportGyroEnableResult(new GyroEnableResult(GyroEnableSuccess, enable));
                        }
                        else if (data[2] == 0x04)  // Magno
                        {
                            bool MagnoEnableSucccess = false;
                            Enable enable = Enable.Failed;

                            if (data[3] == 0x01)        // Enable
                            {
                                MagnoEnableSucccess = true;
                                enable = Enable.Enable;
                            }
                            else if (data[3] == 0x02)   // Disable
                            {
                                MagnoEnableSucccess = true;
                                enable = Enable.Disable;
                            }
                            else if (data[3] == 0x00)   // Failed
                            {
                                MagnoEnableSucccess = false;
                                enable = Enable.Failed;
                            }

                            SeriportMagnoEnableResult(new MagnoEnableResult(MagnoEnableSucccess, enable));
                        }
                    }
                    else if (data[1] == 0x02)  // Bw
                    {
                        if (data[2] == 0x01)   // Accel
                        {
                            bool AccelBwSuccess = false;
                            AccelBw accelbw = AccelBw.None;

                            if (data[3] == 0x01)   // Success
                            {
                                AccelBwSuccess = true;
                                accelbw = (AccelBw)data[4];
                            }
                            else if (data[3] == 0x00)  // Failed
                            {
                                AccelBwSuccess = false;
                            }

                            SeriportAccelBwResult(new AccelBwResult(AccelBwSuccess, accelbw));

                        }
                        else if (data[2] == 0x02)  // Gyro
                        {
                            bool GyroBwSuccess = false;
                            GyroBw gyroBw = GyroBw.None;

                            if (data[3] == 0x01)   // Success
                            {
                                GyroBwSuccess = true;
                                gyroBw = (GyroBw)data[4];
                            }
                            else if (data[3] == 0x00)  // Failed
                            {
                                GyroBwSuccess = false;
                            }

                            SeriportGyroBwResult(new GyroBwResult(GyroBwSuccess, gyroBw));
                        }
                        else if (data[2] == 0x04)  // Magno
                        {
                            bool MagnoBwSuccess = false;
                            MagnoBw magnoBw = MagnoBw.None;

                            if (data[3] == 0x01)   // Success
                            {
                                MagnoBwSuccess = true;
                                magnoBw = (MagnoBw)data[4];
                            }
                            else if (data[3] == 0x00)  // Failed
                            {
                                MagnoBwSuccess = false;
                            }

                            SeriportMagnoBwResult(new MagnoBwResult(MagnoBwSuccess, magnoBw));
                        }
                    }
                    else if (data[1] == 0x03)  // Range
                    {
                        if (data[2] == 0x01)   // Accel
                        {
                            bool AccelRangeSuccess = false;
                            AccelRange accelRange = AccelRange.None;

                            if (data[3] == 0x01)   // Success
                            {
                                AccelRangeSuccess = true;
                                accelRange = (AccelRange)data[4];
                            }
                            else if (data[3] == 0x00)  // Failed
                            {
                                AccelRangeSuccess = false;
                            }

                            SeriportAccelRangeResult(new AccelRangeResult(AccelRangeSuccess, accelRange));
                        }
                        else if (data[2] == 0x02)  // Gyro
                        {
                            bool GyroRangeSuccess = false;
                            GyroRange gyroRange = GyroRange.None;

                            if (data[3] == 0x01)   // Success
                            {
                                GyroRangeSuccess = true;
                                gyroRange = (GyroRange)data[4];
                            }
                            else if (data[3] == 0x00)  // Failed
                            {
                                GyroRangeSuccess = false;
                            }

                            SeriportGyroRangeResult(new GyroRangeResult(GyroRangeSuccess, gyroRange));
                        }
                    }
                    else if (data[1] == 0x04)  // Read Enable
                    {
                        if (data[2] == 0x01)   // Accel
                        {
                            bool AccelReadEnableSuccess = false;
                            Enable enable = Enable.Failed;

                            if (data[3] == 0x01)   // Enable
                            {
                                AccelReadEnableSuccess = true;
                                enable = Enable.Enable;
                            }
                            else if (data[3] == 0x02)  // Disable
                            {
                                AccelReadEnableSuccess = true;
                                enable = Enable.Disable;
                            }
                            else if (data[3] == 0x00)  // Failed
                            {
                                AccelReadEnableSuccess = false;
                            }

                            SeriportAccelReadEnableResult(new AccelReadEnableResult(AccelReadEnableSuccess, enable));
                        }
                        else if (data[2] == 0x02)  // Gyro
                        {
                            bool GyroReadEnableSuccess = false;
                            Enable enable = Enable.Failed;

                            if (data[3] == 0x01)        // Enable
                            {
                                GyroReadEnableSuccess = true;
                                enable = Enable.Enable;
                            }
                            else if (data[3] == 0x02)   // Disable
                            {
                                GyroReadEnableSuccess = true;
                                enable = Enable.Disable;
                            }
                            else if (data[3] == 0x00)   // Failed
                            {
                                GyroReadEnableSuccess = false;
                            }

                            SeriportGyroReadEnableResult(new GyroReadEnableResult(GyroReadEnableSuccess, enable));
                        }
                        else if (data[2] == 0x04)  // Magno
                        {
                            bool MagnoReadEnableSuccess = false;
                            Enable enable = Enable.Failed;

                            if (data[3] == 0x01)        // Enable
                            {
                                MagnoReadEnableSuccess = true;
                                enable = Enable.Enable;
                            }
                            else if (data[3] == 0x02)   // Disable
                            {
                                MagnoReadEnableSuccess = true;
                                enable = Enable.Disable;
                            }
                            else if (data[3] == 0x00)   // Failed
                            {
                                MagnoReadEnableSuccess = false;
                            }

                            SeriportMagnoReadEnableResult(new MagnoReadEnableResult(MagnoReadEnableSuccess, enable));
                        }
                    }
                }
                else if (data[0] == 0x08)  // Event
                {
                    if (data[1] == 0x00)   // Datas
                    {
                        try
                        {
                            byte[] _buffer = new byte[data.Length - 2];
                            Array.Copy(data, 2, _buffer, 0, data.Length - 2);

                            byte[] _data = new byte[8];
                            for (int i = 0; i < _buffer.Length; i = i + 8)
                            {
                                try
                                {
                                    Array.Copy(_buffer, i, _data, 0, 8);
                                    Handle(_data);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }

            #endregion Seriport





            /// <summary>
            /// Madül ile bağlatı kurar.
            /// (Madülle bağlantı kurmadan önce "Open" methodu ile seriportu açın)
            /// </summary>
            /// <returns></returns>
            public result Connect()
            {
                result rslt = new result();
                rslt = DataSend("Hi");
                return rslt;

            }

            /// <summary>
            /// Madül ile bağlantıyı sonlandırır ama seriportu kapatmaz.
            /// Madüle ile bağlantı sonlandırılırdığı takdirde, modülden veri
            /// alınmaz. Configurasyon yapılamaz. Bunun için tekrar "Connect" methodu ile
            /// bağlantı kurulması gerekir.
            /// </summary>
            /// <returns></returns>
            public result Disconnect()
            {
                result rslt = new result();


                return rslt;
            }


            #region Enable
            /// <summary>
            /// Accelerometer'ı aktif eder.
            /// </summary>
            /// <param name="enable"></param>
            /// <returns></returns>
            public result AccelEnable(Enable enable)
            {
                result rslt = new result();
                rslt = DataSend(EnableAccelCommand(enable));
                return rslt;
            }

            /// <summary>
            /// Gyroscope'u aktif eder.
            /// </summary>
            /// <param name="enable"></param>
            /// <returns></returns>
            public result GyroEnable(Enable enable)
            {
                result rslt = new result();
                rslt = DataSend(EnableGyroCommand(enable));
                return rslt;
            }

            /// <summary>
            /// Magnotometer'ı aktif eder.
            /// </summary>
            /// <param name="enable"></param>
            /// <returns></returns>
            public result MagnoEnable(Enable enable)
            {
                result rslt = new result();
                rslt = DataSend(EnableMagnoCommand(enable));
                return rslt;
            }
            #endregion Enable


            #region Bw
            /// <summary>
            /// Accelerometer'ın okuma frekansını ayarlar.
            /// </summary>
            /// <param name="accelBw"></param>
            /// <returns></returns>
            public result AccelSetBw(AccelBw accelBw)
            {
                result rslt = new result();
                rslt = DataSend(BwAccelCommand(accelBw));
                return rslt;
            }

            /// <summary>
            /// Gyroscope'un okuma frekansını ayarlar.
            /// </summary>
            /// <param name="gyroBw"></param>
            /// <returns></returns>
            public result GyroSetBw(GyroBw gyroBw)
            {
                result rslt = new result();
                rslt = DataSend(BwGyroCommand(gyroBw));
                return rslt;
            }

            /// <summary>
            /// Magnotometer'ın okuma frekansını ayarlar.
            /// </summary>
            /// <param name="magnoBw"></param>
            /// <returns></returns>
            public result MagnoSetBw(MagnoBw magnoBw)
            {
                result rslt = new result();
                rslt = DataSend(BwMagnoCommand(magnoBw));
                return rslt;
            }
            #endregion Bw


            #region Range
            /// <summary>
            /// Accelerometer'ın çalışma aralığını (range) ayarlar.
            /// </summary>
            /// <param name="accelRange"></param>
            /// <returns></returns>
            public result AccelSetRange(AccelRange accelRange)
            {
                result rslt = new result();
                rslt = DataSend(RangeAccelCommand(accelRange));
                return rslt;
            }

            /// <summary>
            /// Gyroscope'un çalışma aralığını (range) ayarlar.
            /// </summary>
            /// <param name="gyroRange"></param>
            /// <returns></returns>
            public result GyroSetRange(GyroRange gyroRange)
            {
                result rslt = new result();
                rslt = DataSend(RangeGyroCommand(gyroRange));
                return rslt;
            }
            #endregion Range


            #region ReadEnable
            public result AccelReadEnable(Enable enable)
            {
                result rslt = new result();
                rslt = DataSend(ReadEnableAccelCommand(enable));
                return rslt;
            }

            public result GyroReadEnable(Enable enable)
            {
                result rslt = new result();
                rslt = DataSend(ReadEnableGyroCommand(enable));
                return rslt;
            }

            public result MagnoReadEnable(Enable enable)
            {
                result rslt = new result();
                rslt = DataSend(ReadEnableMagnoCommand(enable));
                return rslt;
            }
            #endregion ReadEnable

            /// <summary>
            /// Aktif edilmiş sensörlerden, verilerin gönerilmesini başlatır.
            /// </summary>
            /// <returns></returns>
            public result Start(Enable AccelReadEnable, Enable GyroReadEnable, Enable MAgnoReadEnable)
            {
                result rslt = new result();
                rslt = DataSend(StartCommand(AccelReadEnable, GyroReadEnable, MAgnoReadEnable));
                return rslt;
            }

            /// <summary>
            /// Aktif edilmiş sönserlerden, verilerin gönderilmesini durdurur.
            /// </summary>
            /// <returns></returns>
            public result Stop()
            {
                result rslt = new result();
                rslt = DataSend(StopCommand());
                return rslt;
            }



            #region Delegate
            //Connect
            public delegate void ConnectResultEventHandler(ConnecResult e);
            public class ConnecResult
            {
                public readonly bool ConnectionSuccess;
                public readonly bool AccelReady;
                public readonly bool AccelSelfTest;
                public readonly bool GyroReady;
                public readonly bool GyroSelfTest;
                public readonly bool MagnoReady;
                public readonly bool MagnoSelfTest;
                public ConnecResult(bool ConnectionSuccess, bool AccelReady, bool AccelSeflTest, bool GyroReady, bool GyroSelfTest, bool MagnoReady, bool MagnoSelfTest)
                {
                    this.ConnectionSuccess = ConnectionSuccess;
                    this.AccelReady = AccelReady;
                    this.AccelSelfTest = AccelSeflTest;
                    this.GyroReady = GyroReady;
                    this.GyroSelfTest = GyroSelfTest;
                    this.MagnoReady = MagnoReady;
                    this.MagnoSelfTest = MagnoSelfTest;
                }
            }

            //Enable 
            public delegate void AccelEnableEventHandler(AccelEnableResult e);
            public class AccelEnableResult
            {
                public readonly bool Success;
                public readonly Optoel.Enable Enable;

                public AccelEnableResult(bool success, Optoel.Enable Enable)
                {
                    this.Success = success;
                    this.Enable = Enable;
                }
            }

            public delegate void GyroEnableEventHandler(GyroEnableResult e);
            public class GyroEnableResult
            {
                public readonly bool Success;
                public readonly Enable Enable;

                public GyroEnableResult(bool success, Enable enable)
                {
                    this.Success = success;
                    this.Enable = enable;
                }
            }

            public delegate void MagnoEnableEventHandler(MagnoEnableResult e);
            public class MagnoEnableResult
            {
                public readonly bool Success;
                public readonly Enable Enable;
                public MagnoEnableResult(bool success, Enable enable)
                {
                    this.Success = success;
                    this.Enable = enable;
                }
            }

            //Bw
            public delegate void AccelBwEventHandler(AccelBwResult e);
            public class AccelBwResult
            {
                public readonly bool Success;
                public readonly AccelBw AccelBw;
                public AccelBwResult(bool success, AccelBw accelBw)
                {
                    this.Success = success;
                    this.AccelBw = accelBw;
                }
            }

            public delegate void GyroBwEventHandler(GyroBwResult e);
            public class GyroBwResult
            {
                public readonly bool Success;
                public readonly GyroBw GyroBw;
                public GyroBwResult(bool success, GyroBw gyroBw)
                {
                    this.Success = success;
                    this.GyroBw = gyroBw;
                }
            }

            public delegate void MagnoBwEventHandler(MagnoBwResult e);
            public class MagnoBwResult
            {
                public readonly bool Success;
                public readonly MagnoBw MagnoBw;
                public MagnoBwResult(bool success, MagnoBw magnoBw)
                {
                    this.Success = success;
                    this.MagnoBw = magnoBw;
                }
            }

            //Range
            public delegate void AccelRangeEventHandler(AccelRangeResult e);
            public class AccelRangeResult
            {
                public readonly bool Success;
                public readonly AccelRange accelRange;
                public AccelRangeResult(bool success, AccelRange accelRange)
                {
                    this.Success = success;
                    this.accelRange = accelRange;
                }
            }

            public delegate void GyroRangeEventHandler(GyroRangeResult e);
            public class GyroRangeResult
            {
                public readonly bool Success;
                public readonly GyroRange gyroRange;
                public GyroRangeResult(bool success, GyroRange gyroRange)
                {
                    this.Success = success;
                    this.gyroRange = gyroRange;
                }
            }

            //Read Enable
            public delegate void AccelReadEnableEventHandler(AccelReadEnableResult e);
            public class AccelReadEnableResult
            {
                public readonly bool Success;
                public readonly Enable Enable;
                public AccelReadEnableResult(bool success, Enable enable)
                {
                    this.Success = success;
                    this.Enable = enable;
                }
            }

            public delegate void GyroReadEnableEventHandler(GyroReadEnableResult e);
            public class GyroReadEnableResult
            {
                public readonly bool Success;
                public readonly Enable Enable;
                public GyroReadEnableResult(bool success, Enable enable)
                {
                    this.Success = success;
                    this.Enable = enable;
                }
            }

            public delegate void MagnoReadEnableEventHandler(MagnoReadEnableResult e);
            public class MagnoReadEnableResult
            {
                public readonly bool Success;
                public readonly Enable Enable;
                public MagnoReadEnableResult(bool success, Enable enable)
                {
                    this.Success = success;
                    this.Enable = enable;
                }
            }

            //Datas
            public delegate void AccelDataEventHandler(Accel Accel);
            public delegate void GyroDataEventHandler(Gyro Gyro);
            public delegate void MagnoDataEventHandler(Magno Magno);


            // Error
            public delegate void ErrorEventHandler();
            #endregion Delegate


            #region Events
            //Connect
            public event ConnectResultEventHandler SeriportConnectResult;

            //Enable
            public event AccelEnableEventHandler SeriportAccelEnableResult;
            public event GyroEnableEventHandler SeriportGyroEnableResult;
            public event MagnoEnableEventHandler SeriportMagnoEnableResult;

            //Bw
            public event AccelBwEventHandler SeriportAccelBwResult;
            public event GyroBwEventHandler SeriportGyroBwResult;
            public event MagnoBwEventHandler SeriportMagnoBwResult;

            //Range
            public event AccelRangeEventHandler SeriportAccelRangeResult;
            public event GyroRangeEventHandler SeriportGyroRangeResult;

            //Read Enable
            public event AccelReadEnableEventHandler SeriportAccelReadEnableResult;
            public event GyroReadEnableEventHandler SeriportGyroReadEnableResult;
            public event MagnoReadEnableEventHandler SeriportMagnoReadEnableResult;

            //Datas
            public event AccelDataEventHandler SeriportAccelData;
            public event GyroDataEventHandler SeriportGyroData;
            public event MagnoDataEventHandler SeriportMagnoData;
            #endregion Events




            #region BMX055

            private readonly List<Accel> _listAccel = new List<Accel>();
            private readonly List<Gyro> _listGyro = new List<Gyro>();
            private readonly List<Magno> _listMAgno = new List<Magno>();

            public List<Accel> ListAccel { get { return _listAccel; } }
            public List<Gyro> ListGyro { get { return _listGyro; } }
            public List<Magno> ListMagno { get { return _listMAgno; } }

            public int AccelCurrentDataNumber = 0;
            public int GyroCurrentDataNumber = 0;
            public int MagnoCurrentDataNumber = 0;

            public int AccelKacanDataSayisi = 0;
            public int GyroKacanDataSayisi = 0;
            public int MagnoKacanDataSayisi = 0;


            public void Handle(byte[] val)
            {
                if ((val[7] & 0x40) == 0x40)   //gyro
                {
                    byte[] by = new byte[2];

                    int GyroData1 = BitConverter.ToInt16(new byte[] { val[0], val[1] }, 0);
                    int GyroData2 = BitConverter.ToInt16(new byte[] { val[2], val[3] }, 0);
                    int GyroData3 = BitConverter.ToInt16(new byte[] { val[4], val[5] }, 0);
                    int GyroDataNumber = (int)(val[6] + (val[7] << 8));

                    float rawX = (float)(GyroData1 * 0.61);
                    float rawY = (float)(GyroData2 * 0.61);
                    float rawZ = (float)(GyroData3 * 0.61);
                    int gyroDataNumber = (GyroDataNumber & 0x3FFF);

                    Gyro gyro = new Gyro(rawX, rawY, rawZ, gyroDataNumber);
                    SeriportGyroData(gyro);


                    if (gyroDataNumber != GyroCurrentDataNumber)
                    {
                        GyroKacanDataSayisi += gyroDataNumber - GyroCurrentDataNumber;
                        GyroCurrentDataNumber = gyroDataNumber;
                    }

                    GyroCurrentDataNumber++;
                    if (GyroCurrentDataNumber >= 16000)
                    {
                        GyroCurrentDataNumber = 0;
                    }

                    ListGyro.Add(gyro);
                }
                else if ((val[7] & 0x80) == 0x80)   //magno
                {
                    int MagnoData1 = Convert.ToInt32((Int16)(val[0] + (val[1] << 8)) >> 3);
                    int MagnoData2 = Convert.ToInt32((Int16)(val[2] + (val[3] << 8)) >> 3);
                    int MagnoData3 = Convert.ToInt32((Int16)(val[4] + (val[5] << 8)) >> 1);
                    int MagnoDataNumber = (int)(val[6] + (val[7] << 8));



                    float rawX = (float)(MagnoData1);
                    float rawY = (float)(MagnoData2);
                    float rawZ = (float)(MagnoData3);
                    int magnoDataNumber = (MagnoDataNumber & 0x3FFF);


                    Magno magno = new Magno(rawX, rawY, rawZ, magnoDataNumber);
                    SeriportMagnoData(magno);

                    if (magnoDataNumber != MagnoCurrentDataNumber)
                    {
                        MagnoKacanDataSayisi += magnoDataNumber - MagnoCurrentDataNumber;
                        MagnoCurrentDataNumber = magnoDataNumber;
                    }

                    MagnoCurrentDataNumber++;
                    if (MagnoCurrentDataNumber >= 16000)
                    {
                        MagnoCurrentDataNumber = 0;
                    }

                    ListMagno.Add(magno);
                }
                else   //accel
                {
                    try
                    {
                        Int16 AccelData1 = (Int16)((Int16)(val[0] | (val[1] << 8)) >> 4);
                        Int16 AccelData2 = (Int16)((Int16)(val[2] | (val[3] << 8)) >> 4);
                        Int16 AccelData3 = (Int16)((Int16)(val[4] | (val[5] << 8)) >> 4);
                        int AccelDataNumber = (int)(val[6] + (val[7] << 8));

                        float rawX = (float)(AccelData1 * 0.98);
                        float rawY = (float)(AccelData2 * 0.98);
                        float rawZ = (float)(AccelData3 * 0.98);
                        int accelDataNumber = (AccelDataNumber & 0x3FFF);

                        Accel accel = new Accel(rawX, rawY, rawZ, accelDataNumber);
                        SeriportAccelData(accel);


                        if (AccelDataNumber != AccelCurrentDataNumber)
                        {
                            AccelKacanDataSayisi += AccelDataNumber - AccelCurrentDataNumber;
                            AccelCurrentDataNumber = AccelDataNumber;
                        }

                        AccelCurrentDataNumber++;
                        if (AccelCurrentDataNumber >= 16000)
                        {
                            AccelCurrentDataNumber = 0;
                        }

                        ListAccel.Add(accel);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            #endregion BMX055

        }




        public class result
        {
            public bool Succes = false;
            public string Message = "";
        }







        public class Ble
        {

            System.IO.Ports.SerialPort SP = new System.IO.Ports.SerialPort();
            Bluegiga.BGLib bglib = new Bluegiga.BGLib();
            //System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            System.Timers.Timer timer = new System.Timers.Timer();
            System.Timers.Timer timerDongle = new System.Timers.Timer();


            public Ble()
            {
                ErrorCodeInit();

                timer.Enabled = false;
                timer.Stop();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Tick);

                timerDongle.Enabled = false;
                timerDongle.Stop();
                timerDongle.Interval = 2000;
                timerDongle.Elapsed += new System.Timers.ElapsedEventHandler(this.timerDongle_Tick);

                bglib.BLEResponseSystemHello += new Bluegiga.BLE.Responses.System.HelloEventHandler(HelloResponse);
                bglib.BLEResponseSystemAddressGet += new Bluegiga.BLE.Responses.System.AddressGetEventHandler(AddressGetResponse);
                bglib.BLEResponseSystemGetConnections += new Bluegiga.BLE.Responses.System.GetConnectionsEventHandler(GetConnectionsResponse);

                bglib.BLEResponseGAPDiscover += new Bluegiga.BLE.Responses.GAP.DiscoverEventHandler(GAPDiscoverResponse);
                bglib.BLEEventGAPScanResponse += new Bluegiga.BLE.Events.GAP.ScanResponseEventHandler(this.GAPScanResponseEvent);

                bglib.BLEResponseGAPConnectDirect += new Bluegiga.BLE.Responses.GAP.ConnectDirectEventHandler(GapConnectDirectResponse);
                bglib.BLEEventConnectionStatus += new Bluegiga.BLE.Events.Connection.StatusEventHandler(ConnectionStatusEvent);

                bglib.BLEEventATTClientFindInformationFound += new Bluegiga.BLE.Events.ATTClient.FindInformationFoundEventHandler(ATTClientFindInformationFoundEvent);
                bglib.BLEEventATTClientProcedureCompleted += new Bluegiga.BLE.Events.ATTClient.ProcedureCompletedEventHandler(ATTClientProcedureCompletedEvent);

                bglib.BLEResponseATTClientFindInformation += new Bluegiga.BLE.Responses.ATTClient.FindInformationEventHandler(ATTClientFindInformationFoundResponse);
                //bglib.BLEEventATTClientAttributeValue += new Bluegiga.BLE.Events.ATTClient.AttributeValueEventHandler(ATTClientAttributeValueEvent);

                bglib.BLEResponseConnectionGetRssi += new Bluegiga.BLE.Responses.Connection.GetRssiEventHandler(ConnectionGetRssiResponse);

                bglib.BLEResponseConnectionUpdate += new Bluegiga.BLE.Responses.Connection.UpdateEventHandler(ConnectionUpdateResponse);
            }



            void HelloResponse(object sender, HelloEventArgs e)
            {
                bglib.SendCommand(SP, bglib.BLECommandSystemAddressGet());

                //DongleConnected = true;
            }

            void AddressGetResponse(object sender, AddressGetEventArgs e)
            {
                _DongleAddressHex = e.address;
                _DongleAddress = MacAddrConvert(e.address);
                bglib.SendCommand(SP, bglib.BLECommandSystemGetConnections());
            }

            void GetConnectionsResponse(object sender, GetConnectionsEventArgs e)
            {
                _MaxSupportedConnections = e.maxconn;
                //DongleConnected = true;
                dongleConnecte = dongleConnected.Connected;
            }


            #region Serial

            byte[] _DongleAddressHex;
            string _DongleAddress;
            public byte[] DongleAddressHex { get { return _DongleAddressHex; } }
            /// <summary>
            /// USB Dongle'ın Mac Address'i.
            /// </summary>
            public string DongleAddress { get { return _DongleAddress; } }

            byte _MaxSupportedConnections;
            /// <summary>
            /// Desteklenen max. cihaz sayısı.
            /// </summary>
            public byte MaxSupportedConnections { get { return _MaxSupportedConnections; } }


            /// <summary>
            /// Seriport açık mı?
            /// </summary>
            public bool IsOpen
            {
                get
                {
                    if (!(SP.IsOpen))
                    {
                        //DongleConnected = false;
                        dongleConnecte = dongleConnected.None;
                        _ConnectMessage = "Connection lost with dongle!";
                    }

                    return SP.IsOpen;
                }
            }


            string _ConnectMessage = "";
            /// <summary>
            /// Seriport bağlantı durum mesajı.
            /// </summary>
            public string ConnectMessage { get { return _ConnectMessage; } }


            //bool DongleConnected = false;
            //bool getDongleMacAddress = false;
            enum dongleConnected
            {
                None = 0,
                Connected = 1,
                NotConnected = 2
            };
            dongleConnected dongleConnecte = dongleConnected.None;

            private void timerDongle_Tick(object sender, ElapsedEventArgs e)
            {
                timerDongle.Stop();
                timerDongle.Enabled = false;
                dongleConnecte = dongleConnected.NotConnected;
            }

            /// <summary>
            /// Seriportu açar. (BaudRade: 921600)
            /// </summary>
            /// <param name="Com"></param>
            /// <returns></returns>
            public async Task<result> OpenAsync(string Com)
            {
                result rslt = new result();
                dongleConnecte = dongleConnected.None;

                Task task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (SP.IsOpen)
                        {
                            rslt.Succes = false;
                            rslt.Message = _ConnectMessage = "Dongle already connected!";
                        }
                        else
                        {
                            SP = new System.IO.Ports.SerialPort();
                            //DongleConnected = false;

                            SP.PortName = Com;
                            SP.BaudRate = 921600;
                            SP.Parity = Parity.None;
                            SP.StopBits = StopBits.One;
                            SP.DataBits = 8;
                            SP.Handshake = Handshake.None;

                            SP.DataReceived += SP_DataReceived;

                            SP.Open();
                            SP.DiscardInBuffer();

                            bglib.SendCommand(SP, bglib.BLECommandSystemHello());

                            timerDongle.Enabled = true;
                            timerDongle.Start();

                            while (true)
                            {
                                if (dongleConnecte == dongleConnected.Connected)
                                {
                                    rslt.Succes = true;
                                    rslt.Message = _ConnectMessage = "Dongle connected.";
                                    break;
                                }
                                else if (dongleConnecte == dongleConnected.NotConnected)
                                {
                                    rslt.Succes = false;
                                    rslt.Message = _ConnectMessage = "Could not communicate with the dongle!";
                                    SP.Close();
                                    break;
                                }
                            }

                            //bglib.SendCommand(SP, bglib.BLECommandSystemAddressGet());
                            //bglib.SendCommand(SP, bglib.BLECommandSystemGetConnections());



                        }
                    }
                    catch (Exception ex)
                    {
                        rslt.Succes = false;
                        rslt.Message = _ConnectMessage = "Dongle failed to connect!\n" + ex.Message;
                    }
                });
                await task;

                return rslt;
            }


            public result Open(string Com)
            {
                result rslt = new result();
                dongleConnecte = dongleConnected.None;

                try
                {
                    if (SP.IsOpen)
                    {
                        rslt.Succes = false;
                        rslt.Message = _ConnectMessage = "Dongle already connected!";
                    }
                    else
                    {
                        SP = new System.IO.Ports.SerialPort();
                        //DongleConnected = false;

                        SP.PortName = Com;
                        SP.BaudRate = 921600;
                        SP.Parity = Parity.None;
                        SP.StopBits = StopBits.One;
                        SP.DataBits = 8;
                        SP.Handshake = Handshake.None;

                        SP.DataReceived += SP_DataReceived;

                        SP.Open();
                        SP.DiscardInBuffer();

                        bglib.SendCommand(SP, bglib.BLECommandSystemHello());

                        timerDongle.Enabled = true;
                        timerDongle.Start();

                        while (true)
                        {
                            if (dongleConnecte == dongleConnected.Connected)
                            {
                                rslt.Succes = true;
                                rslt.Message = _ConnectMessage = "Dongle connected.";
                                break;
                            }
                            else if (dongleConnecte == dongleConnected.NotConnected)
                            {
                                rslt.Succes = false;
                                rslt.Message = _ConnectMessage = "Could not communicate with the dongle!";
                                SP.Close();
                                break;
                            }
                        }

                        //bglib.SendCommand(SP, bglib.BLECommandSystemAddressGet());
                        //bglib.SendCommand(SP, bglib.BLECommandSystemGetConnections());



                    }
                }
                catch (Exception ex)
                {
                    rslt.Succes = false;
                    rslt.Message = _ConnectMessage = "Dongle failed to connect!\n" + ex.Message;
                }


                return rslt;
            }

            /// <summary>
            /// Seriportu kapatır.
            /// </summary>
            /// <returns></returns>
            public result Close()
            {
                result rslt = new result();

                try
                {
                    if (SP.IsOpen)
                    {
                        SP.Close();
                        //DongleConnected = false;
                        dongleConnecte = dongleConnected.None;

                        rslt.Succes = true;
                        rslt.Message = _ConnectMessage = "Bağlantı sonlandırıldı!";
                    }
                    else
                    {
                        rslt.Succes = true;
                    }
                }
                catch (Exception ex)
                {

                    rslt.Succes = false;
                    rslt.Message = _ConnectMessage = ex.Message;
                }

                return rslt;
            }


            void SP_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                Byte[] buffer = new Byte[SP.BytesToRead];

                // read all available bytes from serial port in one chunk
                SP.Read(buffer, 0, buffer.Length);

                // parse all bytes read through BGLib parser
                for (int i = 0; i < buffer.Length; i++)
                {
                    bglib.Parse(buffer[i]);
                }
            }

            #endregion Serial




            #region Ble Discover
            void timer_Tick(object sender, EventArgs e)
            {
                timer.Stop();
                timer.Enabled = false;
                DiscoverStop();
            }

            /// <summary>
            /// Scan işlemini başlatır. "Timeout" saniye cinsinden bir parametredir.
            /// "Timeout = 0" ise scan işlemini durdurmak için "DiscoverStop" methodu kullanılmalı.
            /// "Timeout" sıfırdan farklı bir değer ise, Timeout süresi sonunda scan işlemi otomatik sonlandırılır.
            /// </summary>
            /// <param name="Timeout">(secand)</param>
            public void DiscoverStart(int Timeout)
            {
                ListDevice.Clear();
                bglib.SendCommand(SP, bglib.BLECommandGAPDiscover(1)); // **** 1 ne anlama geliyor bak. ****
                if (Timeout != 0)
                {
                    timer.Interval = Timeout * 1000;
                    timer.Enabled = true;
                    timer.Start();
                }
            }

            /// <summary>
            /// Scan işlemini sonlandırır.
            /// </summary>
            public void DiscoverStop()
            {
                bglib.SendCommand(SP, bglib.BLECommandGAPEndProcedure());

                timer.Stop();
                timer.Enabled = false;

                if (BleDiscoverResult != null)
                    BleDiscoverResult(ListDevice);
            }

            void GAPDiscoverResponse(object sender, DiscoverEventArgs e)
            {
                if (e.result != 0)
                {
                    errorCode errorCode = (errorCode)e.result;
                    string errorMessage = ErrorCode[errorCode];
                    ErrorHandleArgs error = new ErrorHandleArgs(errorCode, errorMessage);

                    if (BleErrorEvent != null)
                        BleErrorEvent(error);
                }
            }

            string DEVICE_NAME = "Ble Optoel";
            void GAPScanResponseEvent(object sender, ScanResponseEventArgs e)
            {
                try
                {
                    byte[] device_name = new byte[10];
                    Array.Copy(e.data, 9, device_name, 0, 10);

                    if (Encoding.ASCII.GetString(device_name) == DEVICE_NAME)
                    {
                        Device device = new Device(e.address_type, e.sender, Encoding.ASCII.GetString(device_name));

                        if (ListDevice.Exists(x => x.MAC_Address == MacAddrConvert(e.sender)) == false)
                        {
                            ListDevice.Add(device);

                            if (BleDiscoverDeivce != null)
                                BleDiscoverDeivce(device);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            string MacAddrConvert(byte[] mac)
            {
                string MAC = "";
                for (int i = 5; 0 <= i; i--)
                {
                    MAC += mac[i].ToString("X") + ":";
                }
                MAC = MAC.Remove(MAC.Length - 1, 1);

                return MAC;
            }

            #endregion Ble Discover



            #region Ble Connect

            protected class Slave
            {
                public readonly byte ConnectionHandle;
                public readonly byte flags;
                public readonly byte[] Address;
                public readonly byte Address_Type;
                public readonly UInt16 ConnInterval = 0;
                public readonly UInt16 Timeout = 0;
                public readonly UInt16 Latency = 0;
                public Slave(byte connHandle, byte flags, byte[] address, byte address_type, UInt16 connInterval, UInt16 timeout, UInt16 latency)
                {
                    this.ConnectionHandle = connHandle;
                    this.flags = flags;
                    this.Address = address;
                    this.Address_Type = address_type;
                    this.ConnInterval = connInterval;
                    this.Timeout = timeout;
                    this.Latency = latency;
                }
            }

            bool ConnectToDeviceSuccess = false;
            /// <summary>
            /// Herhnagi bir Optoel cihazıyla bağlantı kurar.
            /// </summary>
            /// <param name="device"></param>
            public void ConnectToDevice(Device device)
            {
                bglib.SendCommand(SP, bglib.BLECommandGAPConnectDirect(device.MAC_AddressHex, device.Address_Type, 15, 100, 1000, 0));
                this.device = device;
            }

            Slave slave;
            BleSlave bleSlave;
            List<_Handle> listHandle;
            Device device;
            byte BleConnectionHandle;
            void GapConnectDirectResponse(object sender, ConnectDirectEventArgs e)
            {
                if (e.result != 0)
                {
                    Ble.errorCode errorCode = (Ble.errorCode)e.result;
                    string errorMessage = ErrorCode[errorCode];
                    ErrorHandleArgs error = new ErrorHandleArgs(errorCode, errorMessage);

                    if (BleErrorEvent != null)
                        BleErrorEvent(error);
                }
                else
                {
                    BleConnectionHandle = e.connection_handle;
                }
            }

            void ConnectionStatusEvent(object sender, StatusEventArgs e)
            {
                if (e.connection == BleConnectionHandle)
                {
                    if (e.flags == 5)
                    {
                        slave = new Slave(e.connection, e.flags, e.address, e.address_type, e.conn_interval, e.timeout, e.latency);
                        GetUuid(slave);
                        listHandle = new List<_Handle>();
                    }
                }
            }

            void GetUuid(Slave slave)
            {
                bglib.SendCommand(SP, bglib.BLECommandATTClientFindInformation(slave.ConnectionHandle, 0x0001, 0xFFFF));
            }

            void ATTClientFindInformationFoundResponse(object sender, FindInformationEventArgs e)
            {
                if (e.connection == BleConnectionHandle)
                {
                    if (e.result != 0)
                    {
                        Ble.errorCode errorCode = (Ble.errorCode)e.result;
                        string errorMessage = ErrorCode[errorCode];
                        ErrorHandleArgs error = new ErrorHandleArgs(errorCode, errorMessage);

                        if (BleErrorEvent != null)
                            BleErrorEvent(error);
                    }
                }
            }

            void ATTClientFindInformationFoundEvent(object sender, FindInformationFoundEventArgs e)
            {
                if (BleConnectionHandle == e.connection)
                {
                    _Handle handle = new _Handle();
                    handle.Handle = e.chrhandle;
                    handle.Uuid = (ushort)(e.uuid[0] + (e.uuid[1] << 8));
                    listHandle.Add(handle);
                }
            }

            int count = 0;
            void ATTClientProcedureCompletedEvent(object sender, ProcedureCompletedEventArgs e)
            {
                if (e.result != 0)
                {
                    Ble.errorCode errorCode = (Ble.errorCode)e.result;
                    string errorMessage = ErrorCode[errorCode];
                    ErrorHandleArgs error = new ErrorHandleArgs(errorCode, errorMessage);

                    if (BleSlaveErrorEvent != null)
                        BleSlaveErrorEvent(new SlaveErrorEventArgs(e.connection, error));
                }
                else
                {
                    if ((e.connection == BleConnectionHandle) && e.chrhandle == 36)
                    {
                        bleSlave = new BleSlave(slave, listHandle, SP, bglib, device, this);

                        bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, 32, new byte[] { 0x01, 0x00 }));

                    }
                    else if ((e.connection == BleConnectionHandle) && e.chrhandle == 32)
                    {
                        if (e.result == 0)
                        {
                            bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, 35, new byte[] { 0x01, 0x00 }));
                        }
                        else
                        {
                            if (count < 3)
                            {
                                bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, 32, new byte[] { 0x01, 0x00 }));
                                count++;
                            }
                            else
                            {
                                Ble.errorCode errorCode = (Ble.errorCode)e.result;
                                string errorMessage = ErrorCode[errorCode];
                                ErrorHandleArgs error = new ErrorHandleArgs(errorCode, errorMessage);
                                BleErrorEvent(error);
                            }
                        }
                    }
                    else if ((e.connection == BleConnectionHandle) && e.chrhandle == 35)
                    {
                        if (e.result == 0)
                        {
                            if (BleConnectResult != null)
                                BleConnectResult(bleSlave);
                        }
                        else
                        {
                            if (count < 3)
                            {
                                bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, 35, new byte[] { 0x01, 0x00 }));
                                count++;
                            }
                            else
                            {
                                Ble.errorCode errorCode = (Ble.errorCode)e.result;
                                string errorMessage = ErrorCode[errorCode];
                                ErrorHandleArgs error = new ErrorHandleArgs(errorCode, errorMessage);
                                BleErrorEvent(error);
                            }
                        }
                    }
                }
            }

            class _Handle
            {
                public UInt16 Handle;
                public UInt16 Uuid;
            }

            void ConnectionUpdateResponse(object sender, UpdateEventArgs e)
            {
                if (e.result != 0)
                {

                }
            }

            #endregion Ble Connect



            void ConnectionGetRssiResponse(object sender, GetRssiEventArgs e)
            {
                if (BleGetRssi != null)
                    BleGetRssi(e);
            }



            #region Delegagtes

            public delegate void BleDiscoverResultEventHandler(List<Device> e);
            public delegate void BleDiscoverDeviceEventHandler(Device e);
            List<Device> ListDevice = new List<Device>();
            public class Device
            {
                public readonly byte Address_Type;
                public readonly byte[] MAC_AddressHex;
                public readonly string MAC_Address;
                public readonly string DeviceName;
                public Device(byte addr_type, byte[] mac_addr, string devicename)
                {
                    this.Address_Type = addr_type;
                    this.MAC_AddressHex = mac_addr;
                    this.DeviceName = devicename;

                    for (int i = 5; 0 <= i; i--)
                    {
                        MAC_Address += mac_addr[i].ToString("X") + ":";
                    }
                    MAC_Address = MAC_Address.Remove(MAC_Address.Length - 1, 1);
                }
            }

            public delegate void BleConnectResultEventHandler(BleSlave e);

            //delegate void BleATTClientAttributeValueEvent(AttributeValueEventArgs e);

            //delegate void BleConnectionDisconnectedEventHandler(DisconnectedEventArgs e);

            delegate void BleGetRSSIEventHandler(GetRssiEventArgs e);


            public delegate void BleErrorEventHandler(ErrorHandleArgs Error);
            public class ErrorHandleArgs
            {
                public readonly errorCode ErrorCode;
                public readonly string ErrorMessage;
                public ErrorHandleArgs(errorCode errorCode, string errorMessage)
                {
                    this.ErrorCode = errorCode;
                    this.ErrorMessage = errorMessage;
                }
            }

            delegate void BleSlaveErrorEventHandler(SlaveErrorEventArgs e);
            class SlaveErrorEventArgs
            {
                public readonly byte Connection;
                public ErrorHandleArgs ErrorHandle;
                public SlaveErrorEventArgs(byte connection, ErrorHandleArgs errorHandle)
                {
                    this.Connection = connection;
                    this.ErrorHandle = errorHandle;
                }
            }


            //Enable/Disable
            public delegate void AccelEnableEventHandler(BleSlave sender, BleSlave.AccelEnableResultArgs e);
            public delegate void GyroEnableEventHandler(BleSlave sender, BleSlave.GyroEnableResultArgs e);
            public delegate void MagnoEnableEventHandler(BleSlave sender, BleSlave.MagnoEnableResultArgs e);

            //Bw
            public delegate void AccelBwEventHandler(BleSlave sender, BleSlave.AccelBwResultArgs e);
            public delegate void GyroBwEventHandler(BleSlave sender, BleSlave.GyroBwResultArgs e);
            public delegate void MagnoBwEventHandler(BleSlave sender, BleSlave.MagnoBwResultArgs e);

            //Range
            public delegate void AccelRangeEventHandler(BleSlave sender, BleSlave.AccelRangeResultArgs e);
            public delegate void GyroRangeEventHandler(BleSlave sender, BleSlave.GyroRangeResultArgs e);

            //Read Enable
            public delegate void AccelReadEnableEventHandler(BleSlave sender, BleSlave.AccelReadEnableResultArgs e);
            public delegate void GyroReadEnableEventHandler(BleSlave sender, BleSlave.GyroReadEnableResultArgs e);
            public delegate void MagnoReadEnableEventHandler(BleSlave sender, BleSlave.MagnoReadEnableResultArgs e);

            //Data
            public delegate void AccelDataEventHandler(BleSlave sender, BleSlave.Accel Accel);
            public delegate void GyroDataEventHandler(BleSlave sender, BleSlave.Gyro Gyro);
            public delegate void MagnoDataEventHandler(BleSlave sender, BleSlave.Magno Magno);


            public delegate void DisconnectedEventHandler(BleSlave sender, BleSlave.DisconnectedEventArgs e);
            public delegate void GetRSSIEventHandler(BleSlave sender, BleSlave.GetRSSIArgs e);
            public delegate void SlaveErrorEventHandler(BleSlave sender, ErrorHandleArgs e);

            #endregion Delegates



            #region Events

            /// <summary>
            /// Scan işlemi bittiğinde fırlatılır.
            /// </summary>
            public event BleDiscoverResultEventHandler BleDiscoverResult;
            /// <summary>
            /// Scan işlemi sırasında, her yeni Optoel cihazı keşfedildiğinde fırlatılır.
            /// </summary>
            public event BleDiscoverDeviceEventHandler BleDiscoverDeivce;
            /// <summary>
            /// Bir Optoel cihazıyla bağlantı kurulduğunda fırlatılır.
            /// </summary>
            public event BleConnectResultEventHandler BleConnectResult;

            //event BleATTClientAttributeValueEvent BleATTClientAttributeValue;

            //event BleConnectionDisconnectedEventHandler BleConnectionDisconnected;

            event BleGetRSSIEventHandler BleGetRssi;
            /// <summary>
            /// USB Dongle yada bluetooth hatası olduğunu belirtir.
            /// </summary>
            public event BleErrorEventHandler BleErrorEvent;
            event BleSlaveErrorEventHandler BleSlaveErrorEvent;


            //Enable/Disable
            /// <summary>
            /// Bu event, Accelerometer enable/disable yapıldığında fırlatılır.
            /// </summary>
            public event AccelEnableEventHandler AccelEnableResult;
            /// <summary>
            /// Bu event, Gyroscope enable/disable yapıldığında fırlatılır.
            /// </summary>
            public event GyroEnableEventHandler GyroEnableResult;
            /// <summary>
            /// Bu event, Magnotometer enable/disable yapıldığında fırlatılır.
            /// </summary>
            public event MagnoEnableEventHandler MagnoEnableResult;

            //Bw
            /// <summary>
            /// Bu event, Accelerometer'ın çalışma frekansı değiştiğinde fırlatılır.
            /// </summary>
            public event AccelBwEventHandler AccelBwResult;
            /// <summary>
            /// Bu event, Gyroscope'un çalışma frekansı değiştiğinde fırlatılır.
            /// </summary>
            public event GyroBwEventHandler GyroBwResult;
            /// <summary>
            /// Bu event, Magnotometer'ın çalışma frekansı değiştiğinde fırlatılır.
            /// </summary>
            public event MagnoBwEventHandler MagnoBwResult;

            //Range
            /// <summary>
            /// Bu event, Accelerometer'ın range değeri değiştiğinde fırlatılır.
            /// </summary>
            public event AccelRangeEventHandler AccelRangeResult;
            /// <summary>
            /// Bu event, Gyroscope'un range değeri değiştiğinde fırlatılır.
            /// </summary>
            public event GyroRangeEventHandler GyroRangeResult;

            //Read Enable
            /// <summary>
            /// Bu event, Accelerometer'ın ReadEnable durumu dğiştiğinde fırlatılır.
            /// </summary>
            public event AccelReadEnableEventHandler AccelReadEnableResult;
            /// <summary>
            /// Bu event, Gyroscope'un ReadEnable durumu dğiştiğinde fırlatılır.
            /// </summary>
            public event GyroReadEnableEventHandler GyroReadEnableResult;
            /// <summary>
            /// Bu event, Magnotometer'ın ReadEnable durumu dğiştiğinde fırlatılır.
            /// </summary>
            public event MagnoReadEnableEventHandler MagnoReadEnableResult;

            //Data
            /// <summary>
            /// Bu event, Accelerometer'dan yeni data geldiğinde fırlatılır.
            /// </summary>
            public event AccelDataEventHandler AccelData;
            /// <summary>
            /// Bu event, Gyroscope'dan yeni data geldiğinde fırlatılır.
            /// </summary>
            public event GyroDataEventHandler GyroData;
            /// <summary>
            /// Bu event, Magnotometer'dan yeni data geldiğinde fırlatılır.
            /// </summary>
            public event MagnoDataEventHandler MagnoData;

            /// <summary>
            /// Optoel cihazıyla bluetooth bağlantısı koptuğunda fırlatılır. 
            /// </summary>
            public event DisconnectedEventHandler DisConnected;
            public event GetRSSIEventHandler GetRssi;
            /// <summary>
            /// Slave cıhaz ile ilgili herhnagi bir bluetooth hatası oluştuğunu belirtir.
            /// </summary>
            public event SlaveErrorEventHandler SlaveError;

            #endregion Events




            public class BleSlave : Optoel.BMX055
            {
                Slave slave;
                System.IO.Ports.SerialPort SP = new System.IO.Ports.SerialPort();
                List<_Handle> ListHandles;
                Bluegiga.BGLib bglib;
                Device device;
                Ble ble;

                public readonly string DeviceName;
                /// <summary>
                /// Cihazın Mac. Adresi.
                /// </summary>
                public readonly string MacAddress;
                public readonly byte[] MacAddressHEX;
                public int SlaveNumber = 0;
                UInt16 ConnInterval;


                bool _Connected = false;
                /// <summary>
                /// Cihaz ile bluetooth bağlantı durumu.
                /// </summary>
                public bool Connected { get { return _Connected; } }


                public BleSlave(params object[] objects)
                {
                    this.slave = (Slave)objects[0];
                    this.ListHandles = (List<_Handle>)objects[1];
                    this.SP = (System.IO.Ports.SerialPort)objects[2];
                    this.bglib = (Bluegiga.BGLib)objects[3];
                    this.device = (Device)objects[4];
                    this.ble = (Ble)objects[5];

                    this.DeviceName = device.DeviceName;
                    this.MacAddress = device.MAC_Address;
                    this.MacAddressHEX = device.MAC_AddressHex;
                    ConnInterval = slave.ConnInterval;
                    _Connected = true;


                    ble.BleSlaveErrorEvent += new BleSlaveErrorEventHandler(BleSlaveErrorEvents);
                    bglib.BLEEventATTClientAttributeValue += new AttributeValueEventHandler(ATTClientAttributeValueEvent);
                    bglib.BLEResponseConnectionDisconnect += new Bluegiga.BLE.Responses.Connection.DisconnectEventHandler(ConnectionDisconnectRespons);
                    bglib.BLEEventConnectionDisconnected += new Bluegiga.BLE.Events.Connection.DisconnectedEventHandler(ConnectionDisconnectedEvent);

                    ble.BleGetRssi += new BleGetRSSIEventHandler(BleGetRssiEvent);
                }

                void ConnectionDisconnectRespons(object sender, DisconnectEventArgs e)
                {
                    if (e.connection == slave.ConnectionHandle)
                    {
                        if (e.result != 0)
                        {
                            Ble.errorCode errorCode = (Ble.errorCode)e.result;
                            string errorMessage = ble.ErrorCode[errorCode];
                            Ble.ErrorHandleArgs error = new Ble.ErrorHandleArgs(errorCode, errorMessage);

                            if (Error != null)
                                Error(error);

                            if (ble.SlaveError != null)
                                ble.SlaveError(this, error);
                        }
                    }
                }

                void ConnectionDisconnectedEvent(object sender, Bluegiga.BLE.Events.Connection.DisconnectedEventArgs e)
                {
                    if (e.connection == slave.ConnectionHandle)
                    {
                        _Connected = false;

                        string reason = "";
                        //if (SlaveDisconnected != null)
                        //{
                        if (e.reason == 0)
                        {
                            reason = "Disconnected by local user.";
                        }
                        else
                        {
                            Ble.errorCode errorCode = (Ble.errorCode)e.reason;
                            reason = ble.ErrorCode[errorCode];
                        }

                        if (SlaveDisconnected != null)
                            SlaveDisconnected(new DisconnectedEventArgs(e.reason, reason));

                        if (ble.DisConnected != null)
                            ble.DisConnected(this, new DisconnectedEventArgs(e.reason, reason));
                        //}
                    }
                }



                private void ATTClientAttributeValueEvent(object sender, AttributeValueEventArgs e)
                {
                    if (e.connection == slave.ConnectionHandle)
                    {
                        if (e.atthandle == GetHandle(0x0002))
                        {
                            Parse(e.value);
                        }
                        else if (e.atthandle == GetHandle(0x0003))
                        {
                            HandleData(e.value);
                        }
                    }
                }

                void BleGetRssiEvent(GetRssiEventArgs e)
                {
                    if (e.connection == slave.ConnectionHandle)
                    {
                        if (SlaveGetRssi != null)
                            SlaveGetRssi(new GetRSSIArgs(e.rssi));

                        if (ble.GetRssi != null)
                            ble.GetRssi(this, new GetRSSIArgs(e.rssi));
                    }
                }

                void BleSlaveErrorEvents(SlaveErrorEventArgs e)
                {
                    if (e.Connection == slave.ConnectionHandle)
                    {
                        Ble.ErrorHandleArgs error = e.ErrorHandle;

                        if (Error != null)
                            Error(error);

                        if (ble.SlaveError != null)
                            ble.SlaveError(this, error);
                    }
                }


                /// <summary>
                /// Optoel cihazı ile bluetooth bağlantısını sonlandırır.
                /// </summary>
                public void BleDisconnect()
                {
                    bglib.SendCommand(SP, bglib.BLECommandConnectionDisconnect(slave.ConnectionHandle));
                }

                public void GetRssi()
                {
                    bglib.SendCommand(SP, bglib.BLECommandConnectionGetRssi(slave.ConnectionHandle));
                }

                public void EncryptStart()
                {
                    //bglib.SendCommand(SP, bglib.BLECommandSMEncryptStart(slave.ConnectionHandle, 1));
                }


                #region BMX055 Komutları

                #region Enable
                /// <summary>
                /// Accelerometer'ı aktif/pasif eder. Bu komut sonucunda "SlaveAccelEnableResult" eventi fırlatılır.<para/>
                /// Disable: Accelerometer "Derin Uyku" modundadır. (min. güç tüketimi)<para/>
                /// Enable: Accelerometer "Bekleme" modundadır. ("Derin Uyku" modundan biraz daha fazla güç tüketir.)
                /// </summary>
                /// <param name="enable"></param>
                /// <returns></returns>
                public void AccelEnable(Enable enable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), EnableAccelCommand(enable)));
                }

                /// <summary>
                /// Gyroscope'u aktif/pasif eder. Bu komut sonucunda "SlaveGyroEnableResult" eventi fırlatılır.<para/>
                /// Disable: Gyroscope "Derin Uyku" modundadır. (min. güç tüketimi)<para/>
                /// Enable: Gyroscope "Bekleme" modundadır. ("Derin Uyku" modundan biraz daha fazla güç tüketir.)
                /// </summary>
                /// <param name="enable"></param>
                /// <returns></returns>
                public void GyroEnable(Enable enable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), EnableGyroCommand(enable)));
                }

                /// <summary>
                /// Magnotometer'ı aktif/pasif eder. Bu komut sonucunda "SlaveMagnoEnableResult" eventi fırlatılır.<para/>
                /// Disable: Magnotometer "Derin Uyku" modundadır. (min. güç tüketimi)<para/>
                /// Enable: Magnotometer "Bekleme" modundadır. ("Derin Uyku" modundan biraz daha fazla güç tüketir.)
                /// </summary>
                /// <param name="enable"></param>
                /// <returns></returns>
                public void MagnoEnable(Enable enable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), EnableMagnoCommand(enable)));
                }
                #endregion Enable


                #region Bw
                /// <summary>
                /// Accelerometer'ın okuma frekansını ayarlar. Bu komut sonucunda "SlaveAccelBwResult" eventi fırlatılır.
                /// </summary>
                /// <param name="accelBw"></param>
                /// <returns></returns>
                public void AccelSetBw(AccelBw accelBw)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), BwAccelCommand(accelBw)));
                }

                /// <summary>
                /// Gyroscope'un okuma frekansını ayarlar. Bu komut sonucunda "SlaveGyroBwResult" eventi fırlatılır.
                /// </summary>
                /// <param name="gyroBw"></param>
                /// <returns></returns>
                public void GyroSetBw(GyroBw gyroBw)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), BwGyroCommand(gyroBw)));
                }

                /// <summary>
                /// Magnotometer'ın okuma frekansını ayarlar. Bu komut sonucunda "SlaveMagnoBwResult" eventi fırlatılır.
                /// </summary>
                /// <param name="magnoBw"></param>
                /// <returns></returns>
                public void MagnoSetBw(MagnoBw magnoBw)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), BwMagnoCommand(magnoBw)));
                }
                #endregion Bw


                #region Range
                /// <summary>
                /// Accelerometer'ın çalışma aralığını (range) ayarlar. Bu komut sonucunda "SlaveAccelRangeResult" eventi fırlatılır.
                /// </summary>
                /// <param name="accelRange"></param>
                /// <returns></returns>
                public void AccelSetRange(AccelRange accelRange)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), RangeAccelCommand(accelRange)));
                }

                /// <summary>
                /// Gyroscope'un çalışma aralığını (range) ayarlar. Bu komut sonucunda "SlaveGyroRangeResult" eventi fırlatılır.
                /// </summary>
                /// <param name="gyroRange"></param>
                /// <returns></returns>
                public void GyroSetRange(GyroRange gyroRange)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), RangeGyroCommand(gyroRange)));
                }
                #endregion Range


                #region ReadEnable
                /// <summary>
                /// Accelerometer'ın okuma özelliğini aktif/pasif eder. Bu komuttan önce "AccelEnable(Enable.Enable)" komutuyla Accelerometer aktif edilmiş olmalıdır.<para/>
                /// Enable: Accelerometer "Run" modundadır. (max. güç tüketimi)<para/>
                /// Disable: Accelerometer "Bekleme" modundadır. ("Derin Uyku" modundan biraz daha fazla güç tüketir.)
                /// </summary>
                /// <param name="enable"></param>
                public void AccelReadEnable(Enable enable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), ReadEnableAccelCommand(enable)));
                }

                /// <summary>
                /// Gyroscope'un okuma özelliğini aktif/pasif eder. Bu komuttan önce "GyroEnable(Enable.Enable)" komutuyla Gyroscope aktif edilmiş olmalıdır.<para/>
                /// Enable: Gyroscope "Run" modundadır. (max. güç tüketimi)<para/>
                /// Disable: Gyroscope "Bekleme" modundadır. ("Derin Uyku" modundan biraz daha fazla güç tüketir.)
                /// </summary>
                /// <param name="enable"></param>
                public void GyroReadEnable(Enable enable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), ReadEnableGyroCommand(enable)));
                }

                /// <summary>
                /// Magnotometer'ın okuma özelliğini aktif/pasif eder. Bu komuttan önce "MagnoEnable(Enable.Enable)" komutuyla Magnotometer aktif edilmiş olmalıdır.<para/>
                /// Enable: Magnotometer "Run" modundadır. (max. güç tüketimi)<para/>
                /// Disable: Magnotometer "Bekleme" modundadır. ("Derin Uyku" modundan biraz daha fazla güç tüketir.)
                /// </summary>
                /// <param name="enable"></param>
                public void MagnoReadEnable(Enable enable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), ReadEnableMagnoCommand(enable)));
                }
                #endregion ReadEnable


                /// <summary>
                /// Aktif edilmiş sensörlerden, verilerin gönerilmesini başlatır.
                /// </summary>
                /// <returns></returns>
                public void Start(Enable AccelReadEnable, Enable GyroReadEnable, Enable MAgnoReadEnable)
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), StartCommand(AccelReadEnable, GyroReadEnable, MAgnoReadEnable)));
                }

                /// <summary>
                /// Aktif edilmiş sönserlerden, verilerin gönderilmesini durdurur.
                /// </summary>
                /// <returns></returns>
                public void Stop()
                {
                    bglib.SendCommand(SP, bglib.BLECommandATTClientAttributeWrite(slave.ConnectionHandle, GetHandle(0x0002), StopCommand()));
                }

                UInt16 GetHandle(UInt16 _Uuid)
                {
                    UInt16 handle = 0xFFFF;

                    if (ListHandles.Exists(X => X.Uuid == _Uuid))
                    {
                        int index = ListHandles.FindIndex(x => x.Uuid == _Uuid);
                        if (index != -1)
                            handle = ListHandles[index].Handle;
                    }

                    return handle;
                }

                #endregion BMX055 Komutları


                #region Handle

                int AccelCurrentDataNumber = 0;
                int GyroCurrentDataNumber = 0;
                int MagnoCurrentDataNumber = 0;

                public int AccelLostDataCount = 0;
                public int GyroLostDataCount = 0;
                public int MagnoLostDataCount = 0;

                int AccelDataNumber = 0;
                int GyroDataNumber = 0;
                int MagnoDataNumber = 0;
                int AccelCount = 0;
                int GyroCount = 0;
                int MagnoCount = 0;

                List<byte[]> listHandle = new List<byte[]>();

                void HandleData(byte[] val)
                {
                    if (val != null)
                    {

                        listHandle.Add(val);

                        int len = val.Length;

                        if (len <= 11)
                        {
                            if (val[len - 1] == CalcCRC(val, 0, len - 2))
                            {
                                Handle(val);
                            }
                        }
                        else if (val.Length > 11)
                        {
                            if (val[len - 1] == CalcCRC(val, 0, len - 2))
                            {
                                byte[] data = new byte[10];

                                if ((val[1] & 0x40) == 0x40)
                                {
                                    Array.Copy(val, 0, data, 0, 10);
                                    Handle(data);

                                    Array.Copy(val, 10, data, 0, 8);
                                    Handle(data);
                                }
                                else
                                {
                                    Array.Copy(val, 0, data, 0, 8);
                                    Handle(data);

                                    if ((val[9] & 0x40) == 0x40)
                                    {
                                        Array.Copy(val, 8, data, 0, 10);
                                        Handle(data);
                                    }
                                    else
                                    {
                                        Array.Copy(val, 8, data, 0, 8);
                                        Handle(data);
                                    }
                                }
                            }
                        }
                    }
                }

                byte CalcCRC(byte[] array, int startIndex, int endIndex)
                {
                    int sum = 0;

                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        sum += array[i];
                    }
                    byte a = (byte)(sum % 256);
                    return a;
                }

                void Handle(byte[] val)
                {
                    if ((val[1] & 0x80) == 0x80)   //gyro
                    {
                        int GyroData1 = BitConverter.ToInt16(new byte[] { val[2], val[3] }, 0);
                        int GyroData2 = BitConverter.ToInt16(new byte[] { val[4], val[5] }, 0);
                        int GyroData3 = BitConverter.ToInt16(new byte[] { val[6], val[7] }, 0);
                        int gyroDataNumber = (int)(val[0]);

                        float range = 0;
                        switch ((GyroRange)(val[1] & 0x0F))
                        {
                            case GyroRange.GyroRange_2000s:
                                range = (float)0.0610;
                                break;
                            case GyroRange.GyroRange_1000s:
                                range = (float)0.0305;
                                break;
                            case GyroRange.GyroRange_500s:
                                range = (float)0.0153;
                                break;
                            case GyroRange.GyroRange_250s:
                                range = (float)0.0076;
                                break;
                            case GyroRange.GyroRange_125s:
                                range = (float)0.0038;
                                break;
                        }

                        float rawX = (float)(GyroData1 * range);
                        float rawY = (float)(GyroData2 * range);
                        float rawZ = (float)(GyroData3 * range);
                        GyroDataNumber = (GyroCount * 251) + gyroDataNumber;

                        if (gyroDataNumber != GyroCurrentDataNumber)
                        {
                            GyroLostDataCount += gyroDataNumber - GyroCurrentDataNumber;
                            GyroCurrentDataNumber = gyroDataNumber;
                        }

                        GyroCurrentDataNumber++;
                        if (GyroCurrentDataNumber >= 251)
                        {
                            GyroCurrentDataNumber = 0;
                            GyroCount++;
                        }

                        Gyro gyro = new Gyro(rawX, rawY, rawZ, GyroDataNumber);

                        if (GyroData != null)
                            GyroData(gyro);

                        if (ble.GyroData != null)
                            ble.GyroData(this, gyro);

                        ListGyro.Add(gyro);

                    }
                    else if ((val[1] & 0x40) == 0x40)   //magno
                    {
                        int MagnoData1 = Convert.ToInt32((Int16)(val[2] + (val[3] << 8)) >> 3);
                        int MagnoData2 = Convert.ToInt32((Int16)(val[4] + (val[5] << 8)) >> 3);
                        int MagnoData3 = Convert.ToInt32((Int16)(val[6] + (val[7] << 8)) >> 1);
                        int MagnoData4 = Convert.ToInt32((Int16)(val[8] + (val[9] << 8)) >> 2);
                        //int MagnoDataNumber = (int)(val[0] + (val[1] << 8));
                        int magnoDataNumber = (int)(val[0]);

                        float rawX = (float)(MagnoData1);
                        float rawY = (float)(MagnoData2);
                        float rawZ = (float)(MagnoData3);
                        int rHall = MagnoData4;
                        MagnoDataNumber = (MagnoCount * 251) + magnoDataNumber;



                        if (magnoDataNumber != MagnoCurrentDataNumber)
                        {
                            MagnoLostDataCount += magnoDataNumber - MagnoCurrentDataNumber;
                            MagnoCurrentDataNumber = magnoDataNumber;
                        }

                        MagnoCurrentDataNumber++;
                        if (MagnoCurrentDataNumber >= 251)
                        {
                            MagnoCurrentDataNumber = 0;
                            MagnoCount++;
                        }

                        Magno magno = new Magno(rawX, rawY, rawZ, MagnoDataNumber);

                        if (MagnoData != null)
                            MagnoData(magno);

                        if (ble.MagnoData != null)
                            ble.MagnoData(this, magno);

                        ListMagno.Add(magno);
                    }
                    else   //accel
                    {
                        try
                        {
                            Int16 AccelData1 = (Int16)((Int16)(val[2] | (val[3] << 8)) >> 4);
                            Int16 AccelData2 = (Int16)((Int16)(val[4] | (val[5] << 8)) >> 4);
                            Int16 AccelData3 = (Int16)((Int16)(val[6] | (val[7] << 8)) >> 4);
                            //int AccelDataNumber = (int)(val[6] + (val[7] << 8));
                            int accelDataNumber = (int)(val[0]);

                            float range = 0;
                            switch ((AccelRange)val[1])
                            {
                                case AccelRange.AccelRange_2g:
                                    range = (float)0.98;
                                    break;
                                case AccelRange.AccelRange_4g:
                                    range = (float)1.95;
                                    break;
                                case AccelRange.AccelRange_8g:
                                    range = (float)3.91;
                                    break;
                                case AccelRange.AccelRange_16g:
                                    range = (float)7.81;
                                    break;
                            }

                            float rawX = (float)(AccelData1 * range);
                            float rawY = (float)(AccelData2 * range);
                            float rawZ = (float)(AccelData3 * range);
                            AccelDataNumber = (AccelCount * 251) + accelDataNumber;

                            if (accelDataNumber != AccelCurrentDataNumber)
                            {
                                AccelLostDataCount += accelDataNumber - AccelCurrentDataNumber;
                                AccelCurrentDataNumber = accelDataNumber;
                            }

                            AccelCurrentDataNumber++;
                            if (AccelCurrentDataNumber >= 251)
                            {
                                AccelCurrentDataNumber = 0;
                                AccelCount++;
                            }

                            Accel accel = new Accel(rawX, rawY, rawZ, AccelDataNumber);

                            if (AccelData != null)
                                AccelData(accel);

                            if (ble.AccelData != null)
                                ble.AccelData(this, accel);

                            ListAccel.Add(accel);

                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                #endregion Handle


                #region Pars

                private void Parse(byte[] data)
                {
                    if (data[0] == 0x00)   // Response: 0x00
                    {
                        if (data[1] == 0x00)   // Ready
                        {
                            bool accelReady = false;
                            bool accelSelfTest = false;
                            bool gyroReady = false;
                            bool gyroSelfTest = false;
                            bool magnoReady = false;
                            bool magnoSelfTest = false;

                            if (data[2] == 0x01)   // Accel
                            {
                                if (data[3] == 0x01)  // Accel Ready
                                {
                                    accelReady = true;

                                    if (data[4] == 0x00)  // Accel Self-test success
                                    {
                                        accelSelfTest = true;
                                    }
                                    else                    // Accel Self-test failed
                                    {
                                        accelSelfTest = false;
                                    }
                                }
                                else if (data[3] == 0x00) // Accel not ready
                                {
                                    accelReady = false;
                                }
                            }
                            if (data[5] == 0x02)  // Gyro
                            {
                                if (data[6] == 0x01) // Gyro Ready
                                {
                                    gyroReady = true;

                                    if (data[7] == 0x00)  // Gyro Self-test Success
                                    {
                                        gyroSelfTest = true;
                                    }
                                    else                   // Gyro Self-test failed
                                    {
                                        gyroSelfTest = false;
                                    }
                                }
                                else if (data[6] == 0x00) // Gyro not ready
                                {
                                    gyroReady = false;
                                }
                            }
                            if (data[8] == 0x04)  // Magno
                            {
                                if (data[9] == 0x01)  // Magno Ready
                                {
                                    magnoReady = true;

                                    if (data[10] == 0x00) // Magno Selft-test Success   
                                    {
                                        magnoSelfTest = true;
                                    }
                                    else                   // Magno Selft-test failed
                                    {
                                        magnoSelfTest = false;
                                    }
                                }
                                else if (data[9] == 0x00)
                                {
                                    magnoReady = false;
                                }
                            }

                            //ConnecResult connecResult = new ConnecResult(true, accelReady, accelSelfTest, gyroReady, gyroSelfTest, magnoReady, magnoSelfTest);
                            //SeriportConnectResult(connecResult);
                        }
                        else if (data[1] == 0x01)  // Enable/Disable
                        {
                            if (data[2] == 0x01)   // Accel
                            {
                                bool AccelEnableSucces = false;
                                Enable enable = Enable.Failed;

                                if (data[3] == 0x01)        // Enable
                                {
                                    AccelEnableSucces = true;
                                    enable = Enable.Enable;
                                }
                                else if (data[3] == 0x02)   // Disable
                                {
                                    AccelEnableSucces = true;
                                    enable = Enable.Disable;
                                }
                                else if (data[3] == 0x0)     // Failed
                                {
                                    AccelEnableSucces = false;
                                    enable = Enable.Failed;
                                }

                                if (SlaveAccelEnableResult != null)
                                    SlaveAccelEnableResult(new AccelEnableResultArgs(AccelEnableSucces, enable));

                                if (SlaveEnableResult != null)
                                    SlaveEnableResult(new EnableResultArgs(Sensor.Accel, AccelEnableSucces, enable));

                                if (ble.AccelEnableResult != null)
                                    ble.AccelEnableResult(this, new AccelEnableResultArgs(AccelEnableSucces, enable));

                            }
                            else if (data[2] == 0x02)  // Gyro
                            {
                                bool GyroEnableSuccess = false;
                                Enable enable = Enable.Failed;

                                if (data[3] == 0x01)        // Enable
                                {
                                    GyroEnableSuccess = true;
                                    enable = Enable.Enable;
                                }
                                else if (data[3] == 0x02)   // Disable
                                {
                                    GyroEnableSuccess = true;
                                    enable = Enable.Disable;
                                }
                                else if (data[3] == 0x00)    // Failed
                                {
                                    GyroEnableSuccess = false;
                                    enable = Enable.Failed;
                                }

                                if (SlaveGyroEnableResult != null)
                                    SlaveGyroEnableResult(new GyroEnableResultArgs(GyroEnableSuccess, enable));

                                if (SlaveEnableResult != null)
                                    SlaveEnableResult(new EnableResultArgs(Sensor.Gyro, GyroEnableSuccess, enable));

                                if (ble.GyroEnableResult != null)
                                    ble.GyroEnableResult(this, new GyroEnableResultArgs(GyroEnableSuccess, enable));

                            }
                            else if (data[2] == 0x04)  // Magno
                            {
                                bool MagnoEnableSucccess = false;
                                Enable enable = Enable.Failed;

                                if (data[3] == 0x01)        // Enable
                                {
                                    MagnoEnableSucccess = true;
                                    enable = Enable.Enable;
                                }
                                else if (data[3] == 0x02)   // Disable
                                {
                                    MagnoEnableSucccess = true;
                                    enable = Enable.Disable;
                                }
                                else if (data[3] == 0x00)   // Failed
                                {
                                    MagnoEnableSucccess = false;
                                    enable = Enable.Failed;
                                }

                                if (SlaveMagnoEnableResult != null)
                                    SlaveMagnoEnableResult(new MagnoEnableResultArgs(MagnoEnableSucccess, enable));

                                if (SlaveEnableResult != null)
                                    SlaveEnableResult(new EnableResultArgs(Sensor.Magno, MagnoEnableSucccess, enable));

                                if (ble.MagnoEnableResult != null)
                                    ble.MagnoEnableResult(this, new MagnoEnableResultArgs(MagnoEnableSucccess, enable));

                            }
                        }
                        else if (data[1] == 0x02)  // Bw
                        {
                            if (data[2] == 0x01)   // Accel
                            {
                                bool AccelBwSuccess = false;
                                AccelBw accelbw = AccelBw.None;

                                if (data[3] == 0x01)   // Success
                                {
                                    AccelBwSuccess = true;
                                    accelbw = (AccelBw)data[4];
                                }
                                else if (data[3] == 0x00)  // Failed
                                {
                                    AccelBwSuccess = false;
                                }

                                if (SlaveAccelBwResult != null)
                                    SlaveAccelBwResult(new AccelBwResultArgs(AccelBwSuccess, accelbw));

                                if (SlaveBwResult != null)
                                    SlaveBwResult(new BwResultArgs(Sensor.Accel, AccelBwSuccess, accelbw));

                                if (ble.AccelBwResult != null)
                                    ble.AccelBwResult(this, new AccelBwResultArgs(AccelBwSuccess, accelbw));

                            }
                            else if (data[2] == 0x02)  // Gyro
                            {
                                bool GyroBwSuccess = false;
                                GyroBw gyroBw = GyroBw.None;

                                if (data[3] == 0x01)   // Success
                                {
                                    GyroBwSuccess = true;
                                    gyroBw = (GyroBw)data[4];
                                }
                                else if (data[3] == 0x00)  // Failed
                                {
                                    GyroBwSuccess = false;
                                }

                                if (SlaveGyroBwResult != null)
                                    SlaveGyroBwResult(new GyroBwResultArgs(GyroBwSuccess, gyroBw));

                                if (SlaveBwResult != null)
                                    SlaveBwResult(new BwResultArgs(Sensor.Gyro, GyroBwSuccess, gyroBw));


                                if (ble.GyroBwResult != null)
                                    ble.GyroBwResult(this, new GyroBwResultArgs(GyroBwSuccess, gyroBw));

                            }
                            else if (data[2] == 0x04)  // Magno
                            {
                                bool MagnoBwSuccess = false;
                                MagnoBw magnoBw = MagnoBw.None;

                                if (data[3] == 0x01)   // Success
                                {
                                    MagnoBwSuccess = true;
                                    magnoBw = (MagnoBw)data[4];
                                }
                                else if (data[3] == 0x00)  // Failed
                                {
                                    MagnoBwSuccess = false;
                                }

                                if (SlaveMagnoBwResult != null)
                                    SlaveMagnoBwResult(new MagnoBwResultArgs(MagnoBwSuccess, magnoBw));

                                if (SlaveBwResult != null)
                                    SlaveBwResult(new BwResultArgs(Sensor.Magno, MagnoBwSuccess, magnoBw));

                                if (ble.MagnoBwResult != null)
                                    ble.MagnoBwResult(this, new MagnoBwResultArgs(MagnoBwSuccess, magnoBw));
                            }
                        }
                        else if (data[1] == 0x03)  // Range
                        {
                            if (data[2] == 0x01)   // Accel
                            {
                                bool AccelRangeSuccess = false;
                                AccelRange accelRange = AccelRange.None;

                                if (data[3] == 0x01)   // Success
                                {
                                    AccelRangeSuccess = true;
                                    accelRange = (AccelRange)data[4];
                                }
                                else if (data[3] == 0x00)  // Failed
                                {
                                    AccelRangeSuccess = false;
                                }

                                if (SlaveAccelRangeResult != null)
                                    SlaveAccelRangeResult(new AccelRangeResultArgs(AccelRangeSuccess, accelRange));

                                if (SlaveRangeResult != null)
                                    SlaveRangeResult(new RangeResultArgs(Sensor.Accel, AccelRangeSuccess, accelRange));


                                if (ble.AccelRangeResult != null)
                                    ble.AccelRangeResult(this, new AccelRangeResultArgs(AccelRangeSuccess, accelRange));
                            }
                            else if (data[2] == 0x02)  // Gyro
                            {
                                bool GyroRangeSuccess = false;
                                GyroRange gyroRange = GyroRange.None;

                                if (data[3] == 0x01)   // Success
                                {
                                    GyroRangeSuccess = true;
                                    gyroRange = (GyroRange)data[4];
                                }
                                else if (data[3] == 0x00)  // Failed
                                {
                                    GyroRangeSuccess = false;
                                }

                                if (SlaveGyroRangeResult != null)
                                    SlaveGyroRangeResult(new GyroRangeResultArgs(GyroRangeSuccess, gyroRange));

                                if (SlaveRangeResult != null)
                                    SlaveRangeResult(new RangeResultArgs(Sensor.Gyro, GyroRangeSuccess, gyroRange));

                                if (ble.GyroRangeResult != null)
                                    ble.GyroRangeResult(this, new GyroRangeResultArgs(GyroRangeSuccess, gyroRange));
                            }
                        }
                        else if (data[1] == 0x04)  // Read Enable
                        {
                            if (data[2] == 0x01)   // Accel
                            {
                                bool AccelReadEnableSuccess = false;
                                Enable enable = Enable.Failed;

                                if (data[3] == 0x01)   // Enable
                                {
                                    AccelReadEnableSuccess = true;
                                    enable = Enable.Enable;
                                }
                                else if (data[3] == 0x02)  // Disable
                                {
                                    AccelReadEnableSuccess = true;
                                    enable = Enable.Disable;
                                }
                                else if (data[3] == 0x00)  // Failed
                                {
                                    AccelReadEnableSuccess = false;
                                }

                                if (SlaveAccelReadEnableResult != null)
                                    SlaveAccelReadEnableResult(new AccelReadEnableResultArgs(AccelReadEnableSuccess, enable));

                                if (SlaveReadEnableResult != null)
                                    SlaveReadEnableResult(new ReadEnableResultArgs(Sensor.Accel, AccelReadEnableSuccess, enable));

                                if (ble.AccelReadEnableResult != null)
                                    ble.AccelReadEnableResult(this, new AccelReadEnableResultArgs(AccelReadEnableSuccess, enable));
                            }
                            else if (data[2] == 0x02)  // Gyro
                            {
                                bool GyroReadEnableSuccess = false;
                                Enable enable = Enable.Failed;

                                if (data[3] == 0x01)        // Enable
                                {
                                    GyroReadEnableSuccess = true;
                                    enable = Enable.Enable;
                                }
                                else if (data[3] == 0x02)   // Disable
                                {
                                    GyroReadEnableSuccess = true;
                                    enable = Enable.Disable;
                                }
                                else if (data[3] == 0x00)   // Failed
                                {
                                    GyroReadEnableSuccess = false;
                                }

                                if (SlaveGyroReadEnableResult != null)
                                    SlaveGyroReadEnableResult(new GyroReadEnableResultArgs(GyroReadEnableSuccess, enable));

                                if (SlaveReadEnableResult != null)
                                    SlaveReadEnableResult(new ReadEnableResultArgs(Sensor.Gyro, GyroReadEnableSuccess, enable));

                                if (ble.GyroReadEnableResult != null)
                                    ble.GyroReadEnableResult(this, new GyroReadEnableResultArgs(GyroReadEnableSuccess, enable));
                            }
                            else if (data[2] == 0x04)  // Magno
                            {
                                bool MagnoReadEnableSuccess = false;
                                Enable enable = Enable.Failed;

                                if (data[3] == 0x01)        // Enable
                                {
                                    MagnoReadEnableSuccess = true;
                                    enable = Enable.Enable;
                                }
                                else if (data[3] == 0x02)   // Disable
                                {
                                    MagnoReadEnableSuccess = true;
                                    enable = Enable.Disable;
                                }
                                else if (data[3] == 0x00)   // Failed
                                {
                                    MagnoReadEnableSuccess = false;
                                }

                                if (SlaveMagnoReadEnableResult != null)
                                    SlaveMagnoReadEnableResult(new MagnoReadEnableResultArgs(MagnoReadEnableSuccess, enable));

                                if (SlaveReadEnableResult != null)
                                    SlaveReadEnableResult(new ReadEnableResultArgs(Sensor.Magno, MagnoReadEnableSuccess, enable));

                                if (ble.MagnoReadEnableResult != null)
                                    ble.MagnoReadEnableResult(this, new MagnoReadEnableResultArgs(MagnoReadEnableSuccess, enable));
                            }
                        }

                    }
                    else if (data[0] == 0x08) //Event
                    {
                        if (data[1] == 0x0A)
                        {
                            if (data[2] != ConnInterval)
                            {
                                if (data[2] == 6)
                                {
                                    bglib.SendCommand(SP, bglib.BLECommandConnectionUpdate(slave.ConnectionHandle, 6, 8, 0, 1000));
                                }
                                else
                                {
                                    UInt16 maxinterval = (UInt16)data[2];
                                    maxinterval += 5;
                                    bglib.SendCommand(SP, bglib.BLECommandConnectionUpdate(slave.ConnectionHandle, data[2], maxinterval, 0, 1000));
                                }

                                ConnInterval = data[2];
                            }
                        }
                        else if (data[1] == 0x0B)
                        {
                            byte[] trim_x1y1 = new byte[2];
                            byte[] trim_xyz_data = new byte[4];
                            byte[] trim_xy1xy2 = new byte[10];
                            UInt16 temp_msb = 0;

                            Array.Copy(data, 2, trim_x1y1, 0, 2);
                            Array.Copy(data, 4, trim_xyz_data, 0, 4);
                            Array.Copy(data, 8, trim_xy1xy2, 0, 10);

                            TrimData = new MagnoTrimRegisters();

                            TrimData.dig_x1 = (sbyte)trim_x1y1[0];
                            TrimData.dig_y1 = (sbyte)trim_x1y1[1];
                            TrimData.dig_x2 = (sbyte)trim_xyz_data[2];
                            TrimData.dig_y2 = (sbyte)trim_xyz_data[3];
                            temp_msb = (UInt16)(trim_xy1xy2[3] << 8);
                            TrimData.dig_z1 = (UInt16)(temp_msb | trim_xy1xy2[2]);

                            temp_msb = (UInt16)(trim_xy1xy2[1] << 8);
                            TrimData.dig_z2 = (Int16)(temp_msb | trim_xy1xy2[0]);

                            temp_msb = (UInt16)(trim_xy1xy2[7] << 8);
                            TrimData.dig_z3 = (Int16)(temp_msb | trim_xy1xy2[6]);

                            temp_msb = (UInt16)(trim_xyz_data[1] << 8);
                            TrimData.dig_z4 = (Int16)(temp_msb | trim_xyz_data[0]);

                            TrimData.dig_xy1 = trim_xy1xy2[9];
                            TrimData.dig_xy2 = (sbyte)trim_xy1xy2[8];
                            temp_msb = (UInt16)((trim_xy1xy2[5] & 0x7F) << 8);
                            TrimData.dig_xyz1 = (UInt16)(temp_msb | trim_xy1xy2[4]);
                        }
                    }
                }

                #endregion Pars



                #region Delegates

                //Enable 
                public delegate void AccelEnableEventHandler(AccelEnableResultArgs e);
                public class AccelEnableResultArgs
                {
                    public readonly bool Success;
                    public readonly Optoel.Enable Enable;

                    public AccelEnableResultArgs(bool success, Optoel.Enable Enable)
                    {
                        this.Success = success;
                        this.Enable = Enable;
                    }
                }

                public delegate void GyroEnableEventHandler(GyroEnableResultArgs e);
                public class GyroEnableResultArgs
                {
                    public readonly bool Success;
                    public readonly Enable Enable;

                    public GyroEnableResultArgs(bool success, Enable enable)
                    {
                        this.Success = success;
                        this.Enable = enable;
                    }
                }

                public delegate void MagnoEnableEventHandler(MagnoEnableResultArgs e);
                public class MagnoEnableResultArgs
                {
                    public readonly bool Success;
                    public readonly Enable Enable;
                    public MagnoEnableResultArgs(bool success, Enable enable)
                    {
                        this.Success = success;
                        this.Enable = enable;
                    }
                }


                public delegate void EnableEventHandler(EnableResultArgs e);
                public class EnableResultArgs
                {
                    public readonly Sensor Sensor;
                    public readonly bool Success;
                    public readonly Enable Enable;
                    public EnableResultArgs(Sensor sensor, bool success, Enable enable)
                    {
                        this.Sensor = sensor;
                        this.Success = success;
                        this.Enable = enable;
                    }
                }


                //Bw
                public delegate void AccelBwEventHandler(AccelBwResultArgs e);
                public class AccelBwResultArgs
                {
                    public readonly bool Success;
                    public readonly AccelBw AccelBw;
                    public AccelBwResultArgs(bool success, AccelBw accelBw)
                    {
                        this.Success = success;
                        this.AccelBw = accelBw;
                    }
                }

                public delegate void GyroBwEventHandler(GyroBwResultArgs e);
                public class GyroBwResultArgs
                {
                    public readonly bool Success;
                    public readonly GyroBw GyroBw;
                    public GyroBwResultArgs(bool success, GyroBw gyroBw)
                    {
                        this.Success = success;
                        this.GyroBw = gyroBw;
                    }
                }

                public delegate void MagnoBwEventHandler(MagnoBwResultArgs e);
                public class MagnoBwResultArgs
                {
                    public readonly bool Success;
                    public readonly MagnoBw MagnoBw;
                    public MagnoBwResultArgs(bool success, MagnoBw magnoBw)
                    {
                        this.Success = success;
                        this.MagnoBw = magnoBw;
                    }
                }


                public delegate void BwEventHandler(BwResultArgs e);
                public class BwResultArgs
                {
                    public readonly Sensor Sensor;
                    public readonly bool Success;
                    public readonly Enum Bw;

                    public BwResultArgs(Sensor sensor, bool success, Enum bw)
                    {
                        this.Sensor = sensor;
                        this.Success = success;
                        this.Bw = bw;
                    }
                }



                //Range
                public delegate void AccelRangeEventHandler(AccelRangeResultArgs e);
                public class AccelRangeResultArgs
                {
                    public readonly bool Success;
                    public readonly AccelRange accelRange;
                    public AccelRangeResultArgs(bool success, AccelRange accelRange)
                    {
                        this.Success = success;
                        this.accelRange = accelRange;
                    }
                }

                public delegate void GyroRangeEventHandler(GyroRangeResultArgs e);
                public class GyroRangeResultArgs
                {
                    public readonly bool Success;
                    public readonly GyroRange gyroRange;
                    public GyroRangeResultArgs(bool success, GyroRange gyroRange)
                    {
                        this.Success = success;
                        this.gyroRange = gyroRange;
                    }
                }


                public delegate void RangeEventHandler(RangeResultArgs e);
                public class RangeResultArgs
                {
                    public readonly Sensor Sensor;
                    public readonly bool Success;
                    public readonly Enum Range;
                    public RangeResultArgs(Sensor sensor, bool success, Enum range)
                    {
                        this.Sensor = sensor;
                        this.Success = success;
                        this.Range = range;
                    }
                }



                //Read Enable
                public delegate void AccelReadEnableEventHandler(AccelReadEnableResultArgs e);
                public class AccelReadEnableResultArgs
                {
                    public readonly bool Success;
                    public readonly Enable Enable;
                    public AccelReadEnableResultArgs(bool success, Enable enable)
                    {
                        this.Success = success;
                        this.Enable = enable;
                    }
                }

                public delegate void GyroReadEnableEventHandler(GyroReadEnableResultArgs e);
                public class GyroReadEnableResultArgs
                {
                    public readonly bool Success;
                    public readonly Enable Enable;
                    public GyroReadEnableResultArgs(bool success, Enable enable)
                    {
                        this.Success = success;
                        this.Enable = enable;
                    }
                }

                public delegate void MagnoReadEnableEventHandler(MagnoReadEnableResultArgs e);
                public class MagnoReadEnableResultArgs
                {
                    public readonly bool Success;
                    public readonly Enable Enable;
                    public MagnoReadEnableResultArgs(bool success, Enable enable)
                    {
                        this.Success = success;
                        this.Enable = enable;
                    }
                }


                public delegate void ReadEnableEventHandler(ReadEnableResultArgs e);
                public class ReadEnableResultArgs
                {
                    public readonly Sensor Sensor;
                    public readonly bool Success;
                    public readonly Enum Enable;
                    public ReadEnableResultArgs(Sensor sensor, bool success, Enum enable)
                    {
                        this.Sensor = sensor;
                        this.Success = success;
                        this.Enable = enable;
                    }
                }



                //Data
                public delegate void AccelDataEventHandler(Accel Accel);
                public delegate void GyroDataEventHandler(Gyro Gyro);
                public delegate void MagnoDataEventHandler(Magno Magno);


                public delegate void SlaveDisconnectedEventHandler(DisconnectedEventArgs e);
                public class DisconnectedEventArgs
                {
                    public readonly UInt16 ReasonCode;
                    public readonly string Reason;
                    public DisconnectedEventArgs(UInt16 reasonCode, string reason)
                    {
                        this.ReasonCode = reasonCode;
                        this.Reason = reason;
                    }
                }

                public delegate void SlaveGetRSSIEventHandler(GetRSSIArgs e);
                public class GetRSSIArgs
                {
                    public readonly sbyte Rssi;
                    public GetRSSIArgs(sbyte rssi)
                    {
                        this.Rssi = rssi;
                    }
                }



                public delegate void SlaveErrorEventHandler(Ble.ErrorHandleArgs Error);
                public class ErrorHandleArgs
                {
                    public readonly errorCode ErrorCode;
                    public readonly string ErrorMessage;
                    public ErrorHandleArgs(errorCode errorCode, string errorMessage)
                    {
                        this.ErrorCode = errorCode;
                        this.ErrorMessage = errorMessage;
                    }
                }






                #endregion Delegates



                #region Events

                //Enable
                /// <summary>
                /// Bu event, Accelerometer enable/disable yapıldığında fırlatılır.
                /// </summary>
                public event AccelEnableEventHandler SlaveAccelEnableResult;
                /// <summary>
                /// Bu event, Gyroscope enable/disable yapıldığında fırlatılır.
                /// </summary>
                public event GyroEnableEventHandler SlaveGyroEnableResult;
                /// <summary>
                /// Bu event, Magnotometer enable/disable yapıldığında fırlatılır.
                /// </summary>
                public event MagnoEnableEventHandler SlaveMagnoEnableResult;

                public event EnableEventHandler SlaveEnableResult;


                //Bw
                /// <summary>
                /// Bu event, Accelerometer'ın çalışma frekansı değiştiğinde fırlatılır.
                /// </summary>
                public event AccelBwEventHandler SlaveAccelBwResult;
                /// <summary>
                /// Bu event, Gyroscope'un çalışma frekansı değiştiğinde fırlatılır.
                /// </summary>
                public event GyroBwEventHandler SlaveGyroBwResult;
                /// <summary>
                /// Bu event, Magnotometer'ın çalışma frekansı değiştiğinde fırlatılır.
                /// </summary>
                public event MagnoBwEventHandler SlaveMagnoBwResult;

                public event BwEventHandler SlaveBwResult;


                //Range
                /// <summary>
                /// Bu event, Accelerometer'ın range değeri değiştiğinde fırlatılır.
                /// </summary>
                public event AccelRangeEventHandler SlaveAccelRangeResult;
                /// <summary>
                /// Bu event, Gyroscope'un range değeri değiştiğinde fırlatılır.
                /// </summary>
                public event GyroRangeEventHandler SlaveGyroRangeResult;

                public event RangeEventHandler SlaveRangeResult;


                //Read Enable
                /// <summary>
                /// Bu event, Accelerometer'ın ReadEnable durumu dğiştiğinde fırlatılır.
                /// </summary>
                public event AccelReadEnableEventHandler SlaveAccelReadEnableResult;
                /// <summary>
                /// Bu event, Gyroscope'un ReadEnable durumu dğiştiğinde fırlatılır.
                /// </summary>
                public event GyroReadEnableEventHandler SlaveGyroReadEnableResult;
                /// <summary>
                /// Bu event, Magnotometer'ın ReadEnable durumu dğiştiğinde fırlatılır. 
                /// </summary>
                public event MagnoReadEnableEventHandler SlaveMagnoReadEnableResult;

                public event ReadEnableEventHandler SlaveReadEnableResult;


                //Data
                /// <summary>
                /// Bu event, Accelerometer'dan yeni data geldiğinde fırlatılır.
                /// </summary>
                public event AccelDataEventHandler AccelData;
                /// <summary>
                /// Bu event, Gyroscope'dan yeni data geldiğinde fırlatılır. 
                /// </summary>
                public event GyroDataEventHandler GyroData;
                /// <summary>
                /// Bu event, Magnotometer'dan yeni data geldiğinde fırlatılır. 
                /// </summary>
                public event MagnoDataEventHandler MagnoData;

                /// <summary>
                /// Optoel cihazıyla bluetooth bağlantısı koptuğunda fırlatılır. 
                /// </summary>
                public event SlaveDisconnectedEventHandler SlaveDisconnected;
                public event SlaveGetRSSIEventHandler SlaveGetRssi;
                /// <summary>
                /// Slave cıhaz ile ilgili herhnagi bir bluetooth hatası oluştuğunu belirtir.
                /// </summary>
                public event SlaveErrorEventHandler Error;

                #endregion Events

            }




            public Dictionary<errorCode, string> ErrorCode = new Dictionary<errorCode, string>();
            void ErrorCodeInit()
            {
                //BGAPI Errors
                ErrorCode.Add(errorCode.Invalid_Parameter, "Command contained invalid parameter");
                ErrorCode.Add(errorCode.Device_in_Wrong_State, "Device is in wrong state to receive command");
                ErrorCode.Add(errorCode.Out_Of_Memory, "Device has run out of memory");
                ErrorCode.Add(errorCode.Feature_Not_Implemented, "Feature is not implemented.");
                ErrorCode.Add(errorCode.Command_Not_Recognized, "Command was not recognized");
                ErrorCode.Add(errorCode.Timeout, "Command or Procedure failed due to timeout");
                ErrorCode.Add(errorCode.Not_Connected, "Connection handle passed is to command is not a valid handle");
                ErrorCode.Add(errorCode.Flow, "Command would cause either underflow or overflow error");
                ErrorCode.Add(errorCode.User_Attribute, "User attribute was accessed through API which is not supported");
                ErrorCode.Add(errorCode.Command_Too_Long, "Command maximum length exceeded");
                ErrorCode.Add(errorCode.Out_of_Bonds, "Bonding procedure can't be started because device has no space left for bond.");

                //Bluetooth Errors
                ErrorCode.Add(errorCode.Authentication_Failure, "Pairing or authentication failed due to incorrect results in the pairing or authentication procedure. This could be due to an incorrect PIN or Link Key");
                ErrorCode.Add(errorCode.Pin_or_Key_Missing, "Pairing failed because of missing PIN, or authentication failed because of missing Key.");
                ErrorCode.Add(errorCode.Memory_Capacity_Exceeded, "Controller is out of memory.");
                ErrorCode.Add(errorCode.Connection_Timeout, "Link supervision timeout has expired.");
                ErrorCode.Add(errorCode.Connection_Limit_Exceeded, "Controller is at limit of connections it can support.");
                ErrorCode.Add(errorCode.Command_Disallowed, "Command requested cannot be executed because the Controller is in a state where it cannot process this command at this time.");
                ErrorCode.Add(errorCode.Invalid_Command_Parameters, "Command contained invalid parameters.");
                ErrorCode.Add(errorCode.Remote_User_Terminated_Connection, "User on the remote device terminated the connection.");
                ErrorCode.Add(errorCode.Connection_Terminated_by_Local_Host, "Local device terminated the connection.");
                ErrorCode.Add(errorCode.LL_Response_Timeout, "Connection terminated due to link-layer procedure timeout.");
                ErrorCode.Add(errorCode.LL_Instant_Passed, "Received link-layer control packet where instant was in the past.");
                ErrorCode.Add(errorCode.Controller_Busy, "Operation was rejected because the controller is busy and unable to process the request.");
                ErrorCode.Add(errorCode.Unacceptable_Connection_Interval, "The Unacceptable Connection Interval error code indicates that the remote device terminated the connection because of an unacceptable connection interval.");
                ErrorCode.Add(errorCode.Directed_Advertising_Timeout, "Directed advertising completed without a connection being created.");
                ErrorCode.Add(errorCode.MIC_Failure, "Connection was terminated because the Message Integrity Check (MIC) failed on a received packet.");
                ErrorCode.Add(errorCode.Connection_Failed_to_be_Established, "LL initiated a connection but the connection has failed to be established. Controller did not receive any packets from remote end.");


                //Security Manager Protocol Errors


                //Attribute Protocol Errors
                ErrorCode.Add(errorCode.Invalid_Handle, "The attribute handle given was not valid on this server");
                ErrorCode.Add(errorCode.Read_Not_Permitted, "The attribute cannot be read");
                ErrorCode.Add(errorCode.Write_Not_Permitted, "The attribute cannot be written");
                ErrorCode.Add(errorCode.Invalid_PDU, "The attribute PDU was invalid");
                ErrorCode.Add(errorCode.Insufficient_Authentication, "The attribute requires authentication before it can be read or written.");
                ErrorCode.Add(errorCode.Request_Not_Supported, "Attribute Server does not support the request received from the client.");
                ErrorCode.Add(errorCode.Invalid_Offset, "Offset specified was past the end of the attribute");
                ErrorCode.Add(errorCode.Insufficient_Authorization, "The attribute requires authorization before it can be read or written.");
                ErrorCode.Add(errorCode.Prepare_Queue_Full, "Too many prepare writes have been queueud");
                ErrorCode.Add(errorCode.Attribute_Not_Found, "No attribute found within the given attribute handle range.");
                ErrorCode.Add(errorCode.Attribute_Not_Long, "The attribute cannot be read or written using the Read Blob Request");
                ErrorCode.Add(errorCode.Insufficient_Encryption_Key_Size, "The Encryption Key Size used for encrypting this link is insufficient.");
                ErrorCode.Add(errorCode.Invalid_Attribute_Value_Length, "The attribute value length is invalid for the operation");
                ErrorCode.Add(errorCode.Unlikely_Error, "The attribute request that was requested has encountered an error that was unlikely, and therefore could not be completed as requested.");
                ErrorCode.Add(errorCode.Insufficient_Encryption, "The attribute requires encryption before it can be read or written.");
                ErrorCode.Add(errorCode.Unsupported_Group_Type, "The attribute type is not a supported grouping attribute as defined by a higher layer specification.");
                ErrorCode.Add(errorCode.Insufficient_Resources, "Insufficient Resources to complete the request");
                ErrorCode.Add(errorCode.Application_Error_Codes, "Application error code defined by a higher layer specification.");
            }

            public enum errorCode
            {
                //BGAPI Errors
                Invalid_Parameter = 0x0180,
                Device_in_Wrong_State = 0x0181,
                Out_Of_Memory = 0x0182,
                Feature_Not_Implemented = 0x0183,
                Command_Not_Recognized = 0x0184,
                Timeout = 0x0185,
                Not_Connected = 0x0186,
                Flow = 0x0187,
                User_Attribute = 0x0188,
                Invalid_License_Key = 0x0189,
                Command_Too_Long = 0x018A,
                Out_of_Bonds = 0x018B,

                //Bluetooth Errors
                Authentication_Failure = 0x0205,
                Pin_or_Key_Missing = 0x0206,
                Memory_Capacity_Exceeded = 0x0207,
                Connection_Timeout = 0x0208,
                Connection_Limit_Exceeded = 0x0209,
                Command_Disallowed = 0x020C,
                Invalid_Command_Parameters = 0x0212,
                Remote_User_Terminated_Connection = 0x0213,
                Connection_Terminated_by_Local_Host = 0x0216,
                LL_Response_Timeout = 0x0222,
                LL_Instant_Passed = 0x0228,
                Controller_Busy = 0x023A,
                Unacceptable_Connection_Interval = 0x023B,
                Directed_Advertising_Timeout = 0x023C,
                MIC_Failure = 0x023D,
                Connection_Failed_to_be_Established = 0x023E,

                //Security Manager Protocol Errors



                //Attribute Protocol Errors
                Invalid_Handle = 0x0401,
                Read_Not_Permitted = 0x0402,
                Write_Not_Permitted = 0x0403,
                Invalid_PDU = 0x0404,
                Insufficient_Authentication = 0x0405,
                Request_Not_Supported = 0x0406,
                Invalid_Offset = 0x0407,
                Insufficient_Authorization = 0x0408,
                Prepare_Queue_Full = 0x0409,
                Attribute_Not_Found = 0x040A,
                Attribute_Not_Long = 0x040B,
                Insufficient_Encryption_Key_Size = 0x040C,
                Invalid_Attribute_Value_Length = 0x040D,
                Unlikely_Error = 0x040E,
                Insufficient_Encryption = 0x040F,
                Unsupported_Group_Type = 0x0410,
                Insufficient_Resources = 0x0411,
                Application_Error_Codes = 0x0480
            }

        }




        public class BMX055
        {

            public class Accel
            {
                public readonly float X;
                public readonly float Y;
                public readonly float Z;

                public readonly float RawX;
                public readonly float RawY;
                public readonly float RawZ;

                public readonly int AccelDataNumber;
                public readonly int DataNumber;
                public Accel(float rawX, float rawY, float rawZ, int accelDataNumber)
                {
                    this.RawX = rawX;
                    this.RawY = rawY;
                    this.RawZ = rawZ;
                    this.AccelDataNumber = accelDataNumber;
                }
            }

            public class Gyro
            {
                public readonly float X;
                public readonly float Y;
                public readonly float Z;

                public readonly float RawX;
                public readonly float RawY;
                public readonly float RawZ;

                public readonly int GyroDataNumber;
                public readonly int DataNumber;
                public Gyro(float rawX, float rawY, float rawZ, int gyroDataNumber)
                {
                    this.RawX = rawX;
                    this.RawY = rawY;
                    this.RawZ = rawZ;
                    this.GyroDataNumber = gyroDataNumber;
                }
            }

            public class Magno
            {
                public readonly float X;
                public readonly float Y;
                public readonly float Z;

                public readonly float RawX;
                public readonly float RawY;
                public readonly float RawZ;

                public readonly int MagnoDataNumber;
                public readonly int DataNumber;
                public Magno(float rawX, float rawY, float rawZ, int magnoDataNumber)
                {
                    this.RawX = rawX;
                    this.RawY = rawY;
                    this.RawZ = rawZ;
                    this.MagnoDataNumber = magnoDataNumber;
                }
            }


            //private readonly List<Accel> _listAccel = new List<Accel>();
            //private readonly List<Gyro> _listGyro = new List<Gyro>();
            //private readonly List<Magno> _listMAgno = new List<Magno>();

            //public List<Accel> ListAccel { get { return _listAccel; } }
            //public List<Gyro> ListGyro { get { return _listGyro; } }
            //public List<Magno> ListMagno { get { return _listMAgno; } }

            public List<Accel> ListAccel = new List<Accel>();
            public List<Gyro> ListGyro = new List<Gyro>();
            public List<Magno> ListMagno = new List<Magno>();

            protected struct MagnoTrimRegisters
            {
                /*! trim x1 data */
                public sbyte dig_x1;

                /*! trim y1 data */
                public sbyte dig_y1;

                /*! trim x2 data */
                public sbyte dig_x2;

                /*! trim y2 data */
                public sbyte dig_y2;

                /*! trim z1 data */
                public UInt16 dig_z1;

                /*! trim z2 data */
                public Int16 dig_z2;

                /*! trim z3 data */
                public Int16 dig_z3;

                /*! trim z4 data */
                public Int16 dig_z4;

                /*! trim xy1 data */
                public byte dig_xy1;

                /*! trim xy2 data */
                public sbyte dig_xy2;

                /*! trim xyz1 data */
                public UInt16 dig_xyz1;
            };
            protected MagnoTrimRegisters TrimData;

            protected Byte[] EnableAccelCommand(Optoel.Enable enable)
            {
                return new Byte[] { 0x01, 0x01, (byte)enable };
            }
            protected Byte[] EnableGyroCommand(Optoel.Enable enable)
            {
                return new Byte[] { 0x01, 0x02, (byte)enable };
            }
            protected Byte[] EnableMagnoCommand(Optoel.Enable enable)
            {
                return new Byte[] { 0x01, 0x04, (byte)enable };
            }


            protected Byte[] BwAccelCommand(Optoel.AccelBw accelBw)
            {
                return new Byte[] { 0x02, 0x01, (byte)accelBw };
            }
            protected Byte[] BwGyroCommand(Optoel.GyroBw gyroBw)
            {
                return new Byte[] { 0x02, 0x02, (byte)gyroBw };
            }
            protected Byte[] BwMagnoCommand(Optoel.MagnoBw magnoBw)
            {
                return new Byte[] { 0x02, 0x04, (byte)magnoBw };
            }


            protected Byte[] RangeAccelCommand(Optoel.AccelRange accelRange)
            {
                return new Byte[] { 0x03, 0x01, (byte)accelRange };
            }
            protected Byte[] RangeGyroCommand(Optoel.GyroRange gyroRange)
            {
                return new Byte[] { 0x03, 0x02, (byte)gyroRange };
            }


            protected Byte[] ReadEnableAccelCommand(Optoel.Enable enable)
            {
                return new Byte[] { 0x04, 0x01, (byte)enable };
            }
            protected Byte[] ReadEnableGyroCommand(Optoel.Enable enable)
            {
                return new Byte[] { 0x04, 0x02, (byte)enable };
            }
            protected Byte[] ReadEnableMagnoCommand(Optoel.Enable enable)
            {
                return new Byte[] { 0x04, 0x04, (byte)enable };
            }


            protected Byte[] StartCommand(Optoel.Enable AccelReadEnable, Optoel.Enable GyroReadEnable, Optoel.Enable MagnoReadEnable)
            {
                return new Byte[] { 0x05, (byte)AccelReadEnable, (byte)GyroReadEnable, (byte)MagnoReadEnable };
            }
            protected Byte[] StopCommand()
            {
                return new Byte[] { 0x06 };
            }

            protected Byte[] ReGetDataCommand()
            {
                return new Byte[] { 0x07 };
            }

        }


        public enum Sensor
        {
            Accel = 1,
            Gyro = 2,
            Magno = 3
        }


        /// <summary>
        /// Accelerometer okuma hızları.
        /// </summary>
        public enum AccelBw
        {
            None = 0xFF,
            /// <summary>
            /// 15.62 Hz (64 ms)
            /// </summary>
            AccelBw_15f625 = 0x08,
            /// <summary>
            /// 31.25 Hz (32 ms)
            /// </summary>
            AccelBw_31f25 = 0x09,
            /// <summary>
            /// 62.5 Hz (16 ms)
            /// </summary>
            AccelBw_62f5 = 0x0A,
            /// <summary>
            /// 125 Hz (8 ms)
            /// </summary>
            AccelBw_125 = 0x0B,
            /// <summary>
            /// 250 Hz (4 ms)
            /// </summary>
            AccelBw_250 = 0x0C
        }
        /// <summary>
        /// Gyroscope okuma hızları.
        /// </summary>
        public enum GyroBw
        {
            None = 0xFF,
            /// <summary>
            /// 100 Hz (10 ms)
            /// </summary>
            GyroBw_100Hz = 0x87,
            /// <summary>
            /// 200 Hz (5 ms)
            /// </summary>
            GyroBw_200Hz = 0x86
        }
        /// <summary>
        /// Magnetometer okuma hızları.
        /// </summary>
        public enum MagnoBw
        {
            None = 0xFF,
            /// <summary>
            /// 2 Hz (500 ms)
            /// </summary>
            MagnoBw_2Hz = 0x08,
            /// <summary>
            /// 6 Hz (166 ms)
            /// </summary>
            MagnoBw_6Hz = 0x10,
            /// <summary>
            /// 8 Hz (125 ms)
            /// </summary>
            MagnoBw_8Hz = 0x18,
            /// <summary>
            /// 10 Hz (100 ms)
            /// </summary>
            MagnoBw_10Hz = 0x00,
            /// <summary>
            /// 15 Hz (66 ms)
            /// </summary>
            MagnoBw_15Hz = 0x20,
            /// <summary>
            /// 20 Hz (50 ms)
            /// </summary>
            MagnoBw_20Hz = 0x28,
            /// <summary>
            /// 25 Hz (40 ms)
            /// </summary>
            MagnoBw_25Hz = 0x30,
            /// <summary>
            /// 30 Hz (33 ms)
            /// </summary>
            MagnoBw_30Hz = 0x38
        }

        public enum AccelRange
        {
            None = 0xFF,
            AccelRange_2g = 0x03,
            AccelRange_4g = 0x05,
            AccelRange_8g = 0x08,
            AccelRange_16g = 0x0C
        }

        public enum GyroRange
        {
            None = 0xFF,
            GyroRange_2000s = 0x00,
            GyroRange_1000s = 0x01,
            GyroRange_500s = 0x02,
            GyroRange_250s = 0x03,
            GyroRange_125s = 0x04
        }

        public enum Enable
        {
            Failed = 0x00,
            Enable = 0x01,
            Disable = 0x02
        }




    }
}
