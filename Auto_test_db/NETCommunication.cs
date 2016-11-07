using System;
using System.Collections ; 
using System.ComponentModel ; 
using System.Data ; 
using System.Net.Sockets ; 
using System.IO ; 
using System.Threading ; 
using System.Net ; 
using System.Runtime.InteropServices; 

namespace Auto_test_db
{
	/// <summary>
	/// NETCommunication��������������˵����������Ϣ��
	/// ��Ϣ������RestartServer,StopServer,
	/// CreaeFie,ReadFile,DeleteFile,ModifyFile
	/// ��ʱ������ѿ�����������Щ��Ϣ����Ӧ
	/// </summary>
	public class NETCommunication
	{
		static NetworkStream networkStream ; 
		static StreamWriter streamWriter ; 
		static StreamReader streamReader ; 
		static TcpClient myclient;
		public string sleepTime;
		//static FILE *fp;

		[DllImport("MYSOCKET.dll")]
		public static extern string SendRecvMsg( string server_name, int port, string send_msg );
		[DllImport("MYSOCKET.dll")]
		public static extern string SendCmd( string strCmd );

		public NETCommunication()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}
		/// <summary>
		/// ���ӵ��������������������networkStream����ͨ�������׽���
		/// </summary>
		/// 
		private string getStrServer()
		{
			string strServer = ProClass.sValServer.Split(new char [] {':'})[0];
			if (strServer.ToUpper() == "LOCALHOST")
			{
				strServer = "127.0.0.1";
			}		
			return strServer;
		}
		public string ConnectSev()
		{
			//fp=fopen("d:\\1.txt","w");
			try 
			{ 
				myclient = new TcpClient(getStrServer(),5236);
				//myclient = new TcpClient ( "localhost" , 5000 ) ;  //������  ��ͨ���޸�IP��������������
			} 
			catch 
			{ 
				string failed="connect failed";
				return failed;
			} 
			//����networkStream����ͨ�������׽��������ܺͷ������� 
			networkStream = myclient.GetStream ( ) ; 
			streamReader = new StreamReader (networkStream ) ; 
			streamWriter = new StreamWriter (networkStream ) ; 
			string success="connect success";
			return success;
		}

		public string Send_Cmd(string mes)
		{
			string s ; 
			
			try 
			{ 
				s = SendCmd(mes);
				if(s.Substring(s.Length-4) == "Fail")
				{
					s = "0" + s;
				}
				else
				{
					s = "1" + s;
				}
			}
			catch ( Exception ee ) 
			{ 
				s = "0"+"MYSOCKET.dll���ػ����ʧ�ܣ�" + ee.Message;
			} 
			return s;
		}

		/// <summary>
		/// ͨ��networkStream���������������Ϣ�����շ�����Ϣ
		/// </summary>
		public string Send_Rec_Message(string mes)//������Ϣ������
		{

			string s ; 

			try 
			{ 
				s = SendRecvMsg( getStrServer(), 5236,  mes);
			}
			catch ( Exception ee ) 
			{ 
				s = "MYSOCKET.dll���ػ����ʧ�ܣ�" + ee.Message;
			} 
			return s;
			/*//��������
			try 
			{ 
				
				//����ǰ����������д��һ���ַ��� 
				streamWriter.WriteLine(mes); 
				//ˢ�µ�ǰ�������е�����  
				streamWriter.Flush( ) ; 
			}
			catch ( Exception ee ) 
			{ 
				System.Console.WriteLine
					( "�ӷ������˶�ȡ���ݳ��ִ�������Ϊ��" + ee.ToString ( ) ) ;
				string ret="�������˳��ִ���:д��������";
				return ret;
			} 
			//��ȡ������Ϣ������
			try
			{
				s = streamReader.ReadLine( );
				//streamReader.DiscardBufferedData();

			} 
			catch ( Exception ee ) 
			{ 
				System.Console.WriteLine
					( "�ӷ������˶�ȡ���ݳ��ִ�������Ϊ��" + ee.ToString ( ) ) ;
				string ret="�������˳��ִ��󣺶�����";
				return ret;
			}
			s = "\n����ִ�����,�ȴ���������Ӧ,��Ӧʱ���������á�����-��������������-�������";
			Thread.Sleep(Convert.ToInt32(sleepTime));
			return s;*/
		}
		/// <summary>
		///�رճ����д�������
		/// </summary>
		public void CloseStream()
		{
			streamWriter.Close ( ) ; 
            streamReader.Close ( ) ; 
			networkStream.Close ( ) ; 
		}
	}
}
