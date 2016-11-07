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
	/// XmlTest ��ժҪ˵����
	/// </summary>
	public class TRANS_STRUCT
	{
		public Process cProcess;		//ָ��һ������Ľű��Ľ���
		public Thread cReadThread;
		public bool bIsFinishExcute;			//��ʾ�ϴη����Ľű�ִ�����û��
		public TRANS_STRUCT()
		{
			bIsFinishExcute = false;
		}
	}
	
	public class LOOP_STRUCT
	{
		public object prev;				//ָ��LOOP
		public int times;				//ִ�еĴ���
		public int start;				//ִ�еĿ�ʼ
	}

	class CONNECTINFO
	{
#if DM7
        public DmConnection cn;		//��ǰ������
        public DmCommand cm;			//��ǰ��������
        public DmDataReader rd;		//��ǰ�����������ɵĽ����
        public DmTransaction tr;		//��ǰ���ӵ�����
#else
		public OleDbConnection cn;		//��ǰ������
		public OleDbCommand cm;			//��ǰ��������
		public OleDbDataReader rd;		//��ǰ�����������ɵĽ����
		public OleDbTransaction tr;		//��ǰ���ӵ�����
#endif
        public ArrayList alTrArray;			//������Ž������������������顣
		public string sProvider;			//������OLEDB����������	
		public string sServerName;		//������IP������
		public string sUid;				//�û���
		public string sPwd;				//�û�����
		public string sDatabase;			//��ʼ�����ݿ���
		public string sPort;			//�˿�
		public bool   bPoolEnable;		//�Ƿ��������ӳع���
		public bool isOpenResult;		//�Ƿ��Ѿ�ʹ��OPEN�򿪽�����α�
		public int iFetch;				//�α����ڵĵ�ǰ����
	}

	struct XMLRUNINFO
	{		
		public bool bStop;					//�Ƿ���ֹ��ǰ��������������
		public bool bClearEn;				//��ʾ�ö����Ƿ���������ղ��Ի�����

		public string sXmlFileName;			//��Ӧ��XML�ļ�
		public TestThread tdTestThread;	//�ö������ڵ��߳�����

		public XmlTextReader xXmlTextRr;				// = new XmlTextReader(e.Node.Text);
		public XmlDocument xDoc;					// = new XmlDocument();
		public XmlNode xFirstNode;				//����ʼִ�еĵ�һ�����
		public XmlNode xCurrentNode;				//����ִ�еĵ�ǰ���

		public SqlCase cCurrentSqlCase;
	}
	public struct CASERESULT
	{
		public bool bBreak;			//�������Ƴ����Ƿ��������SQL_CASE��ִ��;
		public bool bSuccess;		//������ʾ��SQL_CASE��ִ�н�����ɹ�����ʧ��
		public bool bFailStop;		//������ʾ��SQL_CASEִ��ʱ���Ƿ�������ʧ�ܾ��������SQL_CASE��ִ��
		public bool bExpResult;		//������ʾ���SQl_CASEģ��Ԥ�ڵ�ִ�н��
	}

	public struct SQLRESULT
	{
		public string sExpResult;	//SQLִ�����Ժ�Ԥ�ڵĽ��
		public string sSQLState;			//Ԥ�ڷ��ص�״̬��
		public int iNativeError;			//Ԥ�ڷ��ص�ϵͳ�������
	}
	public class XmlTest
	{
		CONNECTINFO stConnectINfo;
		ArrayList stConnectArry;
		XMLRUNINFO stXmlRunInfo;
		SqlCase cSqlCase;
		ArrayList alTransPro;		//��������������̵�����

		string sProvider;			//������OLEDB����������	
		string sServerName;		//������IP������
		string sUid;				//�û���
		string sPwd;				//�û�����
		string sDatabase;			//��ʼ�����ݿ���
		string sPort;			//�˿�
		bool   bPoolEnable;		//�Ƿ��������ӳع���

		LOOP_STRUCT cLoop;
		Int64		usedtimes;
		int		run_times;
		bool	isHasTimes;
		bool	m_noShow;//�Ƿ���ʾִ�е����
		public string testnum="���Ե�0��";//���ڱ�����Ե�ĸ�����Ϣ
		//public int testnum=0;

		ValClass vlc;
		//-----test
        private Counter sqlCaseCounter = new Counter();    //sqlCase����
		private Counter sqlCounter = new Counter();
		private Counter newConnCounter = new Counter();
		private Counter resultCounter = new Counter();
		private int CurrentLineNum=0;  //��ǰ�к�

		public string xmlFileName;
		
        //-----test
		
		public XmlTest(TestThread m_th)
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
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
		public TRANS_STRUCT tCurrentTransStruct; //ָ��ǰ����������̶�Ӧ�Ľṹ���ý������������Ӧ�Ķ�ȡ����������������߳�
		//���ص�ǰXML�ļ�ִ�еĽڵ�
		public XmlNode GetCurrentXmlNode()
		{
			return stXmlRunInfo.xCurrentNode;
		}
		//�����Ƿ�ִ�гɹ�
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
			vlc.AddPrimaryKey("SERVERPATH", ProClass.sValServerPathDir);   //����
			vlc.AddPrimaryKey("SQLSTR", ProClass.sValGetDmIni);
			
			vlc.AddPrimaryKey("PROCESSID", "");
			vlc.AddPrimaryKey("RECORDNUMS", "");
			vlc.AddPrimaryKey("COLUMNNUMS", "");
			vlc.AddPrimaryKey("PROVIDER", ProClass.sValDriveName);

	
		}
			/// <summary>
		/// //�������øö����������ݿ����Ϣ
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
		/// //���õ�ǰִ�в����õ�����
		/// </summary>
		public bool SetCn(int iIndex, bool bShowInfo)
		{
			Debug.Assert(iIndex >= 0, "�������ӵ�IDС��0", "XmlTest.SetCn ����");
			if(iIndex >= stConnectArry.Count || iIndex < 0)//���XML�ļ���Ҫ���õĵ�ǰ����ID��������ID������С��0
			{
				ShowFMessage("XML�ļ���Ҫ���õĵ�ǰ����ID��������ID" + (stConnectArry.Count - 1) + "������С��0");
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
                ShowSMessage("��ǰ�����������ģ����ڵ������� " + iIndex + " ;" + "ID��" + stConnectINfo.sUid + "; ���" + stConnectINfo.sPwd + "; ��ʼ�⣺" + stConnectINfo.sDatabase + "; ��������" + stConnectINfo.sServerName);
#else
                ShowSMessage("��ǰ�����������ģ����ڵ������� " + iIndex + " ;" + "ID��" + stConnectINfo.sUid + "; ���" + stConnectINfo.sPwd + "; ��ʼ�⣺" + stConnectINfo.sDatabase + "; ��������" + stConnectINfo.sServerName + "; ������" + stConnectINfo.sProvider);
#endif
            return true;
		}
		/// <summary>
		/// //�������ݿ�
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
                        ShowFMessage("Ԥ������ʧ�ܣ�ʵ���ϳɹ���");
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
		/// //�������ݿ�
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
			//	ShowSMessage("�������ӵ������� " + stConnectArry.Count + " ;" + "ID��" + sUid + "; ���" + sPwd + "; ��ʼ�⣺" + sDatabase + "; ��������" + sServerName + "; ������" + sProvider);
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
		/// //�������øö���Ҫִ�е�XML�ļ���
		/// </summary>
		public void SetXmlFile(string m_file)
		{
			Debug.Assert(m_file != "", "ָ��XMLʱ�� �ļ��ַ���Ϊ��", "XmlTest.SetXmlFile ����");
			stXmlRunInfo.sXmlFileName = m_file;			
			vlc.AddPrimaryKey("PATH", stXmlRunInfo.sXmlFileName.Substring(0, stXmlRunInfo.sXmlFileName.LastIndexOf("\\")));
		}

		/// <summary>
		/// //�������иò����������ɵļ�¼���
		/// </summary>
		public bool RunLog()
		{
			ShowSMessage("��ʼִ��");
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
			ShowSMessage("����");
			return cSqlCase.stCaseResult.bSuccess;
		}
		/// <summary>
		/// //��������һ������
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
				//ShowSMessage("�յ��ű���" + sXml);
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
					ShowSMessage("�����˿սű���");
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
		private string GetXmlSingleFileName(string s)//�����õ�xml�ļ��ĵ����ļ���
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
		/// //�������иò�������
		/// </summary>
		public bool Run(bool bTransfer)
		{
			
			if(bTransfer == false)
			ShowSMessage("��ʼִ��");
			if(stXmlRunInfo.xFirstNode == null)				//����ò����ļ����񣬲����Ը����׽��ķ�ʽִ�еģ���ô�Ͷ����ļ����ҳ��׽��
			{
				Debug.Assert(stXmlRunInfo.sXmlFileName != "", "XML�ļ��ַ���Ϊ��", "XmlTest.Run ����");
				
				stXmlRunInfo.xXmlTextRr = new XmlTextReader(stXmlRunInfo.sXmlFileName);
								
				Debug.Assert(stXmlRunInfo.xXmlTextRr!=null, "δ�ҵ�ָ����", "XmlTest.Run ����");
				if(stXmlRunInfo.xXmlTextRr == null)//δ�ҵ�ָ����XML�ļ�
				{				
					ShowFMessage("δ�ҵ�ָ����XML�ļ�");
					return false;
				}				
				stXmlRunInfo.xDoc = new XmlDocument();				
				try
				{
					stXmlRunInfo.xDoc.Load(stXmlRunInfo.xXmlTextRr);
					XmlNode m_XmlNode= FindXmlNode(stXmlRunInfo.xDoc.FirstChild, "SQLTEST");
					if(m_XmlNode == null)
					{
						ShowFMessage("δ��XML�ļ����ҵ��ؼ���(SQLTEST)");
						return false;
					}
					if(ProClass.bValIsLevel ==true)//�жϲ����������趨�Ĳ��Եȼ��Ƿ����Ҫ���ԵĲ��Եȼ�����������ִ�д˲�������������ִ��
					{
						if (!CheckLevel2(m_XmlNode.FirstChild)) 
						{
							return true;
						}
					}
					else if(ProClass.bValIsLevel ==false)//�жϲ����������趨�Ĳ��Եȼ��Ƿ����Ҫ���ԵĲ��Եȼ�����������ִ�д˲�������������ִ��
					{
						if (!CheckLevel1(m_XmlNode.FirstChild)) 
						{
							return true;
						}
					}
					while(run_times > 0)
					{
						SearchXmlNode(m_XmlNode, true);		//���⺯�����棬���ҳ���������ͬʱ���Խ����з����������ݷ����Ľ����ִ�н��Ҫ��Ĳ���
						if(cSqlCase.stCaseResult.bSuccess != cSqlCase.stCaseResult.bExpResult)
						{
							stXmlRunInfo.tdTestThread .ShowFMessage("�ű���Ԥ�ڵ�ִ�н����ʵ�ʵ�ִ�н����һ�£�Ԥ�ڣ�" + cSqlCase.stCaseResult.bExpResult + "; ʵ�ʣ�" + cSqlCase.stCaseResult.bSuccess, true);							
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
			else	////����ò����ļ��������Ը����׽��ķ�ʽִ�е�
			{
				//	stXmlRunInfo.bStop = false;
				SearchXmlNode(stXmlRunInfo.xFirstNode, true);	//ֱ�Ӵ��׽ڵ㿪ʼ������Ľ�㣬��������������Ҫ��Ĳ���
			}
			CloseTransPro();
			ClearEnvironment();	//��������������Ļ���,�ú���ֻ������CLEAR��㿪ʼִ��
			DisConnect(-1);//�ͷ�ȫ������
			ShowSMessage("������ɣ���ʼ����Ƿ��ܹ�������DM������--------");
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
					
							ShowSMessage("����ͨ��");
							ProClass.InsertTestResult(filename, stXmlRunInfo.sXmlFileName, "Y");
							RWArr cRWArr;
							cRWArr.TestExName =GetXmlSingleFileName(stXmlRunInfo.sXmlFileName);//��¼���Խ����Ϣ
							DateTime tm=DateTime.Now ;
							cRWArr.TestDateTime =tm.ToLongTimeString ();
							cRWArr.TestResult ="Success";
							MainForm.mytestarr.Add (cRWArr);//������Խ����Ϣ��mytestarr������
						}
						else
						{
							ShowFMessage("����ʧ��");
							ProClass.InsertTestResult(filename, stXmlRunInfo.sXmlFileName, "N");
							RWArr cRWArr=new RWArr ();
							cRWArr.TestExName =GetXmlSingleFileName(stXmlRunInfo.sXmlFileName);//��¼���Խ����Ϣ
							DateTime tm=DateTime.Now ;
							cRWArr.TestDateTime =tm.ToLongTimeString ();
							cRWArr.TestResult ="Fail";
							MainForm.mytestarr.Add (cRWArr);//������Խ����Ϣ��mytestarr������
						}
					}
				}
				else
				{
					RWArr cRWArr=new RWArr ();
					cRWArr.TestExName =GetXmlSingleFileName(stXmlRunInfo.sXmlFileName);//��¼���Խ����Ϣ
					DateTime tm=DateTime.Now ;
					cRWArr.TestDateTime =tm.ToLongTimeString ();
					cRWArr.TestResult ="Severity";
					MainForm.mytestarr.Add (cRWArr);//������Խ����Ϣ��mytestarr������
				}
	
			}
			if(bTransfer == false)
				ShowSMessage("����\n\n");
			ProClass.ClearSaveProValue();//��սű��б����ȫ�������
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
					ShowSMessage("�������Ӵ���" + cn.ConnectionString);
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
					ShowFMessage("����������ɺ󣬼�⵽���Ӳ���DM������--------");
					ShowSMessage("������������DM������");
					/*string ret1=ProClass.CommServer.ConnectSev();
					if(ret1 == "connect failed")
						ShowFMessage("���ӷ������˲��Թ���serverʧ��!");
					else
						ShowSMessage("���ӷ������˲��Թ���server�ɹ�!");*/
					string mes="START_SERVER";
					string ret;
					ret=ProClass.CommServer.Send_Rec_Message(mes);
					ShowSMessage("�����������������" + mes + "������ִ�н����" + ret + "\n");
					//ProClass.CommServer.CloseStream();
					
					return false;
				}
                catch (Exception e)
                {
                    ShowFMessage(e.Message);
                    ShowFMessage("����������ɺ󣬼�⵽���Ӳ���DM������--------");
                    ShowSMessage("������������DM������");
                    string mes = "START_SERVER";
                    string ret;
                    ret = ProClass.CommServer.Send_Rec_Message(mes);
                    ShowSMessage("�����������������" + mes + "������ִ�н����" + ret + "\n");
                    return false;
                }
				
				ShowSMessage("����������ɺ󣬼�⵽����������DM������--------");
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
				ShowFMessage("��������ʱ�����쳣");
			}
            catch(Exception e)
            {
                ShowFMessage(e.Message);
                ShowFMessage("��������ʱ�����쳣");
            }

			return true;
		}

		/// <summary>
		/// //��������һ���ڵ����ڵ�����
		/// </summary>
		public bool RunInAnNode()
		{			
			if(stXmlRunInfo.xFirstNode == null)
			{
				Debug.Assert(stXmlRunInfo.xFirstNode != null, "XML�ļ��ַ���Ϊ��", "XmlTest.RunInAnNode ����");
				return false;			
			}
			else
			{
				//		stXmlRunInfo.bStop = false;
				SearchXmlNode(stXmlRunInfo.xFirstNode.FirstChild, true);
				if(cSqlCase.stCaseResult.bSuccess != cSqlCase.stCaseResult.bExpResult)
				{
					stXmlRunInfo.tdTestThread .ShowFMessage("NEWCONNECTEXECUTE�ڵ���Ԥ�ڵ�ִ�н����ʵ�ʵ�ִ�н����һ�£�Ԥ�ڣ�" + cSqlCase.stCaseResult.bExpResult + "; ʵ�ʣ�" + cSqlCase.stCaseResult.bSuccess, true);
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
		/// //�������иò��������Ļ����������
		/// </summary>
		public void RunClear()
		{
			Debug.Assert(stXmlRunInfo.xFirstNode != null, "��ִ���������ʱ����������ʼ���Ϊ��ֵ", "XmlTest.RunClear ����");
			ShowSMessage("��ʼִ���������");
			stXmlRunInfo.bStop = false;
			stXmlRunInfo.bClearEn = true;
			stXmlRunInfo.cCurrentSqlCase = new SqlCase();
			SearchXmlNode(stXmlRunInfo.xFirstNode, true);
			stXmlRunInfo.bClearEn = false;
            //-----test			
			sqlCaseCounter.sqlCaseNum = 0;  //������������
            sqlCaseCounter.sqlCaseLineNum = 0;
			sqlCounter.sqlNum = 0;
            sqlCounter.sqlLineNum = 0;
            //-----test
			ShowSMessage("�������");			
		}
		/// <summary>
		/// //��������XML���,�������������
		/// </summary>
		public void SearchXmlNode(XmlNode  m_XmlNode, bool bFindNext)///////////////////////////////////
		{
			if(m_XmlNode == null)
				return;
			try
			{
				//Debug.Assert(m_XmlNode != null, "������XML���Ϊ��ֵ", "XmlTest.SearchXmlNode ����");
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
						SearchXmlNode(m_XmlNode.FirstChild, true);	//������ǰ�ڵ���ӽڵ�
						if(cTempSqlCase.stCaseResult.bSuccess != cTempSqlCase.stCaseResult.bExpResult)
						{
							stXmlRunInfo.tdTestThread .ShowFMessage("SQL_CASE��Ԥ�ڵ�ִ�н����ʵ�ʵ�ִ�н����һ�£�Ԥ�ڣ�" + cTempSqlCase.stCaseResult.bExpResult + "; ʵ�ʣ�" + cTempSqlCase.stCaseResult.bSuccess, true);
                            //------test
							sqlCaseCounter.sqlCaseLineNum = sqlCaseCounter.findLine(stXmlRunInfo.sXmlFileName,"<SQL_CASE>", sqlCaseCounter.sqlCaseNum);
							sqlCaseCounter.sqlCaseLineNum =sqlCaseCounter.sqlCaseLineNum -1;
							if(sqlCaseCounter.sqlCaseLineNum > 0)
							  stXmlRunInfo.tdTestThread .ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ������������ע�͵���һ��
								//stXmlRunInfo.tdTestThread .ShowFMessage("�� "+ CurrentLineNum + " �� " + "ִ�д���" ,true);
							//------test
							cSqlCase.stCaseResult.bSuccess = false;
							if(!ProClass.bValIsErrRun && stXmlRunInfo.bClearEn == false)//��������������������һ������ֵ
								stXmlRunInfo.bStop = true;
						}
						stXmlRunInfo.cCurrentSqlCase = stXmlRunInfo.cCurrentSqlCase.cParentCase;
					}
					else if(AnalyseNode(ref m_XmlNode))		//�������ؼ��֣���ִ��
					{
						SearchXmlNode(m_XmlNode.FirstChild, true);	//������ǰ�ڵ���ӽڵ�				
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
				ShowFMessage("�ڷ������ʱ���쳣��" + e.Message);
			}
		}

		private XmlNode ExeElseNode(XmlNode m_XmlNode)
		{
			while(m_XmlNode != null)
			{
				if (m_XmlNode.Name == "ELSE") 
				{
		//			ShowSMessage("ELSE����");
					SearchXmlNode(m_XmlNode.FirstChild, true);
		//			ShowSMessage("ELSE�˳�");
					break;
				}
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return m_XmlNode;
		}
		//������̷ֱ�ִ�нű����еĸ����ű�
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
				ShowFMessage("ĳ�ļ���·������Ѿ�������256���ַ�");
				return;
			}
			DirectoryInfo m_di = null;
			DirectoryInfo[] di = null;
			m_di = new DirectoryInfo(Proapp_dir);
			di = m_di.GetDirectories("DisposeTrans.exe");//���ص�ǰĿ¼����Ŀ¼
			if(di == null)
			{
				ShowFMessage("��Ӧ�ó���ǰĿ¼���Ҳ���DisposeTrans.exe�ļ�");
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
				+ "\" \"" + sPwd + "\" \"" + sDatabase + "\" \"" + sPort + "\" \"����:1\""; 
			cProcess.StartInfo.Arguments += " \"" + stXmlRunInfo.sXmlFileName + "\""; 
			try
			{				
				cProcess.Start();
				cProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				tCurrentTransStruct.cReadThread = new Thread(new ThreadStart(ReadTransProOutput));//����һ���߳�ί�������в��Ժ���
				tCurrentTransStruct.cReadThread.Priority = ThreadPriority.BelowNormal;
				tCurrentTransStruct.cReadThread.Start();
				while(tCurrentTransStruct != null)
					Thread.Sleep(0);
				cProcess.StandardInput.WriteLine(m_XmlNode.OuterXml.Replace("\r", " ").Replace("\n", " "));	//��ű�����д��ִ�нű�
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message+":��Ӧ�ó���ǰĿ¼���Ҳ���DisposeTrans.exe�ļ�");
			}
			cProcess.StandardInput.WriteLine("EXIT");
			cProcess.WaitForExit();	
		}
		/// <summary>
		/// //����߳�ִ�нű��е�����
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
				+ "\" \"" + sPwd + "\" \"" + sDatabase + "\" \"" + sPort + "\" \"����:1\""; 
			cProcess.StartInfo.Arguments += " \"" + stXmlRunInfo.sXmlFileName + "\""; 
			try
			{					
				cProcess.Start();
            //    ShowSMessage("��ʼ����DisposeTrans.exe");
				cProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				tCurrentTransStruct.cReadThread = new Thread(new ThreadStart(ReadTransProOutput));//����һ���߳�ί�������в��Ժ���
				tCurrentTransStruct.cReadThread.Priority = ThreadPriority.BelowNormal;
				tCurrentTransStruct.cReadThread.Start();
				while(tCurrentTransStruct != null)
					Thread.Sleep(0);
				cProcess.StandardInput.WriteLine(m_XmlNode.OuterXml.Replace("\r", " ").Replace("\n", " "));	//��ű�����д��ִ�нű�
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message+":��Ӧ�ó���ǰĿ¼���Ҳ���DisposeTrans.exe�ļ�");
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
		/// //������ִ��LOOPѭ��
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
				ShowFMessage("û���ҵ�LOOPִ�еĴ��������Ǵ������ַ����Ƿ�" + e.Message);
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
					ShowSMessage("������ִ��ѭ���еĽű�.....");
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
				ShowFMessage(sFileName+" �ļ������ڻ������Ǹ�Ŀ¼��");
			}
			else
			{
				vlc.SetVal("FILESIZE",(fi.Length).ToString ());
			}

		}
		/// <summary>
		/// //�����õ���ǰ����ֵ
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
		
		//���TYPE�ؼ��ֵ�ֵ�Ƿ�Ϸ�
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

			ShowFMessage("�ű��д��ڷǷ���Ԥ��ִ�н���ؼ���(����TYPE�ؼ��ְ������ֵ��ַ��������ѱ������)");
			return false;
		}
		/// <summary>
		/// //ͨ��ִ���������һ���ַ���
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
				ShowFMessage("���ʽ (" +ReplaceRunInfo(GetNodeText(m_XmlNode)) + ") �﷨��:" + err);
			}
			return ret;
		}
		/// <summary>
		/// //����������㣬���Ҹ��ݽڵ�ؼ��֣�������Ӧ�ĺ���ִ�нڵ�
		/// </summary>
		/// //ÿ����һ�η�����㣬�к� CurrentLineNum�ͼ�1���з�
		public bool AnalyseNode(ref XmlNode m_XmlNode)
		{
            CurrentLineNum++;   //��1
			Debug.Assert(m_XmlNode != null, "������XML���Ϊ��ֵ", "XmlTest.AnalyseNode ����");
			string strFromSql = "FromSql:";
			stXmlRunInfo.xCurrentNode = m_XmlNode;
			bool m_FindChild = false;						//������ʾ���ú������غ��Ƿ�Ҫ���������ӽڵ�
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
					testnum=GetNodeText(m_XmlNode);//�ѵ�ǰ��ִ�е��Ĳ��Ե�ĸ�����Ϣ�����ڱ����У������ڱ���ʱ�Ķ�λ
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
							ShowFMessage("�Ƿ���ִ�д���" + e.Message);
						}					
					}
					break;
				case "STARTTIMES":
					if(cLoop == null)
					{
						ShowFMessage("STARTTIMES�ؼ���ֻ����LOOP�ؼ�����ʹ��");				
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
						ShowFMessage("��ʾӰ�������ʱ��ʹ���˷Ƿ����ַ�����" + e.Message);
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
						ShowSMessage("������ʽ����һ�νű���������Ҫ�Ƚϳ���ʱ��.....");
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
						ShowFMessage("�Ƿ��������ַ�����ʾNetiveError," + e.Message);
					}
					break;
				case "CASEEXPRESULT":
					sValues = GetNodeText(m_XmlNode).Trim();				
					stXmlRunInfo.cCurrentSqlCase.stCaseResult.bExpResult = (string.Compare(sValues, "TRUE", true) == 0);
					break;
				case "COMPARERESULT":
					CompareResult(m_XmlNode);
					break;
				case "SERVERCMD":     //���������� �����������йص������ؼ��� SERVERCMD��RUNSERVER��REBOOT
					sValues = GetNodeText(m_XmlNode).Trim();
					sValues = ReplaceRunInfo(sValues);
					if(sValues.ToUpper()=="EXIT"||sValues.ToUpper()=="DEBUG")
					{
						//8.27 �޸� ����ͨ�ż���  ��������
						/*string ret3=ProClass.CommServer.ConnectSev();   //���ӵ����
						if(ret3 == "connect failed")
						{
							ShowFMessage("���ӷ������˲��Թ���serverʧ��");
							break;
						}
						else
						{
							ShowSMessage("���ӷ������˲��Թ���server�ɹ�");
						}*/
						string mess3="STOP_SERVER";
						string stop_ret;
						stop_ret=ProClass.CommServer.Send_Rec_Message(mess3);   //������Ϣ  ����������
						ShowSMessage("�����������������" + mess3 + "������ִ�н����" + stop_ret + "\n");
						
					}
					else
						ShowSMessage("�ű���SERVERCMD�����");
					//ProClass.cDServer.SendCommand(sValues);
					break;
				case "RUNSERVER":
					//8.27 �޸� ����ͨ�ż���  ��������
					/*string ret1=ProClass.CommServer.ConnectSev();   //���ӵ����
					if(ret1 == "connect failed")
					{
						ShowFMessage("���ӷ������˲��Թ���serverʧ��");
						break;
					}
					else
					{
						ShowSMessage("���ӷ������˲��Թ���server�ɹ�");
					}*/
					string mes="START_SERVER";
					string ret;
					ret=ProClass.CommServer.Send_Rec_Message(mes);   //������Ϣ  ����������

					ShowSMessage("�����������������" + mes + "������ִ�н����" + ret + "\n");
					//ProClass.CommServer.CloseStream();  //
					ProClass.cDServer.WaitServerStart();	
					ShowSMessage("����DM��������XML���߳�����������DM");
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
					ShowSMessage("׼��ִ�����  " + GetNodeText(m_XmlNode).Trim());
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
					//8.27 �޸� ����ͨ�ż���  ��������
					/*string ret2=ProClass.CommServer.ConnectSev();   //���ӵ����
					if(ret2 == "connect failed")
					{
						ShowFMessage("���ӷ������˲��Թ���serverʧ��");
						break;
					}
					else
					{
						ShowSMessage("���ӷ������˲��Թ���server�ɹ�");
					}*/
					string mess="RESTART_SERVER";
					string restart_ret;
					restart_ret=ProClass.CommServer.Send_Rec_Message(mess);   //������Ϣ  ����������
					ShowSMessage("�����������������" + mess + "������ִ�н����" + restart_ret + "\n");
					//ProClass.CommServer.CloseStream();

					ShowSMessage("����DM��������XML���߳�����������DM");
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
				case "INITDB":     //��ߵĴ�����δ�޸�
					sValues = GetNodeText(m_XmlNode).Trim();
					sValues = ReplaceRunInfo(sValues);
					/*if (!ProClass.cDServer.InitDb(sValues)) 
					{
						ShowFMessage("��ʼ����ʧ��");
					}*/
					//string initdb_con=ProClass.CommServer.ConnectSev();   //���ӵ����
					string mess_initdb="INIT_DB"+'$'+sValues+ '$' + xmlFileName;   //copy file
					string initdb_ret;
					initdb_ret=ProClass.CommServer.Send_Rec_Message(mess_initdb);   //������Ϣ  ����������
					//ProClass.CommServer.CloseStream();
					ShowSMessage("�����������������" + mess_initdb + "������ִ�н����" + initdb_ret + "\n");
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
						ShowFMessage("��ʾ������ʱ��ʹ���˷Ƿ����ַ�����" + e.Message);
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
						ShowFMessage("��ʾ������ʱ��ʹ���˷Ƿ����ַ�����" + e.Message);
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
						ShowFMessage("��ʾ������ʱ��ʱ��ʹ���˷Ƿ����ַ�����" + e.Message);
					}
					break;
				case "IGNORE":
					ShowSMessage("ע�⣺��������һ����Ҫ����ԵĽڵ�");
					break;
				case "BREAK":
					stXmlRunInfo.cCurrentSqlCase.stCaseResult.bBreak = true;					
		//			ShowSMessage("ע�⣺�����ű��е� BREAK ��㣬����ִ�������������ڵ�SQL_CASE�㷶Χ");
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
							ShowFMessage("��ʾ������ʱ��ʹ���˷Ƿ����ַ�����" + e.Message);
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
					ShowFMessage("RECORD�ؼ���ֻ�ܱ�������RESULT�ؼ��ֽڵ�����");
					break;
				case "COLUMN":
					ShowFMessage("COLUMN�ؼ���ֻ�ܱ�������RECORD�ؼ��ֽڵ�����");
					break;
				case "PARAMETER":
					if (stConnectINfo != null) {
						AddParameter(m_XmlNode);
					}
					else{
						ShowFMessage("��Ӳ�����ǰ���Ƚ�������");
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
						ShowFMessage("CLEARPARAMETERS������û������");
					}
					break;
				case "OPEN":
					if (stConnectINfo != null) {
						stConnectINfo.isOpenResult = true;
						stConnectINfo.iFetch = 0;
					}	
					else{
						ShowFMessage("OPEN������û������");
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
					ShowFMessage("�ùؼ���ֻ���ڶ��߳�ִ�нű�ʱʹ�ã��������MORETHREAD,TOGETHER�ؼ�����");
					break;
				case "EXIT":
					ShowFMessage("�ùؼ���ֻ���ڶ��߳�ִ�нű�ʱʹ�ã��������MORETHREAD,TOGETHER�ؼ�����");
					break;
				case "RESULTROWS":
					try
					{
						int iRows = Convert.ToInt32(GetNodeText(m_XmlNode).Trim());
						CheckResultRows(iRows);
					}
					catch(Exception e)
					{
						ShowFMessage("��ʾ�����������ʱ��ʹ���˷Ƿ����ַ�����" + e.Message);
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
							ShowFMessage("�Ƿ�������ţ���������������̷��ͽű�ʱ�����쳣��" + e.Message);
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
					ShowFMessage("�Ƿ���VALNAME�ڵ�ֵ����ֵӦ����һ����@��ͷ�����������");
					return;
				}
				valname = temp_name.Substring(1);
			}
			else
			{
				ShowFMessage("��BINARY�ڵ���δ�ҵ���VALNAME�ڵ�");
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
						ShowFMessage("�Ƿ���DATASIZEֵ��" + datasize);
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
				ShowFMessage("��BINARY�ڵ���δָ��DATASIZE�ڵ�");
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
				ShowFMessage("��BINARY�ڵ���δָ��SEED�ڵ�");
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
                Console.WriteLine("Ҫ�жϵ�IF���:" + sValues);
				sValues = ProClass.GetBoolVal(sValues, out err);
				if (err != "") {
					ShowFMessage("IF�жϱ��ʽ(" + sTemp + ") �﷨��" + err);
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
					ShowFMessage("IF�жϱ��ʽ(" + sTemp + ") �﷨��" + err);
				}
			}
									
			if (sValues == null) 
			{
				m_XmlNode = ExeElseNode(m_XmlNode);
			}
		}
		public bool CheckLevel2(XmlNode m_XmlNode)//�жϲ����������趨�Ĳ��Եȼ��Ƿ����Ҫ���ԵĲ��Եȼ�����������true�����򷵻�false
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
					ShowSMessage("�Ƿ��������ȼ����ȼ�ָ��ֻ����0-15֮��");
				}
			}
			if (level <= ProClass.iValLevel2)
			{
				return true;
			}
			ShowSMessage("�����ȼ����ڹ����趨�ı�׼���ýű�û�б�ִ��...");
			return false;
		}
		public bool CheckLevel1(XmlNode m_XmlNode)//�жϲ����������趨�Ĳ��Եȼ��Ƿ����Ҫ���ԵĲ��Եȼ�����������true�����򷵻�false
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
					ShowSMessage("�Ƿ��������ȼ����ȼ�ָ��ֻ����0-15֮��");
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
				ShowSMessage("�Ƿ��������ȼ����ȼ�ָ��ֻ����ѡ���ĵȼ�֮��");
			    return false;
			//ShowFMessage("�����ȼ����ڹ����趨�ı�׼���ýű�û�б�ִ��...");
			//return false;
			
		}
		public void FetchNext()
		{
			if (stConnectINfo == null || stConnectINfo.isOpenResult == false) {
				ShowFMessage("��Ѹò�������<OPEN>�ؼ����ڣ�");
				vlc.SetVal("FETCHNEXT", "0");
				return;
			}
			if(stConnectINfo.rd == null)
			{
				ShowFMessage("���������Ϊ�գ���һ��ִ�е����û�в�����Ӧ�Ľ������");
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
							sVal = Convert.ToString(stConnectINfo.rd[i]);		//�����е�ֵ����Ϊ�ַ����ٽ��бȽ�
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
				ShowFMessage("���ò���ֵʱ������鿴�趨��ֵ�ܷ�ת��Ϊ��Ӧ������:" + e.Message);
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

                        //		ShowSMessage("���߲�֧�ֶ�������Ϊ�������������δ������ֵ��ֻ�����˲������͵����ԣ�����������Ϊ�����������ô����Ķ����ƽ���ת��Ϊ�ַ�������ʾ");
						break;
					case "VARBINARY":
#if DM7
                        pr.DmSqlType = DmDbType.VarBinary;
#else
                        pr.OleDbType = OleDbType.VarBinary;
#endif

                        //		ShowSMessage("���߲�֧�ֶ�������Ϊ�������������δ������ֵ��ֻ�����˲������͵����ԣ�����������Ϊ�����������ô����Ķ����ƽ���ת��Ϊ�ַ�������ʾ");
						break;
					case "IMAGE":
#if DM7
                        pr.DmSqlType = DmDbType.Blob;
#else
                        pr.OleDbType = OleDbType.LongVarBinary;
#endif

                        //		ShowSMessage("���߲�֧�ֶ�������Ϊ�������������δ������ֵ��ֻ�����˲������͵����ԣ�����������Ϊ�����������ô����Ķ����ƽ���ת��Ϊ�ַ�������ʾ");
						break;
					case "BLOB":
#if DM7
                        pr.DmSqlType = DmDbType.Blob;
#else
                        pr.OleDbType = OleDbType.LongVarBinary;
#endif

                        //		ShowSMessage("���߲�֧�ֶ�������Ϊ�������������δ������ֵ��ֻ�����˲������͵����ԣ�����������Ϊ�����������ô����Ķ����ƽ���ת��Ϊ�ַ�������ʾ");
						break;
					case "LONGVARBINARY":
#if DM7
                        pr.DmSqlType = DmDbType.Blob;
#else
                        pr.OleDbType = OleDbType.LongVarBinary;
#endif

                        //		ShowSMessage("���߲�֧�ֶ�������Ϊ�������������δ������ֵ��ֻ�����˲������͵����ԣ�����������Ϊ�����������ô����Ķ����ƽ���ת��Ϊ�ַ�������ʾ");
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
						ShowSMessage("δ֪���������ͣ����߽���Ĭ�ϳ��ַ�����������");
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
				ShowFMessage("���ò���ֵʱ������鿴�趨��ֵ�ܷ�ת��Ϊ��Ӧ������:" + e.Message);
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
					ShowFMessage("�Ƿ��Ĳ������ͣ���������ֻ��Ϊ�������֣�(�������)IN, (�������)OUT, (�����������)IN OUT, (����ֵ����)RETURN TYPE");
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
					ShowFMessage("�Ƿ��Ĳ�����С:" + e.Message);
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
							ShowFMessage("����ӷ��������ʱ�������ò������ƣ��Ա��ȡִ���Ժ�ķ���ֵ");
							return;
						}
						sVal = null;
						break;
					case "IN OUT":
						pr.Direction = ParameterDirection.InputOutput;
						if (pr.ParameterName == "") 
						{
							ShowFMessage("����ӷ��������ʱ�������ò������ƣ��Ա��ȡִ���Ժ�ķ���ֵ");
							return;
						}
						break;
					case "RETURN TYPE":
						pr.Direction = ParameterDirection.ReturnValue;
						if (pr.ParameterName == "") 
						{
							ShowFMessage("����ӷ��������ʱ�������ò������ƣ��Ա��ȡִ���Ժ�ķ���ֵ");
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
				ShowFMessage("�����󶨲�����δ�ҵ�Ҫ�󶨵�VAL�ڵ�");
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
				ShowFMessage("����һ��������δ���ɽ����");
				return;
			}
			if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SUCCESS") 
			{
				showFail = true;
			}
			
			if (dr1.FieldCount != dr1.FieldCount) {
				if(showFail) 
				{
					ShowFMessage("�����������������һ��");
				}
				else if (!m_noShow) 
				{
					ShowSMessage("�����������������һ��");
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
							String mes = "�� " + row + " �� " + (col + 1) + " ��ֵ��һ��!��һ��ΪNULLֵ\n";
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
							String mes = "�� " + row + " �� " + (col + 1) + " ��ֵ��һ��!\n";
							mes += "���� " + _id1 + ": ";
							mes += binary1;
							mes += "���� " + _id2 + ": ";
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
						String mes = "�� " + row + " �� " + (col + 1) + " ��ֵ��һ��!\n";
						mes += "���� " + _id1 + ": ";
						if (r1 == System.DBNull.Value) {
							mes += "NULL\n";
						}
						else
						{
							mes += Convert.ToString(r1) + "\n";
						}
						mes += "���� " + _id2 + ": ";
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
					ShowFMessage("�����������������һ��");
				}
				else if (!m_noShow) {
					ShowSMessage("�����������������һ��");
				}
			}
		}
		public void CompareResult(XmlNode m_XmlNode)
		{
			int cn_id1;
			int cn_id2;
			XmlNode x_temp;
			if (ProClass.bValIsShowResult) {
				ShowFMessage("��������������������ʾ����������ܱ��򿪣��޷����н�����ԱȲ���");
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
					ShowFMessage("CONNECTID ֵ�Ƿ�:" + e.Message);
					return;
				}
			}
			else
			{
				ShowFMessage("�Ƿ��Ľ�����Ƚϲ�����δ��COMPARERESULT�ڵ����ҵ�CONNECTID�ڵ�");
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
					ShowFMessage("CONNECTID ֵ�Ƿ�:" + e.Message);
					return;
				}
			}
			else
			{
				ShowFMessage("�Ƿ��Ľ�����Ƚϲ�����δ��COMPARERESULT�ڵ����ҵ��ڶ���CONNECTID�ڵ�");
				return;
			}
			if (cn_id1<0 || cn_id1>=stConnectArry.Count) {
				ShowFMessage("CONNECTID ֵ�Ƿ�, Ӧ�ô��ڵ��� 0, С�� " + stConnectArry.Count + " ֮��" );
				return;
			}
			if (cn_id2<0 || cn_id2>=stConnectArry.Count) 
			{
				ShowFMessage("CONNECTID ֵ�Ƿ�, Ӧ�ô��ڵ��� 0, С�� " + stConnectArry.Count + " ֮��" );
				return;
			}
			if (cn_id2 == cn_id1) {
				ShowFMessage("������Ƚϲ����Ƿ���ָ�����������������ID����ͬ��");
				return;
			}
			try{
				CompareResultDo(cn_id1, cn_id2);
			}
			catch(Exception e)
			{
				ShowFMessage("������ȽϷ����쳣��" + e.Message);
			}			
		}
//		AddItem("<DELTEFILE>", "ɾ��һ���ļ�");
//		AddItem("<COPYFILE>", "��һ���ļ�");
//		AddItem("<OLDFILE>", "���ļ�");
//		AddItem("<NEWFILE>", "���ļ���");
//		AddItem("<CREATEFILE>", "����һ���ļ�");
//		AddItem("<FILENAME>", "�ļ���");
//		AddItem("<WRITEFLAG>", "�ļ��������");//Coverage, Additional
//		AddItem("<WRITEFLAG>", "�ļ��������");//Coverage, Additional
		//SETVAL
		//GETVAL
		//VALNAME
		//SETAT
		//VAL
		public string GetServerPath()
		{
			//�޸ĳɷ���Ϣ��Զ�̷����� �÷������Ǳ��������ļ�
			//8.27 �޸� ����ͨ�ż���  ��������
			/*string create_con=ProClass.CommServer.ConnectSev();   //���ӵ����
			if(create_con == "connect failed")
				ShowFMessage("���ӷ������˲��Թ���serverʧ��");
			else
				ShowSMessage("���ӷ������˲��Թ���server�ɹ�");*/

			//8.27 �޸� ����Ҫ�ⲽ����  ���Թ��߲���Ҫ֪������������˴��η�������·��������
			string messserver="SERVERPATH";    //��װ��Ҫ���͵���Ϣ��ʽ
			string ser_ret=ProClass.CommServer.Send_Rec_Message(messserver);   //������Ϣ 
			ShowSMessage("�����������������" + messserver + "������ִ�н����" + ser_ret + "\n");
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
					ShowFMessage("�Ƿ����ļ�����������δ��CREATEFILE�ڵ����ҵ�FILENAME�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("�Ƿ����ļ�����������δ��CREATEFILE�ڵ����ҵ�FILENAME�ڵ�");
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
				ShowFMessage("���ӷ������˲��Թ���serverʧ��");
			else
				ShowSMessage("���ӷ������˲��Թ���server�ɹ�");*/

			string mess3="CREATE_FILE"+'$'+name+'$'+flag+'$'+val;    
			string ret=ProClass.CommServer.Send_Rec_Message(mess3);  
			//ProClass.CommServer.CloseStream();
			ShowSMessage("�����������������" + mess3 + "������ִ�н����" + ret + "\n");

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
					ShowFMessage("δָ��Ҫ��ȡ���ļ���������FILENAME�ڵ�ָ��");
					return;
				}
			}
			else
			{
				ShowFMessage("δָ��FILENAME�ڵ�");
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
					ShowFMessage("�Ƿ���VALNAME�ڵ�");
					return;
				}
			}
			else
			{
				ShowFMessage("δָ��VALNAME�ڵ�");
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
					ShowFMessage("�Ƿ���DATAFLAG�ڵ�ֵ");
					return;
				}
			}
			else
			{
				ShowFMessage("δָ��DATAFLAG�ڵ�");
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
					ShowFMessage("�Ƿ���DATAFLAG�ڵ�ֵ");
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
						ShowFMessage("�Ƿ���DATA1�ڵ�ֵ����ֵӦ����һ�����������������");
						return;
					}
					binary1 = vlc.GetVal(temp_name.Substring(1));
					if(binary1 == null)
					{
						ShowFMessage("�Ƿ���DATA1�ڵ�ֵ��δ�ҵ������������");
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
				ShowFMessage("δָ��DATA1�ڵ�");
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
						ShowFMessage("�Ƿ���DATA2�ڵ�ֵ����ֵӦ����һ�����������������");
						return;
					}
					binary2 = vlc.GetVal(temp_name.Substring(1));
					if(binary2 == null)
					{
						ShowFMessage("�Ƿ���DATA2�ڵ�ֵ��δ�ҵ������������");
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
				ShowFMessage("δָ��DATA2�ڵ�");
				return;
			}
			
			if(is_binary)
			{
				if(BinaryCompare(binary1, binary2) == false)
				{
					ShowFMessage("���������ƴ����ݲ�һ��");
				}
			}
			else
			{
				try
				{
					if(string.Compare((string)binary1, (string)binary2, false) != 0)
					{
						ShowFMessage("�����ַ������ݲ�һ��");
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
		public void DeleFile(string sFileName)  //���޸�ͨ��
		{
			try
			{
				sFileName = ReplaceRunInfo(sFileName);
				//8.27 �޸� ����ͨ�ż���  ��������
				/*string create_con=ProClass.CommServer.ConnectSev();   //���ӵ����
				if(create_con == "connect failed")
					ShowFMessage("���ӷ������˲��Թ���serverʧ��");
				else
					ShowSMessage("���ӷ������˲��Թ���server�ɹ�");*/

				string mess3="DELETE_FILE"+'$'+sFileName;   //delete file
				string ret=ProClass.CommServer.Send_Rec_Message(mess3);   //������Ϣ  ����������
				//ProClass.CommServer.CloseStream();
                ShowSMessage("�����������������" + mess3 + "������ִ�н����" + ret + "\n");
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
					ShowFMessage("�Ƿ����ļ�����������δ��COPYFILE�ڵ����ҵ�OLDFILE�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("�Ƿ����ļ�����������δ��COPYFILE�ڵ����ҵ�OLDFILE�ڵ�");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "NEWFILE");
			if (x_temp != null) 
			{
				sNewFile = GetNodeText(x_temp).Trim();
				sNewFile = ReplaceRunInfo(sNewFile);
				if (sNewFile.Length == 0) 
				{
					ShowFMessage("�Ƿ����ļ�����������δ��COPYFILE�ڵ����ҵ�NEWFILE�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("�Ƿ����ļ�����������δ��COPYFILE�ڵ����ҵ�NEWFILE�ڵ�");
				return;
			}
			try
			{
				//8.27 �޸� ����ͨ�ż���  ��������
				/*string create_con=ProClass.CommServer.ConnectSev();   //���ӵ����
				if(create_con == "connect failed")
					ShowFMessage("���ӷ������˲��Թ���serverʧ��");
				else
					ShowSMessage("���ӷ������˲��Թ���server�ɹ�");*/

				string mess4="COPY_FILE"+'$'+sOldFile+'$'+sNewFile;   //copy file
				string ret=ProClass.CommServer.Send_Rec_Message(mess4);   //������Ϣ  ����������
				//ProClass.CommServer.CloseStream();
				
				/*if(ret == "fail")
					ShowFMessage("�����ļ�ʧ��!");
				else*/
					ShowSMessage("�����������������" + mess4 + "������ִ�н����" + ret + "\n");
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
		public void ModifyIni(XmlNode m_XmlNode)  //ͨ�� ����˴�����
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
					ShowFMessage("δ��SETDMINI�ڵ����ҵ�VALNAME�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("δ��SETDMINI�ڵ����ҵ�VALNAME�ڵ�");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				if (sVal.Length == 0) 
				{
					ShowFMessage("δ��SETDMINI�ڵ����ҵ�VAL�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("δ��SETDMINI�ڵ����ҵ�VAL�ڵ�");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "INIPATH");
			if (x_temp != null)      //���ָ����·��  �޸ĺ�ű�����ָ��·��������������
			{
				sPath = GetNodeText(x_temp).Trim();
				sPath = ReplaceRunInfo(sPath);
				if (sPath.Length == 0) 
				{
					ShowFMessage("δ��SETDMINI�ڵ����ҵ�INIPATH�ڵ��������");
					return;
				}
			}
			else  //��û��ָ��·��
			{
				sPath = ProClass.cDServer.GetIniPath();

			}

			/*string create_con=ProClass.CommServer.ConnectSev();   //���ӵ����
			if(create_con == "connect failed")
				ShowFMessage("���ӷ������˲��Թ���serverʧ��");
			else
				ShowSMessage("���ӷ������˲��Թ���server�ɹ�");*/

			string messm="MODIFY_INIFILE"+'$'+sPath+'$'+sName+'$'+sVal;    //��װ��Ҫ���͵���Ϣ��ʽ
			string ret=ProClass.CommServer.Send_Rec_Message(messm);   //������Ϣ  
			//ProClass.CommServer.CloseStream();
			/*
			if(ret == "NOT_FOUND" || ret == null)
			{
				ShowFMessage("dm.ini��û���ҵ�����"+sName+"!");
				return;
			}

			if(ret == "fail")
				ShowFMessage(sVal + "����ֵ����ʧ��!");
			else
				ShowSMessage(messm + ret);*/
			ShowSMessage("�����������������" + messm + "������ִ�н����" + ret + "\n");
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
					ShowFMessage("δ��GETDMINI�ڵ����ҵ�VALNAME�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("δ��GETDMINI�ڵ����ҵ�VALNAME�ڵ�");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				if (!sVal.StartsWith("@")) 
				{
					ShowFMessage("VAL�ڵ���Ӧ��ָ�����Ǳ��������ֵ��һ���������������һ����ͨ���ַ���");
					return;
				}
			}
			else
			{
				ShowFMessage("δ��GETDMINI�ڵ����ҵ�VAL�ڵ�");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "INIPATH");
			if (x_temp != null)  //ͬ�� ����ָ��·������������
			{
				sPath = GetNodeText(x_temp).Trim();
				sPath = ReplaceRunInfo(sPath);
				if (sPath.Length == 0) 
				{
					ShowFMessage("δ��GETDMINI�ڵ����ҵ�INIPATH�ڵ��������");  
					return;
				}
			}
			else
			{
				sPath = ProClass.cDServer.GetIniPath();
			}


			//�����д��Ľ�
//			ret = DoFileClass.GetVal(sPath, sName);   //�޸���GetVal   �ø�ֵȥ����@SQLSTR ������ã���@SQLSTRΪ����
				ProClass.sValGetDmIni="0";

			/*string create_con=ProClass.CommServer.ConnectSev();
			if(create_con == "connect failed")
				ShowFMessage("���ӷ������˲��Թ���serverʧ��");
			else
				ShowSMessage("���ӷ������˲��Թ���server�ɹ�");*/

			string messr="READ_INIFILE"+'$'+sPath+'$'+sName;   
			string ret=ProClass.CommServer.Send_Rec_Message(messr);   
			//ProClass.CommServer.CloseStream();

            /*if(ret == "NOT_FOUND" || ret == null)
				ShowFMessage("dm.ini��û���ҵ�����"+sName+"!");
			
			if(ret == "fail")
				ShowFMessage(sVal + "����ֵ����ʧ��!");*/
			ShowSMessage("�����������������" + messr + "������ִ�н����" + ret + "\n");
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
					ShowFMessage("δ��GETVAL�ڵ����ҵ�VALNAME�ڵ��������");
					return;
				}
			}
			else{
				ShowFMessage("δ��GETVAL�ڵ����ҵ�VALNAME�ڵ�");
				return;
			}
			x_temp = FindXmlNode(m_XmlNode.FirstChild, "VAL");
			if (x_temp != null) 
			{
				sVal = GetNodeText(x_temp).Trim();
				if (!sVal.StartsWith("@")) 
				{
					ShowFMessage("VAL�ڵ���Ӧ��ָ�����Ǳ��������ֵ��һ���������������һ����ͨ���ַ���");
					return;
				}
				string sTemp = ProClass.GetSaveProValue(sName);
				if (sTemp == null) {
					ShowFMessage("δ�ҵ����ԣ�" + sName);
				}
				else
					vlc.SetVal(sVal.Substring(1), sTemp);							
			}
			else
			{
				ShowFMessage("δ��GETVAL�ڵ����ҵ�SETAT�ڵ�");
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
					ShowFMessage("δ��SETVAL�ڵ����ҵ�VALNAME�ڵ��������");
					return;
				}
			}
			else
			{
				ShowFMessage("δ��SETVAL�ڵ����ҵ�VALNAME�ڵ�");
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
				ShowFMessage("δ��SETVAL�ڵ����ҵ�VAL�ڵ�");
			}
		}
		//�ú������������������
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
					ShowSMessage("�������ID��" + i + "��δ��˳���Ľ���������ͨ�����͹ر���Ϣ��������");
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
		//�ú���������ָ����������̷���ִ�еĽű�
		public void SendXmlStringToTrans(int iID, string sXml)
		{
			if(iID < 0 || iID >= alTransPro.Count)
			{
				ShowFMessage("ָ��������ų���Χ����ȷ������ŷ�ΧӦ���� 0 - " + alTransPro.Count + " ֮�䣻����ǰҪָ���������Ϊ��" + iID);
				return;
			}
			if(stXmlRunInfo.xCurrentNode == null)
			{
				ShowFMessage("��ǰ��XML�ڵ��ǿ�ֵ");
				return;
			}
			TRANS_STRUCT tTransStruce = (TRANS_STRUCT)alTransPro[iID];
			tCurrentTransStruct = tTransStruce;
			Process cProcess = tTransStruce.cProcess;
			if(cProcess.HasExited)
			{
				ShowFMessage("��������Ѿ���ֹ�ˣ��������������ͽű�����");
				return;
			}
			if(sXml == null || sXml == "")//���δָ�����͵����ݣ���ô���͵�ǰ�ڵ�ĽǱ�
				sXml = stXmlRunInfo.xCurrentNode.OuterXml;

			int i=0;
			while(tTransStruce.bIsFinishExcute == false&&i++<200)//˯��һ�룬ȷ����һ�η��͵Ľű��г�ֵ�ʱ�䱻����
			{
				Thread.Sleep(10);	
			}
			if(tTransStruce.bIsFinishExcute)
			{
				tTransStruce.bIsFinishExcute = false;
				cProcess.StandardInput.WriteLine(sXml.Replace("\r", " ").Replace("\n", " "));	//��ű�����д��ִ�нű�
				i=0;
				while(tTransStruce.bIsFinishExcute == false&&i++<300)//˯��һ�룬ȷ����һ�η��͵Ľű��г�ֵ�ʱ�䱻����
				{
					Thread.Sleep(10);	
				}
				i++;				
			}
			else
			{				
				ShowFMessage("����ID�ţ�" + iID + "�� ��һ����������͵Ľű���û�еõ�ִ�гɹ��Ļ�Ӧ\n, �������ű�ȷʵ��Ҫ����10���ʱ��ִ�У���ô���ڷ��ͽű������ű�������˯��ʱ�䣡");
				cProcess.CloseMainWindow();
			}
		}
		public bool CheckEffectRows(int iExcRows)
		{
			
			if(stConnectINfo == null || stConnectINfo.rd == null)
			{
				ShowFMessage("��һ��ִ�е����û�����ɼ�¼����");
				return false;
			}
			if(stConnectINfo.rd.FieldCount == 0)
			{
				if(iExcRows != stConnectINfo.rd.RecordsAffected)
				{
					ShowFMessage("���ִ��Ӱ�������Ԥ�ڵ�ֵ��һ�£�\n" + "Ԥ��ֵΪ:" + iExcRows + "\nʵ�ʷ���ֵ��" + stConnectINfo.rd.RecordsAffected);
					return false;
				}
			}
			else
			{
				if(stConnectINfo.rd.HasRows)
				{
					if(iExcRows == 0)
					{
						ShowFMessage("Ҫ�󷵻�һ���ս������ʵ���Ϸ��صĽ������Ϊ��");
						return false;
					}
				}
				else
				{
					if(iExcRows != 0)
					{
						ShowFMessage("Ҫ�󷵻�һ���ǿս������ʵ���Ϸ��صĽ����Ϊ��");
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
				ShowFMessage("��һ��ִ�е����û�����ɼ�¼����");
				return false;
			}
			if(stConnectINfo.rd.FieldCount == 0)
			{
				if(iExcRows != stConnectINfo.rd.RecordsAffected)
				{
					ShowFMessage("���ִ��Ӱ�������Ԥ�ڵ�ֵ��һ�£�\n" + "Ԥ��ֵΪ:" + iExcRows + "\nʵ�ʷ���ֵ��" + stConnectINfo.rd.RecordsAffected);
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
						ShowFMessage("��������������������ʾ����������ܱ��򿪣��޷����н��������ͳ�Ʋ���");
						return false;
					}
					while(stConnectINfo.rd.Read())
					{
						rows++;
					}
				}
				if(rows != iExcRows)
				{
					ShowFMessage("�����������Ԥ�ڵ�������һ�£�\n" + "Ԥ��ֵΪ:" + iExcRows + "\nʵ�ʷ���ֵ��" + rows);
					return false;
				}
			}
			return true;
		}
		//��ʼ��ǰ�����ϵ�����
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
						ShowFMessage("��ָ������ĸ��뼶��ʹ��ʹ����ADOδ����ļ���" + sOp);
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
						ShowFMessage("��ָ������ĸ��뼶��ʹ��ʹ����ADOδ����ļ���" + sOp);
						return;
					}
				}
#endif
				
				stConnectINfo.alTrArray.Add(stConnectINfo.tr);
				stConnectINfo.cm.Transaction = stConnectINfo.tr;			
			}
			catch(Exception e)
			{
				ShowFMessage("����������ʱ�����ӷ������쳣��\n" + e.Message);
			}
		}
		//������ǰ�����ϵ�����
		public void EndTrans(string sOp)
		{
			if (stConnectINfo == null) {
				ShowFMessage("����������û������");
				return;
			}
			ShowSMessage("ENDTRANS:" + sOp);
			if(stConnectINfo.alTrArray.Count == 0)
			{
				ShowFMessage("�����������в�������������ڵ�ǰ�������㻹û�п�ʼ��������Ѿ��ر�����������");
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
				ShowFMessage("�������������еõ�����������ǿ�ֵ");
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
				ShowFMessage("���������ʱ�����ӷ������쳣��\n" + e.Message);
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

		//�����µ��߳�ִ�к������ú���������������Ӧ����������е�����������
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
                 //   ShowSMessage("����DisposeTrans.exeִ�гɹ�!");

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

		//��������һ���µĽ��̣��ý���������ִ������ű���
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
				+ "\" \"" + sPwd + "\" \"" + sDatabase + "\" \"" + sPort + "\" \"����:" + alTransPro.Count; 
			tCurrentTransStruct.cProcess.StartInfo.Arguments += "\" \"" + stXmlRunInfo.sXmlFileName + "\""; 
			
			try
			{
				tCurrentTransStruct.cProcess.Start();
				tCurrentTransStruct.cProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
				
				ShowSMessage("�´�����һ���������ס����ID��Ϊ��" + alTransPro.Count);
				alTransPro.Add(tCurrentTransStruct);
				Thread cThread = new Thread(new ThreadStart(ReadTransProOutput));//����һ���߳�ί�������в��Ժ���

				cThread.Start();
				//����ȡ��ǰ������������̵��߳����������Ժ󣬻��tCurrentTransStruct������Ϊ��
				while(tCurrentTransStruct != null)//�ȴ������Ӧ���̶߳�ȡ��ǰ�Ľṹ����ʽ��������
				{
					Thread.Sleep(0);
				}
			}
			catch(Exception e)
			{
				ShowFMessage(e.Message+":��Ӧ�ó���ǰĿ¼���Ҳ���DisposeTrans.exe�ļ�");
			}
		}
		/// <summary>
		/// //ִ��ָ����EXE�ļ�
		/// </summary>
		public void ExecuteProcessEx(string m_PrName)
		{
			string m_tempStr = "";
			string par = ""; //����exe�Ĳ���
			int pot;
			m_PrName = m_PrName.Trim();
			ShowSMessage("��ʼ���ÿ�ִ���ļ���" + m_PrName);
			pot=m_PrName.IndexOf(" ");
			if(pot>0) 
			{
				par=m_PrName.Substring(pot+1,m_PrName.Length-pot-1); //���������������
				m_PrName=m_PrName.Substring(0,pot);
				par = par.Trim();
			}
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("��ǰ�ļ�·���Ƿ���δ����ִ���ļ���" + m_PrName);
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
			ShowSMessage("�������ÿ�ִ���ļ���" + m_PrName);
			m_Process.Close();
			m_Process.Dispose();
			m_Process = null;
		}
		/// <summary>
		/// //ִ��ָ����EXE�ļ�
		/// </summary>
		public void ExecuteProcess(string m_PrName)
		{
/*			ShowSMessage("��ʼ���ÿ�ִ���ļ���" + m_PrName);
			string m_tempStr = "";
			string par = ""; //����exe�Ĳ���
			int pot;
			m_PrName = m_PrName.Trim();
			pot=m_PrName.IndexOf(" ");
			if(pot>0) 
			{
				par=m_PrName.Substring(pot+1,m_PrName.Length-pot-1); //���������������
				m_PrName=m_PrName.Substring(0,pot);
				par = par.Trim();
			}
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("��ǰ�ļ�·���Ƿ���δ����ִ���ļ���" + m_PrName);
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
			string par = ""; //����exe�Ĳ���
			int pot;
			m_PrName = m_PrName.Trim();
			ShowSMessage("��ʼ���ÿ�ִ���ļ���" + m_PrName);
			pot=m_PrName.IndexOf(" ");
			if(pot>0) 
			{
				par=m_PrName.Substring(pot+1,m_PrName.Length-pot-1); //���������������
				m_PrName=m_PrName.Substring(0,pot);
				par = par.Trim();
			}
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("��ǰ�ļ�·���Ƿ���δ����ִ���ļ���" + m_PrName);
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
			//��Ԥ�ڽ�����д���
			if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_FAIL")
			{
                //	if(m_Process.ExitCode == 1)
                if (m_Process.ExitCode == 0)                    //�޸�2016-10-31  �Ž�ҵ
                {
					ShowFMessage("��ִ���ļ���" + m_PrName + " ����ִ�гɹ�");
				}
				else
				{
					ShowSMessage("��ִ���ļ���" + m_PrName + " ִ�н��ʧ��");
				}
			} 
			if (stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SUCCESS")
			{
                //		if(m_Process.ExitCode == 1)
                if (m_Process.ExitCode == 0)              //�޸�2016-10-31  �Ž�ҵ
                {
					ShowSMessage("��ִ���ļ���" + m_PrName + " ����ִ�гɹ�");
				}
				else
				{
					ShowFMessage("��ִ���ļ���" + m_PrName + " ִ�н��ʧ��");
				}
			}
			ShowSMessage("�������ÿ�ִ���ļ���" + m_PrName);
			m_Process.Close();
			m_Process = null;
		}
		/// <summary>
		/// //�½�һ�����ӣ�����ִ��ָ������ڵ�����
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
		//����ִ���Ժ���������ֵ
		private void SaveParameterValue()
		{
			for (int i=0; i<stConnectINfo.cm.Parameters.Count; i++) {
				if (stConnectINfo.cm.Parameters[i].Direction != ParameterDirection.Input) {
					try
					{
						if (stConnectINfo.cm.Parameters[i].Value == null) {
							ShowFMessage("�����������ֵ" + stConnectINfo.cm.Parameters[i].ParameterName + "����û�з������ֵ");
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
						ShowFMessage("�����������ֵ" + stConnectINfo.cm.Parameters[i].ParameterName + "����" + e.Message);
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
		/// //����ִ��SQL���
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
				stXmlRunInfo.tdTestThread .PushSqlCase(m_Sql);	//��SQL���ѹ������
				if (!m_noShow) 
				{
					ShowSMessage(m_Sql);
				}
				stConnectINfo.isOpenResult = false;				
				if (CheckParameterEnableResult()) {
					time1 = new DateTime(DateTime.Now.Ticks);//Ϊִ������ʱ
					stConnectINfo.rd = stConnectINfo.cm.ExecuteReader();
					vlc.SetVal("EFFECTROWS", stConnectINfo.rd.RecordsAffected.ToString());
				}
				else{
					time1 = new DateTime(DateTime.Now.Ticks);//Ϊִ������ʱ
					stConnectINfo.cm.ExecuteNonQuery();
				}				
				time2 = new DateTime(DateTime.Now.Ticks);
				ts = new TimeSpan(time2.Ticks-time1.Ticks) ;//ȡ��ִ�����ĵ�ʱ��
				usedtimes =ts.Hours*((Int64)60000*60) + ((Int64)ts.Minutes)*60000+ts.Seconds*1000+ts.Milliseconds;
				vlc.SetVal("USEDTIMES", usedtimes.ToString());
				SaveParameterValue();
				if(ProClass.bValIsOutTime && !m_noShow)
				{						
					ShowSMessage("ִ�б����������" + Convert.ToString(usedtimes) + "���롣");//��ʾʱ����
				}				
			}
#if DM7
            catch (DmException e)
#else
			catch (OleDbException e)
#endif
            {
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult != "DIRECT_EXECUTE_FAIL")
					stXmlRunInfo.tdTestThread .SaveSqlCase();				//�����ִ�й���SQL����ֳ�				
				
				return CheckExecute(false, m_Sql, e);
			}
			catch(Exception e)
			{
				ShowFMessage("���ִ�й����з����쳣��\n" + e.Message);
				//-----test	
				sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
				if(sqlCounter.sqlLineNum > 0)
				    ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ��
				//-----test
				return false;
			}
			return CheckExecute(true, m_Sql, null);
		}
		/// <summary>
		/// //����һ�е�ֵ
		/// </summary>
		public string GetColumn(XmlNode m_XmlNode)
		{
			Debug.Assert(m_XmlNode != null, "�������н��Ϊ��ֵ", "XmlTest.GetColumn ����");
			m_XmlNode = SkipComment(m_XmlNode);
			if(m_XmlNode == null)
			{
				return null;
			}
			if(m_XmlNode.Name != "COLUMN")
			{
				ShowFMessage("�����������Ʋ��Ǵ����������еĽ�㣬�ý������Ϊ:" + m_XmlNode.Name);
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
		/// //���������ÿһ��
		/// </summary>
		public bool CheckResultRow(XmlNode m_XmlNode)
		{
			m_XmlNode = SkipComment(m_XmlNode);
			if(m_XmlNode == null)
			{
				if(stConnectINfo.rd.HasRows)
				{
					ShowFMessage("Ԥ����䷵��һ���յĽ����������ʵ�ʷ�����һ���ǿս����");
					return false;
				}
				else
				{
					return true;
				}
			}
			if(m_XmlNode.Name != "RECORD")
			{
				ShowFMessage("�������ʼ�е����Ʋ��Ǵ����������еĽ�㣬�ý������Ϊ:" + m_XmlNode.Name);
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
					ShowFMessage("�Ƿ����������ʽ��"+e.Message);
					return false;
				}
				if (m_colNums<=0) 
				{
					ShowFMessage("�Ƿ�������ֵ������Ӧ�ô��ڵ���1");
					return false;
				}
				if (stConnectINfo.rd.FieldCount != m_colNums) 
				{
					ShowFMessage("������е��������ű��е�������һ�£�������е�����Ϊ:" + stConnectINfo.rd.FieldCount);
					return false;
				}
			}
			XmlNode m_tempNode = SkipComment(m_XmlNode.FirstChild);//����ע�Ͳ���
			if(m_tempNode != null)
			{
				if(!stConnectINfo.rd.HasRows)
				{
					ShowFMessage("Ԥ����䷵��һ���ǿյĽ����������ʵ�ʷ�����һ���ս����");
					return false;
				}
			}
			int m_row = 1;//��ʾ��ǰ������			
			while(m_XmlNode != null)				//��ʼ�ȽϽ����
			{
				//���ǽ�������п�ʼ
				vlc.SetVal("RECORDNUMS", m_row.ToString());
				if(!stConnectINfo.rd.Read())
				{
					ShowFMessage("������Ѿ�����δβ������XML�ļ��л���δ�Ƚϵ���");
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
				ShowFMessage("�����δ����δβ������XML�ļ����Ѿ�û���˿��ԱȽϵ���");
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
					ShowFMessage("Ԥ����䷵��һ���յĽ����������ʵ�ʷ�����һ���ǿս����");
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
				ShowFMessage("δ�ҵ�RECORD��");
				return false;
			}
			if (FindXmlNode(m_XmlNode.NextSibling, "RECORD") != null) 
			{
				ShowFMessage("��ʹ��RECORDNUMS�ؼ���ʱ���������ֻ�ܰ���һ��RECORD��");
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
					ShowFMessage("�Ƿ����������ʽ��"+e.Message);
					return false;
				}
				if (m_colNums<=0) 
				{
					ShowFMessage("�Ƿ�������ֵ������Ӧ�ô��ڵ���1");
					return false;
				}
				if (stConnectINfo.rd.FieldCount != m_colNums) 
				{
					ShowFMessage("������е��������ű��е�������һ�£�������е�����Ϊ:" + stConnectINfo.rd.FieldCount);
					return false;
				}
			}
			int m_row = 1;//��ʾ��ǰ������
			while(m_row <= rownums)				//��ʼ�ȽϽ����
			{
				//���ǽ�������п�ʼ
				if(!stConnectINfo.rd.Read())
				{
					ShowFMessage("������Ѿ�����δβ������XML�ļ��л���δ�Ƚϵ���");
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
				ShowFMessage("�����δ����δβ������XML�ļ����Ѿ�û���˿��ԱȽϵ���");
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
				//���ǽ������ÿһ�п�ʼ
				if(m_col >= stConnectINfo.rd.FieldCount)
				{
					ShowFMessage("XML�ļ��е����������˷��ؽ�����е�����");
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
						RetCol = Convert.ToString(stConnectINfo.rd[m_col]);		//�����е�ֵ����Ϊ�ַ����ٽ��бȽ�
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
					errorMessage += "������ڵ�" + (m_row) + "�У���" + (m_col + 1) +"�д�,";
					errorMessage += e.Message;
					ShowFMessage(errorMessage);
					return false;
				}
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT")		//ֻ�Ƚ�XML�ļ��и���ֵ���Ȳ���
				{
					if(String.Compare(ExpCol, 0, RetCol, 0, ExpCol.Length, false) != 0)
					{
						errorMessage += "������ڵ�" + (m_row) + "�У���" + (m_col + 1) +"�д��������һ��\n";
						errorMessage += "Ԥ��ֵ: " + ExpCol;
						errorMessage += "\n����ֵ: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				else//���ֵ�����Ƚ�
				{
					if(ExpCol != RetCol)
					{
						errorMessage += "������ڵ�" + (m_row) + "�У���" + (m_col + 1) +"�д��������һ��\n";
						errorMessage += "Ԥ��ֵ: " + ExpCol;
						errorMessage += "\n����ֵ: " + RetCol;
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
				ShowFMessage("δ����COMUMN");
				return false;
			}
			if (FindXmlNode(m_colNode.NextSibling, "COLUMN") != null) 
			{
				ShowFMessage("��ʹ����COLUMNNUMS�ؼ��ֵ�����£�ֻ��ָ��һ��COLUMN��");
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
						RetCol = Convert.ToString(stConnectINfo.rd[m_col]);		//�����е�ֵ����Ϊ�ַ����ٽ��бȽ�
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
					errorMessage += "������ڵ�" + (m_row) + "�У���" + (m_col + 1) +"�д�,";
					errorMessage += e.Message;
					ShowFMessage(errorMessage);
					return false;
				}
				if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT")		//ֻ�Ƚ�XML�ļ��и���ֵ���Ȳ���
				{
					if(String.Compare(ExpCol, 0, RetCol, 0, ExpCol.Length, false) != 0)
					{
						errorMessage += "������ڵ�" + (m_row) + "�У���" + (m_col + 1) +"�д��������һ��\n";
						errorMessage += "Ԥ��ֵ: " + ExpCol;
						errorMessage += "\n����ֵ: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				else//���ֵ�����Ƚ�
				{
					if(ExpCol != RetCol)
					{
						errorMessage += "������ڵ�" + (m_row) + "�У���" + (m_col + 1) +"�д��������һ��\n";
						errorMessage += "Ԥ��ֵ: " + ExpCol;
						errorMessage += "\n����ֵ: " + RetCol;
						ShowFMessage(errorMessage);
						return false;
					}
				}
				m_col++;	
			}
			return true;
		}
		/// <summary>
		/// ������ת��Ϊʮ�������ַ���
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		private static string ByteArrayToHexString(byte[] bytes)
		{
			if ( bytes == null )
				throw new ArgumentException( "bytes [] ��������" );
			StringBuilder hexString = new StringBuilder( 2 * bytes.Length );

			for ( int i = 0; i < bytes.Length; i++ )
				hexString.AppendFormat( "{0:X2}", bytes[i] );
			return hexString.ToString();
		}


		/// <summary>
		/// //��������ע�ͽ��
		/// </summary>
		public XmlNode SkipComment(XmlNode m_XmlNode)//����ע�ͽڵ�
		{
			if(m_XmlNode == null)
				return null;
			while(m_XmlNode != null && (m_XmlNode.Name == "#comment"))
			{
				/*int index1;
				int index2;
				string testtmpnum=m_XmlNode.Value ;//��ȡע�͵�����
                index1=testtmpnum.IndexOf ("���Ե�");
				index2=testtmpnum.IndexOf ("����");
				if(index1>0&&index2<0)
					testnum=testtmpnum.Substring (index1,8);*/
				m_XmlNode = m_XmlNode.NextSibling;
			}
			return m_XmlNode;
		}
		/// <summary>
		/// //���ִ�еĽ��
		/// </summary>
		public bool CheckResult(bool bCheckColName)
		{
			if(stConnectINfo.rd == null)
			{
				ShowFMessage("���������Ϊ�գ���һ��ִ�е����û�в�����Ӧ�Ľ������");
				return false;
			}
			Debug.Assert(stXmlRunInfo.xCurrentNode != null, "�ڼ������ʱ�������ĵ�ǰ�������ֵΪ��", "XmlTest.CheckResult ����");
			if(stXmlRunInfo.xCurrentNode == null)
			{
				ShowFMessage("�ڼ������ʱ, ��ǰ���е�XML�������Ϊ�գ�û�н�㱻����");
				return false;
			}
			stXmlRunInfo.xCurrentNode = stXmlRunInfo.xCurrentNode.NextSibling;
			stXmlRunInfo.xCurrentNode = SkipComment(stXmlRunInfo.xCurrentNode);
			if(stXmlRunInfo.xCurrentNode == null)
			{
				ShowFMessage("�ڼ������ʱ, ���ɽ����������û�з��ֽ�������");
				return false;
			}
			if(stXmlRunInfo.xCurrentNode.Name != "RESULT")
			{
				ShowFMessage("�����ĵ�ǰ��㣬���Ǵ��������Ľ�㣬�ý������Ϊ:" + stXmlRunInfo.xCurrentNode.Name);
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
						ShowFMessage("�Ƿ����������ʽ��"+e.Message);
						return false;
					}
					if (m_colNums<=0) 
					{
						ShowFMessage("�Ƿ�������ֵ������Ӧ�ô��ڵ���1");
						return false;
					}
					if (stConnectINfo.rd.FieldCount != m_colNums) 
					{
						ShowFMessage("������е��������ű��е�������һ�£�������е�����Ϊ:" + stConnectINfo.rd.FieldCount);
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
						ShowFMessage("�Ƿ����������ʽ��"+e.Message);
						return false;
					}					
					return CheckResultRowEx(stXmlRunInfo.xCurrentNode.FirstChild, rowNums);
				}
			}
		}

		//���������ֻ�����е����ƣ�����������и��и��е�ֵ
		public bool CheckResultColName(XmlNode m_XmlNode)
		{
			m_XmlNode = SkipComment(m_XmlNode);
			if(m_XmlNode == null)
			{
				ShowFMessage("δ����Ҫ�Ƚϵ�������");
				return false;
			}
			if(m_XmlNode.Name != "RECORD")
			{
				ShowFMessage("�������ʼ�е����Ʋ��Ǵ����������еĽ�㣬�ý������Ϊ:" + m_XmlNode.Name);
				return false;
			}
			XmlNode m_tempNode = SkipComment(m_XmlNode.FirstChild);//����ע�Ͳ���
			//			MD5 md5 = new MD5CryptoServiceProvider();
			//
			//			byte[] result1;
			//			byte[] result2;
			int m_maxColNum = 0;//��ʾ��ǰXML�ļ���������е������			
			while(m_tempNode != null)
			{
				string ExpCol = GetColumn(m_tempNode);
				if(ExpCol == null)
				{
					ShowFMessage("δ�ڵ�ǰ�ڵ��ҵ��е�����");
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
					ShowFMessage("���ؽ�����������ƺͽű��еĲ�ƥ��\n���أ�" + stConnectINfo.rd.GetName(m_maxColNum) + "\nԤ�ڣ�" + ExpCol);
					return false;
				}
				m_tempNode = m_tempNode.NextSibling;
				m_tempNode = SkipComment(m_tempNode);
				m_maxColNum++;
				if(m_maxColNum >= stConnectINfo.rd.FieldCount && m_tempNode != null)
				{
					ShowFMessage("���ؽ����������С�ڽű��е�������һ�£�������д��� " + stConnectINfo.rd.FieldCount + "��");
					return false;
				}
			}
			if(m_maxColNum != stConnectINfo.rd.FieldCount)
			{
				ShowFMessage("���ؽ�����������ͽű��е�������һ��\n�������" + stConnectINfo.rd.FieldCount + "\n��  ����" + m_maxColNum);
				return false;
			}
			return true;
		}
		//���������ֻ�����е����ƣ�����������и��и��е�ֵ
		public bool CheckResultColNameEx(XmlNode m_colNode, int m_colnums)
		{
			m_colNode = FindXmlNode(m_colNode, "COLUMN");
			if (m_colNode == null) 
			{
				ShowFMessage("δ����COMUMN");
				return false;
			}
			if (FindXmlNode(m_colNode.NextSibling, "COLUMN") != null) 
			{
				ShowFMessage("��ʹ����COLUMNNUMS�ؼ��ֵ�����£�ֻ��ָ��һ��COLUMN��");
				return false;
			}

			int m_maxColNum = 0;//��ʾ��ǰXML�ļ���������е������			
			while(m_maxColNum<m_colnums)
			{
				vlc.SetVal("COLUMNNUMS", (m_maxColNum+1).ToString());
				string ExpCol = GetColumn(m_colNode);
				if(ExpCol == null)
				{
					ShowFMessage("δ�ڵ�ǰ�ڵ��ҵ��е�����");
					return false;
				}
				ExpCol = ReplaceRunInfo(ExpCol);
				if(ExpCol != stConnectINfo.rd.GetName(m_maxColNum))
				{
					ShowFMessage("���ؽ�����������ƺͽű��еĲ�ƥ��\n���أ�" + stConnectINfo.rd.GetName(m_maxColNum) + "\nԤ�ڣ�" + ExpCol);
					return false;
				}

				m_maxColNum++;
			}
			return true;
		}
		/// <summary>
		/// //���ִ�еĽ��
		/// </summary>
#if DM7
        public bool CheckExecute(bool m_su, string m_sql, DmException e)
#else
		public bool CheckExecute(bool m_su, string m_sql, OleDbException e)
#endif
		{			
			string retcode = "0";
			string sqlstate = "";
			if(stXmlRunInfo.bClearEn)	//����ò�����������������ģ���ô���������е���ȷ�鲻�����
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
			errorMessages = "��䣺" + m_sql + "\nԤ��ִ�н����" + stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult
				+ "\nʵ��ִ�н����";
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
						ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ��
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
						ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ��
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
						errorMessages += "���ִ��ʧ�ܣ�����û�з��ش�����Ϣ����֪ͨOLEDB������Ա";
					}
					else
					{
						errorMessages = "��䣺" + m_sql + "ִ����ɺ�\n";
						bool bFail = false;
						
						if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError != 0)
						{		
#if DM7
                            if (stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError != e.ErrorCode)
                            {
                                errorMessages += "Ԥ�ڷ��� NativeError: " + stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError + "\nʵ�ʷ��ط��� NativeError: " + e.ErrorCode + "\n";
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
								errorMessages += "Ԥ�ڷ��� NativeError: " + stXmlRunInfo.cCurrentSqlCase.stSqlResult.iNativeError + "\nʵ�ʷ��ط��� NativeError: " + e.Errors[0].NativeError + "\n";
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
								errorMessages += "Ԥ�ڷ��� SQLState: " + stXmlRunInfo.cCurrentSqlCase.stSqlResult.sSQLState + "\nʵ�ʷ��ط��� SQLState: " + e.Errors[0].SQLState + "\n";
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
					ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ��
			    //-----test
			}
			else if(stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT" || stXmlRunInfo.cCurrentSqlCase.stSqlResult.sExpResult == "DIRECT_EXECUTE_SELECT_COMPARE_RESULT_FULL")
			{
				if(!m_su)
				{
					ShowFMessage("������������ִ��ʧ���ˣ��޷����н�����Ƚ�");
					//-----test
					sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
					if(sqlCounter.sqlLineNum > 0)
						ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ��
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
					ShowFMessage("������������ִ��ʧ���ˣ��޷����н�����Ƚ�");
					//-----test
					sqlCounter.sqlLineNum = sqlCounter.findLine(stXmlRunInfo.sXmlFileName, "<SQL>", sqlCounter.sqlNum);
					if(sqlCounter.sqlLineNum > 0)
						ShowFMessage("��" + testnum + "��" + "ִ�д���\n ");//ִ���б��������������ĸ����Ե����������Ķ�λ���еı���ʽ��ȡ��
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
		/// ��ʾ��ѯ������ɵĽ����
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

            //��ʾ����
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
		/// //����������������ϵĴ�����Ϣ
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
				
				if(!ProClass.bValIsErrRun && stXmlRunInfo.bClearEn == false)//��������������������һ������ֵ
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
			Debug.Assert(iIndex >= -1, "�������ӵ�IDС��-1", "XmlTest.DisConnect ����");
			if(iIndex == -1)//�����ֵΪ-1,��ô�Ͽ����е�����
			{				
				for(int index=0; index<stConnectArry.Count; index++)
				{
					if(!SetCn(index, false))
					{
						ShowFMessage("��������ʱ����");
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
				ShowSMessage("�����Ѿ�ȫ�����Ͽ�");
				try
				{
#if DM7
                    //DmConnection.ReleaseObjectPool();//��ͼ������ӳ��е�����
#else
                    OleDbConnection.ReleaseObjectPool();//��ͼ������ӳ��е�����
#endif
                }
				catch(Exception e)
				{
					ShowFMessage(e.Message);
				}
                stConnectINfo = stTemp;
				return true;
			}
			if(iIndex >= stConnectArry.Count || iIndex < -1)//���XML�ļ���Ҫ���õĵ�ǰ����ID��������ID������С��-1
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
					ShowSMessage("����Ϊ " + iIndex + " �������Ѿ����Ͽ�");
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
      //          DmConnection.ReleaseObjectPool();//��ͼ������ӳ��е�����
#else
                OleDbConnection.ReleaseObjectPool();//��ͼ������ӳ��е�����
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
			string sConnectStr;			//�����ַ�������
#if DM7
            sConnectStr = "Server=" + sServerName;
            sConnectStr += ";User Id=" + sUid; 
            sConnectStr += ";PWD=" + sPwd;
            Console.Write("XML�����ַ���" + sConnectStr);
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
		//�������ӵ�ǰ������
		private bool ReConnect()
		{	
			if (stConnectINfo == null) {
				ShowFMessage("û���κ�����");
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
					ShowSMessage("��ǰ�����Ѿ����Ͽ�");
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
                ShowSMessage("��ǰ�����Ѿ�������!" + "  �û���" + stConnectINfo.sUid + "; ���" + stConnectINfo.sPwd + "; ��ʼ�⣺" + stConnectINfo.sDatabase + "; ��������" + stConnectINfo.sServerName);
#else
                ShowSMessage("��ǰ�����Ѿ�������!" + "  �û���" + stConnectINfo.sUid + "; ���" + stConnectINfo.sPwd + "; ��ʼ�⣺" + stConnectINfo.sDatabase + "; ��������" + stConnectINfo.sServerName + "; ������" + stConnectINfo.sProvider);

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

		//����stXmlRunInfo.xFirstNode����
		public void SetFirstNode(XmlNode m_FNode)
		{
			stXmlRunInfo.xFirstNode = m_FNode;
		}
		/// <summary>
		/// //�����������XML�ļ�ִ����һ��XML�ļ�
		/// </summary>
		public void ExeXml(string m_XFName)
		{
			ShowSMessage("��ʼ����ִ��XML�ļ���" + m_XFName);			
			string m_tempStr = "";
			int iIndex = -1;
			iIndex = stXmlRunInfo.sXmlFileName.LastIndexOf("\\");
			if(iIndex == -1)
			{
				ShowFMessage("��ǰ�ļ�·���Ƿ���δ����ִ���ļ���" + m_XFName);
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
			if(!m_xmlTest.Run(true))//������õ�XML�ļ�ִ��ʧ�ܣ���ô�����XML�ļ�ֹͣ����
			{
				ShowFMessage("��ǰ�ļ�����ִ�� " + m_XFName + " ʧ��");
			}
			m_xmlTest = null;
			ShowSMessage("����ִ��XML�ļ���" + m_XFName);
		}

		/// <summary>
		/// //������һ��XML�ļ��в��Ҹ������ƵĽ��
		/// </summary>
		public XmlNode FindXmlNode(XmlNode m_XmlNode, string m_XmlNodeName)
		{			
			Debug.Assert(m_XmlNodeName!="", "�����Ľ���ַ���Ϊһ�մ�", "XmlTest.FindXmlNode ����");
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
		/// //������һ��XML�ļ��в��Ҹ������ƵĽ��,ֻ�ڵ�һ���ӽڵ��в�
		/// </summary>
		public XmlNode FindXmlNodeEx(XmlNode m_XmlNode, string m_XmlNodeName)
		{			
			Debug.Assert(m_XmlNodeName!="", "�����Ľ���ַ���Ϊһ�մ�", "XmlTest.FindXmlNode ����");
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
		/// //���������XML�ļ����еĻ���
		/// </summary>
		public void ClearEnvironment()
		{			
			stXmlRunInfo.bClearEn = true;
			if(stXmlRunInfo.xDoc == null)
			{
				stXmlRunInfo.xXmlTextRr = new XmlTextReader(stXmlRunInfo.sXmlFileName);
				Debug.Assert(stXmlRunInfo.xXmlTextRr!=null, "δ�ҵ�ָ����", "XmlTest.ClearEnvironment ����");
				if(stXmlRunInfo.xXmlTextRr == null)//δ�ҵ�ָ����XML�ļ�
				{				
					ShowFMessage("δ�ҵ�ָ����XML�ļ�");
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
