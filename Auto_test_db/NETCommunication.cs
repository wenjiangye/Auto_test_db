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
	/// NETCommunication类用来向服务器端的组件发送消息。
	/// 消息包括：RestartServer,StopServer,
	/// CreaeFie,ReadFile,DeleteFile,ModifyFile
	/// 暂时组件端已可以做出对这些消息的响应
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
			// TODO: 在此处添加构造函数逻辑
			//
		}
		/// <summary>
		/// 连接到服务器端组件，并创建networkStream对象通过网络套节字
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
				//myclient = new TcpClient ( "localhost" , 5000 ) ;  //连本机  可通过修改IP连到其他服务器
			} 
			catch 
			{ 
				string failed="connect failed";
				return failed;
			} 
			//创建networkStream对象通过网络套节字来接受和发送数据 
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
				s = "0"+"MYSOCKET.dll加载或调用失败：" + ee.Message;
			} 
			return s;
		}

		/// <summary>
		/// 通过networkStream对象向组件发送消息并接收反馈信息
		/// </summary>
		public string Send_Rec_Message(string mes)//接收消息有问题
		{

			string s ; 

			try 
			{ 
				s = SendRecvMsg( getStrServer(), 5236,  mes);
			}
			catch ( Exception ee ) 
			{ 
				s = "MYSOCKET.dll加载或调用失败：" + ee.Message;
			} 
			return s;
			/*//发送命令
			try 
			{ 
				
				//往当前的数据流中写入一行字符串 
				streamWriter.WriteLine(mes); 
				//刷新当前数据流中的数据  
				streamWriter.Flush( ) ; 
			}
			catch ( Exception ee ) 
			{ 
				System.Console.WriteLine
					( "从服务器端读取数据出现错误，类型为：" + ee.ToString ( ) ) ;
				string ret="服务器端出现错误:写操作出错";
				return ret;
			} 
			//读取反馈消息并返回
			try
			{
				s = streamReader.ReadLine( );
				//streamReader.DiscardBufferedData();

			} 
			catch ( Exception ee ) 
			{ 
				System.Console.WriteLine
					( "从服务器端读取数据出现错误，类型为：" + ee.ToString ( ) ) ;
				string ret="服务器端出现错误：读出错";
				return ret;
			}
			s = "\n命令执行完毕,等待服务器响应,相应时间请在设置“设置-重启服务器设置-检查间隔”";
			Thread.Sleep(Convert.ToInt32(sleepTime));
			return s;*/
		}
		/// <summary>
		///关闭程序中创建的流
		/// </summary>
		public void CloseStream()
		{
			streamWriter.Close ( ) ; 
            streamReader.Close ( ) ; 
			networkStream.Close ( ) ; 
		}
	}
}
