using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Threading;
using ADOX;
using Dm;
using System.Windows.Forms;
namespace Auto_test_db
{
	/// <summary>
	/// ProClass 的摘要说明。
	/// </summary>
	public class ProClass
	{

        private static OleDbConnection cn;
      //  private static DmConnection cn;


        	private static OleDbCommand cm;
        //private static DmCommand cm;
        public static ClassDmServer cDServer;
		//新增 与服务器组件进行通信的类
		public static NETCommunication CommServer;
		public static ValClass vlc;
	//	public static ArrayList m_XmlFileDirectory;		//用来存放XML文件的目录信息。

		public static string ProServer = "主服务器";
		public static string ProDatabase = "默认库";
		public static string ProUserId = "用户名";
		public static string ProPassword = "口令";
//		public static string ProProt = "端口";
		public static string ProMsgNum = "消息条数";
		public static string ProIsOutMsg = "是否输出消息";
		public static string ProMsgCheckTime = "消息检察间隔";
		public static string ProIsSaveMsg = "是否自动保存消息";
		public static string ProIsMsgSpan = "是否输出消息间隔";
		public static string ProIsOutTime = "是否输出执行时间";
		public static string ProThreadNum = "运行的线程个数";
		public static string ProIsRandRun = "是否随机选择测例";
		public static string ProIsAutoClearMsg = "是否自动清空消息";
		public static string ProIsLoop = "是否无限运行测例";
		public static string ProIsOutVoice = "是否报错发生提示";
		public static string ProIsErrRun = "是否发生错误继续运行";
		public static string ProIsSaveSql = "是否保存执行的语句";
		public static string ProSaveSqlNum = "保存语句的条数";
		public static string ProIsAutoRunServer = "是否服务器异常自起动";
		public static string ProServerCheckTime = "服务器检查间隔";
		public static string ProServerPath = "主服务器路径";
        //public static string ProServerPath= Environment.GetEnvironmentVariable("DM_HOME"); 
		public static string ProXMLFilePath="XML测试文件路径";
		public static string ProDriveName = "OLEDB驱动名称";
		public static string ProConnectPool = "启用连接池";
		public static string ProIsShowResult = "是否显示结果集";
		public static string ProServer0 = "服务器0";
		public static string ProServer1 = "服务器1";
		public static string ProServer2 = "服务器2";
		public static string ProServer3 = "服务器3";
		public static string ProServer4 = "服务器4";
		public static string ProServer5 = "服务器5";
		public static string ProServer6 = "服务器6";
		public static string ProServer7 = "服务器7";
		public static string ProServer8 = "服务器8";
		public static string ProServer9 = "服务器9";
		public static string ProIsWindows = "是否是测试WINDOWS服务器";
		public static string ProLevel1 = "离散的用例等级";
		public static string ProLevel2 = "连续的用例等级";
		public static string sValServer = "";
		public static string sValDatabase = "";
		public static string sValUserId = "";
		public static string sValPassword = "";
		public static string sValMsgNum = "";
		public static string sValIsOutMsg = "";
		public static string sValMsgCheckTime = "";
		public static string sValIsSaveMsg = "";
		public static string sValIsMsgSpan = "";
		public static string sValIsOutTime = "";
		public static string sValThreadNum = "";
		public static string sValIsRandRun = "";
		public static string sValIsAutoClearMsg = "";
		public static string sValIsLoop = "";
		public static string sValIsOutVoice = "";
		public static string sValIsErrRun = "";
		public static string sValIsSaveSql = "";
		public static string sValSaveSqlNum = "";
		public static string sValIsAutoRunServer = "";
		public static string sValConnectPool = "";
		public static string sValServerCheckTime = "";
		public static string sValServerPathDir = "";
		public static string sValGetDmIni="0";
		public static string sValOldXMLFilePath="";   //存放旧的xml文件路径
		public static string sValOldServerIP="";     //存放旧的服务器IP
		public static string sValXMLFilePath="";    //存放当前xml文件的路径
		public static string sValDriveName = "";
		public static string sValServer0 = "";
		public static string sValServer1 = "";
		public static string sValServer2 = "";
		public static string sValServer3 = "";
		public static string sValServer4 = "";
		public static string sValServer5 = "";
		public static string sValServer6 = "";
		public static string sValServer7 = "";
		public static string sValServer8 = "";
		public static string sValServer9 = "";
		public static string sValIsWindows = "";		
		public static string sValIsShowResult = "";
		public static string sValLevel1 = "";
		public static string sValLevel2 = "";
		public static bool bValIsSaveSql;
		public static bool bValIsMsgSpan;
		public static bool bValIsOutMsg;
		public static bool bValIsOutVoice;
		public static bool bValIsRandRun;
		public static bool bValIsOutTime;
		public static bool bValIsErrRun;
		public static bool bValIsShowResult;
		public static bool bValIsLevel;//根据此变量的值，判断是选择连续的测试等级还是离散的测试等级
		public static string iValLevel1;
		public static int iValLevel2;
		public static int  iValMsgNum;


		public ProClass(MainForm _mForm)
		{
			if (vlc == null) {
				vlc = new ValClass();
			}
			if (cDServer == null) {
				cDServer = new ClassDmServer(_mForm);
			}	
			//
			if(CommServer==null)
			{
				CommServer = new NETCommunication();
				
			}
			SetDefault();
			Connect();			
			InitProVals();
			CommServer.sleepTime = sValServerCheckTime;

		}
		~ProClass()
		{
			
		}
		
		public static void CloseProClass()
		{
			if (cn != null && cn.State == ConnectionState.Open) 
			{
				cn.Close();
			}
		}
		private bool Connect()
		{
            bool proexsit = false;
            bool saveproexsit = false;
            bool test_resultexsit = false;
            String constr = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source = .\\provalues.accdb; Persist Security Info = False";
            try
            {
                ADOX.Catalog cat = new ADOX.Catalog();
                ADODB.Connection conn = new ADODB.Connection();
                conn.Open(constr, null, null, -1);
                cat.ActiveConnection = conn;
                ADOX.Tables tbl = cat.Tables;
                for (int i = 0; i < cat.Tables.Count; i++)
                {
                    if (cat.Tables[i].Name == "PRO")
                        proexsit = true;
                    if (cat.Tables[i].Name == "SAVEPRO")
                        saveproexsit = true;
                    if (cat.Tables[i].Name == "TEST_RESULT")
                        test_resultexsit = true;
                }
                conn.Close();
                cat = null;
            }
            catch (Exception EEE)
            {
                MessageBox.Show("加载Interop.Adox组件失败了，检查一下程序目录下面是否有相应文件：" + EEE.Message,
                                 "加载Interop.Adox组件出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
			{
                cn = new OleDbConnection(constr);
                cn.Open();
                cm = cn.CreateCommand();
                if (!proexsit)
                {
                    cm.CommandText = "CREATE TABLE PRO([NAME] VARCHAR(200) PRIMARY KEY, VAL	VARCHAR(255))";
                    cm.ExecuteNonQuery();//创建之前判断存在否
                }
                if (!saveproexsit)
                {
                    cm.CommandText = "CREATE TABLE SAVEPRO([NAME] VARCHAR(200) PRIMARY KEY, VAL	TEXT)";
                    cm.ExecuteNonQuery();//创建之前判断存在否
                }
                if (!test_resultexsit)
                {
                    cm.CommandText = "CREATE TABLE TEST_RESULT(ID INT IDENTITY(1,1) PRIMARY KEY, [NAME] VARCHAR(200), [PATH] VARCHAR(250), TEST_TIME DATETIME DEFAULT NOW, IS_SUCCESS CHAR(1))";
                    cm.ExecuteNonQuery();//创建之前判断存在否
                }
                return true;
			}
			catch(Exception e)
			{           
                MessageBox.Show("操作Access数据库出错,错误消息为:" + e.Message,
                                    "程序的连接属性可能无法保存，请检查connect函数或者数据库", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
			}
		}
		public static void InsertTestResult(string _name, string _path, string is_success)
		{
			if(!ProClass.GetIsSaveMsg())
			{
				return;
			}
			string sql = "INSERT INTO TEST_RESULT([NAME], [PATH], IS_SUCCESS) VALUES('";
			sql += _name + "','" + _path + "','" + is_success + "')";
			Monitor.Enter(vlc);
			try
			{
				cm.CommandText = sql;
				cm.ExecuteNonQuery();
			}
			catch
			{
			}
			Monitor.Exit(vlc);
		}
		public static void RebuileTestResult()
		{
			Monitor.Enter(vlc);
			try
			{
				cm.CommandText = "DROP TABLE TEST_RESULT";
				cm.ExecuteNonQuery();				
			}
			catch
			{
				try
				{
                    cm.CommandText = "CREATE TABLE TEST_RESULT(ID INT IDENTITY(1,1) PRIMARY KEY, [NAME] VARCHAR(200), [PATH] VARCHAR(250), TEST_TIME DATETIME DEFAULT NOW, IS_SUCCESS CHAR(1))";
                    //cm.CommandText = "CREATE TABLE TEST_RESULT(ID INT IDENTITY(1,1) PRIMARY KEY, NAME VARCHAR(200), PATH VARCHAR(250), TEST_TIME DATETIME DEFAULT NOW, IS_SUCCESS CHAR(1))";
                    cm.ExecuteNonQuery();
				}
				catch
				{
				}
			}
			Monitor.Exit(vlc);
		}
		public static string ImportTestResult()
		{
			string ret = null;
			Monitor.Enter(vlc);
			try
			{
                	cm.CommandText = "SELECT [NAME], [PATH], TEST_TIME, IS_SUCCESS FROM TEST_RESULT";
               // cm.CommandText = "SELECT NAME, PATH , TEST_TIME, IS_SUCCESS FROM TEST_RESULT";

                 	OleDbDataReader dr = cm.ExecuteReader();
             //   DmDataReader dr = cm.ExecuteReader();
                ret = cDServer.ImportTestResult(dr);
				dr.Close();
			}
			catch(OleDbException e)
			{
				return e.Message;
			}
			Monitor.Exit(vlc);
			return ret;
		}
		public static String GetSaveProValue(String _name)
		{
			Monitor.Enter(vlc);
			String ret = (string)vlc.GetVal(_name);
			Monitor.Exit(vlc);
			return ret;
		}

		public static void AddSaveProValue(String _name, String _val)
		{
			Monitor.Enter(vlc);
			vlc.SetVal(_name, _val);
			try
			{
                cm.CommandText = "DELETE FROM SAVEPRO WHERE [NAME] = '" + _name + "'";
                //cm.CommandText = "DELETE FROM SAVEPRO WHERE NAME = '" + _name + "'";
                cm.ExecuteNonQuery();
                	cm.CommandText = "INSERT INTO SAVEPRO([NAME], VAL) VALUES('" + _name + "', '" + _val + "')";
               // cm.CommandText = "INSERT INTO SAVEPRO(NAME, VAL) VALUES('" + _name + "', '" + _val + "')";
                cm.ExecuteNonQuery();
			}
			catch
			{
			}
			Monitor.Exit(vlc);
		}

		public static void ClearSaveProValue()
		{
			Monitor.Enter(vlc);
			try
			{
				cm.CommandText = "DELETE FROM SAVEPRO";
				cm.ExecuteNonQuery();
			}
			catch
			{
			}
			Monitor.Exit(vlc);
		}

		public static String GetProValue(String _name)
		{
			String ret = null;
            	String sql = "SELECT VAL FROM [PRO] WHERE [NAME] = '" + _name + "'";
            //String sql = "SELECT VAL FROM PRO WHERE NAME = '" + _name + "'";
            if (cn.State != ConnectionState.Open){
				return ret;
			}
			cm.CommandText = sql;
			try
			{
				ret = Convert.ToString(cm.ExecuteScalar());
			}
			catch(Exception e)
			{
				String ss = e.Message;
			}
			return ret;
		}
		
		public static String GetProValue(String _name, String _defaultVal)
		{
			String ret = _defaultVal;
			Object rc;
            	String sql = "SELECT VAL FROM [PRO] WHERE [NAME] = '" + _name + "'";
            //String sql = "SELECT VAL FROM PRO WHERE NAME = '" + _name + "'";
            if (cn == null || cn.State != ConnectionState.Open)
			{
				return ret;
			}
			cm.CommandText = sql;
			try
			{
				rc = cm.ExecuteScalar();
				if (rc != null && DBNull.Value != rc) {
					ret = Convert.ToString(rc);
				}				
			}
			catch(Exception e)
			{
				String ss = e.Message;
			}
			return ret;
		}

		private static void SetProVal(String _name, String _val)
		{
			if (_name.CompareTo(ProServer) == 0) {
				sValServer = _val;
			}
			else if (_name.CompareTo(ProDatabase) == 0) {
				sValDatabase = _val;
			}
			else if (_name.CompareTo(ProUserId) == 0) 
			{
				sValUserId = _val;
			}
			else if (_name.CompareTo(ProPassword) == 0) 
			{
				sValPassword = _val;
			}
			else if (_name.CompareTo(ProLevel1) == 0) 
			{
				sValLevel1 = _val;
				iValLevel1 = GetLevel1();
			}
			else if (_name.CompareTo(ProLevel2) == 0) 
			{
				sValLevel2 = _val;
				iValLevel2 = GetLevel2();
			}
			else if (_name.CompareTo(ProMsgNum) == 0) 
			{
				sValMsgNum = _val;
				iValMsgNum = GetMsgNum();
			}
			else if (_name.CompareTo(ProIsOutMsg) == 0) 
			{
				sValIsOutMsg = _val;
				bValIsOutMsg = GetIsShowMsg();
			}
			else if (_name.CompareTo(ProMsgCheckTime) == 0) 
			{
				sValMsgCheckTime = _val;
			}
			else if (_name.CompareTo(ProIsSaveMsg) == 0) 
			{
				sValIsSaveMsg = _val;
			}
			else if (_name.CompareTo(ProIsMsgSpan) == 0) 
			{
				sValIsMsgSpan = _val;
				bValIsMsgSpan = GetIsMsgSpan();
			}
			else if (_name.CompareTo(ProIsOutTime) == 0) 
			{
				sValIsOutTime = _val;
				bValIsOutTime = GetIsOutTime();
			}
			else if (_name.CompareTo(ProThreadNum) == 0) 
			{
				sValThreadNum = _val;
			}
			else if (_name.CompareTo(ProIsRandRun) == 0) 
			{
				sValIsRandRun = _val;
				bValIsRandRun = GetIsRandRun();
			}
			else if (_name.CompareTo(ProIsAutoClearMsg) == 0) 
			{
				sValIsAutoClearMsg = _val;
			}
			else if (_name.CompareTo(ProIsLoop) == 0) 
			{
				sValIsLoop = _val;
			}
			else if (_name.CompareTo(ProIsShowResult) == 0) {
				sValIsShowResult = _val;
				bValIsShowResult = GetIsShowResult();
			}
			else if (_name.CompareTo(ProIsOutVoice) == 0) 
			{
				sValIsOutVoice = _val;
				bValIsOutVoice = GetIsOutVoice();
			}
			else if (_name.CompareTo(ProIsErrRun) == 0) 
			{
				sValIsErrRun = _val;
				bValIsErrRun = GetIsErrRun();
			}
			else if (_name.CompareTo(ProIsSaveSql) == 0) 
			{
				sValIsSaveSql = _val;
				bValIsSaveSql = GetIsSaveSql();
			}
			else if (_name.CompareTo(ProSaveSqlNum) == 0) 
			{
				sValSaveSqlNum = _val;
			}
			else if (_name.CompareTo(ProIsAutoRunServer) == 0) 
			{
				sValIsAutoRunServer = _val;
			}
			else if (_name.CompareTo(ProServerCheckTime) == 0) 
			{
				sValServerCheckTime = _val;
			}
			else if (_name.CompareTo(ProDriveName) == 0) 
			{
				sValDriveName = _val;
			}
			else if (_name.CompareTo(ProConnectPool) == 0) 
			{
				sValConnectPool = _val;
			}
			else if (_name.CompareTo(ProServer0) == 0) 
			{
				sValServer0 = _val;
			}
			else if (_name.CompareTo(ProServer1) == 0) 
			{
				sValServer1 = _val;
			}
			else if (_name.CompareTo(ProServer2) == 0) 
			{
				sValServer2 = _val;
			}
			else if (_name.CompareTo(ProServer3) == 0) 
			{
				sValServer3 = _val;
			}
			else if (_name.CompareTo(ProServer4) == 0) 
			{
				sValServer4 = _val;
			}
			else if (_name.CompareTo(ProServer5) == 0) 
			{
				sValServer5 = _val;
			}
			else if (_name.CompareTo(ProServer6) == 0) 
			{
				sValServer6 = _val;
			}
			else if (_name.CompareTo(ProServer7) == 0) 
			{
				sValServer7 = _val;
			}
			else if (_name.CompareTo(ProServer8) == 0) 
			{
				sValServer8 = _val;
			}
			else if (_name.CompareTo(ProServer9) == 0) 
			{
				sValServer9 = _val;
			}
			else if (_name.CompareTo(ProIsWindows) == 0) 
			{
				sValIsWindows = _val;
			}			
		}

		public static bool AddProValue(String _name, String _val)
		{
			if (cn == null || cn.State != ConnectionState.Open)
			{
				return false;
			}
            String updatesql = "UPDATE [PRO] SET VAL='" + _val + "' WHERE [NAME]='" + _name + "'";
            String insertsql = "INSERT INTO [PRO] VALUES('" + _name + "', '" + _val + "')";
           // String updatesql = "UPDATE PRO SET VAL='" + _val + "' WHERE NAME ='" + _name + "'";
            //String insertsql = "INSERT INTO PRO VALUES('" + _name + "', '" + _val + "')";
            cm.CommandText = updatesql;
			try
			{
				if(cm.ExecuteNonQuery() == 0)
				{
					cm.CommandText = insertsql;
					cm.ExecuteNonQuery();
				}
				SetProVal(_name, _val);
				return true;
			}
			catch(Exception e)
			{
				String ss = e.Message;				
			}
			return false;
		}
		private void SetDefault()
		{
			sValServer = "LOCALHOST:5236";
			sValDatabase = "SYSTEM";   //            
			sValUserId = "SYSDBA";
			sValPassword = "SYSDBA";
//			sValProt = "12345";
			sValMsgNum = "5000";
			sValLevel1 ="";
			sValLevel2 = "15";
			bValIsLevel=true;
			sValIsOutMsg = "True";//是否输出消息
			sValMsgCheckTime = "300";
			sValIsSaveMsg = "False";
			sValIsMsgSpan = "False";//是否输出消息间隔
			sValIsOutTime = "False";
			sValThreadNum = "1";
			sValIsRandRun = "False";
			sValIsAutoClearMsg = "True";
			sValIsLoop = "False";
			sValIsOutVoice = "False";
			sValIsErrRun = "False";
			sValIsSaveSql = "False";
			bValIsSaveSql = false;
			bValIsMsgSpan = false;
			bValIsOutMsg = false;
			bValIsOutVoice = false;
			bValIsRandRun = false;
			iValMsgNum = 5000;
			iValLevel1="";
			iValLevel2 = 15;
			sValSaveSqlNum = "20";
			sValIsAutoRunServer = "False";
			sValConnectPool = "False";
			sValServerCheckTime = "10";

			sValServerPathDir=Environment.GetEnvironmentVariable("DM_HOME") + "\\data\\DAMENG"; 
			sValGetDmIni="0";
			
			
            sValXMLFilePath=Environment.CurrentDirectory;
			sValOldXMLFilePath=sValXMLFilePath;

			sValDriveName = "DMOLEDB.1";////////////////////////////-------------IMPORTANT
			sValServer0 = "LOCALHOST";
			sValServer1 = "LOCALHOST";
			sValServer2 = "LOCALHOST";
			sValServer3 = "LOCALHOST";
			sValServer4 = "LOCALHOST";
			sValServer5 = "LOCALHOST";
			sValServer6 = "LOCALHOST";
			sValServer7 = "LOCALHOST";
			sValServer8 = "LOCALHOST";
			sValServer9 = "LOCALHOST";
			sValIsWindows = "True";
		}
		public static void InitProVals()
		{
			sValServer = GetProValue(ProServer, sValServer);
			sValDatabase = GetProValue(ProDatabase, sValDatabase);
			sValUserId = GetProValue(ProUserId, sValUserId);
			sValPassword = GetProValue(ProPassword, sValPassword);
			sValMsgNum = GetProValue(ProMsgNum, sValMsgNum);
			sValLevel1 = GetProValue(ProLevel1, sValLevel1);
			sValLevel2 = GetProValue(ProLevel2, sValLevel2);
			sValIsOutMsg = GetProValue(ProIsOutMsg, sValIsOutMsg);
			sValMsgCheckTime = GetProValue(ProMsgCheckTime, sValMsgCheckTime);
			sValIsSaveMsg = GetProValue(ProIsSaveMsg, sValIsSaveMsg);
			sValIsMsgSpan = GetProValue(ProIsMsgSpan, sValIsMsgSpan);
			sValIsOutTime = GetProValue(ProIsOutTime, sValIsOutTime);
			sValThreadNum = GetProValue(ProThreadNum, sValThreadNum);
			sValIsRandRun = GetProValue(ProIsRandRun, sValIsRandRun);
			sValIsAutoClearMsg = GetProValue(ProIsAutoClearMsg, sValIsAutoClearMsg);
			sValIsLoop = GetProValue(ProIsLoop, sValIsLoop);
			sValIsShowResult = GetProValue(ProIsShowResult, sValIsShowResult);
			sValIsOutVoice = GetProValue(ProIsOutVoice, sValIsOutVoice);
			sValIsErrRun = GetProValue(ProIsErrRun, sValIsErrRun);
			sValIsSaveSql = GetProValue(ProIsSaveSql, sValIsSaveSql);			
			sValSaveSqlNum = GetProValue(ProSaveSqlNum, sValSaveSqlNum);
			sValIsAutoRunServer = GetProValue(ProIsAutoRunServer, sValIsAutoRunServer);
			sValConnectPool = GetProValue(ProConnectPool, sValConnectPool);
			sValServerCheckTime = GetProValue(ProServerCheckTime, sValServerCheckTime);
			//sValServerPathDir = GetServerPath();
			sValGetDmIni="0";
            sValXMLFilePath=GetProValue(ProXMLFilePath, sValXMLFilePath);  ////
           // sValOldXMLFilePath=sValXMLFilePath;

			
			sValDriveName = GetProValue(ProDriveName, sValDriveName);
			sValServer0 = GetProValue(ProServer0, sValServer0);
			sValServer1 = GetProValue(ProServer1, sValServer1);
			sValServer2 = GetProValue(ProServer2, sValServer2);
			sValServer3 = GetProValue(ProServer3, sValServer3);
			sValServer4 = GetProValue(ProServer4, sValServer4);
			sValServer5 = GetProValue(ProServer5, sValServer5);
			sValServer6 = GetProValue(ProServer6, sValServer6);
			sValServer7 = GetProValue(ProServer7, sValServer7);
			sValServer8 = GetProValue(ProServer8, sValServer8);
			sValServer9 = GetProValue(ProServer9, sValServer9);
			sValIsWindows = GetProValue(ProIsWindows, sValIsWindows);
			sValIsShowResult = GetProValue(ProIsShowResult, sValIsShowResult);
			bValIsSaveSql = GetIsSaveSql();
			bValIsMsgSpan = GetIsMsgSpan();
			bValIsOutMsg = GetIsShowMsg();
			bValIsOutVoice = GetIsOutVoice();
			bValIsRandRun = GetIsRandRun();
			bValIsOutTime = GetIsOutTime();
			bValIsErrRun = GetIsErrRun();
			iValMsgNum = GetMsgNum();
			iValLevel1 = GetLevel1();
			iValLevel2 = GetLevel2();
			bValIsShowResult = GetIsShowResult();
		}

		public static int GetMsgCheckTime()
		{
			int ret = 0;
			try
			{
				ret = Convert.ToInt32(sValMsgCheckTime);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static int GetThreadNum()
		{
			int ret = 0;
			try
			{
				ret = Convert.ToInt32(sValThreadNum);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}
		
		public static int GetMsgNum()
		{
			int ret = 5000;
			try
			{
				ret = Convert.ToInt32(sValMsgNum);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}
		public static int GetLevel2()
		{
			int ret = 15;
			try
			{
				ret = Convert.ToInt32(sValLevel2);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}
		public static string GetLevel1()
		{
			string ret = "";
			try
			{
				ret =sValLevel1;
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}
		public static bool GetIsSaveMsg()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsSaveMsg);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsShowMsg()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsOutMsg);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public  static bool GetIsOutVoice()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsOutVoice);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsAutoClearMsg()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsAutoClearMsg);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsLoopRun()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsLoop);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsSaveSql()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsSaveSql);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsMsgSpan()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsMsgSpan);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsRandRun()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsRandRun);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsOutTime()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsOutTime);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsErrRun()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsErrRun);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsAutoRunServer()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsAutoRunServer);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsEnableConnectPool()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValConnectPool);
			}
			catch
			{}
			return ret;
		}

		public static bool GetIsShowResult()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsShowResult);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static bool GetIsWindows()
		{
			bool ret = false;
			try
			{
				ret = Convert.ToBoolean(sValIsWindows);
			}
			catch(Exception e)
			{
				String s = e.Message;
			}
			return ret;
		}

		public static String GetServerName(String _server)
		{
			String ret = "localhost";
			int index = -1;
			index = _server.IndexOf(":");
			if (index > 0) {
				ret = _server.Substring(0, index);
			}
			else{
				ret = _server;
			}
			return ret;
		}

		public static String GetServerPort(String _server)
		{
			String ret = "";
			int index = -1;
			index = _server.IndexOf(":");
			if (index > 0) 
			{
				ret = _server.Substring(index+1);
			}
			return ret;
		}

		public static String GetSqlExpVal(String _sql, out string _err)
		{
			String ret = null;
			_err = "";
			String sql = "SELECT " + _sql;
			if (cn == null || cn.State != ConnectionState.Open)
			{
				return ret;
			}
			cm.CommandText = sql;
			try
			{
				ret = Convert.ToString(cm.ExecuteScalar());
			}
			catch(Exception e)
			{
				_err = e.Message;
			}
			return ret;
		}

		public static String GetBoolVal(String _sql, out string _err)
		{
			String ret = null;
            	String sql = "SELECT TOP 1 [NAME] FROM [PRO] WHERE " + _sql;
           // String sql = "SELECT TOP 1 * from all_tables where " + _sql;
            _err = "";
			if (cn == null || cn.State != ConnectionState.Open)
			{
				return ret;
			}
			cm.CommandText = sql;
            OleDbDataReader rd = null;
          //  DmDataReader rd = null;
            try
			{
				rd = cm.ExecuteReader();
			}
			catch(Exception e)
			{
				_err = e.Message;
			}
			if (rd != null) {
				if (rd.HasRows) {
					ret = "";
				}
				rd.Close();
			}
			return ret;
		}
	}
}
