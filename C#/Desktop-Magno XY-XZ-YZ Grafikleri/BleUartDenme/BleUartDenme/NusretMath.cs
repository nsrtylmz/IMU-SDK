using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BleUartDenme
{
    class NusretMath
    {
        /// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="start"> start index</param>
		/// <param name="end"> end index</param>
		/// <returns></returns>
		public static byte CalcCRC(List<byte> array, int start, int end)
        {
            int sam = 0;

            for (int i = start; i <= end; i++)
            {
                sam += array[i];
            }

            return (byte)sam;
        }

        public static byte CalcCRC(byte[] array, int start, int end)
        {
            int sum = 0;

            for (int i = start; i <= end; i++)
            {
                sum += array[i];
            }

            return (byte)sum;
        }

        /// <summary>
        /// Satirin tamamının checkSum'ını bulur.
        /// </summary>
        /// <param name="satir"></param>
        /// <returns></returns>
        public static byte CalcCRC(string satir)
        {
            int sam = 0;

            for (int i = 0; i < satir.Length; i++)
            {
                sam += (byte)satir[i];
            }

            return (byte)sam;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="satir"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string satir)
        {
            byte[] byts = new byte[satir.Length];
            for (int i = 0; i < satir.Length; i++)
            {
                byts[i] = (byte)satir[i];
            }

            return byts;
        }


        public static class ComTitle
        {
            /* #Shelling
             * [0] Head
             * [1] Head
             * [2] InfactData Lenght
             * 
             * .
             * .
             * .
             * 
             * [n] CRC
             * [n+1] End
             * 
             */


            /* # InfactData
             * 
             * 
             * 
             * 
             * 
             * 
             * 
             * 
             * 
             * 
             */


            /// <summary>
            /// Paket basi.
            /// </summary>
            public const byte Head = 0xFF;
            /// <summary>
            /// Paket Sonu.
            /// </summary>
            public const byte End = 0xFE;
            /// <summary>
            /// Paketteki degerlerin ayiricisi.
            /// </summary>
            public const byte Isolator = 0xFD;
            /// <summary>
            /// Paket ayiricisi veya sonu.
            /// </summary>
            public const byte PacketIsolator = 0xFC;


            public class Titles
            {

                /// <summary>
                /// 
                /// </summary>
                public const byte T_None = 0;
                /// <summary>
                /// Raport basligi.
                /// </summary>
                public const byte T_Report = 1;


            }
        }


        public class UARTListen
        {

            public List<_timeData> ListGettingData = new List<_timeData>();
            public List<_timeData> ListSendingData = new List<_timeData>();
            public List<_timeData> ListSendingGettingData = new List<_timeData>();

            public enum DataWay
            {
                Sending = 0,
                Getting = 1
            }

            public class _timeData
            {
                public byte[] Data;
                public DateTime time;
                public DataWay dataWay;
                public string st;
            }

            public void GettingData(byte[] data)
            {
                _timeData gettingData = new _timeData();

                //Array.Copy(data, gettingData.Data, data.Length);
                gettingData.Data = data;
                
                gettingData.time.AddMinutes(DateTime.Now.Minute);
                gettingData.time.AddSeconds(DateTime.Now.Second);
                gettingData.time.AddMilliseconds(DateTime.Now.Millisecond);

                ListGettingData.Add(gettingData);
            }

            public void SendingData(byte[] data)
            {
                _timeData sendingData = new _timeData();

                Array.Copy(data, sendingData.Data, data.Length);

                sendingData.time.AddMinutes(DateTime.Now.Minute);
                sendingData.time.AddSeconds(DateTime.Now.Second);
                sendingData.time.AddMilliseconds(DateTime.Now.Millisecond);

                ListSendingData.Add(sendingData);
            }

           

            public void SendingGettingData(byte[] data, DataWay dataWay)
            {
                _timeData timeData = new _timeData();

                timeData.st = DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + ":" + DateTime.Now.Millisecond;

                timeData.Data = new byte[data.Length];
                Array.Copy(data, timeData.Data, data.Length);

                //timeData.time.AddMinutes(DateTime.Now.Minute);
                //timeData.time.AddSeconds(DateTime.Now.Second);
                //timeData.time.AddMilliseconds(DateTime.Now.Millisecond);

                timeData.dataWay = dataWay;

                ListSendingGettingData.Add(timeData);
            }

        }

    }
}
