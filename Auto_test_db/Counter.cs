using System;
using System.IO;


namespace Auto_test_db
{
	/// <summary>
	/// Counter 的摘要说明。
	/// </summary>
	public class Counter
	{
		public int sqlCaseNum = 0;//为打印出错<SQL_CASE>标记增加记数变量
		public int sqlCaseLineNum = 0; //出错<SQL_CASE>标记所在行数
		//public int sqlCaseErrLineNum = 0;
		
		public int sqlNum = 0;
		public int sqlLineNum = 0;

/*
		public int newConnNum = 0;
		public int newConnLineNum = 0;

		public int resultNum = 0;
		public int resultLineNum = 0;*/


		public Counter()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}
		
		public int findLine(string filePath, string strDst, int errNum)  //strDst目标结点：比如<SQL_CASE>
		{
			StreamReader sr = null;
			string strLine = null;
			int lineNum = 1;
			int sqlNum = 0;

			try
			{
				sr = new StreamReader(filePath, System.Text.Encoding.Default);
				if(sr == null)
				{
					sr.Close();
					return -1 ;
				}
			}
			catch(Exception e)
			{
				string mes = e.Message;
				sr.Close();
				return -1;				
			}
					
			strLine = sr.ReadLine();			
			while(strLine != null)
			{
				if(strLine.IndexOf(strDst) >= 0)  //该行字符串中存在strDst
				{
					sqlNum++;
					if(sqlNum == errNum)
					{
						sr.Close();
						return lineNum;
					}
				}
				strLine = sr.ReadLine();
				lineNum++;
			}
			sr.Close();
			return 0;
		}
	}
}
