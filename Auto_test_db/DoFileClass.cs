using System;
using System.IO;
namespace Auto_test_db
{
	/// <summary>
	/// DoFileClass 的摘要说明。
	/// </summary>
	public class DoFileClass
	{
		public DoFileClass()
		{
			
		}
		public static string SetVal(String path, String name, String val)
		{
			bool bFound = false;
			String sFileText = "";
			StreamReader sr = null;
			StreamWriter wr = null;
			//下面分别是从保存的设置文件中，读取值来初始化上面那些变量
			try
			{
				sr = new StreamReader(path);
			}
			catch(Exception e)
			{
				return e.Message;
			}
			String sVal = "";
			sVal = sr.ReadLine();
			while (sVal != null) {
				sVal = sVal.Trim();
				if (sVal.Length > name.Length && sVal.StartsWith(name) && (sVal[name.Length] == ' ' ||  sVal[name.Length] == '=')) {
					sVal = name + " = " + val;
					bFound = true;
				}
				sFileText += sVal + "\r\n";
				sVal = sr.ReadLine();
			}
			if (!bFound) {
				sVal = name + " = " + val;
				sFileText += sVal;
			}
			sr.Close();
			try
			{
				wr = new StreamWriter(path);
			}
			catch(Exception e)
			{
				return e.Message;
			}
			wr.Write(sFileText);
			wr.Close();
			return null;
		}

		public static string GetVal(String path, String name)
		{
			String ret = "";
			StreamReader sr = null;
			string temp = path + ".tmp";
			//下面分别是从保存的设置文件中，读取值来初始化上面那些变量
			try
			{
				File.Copy(path, temp, true);
				sr = new StreamReader(temp);
			}
			catch(Exception e)
			{
				File.Delete(temp);
				return e.Message;
			}
			String sVal = "";
			sVal = sr.ReadLine();
			while (sVal != null) 
			{
				sVal = sVal.Trim();
				if (sVal.Length > name.Length && sVal.StartsWith(name) && (sVal[name.Length] == ' ' ||  sVal[name.Length] == '='))
				{
					ret = sVal.Substring(sVal.IndexOf("=") + 1);
					ret = ret.Trim();
					sr.Close();
					File.Delete(temp);
					return ret;
				}
				sVal = sr.ReadLine();
			}

			sr.Close();
			File.Delete(temp);
			return ret;
		}

		public static string CreateFile(String path, object val, bool flag, bool is_binary)
		{
			if(is_binary == false)
			{
				StreamWriter wr = null;
				try
				{
					wr = new StreamWriter(path, flag, System.Text.Encoding.Default);
					wr.Write((string)val);
				}
				catch(Exception e)
				{
					return e.Message;
				}
				if (wr != null) 
				{
					wr.Close();
				}
			}
			else
			{
				FileStream fs = null;
				try
				{
					byte[] images = (byte[])val;
					if(flag)
					{
						fs = new FileStream(path, FileMode.Append, FileAccess.Write);
					}
					else
					{
						fs = new FileStream(path, FileMode.Create, FileAccess.Write);
					}
					fs.Write(images, 0, images.Length);
				}
				catch(Exception e)
				{
					return e.Message;
				}
				if (fs != null) 
				{
					fs.Close();
				}
			}
			return null;
		}
	}
}
