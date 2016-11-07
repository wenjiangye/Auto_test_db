using System;

namespace Auto_test_db
{
	/// <summary>
	/// CTrans 的摘要说明。
	/// </summary>
	public class CTrans
	{
//		public Thread cReadThread;
		public bool bIsFinishExcute;			//表示上次发出的脚本执行完成没有
		public CTrans()
		{
			bIsFinishExcute = false;
		}

		public void ShowSMessage(string sMes)
		{
		}

		public void ShowFMessage(string sMes)
		{
		}
	}
}
