using System;
using System.Xml;
using System.Data;
using System.Data.OleDb;
namespace Auto_test_db
{
	/// <summary>
	/// ����ִ��һ��SQL_CASE�ű�ģ�����
	/// ���ã���û�д�ԭ����XmlTest���з������
	/// </summary>	
	public class SqlCase
	{		
		public CASERESULT stCaseResult;
		public SqlCase cParentCase;
		public SQLRESULT stSqlResult;
		public SqlCase()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
			stCaseResult = new CASERESULT();
			stCaseResult.bBreak = false;			//Ĭ��ֵΪ������
			stCaseResult.bSuccess = true;		//Ĭ��ֵΪִ�гɹ�
			stCaseResult.bExpResult = true;
			stCaseResult.bFailStop = true;

			stSqlResult = new SQLRESULT();
			stSqlResult.sExpResult = "DIRECT_EXECUTE_SUCCESS";
			stSqlResult.sSQLState = "";
		}

	}
}
