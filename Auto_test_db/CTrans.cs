using System;

namespace Auto_test_db
{
	/// <summary>
	/// CTrans ��ժҪ˵����
	/// </summary>
	public class CTrans
	{
//		public Thread cReadThread;
		public bool bIsFinishExcute;			//��ʾ�ϴη����Ľű�ִ�����û��
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
