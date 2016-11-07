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
    /// ClassDmServer ��ժҪ˵����
    /// </summary>
    public class ClassDmServer
	{
		private String sExePath;   //������ִ��·��
		private String sIniFile;     //ini�ļ�

		private Process cProcess;
		Thread cThread;
		private MainForm mForm;
		private bool	bServerReady;
	    public bool	bStartServerIsRun;
		bool			bExit;
		

		private bool	bAutoRestartServer;
		private String  conStr;
		private int		checkConnectTime;
		private System.Threading.Timer m_CheckServerTimer;							//�������������Ƿ���������
		private bool	bCheckRun;
		public ClassDmServer(MainForm _mForm)
		{
			bCheckRun = false;
			bStartServerIsRun = false;
			sIniFile = "";
			sExePath = "";
			mForm = _mForm;
			bAutoRestartServer = false;
			checkConnectTime = 15*60*1000;//15���Ӽ��һ��
			TimerCallback timerDelegate2 = new TimerCallback(CheckServerTime);
			m_CheckServerTimer = new System.Threading.Timer(timerDelegate2, this, 1000, checkConnectTime);
			bExit = false;
//			StartCmd();
		}

		public void StartCmd()//�������̣��ڴ˽����������׼��������������Ȼ�󴴽�һ���߳�ί�������в��Ժ���
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
				cThread = new Thread(new ThreadStart(ReadServerOutput));//����һ���߳�ί�������в��Ժ���
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
		public void SetServerPath(String _path)   //���÷���������·��
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
				try//���Է������Ƿ���Ч
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
				mForm.AddToTextBoxFail("δ���÷�����·������·������ȷ", -1);
				return false;
			}
			index = sExePath.LastIndexOf("\\");
			if (index == -1) 
			{
				mForm.AddToTextBoxFail("δ���÷�����·������·������ȷ", -1);
				return false;
			}
			try
			{
				try
				{
					if (bServerReady && cProcess != null && cProcess.HasExited == false) 
					{
						cProcess.StandardInput.WriteLine("exit");
						cProcess.WaitForExit(10*60*1000);//��ʮ���ӣ��÷������˳���
						if (cProcess.HasExited == false) 
						{//�����û�ˣ�ɱ��
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

		public bool StartServer(String _inifile)   //���ض���ini�ļ�������������
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
						cProcess.WaitForExit(10*60*1000);//��ʮ���ӣ��÷������˳���
						if (cProcess.HasExited == false) 
						{//�����û�ˣ�ɱ��
							cProcess.Kill();
						}
					}
				}
				catch(Exception e)
				{
					mForm.AddToTextBoxSuccess(e.Message, -1, false);
				}
			
				if (sExePath == null || sExePath.Length<10)  //    \dmserver.����Ϊ10
				{
					mForm.AddToTextBoxFail("δ���÷�����·������·������ȷ", -1);
					bStartServerIsRun = false;
					return false;
				}
				index = sExePath.LastIndexOf("\\");
				if (index == -1) 
				{
					mForm.AddToTextBoxFail("δ���÷�����·������·������ȷ", -1);
					bStartServerIsRun = false;
					return false;
				}
				string sServerName = sExePath.Substring(index);
				sServerName = sServerName.Substring(1, sServerName.Length - ".exe".Length - 1);
				Process[] myProcesses = Process.GetProcessesByName(sServerName);
				if(myProcesses.Length > 0)    //�������飿
				{
					myProcesses[0].Kill();   //ɱ����һ�����̣�
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
				
//					Thread cThread = new Thread(new ThreadStart(ReadServerOutput));//����һ���߳�ί�������в��Ժ���
//					cThread.Priority = ThreadPriority.BelowNormal;
//					cThread.Start();

//					while (!bServerReady && cProcess.HasExited == false) 
//					{
//						Thread.Sleep(300);
//					}
					WaitServerStart();
					if (cProcess.HasExited == false) 
					{
						mForm.AddToTextBoxSuccess("������������", -1, false);
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

		//�����µ��߳�ִ�к������ú���������������Ӧ����������е�����������
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
				mForm.AddToTextBoxSuccess("�������� " + sOutString, -1, false);
			}
			cProcess = null;
			bServerReady = false;
		}

		public void SendCommand(String _cmd)
		{
			mForm.AddToTextBoxSuccess("�������� " + _cmd, -1, false);
			if (string.Compare(_cmd.Trim(), "exit", true) == 0 && cProcess == null) {
				int index = sExePath.LastIndexOf("\\");
				if (index == -1) 
				{
					mForm.AddToTextBoxFail("δ���÷�����·������·������ȷ", -1);
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
				Monitor.Enter(cProcess);   //��ָ�������ϻ�ȡ������
				cProcess.StandardInput.WriteLine(_cmd);
				Monitor.Exit(cProcess);   //��ָ���������ͷ�������
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
			try//���Է������Ƿ���Ч
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
					mForm.AddToTextBoxFail("���ӷ�����ʧ�ܣ��Զ����Ա�ֹͣ!\n" + e.Message, -1);
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
			try//���Է������Ƿ���Ч
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
