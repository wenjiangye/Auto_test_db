using System;
using System.Xml;
using System.Data;
using System.Data.OleDb;
namespace Auto_test_db
{
	/// <summary>
	/// 用来执行一个SQL_CASE脚本模块的类
	/// 备用，还没有从原来的XmlTest类中分离出来
	/// </summary>	
	public class SqlCase
	{		
		public CASERESULT stCaseResult;
		public SqlCase cParentCase;
		public SQLRESULT stSqlResult;
		public SqlCase()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
			stCaseResult = new CASERESULT();
			stCaseResult.bBreak = false;			//默认值为不跳出
			stCaseResult.bSuccess = true;		//默认值为执行成功
			stCaseResult.bExpResult = true;
			stCaseResult.bFailStop = true;

			stSqlResult = new SQLRESULT();
			stSqlResult.sExpResult = "DIRECT_EXECUTE_SUCCESS";
			stSqlResult.sSQLState = "";
		}

	}
}
