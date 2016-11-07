using System;
using System.IO;


namespace Auto_test_db
{
	/// <summary>
	/// Counter ��ժҪ˵����
	/// </summary>
	public class Counter
	{
		public int sqlCaseNum = 0;//Ϊ��ӡ����<SQL_CASE>������Ӽ�������
		public int sqlCaseLineNum = 0; //����<SQL_CASE>�����������
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
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}
		
		public int findLine(string filePath, string strDst, int errNum)  //strDstĿ���㣺����<SQL_CASE>
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
				if(strLine.IndexOf(strDst) >= 0)  //�����ַ����д���strDst
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
