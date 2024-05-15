using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using AppEWMFlag.CallModel;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.Linq;
using System.Windows.Documents;
using System.Data;
using System.Data.SqlClient;
using System;

namespace AppEWMFlag
{
    class Program
    {
        static List<SAP> plant = new List<SAP>();
        static void Main(string[] args)
        {
            string _block = "Downloading";
            Console.WriteLine(_block + "... %");

            string data = "SELECT T.SAP_COMPANY,T.SAP_PLANT,T.FACTORY,C.SCHEMA,'' AS DB_INK FROM SFCS.C_SAP_COMPANY_FACTORY_T T INNER JOIN SFCS.C_FACTORY_AREA_T C ON T.FACTORY = C.FACTORY AND T.VALID = 1 WHERE T.SAP_COMPANY IN('TH00')";
            using (OracleConnection conn = ComFuncOracle.GetOracleConnection())
            {
                var _with1 = conn;
                if (_with1.State == ConnectionState.Open)
                    _with1.Close();
                _with1.Open();

                OracleCommand cmd = new OracleCommand(data, conn);
                OracleDataReader reader;
                reader = cmd.ExecuteReader();
                plant.Clear();
                while (reader.Read())
                {

                    SAP item = new SAP();
                    item.plant_sap = reader["SAP_PLANT"].ToString();
                    item.factory = reader["SCHEMA"].ToString();
                    plant.Add(item);

                }
            }
            string data6 = "SELECT T.SAP_COMPANY,T.SAP_PLANT,T.FACTORY,C.SCHEMA,'' AS DB_INK FROM SFCS.C_SAP_COMPANY_FACTORY_T@det06_fm T INNER JOIN SFCS.C_FACTORY_AREA_T C ON T.FACTORY = C.FACTORY AND T.VALID = 1 WHERE T.SAP_COMPANY IN('TH00')";
            using (OracleConnection conn = ComFuncOracle.GetOracleConnection())
            {
                var _with1 = conn;
                if (_with1.State == ConnectionState.Open)
                    _with1.Close();
                _with1.Open();

                OracleCommand cmd = new OracleCommand(data6, conn);
                OracleDataReader reader;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    SAP item = new SAP();
                    item.plant_sap = reader["SAP_PLANT"].ToString();
                    item.factory = reader["SCHEMA"].ToString();
                    plant.Add(item);

                }
            }
            foreach (var factory in plant)
            {
            var date = DateTime.Now.ToString("yyyyMMddhhmmss");
            var date1 = DateTime.Now.ToString("yyyyMMdd");
            var dateFor = (date1 + "000000");
            var dateTo = (date1 + "235959");

            String username = "DETMOA_User";
            String password = "May2021@DETMOA";
            //String username = "DETMES_USER";
            //String password = "Delta123";
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

            //production
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://twtpepoap1.delta.corp:56500/RESTAdapter/DETMES/MM/YTMEWM_GET_ACTIVE_PRODUCT");

            //QA
            //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://twtpepgbqa1.delta.corp:56800/RESTAdapter/DETMES/MM/YTMEWM_GET_ACTIVE_PRODUCT");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
            httpWebRequest.Method = "POST";
            var result = "";
                ////string plant_sap = factory.plant_sap;
                string plant_sap = "EVB1";
                //string factory_M = factory.factory;
                //string factory_M = factory.factory;
                string factory_M = "DET_AM";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                    Call_Json jsonF = new Call_Json()
                    {
                        I_DATE_FR = "20231001000000",
                        I_DATE_TO = "20231025235959",
                        I_PLANT = plant_sap
                    };
                    //Call_Json jsonF = new Call_Json()
                    //{
                    //    I_DATE_FR = dateFor,
                    //    I_DATE_TO = dateTo,
                    //    I_PLANT = plant_sap
                    //};


                    var jsondata = JsonConvert.SerializeObject(jsonF);
                streamWriter.Write(jsondata);

            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
            result = streamReader.ReadToEnd();

            }
            var datarate = JObject.Parse(result);

            var srcArray = datarate.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                }

                trgArray.Add(cleanRow);
            }
            Console.Clear();
            Console.Write(_block + "...... 50%");

            string value_name = datarate.SelectToken("OT_PRODUCT").ToString();
            RootObject myData = JsonConvert.DeserializeObject<RootObject>(value_name);
            //Item items = < Item > (myData);
            //Item singleWord = myData.item[0];
            for (int i = 0; i < myData.item.Count; i++)
            {
                 Item singleWord = myData.item[i];
                    if (myData.item[i] != null)
                    {
                        string MATNR = singleWord.MATNR.ToString();
                        string SERIAL;
                        if (myData.item[i].SERIAL == null)
                        {
                            SERIAL = "";
                        }
                        else
                        {
                            SERIAL = singleWord.SERIAL.ToString();
                        }
                        string CREATEUTC = singleWord.CREATEUTC.ToString();
                        //foreach ()
                        //{
                            using (SqlConnection connn = ComFuncSQL.GetSQLConnection())
                            {
                                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand();

                                command.CommandType = System.Data.CommandType.Text;

                                command.CommandText = "insert into YTMEWM_GET_ACTIVE_PRODUCT([MODEL],[SERIAL],[CREATE_DATE],[DATETIMETOSET]) values('" + MATNR + "','" + factory_M + "','" + CREATEUTC + "','" + plant_sap + "')";
                                command.Connection = connn;

                                connn.Open();
                                command.ExecuteNonQuery();
                                connn.Close();

                            string dataSelect = "SELECT MODEL_NAME FROM " + factory_M + ".C_MODEL_DESC_T  WHERE MODEL_NAME='" + MATNR + "'";
                            using (OracleConnection conn = ComFuncOracle.GetOracleConnection())
                            {
                                var _with1 = conn;
                                if (_with1.State == ConnectionState.Open)
                                    _with1.Close();
                                _with1.Open();

                                OracleCommand cmd = new OracleCommand(dataSelect, conn);
                                OracleDataReader reader;
                                reader = cmd.ExecuteReader();
                                //plant.Clear();
                                while (reader.Read())
                                {

                                    SAP item = new SAP();
                                    item.plant_sap = reader["SAP_PLANT"].ToString();
                                    item.factory = reader["SCHEMA"].ToString();
                                    plant.Add(item);

                                }
                            }
                            if (factory_M != "DET_FM" & factory_M != "DET_CNDC")
                                {
                                    


                                    string updata = "UPDATE " + factory_M + ".C_MODEL_DESC_T SET WMS_SEND_FLAG='1',CHANGE_DATE=SYSDATE, CHANGE_EMP='86204660' WHERE MODEL_NAME='" + MATNR + "' AND WMS_SEND_FLAG='0'";
                                    //string updataQ = "UPDATE " + factory_M + ".C_MODEL_DESC_T SET WMS_SEND_FLAG='1',CHANGE_DATE=SYSDATE, CHANGE_EMP='86204660' WHERE MODEL_NAME='22B-D1P4N104' AND WMS_SEND_FLAG='0'";

                                    using (OracleConnection conn = ComFuncOracle.GetOracleConnection())

                                    {
                                        conn.Open();
                                        OracleCommand cmd = conn.CreateCommand();
                                        cmd.CommandText = updata;

                                        int rowsUpdated = cmd.ExecuteNonQuery();
                                    }
                                }
                                else if (factory_M == "DET_FM")
                                {

                                    string updata = "UPDATE " + factory_M + ".C_MODEL_DESC_T@det06_fm SET WMS_SEND_FLAG='1',CHANGE_DATE=SYSDATE, CHANGE_EMP='86204660' WHERE MODEL_NAME='" + MATNR + "' AND WMS_SEND_FLAG='0'";
                                    //string updataQ = "UPDATE " + factory_M + ".C_MODEL_DESC_T SET WMS_SEND_FLAG='1',CHANGE_DATE=SYSDATE, CHANGE_EMP='86204660' WHERE MODEL_NAME='22B-D1P4N104' AND WMS_SEND_FLAG='0'";
                                    using (OracleConnection conn = ComFuncOracle.GetOracleConnection())

                                    {
                                        conn.Open();
                                        OracleCommand cmd = conn.CreateCommand();
                                        cmd.CommandText = updata;

                                        int rowsUpdated = cmd.ExecuteNonQuery();
                                    }

                                }
                                else if (factory_M == "DET_CNDC")
                                {
                                    string updata = "UPDATE " + factory_M + ".C_MODEL_DESC_T@det06_cndc SET WMS_SEND_FLAG='1',CHANGE_DATE=SYSDATE, CHANGE_EMP='86204660' WHERE MODEL_NAME='" + MATNR + "' AND WMS_SEND_FLAG='0'";
                                    //string updataQ = "UPDATE " + factory_M + ".C_MODEL_DESC_T SET WMS_SEND_FLAG='1',CHANGE_DATE=SYSDATE, CHANGE_EMP='86204660' WHERE MODEL_NAME='22B-D1P4N104' AND WMS_SEND_FLAG='0'";
                                    using (OracleConnection conn = ComFuncOracle.GetOracleConnection())

                                    {
                                        conn.Open();
                                        OracleCommand cmd = conn.CreateCommand();
                                        cmd.CommandText = updata;

                                        int rowsUpdated = cmd.ExecuteNonQuery();
                                    }

                                }
                            }
                        }
                    //}
                    else 
                    { 
                    
                    }
            }
                Console.Clear();
                Console.Write(_block + "......... 100%");

            }
        }

       
    }
    class ComFuncSQL
    {
        public static SqlConnection GetSQLConnection()
        {
            Data Source = THBPOCIMDB\THBPOCIMDB; Initial Catalog = MESPRDDB; Persist Security Info = True; User ID = MESDB
            string connString = "Server=THBPOPRODQADB; Database=MESQADB; User=MESDB; Password=MES12345";

            return new SqlConnection(connString);
        }
      
    }

    class ComFuncOracle
    {
        public static OracleConnection GetOracleConnection()
        {

            string host = "THPUBMES-SCAN";
            int port = 1521;
            string sid = "THPUBMES";
            string user = "sfcs";
            string password = "sfc$2oo7";

            // 'Connection string' to connect directly to Oracle.
            string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
                 + host + ")(PORT = " + port + "))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = "
                 + sid + ")));Password=" + password + ";User ID=" + user;

            return new OracleConnection(connString);
        }
        public static OracleConnection GetOracleQAConnection()
        {

            string host = "10.148.202.12";
            int port = 1521;
            string sid = "MESQADB";
            string user = "DET_PS";
            string password = "detpsqa2020";

            // 'Connection string' to connect directly to Oracle.
            string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
                 + host + ")(PORT = " + port + "))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = "
                 + sid + ")));Password=" + password + ";User ID=" + user;

            return new OracleConnection(connString);
        }          
    }
}
