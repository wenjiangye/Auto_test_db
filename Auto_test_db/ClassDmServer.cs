using System;
using System.Diagnostics;
using System.Threading;
#if DM7
using Dm;
#endif
using System.Data.OleDb;
namespace Auto_test_db
{
    /// <summary>
    /// ClassDmServer 的摘要说明。
    /// </summary>
    public class ClassDmServer
	{
		private String sExePath;   //服务器执行路径
		private String sIniFile;     //ini文件

		private Process cProcess;
		Thread cThread;
		private MainForm mForm;
		private bool	bServerReady;
	    public bool	bStartServerIsRun;
		bool			bExit;
		

		private bool	bAutoRestartServer;
		private String  conStr;
		private int		checkConnectTime;
		private System.Threading.Timer m_CheckServerTimer;							//用来检察服务器是否正常运行
		private bool	bCheckRun;
		public ClassDmServer(MainForm _mForm)
		{
			bCheckRun = false;
			bStartServerIsRun = false;
			sIniFile = "";
			sExePath = "";
			mForm = _mForm;
			bAutoRestartServer = false;
			checkConnectTime = 15*60*1000;//15分钟检查一次
			TimerCallback timerDelegate2 = new TimerCallback(CheckServerTime);
			m_CheckServerTimer = new System.Threading.Timer(timerDelegate2, this, 1000, checkConnectTime);
			bExit = false;
//			StartCmd();
		}

		public void StartCmd()//启动进程，在此进程中允许标准的输入和输出流，然后创建一个线程委托来运行测试函数
		{
			cProcess.StartInfo.WorkingDirectory = sExePath.Substring(0,sExePath.LastIndexOf("\\"));

			cProcess.StartInfo.FileName = "cmd.exe";//			cProcess.StartInfo.FileName = sExePath;
			cProcess.StartInfo.RedirectStandardInput = true;
			cProcess.StartInfo.RedirectStandardOutput = true;
			cProcess.StartInfo.RedirectStandardError = true;
			cProcess.StartInfo.UseShellExecute = false;
			cProcess.StartInfo.CreateNoWindow = true;
			try
			{
				cProcess.Start();
				cThread = new Thread(new ThreadStart(ReadServerOutput));//创建一个线程委托来运行测试函数
				cThread.Priority = ThreadPriority.BelowNormal;
				cThread.Start();
			}
			catch(Exception mes)
			{
				mForm.AddToTextBoxFail(mes.Message, -1);						
			}
		}

		public void ExitCmd()
		{
			try
			{
				if (cProcess != null && cProcess.HasExited == false) 
				{
					bExit = true;
					cProcess.StandardInput.WriteLine("exit");
					cThread.Abort();
					cThread.Join();
				}
			}
			catch(Exception e)
			{
				String m_msg = e.Message;
			}
	//		cProcess.StandardInput.WriteLine("exit");
	//		cProcess.StandardInput.WriteLine("exit");
		}
		public void SetServerPath(String _path)   //设置服务器所在路径
		{
			sExePath = _path;
		}
		public void SetAutoRestartServer(bool _auto)
		{
			bAutoRestartServer = _auto;
		}
		public void SetCheckServerTime(int _time)
		{
			checkConnectTime = _time*60*1000;
			m_CheckServerTimer.Change(Timeout.Infinite, checkConnectTime);
		}
		public void SetConnectStr(string _str)
		{
			conStr = _str;
		}

        public void WaitServerStart()
		{
		    int i=100;
			while (i-- > 0) {
				Thread.Sleep(3000);
				try//测试服务器是否还有效
				{

#if DM7
					DmConnection m_cn = new DmConnection(conStr);
#else
                    OleDbConnection m_cn = new OleDbConnection(conStr);
#endif
					m_cn.Open();
					m_cn.Close();
					return;
				}
				catch(Exception e)
				{
					string m_string = e.Message;
				}
			}
		}

		public bool InitDb(string _arg)
		{
			int index = -1;
			if (sExePath == null || sExePath.Length<10) 
			{
				mForm.AddToTextBoxFail("未设置服务器路径或是路径不正确", -1);
				return false;
			}
			index = sExePath.LastIndexOf("\\");
			if (index == -1) 
			{
				mForm.AddToTextBoxFail("未设置服务器路径或是路径不正确", -1);
				return false;
			}
			try
			{
				try
				{
					if (bServerReady && cProcess != null && cProcess.HasExited == false) 
					{
						cProcess.StandardInput.WriteLine("exit");
						cProcess.WaitForExit(10*60*1000);//等十分钟，让服务器退出来
						if (cProcess.HasExited == false) 
						{//如果还没退？杀了
							cProcess.Kill();
						}
					}
				}
				catch(Exception e)
				{
					mForm.AddToTextBoxSuccess(e.Message, -1, false);
				}			
				

				string sServerName = sExePath.Substring(index);
				sServerName = sServerName.Substring(1, sServerName.Length - ".exe".Length - 1);
				Process[] myProcesses = Process.GetProcessesByName(sServerName);
				if(myProcesses.Length > 0)
				{
					myProcesses[0].Kill();
					Thread.Sleep(300);
				}

				string sInitPath = sExePath.Substring(0,sExePath.LastIndexOf("\\"));
				cProcess = new Process();
				cProcess.StartInfo.WorkingDirectory = sInitPath;
				cProcess.StartInfo.FileName = sInitPath + "\\initdb.exe";
				cProcess.StartInfo.UseShellExecute = false;
				cProcess.StartInfo.CreateNoWindow = false;
				if (_arg.IndexOf("-p") != -1) {
					cProcess.StartInfo.Arguments = _arg;
				}
				else{
					cProcess.StartInfo.Arguments = "-p \"" + sInitPath + "\" " + _arg;
				}
				
				cProcess.StartInfo.Arguments = cProcess.StartInfo.Arguments.Trim();
				cProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;					
				try
				{
					cProcess.Start();
					cProcess.WaitForExit();
					if (cProcess.ExitCode == 0) {
						return StartServer("");
					}
					return false;
				}
				catch(Exception mes)
				{
					mForm.AddToTextBoxFail(mes.Message, -1);						
				}
			}
			catch(Exception mes)
			{
				mForm.AddToTextBoxFail(mes.Message, -1);						
			}
			return false;
		}

		public bool StartServer(String _inifile)   //用特定的ini文件，启动服务器
		{
			int index = -1;
//			if (bStartServerIsRun) {
//				return false;
//			}
			bStartServerIsRun = true;
			if (_inifile != null) {
				sIniFile = _inifile;
			}
			try
			{
				try
				{
					if (bServerReady && cProcess != null && cProcess.HasExited == false) 
					{
						cProcess.StandardInput.WriteLine("exit");
						cProcess.WaitForExit(10*60*1000);//等十分钟，让服务器退出来
						if (cProcess.HasExited == false) 
						{//如果还没退？杀了
							cProcess.Kill();
						}
					}
				}
				catch(Exception e)
				{
					mForm.AddToTextBoxSuccess(e.Message, -1, false);
				}
			
				if (sExePath == null || sExePath.Length<10)  //    \dmserver.长度为10
				{
					mForm.AddToTextBoxFail("未设置服务器路径或是路径不正确", -1);
					bStartServerIsRun = false;
					return false;
				}
				index = sExePath.LastIndexOf("\\");
				if (index == -1) 
				{
					mForm.AddToTextBoxFail("未设置服务器路径或是路径不正确", -1);
					bStartServerIsRun = false;
					return false;
				}
				string sServerName = sExePath.Substring(index);
				sServerName = sServerName.Substring(1, sServerName.Length - ".exe".Length - 1);
				Process[] myProcesses = Process.GetProcessesByName(sServerName);
				if(myProcesses.Length > 0)    //进程树组？
				{
					myProcesses[0].Kill();   //杀掉第一个进程？
					Thread.Sleep(300);
				}
				bServerReady = false;
				cProcess = new Process();
				cProcess.StartInfo.WorkingDirectory = sExePath.Substring(0,sExePath.LastIndexOf("\\"));
				cProcess.StartInfo.FileName = sExePath;
				cProcess.StartInfo.RedirectStandardInput = true;
				cProcess.StartInfo.RedirectStandardOutput = false;
				cProcess.StartInfo.RedirectStandardError = false;
				cProcess.StartInfo.UseShellExecute = false;
				cProcess.StartInfo.CreateNoWindow = false;
				if (sIniFile != null && sIniFile.Trim().Length > 0) {
					cProcess.StartInfo.Arguments = "\"" + sIniFile + "\"";
				}				
				cProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;					
				try
				{
					cProcess.Start();
//					cProcess.StandardInput.WriteLine(sExePath.Substring(0, 2));
//					cProcess.StandardInput.WriteLine("cd \"" + cProcess.StartInfo.WorkingDirectory + "\"");
//					if (sIniFile != null && sIniFile.Length>0) 
//					{
//						cProcess.StandardInput.WriteLine("\"" + sExePath + "\" \"" + sIniFile + "\"");
//					}
//					else
//					{
//						cProcess.StandardInput.WriteLine("\"" + sExePath + "\"");
//					}
				
//					Thread cThread = new Thread(new ThreadStart(ReadServerOutput));//创建一个线程委托来运行测试函数
//					cThread.Priority = ThreadPriority.BelowNormal;
//					cThread.Start();

//					while (!bServerReady && cProcess.HasExited == false) 
//					{
//						Thread.Sleep(300);
//					}
					WaitServerStart();
					if (cProcess.HasExited == false) 
					{
						mForm.AddToTextBoxSuccess("服务器启动了", -1, false);
					}
					else
					{
						bServerReady = false;
						bStartServerIsRun = false;
						return false;
					}
					bStartServerIsRun = false;
					bServerReady = true;
					return true;
				}
				catch(Exception mes)
				{
					mForm.AddToTextBoxFail(mes.Message, -1);						
				}
			}
			catch(Exception mes)
			{
				 mForm.AddToTextBoxFail(mes.Message, -1);						
			}
			bStartServerIsRun = false;
			return false;
		}

		//创建新的线程执行函数，该函数是用来读出对应的事务进程中的输入流内容
		public void ReadServerOutput()
		{
			string sOutString = "";
			while(cProcess.HasExited == false)
			{	
				sOutString = cProcess.StandardOutput.ReadLine();
				if (bExit) {
					return;
				}
				if ( sOutString == null) {
					break;
				}
				if(sOutString.StartsWith("SYSTEM IS READY."))
				{
					bServerReady = true;
				}
				mForm.AddToTextBoxSuccess("服务器： " + sOutString, -1, false);
			}
			cProcess = null;
			bServerReady = false;
		}

		public void SendCommand(String _cmd)
		{
			mForm.AddToTextBoxSuccess("服务器： " + _cmd, -1, false);
			if (string.Compare(_cmd.Trim(), "exit", true) == 0 && cProcess == null) {
				int index = sExePath.LastIndexOf("\\");
				if (index == -1) 
				{
					mForm.AddToTextBoxFail("未设置服务器路径或是路径不正确", -1);
					return;
				}
				string sServerName = sExePath.Substring(index);
				sServerName = sServerName.Substring(1, sServerName.Length - ".exe".Length - 1);
				Process[] myProcesses = Process.GetProcessesByName(sServerName);
				if(myProcesses.Length > 0)
				{
					myProcesses[0].Kill();
				}
				return;
			}
			if (cProcess == null || cProcess.HasExited) {
				if(!StartServer(null))
				{
					return;
				}
			}
			try
			{
				Monitor.Enter(cProcess);   //在指定对象上获取排他锁
				cProcess.StandardInput.WriteLine(_cmd);
				Monitor.Exit(cProcess);   //在指定对象上释放排他锁
			}
			catch(Exception e)
			{
				mForm.AddToTextBoxFail(e.Message, -1);
			}
		}

		public void CheckServerTime(Object state)
		{
			if(mForm.m_stopTest || bCheckRun)
				return;
			bCheckRun = true;
			try//测试服务器是否还有效
			{
#if DM7
					DmConnection m_cn = new DmConnection(conStr);
#else
                    OleDbConnection m_cn = new OleDbConnection(conStr);
#endif
				try
				{
					m_cn.Open();
					m_cn.Close();
					bCheckRun = false;
					return;
				}
				catch(Exception e)
				{
					string str = e.Message;
				}
				try
				{
					m_cn.Open();
					m_cn.Close();
					bCheckRun = false;
					return;
				}
				catch(Exception e)
				{
					string str = e.Message;
				}
				m_cn.Open();
				m_cn.Close();
			}
			catch(Exception e)
			{
				if(bAutoRestartServer)
				{
					StartServer(null);
				}
				else
				{
					mForm.AddToTextBoxFail("连接服务器失败，自动测试被停止!\n" + e.Message, -1);
					mForm.m_stopTest = true;
				}
			}
			bCheckRun = false;
		}
		//public string ImportTestResult(DmDataReader dr)
        public string ImportTestResult(OleDbDataReader dr)
		{
			//OleDbConnection m_cn = null;
            DmConnection m_cn = null;
			try//测试服务器是否还有效
			{
				m_cn = new DmConnection(conStr);
				try
				{
					m_cn.Open();
				}
				catch(Exception e)
				{
					return e.Message;
				}
                //OleDbCommand m_cm = m_cn.CreateCommand();
                DmCommand m_cm = m_cn.CreateCommand();
				try
				{
					m_cm.CommandText = "DROP TABLE TEST_RESULT";
					m_cm.ExecuteNonQuery();
				}
				catch
				{
				}
				m_cm.CommandText = "CREATE TABLE TEST_RESULT(ID INT IDENTITY(1,1) PRIMARY KEY, [NAME] VARCHAR(200), [PATH] VARCHAR(300), TEST_TIME DATETIME, IS_SUCCESS CHAR(1))";
				m_cm.ExecuteNonQuery();
				m_cm.CommandText = "CREATE INDEX I_TEST_RESULT ON TEST_RESULT([NAME], [PATH])";
				m_cm.ExecuteNonQuery();
				while(dr.Read())
				{
					string sql = "INSERT INTO TEST_RESULT(NAME, PATH, TEST_TIME, IS_SUCCESS) VALUES('";
					sql += dr[0].ToString() + "','" + dr[1].ToString() + "','" + Convert.ToString(dr[2]) + "','" + dr[3].ToString() + "')";
					m_cm.CommandText = sql;
					m_cm.ExecuteNonQuery();
				}
			}
			catch(Exception e)
			{
				m_cn.Close();
				return e.Message;
			}
			m_cn.Close();
			return null;
		}
		public string GetIniPath()
		{
			return sIniFile;
		}
	}
}
