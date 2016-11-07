using System;
using System.Xml;
using System.Data;
using System.Text;
#if DM7
using Dm;
#else
using System.Data.OleDb;
#endif
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace Auto_test_db
{
	/// <summary>
	/// XmlTest 的摘要说明。
	/// </summary>
	public class TRANS_STRUCT
	{
		public Process cProcess;		//指向一个处理的脚本的进程
		public Thread cReadThread;
		public bool bIsFinishExcute;			//表示上次发出的脚本执行完成没有
		public TRANS_STRUCT()
		{
			bIsFinishExcute = false;
		}
	}
	
	public class LOOP_STRUCT
	{
		public object prev;				//指向父LOOP
		public int times;				//执行的次数
		public int start;				//执行的开始
	}

	class CONNECTINFO
	{
#if DM7
        public DmConnection cn;		//当前的连接
        public DmCommand cm;			//当前的命令行
        public DmDataReader rd;		//当前的命令行生成的结果集
        public DmTransaction tr;		//当前连接的事务
#else
		public OleDbConnection cn;		//当前的连接
		public OleDbCommand cm;			//当前的命令行
		public OleDbDataReader rd;		//当前的命令行生成的结果集
		public OleDbTransaction tr;		//当前连接的事务
#endif
        public ArrayList alTrArray;			//用来存放建立的事务链对像数组。
		public string sProvider;			//用来存OLEDB驱动的名称	
		public string sServerName;		//服务器IP或名称
		public string sUid;				//用户名
		public string sPwd;				//用户口令
		public string sDatabase;			//初始化数据库名
		public string sPort;			//端口
		public bool   bPoolEnable;		//是否启用连接池功能
		public bool isOpenResult;		//是否已经使用OPEN打开结果集游标
		public int iFetch;				//游标所在的当前行数
	}

	struct XMLRUNINFO
	{		
		public bool bStop;					//是否终止当前测试用例的运行
		public bool bClearEn;				//表示该对像是否是用来清空测试环境的

		public string sXmlFileName;			//相应的XML文件
		public TestThread tdTestThread;	//该对像所在的线程引用

		public XmlTextReader xXmlTextRr;				// = new XmlTextReader(e.Node.Text);
		public XmlDocument xDoc;					// = new XmlDocument();
		public XmlNode xFirstNode;				//对像开始执行的第一个结点
		public XmlNode xCurrentNode;				//对像执行的当前结点

		public SqlCase cCurrentSqlCase;
	}
	public struct CASERESULT
	{
		public bool bBreak;			//用来控制程序是否跳出这个SQL_CASE的执行;
		public bool bSuccess;		//用来表示该SQL_CASE的执行结果，成功还是失败
		public bool bFailStop;		//用来表示该SQL_CASE执行时，是否是碰到失败就跳出这个SQL_CASE的执行
		public bool bExpResult;		//用来表示这个SQl_CASE模块预期的执行结果
	}

	public struct SQLRESULT
	{
		public string sExpResult;	//SQL执行完以后预期的结果
		public string sSQLState;			//预期返回的状态码
		public int iNativeError;			//预期返回的系统错误代码
	}
	public class XmlTest
	{
		CONNECTINFO stConnectINfo;
		ArrayList stConnectArry;
		XMLRUNINFO stXmlRunInfo;
		SqlCase cSqlCase;
		ArrayList alTransPro;		//用来存贮事务进程的链表

		string sProvider;			//用来存OLEDB驱动的名称	
		string sServerName;		//服务器IP或名称
		string sUid;				//用户名
		string sPwd;				//用户口令
		string sDatabase;			//初始化数据库名
		string sPort;			//端口
		bool   bPoolEnable;		//是否启用连接池功能

		LOOP_STRUCT cLoop;
		Int64		usedtimes;
		int		run_times;
		bool	isHasTimes;
		bool	m_noShow;//是否显示执行的语句
		public string testnum="测试点0：";//用于保存测试点的辅助信息
		//public int testnum=0;

		ValClass vlc;
		//-----test
        private Counter sqlCaseCounter = new Counter();    //sqlCase计数
		private Counter sqlCounter = new Counter();
		private Counter newConnCounter = new Counter();
		private Counter resultCounter = new Counter();
		private int CurrentLineNum=0;  //当前行号

		public string xmlFileName;
		
        //-----test
		
		public XmlTest(TestThread m_th)
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
			cLoop = null;
			stConnectArry = new ArrayList();
			stConnectINfo = null;			
			alTransPro = new ArrayList();
			stXmlRunInfo = new XMLRUNINFO();
			stXmlRunInfo.sXmlFileName = "";
			stXmlRunInfo.tdTestThread = m_th;
			
			cSqlCase = new SqlCase();
			stXmlRunInfo.cCurrentSqlCase = cSqlCase;

			run_times = 1;
			isHasTimes = false;
			
			m_noShow = true;////////////////////////////////
			InitRunInfo();
			SetConnectInfo();			
		}		
		public TRANS_STRUCT tCurrentTransStruct; //指向当前创建事务进程对应的结构，该结果用来创建对应的读取事务进程输入流的线程
		//返回当前XML文件执行的节点
		public XmlNode GetCurrentXmlNode()
		{
			return stXmlRunInfo.xCurrentNode;
		}
		//返回是否执行成功
		public bool GetSuccessInfo()
		{
			return cSqlCase.stCaseResult.bSuccess;
		}
		public void InitRunInfo()
		{
			vlc = new ValClass();
			vlc.AddPrimaryKey("SQLSTR", "");
			vlc.AddPrimaryKey("SQLSTR1", "");
			vlc.AddPrimaryKey("SQLSTR2", "");
			vlc.AddPrimaryKey("TEMPSTR", "");
			vlc.AddPrimaryKey("POOL", "FALSE");
	//		vlc.AddPrimaryKey("TIMES", "");
			vlc.AddPrimaryKey("SERVER", ProClass.GetServerName(ProClass.sValServer));
			vlc.AddPrimaryKey("PORT", ProClass.GetServerPort(ProClass.sValServer));
			vlc.AddPrimaryKey("SERVER0", ProClass.GetServerName(ProClass.sValServer0));
			vlc.AddPrimaryKey("PORT0", ProClass.GetServerPort(ProClass.sValServer0));
			vlc.AddPrimaryKey("SERVER1", ProClass.GetServerName(ProClass.sValServer1));
			vlc.AddPrimaryKey("PORT1", ProClass.GetServerPort(ProClass.sValServer1));
			vlc.AddPrimaryKey("SERVER2", ProClass.GetServerName(ProClass.sValServer2));
			vlc.AddPrimaryKey("PORT2", ProClass.GetServerPort(ProClass.sValServer2));
			vlc.AddPrimaryKey("SERVER3", ProClass.GetServerName(ProClass.sValServer3));
			vlc.AddPrimaryKey("PORT3", ProClass.GetServerPort(ProClass.sValServer3));
			vlc.AddPrimaryKey("SERVER4", ProClass.GetServerName(ProClass.sValServer4));
			vlc.AddPrimaryKey("PORT4", ProClass.GetServerPort(ProClass.sValServer4));
			vlc.AddPrimaryKey("SERVER5", ProClass.GetServerName(ProClass.sValServer5));
			vlc.AddPrimaryKey("PORT5", ProClass.GetServerPort(ProClass.sValServer5));
			vlc.AddPrimaryKey("SERVER6", ProClass.GetServerName(ProClass.sValServer6));
			vlc.AddPrimaryKey("PORT6", ProClass.GetServerPort(ProClass.sValServer6));
			vlc.AddPrimaryKey("SERVER7", ProClass.GetServerName(ProClass.sValServer7));
			vlc.AddPrimaryKey("PORT7", ProClass.GetServerPort(ProClass.sValServer7));
			vlc.AddPrimaryKey("SERVER8", ProClass.GetServerName(ProClass.sValServer8));
			vlc.AddPrimaryKey("PORT8", ProClass.GetServerPort(ProClass.sValServer8));
			vlc.AddPrimaryKey("SERVER9", ProClass.GetServerName(ProClass.sValServer9));
			vlc.AddPrimaryKey("PORT9", ProClass.GetServerPort(ProClass.sValServer9));
			vlc.AddPrimaryKey("DATABASE", ProClass.sValDatabase);
			vlc.AddPrimaryKey("SCHEMA", ProClass.sValUserId);
			vlc.AddPrimaryKey("UID", ProClass.sValUserId);
			vlc.AddPrimaryKey("PWD", ProClass.sValPassword);
			vlc.AddPrimaryKey("USEDTIMES", "");
			vlc.AddPrimaryKey("SERVERPATH", ProClass.sValServerPathDir);   //这里
			vlc.AddPrimaryKey("SQLSTR", ProClass.sValGetDmIni);
			
			vlc.AddPrimaryKey("PROCESSID", "");
			vlc.AddPrimaryKey("RECORDNUMS", "");
			vlc.AddPrimaryKey("COLUMNNUMS", "");
			vlc.AddPrimaryKey("PROVIDER", ProClass.sValDriveName);

	
		}
			/// <summary>
		/// //用来设置该对像连接数据库的信息
		/// </summary>
		public void SetConnectInfo()
		{
			sProvider = ProClass.sValDriveName;//////////////////////////////////////////////
			sServerName = ProClass.GetServerName(ProClass.sValServer);
			sUid = ProClass.sValUserId;
			sPwd = ProClass.sValPassword;
			sDatabase = ProClass.sValDatabase;
			sPort = ProClass.GetServerPort(ProClass.sValServer);
			bPoolEnable = ProClass.GetIsEnableConnectPool();
		}

		/// <summary>
		/// //设置当前执行操作用的连接
		/// </summary>
		public bool SetCn(int iIndex, bool bShowInfo)
		{
			Debug.Assert(iIndex >= 0, "设置连接的ID小于0", "XmlTest.SetCn 函数");
			if(iIndex >= stConnectArry.Count || iIndex < 0)//如果XML文件中要设置的当前连接ID大于最大的ID，或是小于0
			{
				ShowFMessage("XML文件中要设置的当前连接ID大于最大的ID" + (stConnectArry.Count - 1) + "，或是小于0");
				return false;
			}			
			stConnectINfo = (CONNECTINFO)stConnectArry[iIndex];
			sProvider = stConnectINfo.sProvider;
			sServerName = stConnectINfo.sServerName;
			sUid = stConnectINfo.sUid;
			sPwd = stConnectINfo.sPwd;
			sDatabase = stConnectINfo.sDatabase;
			sPort = stConnectINfo.sPort;

			if(stConnectINfo.alTrArray.Count > 0)
#if DM7
                stConnectINfo.tr = (DmTransaction)stConnectINfo.alTrArray[stConnectINfo.alTrArray.Count - 1];
#else
				stConnectINfo.tr = (OleDbTransaction)stConnectINfo.alTrArray[stConnectINfo.alTrArray.Count - 1];
#endif
            else
				stConnectINfo.tr = null;
			vlc.AddPrimaryKey("CUR_SERVER", stConnectINfo.sServerName);
			vlc.AddPrimaryKey("CUR_DATABASE", stConnectINfo.sDatabase);
			vlc.AddPrimaryKey("CUR_SCHEMA", stConnectINfo.sUid);
			vlc.AddPrimaryKey("CUR_UID", stConnectINfo.sUid);
			vlc.AddPrimaryKey("CUR_PWD", stConnectINfo.sPwd);
			if(bShowInfo)
#if DM7
                ShowSMessage("当前连接索引更改，现在的索引是 " + iIndex + " ;" + "ID：" + stConnectINfo.sUid + "; 口令：" + stConnectINfo.sPwd + "; 初始库：" + stConnectINfo.sDatabase + "; 服务器：" + stConnectINfo.sServerName);
#else
                ShowSMessage("当前连接索引更改，现在的索引是 " + iIndex + " ;" + "ID：" + stConnectINfo.sUid + "; 口令：" + stConnectINfo.sPwd + "; 初始库：" + stConnectINfo.sDatabase + "; 服务器：" + stConnectINfo.sServerName + "; 驱动：" + stConnectINfo.sProvider);
#endif
            return true;
		}
		/// <summary>
		/// //连接数据库
		/// </summary>
		public bool ConnectionEx()
		{
			string	bTempCnExp;
			bTempCnExp= stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult;
			if(bTempCnExp == "LOGIN_SUCCESS" || bTempCnExp == "LOGIN_FAIL")
			{
                stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult = "DIRECT_EXECUTE_SUCCESS";
#if DM7
                DmConnection cTempCn;
				cTempCn = new DmConnection(CreateConnectStr());
#else
                OleDbConnection cTempCn;
				cTempCn = new OleDbConnection(CreateConnectStr());
#endif
                try
                {
                    ShowSMessage(cTempCn.ConnectionString);
                    cTempCn.Open();
                    cTempCn.Close();
                    if (bTempCnExp == "LOGIN_FAIL")
                    {
                        ShowFMessage("预期连接失败，实际上成功了");
                    }
                }
#if DM7
                catch (DmException e)
                {
                    string errorMessages = "";

                    errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
				catch (OleDbException e)
                {
					string errorMessages = "";
					for (int i=0; i < e.Errors.Count; i++)
					{
						errorMessages += "Index #" + i + "\n" +
							"Message: " + e.Errors[i].Message + "\n" +
							"NativeError: " + e.Errors[i].NativeError + "\n" +
							"Source: " + e.Errors[i].Source + "\n" +
							"SQLState: " + e.Errors[i].SQLState + "\n";
					}
#endif

                    if (bTempCnExp == "LOGIN_FAIL")
                    {
                        ShowSMessage(errorMessages);
                    }
                    else
                    {
                        ShowFMessage(errorMessages);
                    }
                }
                catch (Exception e)
                {
                    ShowFMessage(e.Message);
                }
				return false;
			}
			else
			{
				return Connection();
			}
		}
		void AddConnectInfo()
		{
			stConnectINfo.sProvider = sProvider;
			stConnectINfo.sUid = sUid;
			stConnectINfo.sPwd = sPwd;
			stConnectINfo.sServerName = sServerName;
			stConnectINfo.sDatabase = sDatabase;
			stConnectINfo.bPoolEnable = bPoolEnable;
			stConnectINfo.sPort = sPort;
			stConnectINfo.iFetch = 0;
			stConnectINfo.isOpenResult = false;
			stConnectINfo.cm = stConnectINfo.cn.CreateCommand();
			stConnectINfo.rd = null;
			stConnectINfo.tr = null;
			stConnectINfo.alTrArray = new ArrayList();
			stConnectArry.Add(stConnectINfo);
		}
		/// <summary>
		/// //连接数据库
		/// </summary>
		public bool Connection()
		{
			//	String sCon = "Provider=OraOLEDB.Oracle.1;Data Source=SF;Password=hust4400;Persist Security Info=True;User ID=SYSTEM;Port = 1521";
			CONNECTINFO conTemp = stConnectINfo;
			stConnectINfo = new CONNECTINFO();
#if DM7
            stConnectINfo.cn = new DmConnection(CreateConnectStr());//
#else
            stConnectINfo.cn = new OleDbConnection(CreateConnectStr());//
#endif
            try 
			{				
			//	ShowSMessage("现在连接的索引是 " + stConnectArry.Count + " ;" + "ID：" + sUid + "; 口令：" + sPwd + "; 初始库：" + sDatabase + "; 服务器：" + sServerName + "; 驱动：" + sProvider);
				ShowSMessage(stConnectINfo.cn.ConnectionString);
				stConnectINfo.cn.Open();	
			}
#if DM7
            catch (DmException e)
#else
			catch (OleDbException e)
#endif
            {
				stConnectINfo = conTemp;
				if(stXmlRunInfo.cCurrentSqlCase.stCaseResult.bExpResult)
					stXmlRunInfo.bStop = true;
				string errorMessages = "";
#if DM7
                errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
                for (int i=0; i < e.Errors.Count; i++)
				{
					errorMessages += "Index #" + i + "\n" +
						"Message: " + e.Errors[i].Message + "\n" +
						"NativeError: " + e.Errors[i].NativeError + "\n" +
						"Source: " + e.Errors[i].Source + "\n" +
						"SQLState: " + e.Errors[i].SQLState + "\n";
				}
#endif

                ShowFMessage(errorMessages);
				return false;
			}
            catch (Exception e)
            {
                ShowFMessage(e.Message);
                return false;
            }
			AddConnectInfo();
			
			vlc.AddPrimaryKey("CUR_SERVER", stConnectINfo.sServerName);
			vlc.AddPrimaryKey("CUR_DATABASE", stConnectINfo.sDatabase);
			vlc.AddPrimaryKey("CUR_SCHEMA", stConnectINfo.sUid);
			vlc.AddPrimaryKey("CUR_UID", stConnectINfo.sUid);
			vlc.AddPrimaryKey("CUR_PWD", stConnectINfo.sPwd);
			return true;
		}

		/// <summary>
		/// //用来设置该对像要执行的XML文件名
		/// </summary>
		public void SetXmlFile(string m_file)
		{
			Debug.Assert(m_file != "", "指定XML时， 文件字符串为空", "XmlTest.SetXmlFile 函数");
			stXmlRunInfo.sXmlFileName = m_file;			
			vlc.AddPrimaryKey("PATH", stXmlRunInfo.sXmlFileName.Substring(0, stXmlRunInfo.sXmlFileName.LastIndexOf("\\")));
		}

		/// <summary>
		/// //用来运行该测试用例生成的记录语句
		/// </summary>
		public bool RunLog()
		{
			ShowSMessage("开始执行");
			StreamReader sr = null;
			try
			{
				sr = new StreamReader(stXmlRunInfo.sXmlFileName, System.Text.Encoding.Default);
				if(sr == null)
					return false;
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message);
				return false;				
			}
			
			string m_str = sr.ReadLine();
			string m_str1 = "";
			while(m_str != null && stXmlRunInfo.bStop != true)
			{
				if(m_str[m_str.Length - 1] == ';')
					m_str1 = m_str.Substring(0, m_str.Length - 1);
                Console.WriteLine(m_str1);
				ExecuteSQL(m_str1);
				m_str = sr.ReadLine();
			}
			sr.Close();
			ShowSMessage("结束");
			return cSqlCase.stCaseResult.bSuccess;
		}
		/// <summary>
		/// //用来运行一段事务
		/// </summary>
		/*
		public bool Run(string sXml)
		{
			if(sXml == null || sXml == "")
				return false;
			XmlDocument cDoc;					// = new XmlDocument();
			cDoc = new XmlDocument();
			try
			{
				//ShowSMessage("收到脚本：" + sXml);
				cDoc.LoadXml(sXml);
				if(cDoc.FirstChild != null)
				{
					while(run_times > 0)
					{
						SearchXmlNode(cDoc.FirstChild.FirstChild, true);
						run_times--;
					}
				}
				else
				{
					ShowSMessage("输入了空脚本？");
					return false;
				}
				if(cSqlCase.stCaseResult.bSuccess != cSqlCase.stCaseResult.bExpResult)
				{
					cSqlCase.stCaseResult.bSuccess = false;
				}
				else
				{
					cSqlCase.stCaseResult.bSuccess = true;
				}

			}
			catch(XmlException e)
			{
				ShowFMessage(e.Message);
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message);
			}
			return cSqlCase.stCaseResult.bSuccess;
		}*/
		private string GetXmlSingleFileName(string s)//用来得到xml文件的单个文件名
		{
			int m_index = -1;
			m_index = s.LastIndexOf("\\");
			if(m_index == -1)
			{
				return s;
			}
			else
			{
				return s.Substring(m_index+1);
			}
		}
		/// <summary>
		/// //用来运行该测试用例
		/// </summary>
		public bool Run(bool bTransfer)
		{
			
			if(bTransfer == false)
			ShowSMessage("开始执行");
			if(stXmlRunInfo.xFirstNode == null)				//如果该测试文件对像，不是以给定首结点的方式执行的，那么就读出文件，找出首结点
			{
				Debug.Assert(stXmlRunInfo.sXmlFileName != "", "XML文件字符串为空", "XmlTest.Run 函数");
				
				stXmlRunInfo.xXmlTextRr = new XmlTextReader(stXmlRunInfo.sXmlFileName);
								
				Debug.Assert(stXmlRunInfo.xXmlTextRr!=null, "未找到指定的", "XmlTest.Run 函数");
				if(stXmlRunInfo.xXmlTextRr == null)//未找到指定的XML文件
				{				
					ShowFMessage("未找到指定的XML文件");
					return false;
				}				
				stXmlRunInfo.xDoc = new XmlDocument();				
				try
				{
					stXmlRunInfo.xDoc.Load(stXmlRunInfo.xXmlTextRr);
					XmlNode m_XmlNode= FindXmlNode(stXmlRunInfo.xDoc.FirstChild, "SQLTEST");
					if(m_XmlNode == null)
					{
						ShowFMessage("未在XML文件中找到关键字(SQLTEST)");
						return false;
					}
					if(ProClass.bValIsLevel ==true)//判断测试用例中设定的测试等级是否符合要测试的测试等级，若符合则执行此测试用例，否则不执行
					{
						if (!CheckLevel2(m_XmlNode.FirstChild)) 
						{
							return true;
						}
					}
					else if(ProClass.bValIsLevel ==false)//判断测试用例中设定的测试等级是否符合要测试的测试等级，若符合则执行此测试用例，否则不执行
					{
						if (!CheckLevel1(m_XmlNode.FirstChild)) 
						{
							return true;
						}
					}
					while(run_times > 0)
					{
						SearchXmlNode(m_XmlNode, true);		//在这函数里面，在找出各个结点的同时，对结点进行分析，并根据分析的结果，执行结点要求的操作
						if(cSqlCase.stCaseResult.bSuccess != cSqlCase.stCaseResult.bExpResult)
						{
							stXmlRunInfo.tdTestThread .ShowFMessage("脚本中预期的执行结果和实际的执行结果不一致！预期：" + cSqlCase.stCaseResult.bExpResult + "; 实际：" + cSqlCase.stCaseResult.bSuccess, true);							
							cSqlCase.stCaseResult.bSuccess = false;
						}
						else
						{
							cSqlCase.stCaseResult.bSuccess = true;
						}
						run_times --;
					}
				}
				catch(Exception e)
				{
					ShowFMessage(e.Message);
				}
				finally
				{
					stXmlRunInfo.xXmlTextRr.Close();
				}
			}
			else	////如果该测试文件对像，是以给定首结点的方式执行的
			{
				//	stXmlRunInfo.bStop = false;
				SearchXmlNode(stXmlRunInfo.xFirstNode, true);	//直接从首节点开始搜下面的结点，并分析运行它们要求的操作
			}
			CloseTransPro();
			ClearEnvironment();	//清除本测试用例的环境,该函数只接跳到CLEAR结点开始执行
			DisConnect(-1);//释放全部连接
			ShowSMessage("测试完成，开始检测是否能够连接上DM服务器--------");
			if(bTransfer == false)
			{
				if(CheckServerConnected())
				{
					int index = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
					if(index != -1)
					{
						string filename = stXmlRunInfo.sXmlFileName.Substring(index + 1);
						if(cSqlCase.stCaseResult.bSuccess)
						{
					
							ShowSMessage("测试通过");
							ProClass.InsertTestResult(filename, stXmlRunInfo.sXmlFileName, "Y");
							RWArr cRWArr;
							cRWArr.TestExName =GetXmlSingleFileName(stXmlRunInfo.sXmlFileName);//记录测试结果信息
							DateTime tm=DateTime.Now ;
							cRWArr.TestDateTime =tm.ToLongTimeString ();
							cRWArr.TestResult ="Success";
							MainForm.mytestarr.Add (cRWArr);//保存测试结果信息到mytestarr数组中
						}
						else
						{
							ShowFMessage("测试失败");
							ProClass.InsertTestResult(filename, stXmlRunInfo.sXmlFileName, "N");
							RWArr cRWArr=new RWArr ();
							cRWArr.TestExName =GetXmlSingleFileName(stXmlRunInfo.sXmlFileName);//记录测试结果信息
							DateTime tm=DateTime.Now ;
							cRWArr.TestDateTime =tm.ToLongTimeString ();
							cRWArr.TestResult ="Fail";
							MainForm.mytestarr.Add (cRWArr);//保存测试结果信息到mytestarr数组中
						}
					}
				}
				else
				{
					RWArr cRWArr=new RWArr ();
					cRWArr.TestExName =GetXmlSingleFileName(stXmlRunInfo.sXmlFileName);//记录测试结果信息
					DateTime tm=DateTime.Now ;
					cRWArr.TestDateTime =tm.ToLongTimeString ();
					cRWArr.TestResult ="Severity";
					MainForm.mytestarr.Add (cRWArr);//保存测试结果信息到mytestarr数组中
				}
	
			}
			if(bTransfer == false)
				ShowSMessage("结束\n\n");
			ProClass.ClearSaveProValue();//清空脚本中保存的全局替代符
			return cSqlCase.stCaseResult.bSuccess;
		}

		public String CreateConnectStrEx()
		{
			string m_connectStr;
#if DM7
			m_connectStr = "Server=" + ProClass.GetServerName(ProClass.sValServer);
            m_connectStr += ";User Id=" + ProClass.sValUserId;
            m_connectStr += ";PWD=" + ProClass.sValPassword;
#else

			m_connectStr = "Provider=" + ProClass.sValDriveName + ";User ID=" + ProClass.sValUserId;
			m_connectStr += ";Password=" + ProClass.sValPassword;
			if (ProClass.GetIsEnableConnectPool())
			{
				m_connectStr += ";Data Source=";
			}
			else
			{
				m_connectStr += ";Connect Pool = -1;Data Source=";
			}
			m_connectStr += ProClass.GetServerName(ProClass.sValServer);
			if (ProClass.sValDatabase.Length > 0)
			{
				m_connectStr += ";Initial Catalog=";
				m_connectStr += ProClass.sValDatabase;
			}
			if (ProClass.GetServerPort(ProClass.sValServer).Length > 0)
			{
				m_connectStr += ";Port=";
				m_connectStr += ProClass.GetServerPort(ProClass.sValServer);
			}
#endif
			return m_connectStr;
		}
		public bool CheckServerConnected()
		{
			try
			{
				SetConnectInfo();
#if DM7
				DmConnection cn = new DmConnection(CreateConnectStrEx());
#else
				OleDbConnection cn = new OleDbConnection(CreateConnectStrEx());
#endif
				
				try
				{
					ShowSMessage("测试连接串：" + cn.ConnectionString);
					cn.Open();
					cn.Close();
				}
#if DM7
				catch(DmException e)
				{
					string errorMessages = "";

                    errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
				catch(OleDbException e)
				{
					string errorMessages = "";
					for (int i=0; i < e.Errors.Count; i++)
					{
						errorMessages += "Index #" + i + "\n" +
							"Message: " + e.Errors[i].Message + "\n" +
							"NativeError: " + e.Errors[i].NativeError + "\n" +
							"Source: " + e.Errors[i].Source + "\n" +
							"SQLState: " + e.Errors[i].SQLState + "\n";
					}
#endif
				
					ShowFMessage(errorMessages);
					ShowFMessage("用例运行完成后，检测到连接不上DM服务器--------");
					ShowSMessage("尝试重新启动DM服务器");
					/*string ret1=ProClass.CommServer.ConnectSev();
					if(ret1 == "connect failed")
						ShowFMessage("连接服务器端测试工具server失败!");
					else
						ShowSMessage("连接服务器端测试工具server成功!");*/
					string mes="START_SERVER";
					string ret;
					ret=ProClass.CommServer.Send_Rec_Message(mes);
					ShowSMessage("发送启动服务器命令：" + mes + "服务器执行结果：" + ret + "\n");
					//ProClass.CommServer.CloseStream();
					
					return false;
				}
                catch (Exception e)
                {
                    ShowFMessage(e.Message);
                    ShowFMessage("用例运行完成后，检测到连接不上DM服务器--------");
                    ShowSMessage("尝试重新启动DM服务器");
                    string mes = "START_SERVER";
                    string ret;
                    ret = ProClass.CommServer.Send_Rec_Message(mes);
                    ShowSMessage("发送启动服务器命令：" + mes + "服务器执行结果：" + ret + "\n");
                    return false;
                }
				
				ShowSMessage("用例运行完成后，检测到可以连接上DM服务器--------");
			}

#if DM7
				catch(DmException e)
				{
					string errorMessages = "";

                    errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
			catch(OleDbException e)
			{
				string errorMessages = "";
				for (int i=0; i < e.Errors.Count; i++)
				{
					errorMessages += "Index #" + i + "\n" +
						"Message: " + e.Errors[i].Message + "\n" +
						"NativeError: " + e.Errors[i].NativeError + "\n" +
						"Source: " + e.Errors[i].Source + "\n" +
						"SQLState: " + e.Errors[i].SQLState + "\n";
				}
#endif
				ShowFMessage(errorMessages);
				ShowFMessage("检测服务器时发生异常");
			}
            catch(Exception e)
            {
                ShowFMessage(e.Message);
                ShowFMessage("检测服务器时发生异常");
            }

			return true;
		}

		/// <summary>
		/// //用来运行一个节点以内的内容
		/// </summary>
		public bool RunInAnNode()
		{			
			if(stXmlRunInfo.xFirstNode == null)
			{
				Debug.Assert(stXmlRunInfo.xFirstNode != null, "XML文件字符串为空", "XmlTest.RunInAnNode 函数");
				return false;			
			}
			else
			{
				//		stXmlRunInfo.bStop = false;
				SearchXmlNode(stXmlRunInfo.xFirstNode.FirstChild, true);
				if(cSqlCase.stCaseResult.bSuccess != cSqlCase.stCaseResult.bExpResult)
				{
					stXmlRunInfo.tdTestThread .ShowFMessage("NEWCONNECTEXECUTE节点中预期的执行结果和实际的执行结果不一致！预期：" + cSqlCase.stCaseResult.bExpResult + "; 实际：" + cSqlCase.stCaseResult.bSuccess, true);
					cSqlCase.stCaseResult.bSuccess = false;
				}
				else
				{
					cSqlCase.stCaseResult.bSuccess = true;
				}
				CloseTransPro();
				this.DisConnect(-1);
			}
			return cSqlCase.stCaseResult.bSuccess;
		}
		/// <summary>
		/// //用来运行该测试用例的环境清除函数
		/// </summary>
		public void RunClear()
		{
			Debug.Assert(stXmlRunInfo.xFirstNode != null, "在执行清除工作时，给定的起始结点为空值", "XmlTest.RunClear 函数");
			ShowSMessage("开始执行清除工作");
			stXmlRunInfo.bStop = false;
			stXmlRunInfo.bClearEn = true;
			stXmlRunInfo.cCurrentSqlCase = new SqlCase();
			SearchXmlNode(stXmlRunInfo.xFirstNode, true);
			stXmlRunInfo.bClearEn = false;
            //-----test			
			sqlCaseCounter.sqlCaseNum = 0;  //记数变量归零
            sqlCaseCounter.sqlCaseLineNum = 0;
			sqlCounter.sqlNum = 0;
            sqlCounter.sqlLineNum = 0;
            //-----test
			ShowSMessage("清除结束");			
		}
		/// <summary>
		/// //用来遍历XML结点,并分析各个结点
		/// </summary>
		public void SearchXmlNode(XmlNode  m_XmlNode, bool bFindNext)///////////////////////////////////
		{
			if(m_XmlNode == null)
				return;
			try
			{
				//Debug.Assert(m_XmlNode != null, "给定的XML结点为空值", "XmlTest.SearchXmlNode 函数");
				while(m_XmlNode!=null && stXmlRunInfo.bStop != true)
				{
					if(m_XmlNode.Name == "SQL_CASE")
					{
						//---------test
                        sqlCaseCounter.sqlCaseNum++;
                        //---------test
						SqlCase cTempSqlCase = new SqlCase();
						vlc.SetVal("CASERESULT", "TRUE");
						cTempSqlCase.cParentCase = stXmlRunInfo.cCurrentSqlCase;
						stXmlRunInfo.cCurrentSqlCase = cTempSqlCase;
						SearchXmlNode(m_XmlNode.FirstChild, true);	//遍历当前节点的子节点
						if(cTempSqlCase.stCaseResult.bSuccess != cTempSqlCase.stCaseResult.bExpResult)
						{
							stXmlRunInfo.tdTestThread .ShowFMessage("SQL_CASE中预期的执行结果和实际的执行结果不一致！预期：" + cTempSqlCase.stCaseResult.bExpResult + "; 实际：" + cTempSqlCase.stCaseResult.bSuccess, true);
                            //------test
							sqlCaseCounter.sqlCaseLineNum = sqlCaseCounter.findLine(stXmlRunInfo.sXmlFileName,"<SQL_CASE>", sqlCaseCounter.sqlCaseNum);
							sqlCaseCounter.sqlCaseLineNum =sqlCaseCounter.sqlCaseLineNum -1;
							if(sqlCaseCounter.sqlCaseLineNum > 0)
							  stXmlRunInfo.tdTestThread .ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消，即：下面注释掉的一行
								//stXmlRunInfo.tdTestThread .ShowFMessage("第 "+ CurrentLineNum + " 行 " + "执行错误！" ,true);
							//------test
							cSqlCase.stCaseResult.bSuccess = false;
							if(!ProClass.bValIsErrRun && stXmlRunInfo.bClearEn == false)//如果不允许继续运行下面一个结点的值
								stXmlRunInfo.bStop = true;
						}
						stXmlRunInfo.cCurrentSqlCase = stXmlRunInfo.cCurrentSqlCase.cParentCase;
					}
					else if(AnalyseNode(ref m_XmlNode))		//分析结点关键字，并执行
					{
						SearchXmlNode(m_XmlNode.FirstChild, true);	//遍历当前节点的子节点				
					}
					else
					{
						if(stXmlRunInfo.cCurrentSqlCase.stCaseResult.bBreak)
							return;
					}

					if (bFindNext) {
						if (m_XmlNode != null) {
							m_XmlNode = m_XmlNode.NextSibling;
						}						
					}
					else
					{
						break;
					}
				}
			}
			catch(Exception e)
			{
				ShowFMessage("在分析结点时报异常！" + e.Message);
			}
		}

		private XmlNode ExeElseNode(XmlNode m_XmlNode)
		{
			while(m_XmlNode != null)
			{
				if (m_XmlNode.Name == "ELSE") 
				{
		//			ShowSMessage("ELSE进入");
					SearchXmlNode(m_XmlNode.FirstChild, true);
		//			ShowSMessage("ELSE退出");
					break;
				}
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return m_XmlNode;
		}
		//多个进程分别执行脚本段中的各个脚本
		private void TogetherRun(XmlNode m_XmlNode)
		{
			TRANS_STRUCT ts = new TRANS_STRUCT();
			tCurrentTransStruct = ts;
			tCurrentTransStruct.cProcess = new Process();
			Process cProcess = tCurrentTransStruct.cProcess;
            cProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			/*
			string Proapp_dir=Environment.CurrentDirectory;
			Proapp_dir+= "\\";
			if(Proapp_dir.Length >= 256)
			{
				ShowFMessage("某文件夹路径深度已经超过了256个字符");
				return;
			}
			DirectoryInfo m_di = null;
			DirectoryInfo[] di = null;
			m_di = new DirectoryInfo(Proapp_dir);
			di = m_di.GetDirectories("DisposeTrans.exe");//返回当前目录的子目录
			if(di == null)
			{
				ShowFMessage("在应用程序当前目录下找不到DisposeTrans.exe文件");
				return;
			}
			else
			{ 
			 */
			cProcess.StartInfo.FileName = tCurrentTransStruct.cProcess.StartInfo.WorkingDirectory + "\\DisposeTrans.exe";
			cProcess.StartInfo.RedirectStandardInput = true;
			cProcess.StartInfo.RedirectStandardOutput = true;
			cProcess.StartInfo.RedirectStandardError = true;
			cProcess.StartInfo.UseShellExecute = false;
			cProcess.StartInfo.CreateNoWindow = true;
			cProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			cProcess.StartInfo.Arguments = "\"";
			cProcess.StartInfo.Arguments += sProvider + "\" \"" + sServerName + "\" \"" + sUid
				+ "\" \"" + sPwd + "\" \"" + sDatabase + "\" \"" + sPort + "\" \"进程:1\""; 
			cProcess.StartInfo.Arguments += " \"" + stXmlRunInfo.sXmlFileName + "\""; 
			try
			{				
				cProcess.Start();
				cProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				tCurrentTransStruct.cReadThread = new Thread(new ThreadStart(ReadTransProOutput));//创建一个线程委托来运行测试函数
				tCurrentTransStruct.cReadThread.Priority = ThreadPriority.BelowNormal;
				tCurrentTransStruct.cReadThread.Start();
				while(tCurrentTransStruct != null)
					Thread.Sleep(0);
				cProcess.StandardInput.WriteLine(m_XmlNode.OuterXml.Replace("\r", " ").Replace("\n", " "));	//向脚本进程写入执行脚本
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message+":在应用程序当前目录下找不到DisposeTrans.exe文件");
			}
			cProcess.StandardInput.WriteLine("EXIT");
			cProcess.WaitForExit();	
		}
		/// <summary>
		/// //多个线程执行脚本中的内容
		/// </summary>
		private void MoreThreadExecute(XmlNode m_XmlNode)
		{
			tCurrentTransStruct =  new TRANS_STRUCT();
			tCurrentTransStruct.cProcess = new Process();
			Process cProcess = tCurrentTransStruct.cProcess;

			cProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
		
			cProcess.StartInfo.FileName = tCurrentTransStruct.cProcess.StartInfo.WorkingDirectory + "\\DisposeTrans.exe";
			cProcess.StartInfo.RedirectStandardInput = true;
			cProcess.StartInfo.RedirectStandardOutput = true;
			cProcess.StartInfo.RedirectStandardError = true;
			cProcess.StartInfo.UseShellExecute = false;
			cProcess.StartInfo.CreateNoWindow = true;
			cProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			cProcess.StartInfo.Arguments = "\"";
			cProcess.StartInfo.Arguments += sProvider + "\" \"" + sServerName + "\" \"" + sUid
				+ "\" \"" + sPwd + "\" \"" + sDatabase + "\" \"" + sPort + "\" \"进程:1\""; 
			cProcess.StartInfo.Arguments += " \"" + stXmlRunInfo.sXmlFileName + "\""; 
			try
			{					
				cProcess.Start();
            //    ShowSMessage("开始调用DisposeTrans.exe");
				cProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				tCurrentTransStruct.cReadThread = new Thread(new ThreadStart(ReadTransProOutput));//创建一个线程委托来运行测试函数
				tCurrentTransStruct.cReadThread.Priority = ThreadPriority.BelowNormal;
				tCurrentTransStruct.cReadThread.Start();
				while(tCurrentTransStruct != null)
					Thread.Sleep(0);
				cProcess.StandardInput.WriteLine(m_XmlNode.OuterXml.Replace("\r", " ").Replace("\n", " "));	//向脚本进程写入执行脚本
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message+":在应用程序当前目录下找不到DisposeTrans.exe文件");
				return;
			}
			cProcess.StandardInput.WriteLine("EXIT");
			cProcess.WaitForExit();
		}
		public string GetCurrentTimesName()
		{
			LOOP_STRUCT cTemp = cLoop;
			int up = -1;
			while (cTemp != null) 
			{
				up++;
				cTemp = (LOOP_STRUCT)cTemp.prev;				
			}
			string ret = "";
			for (int i=0; i<up; i++) {
				ret += " ";
			}
			ret += "TIMES";
			return ret;
		}
		/// <summary>
		/// //用来得执行LOOP循环
		/// </summary>
		private void StartLoopNode(XmlNode m_XmlNode)
		{
			bool m_noShowTemp = m_noShow;
			if(cLoop == null)
			{
				cLoop = new LOOP_STRUCT();
				cLoop.prev = null;
				cLoop.times = 0;
			}
			else
			{
				LOOP_STRUCT tempLoop = new LOOP_STRUCT();
				tempLoop.prev = cLoop;
				tempLoop.times = 0;
				cLoop = tempLoop;
			}
			try
			{
				XmlNode m_xmlTempNode = this.FindXmlNode(m_XmlNode.FirstChild, "STARTTIMES");
				if (m_xmlTempNode == null) 
				{
					cLoop.start = 1;
				}
				else
				{
					String sValues = GetSqlExpVal(m_xmlTempNode);
					if (sValues == null) 
					{
						sValues = GetNodeText(m_xmlTempNode);	
					}
					cLoop.start = Convert.ToInt32(ReplaceRunInfo(sValues));
				}
				m_xmlTempNode = this.FindXmlNode(m_XmlNode.FirstChild, "TIMES");
				if (m_xmlTempNode == null) 
				{
					cLoop.times = 2147000000;
				}
				else
				{
					String sValues = GetSqlExpVal(m_xmlTempNode);
					if (sValues == null) 
					{
						sValues = GetNodeText(m_xmlTempNode);	
					}
					cLoop.times = Convert.ToInt32(ReplaceRunInfo(sValues));
				}
			}
			catch(Exception e)
			{
				ShowFMessage("没有找到LOOP执行的次数，或是次数的字符串非法" + e.Message);
			}

			XmlNode m_XmlNodeTemp = FindXmlNodeEx(m_XmlNode.FirstChild, "NOSHOW");
			if (m_XmlNodeTemp != null) 
			{
				m_noShow = true;
				String val = GetNodeText(m_XmlNodeTemp);
				if (String.Compare("false", val, true) == 0) 
				{
					m_noShow = false;
				}
				else
				{
					ShowSMessage("将隐藏执行循环中的脚本.....");
				}
			}
			int times = cLoop.times;

			if (cLoop.start>0) {
				cLoop.times = cLoop.start;
				while(cLoop.times <= times && stXmlRunInfo.cCurrentSqlCase.stCaseResult.bBreak == false && stXmlRunInfo.bStop != true)
				{
//					vlc.SetVal(sTimesName, cLoop.times.ToString());
					SearchXmlNode(m_XmlNode.FirstChild, true);
					cLoop.times ++;
				}
			}
			else{
				cLoop.times = times;
				while(cLoop.times >= 1 && stXmlRunInfo.cCurrentSqlCase.stCaseResult.bBreak == false && stXmlRunInfo.bStop != true)
				{
//					vlc.SetVal(sTimesName, cLoop.times.ToString());
					SearchXmlNode(m_XmlNode.FirstChild, true);
					cLoop.times --;
				}
			}
//			vlc.SetVal(sTimesName, null);
			cLoop = (LOOP_STRUCT)cLoop.prev;
			if (m_XmlNodeTemp != null) 
			{
				m_noShow = m_noShowTemp;
			}
		}
		public void GetFileSize(string sFileName)
		{
			sFileName=ReplaceRunInfo(sFileName);
			System.IO.FileInfo fi=new FileInfo (sFileName);
			if(!fi.Exists )
			{
				ShowFMessage(sFileName+" 文件不存在或者它是个目录！");
			}
			else
			{
				vlc.SetVal("FILESIZE",(fi.Length).ToString ());
			}

		}
		/// <summary>
		/// //用来得到当前结点的值
		/// </summary>
		public string GetNodeText(XmlNode m_XmlNode)
		{
			if(m_XmlNode == null)
			{
				return "";
			}
			m_XmlNode = m_XmlNode.FirstChild;
			while(m_XmlNode != null)
			{
				if(m_XmlNode.Name == "#text")
				{
					return m_XmlNode.Value;
				}
				else if(m_XmlNode.Name == "#significant-whitespace")
				{
					return m_XmlNode.Value;
				}
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return "";
		}
		
		//检察TYPE关键字的值是否合法
		public bool CheckTypeValue(string sValue)
		{
			if(sValue == "DIRECT_EXECUTE_SUCCESS")
			{
				return true;
			}
			if(sValue == "DIRECT_EXECUTE_FAIL")
			{
				return true;
			}
			if(sValue == "DIRECT_EXECUTE_SELECT_WITH_RESULT")
			{
				return true;
			}
			if(sValue == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT")
			{
				return true;
			}
			if(sValue == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT_FULL")
			{
				return true;
			}
			if(sValue == "DIRECT_EXECUTE_IGNORE")
			{
				return true;
			}
			if(sValue == "LOGIN_SUCCESS")
			{
				return true;
			}
			if(sValue == "LOGIN_FAIL")
			{
				return true;
			}

			ShowFMessage("脚本中存在非法的预期执行结果关键字(就是TYPE关键字包含部分的字符串不是已被订义的)");
			return false;
		}
		/// <summary>
		/// //通过执行语句生成一个字符串
		/// </summary>
		public String GetStrFromSql(string _sql, bool _CheckError)
		{
			string ret = null;
			if(stConnectINfo == null)
			{
				if(!Connection())
				{
					return ret;
				}
			}
			else if(stConnectINfo.cn.State != ConnectionState.Open)
			{
				if(!ReConnect())
				{
					return ret;
				}
			}
#if DM7
            DmConnection g_cn = stConnectINfo.cn;
            DmDataReader dr = null;
#else
			OleDbConnection g_cn = stConnectINfo.cn;
			OleDbDataReader dr = null;
#endif
            if (!m_noShow) 
			{
				ShowSMessage(_sql);
			}	
			try
			{
			//	g_cn.Open();
#if DM7
                DmCommand g_cm = g_cn.CreateCommand();
#else
				OleDbCommand g_cm = g_cn.CreateCommand();
#endif
                g_cm.CommandText = _sql;
        //        Console.Write(_sql);
				dr = g_cm.ExecuteReader();///////////////////////////////////////
				if (dr.FieldCount == 0) {
					ret = "1";
				}
				else{
					if (dr.HasRows) {
						dr.Read();
						ret = "NULL";
						if(dr[0] != DBNull.Value)
						{
							ret = Convert.ToString(dr[0]);
						}						
					}
				}
			}
			catch(Exception e)
			{
				if (_CheckError) {
					ShowSMessage(_sql);
					ShowSMessage(e.Message);
				}
				else
				{					
					ShowSMessage(e.Message);
				}
			}
			if (dr != null) {
				dr.Close();
			}
//  			if (g_cn.State == ConnectionState.Open) {
//  				g_cn.Close();
//  			}
			return ret;
		}

		
		public String GetSqlExpVal(XmlNode m_XmlNode)
		{
			string err;
			string ret;
			m_XmlNode = FindXmlNode(m_XmlNode.FirstChild, "EXP");
			if (m_XmlNode == null) 
			{
				return null;
			}
			ret = ProClass.GetSqlExpVal(ReplaceRunInfo(GetNodeText(m_XmlNode)), out err);
			if (err != "") 
			{
				ShowFMessage("表达式 (" +ReplaceRunInfo(GetNodeText(m_XmlNode)) + ") 语法错:" + err);
			}
			return ret;
		}
		/// <summary>
		/// //用来分析结点，并且根据节点关键字，调用相应的函数执行节点
		/// </summary>
		/// //每调用一次分析结点，行号 CurrentLineNum就加1，行否？
		public bool AnalyseNode(ref XmlNode m_XmlNode)
		{
            CurrentLineNum++;   //加1
			Debug.Assert(m_XmlNode != null, "给定的XML结点为空值", "XmlTest.AnalyseNode 函数");
			string strFromSql = "FromSql:";
			stXmlRunInfo.xCurrentNode = m_XmlNode;
			bool m_FindChild = false;						//用来表示，该函数返回后，是否还要搜索它的子节点
			string m_name = m_XmlNode.Name;	
			string sValues;
			string sTempStr;

			switch(m_XmlNode.Name.ToUpper()) {				
				case "SQL":
					sqlCounter.sqlNum++;
					ExecuteSQL(GetNodeText(m_XmlNode));
					break;
				case "FILESIZE":
					string sValue=GetNodeText(m_XmlNode).Trim();
					GetFileSize(sValue);
					break;
				case "TESTPOINTBEGIN":
					testnum=GetNodeText(m_XmlNode);//把当前所执行到的测试点的辅助信息保存在变量中，有利于报错时的定位
					break;
				case "TYPE":
					stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult = GetNodeText(m_XmlNode).Trim();
					CheckTypeValue(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult);
					break;
				case "RESULT":
					break;				
				case "TEMPSTR":
					sTempStr = (string)vlc.GetVal("TEMPSTR");
					vlc.SetVal("TEMPSTR", null);
					sValues = GetSqlExpVal(m_XmlNode);
					if (sValues == null) 
					{
						sValues = GetNodeText(m_XmlNode);	
					}
					else{
						vlc.SetVal(m_name, sValues);
						break;
					}
					if (sValues == "") 
					{
						vlc.SetVal("TEMPSTR", "");
					}
					else if (sValues == "@") 
					{
						sValues = sTempStr;						
						sValues = ReplaceRunInfo(sValues);
						vlc.SetVal("TEMPSTR", sValues);
					}
					else
					{						
						sValues = ReplaceRunInfo(sValues);
						if (sValues.StartsWith(strFromSql)) 
						{
							sValues = GetStrFromSql(sValues.Substring(strFromSql.Length), true);
							vlc.SetVal("TEMPSTR", sValues);
							break;
						}
						if (sValues != null) 
						{
							sValues = sTempStr+ sValues;		
							vlc.SetVal("TEMPSTR", sValues);
						}					
					}
					break;
				case "BEGINTRANS":
					StartTrans(GetNodeText(m_XmlNode).Trim());
					break;
				case "ENDTRANS":
					EndTrans(GetNodeText(m_XmlNode).Trim());
					break;
				case "LOOP":
					StartLoopNode(m_XmlNode);
					break;
				case "TIMES":
					if(cLoop == null && isHasTimes == false)
					{
						isHasTimes = true;
						try
						{
							sValues = GetSqlExpVal(m_XmlNode);
							if (sValues == null) 
							{
								sValues = GetNodeText(m_XmlNode);	
							}
							run_times = Convert.ToInt32(sValues.Trim());
						}
						catch(Exception e)
						{
							ShowFMessage("非法的执行次数" + e.Message);
						}					
					}
					break;
				case "STARTTIMES":
					if(cLoop == null)
					{
						ShowFMessage("STARTTIMES关键字只能在LOOP关键字中使用");				
					}
					break;
				case "NEWCONNECTEXECUTE":
				//	ExecuteNode();
					MoreThreadExecute(m_XmlNode);
					break;
				case "RECONNECT":
					ReConnect();
					m_FindChild = true;
					break;
				case "SERVER":					
					sServerName = ReplaceRunInfo(GetNodeText(m_XmlNode).Trim());
					break;
				case "UID":
					sUid = GetNodeText(m_XmlNode).Trim();
					sUid = ReplaceRunInfo(sUid);
					break;
				case "DATABASE":
					sDatabase = GetNodeText(m_XmlNode).Trim();	
					sDatabase = ReplaceRunInfo(sDatabase);
					break;
				case "PWD":
					sPwd = GetNodeText(m_XmlNode).Trim();
					sPwd = ReplaceRunInfo(sPwd);
					break;
				case "PROVIDER":
					sProvider = GetNodeText(m_XmlNode).Trim();
					vlc.AddPrimaryKey("PROVIDER", sProvider);
					break;
				case "POOL":
					if (String.Compare("TRUE", GetNodeText(m_XmlNode).Trim(), true) == 0) {
						bPoolEnable = true;
						vlc.AddPrimaryKey("POOL", "TRUE");
					}
					else
					{
						bPoolEnable = false;
						vlc.AddPrimaryKey("POOL", "FALSE");
					}
					break;
				case "PORT":
					sPort = GetNodeText(m_XmlNode).Trim();
					sPort = ReplaceRunInfo(sPort);
					break;
				case "EXEPROCESS":
					ExecuteProcess(ReplaceRunInfo(GetNodeText(m_XmlNode).Trim()));
					break;
				case "EXEPROCESSEX":
					ExecuteProcessEx(ReplaceRunInfo(GetNodeText(m_XmlNode).Trim()));
					break;
				case "NEWTRANS":
					StartNewTrans();
					break;
				case "EFFECTROWS":
					try
					{
						int iRows = Convert.ToInt32(GetNodeText(m_XmlNode).Trim());
						CheckEffectRows(iRows);
					}
					catch(Exception e)
					{
						ShowFMessage("表示影响的行数时，使用了非法的字符串，" + e.Message);
					}
					break;
				case "MORETHREAD":
					MoreThreadExecute(m_XmlNode);
					break;
				case "THREADS":
					break;
				case "TOGETHER":
					TogetherRun(m_XmlNode);
					break;
				case "NOSHOW":
					if (m_noShow == false && cLoop == null) 
					{
						ShowSMessage("正在隐式运行一段脚本，可能需要比较长的时间.....");
					}
					if (m_noShow) 
					{
						SearchXmlNode(m_XmlNode.FirstChild, true);
					}
					else
					{
						m_noShow = true;				
						SearchXmlNode(m_XmlNode.FirstChild, true);
						m_noShow = false;
					}
					break;
				case "SQLSTATE":
					stXmlRunInfo.cCurrentSqlCase.stSqlResult.sSQLState = GetNodeText(m_XmlNode).Trim();	
					break;
				case "NERROR":
					try
					{					
						stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError = Convert.ToInt32(GetNodeText(m_XmlNode).Trim());	
					}
					catch(Exception e)
					{
						ShowFMessage("非法的数字字符串表示NetiveError," + e.Message);
					}
					break;
				case "CASEEXPRESULT":
					sValues = GetNodeText(m_XmlNode).Trim();				
					stXmlRunInfo.cCurrentSqlCase.stCaseResult.bExpResult = (string.Compare(sValues, "TRUE", true) == 0);
					break;
				case "COMPARERESULT":
					CompareResult(m_XmlNode);
					break;
				case "SERVERCMD":     //与服务器组件 服务器控制有关的三个关键字 SERVERCMD、RUNSERVER、REBOOT
					sValues = GetNodeText(m_XmlNode).Trim();
					sValues = ReplaceRunInfo(sValues);
					if(sValues.ToUpper()=="EXIT"||sValues.ToUpper()=="DEBUG")
					{
						//8.27 修改 负责通信即可  不需重连
						/*string ret3=ProClass.CommServer.ConnectSev();   //连接到组件
						if(ret3 == "connect failed")
						{
							ShowFMessage("连接服务器端测试工具server失败");
							break;
						}
						else
						{
							ShowSMessage("连接服务器端测试工具server成功");
						}*/
						string mess3="STOP_SERVER";
						string stop_ret;
						stop_ret=ProClass.CommServer.Send_Rec_Message(mess3);   //发送消息  重启服务器
						ShowSMessage("发送启动服务器命令：" + mess3 + "服务器执行结果：" + stop_ret + "\n");
						
					}
					else
						ShowSMessage("脚本中SERVERCMD命令不对");
					//ProClass.cDServer.SendCommand(sValues);
					break;
				case "RUNSERVER":
					//8.27 修改 负责通信即可  不需重连
					/*string ret1=ProClass.CommServer.ConnectSev();   //连接到组件
					if(ret1 == "connect failed")
					{
						ShowFMessage("连接服务器端测试工具server失败");
						break;
					}
					else
					{
						ShowSMessage("连接服务器端测试工具server成功");
					}*/
					string mes="START_SERVER";
					string ret;
					ret=ProClass.CommServer.Send_Rec_Message(mes);   //发送消息  启动服务器

					ShowSMessage("发送启动服务器命令：" + mes + "服务器执行结果：" + ret + "\n");
					//ProClass.CommServer.CloseStream();  //
					ProClass.cDServer.WaitServerStart();	
					ShowSMessage("启动DM服务器后，XML工具尝试重新连接DM");
					if (stConnectINfo == null) 
					{
						Connection();
					}
					else
					{
						ReConnect();
					}
					break;
				case "RUNCMD":
					string retcmd;
					retcmd = ProClass.CommServer.Send_Cmd(GetNodeText(m_XmlNode).Trim());
					ShowSMessage("准备执行命令：  " + GetNodeText(m_XmlNode).Trim());
					if(retcmd.Substring(0,1) == "1")
					{
						ShowSMessage(retcmd.Substring(1));
					}
					else
					{
						ShowFMessage(retcmd.Substring(1));
					}
					break;
				case "REBOOT":
					//8.27 修改 负责通信即可  不需重连
					/*string ret2=ProClass.CommServer.ConnectSev();   //连接到组件
					if(ret2 == "connect failed")
					{
						ShowFMessage("连接服务器端测试工具server失败");
						break;
					}
					else
					{
						ShowSMessage("连接服务器端测试工具server成功");
					}*/
					string mess="RESTART_SERVER";
					string restart_ret;
					restart_ret=ProClass.CommServer.Send_Rec_Message(mess);   //发送消息  重启服务器
					ShowSMessage("发送启动服务器命令：" + mess + "服务器执行结果：" + restart_ret + "\n");
					//ProClass.CommServer.CloseStream();

					ShowSMessage("重启DM服务器后，XML工具尝试重新连接DM");
					if (stConnectINfo == null) 
					{
						Connection();
					}
					else
					{
						ReConnect();
					}
					break;
				case "SETDMINI":    
					ModifyIni(m_XmlNode);
					break;
				case "GETDMINI":    
					GetIni(m_XmlNode);
					break;
				case "COPYFILE":    
					CopyFile(m_XmlNode);
					break;
				case "DELFILE":     
					DeleFile(GetNodeText(m_XmlNode).Trim());
					break;
				case "CREATEFILE":  
					CreateFile(m_XmlNode);
					break;
				case "SETVAL":
					SetVal(m_XmlNode);
					break;
				case "GETVAL":
					GetVal(m_XmlNode);
					break;
				case "IF":
					DoIf(ref m_XmlNode);
					break;
				case "ELSE":
					break;
				case "FMES":
					sValues = GetNodeText(m_XmlNode).Trim();
					ShowFMessage(ReplaceRunInfo(sValues));
					break;
				case "SMES":
					sValues = GetNodeText(m_XmlNode).Trim();
					ShowSMessage(ReplaceRunInfo(sValues));
					break;
				case "INITDB":     //这边的处理函数未修改
					sValues = GetNodeText(m_XmlNode).Trim();
					sValues = ReplaceRunInfo(sValues);
					/*if (!ProClass.cDServer.InitDb(sValues)) 
					{
						ShowFMessage("初始化库失败");
					}*/
					//string initdb_con=ProClass.CommServer.ConnectSev();   //连接到组件
					string mess_initdb="INIT_DB"+'$'+sValues+ '$' + xmlFileName;   //copy file
					string initdb_ret;
					initdb_ret=ProClass.CommServer.Send_Rec_Message(mess_initdb);   //发送消息  重启服务器
					//ProClass.CommServer.CloseStream();
					ShowSMessage("发送启动服务器命令：" + mess_initdb + "服务器执行结果：" + initdb_ret + "\n");
					break;
				case "CONTENT":
					ShowSMessage(GetNodeText(m_XmlNode).Trim());
					break;
				case "EXEXML":
					ExeXml(GetNodeText(m_XmlNode).Trim());
					break;
				case "CONNECT":
					sValues = GetNodeText(m_XmlNode).Trim();
					try
					{
						if (sValues != "") 
						{
							int connectNum = Convert.ToInt32(sValues);
							if (connectNum>=stConnectArry.Count) 
							{
								ConnectionEx();
							}
							else
							{
								SetCn(connectNum, true);
							}
						}
						else
						{
							ConnectionEx();
						}
					}
					catch(Exception e)
					{
						ShowFMessage("表示连个数时，使用了非法的字符串，" + e.Message);
					}								
					m_FindChild = true;
					break;
				case "SETCONNECTID":
					try
					{
						SetCn(Convert.ToInt32(GetNodeText(m_XmlNode).Trim()), FindXmlNodeEx(m_XmlNode.FirstChild, "NOSHOW")==null);
					}
					catch(Exception e)
					{
						ShowFMessage("表示连个数时，使用了非法的字符串，" + e.Message);
					}
					break;
				case "SQLTEST":
						m_FindChild = true;
					break;
				case  "CLEAR":
					if(stXmlRunInfo.bClearEn == false)
					{
						break;
					}
					stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult = "DIRECT_EXECUTE_SUCCESS";
					m_FindChild = true;				 
					break;
				case "SLEEP":
					try
					{
						int iSleep = Convert.ToInt32(GetNodeText(m_XmlNode).Trim());
						if(iSleep >= 0)
						{
							Thread.Sleep(iSleep);
						}
						else if(tCurrentTransStruct != null)
						{
							while(tCurrentTransStruct.bIsFinishExcute == false)
							{
								Thread.Sleep(1000);
							}
						}
					}
					catch(Exception e)
					{
						ShowFMessage("表示连挂起时间时，使用了非法的字符串，" + e.Message);
					}
					break;
				case "IGNORE":
					ShowSMessage("注意：程序跳过一个被要求忽略的节点");
					break;
				case "BREAK":
					stXmlRunInfo.cCurrentSqlCase.stCaseResult.bBreak = true;					
		//			ShowSMessage("注意：遇到脚本中的 BREAK 结点，程序执行跳出了它所在的SQL_CASE点范围");
					break;
				case "DISCONNECT":
					sValues = GetNodeText(m_XmlNode).Trim();
					if(sValues == "")
					{
						DisConnect(-1);
					}
					else
					{
						try
						{
							DisConnect(Convert.ToInt32(sValues));
						}
						catch(Exception e)
						{
							ShowFMessage("表示连个数时，使用了非法的字符串，" + e.Message);
						}
					}
					break;
				case "WINDOWS":
					if(ProClass.GetIsWindows())
					{
						m_FindChild = true;
					}
					break;
				case "LINUX":
					if(!ProClass.GetIsWindows())
					{
						m_FindChild = true;
					}
					break;
				case "RECORD":
					ShowFMessage("RECORD关键字只能被包含在RESULT关键字节点里面");
					break;
				case "COLUMN":
					ShowFMessage("COLUMN关键字只能被包含在RECORD关键字节点里面");
					break;
				case "PARAMETER":
					if (stConnectINfo != null) {
						AddParameter(m_XmlNode);
					}
					else{
						ShowFMessage("添加参数绑定前请先建立连接");
					}
					break;
				case "CLEARPARAMETERS":
					if (stConnectINfo != null) {
						if (stConnectINfo.rd != null) 
						{
							stConnectINfo.rd.Close();
							stConnectINfo.rd = null;						
						}		
						stConnectINfo.cm.Parameters.Clear();
					}
					else{
						ShowFMessage("CLEARPARAMETERS操作上没有连接");
					}
					break;
				case "OPEN":
					if (stConnectINfo != null) {
						stConnectINfo.isOpenResult = true;
						stConnectINfo.iFetch = 0;
					}	
					else{
						ShowFMessage("OPEN操作上没有连接");
					}
					m_FindChild = true;
					break;
				case "FETCHNEXT":
					FetchNext();
					break;
				case "TIMETICKS":
					vlc.SetVal(GetNodeText(m_XmlNode).Trim(), DateTime.Now.Ticks.ToString());
					break;
				case "ENTER":
					ShowFMessage("该关键字只能在多线程执行脚本时使用，比如放在MORETHREAD,TOGETHER关键字中");
					break;
				case "EXIT":
					ShowFMessage("该关键字只能在多线程执行脚本时使用，比如放在MORETHREAD,TOGETHER关键字中");
					break;
				case "RESULTROWS":
					try
					{
						int iRows = Convert.ToInt32(GetNodeText(m_XmlNode).Trim());
						CheckResultRows(iRows);
					}
					catch(Exception e)
					{
						ShowFMessage("表示结果集的行数时，使用了非法的字符串，" + e.Message);
					}							
					break;
				case "READFILE":
					ReadFile(m_XmlNode);
					break;
				case "DATACOMPARE":
					DoDataCompare(m_XmlNode);
					break;
				case "BINARY":
					BinaryData(m_XmlNode);
					break;
				default:
					if(m_name.StartsWith("TRANS"))
					{
						try
						{
							int iTransId = Convert.ToInt32(m_name.Substring("TRANS".Length));
							SendXmlStringToTrans(iTransId, null);
						}
						catch(Exception e)
						{
							ShowFMessage("非法的事务号，或是在向事务进程发送脚本时出现异常！" + e.Message);
						}
					}
					else if (!m_name.StartsWith("#")) {
						sValues = GetSqlExpVal(m_XmlNode);
						if (sValues == null) 
						{
							sValues = GetNodeText(m_XmlNode);	
						}
						else{
							vlc.SetVal(m_name, sValues);
							break;
						}
						if (sValues == "") 
						{
							vlc.SetVal(m_name, "");
						}
						else if (sValues == "@") 
						{
							sValues = (string)vlc.GetVal(m_name);
							sValues = ReplaceRunInfo(sValues);
							vlc.SetVal(m_name, sValues);
						}
						else
						{						
							sValues = ReplaceRunInfo(sValues);
							if (sValues.StartsWith(strFromSql)) 
							{
								sValues = GetStrFromSql(sValues.Substring(strFromSql.Length), true);
								vlc.SetVal(m_name, sValues);
								break;
							}
							if (sValues != null) 
							{
								sValues = vlc.GetVal(m_name) + sValues;		
								vlc.SetVal(m_name, sValues);
							}					
						}
						break;
					}
					break;
			}			
			return m_FindChild;
		}
		public void BinaryData(XmlNode m_XmlNode)
		{
			string valname = "";
			int datasize = 0;
			int seed = 0;
			string temp_name;
			XmlNode x_temp;
			byte[] buf = null;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VALNAME");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				if (temp_name.StartsWith("@") == false)
				{
					ShowFMessage("非法的VALNAME节点值，该值应该是一个以@打头的替代符名称");
					return;
				}
				valname = temp_name.Substring(1);
			}
			else
			{
				ShowFMessage("在BINARY节点中未找到期VALNAME节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "DATASIZE");
			if (x_temp != null) 
			{
				temp_name = GetSqlExpVal(x_temp);
				if (temp_name == null)
				{
					temp_name = GetNodeText(x_temp).Trim();				
					temp_name = ReplaceRunInfo(temp_name);
				}
				
				try
				{
					datasize = Convert.ToInt32(temp_name);
					if (datasize < 0)
					{
						ShowFMessage("非法的DATASIZE值：" + datasize);
						return;
					}
				}
				catch (System.Exception e)
				{
					ShowFMessage(e.Message);
					return;
				}
			}
			else
			{
				ShowFMessage("在BINARY节点中未指定DATASIZE节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "SEED");
			if (x_temp != null) 
			{
				temp_name = GetSqlExpVal(x_temp);
				if (temp_name == null)
				{
					temp_name = GetNodeText(x_temp).Trim();				
					temp_name = ReplaceRunInfo(temp_name);
				}
				try
				{
					seed = Convert.ToInt32(temp_name);
				}
				catch (System.Exception e)
				{
					ShowFMessage(e.Message);
					return;
				}
			}
			else
			{
				ShowFMessage("在BINARY节点中未指定SEED节点");
				return;
			}
			buf = new byte[datasize];
			byte start = (byte)(seed % 256);
			for (int i=0; i<datasize; i++)
			{
				buf[i] = start;
				if (start >= 255)
				{
					start = 0;
				}
				else
				{
					start ++;
				}				
			}
			vlc.SetVal(valname, buf);
		}
		public void DoIf(ref XmlNode m_XmlNode)
		{
			string sValues;
			string FromSql = "FromSql:";
			string err = null;
			sValues = GetNodeText(m_XmlNode).Trim();
			sValues = ReplaceRunInfo(sValues);
			string sTemp = sValues;
			if (sValues.StartsWith("BOOL:")) 
			{
				sValues = sValues.Substring(5);
                Console.WriteLine("要判断的IF语句:" + sValues);
				sValues = ProClass.GetBoolVal(sValues, out err);
				if (err != "") {
					ShowFMessage("IF判断表达式(" + sTemp + ") 语法错：" + err);
				}
			}
			else if (sValues.StartsWith(FromSql)) {
				sValues = GetStrFromSql(sValues.Substring(FromSql.Length), false);
			}
			else
			{
				sValues = ProClass.GetBoolVal(sValues, out err);
				if (err != "") 
				{
					ShowFMessage("IF判断表达式(" + sTemp + ") 语法错：" + err);
				}
			}
									
			if (sValues == null) 
			{
				m_XmlNode = ExeElseNode(m_XmlNode);
			}
		}
		public bool CheckLevel2(XmlNode m_XmlNode)//判断测试用例中设定的测试等级是否符合要测试的测试等级，若符合则true，否则返回false
		{
			m_XmlNode = SkipComment(m_XmlNode);
			int level = 15;
			if (m_XmlNode != null && m_XmlNode.Name == "LEVEL") {
				string sLevel = GetNodeText(m_XmlNode);
				if (sLevel == "") {
					level = 0;
				}
				else{
					level = Convert.ToInt32(sLevel);
				}				
				if (level > 15 || level<0) {
					ShowSMessage("非法的用例等级，等级指定只能在0-15之间");
				}
			}
			if (level <= ProClass.iValLevel2)
			{
				return true;
			}
			ShowSMessage("用例等级低于工具设定的标准，该脚本没有被执行...");
			return false;
		}
		public bool CheckLevel1(XmlNode m_XmlNode)//判断测试用例中设定的测试等级是否符合要测试的测试等级，若符合则true，否则返回false
		{
			m_XmlNode = SkipComment(m_XmlNode);
			int level=0;
			if (m_XmlNode != null && m_XmlNode.Name == "LEVEL") 
			{
				string sLevel = GetNodeText(m_XmlNode);
				if (sLevel == "") 
				{
					level = 0;
				}
				else
				{
					level = Convert.ToInt32(sLevel);
				}				
				if (level > 15 || level<0) 
				{
					ShowSMessage("非法的用例等级，等级指定只能在0-15之间");
					//return false;
				}
			}
			string s=ProClass.iValLevel1;
		    char[] separator ={' ',','};
		    string[] s_Level = s.Split(separator);
			int i=0;
			for(i=0;i<s_Level.Length;i++)
			{
				int ml=Convert.ToInt32(s_Level[i]);
				if(level==ml)
					break;
			}
			if(i<s_Level.Length||level==0)
				return true;
			else
				ShowSMessage("非法的用例等级，等级指定只能在选定的等级之中");
			    return false;
			//ShowFMessage("用例等级低于工具设定的标准，该脚本没有被执行...");
			//return false;
			
		}
		public void FetchNext()
		{
			if (stConnectINfo == null || stConnectINfo.isOpenResult == false) {
				ShowFMessage("请把该操作放在<OPEN>关键字内！");
				vlc.SetVal("FETCHNEXT", "0");
				return;
			}
			if(stConnectINfo.rd == null)
			{
				ShowFMessage("结果集对像为空，上一次执行的语句没有产生相应的结果集！");
			}
			if (stConnectINfo.rd.Read()) {
				stConnectINfo.iFetch ++;
				vlc.SetVal("FETCHNEXT", stConnectINfo.iFetch.ToString());
				for (int i=0; i<stConnectINfo.rd.FieldCount; i++) {
					string colName = stConnectINfo.rd.GetName(i);
					string sVal;
					if (colName.Length>0) {
						if(stConnectINfo.rd[i] == System.DBNull.Value)
						{
							sVal = "NULL";
							vlc.SetVal(colName, sVal);
						}
						else if(stConnectINfo.rd.GetFieldType(i) == System.Type.GetType("System.Byte[]"))
						{			
							vlc.SetVal(colName, stConnectINfo.rd[i]);
						}
						else
						{
							sVal = Convert.ToString(stConnectINfo.rd[i]);		//把列中的值。变为字符串再进行比较
							vlc.SetVal(colName, sVal);
						}
						
					}					
				}
			}
			else{
				vlc.SetVal("FETCHNEXT", "0");
			}
		}
#if DM7
        public void SetParameterValue(DmParameter pr)
#else
		public void SetParameterValue(OleDbParameter pr)
#endif
        {
			if (pr.ParameterName == "") {
				return;
			}
			string val = "";
#if DM7
            if (pr.DmSqlType != DmDbType.Binary && pr.DmSqlType != DmDbType.VarBinary && pr.DmSqlType != DmDbType.Blob)
#else
			if(pr.OleDbType != OleDbType.Binary && pr.OleDbType != OleDbType.VarBinary && pr.OleDbType != OleDbType.LongVarBinary)
#endif
            {
				val = (string)vlc.GetVal(pr.ParameterName);
				if (val != null && string.Compare("NULL", val, true) == 0) 
				{
					val = null;
					pr.Value = DBNull.Value;
					return;
				}
			}
			try
			{
#if DM7
                switch(pr.DmSqlType) 
#else
				switch(pr.OleDbType) 
#endif
                {
#if DM7
                    case DmDbType.Bit:
#else
					case OleDbType.Boolean:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToBoolean(val);
						break;
#if DM7
                    case DmDbType.Byte:
#else
					case OleDbType.TinyInt:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToSByte(val);
						break;
#if DM7
                    case DmDbType.Int16:
#else
                    case OleDbType.SmallInt:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToInt16(val);
						break;
#if DM7
                    case DmDbType.Int32:
#else
                    case OleDbType.Integer:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToInt32(val);
						break;
#if DM7
                    case DmDbType.Int64:	
#else
                    case OleDbType.BigInt:	
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToInt64(val);
						break;
#if DM7
                    case DmDbType.Float:
#else
                    case OleDbType.Single:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToSingle(val);
						break;					
#if DM7
                    case DmDbType.Double:
#else
                    case OleDbType.Double:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToDouble(val);
						break;
#if DM7
                    case DmDbType.Decimal:
#else
                    case OleDbType.Decimal:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToDecimal(val);
						break;	
#if DM7
                    case DmDbType.Binary:
#else
                    case OleDbType.Binary:
#endif
						pr.Value = vlc.GetVal(pr.ParameterName);
						break;
#if DM7
                    case DmDbType.VarBinary:
#else
                    case OleDbType.VarBinary:
#endif
						pr.Value = vlc.GetVal(pr.ParameterName);
						break;
#if DM7
                    case DmDbType.Blob:
#else
                    case OleDbType.LongVarBinary:
#endif
						pr.Value = vlc.GetVal(pr.ParameterName);
						break;
#if DM7
                    case DmDbType.Time:
#else
                    case OleDbType.DBTime:
#endif
						if (val != null && val != "")
						{
							DateTime dt = Convert.ToDateTime(val);
							TimeSpan ts = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
							pr.Value = ts;
						}						
						break;					
#if DM7
                    case DmDbType.Date:
#else
                    case OleDbType.DBDate:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToDateTime(val);
						break;					
#if DM7
                    case DmDbType.DateTime:
#else
                    case OleDbType.DBTimeStamp:
#endif
						if (val != null && val != "")
							pr.Value = Convert.ToDateTime(val);
						break;					
					default:
						pr.Value = val;												
						break;
				}
			}
			catch(Exception e)
			{
				ShowFMessage("设置参数值时报错，请查看设定的值能否转化为相应的类型:" + e.Message);
			}			
		}
#if DM7
        public void SetParameterDataType(DmParameter pr, string datatype)
#else
        public void SetParameterDataType(OleDbParameter pr, string datatype)
#endif
        {
			try
			{
				switch(datatype) 
				{
					case "BIT":
#if DM7
                        pr.DmSqlType = DmDbType.Bit;
#else
                        pr.OleDbType = OleDbType.Boolean;
#endif

                        break;
					case "TINYINT":
#if DM7
                        pr.DmSqlType = DmDbType.Byte;
#else
                        pr.OleDbType = OleDbType.TinyInt;
#endif

                            break;
					case "SMALLINT":
#if DM7
                        pr.DmSqlType = DmDbType.Int16;
#else
                        pr.OleDbType = OleDbType.SmallInt;
#endif

                        break;
					case "INT":
#if DM7
                        pr.DmSqlType = DmDbType.Int32;
#else
                        pr.OleDbType = OleDbType.Integer;
#endif

                        break;
					case "BIGINT":
#if DM7
                        pr.DmSqlType = DmDbType.Int64;
#else
                        pr.OleDbType = OleDbType.BigInt;
#endif

                        break;
					case "REAL":
#if DM7
                        pr.DmSqlType = DmDbType.Float;
#else
                        pr.OleDbType = OleDbType.Single;
#endif

                        break;					
					case "DOUBLE":
#if DM7
                        pr.DmSqlType = DmDbType.Double;
#else
                        pr.OleDbType = OleDbType.Double;
#endif

                        break;
					case "FLOAT":
#if DM7
                        pr.DmSqlType = DmDbType.Double;
#else
                        pr.OleDbType = OleDbType.Double;
#endif

                        break;
					case "NUMERIC":
#if DM7
                        pr.DmSqlType = DmDbType.Decimal;
#else
                        pr.OleDbType = OleDbType.Decimal;
#endif

                        break;	
					case "DECIMAL":
#if DM7
                        pr.DmSqlType = DmDbType.Decimal;
#else
                        pr.OleDbType = OleDbType.Decimal;
#endif

                        break;
					case "DEC":
#if DM7
                        pr.DmSqlType = DmDbType.Decimal;
#else
                        pr.OleDbType = OleDbType.Decimal;
#endif

                        break;
					case "CHAR":
#if DM7
                        pr.DmSqlType = DmDbType.Char;
#else
                        pr.OleDbType = OleDbType.Char;
#endif

                        break;	
					case "VARCHAR":
#if DM7
                        pr.DmSqlType = DmDbType.VarChar;
#else
                        pr.OleDbType = OleDbType.VarChar;
#endif

                        break;
					case "LONGVARCHAR":
#if DM7
                        pr.DmSqlType = DmDbType.Text;
#else
                        pr.OleDbType = OleDbType.LongVarChar;
#endif

                        break;
					case "TEXT":
#if DM7
                        pr.DmSqlType = DmDbType.Text;
#else
                        pr.OleDbType = OleDbType.LongVarChar;
#endif

                        break;
					case "CLOB":
#if DM7
                        pr.DmSqlType = DmDbType.Text;
#else
                        pr.OleDbType = OleDbType.LongVarChar;
#endif

                        break;
					case "BINARY":
#if DM7
                        pr.DmSqlType = DmDbType.Binary;
#else
                        pr.OleDbType = OleDbType.Binary;
#endif

                        //		ShowSMessage("工具不支持二进制作为输入参数，所以未设置其值，只设置了参数类型的属性，如果这个参数为输出参数，那么输出的二进制将被转化为字符串来显示");
						break;
					case "VARBINARY":
#if DM7
                        pr.DmSqlType = DmDbType.VarBinary;
#else
                        pr.OleDbType = OleDbType.VarBinary;
#endif

                        //		ShowSMessage("工具不支持二进制作为输入参数，所以未设置其值，只设置了参数类型的属性，如果这个参数为输出参数，那么输出的二进制将被转化为字符串来显示");
						break;
					case "IMAGE":
#if DM7
                        pr.DmSqlType = DmDbType.Blob;
#else
                        pr.OleDbType = OleDbType.LongVarBinary;
#endif

                        //		ShowSMessage("工具不支持二进制作为输入参数，所以未设置其值，只设置了参数类型的属性，如果这个参数为输出参数，那么输出的二进制将被转化为字符串来显示");
						break;
					case "BLOB":
#if DM7
                        pr.DmSqlType = DmDbType.Blob;
#else
                        pr.OleDbType = OleDbType.LongVarBinary;
#endif

                        //		ShowSMessage("工具不支持二进制作为输入参数，所以未设置其值，只设置了参数类型的属性，如果这个参数为输出参数，那么输出的二进制将被转化为字符串来显示");
						break;
					case "LONGVARBINARY":
#if DM7
                        pr.DmSqlType = DmDbType.Blob;
#else
                        pr.OleDbType = OleDbType.LongVarBinary;
#endif

                        //		ShowSMessage("工具不支持二进制作为输入参数，所以未设置其值，只设置了参数类型的属性，如果这个参数为输出参数，那么输出的二进制将被转化为字符串来显示");
						break;
					case "TIME":
#if DM7
                        pr.DmSqlType = DmDbType.Time;
#else
                        pr.OleDbType = OleDbType.DBTime;
#endif

                        break;					
					case "DATE":
#if DM7
                        pr.DmSqlType = DmDbType.Date;
#else
                        pr.OleDbType = OleDbType.DBDate;
#endif

                        break;					
					case "DATETIME":
#if DM7
                        pr.DmSqlType = DmDbType.DateTime;
#else
                        pr.OleDbType = OleDbType.DBTimeStamp;
#endif

                        break;					
					case "TIMPSTAMP":
#if DM7
                        pr.DmSqlType = DmDbType.DateTime;
#else
                        pr.OleDbType = OleDbType.DBTimeStamp;
#endif

                        break;
					default:
						ShowSMessage("未知的数据类型，工具将它默认成字符串类型来绑定");
#if DM7
                        pr.DmSqlType = DmDbType.VarChar;
#else
                        pr.OleDbType = OleDbType.VarChar;
#endif

                        break;
				}
			}
			catch(Exception e)
			{
				ShowFMessage("设置参数值时报错，请查看设定的值能否转化为相应的类型:" + e.Message);
			}
			
		}
		public void AddParameter(XmlNode m_xmlNode)
		{
			string sType = "IN";
			string sVal;
			string sDataType = "VarChar";
			int size = 0;
			XmlNode x_temp;
			x_temp = FindXmlNode(m_xmlNode.FirstChild, "TYPE");
			if (x_temp != null) 
			{
				sType = GetNodeText(x_temp).Trim();
				if (sType != "IN" && sType != "OUT" && sType != "IN OUT" && sType != "RETURN TYPE") 
				{
					ShowFMessage("非法的参数类型，参数类型只能为以下四种：(输入参数)IN, (输出参数)OUT, (输入输出参数)IN OUT, (返回值参数)RETURN TYPE");
					return;
				}
			}
			x_temp = FindXmlNode(m_xmlNode.FirstChild, "SIZE");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				try
				{
					size = Convert.ToInt32(sVal);
				}
				catch(Exception e)
				{
					ShowFMessage("非法的参数大小:" + e.Message);
					return;
				}
			}
			x_temp = FindXmlNode(m_xmlNode.FirstChild, "DATATYPE");
			if (x_temp != null) 
			{
				sDataType = GetNodeText(x_temp).Trim();
				
			}
#if DM7
            DmParameter pr = stConnectINfo.cm.CreateParameter();
#else
            OleDbParameter pr = stConnectINfo.cm.CreateParameter();
#endif
            x_temp = FindXmlNode(m_xmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				if (stConnectINfo.rd != null) 
				{
					stConnectINfo.rd.Close();
				}
				
				sVal = GetNodeText(x_temp).Trim();
				if (sVal.StartsWith("@")) {
					pr.ParameterName = sVal.Substring(1);	
					sVal = null;					
				}
				else{
					sVal = ReplaceRunInfo(sVal);
				}			
							
				switch(sType) 
				{
					case "OUT":
						pr.Direction = ParameterDirection.Output;
						if (pr.ParameterName == "") {
							ShowFMessage("在添加非输入参数时，请设置参数名称，以便获取执行以后的返回值");
							return;
						}
						sVal = null;
						break;
					case "IN OUT":
						pr.Direction = ParameterDirection.InputOutput;
						if (pr.ParameterName == "") 
						{
							ShowFMessage("在添加非输入参数时，请设置参数名称，以便获取执行以后的返回值");
							return;
						}
						break;
					case "RETURN TYPE":
						pr.Direction = ParameterDirection.ReturnValue;
						if (pr.ParameterName == "") 
						{
							ShowFMessage("在添加非输入参数时，请设置参数名称，以便获取执行以后的返回值");
							return;
						}
						sVal = null;
						break;
					default:
						pr.Direction = ParameterDirection.Input;
						break;
				}
				pr.Size = size;
				SetParameterDataType(pr, sDataType);
				stConnectINfo.cm.Parameters.Add(pr);
				if (sVal != null) {
					vlc.SetVal(pr.ParameterName, sVal);
				}
			}
			else
			{
				ShowFMessage("参数绑定操作，未找到要绑定的VAL节点");
			}
		}
		public void CompareResultDo(int _id1, int _id2)
		{
			bool showFail = false;
#if DM7
            DmDataReader dr1;
            DmDataReader dr2;
#else
            OleDbDataReader dr1; 
			OleDbDataReader dr2;
#endif
            dr1 = ((CONNECTINFO)stConnectArry[_id1]).rd;
			dr2 = ((CONNECTINFO)stConnectArry[_id2]).rd;
			if (dr1 == null || dr2 == null) {
				ShowFMessage("至少一个连拉上未生成结果集");
				return;
			}
			if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SUCCESS") 
			{
				showFail = true;
			}
			
			if (dr1.FieldCount != dr1.FieldCount) {
				if(showFail) 
				{
					ShowFMessage("两个结果集的列数不一致");
				}
				else if (!m_noShow) 
				{
					ShowSMessage("两个结果集的列数不一致");
				}
				return;
			}
			Object r1;
			Object r2;
			int col = 0;
			int row = 0;
			string binary1;
			string binary2;
			bool ret1 = dr1.Read();
			bool ret2 = dr2.Read();
			while(ret1 && ret2)
			{
				row ++;
				for (col = 0; col < dr1.FieldCount; col ++) {
					r1 = dr1[col];
					r2 = dr2[col];
					if(r1.GetType() == System.Type.GetType("System.Byte[]"))
					{
						if (r1 == r2)
						{
							continue;
						}
						if (r1 == System.DBNull.Value || r2 == System.DBNull.Value) 
						{
							String mes = "第 " + row + " 行 " + (col + 1) + " 列值不一致!有一方为NULL值\n";
							if (showFail) 
							{
								ShowFMessage(mes);							
							}
							else if (!m_noShow)
							{
								ShowSMessage(mes);
							}
							return;
						}
						binary1 = "0X" + ByteArrayToHexString((byte[])r1) + "\n";
						binary2 = "0X" + ByteArrayToHexString((byte[])r2) + "\n";
						if(binary1.CompareTo(binary2) != 0)
						{
							String mes = "第 " + row + " 行 " + (col + 1) + " 列值不一致!\n";
							mes += "连接 " + _id1 + ": ";
							mes += binary1;
							mes += "连接 " + _id2 + ": ";
							mes += binary2;
							if (showFail) 
							{
								ShowFMessage(mes);							
							}
							else if (!m_noShow)
							{
								ShowSMessage(mes);
							}
							return;
						}
					}
					else if (!r1.Equals(r2)) {
						String mes = "第 " + row + " 行 " + (col + 1) + " 列值不一致!\n";
						mes += "连接 " + _id1 + ": ";
						if (r1 == System.DBNull.Value) {
							mes += "NULL\n";
						}
						else
						{
							mes += Convert.ToString(r1) + "\n";
						}
						mes += "连接 " + _id2 + ": ";
						if (r2 == System.DBNull.Value) 
						{
							mes += "NULL";
						}
						else
						{
							mes += Convert.ToString(r2);
						}
						if (showFail) {
							ShowFMessage(mes);							
						}
						else if (!m_noShow){
							ShowSMessage(mes);
						}
						return;
					}
				}
				ret1 = dr1.Read();
				ret2 = dr2.Read();
			}
			if (ret1 != ret2) {
				if(showFail) {
					ShowFMessage("两个结果集的行数不一致");
				}
				else if (!m_noShow) {
					ShowSMessage("两个结果集的行数不一致");
				}
			}
		}
		public void CompareResult(XmlNode m_XmlNode)
		{
			int cn_id1;
			int cn_id2;
			XmlNode x_temp;
			if (ProClass.bValIsShowResult) {
				ShowFMessage("“不检查结果集的情况下显示结果集”功能被打开，无法进行结果集对比操作");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "CONNECTID");
			if (x_temp != null) 
			{
				try
				{
					cn_id1 = Convert.ToInt32(GetNodeText(x_temp).Trim());
				}
				catch(Exception e)
				{
					ShowFMessage("CONNECTID 值非法:" + e.Message);
					return;
				}
			}
			else
			{
				ShowFMessage("非法的结果集比较操作，未在COMPARERESULT节点中找到CONNECTID节点");
				return;
			}
			x_temp = FindXmlNode(x_temp.NextSibling, "CONNECTID");
			if (x_temp != null) 
			{
				try
				{
					cn_id2 = Convert.ToInt32(GetNodeText(x_temp).Trim());
				}
				catch(Exception e)
				{
					ShowFMessage("CONNECTID 值非法:" + e.Message);
					return;
				}
			}
			else
			{
				ShowFMessage("非法的结果集比较操作，未在COMPARERESULT节点中找到第二个CONNECTID节点");
				return;
			}
			if (cn_id1<0 || cn_id1>=stConnectArry.Count) {
				ShowFMessage("CONNECTID 值非法, 应该大于等于 0, 小于 " + stConnectArry.Count + " 之间" );
				return;
			}
			if (cn_id2<0 || cn_id2>=stConnectArry.Count) 
			{
				ShowFMessage("CONNECTID 值非法, 应该大于等于 0, 小于 " + stConnectArry.Count + " 之间" );
				return;
			}
			if (cn_id2 == cn_id1) {
				ShowFMessage("结果集比较操作非法，指定的两个结果集连接ID是相同的");
				return;
			}
			try{
				CompareResultDo(cn_id1, cn_id2);
			}
			catch(Exception e)
			{
				ShowFMessage("结果集比较发生异常：" + e.Message);
			}			
		}
//		AddItem("<DELTEFILE>", "删除一个文件");
//		AddItem("<COPYFILE>", "考一个文件");
//		AddItem("<OLDFILE>", "旧文件");
//		AddItem("<NEWFILE>", "新文件夹");
//		AddItem("<CREATEFILE>", "创建一个文件");
//		AddItem("<FILENAME>", "文件名");
//		AddItem("<WRITEFLAG>", "文件操作标计");//Coverage, Additional
//		AddItem("<WRITEFLAG>", "文件操作标计");//Coverage, Additional
		//SETVAL
		//GETVAL
		//VALNAME
		//SETAT
		//VAL
		public string GetServerPath()
		{
			//修改成发消息到远程服务器 让服务器那边来创建文件
			//8.27 修改 负责通信即可  不需重连
			/*string create_con=ProClass.CommServer.ConnectSev();   //连接到组件
			if(create_con == "connect failed")
				ShowFMessage("连接服务器端测试工具server失败");
			else
				ShowSMessage("连接服务器端测试工具server成功");*/

			//8.27 修改 不需要这步操作  测试工具不需要知道服务器组件端达梦服务器的路径是哪里
			string messserver="SERVERPATH";    //组装成要发送的消息格式
			string ser_ret=ProClass.CommServer.Send_Rec_Message(messserver);   //发送消息 
			ShowSMessage("发送启动服务器命令：" + messserver + "服务器执行结果：" + ser_ret + "\n");
			//ProClass.CommServer.CloseStream();
			return ser_ret;
		}

		public void CreateFile(XmlNode m_XmlNode)
		{
			string name;
			string val = "";
			string flag = "Coverage";
			XmlNode x_temp;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "FILENAME");
			if (x_temp != null) 
			{
				name = GetNodeText(x_temp).Trim();
				name = ReplaceRunInfo(name);
				if (name.Length == 0) 
				{
					ShowFMessage("非法的文件拷贝操作，未在CREATEFILE节点中找到FILENAME节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("非法的文件拷贝操作，未在CREATEFILE节点中找到FILENAME节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				val = GetNodeText(x_temp).Trim();
				val = ReplaceRunInfo(val);
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "WRITEFLAG");
			if (x_temp != null) 
			{
				flag = GetNodeText(x_temp).Trim();
			}
			/*string create_con=ProClass.CommServer.ConnectSev();  
			if(create_con == "connect failed")
				ShowFMessage("连接服务器端测试工具server失败");
			else
				ShowSMessage("连接服务器端测试工具server成功");*/

			string mess3="CREATE_FILE"+'$'+name+'$'+flag+'$'+val;    
			string ret=ProClass.CommServer.Send_Rec_Message(mess3);  
			//ProClass.CommServer.CloseStream();
			ShowSMessage("发送启动服务器命令：" + mess3 + "服务器执行结果：" + ret + "\n");

		}
		public void ReadFile(XmlNode m_XmlNode)
		{
			int file_size;
			XmlNode x_temp;
			string temp_name;
			string path;
			string valname;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "FILENAME");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				temp_name = ReplaceRunInfo(temp_name);
				if (temp_name.Length == 0) 
				{
					ShowFMessage("未指定要读取的文件名，请用FILENAME节点指定");
					return;
				}
			}
			else
			{
				ShowFMessage("未指定FILENAME节点");
				return;
			}
			path = temp_name;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VALNAME");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				temp_name = ReplaceRunInfo(temp_name);
				if (temp_name.StartsWith("@") == false) 
				{
					ShowFMessage("非法的VALNAME节点");
					return;
				}
			}
			else
			{
				ShowFMessage("未指定VALNAME节点");
				return;
			}
			valname = temp_name;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "DATAFLAG");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				temp_name = ReplaceRunInfo(temp_name);
				if (temp_name.Length == 0) 
				{
					ShowFMessage("非法的DATAFLAG节点值");
					return;
				}
			}
			else
			{
				ShowFMessage("未指定DATAFLAG节点");
				return;
			}
			if (string.Compare(temp_name, "Binary",  true) == 0) 
			{
				try
				{
					FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
					file_size = (int)fs.Seek(0, SeekOrigin.End);
					fs.Seek(0, SeekOrigin.Begin);
					byte[] buff = new byte[file_size];
					fs.Read(buff, 0, (int)file_size);
					fs.Close();
					vlc.SetVal(valname.Substring(1), buff);
				}
				catch(Exception e)
				{
					ShowFMessage(e.Message);
				}				
			}
			else
			{
				StreamReader sr = null;
				try
				{
					sr = new StreamReader(path, System.Text.Encoding.Default);
					vlc.SetVal(valname.Substring(1), sr.ReadToEnd());
					sr.Close();
				}
				catch(Exception e)
				{
					ShowFMessage(e.Message);
				}
			}			
			return;
		}
		public void DoDataCompare(XmlNode m_XmlNode)
		{
			string temp_name;
			object binary1;
			object binary2;
			bool is_binary = false;
			XmlNode x_temp;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "DATAFLAG");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				temp_name = ReplaceRunInfo(temp_name);
				if (temp_name.Length == 0) 
				{
					ShowFMessage("非法的DATAFLAG节点值");
					return;
				}
				if(string.Compare(temp_name, "Binary", true) == 0)
				{
					is_binary = true;
				}
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "DATA1");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				if(is_binary)
				{
					if (temp_name.StartsWith("@") == false) 
					{
						ShowFMessage("非法的DATA1节点值，该值应该是一个独立的替代符变量");
						return;
					}
					binary1 = vlc.GetVal(temp_name.Substring(1));
					if(binary1 == null)
					{
						ShowFMessage("非法的DATA1节点值，未找到该替代符变量");
						return;
					}
				}
				else
				{
					temp_name = ReplaceRunInfo(temp_name);
					binary1= temp_name;
				}				
			}
			else
			{
				ShowFMessage("未指定DATA1节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "DATA2");
			if (x_temp != null) 
			{
				temp_name = GetNodeText(x_temp).Trim();
				if(is_binary)
				{
					if (temp_name.StartsWith("@") == false) 
					{
						ShowFMessage("非法的DATA2节点值，该值应该是一个独立的替代符变量");
						return;
					}
					binary2 = vlc.GetVal(temp_name.Substring(1));
					if(binary2 == null)
					{
						ShowFMessage("非法的DATA2节点值，未找到该替代符变量");
						return;
					}
				}
				else
				{
					temp_name = ReplaceRunInfo(temp_name);
					binary2= temp_name;
				}				
			}
			else
			{
				ShowFMessage("未指定DATA2节点");
				return;
			}
			
			if(is_binary)
			{
				if(BinaryCompare(binary1, binary2) == false)
				{
					ShowFMessage("两个二进制串内容不一致");
				}
			}
			else
			{
				try
				{
					if(string.Compare((string)binary1, (string)binary2, false) != 0)
					{
						ShowFMessage("两个字符串内容不一致");
					}
				}
				catch(Exception e)
				{
					ShowFMessage(e.Message);
				}
			}
		}
		public bool BinaryCompare(object binary1, object binary2)
		{
			try
			{
				byte[] b1 = (byte[])binary1;
				byte[] b2 = (byte[])binary2;
				if(b1.Length != b2.Length)
				{
					return false;
				}
				for(int i=0; i<b1.Length; i++)
				{
					if(b1[i] != b2[i])
					{
						return false;
					}
				}
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message);
				return false;
			}
			return true;
		}
		public void DeleFile(string sFileName)  //已修改通过
		{
			try
			{
				sFileName = ReplaceRunInfo(sFileName);
				//8.27 修改 负责通信即可  不需重连
				/*string create_con=ProClass.CommServer.ConnectSev();   //连接到组件
				if(create_con == "connect failed")
					ShowFMessage("连接服务器端测试工具server失败");
				else
					ShowSMessage("连接服务器端测试工具server成功");*/

				string mess3="DELETE_FILE"+'$'+sFileName;   //delete file
				string ret=ProClass.CommServer.Send_Rec_Message(mess3);   //发送消息  重启服务器
				//ProClass.CommServer.CloseStream();
                ShowSMessage("发送启动服务器命令：" + mess3 + "服务器执行结果：" + ret + "\n");
			}
			catch(Exception e)
			{
				if (stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_IGNORE") 
				{
					ShowSMessage(e.Message);
				}
				else
				{
					ShowFMessage(e.Message);
				}				
			}
		}


		public void CopyFile(XmlNode m_XmlNode)
		{
			string sOldFile = "";
			string sNewFile = "";
			XmlNode x_temp;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "OLDFILE");
			if (x_temp != null) 
			{
				sOldFile = GetNodeText(x_temp).Trim();
				sOldFile = ReplaceRunInfo(sOldFile);
				if (sOldFile.Length == 0) 
				{
					ShowFMessage("非法的文件拷贝操作，未在COPYFILE节点中找到OLDFILE节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("非法的文件拷贝操作，未在COPYFILE节点中找到OLDFILE节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "NEWFILE");
			if (x_temp != null) 
			{
				sNewFile = GetNodeText(x_temp).Trim();
				sNewFile = ReplaceRunInfo(sNewFile);
				if (sNewFile.Length == 0) 
				{
					ShowFMessage("非法的文件拷贝操作，未在COPYFILE节点中找到NEWFILE节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("非法的文件拷贝操作，未在COPYFILE节点中找到NEWFILE节点");
				return;
			}
			try
			{
				//8.27 修改 负责通信即可  不需重连
				/*string create_con=ProClass.CommServer.ConnectSev();   //连接到组件
				if(create_con == "connect failed")
					ShowFMessage("连接服务器端测试工具server失败");
				else
					ShowSMessage("连接服务器端测试工具server成功");*/

				string mess4="COPY_FILE"+'$'+sOldFile+'$'+sNewFile;   //copy file
				string ret=ProClass.CommServer.Send_Rec_Message(mess4);   //发送消息  重启服务器
				//ProClass.CommServer.CloseStream();
				
				/*if(ret == "fail")
					ShowFMessage("复制文件失败!");
				else*/
					ShowSMessage("发送启动服务器命令：" + mess4 + "服务器执行结果：" + ret + "\n");
			}
			catch(Exception e)
			{
				if (stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_IGNORE") {
					ShowSMessage(e.Message);
				}
				else{
					ShowFMessage(e.Message);
				}				
			}
			
		}
		public void ModifyIni(XmlNode m_XmlNode)  //通过 组件端待处理
		{
			string sName;
			string sVal;
			string sPath;
			XmlNode x_temp;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VALNAME");
			if (x_temp != null) 
			{
				sName = GetNodeText(x_temp).Trim();
				if (sName.Length == 0) 
				{
					ShowFMessage("未在SETDMINI节点中找到VALNAME节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("未在SETDMINI节点中找到VALNAME节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				if (sVal.Length == 0) 
				{
					ShowFMessage("未在SETDMINI节点中找到VAL节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("未在SETDMINI节点中找到VAL节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "INIPATH");
			if (x_temp != null)      //如果指定了路径  修改后脚本必须指定路径！！！！！！
			{
				sPath = GetNodeText(x_temp).Trim();
				sPath = ReplaceRunInfo(sPath);
				if (sPath.Length == 0) 
				{
					ShowFMessage("未在SETDMINI节点中找到INIPATH节点的属性名");
					return;
				}
			}
			else  //若没有指定路径
			{
				sPath = ProClass.cDServer.GetIniPath();

			}

			/*string create_con=ProClass.CommServer.ConnectSev();   //连接到组件
			if(create_con == "connect failed")
				ShowFMessage("连接服务器端测试工具server失败");
			else
				ShowSMessage("连接服务器端测试工具server成功");*/

			string messm="MODIFY_INIFILE"+'$'+sPath+'$'+sName+'$'+sVal;    //组装成要发送的消息格式
			string ret=ProClass.CommServer.Send_Rec_Message(messm);   //发送消息  
			//ProClass.CommServer.CloseStream();
			/*
			if(ret == "NOT_FOUND" || ret == null)
			{
				ShowFMessage("dm.ini中没有找到参数"+sName+"!");
				return;
			}

			if(ret == "fail")
				ShowFMessage(sVal + "参数值设置失败!");
			else
				ShowSMessage(messm + ret);*/
			ShowSMessage("发送启动服务器命令：" + messm + "服务器执行结果：" + ret + "\n");
		}
		public void GetIni(XmlNode m_XmlNode)   //xiugai zhong
		{
			string sName;
			string sVal;
			string sPath;
			XmlNode x_temp;

			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VALNAME");
			if (x_temp != null) 
			{
				sName = GetNodeText(x_temp).Trim();
				if (sName.Length == 0) 
				{
					ShowFMessage("未在GETDMINI节点中找到VALNAME节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("未在GETDMINI节点中找到VALNAME节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				if (!sVal.StartsWith("@")) 
				{
					ShowFMessage("VAL节点中应该指明的是被存放属性值的一个替代符，而不是一个普通的字符串");
					return;
				}
			}
			else
			{
				ShowFMessage("未在GETDMINI节点中找到VAL节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "INIPATH");
			if (x_temp != null)  //同上 必须指定路径！！！！！
			{
				sPath = GetNodeText(x_temp).Trim();
				sPath = ReplaceRunInfo(sPath);
				if (sPath.Length == 0) 
				{
					ShowFMessage("未在GETDMINI节点中找到INIPATH节点的属性名");  
					return;
				}
			}
			else
			{
				sPath = ProClass.cDServer.GetIniPath();
			}


			//这里有待改进
//			ret = DoFileClass.GetVal(sPath, sName);   //修改了GetVal   用该值去设置@SQLSTR 如何设置？？@SQLSTR为整型
				ProClass.sValGetDmIni="0";

			/*string create_con=ProClass.CommServer.ConnectSev();
			if(create_con == "connect failed")
				ShowFMessage("连接服务器端测试工具server失败");
			else
				ShowSMessage("连接服务器端测试工具server成功");*/

			string messr="READ_INIFILE"+'$'+sPath+'$'+sName;   
			string ret=ProClass.CommServer.Send_Rec_Message(messr);   
			//ProClass.CommServer.CloseStream();

            /*if(ret == "NOT_FOUND" || ret == null)
				ShowFMessage("dm.ini中没有找到参数"+sName+"!");
			
			if(ret == "fail")
				ShowFMessage(sVal + "参数值设置失败!");*/
			ShowSMessage("发送启动服务器命令：" + messr + "服务器执行结果：" + ret + "\n");
			vlc.SetVal(sVal.Substring(1), ret);
		}
		public void GetVal(XmlNode m_XmlNode)
		{
			string sName;
			string sVal;
			XmlNode x_temp;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VALNAME");
			if (x_temp != null) {
				sName = GetNodeText(x_temp).Trim();
				if (sName.Length == 0) {
					ShowFMessage("未在GETVAL节点中找到VALNAME节点的属性名");
					return;
				}
			}
			else{
				ShowFMessage("未在GETVAL节点中找到VALNAME节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				if (!sVal.StartsWith("@")) 
				{
					ShowFMessage("VAL节点中应该指明的是被存放属性值的一个替代符，而不是一个普通的字符串");
					return;
				}
				string sTemp = ProClass.GetSaveProValue(sName);
				if (sTemp == null) {
					ShowFMessage("未找到属性：" + sName);
				}
				else
					vlc.SetVal(sVal.Substring(1), sTemp);							
			}
			else
			{
				ShowFMessage("未在GETVAL节点中找到SETAT节点");
			}
		}

		public void SetVal(XmlNode m_XmlNode)
		{
			string sName;
			string sVal;
			XmlNode x_temp;
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VALNAME");
			if (x_temp != null) 
			{
				sName = GetNodeText(x_temp).Trim();
				if (sName.Length == 0) 
				{
					ShowFMessage("未在SETVAL节点中找到VALNAME节点的属性名");
					return;
				}
			}
			else
			{
				ShowFMessage("未在SETVAL节点中找到VALNAME节点");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				sVal = ReplaceRunInfo(sVal);
				ProClass.AddSaveProValue(sName, sVal);
			}
			else
			{
				ShowFMessage("未在SETVAL节点中找到VAL节点");
			}
		}
		//该函数用来结束事务进程
		public void CloseTransPro()
		{			
			for(int i=0; i<alTransPro.Count; i++)
			{
				TRANS_STRUCT tTransStruce = (TRANS_STRUCT)alTransPro[i];
				if(!tTransStruce.cProcess.HasExited)
				{
					SendXmlStringToTrans(i, "Exit");
				}
			}
			if (alTransPro.Count>0) {
				Thread.Sleep(alTransPro.Count*200);	
			}
			
			for(int i=0; i<alTransPro.Count; i++)
			{
				TRANS_STRUCT tTransStruce = (TRANS_STRUCT)alTransPro[i];
				tTransStruce.cProcess.Refresh();
				if(!tTransStruce.cProcess.HasExited)
				{
					ShowSMessage("事务进程ID：" + i + "，未能顺利的结束，程序将通过发送关闭消息来结束它");
					try
					{
						tTransStruce.cProcess.CloseMainWindow();
						if(!tTransStruce.cProcess.HasExited)
						{
							tTransStruce.cProcess.Kill();
						}
					}
					catch(Exception e)
					{
						ShowSMessage(e.Message);
					}
					Thread.Sleep(100);
				}
			}

			alTransPro.Clear();
		}
		//该函数用来向指定的事务进程发送执行的脚本
		public void SendXmlStringToTrans(int iID, string sXml)
		{
			if(iID < 0 || iID >= alTransPro.Count)
			{
				ShowFMessage("指定的事务号超范围，正确的事务号范围应该在 0 - " + alTransPro.Count + " 之间；而当前要指定的事务号为：" + iID);
				return;
			}
			if(stXmlRunInfo.xCurrentNode == null)
			{
				ShowFMessage("当前的XML节点是空值");
				return;
			}
			TRANS_STRUCT tTransStruce = (TRANS_STRUCT)alTransPro[iID];
			tCurrentTransStruct = tTransStruce;
			Process cProcess = tTransStruce.cProcess;
			if(cProcess.HasExited)
			{
				ShowFMessage("事务进程已经终止了，不能再向它发送脚本命令");
				return;
			}
			if(sXml == null || sXml == "")//如果未指定发送的内容，那么发送当前节点的角本
				sXml = stXmlRunInfo.xCurrentNode.OuterXml;

			int i=0;
			while(tTransStruce.bIsFinishExcute == false&&i++<200)//睡眠一秒，确保上一次发送的脚本有充分的时间被处理
			{
				Thread.Sleep(10);	
			}
			if(tTransStruce.bIsFinishExcute)
			{
				tTransStruce.bIsFinishExcute = false;
				cProcess.StandardInput.WriteLine(sXml.Replace("\r", " ").Replace("\n", " "));	//向脚本进程写入执行脚本
				i=0;
				while(tTransStruce.bIsFinishExcute == false&&i++<300)//睡眠一秒，确保上一次发送的脚本有充分的时间被处理
				{
					Thread.Sleep(10);	
				}
				i++;				
			}
			else
			{				
				ShowFMessage("事务ID号：" + iID + "， 上一次向该事务发送的脚本还没有得到执行成功的回应\n, 如果是因脚本确实需要超过10秒的时间执行，那么请在发送脚本的主脚本中增加睡眠时间！");
				cProcess.CloseMainWindow();
			}
		}
		public bool CheckEffectRows(int iExcRows)
		{
			
			if(stConnectINfo == null || stConnectINfo.rd == null)
			{
				ShowFMessage("上一次执行的语句没有生成记录对像");
				return false;
			}
			if(stConnectINfo.rd.FieldCount == 0)
			{
				if(iExcRows != stConnectINfo.rd.RecordsAffected)
				{
					ShowFMessage("语句执行影响的行数预期的值不一致！\n" + "预期值为:" + iExcRows + "\n实际返回值：" + stConnectINfo.rd.RecordsAffected);
					return false;
				}
			}
			else
			{
				if(stConnectINfo.rd.HasRows)
				{
					if(iExcRows == 0)
					{
						ShowFMessage("要求返回一个空结果集，实际上返回的结果集不为空");
						return false;
					}
				}
				else
				{
					if(iExcRows != 0)
					{
						ShowFMessage("要求返回一个非空结果集，实际上返回的结果集为空");
						return false;
					}
				}
			}
			return true;
		}
		public bool CheckResultRows(int iExcRows)
		{
			
			if(stConnectINfo == null || stConnectINfo.rd == null)
			{
				ShowFMessage("上一次执行的语句没有生成记录对像");
				return false;
			}
			if(stConnectINfo.rd.FieldCount == 0)
			{
				if(iExcRows != stConnectINfo.rd.RecordsAffected)
				{
					ShowFMessage("语句执行影响的行数预期的值不一致！\n" + "预期值为:" + iExcRows + "\n实际返回值：" + stConnectINfo.rd.RecordsAffected);
					return false;
				}
			}
			else
			{
				int rows = 0;
				if(stConnectINfo.rd.HasRows)
				{
					if (ProClass.bValIsShowResult) 
					{
						ShowFMessage("“不检查结果集的情况下显示结果集”功能被打开，无法进行结果集行数统计操作");
						return false;
					}
					while(stConnectINfo.rd.Read())
					{
						rows++;
					}
				}
				if(rows != iExcRows)
				{
					ShowFMessage("结果集行数跟预期的行数不一致！\n" + "预期值为:" + iExcRows + "\n实际返回值：" + rows);
					return false;
				}
			}
			return true;
		}
		//开始当前连接上的事务
		public void StartTrans(string sOp)
		{
			ShowSMessage("BEGINTRANS:" + sOp);
			try
			{
				if(stConnectINfo == null)
				{
					if(!Connection())
					{
						return;
					}
				}
				else if(stConnectINfo.cn.State != ConnectionState.Open)
				{
					if(!ReConnect())
					{
						return;
					}
				}
				if(stConnectINfo.rd != null)
					stConnectINfo.rd.Close();
#if DM7
                if (true)
#else
                if(stConnectINfo.tr == null)
#endif
                {
					if(sOp == null || sOp == "")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction();
					else if(sOp == "Chaos")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction(IsolationLevel.Chaos);
					else if(sOp == "ReadCommitted")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction(IsolationLevel.ReadCommitted);
					else if(sOp == "ReadUncommitted")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction(IsolationLevel.ReadUncommitted);
					else if(sOp == "RepeatableRead")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction(IsolationLevel.RepeatableRead);
					else if(sOp == "Serializable")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction(IsolationLevel.Serializable);
					else if(sOp == "Unspecified")
						stConnectINfo.tr = stConnectINfo.cn.BeginTransaction(IsolationLevel.Unspecified);
					else
					{
						ShowFMessage("在指定事务的隔离级别使，使用了ADO未定义的级别：" + sOp);
						return;
					}
				}
#if DM7
                else
                {
                    ;
                }
#else
				else
				{
					if(sOp == null || sOp == "")
						stConnectINfo.tr = stConnectINfo.tr.Begin();
					else if(sOp == "Chaos")
						stConnectINfo.tr.Begin(IsolationLevel.Chaos);
					else if(sOp == "ReadCommitted")
						stConnectINfo.tr.Begin(IsolationLevel.ReadCommitted);
					else if(sOp == "ReadUncommitted")
						stConnectINfo.tr.Begin(IsolationLevel.ReadUncommitted);
					else if(sOp == "RepeatableRead")
						stConnectINfo.tr.Begin(IsolationLevel.RepeatableRead);
					else if(sOp == "Serializable")
						stConnectINfo.tr.Begin(IsolationLevel.Serializable);
					else if(sOp == "Unspecified")
						stConnectINfo.tr.Begin(IsolationLevel.Unspecified);
					else
					{
						ShowFMessage("在指定事务的隔离级别使，使用了ADO未定义的级别：" + sOp);
						return;
					}
				}
#endif
				
				stConnectINfo.alTrArray.Add(stConnectINfo.tr);
				stConnectINfo.cm.Transaction = stConnectINfo.tr;			
			}
			catch(Exception e)
			{
				ShowFMessage("在事务启动时，连接发生了异常！\n" + e.Message);
			}
		}
		//结束当前连接上的事务
		public void EndTrans(string sOp)
		{
			if (stConnectINfo == null) {
				ShowFMessage("结束事务上没有连接");
				return;
			}
			ShowSMessage("ENDTRANS:" + sOp);
			if(stConnectINfo.alTrArray.Count == 0)
			{
				ShowFMessage("事务对像队列中不存在事务对像，在当前连接上你还没有开始事务或是已经关闭了所有事务");
				return;
			}
#if DM7
            stConnectINfo.tr = (DmTransaction)stConnectINfo.alTrArray[stConnectINfo.alTrArray.Count - 1];
#else
            stConnectINfo.tr = (OleDbTransaction)stConnectINfo.alTrArray[stConnectINfo.alTrArray.Count - 1];
#endif
            stConnectINfo.alTrArray.Remove(stConnectINfo.tr);
			if(stConnectINfo.tr == null)
			{
				ShowFMessage("从事务对像队列中得到的事务对像是空值");
				return;
			}
			if(stConnectINfo.rd != null)
				stConnectINfo.rd.Close();
			try
			{
				if(sOp==null || sOp=="" || String.Compare(sOp, "ROLLBACK", true) == 0)
				{
					stConnectINfo.tr.Rollback();
				}
				else
					stConnectINfo.tr.Commit();	
				if(stConnectINfo.tr.Connection == null)
				{
					stConnectINfo.tr = null;
					stConnectINfo.cm.Transaction = null;
				}
			}
			catch(Exception e)
			{				
				ShowFMessage("在事务结束时，连接发生了异常！\n" + e.Message);
				stConnectINfo.tr = null;
				stConnectINfo.cm.Transaction = null;
			}
			if(stConnectINfo.alTrArray.Count > 0)
			{
#if DM7
                stConnectINfo.tr = (DmTransaction)stConnectINfo.alTrArray[stConnectINfo.alTrArray.Count - 1];
#else
                stConnectINfo.tr = (OleDbTransaction)stConnectINfo.alTrArray[stConnectINfo.alTrArray.Count - 1];
#endif
                stConnectINfo.cm.Transaction = stConnectINfo.tr;
			}
		}

		//创建新的线程执行函数，该函数是用来读出对应的事务进程中的输入流内容
		public void ReadTransProOutput()
		{          
			TRANS_STRUCT tTransStruce = tCurrentTransStruct;
			tCurrentTransStruct = null;
			Process cProcess = tTransStruce.cProcess;
			while(cProcess.HasExited == false)
			{
				string sOutString = cProcess.StandardOutput.ReadLine();
				if(sOutString == "OK")
				{
					tTransStruce.bIsFinishExcute = true;
                 //   ShowSMessage("调用DisposeTrans.exe执行成功!");

                }
				else if(sOutString != null)
				{
                    if (sOutString.StartsWith("0"))
                    {
                        ShowFMessage(sOutString.Substring(1));
                    }
                    else
                    {
                        ShowSMessage(sOutString);
                      //  ShowSMessage(Convert.ToString(loop++));
                    }

				}
			}
		}

		//用来创建一个新的进程，该进程是用来执行事务脚本的
		public void StartNewTrans()
		{
			tCurrentTransStruct = new TRANS_STRUCT();
			tCurrentTransStruct.cProcess = new Process();
			tCurrentTransStruct.bIsFinishExcute = true;
			tCurrentTransStruct.cProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			
			tCurrentTransStruct.cProcess.StartInfo.FileName = tCurrentTransStruct.cProcess.StartInfo.WorkingDirectory + "\\DisposeTrans.exe";
			tCurrentTransStruct.cProcess.StartInfo.RedirectStandardInput = true;
			tCurrentTransStruct.cProcess.StartInfo.RedirectStandardOutput = true;
			tCurrentTransStruct.cProcess.StartInfo.RedirectStandardError = true;
			tCurrentTransStruct.cProcess.StartInfo.UseShellExecute = false;
			tCurrentTransStruct.cProcess.StartInfo.CreateNoWindow = true;
			tCurrentTransStruct.cProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			tCurrentTransStruct.cProcess.StartInfo.Arguments = "\"";
			tCurrentTransStruct.cProcess.StartInfo.Arguments += sProvider + "\" \"" + sServerName + "\" \"" + sUid
				+ "\" \"" + sPwd + "\" \"" + sDatabase + "\" \"" + sPort + "\" \"进程:" + alTransPro.Count; 
			tCurrentTransStruct.cProcess.StartInfo.Arguments += "\" \"" + stXmlRunInfo.sXmlFileName + "\""; 
			
			try
			{
				tCurrentTransStruct.cProcess.Start();
				tCurrentTransStruct.cProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				
				ShowSMessage("新创建了一个事务，请记住它的ID号为：" + alTransPro.Count);
				alTransPro.Add(tCurrentTransStruct);
				Thread cThread = new Thread(new ThreadStart(ReadTransProOutput));//创建一个线程委托来运行测试函数

				cThread.Start();
				//当读取当前创建的事务进程的线程运行起来以后，会把tCurrentTransStruct参数置为空
				while(tCurrentTransStruct != null)//等待这个对应的线程读取当前的结构后正式运行起来
				{
					Thread.Sleep(0);
				}
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message+":在应用程序当前目录下找不到DisposeTrans.exe文件");
			}
		}
		/// <summary>
		/// //执行指定的EXE文件
		/// </summary>
		public void ExecuteProcessEx(string m_PrName)
		{
			string m_tempStr = "";
			string par = ""; //调用exe的参数
			int pot;
			m_PrName = m_PrName.Trim();
			ShowSMessage("开始调用可执行文件：" + m_PrName);
			pot=m_PrName.IndexOf(" ");
			if(pot>0) 
			{
				par=m_PrName.Substring(pot+1,m_PrName.Length-pot-1); //程序名与参数分离
				m_PrName=m_PrName.Substring(0,pot);
				par = par.Trim();
			}
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("当前文件路径非法，未调用执行文件：" + m_PrName);
				return;
			}
			else
			{
				if(m_PrName.StartsWith("\\"))
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex);
				else
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex+1);
			}
			
			Process m_Process = new Process();
			;
			m_Process.StartInfo.FileName = m_tempStr + m_PrName;
			m_tempStr += m_PrName.Substring(0, m_PrName.LastIndexOf("\\") + 1);
			m_Process.StartInfo.WorkingDirectory = m_tempStr;			
			m_Process.StartInfo.RedirectStandardOutput = true;
			m_Process.StartInfo.RedirectStandardInput = true;
			m_Process.StartInfo.RedirectStandardError = true;
			m_Process.StartInfo.UseShellExecute = false;
			m_Process.StartInfo.CreateNoWindow = true;
			m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			if(par.Length > 0)
			{
				m_Process.StartInfo.Arguments = ReplaceRunInfo(par);
			}
			else
			{				
				m_Process.StartInfo.Arguments = sServerName + " " + sUid + " " + sPwd + " " + sDatabase  + " " + sPort; 
			}
			try
			{
				m_Process.Start();
				m_Process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				
				while(m_Process.HasExited == false)
				{
					string sOutString = m_Process.StandardOutput.ReadLine();
					if(sOutString != null)
					{
						if(sOutString.StartsWith("0"))
						{
							ShowFMessage(sOutString.Substring(1));
						}
						else
							ShowSMessage(sOutString);
					}
				}
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message + m_tempStr);
				m_Process.Close();
				return;
			}
			m_Process.WaitForExit();
			ShowSMessage("结束调用可执行文件：" + m_PrName);
			m_Process.Close();
			m_Process.Dispose();
			m_Process = null;
		}
		/// <summary>
		/// //执行指定的EXE文件
		/// </summary>
		public void ExecuteProcess(string m_PrName)
		{
/*			ShowSMessage("开始调用可执行文件：" + m_PrName);
			string m_tempStr = "";
			string par = ""; //调用exe的参数
			int pot;
			m_PrName = m_PrName.Trim();
			pot=m_PrName.IndexOf(" ");
			if(pot>0) 
			{
				par=m_PrName.Substring(pot+1,m_PrName.Length-pot-1); //程序名与参数分离
				m_PrName=m_PrName.Substring(0,pot);
				par = par.Trim();
			}
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("当前文件路径非法，未调用执行文件：" + m_PrName);
				return;
			}
			else
			{
				if(m_PrName.StartsWith("\\"))
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex);
				else
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex+1);
			}
			
			Process m_Process = new Process();
			m_Process.StartInfo.FileName = m_tempStr + m_PrName;

			m_tempStr += m_PrName.Substring(0, m_PrName.IndexOf("\\") + 1);
			m_Process.StartInfo.WorkingDirectory = m_tempStr;			
			
			if(par.Length > 0)
			{
				m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
				m_Process.StartInfo.Arguments = ReplaceRunInfo(par);
			}
			else
			{
				m_Process.StartInfo.RedirectStandardOutput = true;
				m_Process.StartInfo.RedirectStandardError = true;
				m_Process.StartInfo.RedirectStandardInput = true;
				m_Process.StartInfo.UseShellExecute = false;
				m_Process.StartInfo.CreateNoWindow = true;
				m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				m_Process.StartInfo.Arguments = sServerName + " " + sUid + " " + sPwd + " " + sDatabase  + " " + sPort; 
			}
			try
			{				
				m_Process.Start();
				m_Process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				m_PrName ="[" + m_PrName + "] ";
				while(m_Process.HasExited == false)
				{
					string sOutString = m_Process.StandardOutput.ReadLine();
					if(sOutString != null)
					{
						ShowSMessage(m_PrName + sOutString);
					}
				}
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message + m_tempStr);
				m_Process.Close();
				return;
			}
*/
			string m_tempStr = "";
			string par = ""; //调用exe的参数
			int pot;
			m_PrName = m_PrName.Trim();
			ShowSMessage("开始调用可执行文件：" + m_PrName);
			pot=m_PrName.IndexOf(" ");
			if(pot>0) 
			{
				par=m_PrName.Substring(pot+1,m_PrName.Length-pot-1); //程序名与参数分离
				m_PrName=m_PrName.Substring(0,pot);
				par = par.Trim();
			}
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("当前文件路径非法，未调用执行文件：" + m_PrName);
				return;
			}
			else
			{
				if(m_PrName.StartsWith("\\"))
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex);
				else
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex+1);
			}
			
			Process m_Process = new Process();
			m_Process.StartInfo.FileName = m_tempStr + m_PrName;
			m_tempStr += m_PrName.Substring(0, m_PrName.LastIndexOf("\\") + 1);
			m_Process.StartInfo.WorkingDirectory = m_tempStr;			
			m_Process.StartInfo.RedirectStandardOutput = true;
			m_Process.StartInfo.RedirectStandardInput = true;
			m_Process.StartInfo.RedirectStandardError = true;
			m_Process.StartInfo.UseShellExecute = false;
			m_Process.StartInfo.CreateNoWindow = true;
			m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			if(par.Length > 0)
			{
				m_Process.StartInfo.Arguments = ReplaceRunInfo(par);
			}
			else
			{				
				m_Process.StartInfo.Arguments = sServerName + " " + sUid + " " + sPwd + " " + sDatabase  + " " + sPort; 
			}
			try
			{
				m_Process.Start();
				m_Process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				
				while(m_Process.HasExited == false)
				{
					string sOutString = m_Process.StandardOutput.ReadLine();
					if(sOutString != null)
					{
							ShowSMessage(sOutString);
					}
				}
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message + m_tempStr);
				m_Process.Close();
				return;
			}

			m_Process.WaitForExit();
			//对预期结果进行处理
			if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_FAIL")
			{
                //	if(m_Process.ExitCode == 1)
                if (m_Process.ExitCode == 0)                    //修改2016-10-31  闻江业
                {
					ShowFMessage("可执行文件：" + m_PrName + " 返回执行成功");
				}
				else
				{
					ShowSMessage("可执行文件：" + m_PrName + " 执行结果失败");
				}
			} 
			if (stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SUCCESS")
			{
                //		if(m_Process.ExitCode == 1)
                if (m_Process.ExitCode == 0)              //修改2016-10-31  闻江业
                {
					ShowSMessage("可执行文件：" + m_PrName + " 返回执行成功");
				}
				else
				{
					ShowFMessage("可执行文件：" + m_PrName + " 执行结果失败");
				}
			}
			ShowSMessage("结束调用可执行文件：" + m_PrName);
			m_Process.Close();
			m_Process = null;
		}
		/// <summary>
		/// //新建一个连接，用来执行指定结点内的内容
		/// </summary>
		public void ExecuteNode()
		{
			XmlTest m_xmlTest = new XmlTest(stXmlRunInfo.tdTestThread );
			m_xmlTest.SetFirstNode(stXmlRunInfo.xCurrentNode);
			m_xmlTest.SetXmlFile(stXmlRunInfo.sXmlFileName);

			bool bSuccess  = m_xmlTest.RunInAnNode();
			if(bSuccess == false)
			{
				stXmlRunInfo.cCurrentSqlCase.stCaseResult.bSuccess = bSuccess;
			}
			m_xmlTest = null;
		}
		public int GetUpTimes(int up)
		{
			if (up < 0) 
			{
				return -1;
			}
			LOOP_STRUCT cTemp = cLoop;
			while (up-->0 && cTemp != null) 
			{
				cTemp = (LOOP_STRUCT)cTemp.prev;
			}
			if (cTemp == null) 
			{
				return -1;
			}
			return cTemp.times;
		}
		public string ReplaceRunInfo(string sql)
		{	
			if(cLoop != null)
			{
			//	sql = sql.Replace("@TIMES", cLoop.times.ToString());
				int index = sql.IndexOf("@");
				while (index != -1) 
				{
					String sTemp = sql.Substring(index);
					int iTemp1 = sTemp.IndexOf("TIMES");
					int iTemp2 = GetUpTimes(iTemp1-1);
					if (iTemp2 != -1) 
					{
						sTemp = sTemp.Substring(0, iTemp1 + "TIMES".Length);
						sql = sql.Replace(sTemp, iTemp2.ToString());
					}
					index = sql.IndexOf("@", index + 1);
				}
			}
			return vlc.ReplaceRunInfo(sql);
		}
		//保存执行以后的输出参数值
		private void SaveParameterValue()
		{
			for (int i=0; i<stConnectINfo.cm.Parameters.Count; i++) {
				if (stConnectINfo.cm.Parameters[i].Direction != ParameterDirection.Input) {
					try
					{
						if (stConnectINfo.cm.Parameters[i].Value == null) {
							ShowFMessage("保存输出参数值" + stConnectINfo.cm.Parameters[i].ParameterName + "出错：没有发现输出值");
						}
						else if(stConnectINfo.cm.Parameters[i].Value == System.DBNull.Value)
						{
							vlc.SetVal(stConnectINfo.cm.Parameters[i].ParameterName, "NULL");
						}
						else if(stConnectINfo.cm.Parameters[i].Value.GetType() == System.Type.GetType("System.Byte[]"))
						{		
							vlc.SetVal(stConnectINfo.cm.Parameters[i].ParameterName, "0X" + ByteArrayToHexString((byte[])stConnectINfo.cm.Parameters[i].Value));
						}
						else
						{
							vlc.SetVal(stConnectINfo.cm.Parameters[i].ParameterName, Convert.ToString(stConnectINfo.cm.Parameters[i].Value));
						}						
					}
					catch(Exception e)
					{
						ShowFMessage("保存输出参数值" + stConnectINfo.cm.Parameters[i].ParameterName + "出错：" + e.Message);
					}
				}
			}
		}
		public bool CheckParameterEnableResult()
		{
			bool ret = true;
			for (int i=0; i<stConnectINfo.cm.Parameters.Count; i++) {
				if (stConnectINfo.cm.Parameters[i].Direction != ParameterDirection.Input) 
				{
					ret = false;
				}
				SetParameterValue(stConnectINfo.cm.Parameters[i]);
			}
			return ret;			
		}
		/// <summary>
		/// //用来执行SQL语句
		/// </summary>
		public bool ExecuteSQL(string m_Sql)
		{	
			vlc.SetVal("SQLSTATE", "");
			vlc.SetVal("RETCODE", "0");
			vlc.SetVal("EFFECTROWS", "0");
			m_Sql = ReplaceRunInfo(m_Sql);
			
			if(stConnectINfo == null)
			{
				if(!Connection())
				{
					return false;
				}
			}
			else if(stConnectINfo.cn.State != ConnectionState.Open)
			{
				if(!ReConnect())
				{
					return false;
				}
			}
			if(stConnectINfo.rd != null)
				stConnectINfo.rd.Close();
			stConnectINfo.cm.CommandText = m_Sql;
			stConnectINfo.cm.CommandTimeout = 0;
			try
			{
				TimeSpan ts;
				DateTime time1;
				DateTime time2;
				m_Sql = m_Sql.Trim();
									
				stConnectINfo.rd = null;
				stXmlRunInfo.tdTestThread .PushSqlCase(m_Sql);	//把SQL语句压入链表
				if (!m_noShow) 
				{
					ShowSMessage(m_Sql);
				}
				stConnectINfo.isOpenResult = false;				
				if (CheckParameterEnableResult()) {
					time1 = new DateTime(DateTime.Now.Ticks);//为执行语句计时
					stConnectINfo.rd = stConnectINfo.cm.ExecuteReader();
					vlc.SetVal("EFFECTROWS", stConnectINfo.rd.RecordsAffected.ToString());
				}
				else{
					time1 = new DateTime(DateTime.Now.Ticks);//为执行语句计时
					stConnectINfo.cm.ExecuteNonQuery();
				}				
				time2 = new DateTime(DateTime.Now.Ticks);
				ts = new TimeSpan(time2.Ticks-time1.Ticks) ;//取出执行消耗的时间
				usedtimes =ts.Hours*((Int64)60000*60) + ((Int64)ts.Minutes)*60000+ts.Seconds*1000+ts.Milliseconds;
				vlc.SetVal("USEDTIMES", usedtimes.ToString());
				SaveParameterValue();
				if(ProClass.bValIsOutTime && !m_noShow)
				{						
					ShowSMessage("执行本语句消耗了" + Convert.ToString(usedtimes) + "毫秒。");//显示时间间隔
				}				
			}
#if DM7
            catch (DmException e)
#else
			catch (OleDbException e)
#endif
            {
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult != "DIRECT_EXECUTE_FAIL")
					stXmlRunInfo.tdTestThread .SaveSqlCase();				//保存成执行过的SQL语句现场				
				
				return CheckExecute(false, m_Sql, e);
			}
			catch(Exception e)
			{
				ShowFMessage("语句执行过程中发生异常！\n" + e.Message);
				//-----test	
				sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
				if(sqlCounter.sqlLineNum > 0)
				    ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消
				//-----test
				return false;
			}
			return CheckExecute(true, m_Sql, null);
		}
		/// <summary>
		/// //返回一列的值
		/// </summary>
		public string GetColumn(XmlNode m_XmlNode)
		{
			Debug.Assert(m_XmlNode != null, "给定的列结点为空值", "XmlTest.GetColumn 函数");
			m_XmlNode = SkipComment(m_XmlNode);
			if(m_XmlNode == null)
			{
				return null;
			}
			if(m_XmlNode.Name != "COLUMN")
			{
				ShowFMessage("给定结点的名称不是代表结果集中列的结点，该结点名称为:" + m_XmlNode.Name);
				return null;
			}
			String sValues = GetSqlExpVal(m_XmlNode);
			if (sValues == null) 
			{
				sValues = GetNodeText(m_XmlNode);	
			}
			return sValues;
		}
		/// <summary>
		/// //检察结果集的每一行
		/// </summary>
		public bool CheckResultRow(XmlNode m_XmlNode)
		{
			m_XmlNode = SkipComment(m_XmlNode);
			if(m_XmlNode == null)
			{
				if(stConnectINfo.rd.HasRows)
				{
					ShowFMessage("预期语句返回一个空的结果集，但是实际返回了一个非空结果集");
					return false;
				}
				else
				{
					return true;
				}
			}
			if(m_XmlNode.Name != "RECORD")
			{
				ShowFMessage("结果集起始行的名称不是代表结果集中行的结点，该结点名称为:" + m_XmlNode.Name);
				return false;
			}
			XmlNode m_colNum = FindXmlNode(m_XmlNode.FirstChild, "COLUMNNUMS");
			int m_colNums = 0;
			if (m_colNum != null) 
			{
				try
				{
					String sValues = GetSqlExpVal(m_colNum);
					if (sValues == null) 
					{
						sValues = GetNodeText(m_colNum);	
					}
					m_colNums = Convert.ToInt32(ReplaceRunInfo(sValues.Trim()));
				}				
				catch(Exception e)
				{
					ShowFMessage("非法的例数表达式："+e.Message);
					return false;
				}
				if (m_colNums<=0) 
				{
					ShowFMessage("非法的例数值，列数应该大于等于1");
					return false;
				}
				if (stConnectINfo.rd.FieldCount != m_colNums) 
				{
					ShowFMessage("结果集中的列数跟脚本中的列数不一致，结果集中的列数为:" + stConnectINfo.rd.FieldCount);
					return false;
				}
			}
			XmlNode m_tempNode = SkipComment(m_XmlNode.FirstChild);//跳过注释部分
			if(m_tempNode != null)
			{
				if(!stConnectINfo.rd.HasRows)
				{
					ShowFMessage("预期语句返回一个非空的结果集，但是实际返回了一个空结果集");
					return false;
				}
			}
			int m_row = 1;//表示当前的行数			
			while(m_XmlNode != null)				//开始比较结果集
			{
				//这是结果集的行开始
				vlc.SetVal("RECORDNUMS", m_row.ToString());
				if(!stConnectINfo.rd.Read())
				{
					ShowFMessage("结果集已经到达未尾，但是XML文件中还有未比较的行");
					return false;
				}				
				if (m_colNum == null) 
				{
					if (!CheckColumn(m_XmlNode.FirstChild, m_row)) 
					{
						break;
					}
				}
				else
				{
					if (CheckColumnEx(m_XmlNode.FirstChild, m_row, m_colNums)) 
					{
						break;
					}
				}
				m_XmlNode = m_XmlNode.NextSibling;
				m_XmlNode = SkipComment(m_XmlNode);
				m_row++;
			}
			if(m_XmlNode == null && stConnectINfo.rd.Read())
			{
				ShowFMessage("结果集未到达未尾，但是XML文件中已经没有了可以比较的行");
				return false;
			}
			return true;
		}
		public bool CheckResultRowEx(XmlNode m_XmlNode, int rownums)
		{			
			if(rownums <= 0)
			{
				if(stConnectINfo.rd.HasRows)
				{
					ShowFMessage("预期语句返回一个空的结果集，但是实际返回了一个非空结果集");
					return false;
				}
				else
				{
					return true;
				}
			}
			m_XmlNode = FindXmlNode(m_XmlNode, "RECORD");
			if (m_XmlNode == null) 
			{
				ShowFMessage("未找到RECORD行");
				return false;
			}
			if (FindXmlNode(m_XmlNode.NextSibling, "RECORD") != null) 
			{
				ShowFMessage("在使用RECORDNUMS关键字时，结果集中只能包含一个RECORD行");
				return false;
			}
			XmlNode m_colNum = FindXmlNode(m_XmlNode.FirstChild, "COLUMNNUMS");
			int m_colNums = 0;
			if (m_colNum != null) 
			{
				try
				{
					String sValues = GetSqlExpVal(m_colNum);
					if (sValues == null) 
					{
						sValues = GetNodeText(m_colNum);	
					}
					m_colNums = Convert.ToInt32(ReplaceRunInfo(sValues.Trim()));
				}				
				catch(Exception e)
				{
					ShowFMessage("非法的例数表达式："+e.Message);
					return false;
				}
				if (m_colNums<=0) 
				{
					ShowFMessage("非法的例数值，列数应该大于等于1");
					return false;
				}
				if (stConnectINfo.rd.FieldCount != m_colNums) 
				{
					ShowFMessage("结果集中的列数跟脚本中的列数不一致，结果集中的列数为:" + stConnectINfo.rd.FieldCount);
					return false;
				}
			}
			int m_row = 1;//表示当前的行数
			while(m_row <= rownums)				//开始比较结果集
			{
				//这是结果集的行开始
				if(!stConnectINfo.rd.Read())
				{
					ShowFMessage("结果集已经到达未尾，但是XML文件中还有未比较的行");
					return false;
				}		
				vlc.SetVal("RECORDNUMS", m_row.ToString());
				if (m_colNum == null) 
				{
					if (!CheckColumn(m_XmlNode.FirstChild, m_row)) 
					{
						break;
					}
				}
				else
				{
					if (!CheckColumnEx(m_XmlNode.FirstChild, m_row, m_colNums)) 
					{
						break;
					}
				}
				m_row++;
			}
			if(stConnectINfo.rd.Read())
			{
				ShowFMessage("结果集未到达未尾，但是XML文件中已经没有了可以比较的行");
				return false;
			}
			return true;
		}
		public bool CheckColumn(XmlNode m_colNode, int m_row)
		{
			string errorMessage = "";
			int m_col = 0;
			m_colNode = SkipComment(m_colNode);
			while(m_colNode!= null)
			{
				//这是结果集的每一列开始
				if(m_col >= stConnectINfo.rd.FieldCount)
				{
					ShowFMessage("XML文件中的例数超过了返回结果集中的列数");
					return false;
				}
				string ExpCol = GetColumn(m_colNode);
				if(ExpCol == null)
				{
					return false;
				}
				ExpCol = ReplaceRunInfo(ExpCol);		
				string RetCol = "";
				try
				{
					if(stConnectINfo.rd[m_col] == System.DBNull.Value)
					{
						RetCol = "NULL";
					}
					else if(stConnectINfo.rd.GetFieldType(m_col) == System.Type.GetType("System.Byte[]"))
					{			
						RetCol = "0X" + ByteArrayToHexString((byte[])stConnectINfo.rd[m_col]);
						ExpCol = ExpCol.ToUpper();
					}
					else
					{
						RetCol = Convert.ToString(stConnectINfo.rd[m_col]);		//把列中的值。变为字符串再进行比较
					}
				}
#if DM7
                catch (DmException e)
                {
                    string errorMessages = "";

                    errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
			    catch (OleDbException e)
				{
					for (int i=0; i < e.Errors.Count; i++)
					{
						errorMessage += "Index #" + i + "\n" +
							"Message: " + e.Errors[i].Message + "\n" +
							"NativeError: " + e.Errors[i].NativeError + "\n" +
							"Source: " + e.Errors[i].Source + "\n" +
							"SQLState: " + e.Errors[i].SQLState + "\n";
					}
#endif
                    ShowFMessage(errorMessage);
					return false;
				}
				catch (Exception e)
				{
					errorMessage += "结果集在第" + (m_row) + "行，第" + (m_col + 1) +"列处,";
					errorMessage += e.Message;
					ShowFMessage(errorMessage);
					return false;
				}
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT")		//只比较XML文件中给定值长度部分
				{
					if(String.Compare(ExpCol, 0, RetCol, 0, ExpCol.Length, false) != 0)
					{
						errorMessage += "结果集在第" + (m_row) + "行，第" + (m_col + 1) +"列处，结果不一致\n";
						errorMessage += "预期值: " + ExpCol;
						errorMessage += "\n返回值: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				else//结果值整个比较
				{
					if(ExpCol != RetCol)
					{
						errorMessage += "结果集在第" + (m_row) + "行，第" + (m_col + 1) +"列处，结果不一致\n";
						errorMessage += "预期值: " + ExpCol;
						errorMessage += "\n返回值: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				m_colNode = m_colNode.NextSibling;
				m_colNode = SkipComment(m_colNode);
				m_col++;	
			}
			return true;
		}
		public bool CheckColumnEx(XmlNode m_colNode, int m_row, int m_colnums)
		{
			string errorMessage = "";
			int m_col = 0;
			m_colNode = FindXmlNode(m_colNode, "COLUMN");
			if (m_colNode == null) 
			{
				ShowFMessage("未发现COMUMN");
				return false;
			}
			if (FindXmlNode(m_colNode.NextSibling, "COLUMN") != null) 
			{
				ShowFMessage("在使用了COLUMNNUMS关键字的情况下，只能指定一个COLUMN列");
				return false;
			} 
			while(m_col < m_colnums)
			{
				vlc.SetVal("COLUMNNUMS", (m_col+1).ToString());
				string ExpCol = GetColumn(m_colNode);
				if(ExpCol == null)
				{
					return false;
				}
				ExpCol = ReplaceRunInfo(ExpCol);
				string RetCol = "";
				try
				{
					if(stConnectINfo.rd[m_col] == System.DBNull.Value)
					{
						RetCol = "NULL";
					}
					else if(stConnectINfo.rd.GetFieldType(m_col) == System.Type.GetType("System.Byte[]"))
					{			
						RetCol = "0X" + ByteArrayToHexString((byte[])stConnectINfo.rd[m_col]);
						ExpCol = ExpCol.ToUpper();
					}
					else
					{
						RetCol = Convert.ToString(stConnectINfo.rd[m_col]);		//把列中的值。变为字符串再进行比较
					}
				}
#if DM7
                catch (DmException e)
                {
                    string errorMessages = "";

                    errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
		    	catch (OleDbException e)
				{
					for (int i=0; i < e.Errors.Count; i++)
					{
						errorMessage += "Index #" + i + "\n" +
							"Message: " + e.Errors[i].Message + "\n" +
							"NativeError: " + e.Errors[i].NativeError + "\n" +
							"Source: " + e.Errors[i].Source + "\n" +
							"SQLState: " + e.Errors[i].SQLState + "\n";
					}
#endif
					ShowFMessage(errorMessage);
					return false;
				}
				catch (Exception e)
				{
					errorMessage += "结果集在第" + (m_row) + "行，第" + (m_col + 1) +"列处,";
					errorMessage += e.Message;
					ShowFMessage(errorMessage);
					return false;
				}
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT")		//只比较XML文件中给定值长度部分
				{
					if(String.Compare(ExpCol, 0, RetCol, 0, ExpCol.Length, false) != 0)
					{
						errorMessage += "结果集在第" + (m_row) + "行，第" + (m_col + 1) +"列处，结果不一致\n";
						errorMessage += "预期值: " + ExpCol;
						errorMessage += "\n返回值: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				else//结果值整个比较
				{
					if(ExpCol != RetCol)
					{
						errorMessage += "结果集在第" + (m_row) + "行，第" + (m_col + 1) +"列处，结果不一致\n";
						errorMessage += "预期值: " + ExpCol;
						errorMessage += "\n返回值: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				m_col++;	
			}
			return true;
		}
		/// <summary>
		/// 数据流转换为十六进制字符串
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		private static string ByteArrayToHexString(byte[] bytes)
		{
			if ( bytes == null )
				throw new ArgumentException( "bytes [] 参数出错" );
			StringBuilder hexString = new StringBuilder( 2 * bytes.Length );

			for ( int i = 0; i < bytes.Length; i++ )
				hexString.AppendFormat( "{0:X2}", bytes[i] );
			return hexString.ToString();
		}


		/// <summary>
		/// //用来跳过注释结点
		/// </summary>
		public XmlNode SkipComment(XmlNode m_XmlNode)//解析注释节点
		{
			if(m_XmlNode == null)
				return null;
			while(m_XmlNode != null && (m_XmlNode.Name == "#comment"))
			{
				/*int index1;
				int index2;
				string testtmpnum=m_XmlNode.Value ;//获取注释的内容
                index1=testtmpnum.IndexOf ("测试点");
				index2=testtmpnum.IndexOf ("结束");
				if(index1>0&&index2<0)
					testnum=testtmpnum.Substring (index1,8);*/
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return m_XmlNode;
		}
		/// <summary>
		/// //检察执行的结果
		/// </summary>
		public bool CheckResult(bool bCheckColName)
		{
			if(stConnectINfo.rd == null)
			{
				ShowFMessage("结果集对像为空，上一次执行的语句没有产生相应的结果集！");
				return false;
			}
			Debug.Assert(stXmlRunInfo.xCurrentNode != null, "在检查结果集时，给定的当前结点引用值为空", "XmlTest.CheckResult 函数");
			if(stXmlRunInfo.xCurrentNode == null)
			{
				ShowFMessage("在检查结果集时, 当前运行的XML结点引用为空，没有结点被引用");
				return false;
			}
			stXmlRunInfo.xCurrentNode = stXmlRunInfo.xCurrentNode.NextSibling;
			stXmlRunInfo.xCurrentNode = SkipComment(stXmlRunInfo.xCurrentNode);
			if(stXmlRunInfo.xCurrentNode == null)
			{
				ShowFMessage("在检查结果集时, 生成结果集语句后面没有发现结果集结点");
				return false;
			}
			if(stXmlRunInfo.xCurrentNode.Name != "RESULT")
			{
				ShowFMessage("被检察的当前结点，不是代表结果集的结点，该结点名称为:" + stXmlRunInfo.xCurrentNode.Name);
				return false;
			}
			if(bCheckColName)
			{
				XmlNode m_colNum = FindXmlNode(stXmlRunInfo.xCurrentNode.FirstChild, "COLUMNNUMS");
				int m_colNums = 0;
				if (m_colNum != null) 
				{
					try
					{
						String sValues = GetSqlExpVal(m_colNum);
						if (sValues == null) 
						{
							sValues = GetNodeText(m_colNum);	
						}
						m_colNums = Convert.ToInt32(ReplaceRunInfo(sValues.Trim()));
					}				
					catch(Exception e)
					{
						ShowFMessage("非法的例数表达式："+e.Message);
						return false;
					}
					if (m_colNums<=0) 
					{
						ShowFMessage("非法的例数值，列数应该大于等于1");
						return false;
					}
					if (stConnectINfo.rd.FieldCount != m_colNums) 
					{
						ShowFMessage("结果集中的列数跟脚本中的列数不一致，结果集中的列数为:" + stConnectINfo.rd.FieldCount);
						return false;
					}
				}
				if (m_colNum != null) 
				{
					return CheckResultColNameEx(stXmlRunInfo.xCurrentNode.FirstChild, m_colNums);
				}
				else
				{
					return CheckResultColName(stXmlRunInfo.xCurrentNode.FirstChild);
				}
				
			}
			else
			{
				XmlNode m_rowNum = FindXmlNode(stXmlRunInfo.xCurrentNode.FirstChild, "RECORDNUMS");
				if (m_rowNum == null) 
				{
					return CheckResultRow(stXmlRunInfo.xCurrentNode.FirstChild);
				}
				else
				{
					int rowNums = 0;
					try
					{
						String sValues = GetSqlExpVal(m_rowNum);
						if (sValues == null) 
						{
							sValues = GetNodeText(m_rowNum);	
						}
						rowNums = Convert.ToInt32(ReplaceRunInfo(sValues.Trim()));
					}
					catch(Exception e)
					{
						ShowFMessage("非法的行数表达式："+e.Message);
						return false;
					}					
					return CheckResultRowEx(stXmlRunInfo.xCurrentNode.FirstChild, rowNums);
				}
			}
		}

		//检察结果集，只检察各列的名称，并不检察其中各行各列的值
		public bool CheckResultColName(XmlNode m_XmlNode)
		{
			m_XmlNode = SkipComment(m_XmlNode);
			if(m_XmlNode == null)
			{
				ShowFMessage("未给定要比较的列名行");
				return false;
			}
			if(m_XmlNode.Name != "RECORD")
			{
				ShowFMessage("结果集起始行的名称不是代表结果集中行的结点，该结点名称为:" + m_XmlNode.Name);
				return false;
			}
			XmlNode m_tempNode = SkipComment(m_XmlNode.FirstChild);//跳过注释部分
			//			MD5 md5 = new MD5CryptoServiceProvider();
			//
			//			byte[] result1;
			//			byte[] result2;
			int m_maxColNum = 0;//表示当前XML文件结果集中列的最大数			
			while(m_tempNode != null)
			{
				string ExpCol = GetColumn(m_tempNode);
				if(ExpCol == null)
				{
					ShowFMessage("未在当前节点找到列的名称");
					return false;
				}
				ExpCol = ReplaceRunInfo(ExpCol);
				//				ExpCol = ReplaceRunInfo(ExpCol);
				//				Convert.tob
				//				md5.ComputeHash(
				//				result1 = md5.ComputeHash(ExpCol.t);
				//				result2 = md5.ComputeHash();
				if(ExpCol != stConnectINfo.rd.GetName(m_maxColNum))
				{
					ShowFMessage("返回结果集的列名称和脚本中的不匹配\n返回：" + stConnectINfo.rd.GetName(m_maxColNum) + "\n预期：" + ExpCol);
					return false;
				}
				m_tempNode = m_tempNode.NextSibling;
				m_tempNode = SkipComment(m_tempNode);
				m_maxColNum++;
				if(m_maxColNum >= stConnectINfo.rd.FieldCount && m_tempNode != null)
				{
					ShowFMessage("返回结果集的列数小于脚本中的列数不一致，结果集中存在 " + stConnectINfo.rd.FieldCount + "列");
					return false;
				}
			}
			if(m_maxColNum != stConnectINfo.rd.FieldCount)
			{
				ShowFMessage("返回结果集的列数和脚本中的列数不一致\n结果集：" + stConnectINfo.rd.FieldCount + "\n脚  本：" + m_maxColNum);
				return false;
			}
			return true;
		}
		//检察结果集，只检察各列的名称，并不检察其中各行各列的值
		public bool CheckResultColNameEx(XmlNode m_colNode, int m_colnums)
		{
			m_colNode = FindXmlNode(m_colNode, "COLUMN");
			if (m_colNode == null) 
			{
				ShowFMessage("未发现COMUMN");
				return false;
			}
			if (FindXmlNode(m_colNode.NextSibling, "COLUMN") != null) 
			{
				ShowFMessage("在使用了COLUMNNUMS关键字的情况下，只能指定一个COLUMN列");
				return false;
			}

			int m_maxColNum = 0;//表示当前XML文件结果集中列的最大数			
			while(m_maxColNum<m_colnums)
			{
				vlc.SetVal("COLUMNNUMS", (m_maxColNum+1).ToString());
				string ExpCol = GetColumn(m_colNode);
				if(ExpCol == null)
				{
					ShowFMessage("未在当前节点找到列的名称");
					return false;
				}
				ExpCol = ReplaceRunInfo(ExpCol);
				if(ExpCol != stConnectINfo.rd.GetName(m_maxColNum))
				{
					ShowFMessage("返回结果集的列名称和脚本中的不匹配\n返回：" + stConnectINfo.rd.GetName(m_maxColNum) + "\n预期：" + ExpCol);
					return false;
				}

				m_maxColNum++;
			}
			return true;
		}
		/// <summary>
		/// //检察执行的结果
		/// </summary>
#if DM7
        public bool CheckExecute(bool m_su, string m_sql, DmException e)
#else
		public bool CheckExecute(bool m_su, string m_sql, OleDbException e)
#endif
		{			
			string retcode = "0";
			string sqlstate = "";
			if(stXmlRunInfo.bClearEn)	//如果该操作是用来清除环境的，那么，对它运行的正确情不作检察
			{
				if(!m_su)
				{
					string sMessages = "";
#if DM7
                    sMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
					for (int i=0; i < e.Errors.Count; i++)
					{
						sMessages += "Index #" + i + "\n" +
							"Message: " + e.Errors[i].Message + "\n" +
							"SQLState: " + e.Errors[i].SQLState + "\n";
					}
#endif

                    ShowSMessage(sMessages);
				}
				return true;
			}
			string errorMessages = "";
			errorMessages = "语句：" + m_sql + "\n预期执行结果：" + stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult
				+ "\n实际执行结果：";
			if(m_su)
			{
				errorMessages += "DIRECT_EXECUTE_SUCCESS\n";
			}
			else
			{				
				errorMessages += "DIRECT_EXECUTE_FAIL\n";
				string sMessages = "";
#if DM7
                errorMessages += "Message: " + e.Message + "\n" +
                            "NativeError: " + e.ErrorCode + "\n" +
                            "Source: " + e.Source + "\n";
#else
                for (int i=0; i < e.Errors.Count; i++)
				{
					retcode = e.Errors[i].NativeError.ToString();
					sqlstate = e.Errors[i].SQLState;
					sMessages += "Index #" + i + "\n" +
						"Message: " + e.Errors[i].Message + "\n" +
						"SQLState: " + e.Errors[i].SQLState + "\n";
				}
#endif

                vlc.SetVal("SQLSTATE", sqlstate);
				vlc.SetVal("RETCODE", retcode);
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult != "DIRECT_EXECUTE_FAIL")
				{						
					if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_IGNORE")
					{
						ShowSMessage(sMessages);
						return true;
					}
					ShowFMessage(sMessages);
				    //-----test	
					sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
					if(sqlCounter.sqlLineNum > 0)
						ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消
				    //-----test
				}
				else
				{
					ShowSMessage(sMessages);
				}
			}
			
			if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SUCCESS")
			{
				if(m_su)
				{
					if (ProClass.bValIsShowResult) {
						ShowResult();
					}
					return true;
				}
				else
				{					
					ShowFMessage(errorMessages);
					//-----test	
					sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
					if(sqlCounter.sqlLineNum > 0)
						ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消
					//-----test
				}
			}
			else if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_FAIL")
			{
				if(!m_su)
				{
#if DM7
                    if (e == null)
#else
                    if(e == null || e.Errors.Count<1)
#endif
                    {
						errorMessages += "语句执行失败，但是没有返回错误信息，请通知OLEDB开发人员";
					}
					else
					{
						errorMessages = "语句：" + m_sql + "执行完成后\n";
						bool bFail = false;
						
						if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError != 0)
						{		
#if DM7
                            if (stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError != e.ErrorCode)
                            {
                                errorMessages += "预期返回 NativeError: " + stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError + "\n实际返回返回 NativeError: " + e.ErrorCode + "\n";
                                bFail = true;
                            }
#else
                            int i=0;
                            for(i=0; i<e.Errors.Count; i++)
							{
								if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError == e.Errors[i].NativeError)
								{
									break;
								}
							}
							if(i >= e.Errors.Count)
							{
								errorMessages += "预期返回 NativeError: " + stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError + "\n实际返回返回 NativeError: " + e.Errors[0].NativeError + "\n";
								bFail = true;
							}
#endif

                        }
						if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sSQLState != "" && stXmlRunInfo.cCurrentSqlCase.stSqlResult.sSQLState != null)
						{
#if DM7
                            
#else
                            int i=0;
                            for(i=0; i<e.Errors.Count; i++)
							{
								if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sSQLState == e.Errors[0].SQLState)
								{
									break;
								}
							}
							if(i >= e.Errors.Count)
							{
								errorMessages += "预期返回 SQLState: " + stXmlRunInfo.cCurrentSqlCase.stSqlResult.sSQLState + "\n实际返回返回 SQLState: " + e.Errors[0].SQLState + "\n";
								bFail = true;
							}
#endif

                        }		
						if(bFail == false)
							return true;	
					}
				}
				ShowFMessage(errorMessages);
				//-----test
				sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
				if(sqlCounter.sqlLineNum > 0)
					ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消
			    //-----test
			}
			else if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT" || stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT_FULL")
			{
				if(!m_su)
				{
					ShowFMessage("结果集生成语句执行失败了，无法进行结果集比较");
					//-----test
					sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
					if(sqlCounter.sqlLineNum > 0)
						ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消
					//-----test
					return false;
				}
				if(CheckResult(false))
					return true;
			}
			else if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_WITH_RESULT")
			{
				if(!m_su)
				{
					ShowFMessage("结果集生成语句执行失败了，无法进行结果集比较");
					//-----test
					sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
					if(sqlCounter.sqlLineNum > 0)
						ShowFMessage("【" + testnum + "】" + "执行错误！\n ");//执行中报错处理，报告是在哪个测试点出错，把下面的定位到行的报错方式给取消
					//-----test
					return false;
				}
				if(CheckResult(true))
					return true;
			}
			return false;
		}

		public int GetTypeSize(System.Type _type)
		{
			if (_type == System.Type.GetType("System.Int16")) {
				return 7;
			}
			else if (_type == System.Type.GetType("System.SByte")) 
			{
				return 5;
			}
			else if (_type == System.Type.GetType("System.Int32")) {
				return 12;
			}
			else if (_type == System.Type.GetType("System.Int64")) 
			{
				return 21;
			}
			else if (_type == System.Type.GetType("System.Single")) {
				return 21;
			}
			else if (_type == System.Type.GetType("System.Double")) 
			{
				return 21;
			}
			else {
				return 30;
			}
		}
		/// <summary>
		/// 显示查询语句生成的结果集
		/// </summary>
		public void ShowResult()
		{
			if(stConnectINfo.rd == null || stConnectINfo.rd.FieldCount == 0)
			{
				return;
			}
			XmlNode m_xmlNode = stXmlRunInfo.xCurrentNode;
			m_xmlNode = SkipComment(m_xmlNode.NextSibling);
			if (m_xmlNode != null && (m_xmlNode.Name == "EFFECTROWS" || m_xmlNode.Name == "OPEN")) {
				return;
			}
			int maxColLen = 30;
			String colName = "ROWNUM";
			String colText = "";
			int rows=1;
			int cols=0;
#if DM7
            DmDataReader rd = stConnectINfo.rd;
#else
            OleDbDataReader rd = stConnectINfo.rd;
#endif

            //显示列名
			while (colName.Length<10) 
			{
				colName += " ";
			}
			colText = colName;			
			for (cols=0; cols < rd.FieldCount; cols++) {
				colName = rd.GetName(cols);
				maxColLen = GetTypeSize(rd.GetFieldType(cols));
				if (colName.Length>=maxColLen) {
					colName = colName.Substring(0, maxColLen-1);
				}
				else
				{
					while (colName.Length<maxColLen-1) 
					{
						colName += " ";
					}
				}
				colText += colName;
			}
			
			colText = colText.TrimEnd();
			ShowSMessage(colText);
			while (rd.Read() && rows<100) {
				colName = rows.ToString();
				while (colName.Length<10) 
				{
					colName += " ";
				}
				colText = colName;
				for (cols=0; cols < rd.FieldCount; cols++){
					maxColLen = GetTypeSize(rd.GetFieldType(cols));
					if(rd[cols] == System.DBNull.Value){
						colName = "NULL";
					}
					else if(rd.GetFieldType(cols) == System.Type.GetType("System.Byte[]")){
						colName = "0X" + ByteArrayToHexString((byte[])stConnectINfo.rd[cols]);
					}
					else{
						colName = Convert.ToString(stConnectINfo.rd[cols]);
					}
					if (colName.Length>=maxColLen) 
					{
						colName = colName.Substring(0, maxColLen-1);
					}
					else{
						while (colName.Length<maxColLen-1) {
							colName += " ";
						}
					}
					colText += colName;
				}
				colText = colText.TrimEnd();
				ShowSMessage(colText);
				rows ++;
			}
		}
		/// <summary>
		/// //用来清除关于连接上的错误信息
		/// </summary>

		public void ShowSMessage(string m_message)
		{
		//	Monitor.Enter(this);
			stXmlRunInfo.tdTestThread.ShowSMessage(m_message, false);
		//	Monitor.Exit(this);
		}
		public void ShowFMessage(string m_message)
		{
			stXmlRunInfo.cCurrentSqlCase.stCaseResult.bSuccess = false;
			vlc.SetVal("CASERESULT", "FALSE");
			if(stXmlRunInfo.cCurrentSqlCase.stCaseResult.bExpResult)
			{
				cSqlCase.stCaseResult.bSuccess = false;
				
				if(!ProClass.bValIsErrRun && stXmlRunInfo.bClearEn == false)//如果不允许继续运行下面一个结点的值
					stXmlRunInfo.bStop = true;
				stXmlRunInfo.tdTestThread .ShowFMessage(m_message, true);
			}
			else
			{
				stXmlRunInfo.tdTestThread .ShowSMessage(m_message, false);
			}
		}

		public bool DisConnect(int iIndex)
		{
            CONNECTINFO stTemp = stConnectINfo;
			if (stConnectINfo == null) {
				return false;;
			}
			Debug.Assert(iIndex >= -1, "设置连接的ID小于-1", "XmlTest.DisConnect 函数");
			if(iIndex == -1)//如果该值为-1,那么断开所有的连接
			{				
				for(int index=0; index<stConnectArry.Count; index++)
				{
					if(!SetCn(index, false))
					{
						ShowFMessage("设置连接时出错");
                        stConnectINfo = stTemp;
						return false;
					}
					try
					{
						if(stConnectINfo.cn != null && stConnectINfo.cn.State == ConnectionState.Open && stConnectINfo.cm != null)
						{
							try
							{
								while(stConnectINfo.tr != null)
								{
									EndTrans("ROLLBACK");
								}
							}
							catch (Exception e)
							{
								ShowFMessage(e.Message);
							}
						}
						if(stConnectINfo.rd != null)
						{
							stConnectINfo.rd.Close();
						}
						if(stConnectINfo.cn.State == ConnectionState.Open)
						{
							stConnectINfo.cn.Close();
							stConnectINfo.cn.Dispose();
						}
					}
#if DM7
                    catch (DmException e)
                    {
                        string errorMessages = "";

                        errorMessages += "Message: " + e.Message + "\n" +
                                "NativeError: " + e.ErrorCode + "\n" +
                                "Source: " + e.Source + "\n";
#else
                    catch (OleDbException e)
                    {
						string errorMessages = "";
						for (int i=0; i < e.Errors.Count; i++)
						{
							errorMessages += "Index #" + i + "\n" +
								"Message: " + e.Errors[i].Message + "\n" +
								"NativeError: " + e.Errors[i].NativeError + "\n" +
								"Source: " + e.Errors[i].Source + "\n" +
								"SQLState: " + e.Errors[i].SQLState + "\n";
						}
#endif
                        ShowFMessage(errorMessages);
                        stConnectINfo = stTemp;
						return false;
					}
                    catch (Exception e)
                    {
                        ShowFMessage(e.Message);
                        stConnectINfo = stTemp;
                        return false;
                    }
				}
				ShowSMessage("连接已经全部被断开");
				try
				{
#if DM7
                    //DmConnection.ReleaseObjectPool();//试图清除连接池中的连接
#else
                    OleDbConnection.ReleaseObjectPool();//试图清除连接池中的连接
#endif
                }
				catch(Exception e)
				{
					ShowFMessage(e.Message);
				}
                stConnectINfo = stTemp;
				return true;
			}
			if(iIndex >= stConnectArry.Count || iIndex < -1)//如果XML文件中要设置的当前连接ID大于最大的ID，或是小于-1
			{
                stConnectINfo = stTemp;
				return true;
			}
			SetCn(iIndex, false);
			try
			{
				while(stConnectINfo.tr != null)
				{
					EndTrans("ROLLBACK");
				}
				if(stConnectINfo.rd != null)
					stConnectINfo.rd.Close();
				if(stConnectINfo.cn.State == ConnectionState.Open)
				{
					stConnectINfo.cn.Close();
					ShowSMessage("索引为 " + iIndex + " 的连接已经被断开");
				}
			}
#if DM7
            catch (DmException e)
            {
                string errorMessages = "";

                errorMessages += "Message: " + e.Message + "\n" +
                        "NativeError: " + e.ErrorCode + "\n" +
                        "Source: " + e.Source + "\n";
#else
            catch (OleDbException e)
			{
				string errorMessages = "";
				for (int i=0; i < e.Errors.Count; i++)
				{
					errorMessages += "Index #" + i + "\n" +
						"Message: " + e.Errors[i].Message + "\n" +
						"NativeError: " + e.Errors[i].NativeError + "\n" +
						"Source: " + e.Errors[i].Source + "\n" +
						"SQLState: " + e.Errors[i].SQLState + "\n";
				}
#endif
				ShowFMessage(errorMessages);
                stConnectINfo = stTemp;
				return false;
			}
            catch (Exception e)
            {
                ShowFMessage(e.Message);
                stConnectINfo = stTemp;
                return false;
            }
			try
			{
#if DM7
      //          DmConnection.ReleaseObjectPool();//试图清除连接池中的连接
#else
                OleDbConnection.ReleaseObjectPool();//试图清除连接池中的连接
#endif
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message);
			}
            stConnectINfo = stTemp;
			return true;
		}
		string CreateConnectStr()
		{
			string sConnectStr;			//连接字符串对像
#if DM7
            sConnectStr = "Server=" + sServerName;
            sConnectStr += ";User Id=" + sUid; 
            sConnectStr += ";PWD=" + sPwd;
            Console.Write("XML连接字符串" + sConnectStr);
#else
			sConnectStr = "Provider=" + sProvider + ";User ID=" + sUid;
			sConnectStr += ";Password=" + sPwd;
			sConnectStr += ";Data Source=";
			sConnectStr += sServerName;
			sConnectStr += ";Persist Security Info=True";
			if (sDatabase.Length > 0) 
			{
				sConnectStr += ";Initial Catalog="+sDatabase;
			}
			if (sPort.Length > 0) 
			{
				sConnectStr += ";Port=";
				sConnectStr += sPort;
			}
			if (!bPoolEnable) {
				sConnectStr += ";Connect Pool = -1";
			}
#endif
			return sConnectStr;
		}
		//重新连接当前的连接
		private bool ReConnect()
		{	
			if (stConnectINfo == null) {
				ShowFMessage("没有任何连接");
			}
			try
			{
				while(stConnectINfo.tr != null)
				{
					EndTrans("ROLLBACK");
				}
				if(stConnectINfo.rd != null)
					stConnectINfo.rd.Close();
				if(stConnectINfo.cn.State == ConnectionState.Open)
				{
					stConnectINfo.cn.Close();
					ShowSMessage("当前连接已经被断开");
				}
			}
#if DM7
            catch (DmException e)
            {
                string errorMessages = "";

                errorMessages += "Message: " + e.Message + "\n" +
                        "NativeError: " + e.ErrorCode + "\n" +
                        "Source: " + e.Source + "\n";
#else
            catch (OleDbException e)
			{
				string errorMessages = "";
				for (int i=0; i < e.Errors.Count; i++)
				{
					errorMessages += "Index #" + i + "\n" +
						"Message: " + e.Errors[i].Message + "\n" +
						"NativeError: " + e.Errors[i].NativeError + "\n" +
						"Source: " + e.Errors[i].Source + "\n" +
						"SQLState: " + e.Errors[i].SQLState + "\n";
				}
#endif
				ShowFMessage(errorMessages);
				return false;
			}
            catch (Exception e)
            {
                ShowFMessage(e.Message);
                return false;
            }
			try
			{				
				stConnectINfo.cn.Open();
#if DM7
                ShowSMessage("当前连接已经被重连!" + "  用户名" + stConnectINfo.sUid + "; 口令：" + stConnectINfo.sPwd + "; 初始库：" + stConnectINfo.sDatabase + "; 服务器：" + stConnectINfo.sServerName);
#else
                ShowSMessage("当前连接已经被重连!" + "  用户名" + stConnectINfo.sUid + "; 口令：" + stConnectINfo.sPwd + "; 初始库：" + stConnectINfo.sDatabase + "; 服务器：" + stConnectINfo.sServerName + "; 驱动：" + stConnectINfo.sProvider);

#endif
            }
#if DM7
            catch (DmException e)
            {
                string errorMessages = "";

                errorMessages += "Message: " + e.Message + "\n" +
                        "NativeError: " + e.ErrorCode + "\n" +
                        "Source: " + e.Source + "\n";
#else
            catch (OleDbException e)
			{
				string errorMessages = "";
				for (int i=0; i < e.Errors.Count; i++)
				{
					errorMessages += "Index #" + i + "\n" +
						"Message: " + e.Errors[i].Message + "\n" +
						"NativeError: " + e.Errors[i].NativeError + "\n" +
						"Source: " + e.Errors[i].Source + "\n" +
						"SQLState: " + e.Errors[i].SQLState + "\n";
				}
#endif
				ShowFMessage(errorMessages);
				if(stXmlRunInfo.cCurrentSqlCase.stCaseResult.bExpResult)
					stXmlRunInfo.bStop = true;
				return false;
			}
            catch (Exception e)
            {
                ShowFMessage(e.Message);
                if (stXmlRunInfo.cCurrentSqlCase.stCaseResult.bExpResult)
                    stXmlRunInfo.bStop = true;
                return false;
            }
			return true;
		}

		//设置stXmlRunInfo.xFirstNode变量
		public void SetFirstNode(XmlNode m_FNode)
		{
			stXmlRunInfo.xFirstNode = m_FNode;
		}
		/// <summary>
		/// //用来在这个个XML文件执行另一个XML文件
		/// </summary>
		public void ExeXml(string m_XFName)
		{
			ShowSMessage("开始调用执行XML文件：" + m_XFName);			
			string m_tempStr = "";
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("当前文件路径非法，未调用执行文件：" + m_XFName);
				return;
			}
			else
			{
				if(m_XFName.StartsWith("\\"))
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex);
				else
					m_tempStr = stXmlRunInfo.sXmlFileName.Substring(0, iIndex+1);	
			}
			m_tempStr += m_XFName;
			XmlTest m_xmlTest = new XmlTest(stXmlRunInfo.tdTestThread );
			m_xmlTest.SetXmlFile(m_tempStr);
			if(!m_xmlTest.Run(true))//如果调用的XML文件执行失败，那么，这个XML文件停止测试
			{
				ShowFMessage("当前文件调用执行 " + m_XFName + " 失败");
			}
			m_xmlTest = null;
			ShowSMessage("结束执行XML文件：" + m_XFName);
		}

		/// <summary>
		/// //用来在一个XML文件中查找给定名称的结点
		/// </summary>
		public XmlNode FindXmlNode(XmlNode m_XmlNode, string m_XmlNodeName)
		{			
			Debug.Assert(m_XmlNodeName!="", "给定的结点字符串为一空串", "XmlTest.FindXmlNode 函数");
			if(m_XmlNodeName == "")
				return null;
			while(m_XmlNode!=null)
			{
				if(m_XmlNode.Name == m_XmlNodeName)
					return m_XmlNode;
				XmlNode m_XmlTempNode = FindXmlNode(m_XmlNode.FirstChild, m_XmlNodeName);
				if(m_XmlTempNode != null)
					return m_XmlTempNode;
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return null;
		}
		/// <summary>
		/// //用来在一个XML文件中查找给定名称的结点,只在第一层子节点中查
		/// </summary>
		public XmlNode FindXmlNodeEx(XmlNode m_XmlNode, string m_XmlNodeName)
		{			
			Debug.Assert(m_XmlNodeName!="", "给定的结点字符串为一空串", "XmlTest.FindXmlNode 函数");
			if(m_XmlNodeName == "")
				return null;
			while(m_XmlNode!=null)
			{
				if(m_XmlNode.Name == m_XmlNodeName)
					return m_XmlNode;
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return null;
		}
		/// <summary>
		/// //用来清除该XML文件运行的环境
		/// </summary>
		public void ClearEnvironment()
		{			
			stXmlRunInfo.bClearEn = true;
			if(stXmlRunInfo.xDoc == null)
			{
				stXmlRunInfo.xXmlTextRr = new XmlTextReader(stXmlRunInfo.sXmlFileName);
				Debug.Assert(stXmlRunInfo.xXmlTextRr!=null, "未找到指定的", "XmlTest.ClearEnvironment 函数");
				if(stXmlRunInfo.xXmlTextRr == null)//未找到指定的XML文件
				{				
					ShowFMessage("未找到指定的XML文件");
					return;
				}
				stXmlRunInfo.xDoc = new XmlDocument();
				try
				{
					stXmlRunInfo.xDoc.Load(stXmlRunInfo.xXmlTextRr);					
				}
				catch(Exception e)
				{
					ShowFMessage(e.Message);
					return;
				}
				finally
				{
					stXmlRunInfo.xXmlTextRr.Close();
				}
				
			}
			stXmlRunInfo.xFirstNode = FindXmlNode(stXmlRunInfo.xDoc.FirstChild, "CLEAR");
			
			if(stXmlRunInfo.xFirstNode != null)
			{
				RunClear();
			}
		}
	}
}
