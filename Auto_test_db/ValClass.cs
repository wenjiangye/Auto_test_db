using System;
using System.Collections;
namespace Auto_test_db
{
	/// <summary>
	/// ValClass 的摘要说明。
	/// 用来管理脚本中自定义的变量
	/// </summary>
	public class ValClass
	{
		public const int NOTFOUND = -1;	//没有在字节点中发现字符串
		PKeyCharValue aPKeyTreeHeadList;		
		public ValClass()
		{
			aPKeyTreeHeadList = new PKeyCharValue('S', null, true);
		}

		//往关键字树中添加关键字
		public void AddPrimaryKey(string sPKeyName, object val)
		{
			PKeyCharValue cTempPKeyCharValue = aPKeyTreeHeadList;
			int i=0;
			int iIndex;
			if (sPKeyName.Length == 0) {
				return;
			}
			for(; i<sPKeyName.Length-1; i++)
			{
				iIndex = cTempPKeyCharValue.AddCharElement(new PKeyCharValue(sPKeyName[i], null, false));
				cTempPKeyCharValue = (PKeyCharValue)cTempPKeyCharValue.aPKeyTreeList[iIndex];				
			}			
			cTempPKeyCharValue.AddCharElement(new PKeyCharValue(sPKeyName[i], val, true));	
		}

		public void SetVal(string key, object val)
		{
			int i=0;
			PKeyCharValue aTempTreeList = aPKeyTreeHeadList;
			int iTempIndex = aTempTreeList.IndexOfArray(key[i]);
			while(iTempIndex != NOTFOUND)
			{
				i++;
				if(i >= key.Length)
				{
					aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
					break;
				}
				aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
				iTempIndex = aTempTreeList.IndexOfArray(key[i]);					
			}
			if (iTempIndex == NOTFOUND) {
				AddPrimaryKey(key, val);//找不着就添加一个
			}
			else{
				aTempTreeList.sValue = val;
			}
		}

		public object GetVal(string key)
		{
			int i=0;
			PKeyCharValue aTempTreeList = aPKeyTreeHeadList;
			int iTempIndex = aTempTreeList.IndexOfArray(key[i]);
			while(iTempIndex != NOTFOUND)
			{
				i++;
				if(i >= key.Length)
				{
					aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
					break;
				}
				aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
				iTempIndex = aTempTreeList.IndexOfArray(key[i]);					
			}
			if (iTempIndex == NOTFOUND) 
			{
				return null;
			}
			else
			{
				return aTempTreeList.sValue;
			}
		}

		public string ReplaceRunInfo(string _sql)
		{
			string sql = "";
			string sTemp = "";
			PKeyCharValue aTempTreeList;
			int iTempIndex;
			int i;
			int index = _sql.IndexOf("@");
			while (index != NOTFOUND) {
				sTemp = _sql.Substring(0, index);//去掉字符@
				sql += sTemp;
				_sql = _sql.Substring(index);
				if (_sql.Length == 1) {
         //           Console.WriteLine(sql);
                    return sql;
				}
				aTempTreeList = aPKeyTreeHeadList;
				i = 1;
				iTempIndex = aTempTreeList.IndexOfArray(_sql[i]);
				while(iTempIndex != NOTFOUND)
				{
					i++;
					if(i >= _sql.Length)
					{
						aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
						break;
					}
					aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
					iTempIndex = aTempTreeList.IndexOfArray(_sql[i]);					
				}
				if (aTempTreeList.sValue != null) 
				{
					if (aTempTreeList.sValue.GetType() == Type.GetType("System.Byte[]"))
					{
						byte[] a = (byte[])aTempTreeList.sValue;
						sql += "二进制数据(" + a.Length + ")";
					}
					else
					{
						sql += aTempTreeList.sValue;
					}
					_sql = _sql.Substring(i);					
				}
				else{
					sql += _sql.Substring(0, i);
					_sql = _sql.Substring(i);	
				}
				index = _sql.IndexOf("@");
			}
    //        Console.WriteLine(sql + _sql);
            return sql + _sql;
		}
		public string ReplaceRunInfoEx(string _sql)
		{
			string sql = "";
			string sTemp = "";
			PKeyCharValue aTempTreeList;
			int iTempIndex;
			int i;
			int index = _sql.IndexOf("@");
			while (index != NOTFOUND) 
			{
				sTemp = _sql.Substring(0, index);
				sql += sTemp;
				_sql = _sql.Substring(index);
				if (_sql.Length == 1) 
				{
					return sql;
				}
				aTempTreeList = aPKeyTreeHeadList;
				i = 1;
				iTempIndex = aTempTreeList.IndexOfArray(_sql[i]);
				while(iTempIndex != NOTFOUND)
				{
					i++;
					if(i >= _sql.Length)
					{
						aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
						break;
					}
					aTempTreeList = (PKeyCharValue)aTempTreeList.aPKeyTreeList[iTempIndex];
					iTempIndex = aTempTreeList.IndexOfArray(_sql[i]);					
				}
				if (aTempTreeList.sValue != null) 
				{
					if (aTempTreeList.sValue.GetType() == Type.GetType("System.Byte[]"))
					{
						byte[] a = (byte[])aTempTreeList.sValue;
						sql += "二进制数据(" + a.Length + ")";
					}
					else
					{
						sql += aTempTreeList.sValue;
					}
					_sql = _sql.Substring(i-1);					
				}
				else
				{
					sql += _sql.Substring(0, i);
					_sql = _sql.Substring(i);	
				}			
			}
         //   Console.WriteLine(sql + _sql);
			return sql + _sql;
		}
	}
	public class PKeyCharValue//代表关键字除最后一个字符以外的字符元素
	{
		public const int NOTINSERT = -1;//代表字符没有被插入到树中
		public const int NOTFOUND = -1;	//没有在字节点中发现字符串
		public char chKeyChar;
		public ArrayList aPKeyTreeList;	
		public object sValue;
		public bool overwrite;
		public PKeyCharValue(char chPKChar, object val, bool _overwrite)
		{
			aPKeyTreeList = new ArrayList();
			overwrite = _overwrite;
			sValue = val;			
			chKeyChar = chPKChar;
		}

		public int AddCharElement(PKeyCharValue cPKeyCharValue)
		{
			int iIndex = NOTINSERT;
			for(int i=0; i<aPKeyTreeList.Count; i++)
			{
				if(cPKeyCharValue.chKeyChar == ((PKeyCharValue)aPKeyTreeList[i]).chKeyChar)
				{
					iIndex = i;
					if (cPKeyCharValue.overwrite) {
						((PKeyCharValue)aPKeyTreeList[i]).sValue = cPKeyCharValue.sValue;
					}					
					break;
				}
				else if(cPKeyCharValue.chKeyChar < ((PKeyCharValue)aPKeyTreeList[i]).chKeyChar)
				{
					aPKeyTreeList.Insert(i, cPKeyCharValue);
					iIndex = i;
					break;
				}
			}
			if(iIndex == NOTINSERT)//没有被插入到树，那么就加进去
			{
				aPKeyTreeList.Add(cPKeyCharValue);
				iIndex = aPKeyTreeList.Count - 1;
			}
			return iIndex;
		}

		public int IndexOfArray(char chKeyChar)//算法为二分查找
		{
			int iLow = 0;
			int iHigh = aPKeyTreeList.Count-1;
			int iMid = 0;
			while(iLow <= iHigh)
			{
				iMid = (iLow+iHigh) / 2;
				if(chKeyChar == ((PKeyCharValue)aPKeyTreeList[iMid]).chKeyChar)
				{
					return iMid;
				}
				else if(chKeyChar < ((PKeyCharValue)aPKeyTreeList[iMid]).chKeyChar)
					iHigh = iMid-1;
				else 
					iLow = iMid+1;
			}
			return NOTFOUND;
		}
	}
}
