﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace BleUartDenme
{
    class Settings
    {
        public class _ComPortSetting
        {
            public string ComName { get; set; } = "Com?";
            public int BautRade { get; set; } = 921600;
        }


        public class _StaticSetting
        {
            public _ComPortSetting SP_PortSetting = new _ComPortSetting();
            public _ComPortSetting Ble_PortSetting = new _ComPortSetting();
            //public _StaticSetting()
            //{
            //    SP_PortSetting.BautRade = 115200;
            //    Ble_PortSetting.BautRade = 921600;
            //}
        }
        public static _StaticSetting StaticSetting = new _StaticSetting();




        static string SettingsFilePath = "Setting.setting";
       
        public static _result GetStaticSettings()
        {
            _result rslt = new _result();

            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    StaticSetting = JsonConvert.DeserializeObject<_StaticSetting>(json);
                    rslt.Succes = true;
                }
                else
                {
                    rslt.Succes = false;
                    rslt.Message = "'" + SettingsFilePath + "' adında bir dosya bulunamadı!";
                }
            }
            catch (Exception ex)
            {

                rslt.Succes = false;
                rslt.Message = ex.Message;
            }

            return rslt;
        }


        public static _result SaveStaticSettings()
        {
            _result rslt = new _result();

            try
            {
                string json = JsonConvert.SerializeObject(StaticSetting);
                File.WriteAllText(SettingsFilePath, json);
                rslt.Succes = true;
            }
            catch (Exception ex)
            {
                rslt.Succes = false;
                rslt.Message = ex.Message;
            }

            return rslt;
        }


        static string MagCompanFilePath = "MagCompanFilePath.json";

        public class MagCompan
        {
            public float[] offset = new float[3] { 0, 0, 0 };
            public float[] scale = new float[3] { 0, 0, 0 };
            public float[] m = new float[3] { 0, 0, 0 };
        }

        public static _result SaveMagCompan(MagCompan magCompan)
        {
            _result rslt = new _result();

            try
            {
                string json = JsonConvert.SerializeObject(magCompan);
                File.WriteAllText(MagCompanFilePath, json);
                rslt.Succes = true;
            }
            catch (Exception ex)
            {
                rslt.Succes = false;
                rslt.Message = ex.Message;
            }

            return rslt;
        }

        public static MagCompan GetMagCompan()
        {
            MagCompan magCompan = null;
            
            try
            {
                if (File.Exists(MagCompanFilePath))
                {
                    string json = File.ReadAllText(MagCompanFilePath);
                    magCompan = JsonConvert.DeserializeObject<MagCompan>(json);
                }  
            }
            catch (Exception ex)
            {      
            }
            
            return magCompan;
        }


        public class _result
        {
            public bool Succes = false;
            public string Message = "";
        }
    }
}
