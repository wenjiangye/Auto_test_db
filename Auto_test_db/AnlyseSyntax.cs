using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
namespace Auto_test_db
{
	/// <summary>
	/// AnlyseSyntax 的摘要说明。
	/// </summary>	
	
	public class AnlyseSyntax
	{
		private char[] LIST_SEPARATOR ={' ', ',', '<', '>', '(', ')', '\n', '\t', '\b', ';', '\r', '\\', '{', '}'}; //脚本中定义的分隔符
		private char[] NOTSTARTCHAR ={' ', ',', '>', '(', ')', '\n', '\t', '\b', ';', '\r', '\\', '{', '}'}; //这些字符不能用来作一个关键字的开始字符
		private const int NOTFOUND = -1;//代表字符不是分隔符
		private int LESSTHANCHARINDEX;
		private int MORETHANCHARINDEX;
		private const string COMMENTSTART = "<!--";	//注释开头部分
		private const string COMMENTEND = "-->";		//注释结尾部分
		private Color COMMENTCOLOR = Color.Green;	//注释字体颜色
		private Color ERRORKEYCOLOR = Color.Red;		//错误的关键字颜色
		//下面是个种颜色的RTF格式字符串
		enum COLORTYPE
		{
			Black = 0,
			Red,
			Gray,
			Green,
			Blue,
			MidnightBlue,
			Orange,
			DarkCyan,
			Purple,
			DodgerBlue,
			LimeGreen,
			OliveDrab,
			DarkRed,
			DeepSkyBlue,
			Magenta,
			ZNum
		}
		struct STRINGCOLORFORMAT
		{
			public string sFomatStr;
			public Color cColor;
		}
		STRINGCOLORFORMAT[] aStringColorFormat;
		private string sRtfStart;
		private string sRtfEnd;
		private string sRtf;		//临时的RTF格式串
		private string sRtfText;
		PKeyCharElement aPKeyTreeHeadList;
		bool bCommentStartForLastLine;							//表示上一行分析的结果是否包含注释的开始
		public AnlyseSyntax()
		{
			aPKeyTreeHeadList = new PKeyCharElement('a', Color.Black);
			SortListSeparator();//排序分隔字符
			SortNotStartChar();
			CreateColorString();
			AddXmlKey();
			AddSqlKey();
		}
		private int FindCommentEnd(string sText, int iStart)
		{
			if(COMMENTSTART.Length+iStart > sText.Length)
			{
				return sText.Length;
			}
			if(sText.IndexOf(COMMENTSTART, iStart, COMMENTSTART.Length) == iStart)
			{
				iStart += COMMENTSTART.Length;
			}
			int iEndLen = sText.Length - COMMENTEND.Length;
			while(iStart <= iEndLen )
			{
				if(sText[iStart] == '-')
				{
					if(sText[iStart+1] == '-' && sText[iStart+2] == '>')
					{
						bCommentStartForLastLine = false;//标识注释结束
						return iStart+3;
					}
				}
				iStart ++;
			}
			return sText.Length;
		}
		//增加一个字符串到临时RTF格式字符串中(sRtf)
		private void AppendToSRtf(string sText, Color cColor)
		{
			if(sText == "" || sText == null)
				return;
			for(int i=0; i<((int)COLORTYPE.ZNum); i++)
			{
				if(aStringColorFormat[i].cColor == cColor)
				{
					sRtf += aStringColorFormat[i].sFomatStr + sText;
					return;
				}
			}
			sRtf += sText;
		}
		private int FindStartCharIndex(string sText, int iStart)
		{
			int iLen = sText.Length;
			while(iStart < iLen)
			{
				if(IsPKStart(sText[iStart]))
					return iStart;
				iStart++;
			}
			return iLen;
		}
		//是否是注释的开始
		private bool IsCommentStart(string sText, int iStart)
		{
			int iLen = sText.Length;
			if(iStart <= iLen-COMMENTSTART.Length)
			{
				if(sText[iStart] == '<' && sText[++iStart] == '!' && sText[++iStart] == '-' && sText[++iStart] == '-')
				{
					bCommentStartForLastLine = true;
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		private int FindListSeparatorIndex(char chListSeptorChar)
		{
			int iLow = 0;
			int iHigh = LIST_SEPARATOR.Length - 1;
			int iMid = 0;
			while(iLow <= iHigh)
			{
				iMid = (iLow+iHigh) / 2;
				if(chListSeptorChar == LIST_SEPARATOR[iMid])
				{
					return iMid;
				}
				else if(chListSeptorChar < LIST_SEPARATOR[iMid])
					iHigh = iMid-1;
				else 
					iLow = iMid+1;
			}
			return NOTFOUND;
		}
		//分析读入的一行，并生成相应格式的RTF文本串
		private void AnyslseSynataxForAnLine(string sLineText)
		{
			sLineText = sLineText.Replace("\\","\\\\"); 
			sLineText = sLineText.Replace("{","\\{");
			sLineText = sLineText.Replace("}","\\}");
			int iTextLen = sLineText.Length;
			int iTempStart = 0;		//重新开始分析的位置
			int iTempLen = 0;				//分析字符的长度
			int iFindListSptorIndex = NOTFOUND;			//分隔符号索引
			int iTempIndex = NOTFOUND;					//当前字符在语法树中当前节点子节点的索引
			bool bCommentStart = false;		//代表当前起始位置是注释的开始
			Color cColor = Color.Black;
			
			PKeyCharElement aTempTreeList = aPKeyTreeHeadList;			//当前语法树节点
			int i=0;
			iTempStart = i;
			if(bCommentStartForLastLine)
			{
				int iCommentEndIndex = FindCommentEnd(sLineText, iTempStart);
				cColor = COMMENTCOLOR;		
				i = iCommentEndIndex;
				iTempLen = i-iTempStart;
				AppendToSRtf(sLineText.Substring(iTempStart, iTempLen), cColor);
			}
			iTempStart = i;
			while(iTempStart<iTextLen)
			{	
				iTempLen = i-iTempStart;
				AppendToSRtf(sLineText.Substring(iTempStart, iTempLen), cColor);
				iTempStart = i;
				if(iTempStart >=  iTextLen)
					break;
				cColor = Color.Black;
				i=FindStartCharIndex(sLineText, i);
				iTempLen = i-iTempStart;
				AppendToSRtf(sLineText.Substring(iTempStart, iTempLen), cColor);

				iTempStart = i;
				if(iTempStart >=  iTextLen)
					break;
				//判断开始处是不是以注释关键字开始的
				bCommentStart = IsCommentStart(sLineText, iTempStart);
				if(bCommentStart)
				{
					int iCommentEndIndex = FindCommentEnd(sLineText, iTempStart);
					cColor = COMMENTCOLOR;		
					i = iCommentEndIndex;
					
					continue;
				}
				aTempTreeList = aPKeyTreeHeadList;
				iTempIndex = aTempTreeList.IndexOfArray(sLineText[i]);
				while(iTempIndex != NOTFOUND)
				{
					i++;
					if(i >= iTextLen)
					{
						aTempTreeList = (PKeyCharElement)aTempTreeList.aPKeyTreeList[iTempIndex];
						break;
					}
					aTempTreeList = (PKeyCharElement)aTempTreeList.aPKeyTreeList[iTempIndex];
					iTempIndex = aTempTreeList.IndexOfArray(sLineText[i]);					
				}
				if(i >= iTextLen)
				{
					iFindListSptorIndex = NOTFOUND-1;
				}
				else
				{
					iFindListSptorIndex = FindListSeparatorIndex(sLineText[i]);
					if(iFindListSptorIndex == MORETHANCHARINDEX)
					{
						cColor = ERRORKEYCOLOR;
						iFindListSptorIndex = NOTFOUND;
					}
				}
				if(iFindListSptorIndex == NOTFOUND)
				{
					if(aTempTreeList.chKeyChar == '>')
					{
						cColor = aTempTreeList.cFontColor;
						continue;
					}
					else//查找下一个分隔符所在的位置
					{
						i++;
						if(i >= iTextLen)
						{
							i = iTextLen;
							continue;
						}
						iFindListSptorIndex = FindListSeparatorIndex(sLineText[i]);
						while(iFindListSptorIndex == NOTFOUND)
						{
							i++;
							if(i >= iTextLen)
							{
								i = iTextLen;
								break;
							}
							iFindListSptorIndex = FindListSeparatorIndex(sLineText[i]);
						}
						if(iFindListSptorIndex == MORETHANCHARINDEX)
						{
							cColor = ERRORKEYCOLOR;
							i++;
						}
						else
						{
							cColor = Color.Black;
						}
						continue;
					}			
				}
				else
				{
					cColor = aTempTreeList.cFontColor;
					continue;
				}
			}
		}
		//判断指定的字符是否有可能为字段的开始
		private bool IsPKStart(char chChar)
		{
			int iLow = 0;
			int iHigh = NOTSTARTCHAR.Length - 1;
			int iMid = 0;
			bool bFound = false;
			while(iLow <= iHigh)
			{
				iMid = (iLow+iHigh) / 2;
				if(chChar == NOTSTARTCHAR[iMid])
				{
					bFound = true;
					break;
				}
				else if(chChar < NOTSTARTCHAR[iMid])
					iHigh = iMid-1;
				else 
					iLow = iMid+1;
			}
			return !bFound;
		}
		private void CreateColorString()
		{
			System.Windows.Forms.RichTextBox richTextBox1 = new System.Windows.Forms.RichTextBox();
			aStringColorFormat = new STRINGCOLORFORMAT[(int)COLORTYPE.ZNum];
			aStringColorFormat[(int)COLORTYPE.Black].cColor = Color.Black;
			aStringColorFormat[(int)COLORTYPE.Blue].cColor = Color.Blue;
			aStringColorFormat[(int)COLORTYPE.DarkCyan].cColor = Color.DarkCyan;
			aStringColorFormat[(int)COLORTYPE.Green].cColor = Color.Green;
			aStringColorFormat[(int)COLORTYPE.Purple].cColor = Color.Purple;
			aStringColorFormat[(int)COLORTYPE.Red].cColor = Color.Red;
			aStringColorFormat[(int)COLORTYPE.DodgerBlue].cColor = Color.DodgerBlue;
			aStringColorFormat[(int)COLORTYPE.Gray].cColor = Color.Gray;
			aStringColorFormat[(int)COLORTYPE.LimeGreen].cColor = Color.LimeGreen;
			aStringColorFormat[(int)COLORTYPE.DarkRed].cColor = Color.DarkRed				;
			aStringColorFormat[(int)COLORTYPE.Orange].cColor = Color.Orange;
			aStringColorFormat[(int)COLORTYPE.OliveDrab].cColor = Color.OliveDrab;
			aStringColorFormat[(int)COLORTYPE.MidnightBlue].cColor = Color.MidnightBlue;
			aStringColorFormat[(int)COLORTYPE.DeepSkyBlue].cColor = Color.DeepSkyBlue;
			aStringColorFormat[(int)COLORTYPE.Magenta].cColor = Color.Magenta;
			string sTempStartEnd = "}\r\n\\viewkind4\\uc1\\pard\\lang2052\\f0\\fs18";

			sRtfStart = "{\\rtf1\\ansi\\ansicpg936\\deff0\\deflang1033\\deflangfe2052{\\fonttbl{\\f0\\fnil\\fcharset134 \\'cb\\'ce\\'cc\\'e5;}}\r\n{\\colortbl ;";
			sRtfEnd = "}\r\n\0";
			
			richTextBox1.Text = "TTTT";
			richTextBox1.SelectAll();
			string sTempSelectRtf;
			for(int i=0; i<((int)COLORTYPE.ZNum); i++)
			{
				richTextBox1.SelectionColor = aStringColorFormat[i].cColor;
				sTempSelectRtf = richTextBox1.SelectedRtf.Substring(richTextBox1.SelectedRtf.IndexOf("colortbl ;"));
				sTempSelectRtf = sTempSelectRtf.Substring("colortbl ;".Length);
				int iLen = sTempSelectRtf.IndexOf("}");				
				sRtfStart += sTempSelectRtf.Substring(0, iLen);
				aStringColorFormat[i].sFomatStr = "\\cf" + (i+1) + " ";
			}
			sRtfStart += sTempStartEnd;
		}
		public string FormatFileText(string sPath)
		{			
			if(sPath == null || sPath == "")
				return "";
			StreamReader sr = null;
			try
			{
				sr = new StreamReader(sPath, System.Text.Encoding.Default);
			}
			catch(Exception e)
			{
				String mes = e.Message;
				return "";
			}
			string sTempStr;	
			int iReadLines = 1;			
			sTempStr = sr.ReadLine();
			sRtfText = sRtfStart;
			while(sTempStr != null)
			{
				iReadLines++;
				try
				{
					AnyslseSynataxForAnLine(sTempStr);//分析后的字符串会存在sRtf对像中
					sRtf += "\\par\r\n";
					sRtfText = sRtfText + sRtf;			
					sRtf = "";
				}
				catch(Exception e)
				{
					String mes = e.Message;
				}
				try
				{
					sTempStr = sr.ReadLine();
				}
				catch(Exception e)
				{
					String mes = e.Message;
					break;
				}
				if(iReadLines > 2000)
				{
					MessageBox.Show("读取的文件超出了二千行，对这个应用程序来说实在是太大了！换个工具编辑这个文件吧:(", "读取文件失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					break;
				}
			}
			sRtfText += sRtfEnd;
			if(sr != null)
				sr.Close();
			return sRtfText;
		}
		//排序分隔字符
		private void SortListSeparator()
		{
			char chTemp;
			for(int i=0; i<LIST_SEPARATOR.Length-1; i++)
			{
				for(int j=i+1; j<LIST_SEPARATOR.Length; j++)
				{
					if(LIST_SEPARATOR[i] > LIST_SEPARATOR[j])
					{
						chTemp = LIST_SEPARATOR[i];
						LIST_SEPARATOR[i] = LIST_SEPARATOR[j];
						LIST_SEPARATOR[j] = chTemp;
					}
				}				
			}
			MakeLessThanMoreThanCharIndex();
		}
		private void MakeLessThanMoreThanCharIndex()
		{
			for(int i=0; i<LIST_SEPARATOR.Length; i++)
			{
				if(LIST_SEPARATOR[i] == '<')
				{
					LESSTHANCHARINDEX = i;
				}
				else if(LIST_SEPARATOR[i] == '>')
				{
					MORETHANCHARINDEX = i;
				}
			}
		}
		//排序不能作为开始字符的数组
		private void SortNotStartChar()
		{
			char chTemp;
			for(int i=0; i<NOTSTARTCHAR.Length-1; i++)
			{
				for(int j=i+1; j<NOTSTARTCHAR.Length; j++)
				{
					if(NOTSTARTCHAR[i] > NOTSTARTCHAR[j])
					{
						chTemp = NOTSTARTCHAR[i];
						NOTSTARTCHAR[i] = NOTSTARTCHAR[j];
						NOTSTARTCHAR[j] = chTemp;
					}
				}				
			}			
		}
		//往关键字树中添加关键字
		private void AddPrimaryKey(string sPKeyName, Color cColor)
		{
			PKeyCharElement cTempPKeyCharElement = aPKeyTreeHeadList;
			int i=0;
			for(; i<sPKeyName.Length-1; i++)
			{
				int iIndex = cTempPKeyCharElement.AddCharElement(new PKeyCharElement(sPKeyName[i], Color.Black));
				cTempPKeyCharElement = (PKeyCharElement)cTempPKeyCharElement.aPKeyTreeList[iIndex];
			}
			cTempPKeyCharElement.AddCharElement(new PKeyCharElement(sPKeyName[i], cColor));

			if(!sPKeyName.StartsWith("<"))
			{
				sPKeyName = sPKeyName.ToLower();
				cTempPKeyCharElement = aPKeyTreeHeadList;
				i = 0;
				for(; i<sPKeyName.Length-1; i++)
				{
					int iIndex = cTempPKeyCharElement.AddCharElement(new PKeyCharElement(sPKeyName[i], Color.Black));
					cTempPKeyCharElement = (PKeyCharElement)cTempPKeyCharElement.aPKeyTreeList[iIndex];
				}
				cTempPKeyCharElement.AddCharElement(new PKeyCharElement(sPKeyName[i], cColor));
			}
		}
		private void AddXmlKey()
		{
			this.AddPrimaryKey("<?xml version=\"1.0\" encoding=\"GB2312\" ?>", Color.Gray);
			
			this.AddPrimaryKey("<BREAK>", Color.DarkRed);
			this.AddPrimaryKey("<IGNORE>", Color.DarkRed);
			this.AddPrimaryKey("</BREAK>", Color.DarkRed);
			this.AddPrimaryKey("</IGNORE>", Color.DarkRed);
			
			
			this.AddPrimaryKey("<SERVER>", Color.Orange);
			this.AddPrimaryKey("<UID>", Color.Orange);
			this.AddPrimaryKey("<PWD>", Color.Orange);
			this.AddPrimaryKey("<DATABASE>", Color.Orange);
			this.AddPrimaryKey("<PROVIDER>", Color.Orange);
			this.AddPrimaryKey("<CONNECT>", Color.Orange);
			this.AddPrimaryKey("<PORT>", Color.Orange);
			this.AddPrimaryKey("</SERVER>", Color.Orange);
			this.AddPrimaryKey("</UID>", Color.Orange);
			this.AddPrimaryKey("</PWD>", Color.Orange);
			this.AddPrimaryKey("</DATABASE>", Color.Orange);
			this.AddPrimaryKey("</PROVIDER>", Color.Orange);
			this.AddPrimaryKey("</CONNECT>", Color.Orange);
			this.AddPrimaryKey("</PORT>", Color.Orange);
			this.AddPrimaryKey("<RECONNECT>", Color.Orange);
			this.AddPrimaryKey("<DISCONNECT>", Color.Orange);
			this.AddPrimaryKey("</RECONNECT>", Color.Orange);
			this.AddPrimaryKey("</DISCONNECT>", Color.Orange);
			this.AddPrimaryKey("<SETCONNECTID>", Color.Orange);
			this.AddPrimaryKey("</SETCONNECTID>", Color.Orange);
			this.AddPrimaryKey("<RUNSERVER>", Color.Orange);
			this.AddPrimaryKey("</RUNSERVER>", Color.Orange);
			this.AddPrimaryKey("<SERVERCMD>", Color.Orange);
			this.AddPrimaryKey("</SERVERCMD>", Color.Orange);
			this.AddPrimaryKey("<REBOOT>", Color.Orange);
			this.AddPrimaryKey("</REBOOT>", Color.Orange);
			this.AddPrimaryKey("<INITDB>", Color.Orange);
			this.AddPrimaryKey("</INITDB>", Color.Orange);

			this.AddPrimaryKey("<TRANS0>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS0>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS1>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS1>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS2>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS2>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS3>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS3>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS4>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS4>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS5>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS5>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS6>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS6>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS7>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS7>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS8>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS8>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS9>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS9>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS10>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS10>", Color.OliveDrab);
			this.AddPrimaryKey("<TRANS11>", Color.OliveDrab);
			this.AddPrimaryKey("</TRANS11>", Color.OliveDrab);

			this.AddPrimaryKey("<ENDTRANS>", Color.OliveDrab);
			this.AddPrimaryKey("<NEWTRANS>", Color.OliveDrab);
			this.AddPrimaryKey("</ENDTRANS>", Color.OliveDrab);
			this.AddPrimaryKey("</NEWTRANS>", Color.OliveDrab);
			this.AddPrimaryKey("<BEGINTRANS>", Color.OliveDrab);
			this.AddPrimaryKey("</BEGINTRANS>", Color.OliveDrab);
			this.AddPrimaryKey("<SLEEP>", Color.OliveDrab);
			this.AddPrimaryKey("</SLEEP>", Color.OliveDrab);

			this.AddPrimaryKey("<SQLTEST>", Color.MidnightBlue);
			this.AddPrimaryKey("<LEVEL>", Color.MidnightBlue);
			this.AddPrimaryKey("<CONTENT>", Color.MidnightBlue);
			this.AddPrimaryKey("<SMES>", Color.MidnightBlue);
			this.AddPrimaryKey("<FMES>", Color.MidnightBlue);
			this.AddPrimaryKey("<SQL_CASE>", Color.MidnightBlue);	
			this.AddPrimaryKey("</SQLTEST>", Color.MidnightBlue);
			this.AddPrimaryKey("</LEVEL>", Color.MidnightBlue);
			this.AddPrimaryKey("</CONTENT>", Color.MidnightBlue);
			this.AddPrimaryKey("</SMES>", Color.MidnightBlue);
			this.AddPrimaryKey("</FMES>", Color.MidnightBlue);
			this.AddPrimaryKey("</SQL_CASE>", Color.MidnightBlue);
			this.AddPrimaryKey("<CASEEXPRESULT>", Color.MidnightBlue);
			this.AddPrimaryKey("</CASEEXPRESULT>", Color.MidnightBlue);

			this.AddPrimaryKey("<LOOP>", Color.MidnightBlue);
			this.AddPrimaryKey("</LOOP>", Color.MidnightBlue);
			this.AddPrimaryKey("<STARTTIMES>", Color.MidnightBlue);
			this.AddPrimaryKey("</STARTTIMES>", Color.MidnightBlue);
			this.AddPrimaryKey("<TIMES>", Color.MidnightBlue);
			this.AddPrimaryKey("</TIMES>", Color.MidnightBlue);

			this.AddPrimaryKey("<MORETHREAD>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</MORETHREAD>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<THREADS>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</THREADS>", Color.DeepSkyBlue);
			
			this.AddPrimaryKey("<TOGETHER>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</TOGETHER>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<UNIT>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</UNIT>", Color.DeepSkyBlue);

			this.AddPrimaryKey("<ENTER>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</ENTER>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<EXIT>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</EXIT>", Color.DeepSkyBlue);

			this.AddPrimaryKey("<NEWCONNECTEXECUTE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</NEWCONNECTEXECUTE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<EXEXML>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</EXEXML>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<EXEPROCESS>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</EXEPROCESS>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<EXEPROCESSEX>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</EXEPROCESSEX>", Color.DeepSkyBlue);

			this.AddPrimaryKey("<PARAMETER>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<CLEARPARAMETERS>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</PARAMETER>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</CLEARPARAMETERS>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<DATATYPE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</DATATYPE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<SIZE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</SIZE>", Color.DeepSkyBlue);

			this.AddPrimaryKey("<TYPE>", Color.Blue);
			this.AddPrimaryKey("<SQL>", Color.Blue);
			this.AddPrimaryKey("<FILESIZE>", Color.Blue);
			this.AddPrimaryKey("</FILESIZE>", Color.Blue);
			this.AddPrimaryKey("<TestPointBegin>", Color.Blue);
			this.AddPrimaryKey("</TestPointBegin>", Color.Blue);
			this.AddPrimaryKey("<SQLSTR>", Color.Blue);
			this.AddPrimaryKey("<SQLSTR1>", Color.Blue);
			this.AddPrimaryKey("<SQLSTR2>", Color.Blue);
			this.AddPrimaryKey("<TEMPSTR>", Color.Blue);
			this.AddPrimaryKey("<EXP>", Color.Blue);
			this.AddPrimaryKey("<NOSHOW>", Color.Blue);
			this.AddPrimaryKey("<NERROR>", Color.Blue);			
			this.AddPrimaryKey("<SQLSTATE>", Color.Blue);
			this.AddPrimaryKey("</SQL>", Color.Blue);	
			this.AddPrimaryKey("</SQLSTR>", Color.Blue);
			this.AddPrimaryKey("</SQLSTR1>", Color.Blue);
			this.AddPrimaryKey("</SQLSTR2>", Color.Blue);
			this.AddPrimaryKey("</TEMPSTR>", Color.Blue);
			this.AddPrimaryKey("</EXP>", Color.Blue);
			this.AddPrimaryKey("</NOSHOW>", Color.Blue);
			this.AddPrimaryKey("</NERROR>", Color.Blue);
			this.AddPrimaryKey("</TYPE>", Color.Blue);
			this.AddPrimaryKey("</SQLSTATE>", Color.Blue);
			this.AddPrimaryKey("<EFFECTROWS>", Color.Blue);
			this.AddPrimaryKey("</EFFECTROWS>", Color.Blue);

			this.AddPrimaryKey("<CLEAR>", Color.Magenta);
			this.AddPrimaryKey("</CLEAR>", Color.Magenta);
			this.AddPrimaryKey("<IF>", Color.Magenta);
			this.AddPrimaryKey("</IF>", Color.Magenta);
			this.AddPrimaryKey("<ELSE>", Color.Magenta);
			this.AddPrimaryKey("</ELSE>", Color.Magenta);

			this.AddPrimaryKey("<!--", Color.Green);
			this.AddPrimaryKey("-->", Color.Green);

			this.AddPrimaryKey("<RESULT>", Color.LimeGreen);
			this.AddPrimaryKey("<RESULTROWS>", Color.LimeGreen);
			this.AddPrimaryKey("<RECORD>", Color.LimeGreen);
			this.AddPrimaryKey("<COLUMN>", Color.LimeGreen);
			this.AddPrimaryKey("<RECORDNUMS>", Color.LimeGreen);
			this.AddPrimaryKey("<COLUMNNUMS>", Color.LimeGreen);
			this.AddPrimaryKey("<COLUMN xml:space=\"preserve\">", Color.LimeGreen);
			this.AddPrimaryKey("</RESULT>", Color.LimeGreen);
			this.AddPrimaryKey("</RESULTROWS>", Color.LimeGreen);
			this.AddPrimaryKey("</RECORD>", Color.LimeGreen);
			this.AddPrimaryKey("</COLUMN>", Color.LimeGreen);
			this.AddPrimaryKey("</RECORDNUMS>", Color.LimeGreen);
			this.AddPrimaryKey("</COLUMNNUMS>", Color.LimeGreen);
			this.AddPrimaryKey("<COMPARERESULT>", Color.LimeGreen);
			this.AddPrimaryKey("<CONNECTID>", Color.LimeGreen);
			this.AddPrimaryKey("</COMPARERESULT>", Color.LimeGreen);
			this.AddPrimaryKey("</CONNECTID>", Color.LimeGreen);

			this.AddPrimaryKey("<OPEN>", Color.LimeGreen);
			this.AddPrimaryKey("<FETCHNEXT>", Color.LimeGreen);
			this.AddPrimaryKey("</OPEN>", Color.LimeGreen);
			this.AddPrimaryKey("</FETCHNEXT>", Color.LimeGreen);

			this.AddPrimaryKey("<READFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</READFILE>", Color.DeepSkyBlue);

			this.AddPrimaryKey("<DATACOMPARE>", Color.LimeGreen);
			this.AddPrimaryKey("</DATACOMPARE>", Color.LimeGreen);
			this.AddPrimaryKey("<DATAFLAG>", Color.LimeGreen);
			this.AddPrimaryKey("</DATAFLAG>", Color.LimeGreen);
			this.AddPrimaryKey("<DATA1>", Color.LimeGreen);
			this.AddPrimaryKey("</DATA1>", Color.LimeGreen);
			this.AddPrimaryKey("<DATA2>", Color.LimeGreen);
			this.AddPrimaryKey("</DATA2>", Color.LimeGreen);

			this.AddPrimaryKey("<BINARY>", Color.LimeGreen);
			this.AddPrimaryKey("</BINARY>", Color.LimeGreen);
			this.AddPrimaryKey("<DATASIZE>", Color.LimeGreen);
			this.AddPrimaryKey("</DATASIZE>", Color.LimeGreen);
			this.AddPrimaryKey("<SEED>", Color.LimeGreen);
			this.AddPrimaryKey("</SEED>", Color.LimeGreen);

			this.AddPrimaryKey("<DELFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</DELFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<COPYFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</COPYFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<OLDFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</OLDFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<NEWFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</NEWFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<CREATEFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</CREATEFILE>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<FILENAME>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</FILENAME>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<WRITEFLAG>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</WRITEFLAG>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<SETDMINI>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</SETDMINI>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<GETDMINI>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</GETDMINI>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<SETVAL>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</SETVAL>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<GETVAL>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</GETVAL>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<VALNAME>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</VALNAME>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<SETAT>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</SETAT>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<VAL>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</VAL>", Color.DeepSkyBlue);
			this.AddPrimaryKey("<INIPATH>", Color.DeepSkyBlue);
			this.AddPrimaryKey("</INIPATH>", Color.DeepSkyBlue);

			this.AddPrimaryKey("SQLSTR", Color.DarkRed);
			this.AddPrimaryKey("SQLSTATE", Color.DarkRed);
			this.AddPrimaryKey("RETCODE", Color.DarkRed);
			this.AddPrimaryKey("EFFECTROWS", Color.DarkRed);
			this.AddPrimaryKey("SQLSTR1", Color.DarkRed);
			this.AddPrimaryKey("SQLSTR2", Color.DarkRed);
			this.AddPrimaryKey("TEMPSTR", Color.DarkRed);
			this.AddPrimaryKey("TIMES", Color.DarkRed);
			this.AddPrimaryKey("RECORDNUMS", Color.DarkRed);
			this.AddPrimaryKey("COLUMNNUMS", Color.DarkRed);
			this.AddPrimaryKey("BOOL", Color.DarkRed);
			this.AddPrimaryKey("FromSql", Color.DarkRed);
			this.AddPrimaryKey("Coverage", Color.Purple);
			this.AddPrimaryKey("Additional", Color.Purple);
			this.AddPrimaryKey("Chaos", Color.Purple);
			this.AddPrimaryKey("ReadCommitted", Color.Purple);
			this.AddPrimaryKey("ReadUncommitted", Color.Purple);
			this.AddPrimaryKey("RepeatableRead", Color.Purple);
			this.AddPrimaryKey("Serializable", Color.Purple);
			this.AddPrimaryKey("Unspecified", Color.Purple);

			this.AddPrimaryKey("DIRECT_EXECUTE_SUCCESS", Color.DarkCyan);
			this.AddPrimaryKey("DIRECT_EXECUTE_FAIL", Color.DarkCyan);
			this.AddPrimaryKey("DIRECT_EXECUTE_SELECT_WITH_RESULT", Color.DarkCyan);
			this.AddPrimaryKey("DIRECT_EXECUTE_SELECT_COMPARE_RESULT", Color.DarkCyan);
			this.AddPrimaryKey("DIRECT_EXECUTE_SELECT_COMPARE_RESULT_FULL", Color.DarkCyan);
			this.AddPrimaryKey("DIRECT_EXECUTE_IGNORE", Color.DarkCyan);
			this.AddPrimaryKey("LOGIN_SUCCESS", Color.DarkCyan);
			this.AddPrimaryKey("LOGIN_FAIL", Color.DarkCyan);

			this.AddPrimaryKey("</TESTPOINTBEGIN>", Color.MidnightBlue);
			this.AddPrimaryKey("<TESTPOINTBEGIN>", Color.MidnightBlue);

			this.AddPrimaryKey("</POOL>", Color.Orange);
			this.AddPrimaryKey("<POOL>", Color.Orange);

			this.AddPrimaryKey("</WINDOWS>", Color.MidnightBlue);
			this.AddPrimaryKey("<WINDOWS>", Color.MidnightBlue);

			this.AddPrimaryKey("</LINUX>", Color.MidnightBlue);
			this.AddPrimaryKey("<LINUX>", Color.MidnightBlue);

			this.AddPrimaryKey("</TIMETICKS>", Color.Blue);
			this.AddPrimaryKey("<TIMETICKS>", Color.Blue);	
		}
		private void AddSqlKey()
		{
			AddPrimaryKey("INT", Color.DodgerBlue);
			AddPrimaryKey("BIT", Color.DodgerBlue);
			AddPrimaryKey("BIGINT", Color.DodgerBlue);
			AddPrimaryKey("TINYINT", Color.DodgerBlue);
			AddPrimaryKey("SMALLINT", Color.DodgerBlue);
			AddPrimaryKey("NUMERIC", Color.DodgerBlue);
			AddPrimaryKey("DECIMAL", Color.DodgerBlue);
			AddPrimaryKey("MONEY", Color.DodgerBlue);
			AddPrimaryKey("DOUBLE", Color.DodgerBlue);
			AddPrimaryKey("PRECISION", Color.DodgerBlue);
			AddPrimaryKey("FLOAT", Color.DodgerBlue);
			AddPrimaryKey("REAL", Color.DodgerBlue);
			AddPrimaryKey("CHAR", Color.DodgerBlue);
			AddPrimaryKey("DATE", Color.DodgerBlue);
			AddPrimaryKey("TIME", Color.DodgerBlue);
			AddPrimaryKey("TIMESTAMP", Color.DodgerBlue);
			AddPrimaryKey("TEXT", Color.DodgerBlue);
			AddPrimaryKey("IMAGE", Color.DodgerBlue);
			AddPrimaryKey("VARCHAR", Color.DodgerBlue);
			AddPrimaryKey("LONGVARCHAR", Color.DodgerBlue);
			AddPrimaryKey("DATETIME", Color.DodgerBlue);
			AddPrimaryKey("BINARY", Color.DodgerBlue);
			AddPrimaryKey("VARBINARY", Color.DodgerBlue);
			AddPrimaryKey("LONGVARBINARY", Color.DodgerBlue);
			AddPrimaryKey("CLOB", Color.DodgerBlue);
			AddPrimaryKey("BLOB", Color.DodgerBlue);

			AddPrimaryKey("ABSOLUTE", Color.Purple);
			AddPrimaryKey("ACTION", Color.Purple);
			AddPrimaryKey("ADD", Color.Purple);
			AddPrimaryKey("AFTER", Color.Purple);
			AddPrimaryKey("ALL", Color.Purple);
			AddPrimaryKey("ALTER", Color.Purple);
			AddPrimaryKey("AND", Color.Purple);
			AddPrimaryKey("ANY", Color.Purple);
			AddPrimaryKey("AS", Color.Purple);
			AddPrimaryKey("ASC", Color.Purple);
			AddPrimaryKey("ASSIGN", Color.Purple);
			AddPrimaryKey("AUDIT", Color.Purple);
			AddPrimaryKey("AUTHORIZATION", Color.Purple);
			AddPrimaryKey("AVG", Color.Purple);
			AddPrimaryKey("BEFORE", Color.Purple);
			AddPrimaryKey("BEGIN", Color.Purple);
			AddPrimaryKey("BETWEEN", Color.Purple);
			AddPrimaryKey("BITMAP", Color.Purple);
			AddPrimaryKey("BOOLEAN", Color.Purple);
			AddPrimaryKey("BOTH", Color.Purple);
			AddPrimaryKey("BY", Color.Purple);
			AddPrimaryKey("CACHE", Color.Purple);
			AddPrimaryKey("CALL", Color.Purple);
			AddPrimaryKey("CASCADE", Color.Purple);
			AddPrimaryKey("CASE", Color.Purple);
			AddPrimaryKey("CATALOG", Color.Purple);
			AddPrimaryKey("CAST", Color.Purple);
			AddPrimaryKey("CHAIN", Color.Purple);
			AddPrimaryKey("CHECK", Color.Purple);
			AddPrimaryKey("CLOSE", Color.Purple);
			AddPrimaryKey("CLUSTER", Color.Purple);
			AddPrimaryKey("COALESCE", Color.Purple);
			AddPrimaryKey("COLUMN", Color.Purple);
			AddPrimaryKey("COMMIT", Color.Purple);
			AddPrimaryKey("COMMITTED", Color.Purple);
			AddPrimaryKey("COMMITWORK", Color.Purple);
			AddPrimaryKey("COMPILE", Color.Purple);
			AddPrimaryKey("CONNECT", Color.Purple);
			AddPrimaryKey("CONSTRAINT", Color.Purple);
			AddPrimaryKey("CONVERT", Color.Purple);
			AddPrimaryKey("COUNT", Color.Purple);
			AddPrimaryKey("CREATE", Color.Purple);
			AddPrimaryKey("CROSS", Color.Purple);
			AddPrimaryKey("CURRENT", Color.Purple);
			AddPrimaryKey("CURSOR", Color.Purple);
			AddPrimaryKey("CYCLE", Color.Purple);
			AddPrimaryKey("DATABASE", Color.Purple);
			AddPrimaryKey("DATAFILE", Color.Purple);
			AddPrimaryKey("DATEADD", Color.Purple);
			AddPrimaryKey("DATEDIFF", Color.Purple);
			AddPrimaryKey("DATEPART", Color.Purple);
			AddPrimaryKey("DEBUG", Color.Purple);
			AddPrimaryKey("DECLARE", Color.Purple);
			AddPrimaryKey("DECODE", Color.Purple);
			AddPrimaryKey("DEFAULT", Color.Purple);
			AddPrimaryKey("DEFERRABLE", Color.Purple);
			AddPrimaryKey("DELETE", Color.Purple);
			AddPrimaryKey("DELETING", Color.Purple);
			AddPrimaryKey("DESC", Color.Purple);
			AddPrimaryKey("DISCONNECT", Color.Purple);
			AddPrimaryKey("DISABLE", Color.Purple);
			AddPrimaryKey("DISTINCT", Color.Purple);
			AddPrimaryKey("DO", Color.Purple);
			AddPrimaryKey("DOWNTO", Color.Purple);
			AddPrimaryKey("DROP", Color.Purple);
			AddPrimaryKey("EACH", Color.Purple);
			AddPrimaryKey("ELSE", Color.Purple);
			AddPrimaryKey("ELSEIF", Color.Purple);
			AddPrimaryKey("ELSIF", Color.Purple);
			AddPrimaryKey("ENABLE", Color.Purple);
			AddPrimaryKey("END", Color.Purple);
			AddPrimaryKey("ESCAPE", Color.Purple);
			AddPrimaryKey("EXCEPTION", Color.Purple);
			AddPrimaryKey("EXCLUSIVE", Color.Purple);
			AddPrimaryKey("EXECUTE", Color.Purple);
			AddPrimaryKey("EXISTS", Color.Purple);
			AddPrimaryKey("EXIT", Color.Purple);
			AddPrimaryKey("EXPLAIN", Color.Purple);
			AddPrimaryKey("EXTERNAL", Color.Purple);
			AddPrimaryKey("EXTRACT", Color.Purple);
			AddPrimaryKey("FALSE", Color.Purple);
			AddPrimaryKey("FETCH", Color.Purple);
			AddPrimaryKey("FILLFACTOR", Color.Purple);
			AddPrimaryKey("FIRST", Color.Purple);
			AddPrimaryKey("FOR", Color.Purple);
			AddPrimaryKey("FOREIGN", Color.Purple);
			AddPrimaryKey("FOUND", Color.Purple);
			AddPrimaryKey("FROM", Color.Purple);
			AddPrimaryKey("FULL", Color.Purple);
			AddPrimaryKey("FUNCTION", Color.Purple);
			AddPrimaryKey("GLOBAL", Color.Purple);
			AddPrimaryKey("GOTO", Color.Purple);
			AddPrimaryKey("GRANT", Color.Purple);
			AddPrimaryKey("GROUP", Color.Purple);
			AddPrimaryKey("HAVING", Color.Purple);
			AddPrimaryKey("HEXTORAW", Color.Purple);
			AddPrimaryKey("IDENTIFIED", Color.Purple);
			AddPrimaryKey("IDENTIFY", Color.Purple);
			AddPrimaryKey("IDENTITY", Color.Purple);
			AddPrimaryKey("IDENTITY_INSERT", Color.Purple);
			AddPrimaryKey("IF", Color.Purple);
			AddPrimaryKey("IFNULL", Color.Purple);
			AddPrimaryKey("IMMEDIATE", Color.Purple);
			AddPrimaryKey("IN", Color.Purple);
			AddPrimaryKey("INCREASE", Color.Purple);
			AddPrimaryKey("INCREMENT", Color.Purple);
			AddPrimaryKey("INDEX", Color.Purple);
			AddPrimaryKey("INITIAL", Color.Purple);
			AddPrimaryKey("INITIALLY", Color.Purple);
			AddPrimaryKey("INNER", Color.Purple);
			AddPrimaryKey("INSERT", Color.Purple);
			AddPrimaryKey("INSERTING", Color.Purple);
			AddPrimaryKey("INTENTION", Color.Purple);
			AddPrimaryKey("INTERVAL", Color.Purple);
			AddPrimaryKey("INTO", Color.Purple);
			AddPrimaryKey("IS", Color.Purple);
			AddPrimaryKey("ISNULL", Color.Purple);
			AddPrimaryKey("ISOLATION", Color.Purple);
			AddPrimaryKey("ISOPEN", Color.Purple);
			AddPrimaryKey("JOIN", Color.Purple);
			AddPrimaryKey("KEY", Color.Purple);
			AddPrimaryKey("LAST", Color.Purple);
			AddPrimaryKey("LEADING", Color.Purple);
			AddPrimaryKey("LEFT", Color.Purple);
			AddPrimaryKey("LEVEL", Color.Purple);
			AddPrimaryKey("LIKE", Color.Purple);
			AddPrimaryKey("LOCAL", Color.Purple);
			AddPrimaryKey("LOCK", Color.Purple);
			AddPrimaryKey("LOOP", Color.Purple);
			AddPrimaryKey("MATCH", Color.Purple);
			AddPrimaryKey("MAX", Color.Purple);
			AddPrimaryKey("MAXVALUE", Color.Purple);
			AddPrimaryKey("MIN", Color.Purple);
			AddPrimaryKey("MINEXTENTS", Color.Purple);
			AddPrimaryKey("MINVALUE", Color.Purple);
			AddPrimaryKey("MODE", Color.Purple);
			AddPrimaryKey("MODIFY", Color.Purple);
			AddPrimaryKey("NATURAL", Color.Purple);
			AddPrimaryKey("NEW", Color.Purple);
			AddPrimaryKey("NEXT", Color.Purple);
			AddPrimaryKey("NOAUDIT", Color.Purple);
			AddPrimaryKey("NOCACHE", Color.Purple);
			AddPrimaryKey("NOCYCLE", Color.Purple);
			AddPrimaryKey("NOMAXVALUE", Color.Purple);
			AddPrimaryKey("NOMINVALUE", Color.Purple);
			AddPrimaryKey("NOORDER", Color.Purple);
			AddPrimaryKey("NOT", Color.Purple);
			AddPrimaryKey("NOTFOUND", Color.Purple);
			AddPrimaryKey("NULL", Color.Purple);
			AddPrimaryKey("NULLIF", Color.Purple);
			AddPrimaryKey("NVL", Color.Purple);
			AddPrimaryKey("OF", Color.Purple);
			AddPrimaryKey("OFF", Color.Purple);
			AddPrimaryKey("OLD", Color.Purple);
			AddPrimaryKey("ON", Color.Purple);
			AddPrimaryKey("ONLY", Color.Purple);
			AddPrimaryKey("OPEN", Color.Purple);
			AddPrimaryKey("OPTION", Color.Purple);
			AddPrimaryKey("OR", Color.Purple);
			AddPrimaryKey("ORDER", Color.Purple);
			AddPrimaryKey("OUT", Color.Purple);
			AddPrimaryKey("OUTER", Color.Purple);
			AddPrimaryKey("OVERLAPS", Color.Purple);
			AddPrimaryKey("PARTIAL", Color.Purple);
			AddPrimaryKey("PENDANT", Color.Purple);
			AddPrimaryKey("PERCENT", Color.Purple);
			AddPrimaryKey("PRECISION", Color.Purple);
			AddPrimaryKey("PRESERVE", Color.Purple);
			AddPrimaryKey("PRIMARY", Color.Purple);
			AddPrimaryKey("PRINT", Color.Purple);
			AddPrimaryKey("PRIOR", Color.Purple);
			AddPrimaryKey("PRIVILEGES", Color.Purple);
			AddPrimaryKey("PROCEDURE", Color.Purple);
			AddPrimaryKey("RAISE", Color.Purple);
			AddPrimaryKey("RAWTOHEX", Color.Purple);
			AddPrimaryKey("READ", Color.Purple);
			AddPrimaryKey("REFERENCES", Color.Purple);
			AddPrimaryKey("REFERENCING", Color.Purple);
			AddPrimaryKey("RELATIVE", Color.Purple);
			AddPrimaryKey("RENAME", Color.Purple);
			AddPrimaryKey("REPEAT", Color.Purple);
			AddPrimaryKey("REPEATABLE", Color.Purple);
			AddPrimaryKey("REPLACE", Color.Purple);
			AddPrimaryKey("RESTRICT", Color.Purple);
			AddPrimaryKey("RETURN", Color.Purple);
			AddPrimaryKey("REVERSE", Color.Purple);
			AddPrimaryKey("REVOKE", Color.Purple);
			AddPrimaryKey("RIGHT", Color.Purple);
			AddPrimaryKey("ROLE", Color.Purple);
			AddPrimaryKey("ROLLBACK", Color.Purple);
			AddPrimaryKey("ROW", Color.Purple);
			AddPrimaryKey("ROWCOUNT", Color.Purple);
			AddPrimaryKey("ROWNUM", Color.Purple);
			AddPrimaryKey("ROWS", Color.Purple);
			AddPrimaryKey("SAVEPOINT", Color.Purple);
			AddPrimaryKey("SCHEM", Color.Purple);
			AddPrimaryKey("SELECT", Color.Purple);
			AddPrimaryKey("SERIALIZABLE", Color.Purple);
			AddPrimaryKey("SEQUENCE", Color.Purple);
			AddPrimaryKey("SET", Color.Purple);
			AddPrimaryKey("SHARE", Color.Purple);
			AddPrimaryKey("SOME", Color.Purple);
			AddPrimaryKey("SQL", Color.Purple);
			AddPrimaryKey("STATEMENT", Color.Purple);
			AddPrimaryKey("STORAGE", Color.Purple);
			AddPrimaryKey("SUBSTRING", Color.Purple);
			AddPrimaryKey("SUCCESSFUL", Color.Purple);
			AddPrimaryKey("SUM", Color.Purple);
			AddPrimaryKey("TABLE", Color.Purple);
			AddPrimaryKey("TEMPORARY", Color.Purple);
			AddPrimaryKey("THEN", Color.Purple);
			AddPrimaryKey("TIES", Color.Purple);
			AddPrimaryKey("TIMESTAMPADD", Color.Purple);
			AddPrimaryKey("TIMESTAMPDIFF", Color.Purple);
			AddPrimaryKey("TO", Color.Purple);
			AddPrimaryKey("TOP", Color.Purple);
			AddPrimaryKey("TRAILING", Color.Purple);
			AddPrimaryKey("TRANSACTION", Color.Purple);
			AddPrimaryKey("TRIGGER", Color.Purple);
			AddPrimaryKey("TRIGGERS", Color.Purple);
			AddPrimaryKey("TRIM", Color.Purple);
			AddPrimaryKey("TRUE", Color.Purple);
			AddPrimaryKey("TRUNCATE", Color.Purple);
			AddPrimaryKey("TYPECAST", Color.Purple);
			AddPrimaryKey("UNCOMMITED", Color.Purple);
			AddPrimaryKey("UNION", Color.Purple);
			AddPrimaryKey("UNIQUE", Color.Purple);
			AddPrimaryKey("UNTIL", Color.Purple);
			AddPrimaryKey("UPDATE", Color.Purple);
			AddPrimaryKey("UPDATING", Color.Purple);
			AddPrimaryKey("USER", Color.Purple);
			AddPrimaryKey("USING", Color.Purple);
			AddPrimaryKey("VALUES", Color.Purple);
			AddPrimaryKey("VARYING", Color.Purple);
			AddPrimaryKey("VIEW", Color.Purple);
			AddPrimaryKey("VSIZE", Color.Purple);
			AddPrimaryKey("WHEN", Color.Purple);
			AddPrimaryKey("WHENEVER", Color.Purple);
			AddPrimaryKey("WHERE", Color.Purple);
			AddPrimaryKey("WHILE", Color.Purple);
			AddPrimaryKey("WITH", Color.Purple);
			AddPrimaryKey("WORK", Color.Purple);
			AddPrimaryKey("WRITE", Color.Purple);			
		}
	}
	public class PKeyCharElement//代表关键字除最后一个字符以外的字符元素
	{
		public const int NOTINSERT = -1;//代表字符没有被插入到树中
		public const int NOTFOUND = -1;	//没有在字节点中发现字符串
		public char chKeyChar;
		public ArrayList aPKeyTreeList;	
		public Color cFontColor;
		public PKeyCharElement(char chPKChar, Color cColor)
		{
			aPKeyTreeList = new ArrayList();
			cFontColor = cColor;
			chKeyChar = chPKChar;
		}

		public int AddCharElement(PKeyCharElement cPKeyCharElement)
		{
			int iIndex = NOTINSERT;
			for(int i=0; i<aPKeyTreeList.Count; i++)
			{
				if(cPKeyCharElement.chKeyChar == ((PKeyCharElement)aPKeyTreeList[i]).chKeyChar)
				{
					iIndex = i;
					if(cPKeyCharElement.cFontColor != Color.Black)
					{
						((PKeyCharElement)aPKeyTreeList[i]).cFontColor = cPKeyCharElement.cFontColor;
					}
					break;
				}
				else if(cPKeyCharElement.chKeyChar < ((PKeyCharElement)aPKeyTreeList[i]).chKeyChar)
				{
					aPKeyTreeList.Insert(i, cPKeyCharElement);
					iIndex = i;
					break;
				}
			}
			if(iIndex == NOTINSERT)//没有被插入到树，那么就加进去
			{
				aPKeyTreeList.Add(cPKeyCharElement);
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
				if(chKeyChar == ((PKeyCharElement)aPKeyTreeList[iMid]).chKeyChar)
				{
					return iMid;
				}
				else if(chKeyChar < ((PKeyCharElement)aPKeyTreeList[iMid]).chKeyChar)
					iHigh = iMid-1;
				else 
					iLow = iMid+1;
			}
			return NOTFOUND;
		}
	}
}
