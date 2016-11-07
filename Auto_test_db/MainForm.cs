#define DM7
#undef DM6
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

#if DM7
using Dm;
#else
using System.Data.OleDb;
#endif
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

namespace Auto_test_db
{
	/// <summary>
	/// 测试线程的结构，用来管理测试线程
	/// </summary>
	public struct THREAD_STRUCT
	{
		public Thread m_thread;
		public TestThread m_TestThread;
		public string m_xmlFileName;
		public int m_xmlIndex;			//在选中文件数组中的索引
		public DateTime m_DateTime;
	}
	public struct RWArr//用于记录测试结果信息，然后将其记录的信息保存到mytestarr数组中
	{
		public string TestExName;//用于记录测试用例的文件名
		public string TestDateTime;//用于记录测试此测试用例的时刻
		public string TestResult;//用于记录此测试用例的测试结果，即是“成功”还是“失败”
	}
	
	/// <summary>
	/// 主窗口
	/// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
#if DM7
        public const string sVersion = " -- 版本(1.0.8.0 Build2012.10.11(DM7))";
#else
        public const string sVersion = " -- 版本(1.0.8.0 Build2012.10.11(DM6))";
#endif
        ///Tag变量值为-1,说明该节点不是有效的节点，是用来显示XML文件测试描述或是其它信息的
        ///Tag变量值为0, 说明该节点是代表一个文件夹
        ///Tag变量值为1, 说明该节点是代表一个XML测试文件
        ///Tag变量值为2, 说明该节点是代表一个RTF记录文件
        ///Tag变量值为3, 说明该节点是代表一个RTF记录文件夹
        ///Tag变量值为4, 说明该节点是代表一个XML执行报错时，产生的SQL语句记录文件夹
        ///Tag变量值为5, 说明该节点是代表一个XML执行报错时，产生的SQL语句记录文件
        public const int NONE = -1;
        public const int XMLDIRECTORY = 0;
        public const int XMLFILE = 1;
        public const int RTFFILE = 2;
        public const int RTFDIRECTORY = 3;
        public const int LOGRTFDIRECTORY = 4;
		//public static XmlTest xml;
        private System.Windows.Forms.ImageList m_imageList;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem cMenuItemSpSu;
        private System.Windows.Forms.MenuItem cMenuItemSpFa;
        private System.Windows.Forms.Button button_continue;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Button button_Start_WF;
        private System.Windows.Forms.Button button_End_RSF;

        public const int LOGFILE = 5;
        public delegate void AddTextToTextBoxSuccessDelegate(string m_text, int m_id, bool m_fail);	//往显示所有消息框中添加消息的委托函数
        public delegate void AddTextToTextBoxFailDelegate(string m_text, int m_id, bool iNoVoice);		//往显示失败消息框中添加消息的委托函数
        public delegate void AddNodeDelegate(TreeNode tNode, string sName, bool bDi);
        public delegate void DeleteNodeDelegate(TreeNode tNode);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr OpenThread(UInt32 dwDesiredAccess, bool bInheritHandle, UInt32 dwThreadId);
        [DllImport("Kernel32.dll")]
        private static extern bool TerminateThread(IntPtr handle, int exitcode);
        [DllImport("User32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int nMaxCount);
        [DllImport("User32.dll")]
        private static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath);
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Splitter splitter2;
        public  System.Windows.Forms.TreeView treeView_xml;				//节点树
        private System.Windows.Forms.Button button_start;				//开始按钮
        private System.Windows.Forms.RichTextBox richTextBox_fail;		//显示所有消息的文本框
        private System.Windows.Forms.RichTextBox richTextBox_success;	//显示错误消息的文本框
        private System.Windows.Forms.Button button_finish;				//完成按钮
        private System.Windows.Forms.Button button_finish_all;			//终止按钮
        private System.Windows.Forms.Button button_set;					//设置按钮
        private MenuItem m_treeReflesh;									//树刷新菜单
        private MenuItem m_treeOpen;									//树节点打开操作菜单
        private MenuItem m_treeRun;										//树节点运行操作菜单
        private MenuItem m_treeXmlEdit;									//用脚本编辑器打开
		private MenuItem m_treeImportTestResult;						//把测试结果导入到数据库中
		private MenuItem m_treeClearTestResult;							//清空测试结果
        //		private MenuItem m_treeNewDi;									//树节点新建文件夹操作菜单
        //		private MenuItem m_treeReName;									//树节点重命名文件操作菜单
        //		private MenuItem m_treeDelete;									//树节点删除文件操作菜单
        private Queue oGetXmlQueue = new Queue();						//在获取测试文件索引，保证只有一个线程执行那个函数
        private Queue oSqlCaseQueue = new Queue();						//在获取m_SqlCase的使用权，保证只有一个线程执行
        private TreeNode m_TopNode;										//测试用例的第一个结点
        private TreeNode m_MonuseNode;									//在鼠标位置上的结点
        private SQL_CASE m_SqlCase;										//指向最近执行语句的结构链

        public AddTextToTextBoxSuccessDelegate AddSuccessText;			//定义往显示所有消息框中添加消息的委托函数
        public AddTextToTextBoxFailDelegate AddFailText;				//定义往显示失败消息框中添加消息的委托函数
        public AddNodeDelegate AddNodeDl;
        public DeleteNodeDelegate DeleteNodeDl;
        public THREAD_STRUCT[] m_thread_struct;							//管理测试线程的结构
        public int m_TreeNodeIndex;									//指示当前被创建的线程所用xml文件在左边树中的索引
        private ArrayList m_XmlFileList;								//用来存放当前被选中的XML文件列表
        public ArrayList m_XmlSelectNodeList;	//用来存放选中的树节点。跟上面一个是数组是一一对应的
        public static ArrayList mytestarr;      //用于保存测试结果的信息
        public string w_path;
        public int m_message_num;										//用来表示当前消息框已经显示了多少条消息
        public bool m_stopTest;											//指示是否要停下测试线程
        public bool m_runLog;											//指示要运行的时否是测试产生的SQL语句记录文件
        public bool m_IsLogText;										//指示当前消息框中显示的是否是历史消息
        private bool bOwnCheck;											//指出当前的CHECK消息是程序内部自己发出来的
        private int m_FailNum;											//指示当前测试用例总共有多少个失败
        private int m_SuccessNum;										//指示当前测试用例总共有多少个成功
        private System.Threading.Timer m_CheckMessageTimer;				//用来定时访问各个线程所发出来消息，以便了解它们是否还在正常运行

        private bool m_HaveNodeCheck;									//指示当前树中是否有节点被打勾
        private bool bIni;												//指示程序已经运行过一次了

        private FileSystemWatcher watcher;								//文件目录监视对像
        private System.Windows.Forms.Button button_Clear;				//清除按钮
        private System.Windows.Forms.MainMenu FileMenu;					//主菜单
        private System.Windows.Forms.MenuItem menuItem_F;				//刷新树菜单
        private System.Windows.Forms.MenuItem menuItem_S;				//设置主菜单
        private System.Windows.Forms.MenuItem menuItem_refleshTree;		//刷新树菜单
        private System.Windows.Forms.MenuItem menuItem_SaveFile;		//保存消息菜单
        private System.Windows.Forms.MenuItem menuItem_Exit;			//退出菜单
        private System.Windows.Forms.MenuItem menuItem_ShowMessage;		//设置是否显示消息菜单
        private System.Windows.Forms.MenuItem menuItem_Warning;			//设置是否发声报警的菜单
        private System.Windows.Forms.MenuItem menuItem_AutoSave;		//设置是否是自动保存消息菜单
        private System.Windows.Forms.MenuItem menuItem_AutoClear;		//设置是否在每次点击开始按钮时，自动清空消息区的菜单
        private System.Windows.Forms.MenuItem menuItem_Loop;			//设置是否循环测试菜单
        private System.Windows.Forms.MenuItem menuItem_Set;				//树节点图像列表
        private System.Windows.Forms.MenuItem menuItem1;				//关于菜单
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem_OpenFile;				//帮助菜单
        private System.ComponentModel.IContainer components;

        private ProClass proVal;
        public MainForm()
        {
            //8.27 修改 主窗体运行即连IP
			//string conn=ProClass.CommServer.ConnectSev();

            //
            // Windows 窗体设计器支持所必需的
            //
            InitializeComponent();
            this.Text += sVersion;//主窗体的名字
            //
            // TODO: 在 InitializeComponent 调用后添加任何构造函数代码
            //创建存放测试结果的目录
            try
            {
                DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory);
                m_di.CreateSubdirectory("测试结果");

            }
            catch (Exception e)
            {
                string m_txt = e.Message;
            }
            m_stopTest = true;
            m_SqlCase = null;

            //下面设置左边树显示的一些特性
            treeView_xml.HideSelection = false;		//失去焦点时，也显示选中的节点
            treeView_xml.HotTracking = true;		//显示下划线
            bOwnCheck = false;

            //下面为树列表，加右建菜单
            MenuItem myMenuItemFile;
            treeView_xml.ContextMenu = new ContextMenu();
            m_treeReflesh = new MenuItem("刷新(&F)", (new System.EventHandler(this.menuFlushNode_Click)), Shortcut.CtrlF);
            m_treeOpen = new MenuItem("打开(&O)", (new System.EventHandler(this.menuOpenNode_Click)), Shortcut.CtrlQ);
            m_treeRun = new MenuItem("运行(&R)", (new System.EventHandler(this.button_start_Click)), Shortcut.CtrlR);
            m_treeXmlEdit = new MenuItem("编辑(&E)", (new System.EventHandler(this.button_edit_Click)), Shortcut.CtrlE);
			m_treeImportTestResult = new MenuItem("导入数据库(&I)", (new System.EventHandler(this.button_import_Click)), Shortcut.CtrlI);
			m_treeClearTestResult = new MenuItem("清空(&C)", (new System.EventHandler(this.button_clear_Click)), Shortcut.None);
            //	m_treeNewDi = new MenuItem("新建文件夹(&N)", (new System.EventHandler(this.menuNewNode_Click)), Shortcut.CtrlN);									//树节点新建文件夹操作菜单
            //	m_treeReName = new MenuItem("重命令(&M)", (new System.EventHandler(this.menuReName_Click)), Shortcut.CtrlM);									//树节点重命名文件操作菜单
            //	m_treeDelete = new MenuItem("删除(&D)", (new System.EventHandler(this.menuFlushNode_Click)), Shortcut.CtrlD);									//树节点删除文件操作菜单
           treeView_xml.ContextMenu.MenuItems.Add(m_treeReflesh);
            //刷新树
            menuItem_refleshTree.Click += new System.EventHandler(this.menuFlushNode_Click);
            menuItem_refleshTree.Shortcut = Shortcut.CtrlF;
            //保存消息菜单
            menuItem_SaveFile.Click += new System.EventHandler(this.menuSaveFile_Click);
            menuItem_SaveFile.Shortcut = Shortcut.CtrlS;
            //退出程序
            menuItem_Exit.Click += new System.EventHandler(this.menuExit_Click);
            menuItem_Exit.Shortcut = Shortcut.CtrlX;
            //消息显示菜单
            menuItem_ShowMessage.Click += new System.EventHandler(this.menuShowMessage_Click);
            menuItem_ShowMessage.Shortcut = Shortcut.CtrlP;
            //声音报警菜单
            menuItem_Warning.Click += new System.EventHandler(this.menuWaring_Click);
            menuItem_Warning.Shortcut = Shortcut.CtrlW;
            //自动保存消息菜单
            menuItem_AutoSave.Click += new System.EventHandler(this.menuAutoSave_Click);
            menuItem_AutoSave.Shortcut = Shortcut.CtrlU;
            //自动清空消息菜单
            menuItem_AutoClear.Click += new System.EventHandler(this.menuAutoClear_Click);
            menuItem_AutoClear.Shortcut = Shortcut.CtrlL;
            //循环测试
            menuItem_Loop.Click += new System.EventHandler(this.menuLoop_Click);
            menuItem_Loop.Shortcut = Shortcut.CtrlK;
            //参数设置菜单
            menuItem_Set.Click += new System.EventHandler(this.button_set_Click);
            menuItem_Set.Shortcut = Shortcut.CtrlT;
            //为显示错误的消息框加右键菜单
            richTextBox_fail.ContextMenu = new ContextMenu();
            myMenuItemFile = new MenuItem("拷贝(&Y)");
            myMenuItemFile.Shortcut = Shortcut.CtrlC;
            richTextBox_fail.ContextMenu.MenuItems.Add(myMenuItemFile);
            myMenuItemFile.Click += new System.EventHandler(this.menuClearAndCopyMessage_Click);
            myMenuItemFile = new MenuItem("清空(&C)");
            richTextBox_fail.ContextMenu.MenuItems.Add(myMenuItemFile);
            myMenuItemFile.Click += new System.EventHandler(this.menuClearAndCopyMessage_Click);
            //为显示所有消息框加右键菜单
            richTextBox_success.ContextMenu = richTextBox_fail.ContextMenu;
            //初始化一些运行时变量
            m_message_num = 0;			//消息框中已有的消息条数，该值会在每次保存完当前消息后清0
            m_TreeNodeIndex = 0;		//当前运行的文件，在选中的树节点文件列表中的索引。该值会在要求循环测试时，到达列表最后一个文件后，再跳回第一个开始

            m_XmlFileList = new ArrayList();		//该数组会存放当前用户选中树中的所有文件全路径
            m_XmlSelectNodeList = new ArrayList();	//该数组存放选中的树节点
            mytestarr = new ArrayList();
            AddSuccessText = new AddTextToTextBoxSuccessDelegate(AddToTextBoxSuccess);//初始化委拖函数
            AddFailText = new AddTextToTextBoxFailDelegate(AddToTextBoxFail);
            AddNodeDl = new AddNodeDelegate(AddNode);
            DeleteNodeDl = new DeleteNodeDelegate(DeleteNode);
			try
			{
				proVal = new ProClass(this);
			}
			catch(Exception e)
			{
				AddToTextBoxFail(e.Message, -1);
			}
            ProClass.cDServer.SetConnectStr(CreateConnectStr());


            FillTreeView();				//填充树列表，把当前目录下所有的XML文件追加进节点
			//8.27 修改 主窗体运行 填充树列表后 即连IP  放在前面了

			//8.27 (2)修改  以后的每一步都是独立通信的 这一语句不要
			//string conn=ProClass.CommServer.ConnectSev();



            //		OpenFile(Environment.CurrentDirectory + "\\测试结果\\Default.lst");
            //创建一个定时器，每三百秒检察一下各线程的最新消息时间和当前时间的间隔，以便判断它们是否还正常运行
            TimerCallback timerDelegate1 = new TimerCallback(CheckMessageTime);
            m_CheckMessageTimer = new System.Threading.Timer(timerDelegate1, this, 1000, ProClass.GetMsgCheckTime() * 1000);
            m_IsLogText = false;//指示当前消息框中显示的是否是历史消息
            this.AcceptButton = button_start;
            this.CancelButton = button_finish;
            CreateWatcher();
        }    //MainForm结束点

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码
        /// <summary>
        /// 设计器支持所需的方法 - 不要使用代码编辑器修改
        /// 此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.treeView_xml = new System.Windows.Forms.TreeView();
            this.m_imageList = new System.Windows.Forms.ImageList(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.richTextBox_success = new System.Windows.Forms.RichTextBox();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.richTextBox_fail = new System.Windows.Forms.RichTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button_End_RSF = new System.Windows.Forms.Button();
            this.button_Start_WF = new System.Windows.Forms.Button();
            this.button_continue = new System.Windows.Forms.Button();
            this.button_Clear = new System.Windows.Forms.Button();
            this.button_set = new System.Windows.Forms.Button();
            this.button_finish_all = new System.Windows.Forms.Button();
            this.button_finish = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.FileMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem_F = new System.Windows.Forms.MenuItem();
            this.menuItem_OpenFile = new System.Windows.Forms.MenuItem();
            this.menuItem_SaveFile = new System.Windows.Forms.MenuItem();
            this.menuItem_refleshTree = new System.Windows.Forms.MenuItem();
            this.menuItem_Exit = new System.Windows.Forms.MenuItem();
            this.menuItem_S = new System.Windows.Forms.MenuItem();
            this.menuItem_ShowMessage = new System.Windows.Forms.MenuItem();
            this.menuItem_Warning = new System.Windows.Forms.MenuItem();
            this.menuItem_AutoSave = new System.Windows.Forms.MenuItem();
            this.menuItem_AutoClear = new System.Windows.Forms.MenuItem();
            this.menuItem_Loop = new System.Windows.Forms.MenuItem();
            this.menuItem_Set = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.cMenuItemSpSu = new System.Windows.Forms.MenuItem();
            this.cMenuItemSpFa = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView_xml
            // 
            this.treeView_xml.BackColor = System.Drawing.Color.WhiteSmoke;
            this.treeView_xml.CheckBoxes = true;
            this.treeView_xml.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView_xml.Location = new System.Drawing.Point(0, 0);
            this.treeView_xml.Name = "treeView_xml";
            this.treeView_xml.Size = new System.Drawing.Size(200, 469);
            this.treeView_xml.TabIndex = 0;
            this.treeView_xml.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_xml_BeforeCheck);
            this.treeView_xml.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView_xml_AfterCheck);
            this.treeView_xml.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeView_xml_AfterCollapse);
            this.treeView_xml.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView_xml_AfterExpand);
            this.treeView_xml.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_xml_AfterSelect);
            this.treeView_xml.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_xml_KeyDown);
            this.treeView_xml.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_xml_MouseDown);
            // 
            // m_imageList
            // 
            this.m_imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_imageList.ImageStream")));
            this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.m_imageList.Images.SetKeyName(0, "");
            this.m_imageList.Images.SetKeyName(1, "");
            this.m_imageList.Images.SetKeyName(2, "");
            this.m_imageList.Images.SetKeyName(3, "");
            this.m_imageList.Images.SetKeyName(4, "");
            this.m_imageList.Images.SetKeyName(5, "");
            this.m_imageList.Images.SetKeyName(6, "");
            this.m_imageList.Images.SetKeyName(7, "");
            this.m_imageList.Images.SetKeyName(8, "");
            this.m_imageList.Images.SetKeyName(9, "");
            this.m_imageList.Images.SetKeyName(10, "");
            this.m_imageList.Images.SetKeyName(11, "");
            this.m_imageList.Images.SetKeyName(12, "");
            this.m_imageList.Images.SetKeyName(13, "");
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(200, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 469);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.richTextBox_success);
            this.panel1.Controls.Add(this.splitter2);
            this.panel1.Controls.Add(this.richTextBox_fail);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(203, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(911, 469);
            this.panel1.TabIndex = 3;
            // 
            // richTextBox_success
            // 
            this.richTextBox_success.BackColor = System.Drawing.Color.WhiteSmoke;
            this.richTextBox_success.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_success.Location = new System.Drawing.Point(0, 0);
            this.richTextBox_success.Name = "richTextBox_success";
            this.richTextBox_success.ReadOnly = true;
            this.richTextBox_success.Size = new System.Drawing.Size(911, 269);
            this.richTextBox_success.TabIndex = 3;
            this.richTextBox_success.Text = "";
            this.richTextBox_success.TextChanged += new System.EventHandler(this.richTextBox_success_TextChanged);
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 269);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(911, 8);
            this.splitter2.TabIndex = 2;
            this.splitter2.TabStop = false;
            // 
            // richTextBox_fail
            // 
            this.richTextBox_fail.AutoWordSelection = true;
            this.richTextBox_fail.BackColor = System.Drawing.Color.WhiteSmoke;
            this.richTextBox_fail.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBox_fail.ForeColor = System.Drawing.Color.Red;
            this.richTextBox_fail.Location = new System.Drawing.Point(0, 277);
            this.richTextBox_fail.Name = "richTextBox_fail";
            this.richTextBox_fail.ReadOnly = true;
            this.richTextBox_fail.Size = new System.Drawing.Size(911, 152);
            this.richTextBox_fail.TabIndex = 1;
            this.richTextBox_fail.Text = "";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.button_End_RSF);
            this.panel2.Controls.Add(this.button_Start_WF);
            this.panel2.Controls.Add(this.button_continue);
            this.panel2.Controls.Add(this.button_Clear);
            this.panel2.Controls.Add(this.button_set);
            this.panel2.Controls.Add(this.button_finish_all);
            this.panel2.Controls.Add(this.button_finish);
            this.panel2.Controls.Add(this.button_start);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 429);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(911, 40);
            this.panel2.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(176, 40);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "与服务器端通信";
            // 
            // button_End_RSF
            // 
            this.button_End_RSF.ForeColor = System.Drawing.Color.Red;
            this.button_End_RSF.Location = new System.Drawing.Point(440, 8);
            this.button_End_RSF.Name = "button_End_RSF";
            this.button_End_RSF.Size = new System.Drawing.Size(136, 23);
            this.button_End_RSF.TabIndex = 7;
            this.button_End_RSF.Text = "结束保存测试结果信息";
            this.button_End_RSF.Click += new System.EventHandler(this.button_End_RSF_Click);
            // 
            // button_Start_WF
            // 
            this.button_Start_WF.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.button_Start_WF.Location = new System.Drawing.Point(8, 8);
            this.button_Start_WF.Name = "button_Start_WF";
            this.button_Start_WF.Size = new System.Drawing.Size(136, 23);
            this.button_Start_WF.TabIndex = 6;
            this.button_Start_WF.Text = "开始保存测试结果信息";
            this.button_Start_WF.Click += new System.EventHandler(this.button_Start_WF_Click);
            // 
            // button_continue
            // 
            this.button_continue.Enabled = false;
            this.button_continue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.button_continue.Location = new System.Drawing.Point(224, 8);
            this.button_continue.Name = "button_continue";
            this.button_continue.Size = new System.Drawing.Size(64, 24);
            this.button_continue.TabIndex = 5;
            this.button_continue.Text = "继续(&T)";
            this.button_continue.Click += new System.EventHandler(this.button_continue_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.ForeColor = System.Drawing.Color.SeaGreen;
            this.button_Clear.Location = new System.Drawing.Point(656, 8);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(80, 24);
            this.button_Clear.TabIndex = 4;
            this.button_Clear.Text = "清除环境(&C)";
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // button_set
            // 
            this.button_set.ForeColor = System.Drawing.Color.SeaGreen;
            this.button_set.Location = new System.Drawing.Point(584, 8);
            this.button_set.Name = "button_set";
            this.button_set.Size = new System.Drawing.Size(64, 24);
            this.button_set.TabIndex = 3;
            this.button_set.Text = "设置(&S)";
            this.button_set.Click += new System.EventHandler(this.button_set_Click);
            // 
            // button_finish_all
            // 
            this.button_finish_all.Enabled = false;
            this.button_finish_all.ForeColor = System.Drawing.Color.Red;
            this.button_finish_all.Location = new System.Drawing.Point(368, 8);
            this.button_finish_all.Name = "button_finish_all";
            this.button_finish_all.Size = new System.Drawing.Size(64, 24);
            this.button_finish_all.TabIndex = 2;
            this.button_finish_all.Text = "终止(&T)";
            this.button_finish_all.Click += new System.EventHandler(this.button_finish_all_Click);
            // 
            // button_finish
            // 
            this.button_finish.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_finish.Enabled = false;
            this.button_finish.ForeColor = System.Drawing.Color.DarkRed;
            this.button_finish.Location = new System.Drawing.Point(296, 8);
            this.button_finish.Name = "button_finish";
            this.button_finish.Size = new System.Drawing.Size(64, 24);
            this.button_finish.TabIndex = 1;
            this.button_finish.Text = "停止(&B)";
            this.button_finish.Click += new System.EventHandler(this.button_finish_Click);
            // 
            // button_start
            // 
            this.button_start.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_start.ForeColor = System.Drawing.Color.Blue;
            this.button_start.Location = new System.Drawing.Point(152, 8);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(64, 24);
            this.button_start.TabIndex = 0;
            this.button_start.Text = "开始(&R)";
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // FileMenu
            // 
            this.FileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_F,
            this.menuItem_S,
            this.menuItem3,
            this.menuItem1});
            // 
            // menuItem_F
            // 
            this.menuItem_F.Index = 0;
            this.menuItem_F.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_OpenFile,
            this.menuItem_SaveFile,
            this.menuItem_refleshTree,
            this.menuItem_Exit});
            this.menuItem_F.Text = "文件(&F)";
            // 
            // menuItem_OpenFile
            // 
            this.menuItem_OpenFile.Index = 0;
            this.menuItem_OpenFile.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuItem_OpenFile.Text = "打开(&O)";
            this.menuItem_OpenFile.Click += new System.EventHandler(this.menuItem_OpenFile_Click);
            // 
            // menuItem_SaveFile
            // 
            this.menuItem_SaveFile.Index = 1;
            this.menuItem_SaveFile.Text = "保存(&S)";
            // 
            // menuItem_refleshTree
            // 
            this.menuItem_refleshTree.Index = 2;
            this.menuItem_refleshTree.Text = "刷新树(&F)";
            // 
            // menuItem_Exit
            // 
            this.menuItem_Exit.Index = 3;
            this.menuItem_Exit.Text = "退出(&X)";
            // 
            // menuItem_S
            // 
            this.menuItem_S.Index = 1;
            this.menuItem_S.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_ShowMessage,
            this.menuItem_Warning,
            this.menuItem_AutoSave,
            this.menuItem_AutoClear,
            this.menuItem_Loop,
            this.menuItem_Set});
            this.menuItem_S.Text = "设置(&S)";
            this.menuItem_S.Popup += new System.EventHandler(this.menu_SetMenu_Click);
            // 
            // menuItem_ShowMessage
            // 
            this.menuItem_ShowMessage.Index = 0;
            this.menuItem_ShowMessage.Text = "显示消息(&P)";
            // 
            // menuItem_Warning
            // 
            this.menuItem_Warning.Index = 1;
            this.menuItem_Warning.Text = "错误报警(&W)";
            // 
            // menuItem_AutoSave
            // 
            this.menuItem_AutoSave.Index = 2;
            this.menuItem_AutoSave.Text = "自动保存(&A)";
            // 
            // menuItem_AutoClear
            // 
            this.menuItem_AutoClear.Index = 3;
            this.menuItem_AutoClear.Text = "自动清空(&C)";
            // 
            // menuItem_Loop
            // 
            this.menuItem_Loop.Index = 4;
            this.menuItem_Loop.Text = "循环测试(&L)";
            // 
            // menuItem_Set
            // 
            this.menuItem_Set.Index = 5;
            this.menuItem_Set.Text = "参数设置(&S)";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.cMenuItemSpSu,
            this.cMenuItemSpFa});
            this.menuItem3.Text = "分离测例(&B)";
            // 
            // cMenuItemSpSu
            // 
            this.cMenuItemSpSu.Index = 0;
            this.cMenuItemSpSu.Text = "分离成功测例";
            this.cMenuItemSpSu.Click += new System.EventHandler(this.cMenuItemSpSu_Click);
            // 
            // cMenuItemSpFa
            // 
            this.cMenuItemSpFa.Index = 1;
            this.cMenuItemSpFa.Text = "分离失败测例";
            this.cMenuItemSpFa.Click += new System.EventHandler(this.cMenuItemSpFa_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 3;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2});
            this.menuItem1.Text = "帮助(&H)";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "关于(&A)";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1114, 469);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.treeView_xml);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.FileMenu;
            this.Name = "MainForm";
            this.Text = "XML自动测试工具";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.Closed += new System.EventHandler(this.MainForm_Closed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
        #endregion

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new MainForm());
        }
        private void AddNode(TreeNode tNode, string sName, bool bDi)
        {
            TreeNode newNode = new TreeNode(sName);
            if (bDi)
            {
                newNode.Tag = XMLDIRECTORY;	//表示该结点代表的是一个目录
                newNode.ImageIndex = 2;
                tNode.Nodes.Add(newNode);
                SearchXmlFile(newNode, ProClass.sValXMLFilePath + newNode.FullPath.Substring(("所有测例").Length));
                //SearchXmlFile(newNode, Environment.CurrentDirectory + newNode.FullPath.Substring(("所有测例").Length));
            }
            else  //是XMLFILE文件
            {
                newNode.ImageIndex = 4;    //初始XMLFILE文件的图象？
                newNode.Tag = XMLFILE;		//表示该结点代表的是一个文件			
                TreeNode TempNode = new TreeNode("");	//生成一个结点，将来用来显示对应XML文件的描述
                TempNode.Tag = NONE;
                TempNode.ImageIndex = 5;  //对应XML文件的描述结点 
                newNode.Nodes.Add(TempNode);
                tNode.Nodes.Add(newNode);  //一并加至tNode中
            }
        }
        // Define the event handlers.
        private void OnCreate(object source, FileSystemEventArgs e)
        {
            bool bDi = false;
            int iRes = CheckIsXmlFile(e.FullPath);
            if (iRes == NONE)
                return;
            bDi = (iRes == XMLDIRECTORY);//表示新建的是文件夹
            char[] separator = new char[1];
            separator[0] = '\\';
            string sPath = e.FullPath.Substring(ProClass.sValXMLFilePath.Length);
            string[] sSpi = sPath.Split(separator);
            TreeNode FirstNode = m_TopNode;
            string sTemp;
            for (int i = 0; i < sSpi.Length - 1; i++)
            {
                sTemp = sSpi[i];
                if (sTemp != "")
                    FirstNode = FindChildNode(FirstNode, sTemp);
            }
            if (FirstNode == null)
                return;
            treeView_xml.Invoke(AddNodeDl, new object[] { FirstNode, sSpi[sSpi.Length - 1], bDi });

        }

        private void DeleteNode(TreeNode tNode)
        {
            tNode.Remove();
        }

        private void OnChange(object source, FileSystemEventArgs e)
        {
            char[] separator = new char[1];
            separator[0] = '\\';
        }
        // Define the event handlers.
        private void OnDelete(object source, FileSystemEventArgs e)
        {
            if (CheckIsXmlFile(e.FullPath) == NONE)
                return;
            char[] separator = new char[1];
            separator[0] = '\\';
            string sPath = e.FullPath.Substring(ProClass.sValXMLFilePath.Length);
            string[] sSpi = sPath.Split(separator);
            TreeNode FirstNode = m_TopNode;
            foreach (string sTemp in sSpi)
            {
                if (sTemp != "")
                    FirstNode = FindChildNode(FirstNode, sTemp);
            }
            if (FirstNode == null || FirstNode.Parent == null)
            {
                return;
            }
            treeView_xml.Invoke(DeleteNodeDl, new object[] { FirstNode });
        }
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (CheckIsXmlFile(e.FullPath) == NONE)
                return;
            char[] separator = new char[1];
            separator[0] = '\\';
            //			string sPath = e.OldFullPath.Substring(Environment.CurrentDirectory.Length);
            string sPath = e.OldFullPath.Substring(ProClass.sValXMLFilePath.Length);
            string[] sSpi = sPath.Split(separator);
            TreeNode FirstNode = m_TopNode;
            foreach (string sTemp in sSpi)
            {
                if (sTemp != "")
                    FirstNode = FindChildNode(FirstNode, sTemp);
            }
            if (FirstNode == null)
            {
                AddToTextBoxFail("在树列表中未找到要重命名的文件：" + e.FullPath, -1);
                return;
            }
            sSpi = e.Name.Split(separator);
            FirstNode.Text = sSpi[sSpi.Length - 1];//.ToUpper()

        }
        private int CheckIsXmlFile(string sPath)
        {
            if (sPath.StartsWith("测试结果"))
                return NONE;
            if (sPath.Length < ProClass.sValXMLFilePath.Length)
            {
                AddToTextBoxFail("修改的文件路径不在当前目录下面？: " + sPath, -1);
                return NONE;
            }
            if (sPath.Length > 256)
            {
                AddToTextBoxFail("新增的文件路径太深: " + sPath, -1);
                return NONE;
            }
            bool bDi = false;
            try
            {
                DirectoryInfo di = new DirectoryInfo(sPath);
                bDi = (di.Attributes == FileAttributes.Directory);
            }
            catch (Exception e)
            {
                AddToTextBoxFail("判断文件是否是文件夹时，发生异常！: " + e.Message, -1);
            }
            if (!bDi)
            {
                if (sPath.EndsWith(".xml") || sPath.EndsWith(".XML") || sPath.EndsWith(".Xml"))
                    return 1;//表示是文件
                else
                    return NONE;
            }
            else
            {
                return XMLDIRECTORY;//表示是文件夹
            }
        }
        private TreeNode FindChildNode(TreeNode FirstNode, string sName)
        {
            if (FirstNode == null || FirstNode.FirstNode == null)
                return null;
            FirstNode = FirstNode.FirstNode;
            while (FirstNode != null)
            {
                if (string.Compare(FirstNode.Text, sName, true) == 0)
                    return FirstNode;
                FirstNode = FirstNode.NextNode;
            }
            return null;
        }
        //创建目录监视对像函数
        public void CreateWatcher()
        {
            // If a directory is not specified, exit program.
            watcher = new FileSystemWatcher();
			try
			{
				watcher.Path = ProClass.sValXMLFilePath;
			}
			catch
			{
				return;
			}
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "";
            watcher.IncludeSubdirectories = true;
            // Add event handlers.
            watcher.Created += new FileSystemEventHandler(OnCreate);
            watcher.Deleted += new FileSystemEventHandler(OnDelete);
            watcher.Changed += new FileSystemEventHandler(OnChange);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// //新建文件夹
        /// </summary>
        private void button_edit_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //如果当前选中的菜单不是空结点，并且节点Tag标识不为空或是它的值不为-1，则执行打开的操作。注：节点tag标识为-1，说明为显示文件描述信息的结点
            if ((treeView_xml.SelectedNode != null && treeView_xml.SelectedNode.Tag != null && (int)treeView_xml.SelectedNode.Tag != NONE) || treeView_xml.SelectedNode.Equals(m_TopNode))
            {
                //跟据当前路径计算节点所代表文件的全路径
                string m_DiPath = "";
                if ((int)(treeView_xml.SelectedNode.Tag) == XMLDIRECTORY || (int)(treeView_xml.SelectedNode.Tag) == XMLFILE)
                    m_DiPath = ProClass.sValXMLFilePath + treeView_xml.SelectedNode.FullPath.Substring(4);
                else
                {
                    char[] separator = new char[2];
                    separator[0] = '(';
                    separator[1] = ')';
                    string m_path1 = treeView_xml.SelectedNode.FullPath;
                    string[] m_pathArry = m_path1.Split(separator);
                    m_path1 = m_pathArry[0];
                    for (int i = 1; i < m_pathArry.Length; i++)
                    {
                        if (m_pathArry[i].StartsWith("\\"))
                        {
                            m_path1 += m_pathArry[i];
                        }
                    }
                    m_DiPath = ProClass.sValXMLFilePath + "\\" + m_path1;
                }

                Process m_Process = new Process();
                m_Process.StartInfo.FileName = "SqlXmlEdit.exe";
                m_Process.StartInfo.Arguments = "\"" + m_DiPath + "\"";
                m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                try
                {
                    m_Process.Start();
                }
                catch (Exception we)//可能会发生找不到文件的异常
                {
                    AddToTextBoxFail("请确认SqlXmlEdit工具，是否在该程序的目录下面\n" + we.Message, -1);
                    return;
                }
            }
            else
            {
                AddToTextBoxFail("请选择正确的结点，先用左键选中，然后再用右键菜单打开", -1);
            }
        }
		private void button_import_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.No == MessageBox.Show("确认要把测试结果导入到数据库吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			AddToTextBoxSuccess_low("正在导入信息到数据库，如果这个过程过慢，说明测试结果中已经累计了很多测试记录，你需要对其进行清空操作", -1, false);
			string ret = ProClass.ImportTestResult();
			if(ret == null)
			{
				AddToTextBoxSuccess_low("已经成功导入到了数据库，表名为 " + ProClass.sValUserId + ".TEST_RESULT", -1, false);
			}
			else
			{
				AddToTextBoxSuccess_low("导入过程中发生错误：" + ret, -1, true);
				return;
			}
			if(DialogResult.No == MessageBox.Show("是否要对比测试记录中的数据？注意：如果选是，那么工具会对数据作处理，会删除掉只执行了一次的记录!", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			try//测试服务器是否还有效
			{
#if DM7
                DmDataReader dr;
                DmConnection cn = new DmConnection();
                DmCommand cm = new DmCommand();
#else
                OleDbDataReader dr;
                OleDbConnection cn = new OleDbConnection();
                OleDbCommand cm = new OleDbCommand();
#endif
                cn.ConnectionString = CreateConnectStr();
				cn.Open();
				
				cm.Connection = cn;
				cm.CommandText = "DELETE FROM TEST_RESULT WHERE NAME IN(SELECT NAME FROM TEST_RESULT GROUP BY NAME HAVING COUNT(*)=1)";
				AddToTextBoxSuccess_low(cm.CommandText, -1, false);
				try
				{
					cm.ExecuteNonQuery();
					cm.CommandText = "SELECT NAME, TEST_TIME, IS_SUCCESS FROM TEST_RESULT WHERE NAME IN(SELECT A.NAME FROM (SELECT NAME, COUNT(*) AS ITEM_NUMS FROM TEST_RESULT GROUP BY NAME) AS A,(SELECT NAME, IS_SUCCESS, COUNT(*) AS ITEM_NUMS FROM TEST_RESULT GROUP BY NAME,IS_SUCCESS) AS B WHERE A.NAME=B.NAME AND A.ITEM_NUMS<>B.ITEM_NUMS) ORDER BY NAME";
					AddToTextBoxSuccess_low(cm.CommandText, -1, false);
					dr = cm.ExecuteReader();
					string name = "";
					int id = -1;
					AddToTextBoxSuccess_low("用例文件		执行时间		是否成功", 3, false);
					while(dr.Read())
					{
						if(name != dr[0].ToString())
						{
							name = dr[0].ToString();
							id ++;
							if(id > 3)
							{
								id = 0;
							}
						}
						AddToTextBoxSuccess_low(dr[0].ToString() + "	" + Convert.ToString(dr[1]) + "	" + dr[2].ToString(), id, false);						
					}
					dr.Close();
				}
#if DM7
                catch (DmException e1)
#else
                catch (OleDbException e1)
#endif
                {
					AddToTextBoxFail(e1.Message, -1);
				}
				cn.Close();
			}
			catch (Exception e1)
			{
				AddToTextBoxSuccess_low(e1.Message, -1, true);
			}
		}
		private void button_clear_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.No == MessageBox.Show("确认要清空测试记录吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			ProClass.RebuileTestResult();
			DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory + "\\" + "测试结果");
			di.Delete(true);
			AddToTextBoxSuccess("已经完成", -1, false);
		}
        /// <summary>
        /// //为设置菜单下面的子菜单前面打勾
        /// </summary>
        private void menu_SetMenu_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //下面为设置菜单下面的子菜单前面打勾
            if (ProClass.GetIsSaveMsg())//自动保存消息菜单
            {
                menuItem_AutoSave.Checked = true;
            }
            else
            {
                menuItem_AutoSave.Checked = false;
            }
            if (ProClass.GetIsShowMsg())//消息显示菜单
            {
                menuItem_ShowMessage.Checked = true;
            }
            else
            {
                menuItem_ShowMessage.Checked = false;
            }
            if (ProClass.GetIsOutVoice())//声音报警菜单
            {
                menuItem_Warning.Checked = true;
            }
            else
            {
                menuItem_Warning.Checked = false;
            }
            if (ProClass.GetIsAutoClearMsg())//自动清空消息菜单
            {
                menuItem_AutoClear.Checked = true;
            }
            else
            {
                menuItem_AutoClear.Checked = false;
            }
            if (ProClass.GetIsLoopRun())//循环测试
            {
                menuItem_Loop.Checked = true;
            }
            else
            {
                menuItem_Loop.Checked = false;
            }
            if (button_start.Enabled)//设置对话框菜单
            {
                menuItem_Set.Enabled = true;
            }
            else
            {
                menuItem_Set.Enabled = false;
            }
        }
        /// <summary>
        /// //自动循环运行测试
        /// </summary>
        private void menuLoop_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (ProClass.GetIsLoopRun())//如果当前允许自动循环测试
            {
                ((MenuItem)sender).Checked = false;
                ProClass.AddProValue(ProClass.ProIsLoop, false.ToString());
            }
            else//跟上面相反
            {
                ProClass.AddProValue(ProClass.ProIsLoop, true.ToString());
                ((MenuItem)sender).Checked = true;
            }
        }
        /// <summary>
        /// //自动清空消息菜单
        /// </summary>
        private void menuAutoClear_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (ProClass.GetIsAutoClearMsg())
            {
                ((MenuItem)sender).Checked = false;
                ProClass.AddProValue(ProClass.ProIsAutoClearMsg, false.ToString());
            }
            else
            {
                ((MenuItem)sender).Checked = true;
                ProClass.AddProValue(ProClass.ProIsAutoClearMsg, true.ToString());
            }
        }
        /// <summary>
        /// //自动保存菜单
        /// </summary>
        private void menuAutoSave_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (ProClass.GetIsSaveMsg())
            {
                ((MenuItem)sender).Checked = false;
                ProClass.AddProValue(ProClass.ProIsSaveMsg, false.ToString());
            }
            else
            {
                ProClass.AddProValue(ProClass.ProIsSaveMsg, true.ToString());
                ((MenuItem)sender).Checked = true;
            }
        }
        /// <summary>
        /// //声音警报菜单
        /// </summary>
        private void menuWaring_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (ProClass.GetIsOutVoice())
            {
                ((MenuItem)sender).Checked = false;
                ProClass.AddProValue(ProClass.ProIsOutVoice, false.ToString());
            }
            else
            {
                ProClass.AddProValue(ProClass.ProIsOutVoice, true.ToString());
                ((MenuItem)sender).Checked = true;
            }
        }
        /// <summary>
        /// //显示消息
        /// </summary>
        private void menuShowMessage_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (ProClass.GetIsShowMsg())
            {
                ((MenuItem)sender).Checked = false;
                ProClass.AddProValue(ProClass.ProIsOutMsg, false.ToString());
            }
            else
            {
                ProClass.AddProValue(ProClass.ProIsOutMsg, true.ToString());
                ((MenuItem)sender).Checked = true;
            }
        }
        /// <summary>
        /// //退出消息菜单事件
        /// </summary>
        private void menuExit_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            this.Close();
            //		Application.Exit();
        }

        /// <summary>
        /// //保存左边树节点信息
        /// </summary>
        private void SaveFile(string m_SaFiName)
        {
            if (m_SaFiName == "")
                return;
            if (m_SaFiName.Length > 256)
            {
                AddToTextBoxFail("文件路径长度超过了256个字符，请输入正确的文件名", -1);
                return;
            }
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(m_SaFiName);
                if (sw == null)
                {
                    AddToTextBoxFail("未能创建指定的文件：" + m_SaFiName, -1);
                    return;
                }
                this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                GetSelectXmlList(false);

                for (int i = 0; i < m_XmlSelectNodeList.Count; i++)
                {
                    sw.WriteLine(((TreeNode)m_XmlSelectNodeList[i]).Text);
                }
                sw.Flush();
                sw.Close();
                this.Cursor = System.Windows.Forms.Cursors.Arrow;
            }
            catch (Exception ee)
            {
                AddToTextBoxFail("保存左边树节点信息时出错！\n" + ee.Message, -1);
                if (sw != null)
                {
                    sw.Close();
                }
                this.Cursor = System.Windows.Forms.Cursors.Arrow;
                return;
            }
        }
        /// <summary>
        /// //保存菜单事件
        /// </summary>
        private void menuSaveFile_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            SaveFileDialog m_SaveFileDialog = new SaveFileDialog();
            m_SaveFileDialog.DefaultExt = "lst";
            m_SaveFileDialog.Filter = "列表文件(*.lst)|*.lst";
            try
            {
                m_SaveFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\测试结果";
            }
            catch (Exception we)//可能会发生找不到文件的异常
            {
                AddToTextBoxFail(we.Message, -1);
            }
            m_SaveFileDialog.RestoreDirectory = true;
            m_SaveFileDialog.Title = "保存选中节点信息到";
            m_SaveFileDialog.ShowDialog();
            SaveFile(m_SaveFileDialog.FileName);
        }
        /// <summary>
        /// //树节点打开菜单操作事件
        /// </summary>
        private void menuOpenNode_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //如果当前选中的菜单不是空结点，并且节点Tag标识不为空或是它的值不为-1，则执行打开的操作。注：节点tag标识为-1，说明为显示文件描述信息的结点
            if ((treeView_xml.SelectedNode != null && treeView_xml.SelectedNode.Tag != null && (int)treeView_xml.SelectedNode.Tag != NONE) || treeView_xml.SelectedNode.Equals(m_TopNode))
            {
                //跟据当前路径计算节点所代表文件的全路径
                string m_DiPath = "";
                if ((int)(treeView_xml.SelectedNode.Tag) == XMLDIRECTORY || (int)(treeView_xml.SelectedNode.Tag) == XMLFILE)
                    m_DiPath = ProClass.sValXMLFilePath + treeView_xml.SelectedNode.FullPath.Substring(4);
                else
                {
                    char[] separator = new char[2];
                    separator[0] = '(';
                    separator[1] = ')';
                    string m_path1 = treeView_xml.SelectedNode.FullPath;
                    string[] m_pathArry = m_path1.Split(separator);
                    m_path1 = m_pathArry[0];
                    for (int i = 1; i < m_pathArry.Length; i++)
                    {
                        if (m_pathArry[i].StartsWith("\\"))
                        {
                            m_path1 += m_pathArry[i];
                        }
                    }
                    m_DiPath = ProClass.sValXMLFilePath + "\\" + m_path1;
                }

                Process m_Process = new Process();
                m_Process.StartInfo.FileName = m_DiPath;
                m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                try
                {
                    m_Process.Start();
                }
                catch (Exception we)//可能会发生找不到文件的异常
                {
                    AddToTextBoxFail(we.Message, -1);
                    return;
                }
            }
            else
            {
                AddToTextBoxFail("请选择正确的结点，先用左键选中，然后再用右键菜单打开", -1);
            }
        }
        /// <summary>
        /// //树节点刷新事件
        /// </summary>
        private void menuFlushNode_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (m_MonuseNode == null)
            {
                FillTreeView();
                return;
            }
            if (treeView_xml.SelectedNode != null)
            {
                if ((int)treeView_xml.SelectedNode.Tag == XMLDIRECTORY)
                {
                    treeView_xml.SelectedNode.Nodes.Clear();
                    // SearchXmlFile(treeView_xml.SelectedNode, Environment.CurrentDirectory + treeView_xml.SelectedNode.FullPath.Substring(("所有测例").Length));
                    SearchXmlFile(treeView_xml.SelectedNode, ProClass.sValXMLFilePath + treeView_xml.SelectedNode.FullPath.Substring(("所有测例").Length));
                }
                else if ((int)treeView_xml.SelectedNode.Tag == RTFDIRECTORY || (int)treeView_xml.SelectedNode.Tag == LOGRTFDIRECTORY)
                {
                    treeView_xml.SelectedNode.Nodes.Clear();

                    char[] separator = new char[2];
                    separator[0] = '(';
                    separator[1] = ')';
                    string m_path1 = treeView_xml.SelectedNode.FullPath;
                    string[] m_pathArry = m_path1.Split(separator);
                    m_path1 = m_pathArry[0];
                    for (int i = 1; i < m_pathArry.Length; i++)
                    {
                        if (m_pathArry[i].StartsWith("\\"))
                        {
                            m_path1 += m_pathArry[i];
                        }
                    }
                    //string m_DiPath = ProClass.sValXMLFilePath + "\\" + m_path1;   00:00点改 是否正确？
                    string m_DiPath = Environment.CurrentDirectory + "\\" + m_path1;
                    SearchLogFile(treeView_xml.SelectedNode, m_DiPath);
                }
                else if ((int)treeView_xml.SelectedNode.Tag == XMLFILE)
                {
                    treeView_xml.SelectedNode.Collapse();
                    TreeNode m_tempNode = treeView_xml.SelectedNode.FirstNode;
                    m_tempNode.Text = "";
                    treeView_xml.SelectedNode.Nodes.Clear();
                    treeView_xml.SelectedNode.Nodes.Add(m_tempNode);
                }
                treeView_xml.SelectedNode.Expand();
            }
        }
        /// <summary>
        /// 菜单清空事件。这里面还包含了两个消息框拷贝字符串的事件
        /// 为了实现方便，把两个框的两个菜单执行集成到这一个函数里了
        /// </summary>
        private void menuClearAndCopyMessage_Click(object sender, System.EventArgs e)
        {
            //菜单清空事件。这里面还包含了两个消息框拷贝字符串的事件
            if (((MenuItem)sender).Text == "清空(&C)")//如果是清空菜单发出来的消息
            {
                SaveMessageToFile();
            }
            else
            {
                if (richTextBox_success.SelectionLength > 0 && richTextBox_success.Focused)//如果是显示所有消息框发出的拷贝消息
                {
                    richTextBox_success.Copy();
                }
                else
                {
                    richTextBox_fail.Copy();
                }
            }
        }
        /// <summary>
        /// 把SQL语句压入缓冲区
        /// </summary>
        public void PushSqlCase(string m_sql)
        {
            Console.Write("sql语句" + m_sql);

            if (ProClass.bValIsSaveSql)
            {
                if (m_SqlCase == null || m_sql == null || m_sql == "")
                    return;
                Monitor.Enter(oSqlCaseQueue);
                m_SqlCase.sql = m_sql;
                m_SqlCase = m_SqlCase.pNext;
                Monitor.Exit(oSqlCaseQueue);
            }
        }

        /// <summary>
        /// 把缓冲区的SQL语句写到相应的XML文件
        /// </summary>
        public void SaveSqlCase()
        {
            if (m_SqlCase == null)
                return;
            Monitor.Enter(oSqlCaseQueue);
            DateTime m_DateTime = DateTime.Now;
            string m_tempData = m_DateTime.ToLongDateString();
            string m_tempTime = m_DateTime.ToLongTimeString() + m_DateTime.Millisecond;
            m_tempTime = m_tempTime.Replace(":", "_");
            string m_path = Environment.CurrentDirectory + "\\测试结果\\" + m_tempData + "\\ErrorLog\\Error_" + m_tempTime + ".RTF";

            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(m_path, true, Encoding.Default);
            }
            catch (Exception e)
            {
                AddToTextBoxSuccess(e.Message, -1, true);
                try
                {
                    DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory + "\\测试结果");
                    m_di.CreateSubdirectory(m_tempData);
                    m_di = new DirectoryInfo(Environment.CurrentDirectory + "\\测试结果\\" + m_tempData);
                    m_di.CreateSubdirectory("ErrorLog");
                    sw = new StreamWriter(m_path, true, Encoding.Default);
                }
                catch (IOException ee)
                {
                    AddToTextBoxSuccess(ee.Message, -1, true);
                    try
                    {
                        DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory + "\\测试结果\\" + m_tempData);
                        m_di.CreateSubdirectory("ErrorLog");
                        sw = new StreamWriter(m_path);
                    }
                    catch (Exception eee)
                    {
                        MessageBox.Show("未能在测试结果下面创建文件夹 ErrorLog \n错误消息为：" + eee.Message);
                        Monitor.Exit(oSqlCaseQueue);
                        return;
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("未能在测试结果下面创建文件夹" + m_tempData + " \n错误消息为：" + ee.Message);
                    Monitor.Exit(oSqlCaseQueue);
                    return;
                }
            }
            try
            {
                SQL_CASE m_tempCase = m_SqlCase;
                do
                {
                    if (m_tempCase.sql != null && m_tempCase.sql != "")
                    {
                        sw.WriteLine(m_tempCase.sql);
                    }
                    m_tempCase = m_tempCase.pNext;
                } while (!m_tempCase.Equals(m_SqlCase));
                sw.Flush();
                sw.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show("保存缓冲区的SQL出错！\n" + ee.Message);
                if (sw != null)
                {
                    sw.Close();
                }
            }
            Monitor.Exit(oSqlCaseQueue);
        }
        /// <summary>
        /// //初始化SQL语句结构链
        /// </summary>
        public void IniSqlCase(int m_num)        
        {
            if (m_SqlCase != null)
                m_SqlCase.pNext = null;
            m_SqlCase = null;
            if (m_num < 2)
                return;
            m_SqlCase = new SQL_CASE();
            SQL_CASE m_temp = m_SqlCase;
            if (m_SqlCase == null)
                return;
            for (int i = 1; i < m_num; i++)
            {
                m_SqlCase.pNext = new SQL_CASE();
                if (m_SqlCase.pNext == null)
                {
                    m_SqlCase = null;
                    AddToTextBoxFail("没有足够的空间来装备执行语句链", -1);
                    return;
                }

                m_SqlCase = (SQL_CASE)m_SqlCase.pNext;
            }
            m_SqlCase.pNext = m_temp;
        }
        /// <summary>
        /// //把当前树结点转到同层的最后一个结点
        /// </summary>
        public TreeNode ReturnLastNode(TreeNode FirstNode)
        {
            while (FirstNode.NextNode != null)
            {
                FirstNode = FirstNode.NextNode;
            }
            return FirstNode;
        }
        /// <summary>
        /// //遍历当前目录下所有的目录，找出LOG文件
        /// </summary>
        public void SearchLogFile(TreeNode FirstNode, string m_diName)
        {
            m_diName += "\\";
            if (m_diName.Length >= 256)
            {
                AddToTextBoxFail("某文件夹路径深度已经超过了256个字符", -1);
                return;
            }
            DirectoryInfo m_di = null;
            DirectoryInfo[] di = null;
            try
            {
                m_di = new DirectoryInfo(m_diName);
                if (m_di == null)
                    return;
                di = m_di.GetDirectories();
                if (di == null)
                    return;
            }
            catch (Exception e)
            {
                AddToTextBoxSuccess(e.Message, -1, true);
				return;
            }
            IComparer myComparerDi = new mySortDirectoryInfoClass();
            Array.Sort(di, myComparerDi);
            foreach (DirectoryInfo diTemp in di)//得到当前节点目录下所有的子目录
            {
                TreeNode newNode = new TreeNode(diTemp.Name);
                if (diTemp.Name == "Fail" || diTemp.Name == "Success")
                {
                    newNode.Tag = RTFDIRECTORY;	//表示该结点代表的是一个保存记录文件的目录
                }
                else
                {
                    newNode.Tag = LOGRTFDIRECTORY;	//Tag变量值为4, 说明该节点是代表一个XML执行报错时，产生的SQL语句记录文件夹
                }
                newNode.ImageIndex = 2;
                FirstNode.Nodes.Add(newNode);
                SearchLogFile(newNode, m_diName + diTemp.Name);
                if ((int)newNode.Tag == RTFDIRECTORY)
                {
                    newNode.Text = newNode.Text + "(" + newNode.Nodes.Count + ")";
                }
            }
            //	FirstNode = ReturnLastNode(FirstNode);//把当前树结点转到同层的最后一个结点，以方便在它后面添加新的节点
            FileInfo[] fi = null;
            fi = m_di.GetFiles("*.RTF");//得到当前节点目录下面所有的RTF文件
            IComparer myComparerFi = new mySortFileInfoClass();
            Array.Sort(fi, myComparerFi);
            foreach (FileInfo fiTemp in fi)//把它们添加到左边树列表中
            {
                TreeNode newNode = new TreeNode(fiTemp.Name);
                newNode.ImageIndex = 13;
                if ((int)(FirstNode.Tag) == LOGRTFDIRECTORY)
                    newNode.Tag = LOGFILE;		//Tag变量值为5, 说明该节点是代表一个XML执行报错时，产生的SQL语句记录文件
                else
                    newNode.Tag = RTFFILE;		//表示该结点代表的是一个记录文件
                FirstNode.Nodes.Add(newNode);
            }
        }
        /// <summary>
        /// //遍历当前目录下所有的目录，找出xml文件
        /// </summary>
        public void SearchXmlFile(TreeNode FirstNode, string m_diName)
        {
            m_diName += "\\";
            if (m_diName.Length >= 256)
            {
                AddToTextBoxFail("某文件夹路径深度已经超过了256个字符", -1);
                return;
            }
            DirectoryInfo m_di = null;
            DirectoryInfo[] di = null;
            try
            {
                m_di = new DirectoryInfo(m_diName);
                if (m_di == null)
                    return;
                di = m_di.GetDirectories();//返回当前目录的子目录
                if (di == null)
                    return;
            }
            catch (Exception e)
            {
                AddToTextBoxSuccess(e.Message, -1, true);
				return;
            }
            IComparer myComparerDi = new mySortDirectoryInfoClass();
            Array.Sort(di, myComparerDi);
            foreach (DirectoryInfo diTemp in di)//得到当前节点目录下所有的子目录
            {
                if ((diTemp.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    if (diTemp.Name != "测试结果")//如果目录的名称为测试结果，那么该目录下的文件夹不会被添加到树中
                    {
                        TreeNode newNode = new TreeNode(diTemp.Name);
                        newNode.Tag = XMLDIRECTORY;	//表示该结点代表的是一个目录
                        newNode.ImageIndex = 2;
                        FirstNode.Nodes.Add(newNode);
                        SearchXmlFile(newNode, m_diName + diTemp.Name);
                    }
                }
            }
            FileInfo[] fi = m_di.GetFiles("*.xml");//得到当前节点目录下面所有的XML文件
            IComparer myComparerFi = new mySortFileInfoClass();
            Array.Sort(fi, myComparerFi);
            foreach (FileInfo fiTemp in fi)//把它们添加到左边树列表中
            {
                TreeNode newNode = new TreeNode(fiTemp.Name);
                newNode.ImageIndex = 4;
                newNode.Tag = XMLFILE;		//表示该结点代表的是一个文件
                FirstNode.Nodes.Add(newNode);
                TreeNode TempNode = new TreeNode("");	//生成一个结点，将来用来显示对应XML文件的描述
                TempNode.Tag = NONE;
                TempNode.ImageIndex = 5;
                newNode.Nodes.Add(TempNode);
            }
        }

        /// <summary>
        /// //把当前目录下的文件名填充到左边树中
        /// </summary>
        public void FillTreeView()
        {
            //////////////////////////////////////////////
            ///节点说明：
            ///每个节点的Tag变量，将用来指定节点的属性
            ///Tag变量值为-1,说明该节点不是有效的节点，是用来显示XML文件测试描述或是其它信息的
            ///Tag变量值为0, 说明该节点是代表一个文件夹
            ///Tag变量值为1, 说明该节点是代表一个XML测试文件
            ///Tag变量值为2, 说明该节点是代表一个RTF记录文件
            ///Tag变量值为3, 说明该节点是代表一个RTF记录文件夹
            ///Tag变量值为4, 说明该节点是代表一个XML执行报错时，产生的SQL语句记录文件夹
            ///Tag变量值为5, 说明该节点是代表一个XML执行报错时，产生的SQL语句记录文件
            //////////////////////////////////////////////	
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            treeView_xml.BeginUpdate();//禁用任何树视图重绘
            treeView_xml.Nodes.Clear();//从集合中删除所有树节点
            treeView_xml.ImageList = m_imageList;//得到节点图标
            TreeNode FirstNode = new TreeNode("所有用例");
            FirstNode.Tag = XMLDIRECTORY;		//不是一个有效的测试节点，说明是目录节点		
            FirstNode.ImageIndex = 0;//获取和设置当前树节点处于未选定状态时所显示图像的图像列表索引值
            treeView_xml.Nodes.Add(FirstNode);
            m_TopNode = FirstNode;//当前树节点的第一个节点

            if (ProClass.sValXMLFilePath == null || ProClass.sValXMLFilePath == "")
                ProClass.sValXMLFilePath = Environment.CurrentDirectory;
            SearchXmlFile(FirstNode, ProClass.sValXMLFilePath);
            TreeNode m_result = new TreeNode("测试结果");
            m_result.Tag = RTFDIRECTORY;		//Tag变量值为3, 说明该节点是代表一个RTF记录文件夹
            m_result.ImageIndex = 2;   //RTF记录文件夹
            treeView_xml.Nodes.Add(m_result);
            SearchLogFile(m_result, Environment.CurrentDirectory + "\\测试结果");
            FirstNode.Expand();
            treeView_xml.EndUpdate();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }

        //树节点的展开事件   根据路径ProClass.sValXMLFilePathDir读取文件
        private void treeView_xml_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            //下面代码用来动态的显示节点图标
            if ((int)e.Node.Tag == NONE)
                e.Node.ImageIndex = 1;
            else if ((int)e.Node.Tag == XMLDIRECTORY || (int)e.Node.Tag == RTFDIRECTORY)
            {
                e.Node.ImageIndex = 3;   //文件夹
            }
            //判断当前的节点的子节点，标识是否为空字符，如果是，那么它就是用来显示XML文件描述的节点
            if (e.Node.FirstNode != null && e.Node.FirstNode.Text == "")
            {
                XmlTextReader xmltext = null;
                XmlDocument doc = null;
                try
                {
                    //生成当前节点相对于当前目录的全路径
                    string m_path = "";
                    //应该根据这个路径来读取文件
                    if (e.Node.FullPath.StartsWith("所有用例"))
                        //m_path = Environment.CurrentDirectory + e.Node.FullPath.Substring(("所有用例").Length);
                        m_path = ProClass.sValXMLFilePath + e.Node.FullPath.Substring(("所有用例").Length);
                    else
                        //m_path = Environment.CurrentDirectory + "\\" + e.Node.FullPath;
                        m_path = ProClass.sValXMLFilePath + "\\" + e.Node.FullPath;
                    xmltext = new XmlTextReader(m_path);
                    doc = new XmlDocument();
                    doc.Load(xmltext);
                }
                catch (Exception err)
                {
                    AddToTextBoxFail(err.Message, -1);
                    return;
                }
                finally
                {
                    if (xmltext != null)
                    {
                        xmltext.Close();
                    }
                }
                XmlNodeList xmlList;
                try
                {
                    if (doc == null)
                        return;
                    xmlList = doc.GetElementsByTagName("CONTENT");//读取文件中描述符部分的信息，并把写到节点中显示出来
                    if (xmlList != null && xmlList.Count >= 1)
                        e.Node.FirstNode.Text = xmlList[0].InnerXml;
                    else
                        e.Node.FirstNode.Text = "无描述信息";
                    for (int i = 1; i < xmlList.Count; i++)
                    {
                        TreeNode m_TempNode = new TreeNode();
                        m_TempNode.Text = xmlList[i].InnerXml;
                        m_TempNode.Tag = NONE;
                        m_TempNode.ImageIndex = 5;
                        e.Node.Nodes.Add(m_TempNode);
                    }
                }
                catch (Exception xmlE)
                {
                    AddToTextBoxFail(xmlE.Message, -1);
                    e.Node.FirstNode.Text = "无描述信息";
                }
            }
        }

        private void CreateIniDatabase()
        {
            try//测试服务器是否还有效
            {
#if DM7
                DmConnection cn = new DmConnection();
                DmCommand cm = new DmCommand();
#else
                OleDbConnection cn = new OleDbConnection();
                OleDbCommand cm = new OleDbCommand();
#endif
                cn.ConnectionString = CreateConnectStr();
                cn.Open();                
                cm.Connection = cn;
                cm.CommandText = "SET SCHEMA " + ProClass.sValDatabase;
                Console.Write("默认数据库" + ProClass.sValDatabase);
                try
                {
                    cm.ExecuteNonQuery();
                }
#if DM7
                catch (DmException e)
#else
                catch (OleDbException e)
#endif
                {
                    AddToTextBoxFail(e.Message, -1);
                    AddToTextBoxSuccess("正在创建初始化运行库，请稍后......", -1, false);
                //    cm.CommandText = "CREATE SCHEMA " + ProClass.sValDatabase + " DATAFILE '" + ProClass.sValDatabase + ".DBF' SIZE 256";
                       cm.CommandText = "CREATE SCHEMA " + ProClass.sValDatabase ;
                    try
                    {
                        cm.ExecuteNonQuery();
                        AddToTextBoxSuccess("初始化运行库成功创建", -1, false);
                    }
#if DM7
                    catch (DmException mes)
#else
                    catch (OleDbException mes)
#endif
                    {
                        AddToTextBoxFail("初始化库创建失败：" + mes.Message, -1);
                    }
                }
                cn.Close();
            }
            catch (Exception e)
            {
                AddToTextBoxFail(e.Message, -1);
            }
        }
        //开始按钮事件
        private void button_start_Click(object sender, System.EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            if (bIni == false)//第一次运行
            {
                CreateIniDatabase();
                AddToTextBoxSuccess("第一次运行...", -1 ,false);
                bIni = true;
            }
            if (ProClass.GetIsAutoClearMsg())//是否在每次开始测试前，清空消息区
            {
                SaveMessageToFile();
            }
            m_IsLogText = false;           //指示当前消息框中显示的是否是历史消息
            //设置其它按钮的状态
            button_start.Enabled = false;
            button_continue.Enabled = false;
            button_finish.Enabled = true;
            button_finish_all.Enabled = false;
            button_set.Enabled = false;
            button_Clear.Enabled = button_set.Enabled;
            m_stopTest = false;//标识开始测试标识符   指示是否要停下测试线程
            //得到树中被选定的节点代表的文件列表
            GetSelectXmlList(true);
          //  m_XmlFileList 		    该数组会存放当前用户选中树中的所有文件全路径
          //  m_XmlSelectNodeList 	    该数组存放选中的树节点
            CreateThread();
            richTextBox_success.Focus();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }
        //继续按钮事件   
        private void button_continue_Click(object sender, System.EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            if (bIni == false)
            {
                CreateIniDatabase();
                bIni = true;
            }
            if (ProClass.GetIsAutoClearMsg())//是否在每次开始测试前，清空消息区
            {
                SaveMessageToFile();
            }
            m_IsLogText = false;
            //设置其它按钮的状态
            button_continue.Enabled = false;
            button_start.Enabled = false;
            button_finish.Enabled = true;
            button_finish_all.Enabled = false;
            button_set.Enabled = false;
            button_Clear.Enabled = button_set.Enabled;
            m_stopTest = false;//标识开始测试标识符
            //得到树中被选定的节点代表的文件列表
            GetSelectXmlList(true);

            //对上面得到的文件列表作处理，删除掉已经执行过的文件。从第一个没有执行过的文件中开始执行。
            //利用ImageIndex ==4判断是否执行过。一开始的ImageIndex ==4   line617
            //或ImageIndex ==8执行成功  ImageIndex ==9 执行失败   ImageIndex ==12异常中断，测试停止
            //创建线程执行列表中的文件  .ImageIndex = 8
            //for(int i=0;i<m_XmlSelectNodeList.Count;i++)
            //{
            while (true)
            {
                if (    //用这个判断是否执行过，看下行不行  if里为执行过
                    ((TreeNode)m_XmlSelectNodeList[0]).ImageIndex == 8 ||
                    ((TreeNode)m_XmlSelectNodeList[0]).ImageIndex == 9 ||
                    ((TreeNode)m_XmlSelectNodeList[0]).ImageIndex == 12
                    )
                    m_XmlSelectNodeList.RemoveAt(0);   //删除掉该结点  也可考虑使用RemoveRrange
                else break;     //找到未执行的第一个，跳出循环 
            }
            CreateThread();
            richTextBox_success.Focus();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }

        //停止按钮事件
        private void button_finish_Click(object sender, System.EventArgs e)
        {
            //改变运行状态。标识将要停止测试
            m_stopTest = true;
            button_continue.Enabled = true;
            button_finish.Enabled = false;
            button_finish_all.Enabled = true;
            richTextBox_success.Focus();
        }

        //终止按钮事件
        private void button_finish_all_Click(object sender, System.EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            richTextBox_success.Focus();
            Process[] myProcesses = Process.GetProcessesByName("DisposeTrans");
            for (int i = 0; i < myProcesses.Length; i++)
            {
                try
                {
                    myProcesses[i].Kill();
                }
                catch (Exception err)
                {
                    AddToTextBoxFail(err.Message, -1);
                }

            }
            //终止各个线程
            //这里存在一个很大的问题。就是在一个线程因为执行某条语句被阻死以后，杀掉相对应的那个线程，并不能释放线程中那条语句的执行。因为，ADO执行中，是自己又新建的线程来执行语句的。
            button_finish_all.Enabled = false;
            if (m_thread_struct != null)
            {
                //下面是检察线程数组中各个线程的运行状态。
                for (int i = 0; i < ProClass.GetThreadNum(); i++)
                {
                    if (m_thread_struct[i].m_xmlFileName == null || m_thread_struct[i].m_xmlFileName == "" || m_thread_struct[i].m_thread.ThreadState == System.Threading.ThreadState.Stopped || m_thread_struct[i].m_thread.ThreadState == System.Threading.ThreadState.Aborted)
                        continue;//如果线程已经自动动停止则不作处理
                    else//如果线程还在运行
                    {
                        m_thread_struct[i].m_thread.Abort();		//试图终止一个线程的运行
                        m_thread_struct[i].m_thread.Join(10000);		//等待它终止
                        //如果不行，调用API来杀它.其实这样做没有多大用处，寒啊：（
                        if (m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Stopped && m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Aborted)
                        {
                            AddToTextBoxFail("线程 " + i + " 被强行终止，该线程运行的测试已经失败，请查看消息记录", -1);
                            m_thread_struct[i].m_TestThread.DisConnect();
                            try
                            {
                                int m_index = m_thread_struct[i].m_xmlIndex;
                                if (m_index != -1)
                                    ((TreeNode)m_XmlSelectNodeList[m_index]).ImageIndex = 12;
                            }
                            catch (Exception me)
                            {
                                AddToTextBoxFail(me.Message, -1);
                            }
                            IntPtr m_IntPtr = OpenThread(1, false, m_thread_struct[i].m_TestThread.GetThreadID());
                            TerminateThread(m_IntPtr, 0);
                        }
                    }
                }
            }
            //下面是改变按钮的状态
            button_finish.Enabled = false;
            button_start.Enabled = true;
            button_set.Enabled = true;
            button_Clear.Enabled = button_set.Enabled;
            LineOutFailXmlNode();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }

        private void button_set_Click(object sender, System.EventArgs e)
        {
			string  sValOldXMLFilePath = ProClass.sValXMLFilePath;
            Form_Set m_SetFom = new Form_Set();
            m_SetFom.ShowDialog();

			ProClass.sValOldServerIP=ProClass.sValServer;
            //在设置对话框结束后，重新读取文件中的设置信息
            if (m_SetFom.DialogResult == DialogResult.OK)
            {
				if(ProClass.sValXMLFilePath == null || ProClass.sValXMLFilePath == "")
				{
					ProClass.sValXMLFilePath = Environment.CurrentDirectory;
				}
				if(sValOldXMLFilePath != ProClass.sValXMLFilePath)
				{
					FillTreeView();
				}
                //8.27 修改  如果设置之后改变了服务器端的IP，则需要关闭当前连接，重新建立新的连接
				if(ProClass.sValOldServerIP!=ProClass.sValServer)
				{
					ProClass.CommServer.CloseStream();
					string con=ProClass.CommServer.ConnectSev(); 
					//if(con=="connect success");
				}

                m_thread_struct = null;	//把线程数组指针清空，因为设置操作，可能会改变线程的个数，所以要重新生成新的数组
            }
        }
        /// <summary>
        /// //把消息框中的内容保存到文件
        /// </summary>
        public void SaveMessageToFile()
        {
            m_message_num = 0;
            if (!ProClass.GetIsSaveMsg() || m_IsLogText)//如果不允许保存为文件
            {
                richTextBox_fail.Clear();
                richTextBox_success.Clear();
                return;
            }
            string m_path = Environment.CurrentDirectory + "\\" + "测试结果";
            DirectoryInfo m_di = new DirectoryInfo(m_path);

            //下面是根据当前时间，生成相应的文件名
            DateTime m_DateTime = DateTime.Now;

            string m_temp = m_DateTime.ToLongDateString();
            try
            {
                m_di.CreateSubdirectory(m_temp);
                m_path += "\\" + m_temp;
                m_di = new DirectoryInfo(m_path);
                m_di.CreateSubdirectory("Success");
                m_di.CreateSubdirectory("Fail");
            }
            catch (IOException e)
            {
                m_path += "\\" + m_temp;
                AddToTextBoxSuccess(e.Message, -1, true);
            }
            catch (Exception e)
            {
                AddToTextBoxSuccess(e.Message, -1, true);
                MessageBox.Show("未能在测试结果下面创建文件夹" + m_temp);
                return;
            }

            m_temp = m_DateTime.ToLongTimeString();
            m_temp = m_temp.Replace(":", "_");
            try
            {
                if (richTextBox_success.TextLength > 0)
                    richTextBox_success.SaveFile(m_path + "\\Success\\" + m_temp + "_Success.RTF");
                if (richTextBox_fail.TextLength > 0)
                    richTextBox_fail.SaveFile(m_path + "\\Fail\\" + m_temp + "_Fail.RTF");
            }
            catch (Exception e)
            {
                string m_txt = e.Message;
            }

            richTextBox_fail.Clear();
            richTextBox_success.Clear();
        }
        /// <summary>
        /// //智能的检察一下每个线程发消息之间的间隔时间为多少，以便判断它们是否还在正常运行
        /// 按停止按钮，过一段时间以后，开始按钮状态没有被改变，那么按终止按钮来找出可能已经死的线程
        /// </summary>
        public void CheckMessageTime(Object state)
        {
            if (m_stopTest || m_thread_struct == null)
                return;
            m_CheckMessageTimer.Change(2000000000, 2000000000);
            //检查线程数组中，各个线程上次消息跟当前时间的间隔
            DateTime m_now = DateTime.Now;
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_xmlFileName == null || m_thread_struct[i].m_xmlFileName == "" || m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Running)
                    continue;//如果该线程已经停止运行就跳过
                TimeSpan m_TimeSpan = m_now.Subtract(m_thread_struct[i].m_DateTime);
                if ((m_TimeSpan.Seconds + m_TimeSpan.Minutes * 60) > 300)
                {
                    AddToTextBoxFail("线程 " + i + " 已经有五分钟没有产生过消息了，请确认一下它是否还在运行", -1);
                }
            }
            m_CheckMessageTimer.Change(ProClass.GetMsgCheckTime() * 1000, ProClass.GetMsgCheckTime() * 1000);
        }

        /// <summary>
        /// //往显示成功消息的文件框中添加一行消息
        /// </summary>
        public void AddToTextBoxSuccess(string m_txt, int m_id, bool m_fail)
        {
            Debug.Assert((m_id < ProClass.GetThreadNum()) && (m_id > -2), "给定的消息所属线程ID，超出了规定的值", "MainForm.AddToTextBoxSuccess");
        
            if (!ProClass.bValIsOutMsg)//bValIsOutMsg是否输出执行消息
                return;
            if (m_id != -1)//-1被定义为界面线程，如果是界面线程消息，那么，检察它这个消息与上个消息之间的间隔时间
            {
                DateTime m_now = DateTime.Now;
                m_thread_struct[m_id].m_DateTime = m_now;
                m_txt = "(" + m_id + ")" + m_txt;
            }
            AddToTextBoxSuccess_low(m_txt, m_id, m_fail);
        }
		private void AddToTextBoxSuccess_low(string m_txt, int m_id, bool m_fail)
		{
			if (ProClass.bValIsMsgSpan)
            {
                m_txt += "\n\n";
            }
            else
            {
                m_txt += "\n";
            }
            Monitor.Enter(this);//进入锁
            if (m_message_num++ > ProClass.iValMsgNum)//保存消息到文件。
            {
                SaveMessageToFile();
            }
            Monitor.Exit(this);//释放锁 

			Monitor.Enter(richTextBox_success);//进入锁
            if (m_fail && richTextBox_success.SelectionColor != Color.Red)
                richTextBox_success.SelectionColor = Color.Red;
            else if (m_id == -1)
            {
                richTextBox_success.SelectionColor = Color.Black;
            }
            else if (m_id == 0)
            {
                richTextBox_success.SelectionColor = Color.Blue;
            }
            else if (m_id == 1)
            {
                richTextBox_success.SelectionColor = Color.DeepSkyBlue;
            }
            else if (m_id == 2)
            {
                richTextBox_success.SelectionColor = Color.SteelBlue;
            }
            else if (m_id == 3)
            {
                richTextBox_success.SelectionColor = Color.RoyalBlue;
            }
            else if (m_id == 4)
            {
                richTextBox_success.SelectionColor = Color.MidnightBlue;
            }
            else if (m_id == 5)
            {
                richTextBox_success.SelectionColor = Color.DodgerBlue;
            }
            else if (m_id == 6)
            {
                richTextBox_success.SelectionColor = Color.SlateBlue;
            }
            else if (m_id == 7)
            {
                richTextBox_success.SelectionColor = Color.BlueViolet;
            }
            else if (m_id == 8)
            {
                richTextBox_success.SelectionColor = Color.Indigo;
            }
            else if (m_id == 9)
            {
                richTextBox_success.SelectionColor = Color.Violet;
            }
            else if (m_id == 10)
            {
                richTextBox_success.SelectionColor = Color.OliveDrab;
            }
            else if (m_id == 11)
            {
                richTextBox_success.SelectionColor = Color.DarkOliveGreen;
            }
            else if (m_id == 12)
            {
                richTextBox_success.SelectionColor = Color.DarkCyan;
            }
            else if (m_id == 13)
            {
                richTextBox_success.SelectionColor = Color.DarkSeaGreen;
            }
            else if (m_id == 14)
            {
                richTextBox_success.SelectionColor = Color.ForestGreen;
            }
            else if (m_id == 15)
            {
                richTextBox_success.SelectionColor = Color.LimeGreen;
            }
            else if (m_id == 16)
            {
                richTextBox_success.SelectionColor = Color.Green;
            }
            else if (m_id == 17)
            {
                richTextBox_success.SelectionColor = Color.LightSeaGreen;
            }
            else if (m_id == 18)
            {
                richTextBox_success.SelectionColor = Color.DarkSlateGray;
            }
            else if (m_id == 19)
            {
                richTextBox_success.SelectionColor = Color.MediumBlue;
            }
            else
                richTextBox_success.SelectionColor = Color.Black;
            richTextBox_success.AppendText(m_txt);
            richTextBox_success.ScrollToCaret();
            Monitor.Exit(richTextBox_success);//释放锁 
        }

        /// <summary>
        /// //往显示失败消息的文件框中添加一行消息
        /// </summary>
        public void AddToTextBoxFail(string m_txt, int m_id, bool iNoVoice)
        {
            if (ProClass.bValIsMsgSpan)
            {
                m_txt += "\n\n";
            }
            else
            {
                m_txt += "\n";
            }
            if (m_id != -1)
                m_txt = "(" + m_id + ")" + m_txt;
            Monitor.Enter(richTextBox_fail);
            if (ProClass.bValIsOutVoice && iNoVoice)//bValIsOutVoice是否报错发声提示
            {
                Voice m_Voice = new Voice();
                m_Voice.CreateVoice();
            }
            richTextBox_fail.AppendText(m_txt);//进入锁
            //	richTextBox_fail.Focus();
            richTextBox_fail.ScrollToCaret();
            Monitor.Exit(richTextBox_fail);//释放锁 
        }
        /// <summary>
        /// //往显示失败消息的文件框中添加一行消息
        /// </summary>
        public void AddToTextBoxFail(string m_txt, int m_id)
        {
            if (ProClass.bValIsMsgSpan)
            {
                m_txt += "\n\n";
            }
            else
            {
                m_txt += "\n";
            }
            if (m_id != -1)
                m_txt = "(" + m_id + ")" + m_txt;
            CheckForIllegalCrossThreadCalls = false;
            Monitor.Enter(richTextBox_fail);
            
            richTextBox_fail.AppendText(m_txt);//进入锁
            //	richTextBox_fail.Focus();
            richTextBox_fail.ScrollToCaret();
            Monitor.Exit(richTextBox_fail);//释放锁 
        }

        public void UpdateSelectNodeList(object o, System.EventArgs e)
        {
            int index;
            int imageIndex;
            index = (int)o;
            index = index >> 16;
            imageIndex = (int)o;
            imageIndex &= 0xFFFF;
            if (imageIndex == 0xFFFF)
            {
                if (((TreeNode)m_XmlSelectNodeList[index]).IsSelected)
                    ((TreeNode)m_XmlSelectNodeList[index]).SelectedImageIndex = ((TreeNode)m_XmlSelectNodeList[index]).ImageIndex;
            }
            else
                ((TreeNode)m_XmlSelectNodeList[index]).ImageIndex = imageIndex;
        }
        /// <summary>
        /// //检察当前选取的xml文件是否已经在运行
        /// </summary>
        public bool CheckNotExistXml(int m_threadIndex, int m_index)
        {
            if (m_threadIndex == -1)
                return true;
            Debug.Assert(m_index < m_XmlFileList.Count && m_XmlFileList.Count >= 0, "指定的索引，在节点对像数组中越界了", "MainForm.CheckNotExistXml 函数");
            //在当前数组中检察是否已经存在指定的文件
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_xmlIndex == m_index)
                {
                    return false;
                }
            }

            m_thread_struct[m_threadIndex].m_xmlIndex = m_index;
#if DM6
          ((TreeNode)m_XmlSelectNodeList[m_index]).ImageIndex = 10;
#else
            m_index = (m_index << 16) + 10;
            treeView_xml.BeginInvoke(new System.EventHandler(UpdateSelectNodeList), m_index);
#endif
            return true;
        }

        /// <summary>
        /// //从左边树列表中得到一个被选择的Xml文件列表
        /// FirstNode 要遍历的树节点
        /// m_diName  节点代表的全路径
        /// m_checked 节点是否被选中
        /// </summary>
        public void SearchCheckNode(TreeNode FirstNode, string m_diName, bool m_checked)
        {
            //该函数为遍历函数
            m_diName += "\\";
            while (FirstNode != null)
            {
                if (FirstNode.Checked == true)
                {
                    m_HaveNodeCheck = true;//标识运行时，是否有节点前面被打上勾。如果没有，那么就会拿当前选中（不是打勾选中）的节点来运行
                }
                //如果节点是文件夹，那么遍历该节点
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == 0 || (int)FirstNode.Tag == 3 || (int)FirstNode.Tag == LOGRTFDIRECTORY)))
                {
                    if (FirstNode.Text != "不被加载的文件")
                    {
                        SearchCheckNode(FirstNode.FirstNode, m_diName + FirstNode.Text, m_checked | FirstNode.Checked);
                    }
                }
                else if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLFILE || (int)FirstNode.Tag == LOGFILE)) && (FirstNode.Checked || m_checked))
                {
                    //如果是该节点是文件节点，则计算出它的全路径，加到运行列表中
                    string m_NodeXmlName = m_diName + FirstNode.Text;
                    m_XmlFileList.Add(m_NodeXmlName);
                    m_XmlSelectNodeList.Add(FirstNode);//把选中的节点，加到数组中
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        /// <summary>
        /// //遍历所有节点，计算得出所选节点在当前目录下的相对路径
        /// 参数m_TreeNode为要搜索的下一个结点
        /// </summary>
        public string GetNodeDirectory(TreeNode m_TreeNode, string m_di)
        {
            if (m_TreeNode == null)
                return "";
            while (m_TreeNode != null)
            {
                if (m_TreeNode.Equals(treeView_xml.SelectedNode))
                    return m_di;
                if ((m_TreeNode.Tag != null && (int)m_TreeNode.Tag == 0))
                {
                    string m_temp = GetNodeDirectory(m_TreeNode.FirstNode, m_di + m_TreeNode.Text);
                    if (m_temp != "")
                        return m_temp;
                }
                m_TreeNode = m_TreeNode.NextNode;
            }
            return "";
        }
        /// <summary>
        /// //从左边树列表中得到一个被选择的Xml文件列表
        /// </summary>
        public void GetSelectXmlList(bool m_FindSelectNode)
        {
            bOwnCheck = true;
            m_HaveNodeCheck = false;
            TreeNode FirstNode = m_TopNode;

            m_XmlFileList.Clear();
            m_XmlSelectNodeList.Clear();
            m_TreeNodeIndex = 0;
            m_runLog = false;

            SearchCheckNode(FirstNode.FirstNode, ProClass.sValXMLFilePath, FirstNode.Checked);
            //如果当前运行列表中，文件个数为0,那么，选用当前激活的节点运行
            if (m_FindSelectNode && m_XmlFileList.Count == 0 && treeView_xml.SelectedNode != null && !m_HaveNodeCheck && treeView_xml.SelectedNode.Checked != true)
            {
                treeView_xml.SelectedNode.Checked = true;
                SearchCheckNode(FirstNode.FirstNode, ProClass.sValXMLFilePath, FirstNode.Checked);
                treeView_xml.SelectedNode.Checked = false;
            }
            if (FirstNode.NextNode != null && m_XmlFileList.Count == 0)//如果上面的操作还未发现被选中的XML文件，那么就到LOG记录文件夹中查
            {
                m_runLog = true;   //NextNode下一个同辈树结点
                SearchCheckNode(FirstNode.NextNode, ProClass.sValXMLFilePath, false);
                //如果当前运行列表中，文件个数为0,那么，选用当前激活的节点运行
                if (m_FindSelectNode && m_XmlFileList.Count == 0 && treeView_xml.SelectedNode != null && !m_HaveNodeCheck)
                {
                    treeView_xml.SelectedNode.Checked = true;
                    SearchCheckNode(FirstNode.NextNode, ProClass.sValXMLFilePath, false);
                    treeView_xml.SelectedNode.Checked = false;
                }
            }
            bOwnCheck = false;
        }
        /// <summary>
        /// //根据索引，在列表中得到一个Xml文件用来测试
        /// </summary>
        public string GetXmlFileName(int m_index)
        {
            Debug.Assert(m_index < m_XmlFileList.Count && m_index >= 0, "指定的索引，在文件组中越界了", "MainForm.GetXmlFileName 函数");
            if (m_index >= m_XmlFileList.Count || m_index < 0)
                return "";
            return (string)m_XmlFileList[m_index];
        }
        /// <summary>
        /// //从左边树列表中得到一个Xml文件在列表中的索引用来测试
        /// </summary>
        public int GetXmlFileIndex(int m_TheadIndex)
        {

            int m_NIndex = -1;
            if (m_XmlFileList.Count < 1)
            {
                AddToTextBoxFail("运行队列中XML文件数为0，请选择要运行的XML文件", -1);
                m_stopTest = true;
                return m_NIndex;
            }
            Monitor.Enter(oGetXmlQueue);

            //如果是随机选取用例。
            if (ProClass.bValIsRandRun && ProClass.GetIsLoopRun())
            {
                int m_loop = 10;
                Random m_Random = new Random();
                while ((m_loop--) > 0)
                {
                    m_NIndex = m_Random.Next(0, m_XmlFileList.Count);//得到一个在文件个数之内的随机数
                    Debug.Assert(m_NIndex < m_XmlFileList.Count && m_NIndex >= 0, "指定的索引，在文件组中越界了", "MainForm.GetXmlFileIndex 函数");
                    if (m_NIndex >= m_XmlFileList.Count)
                    {
                        m_NIndex = -1;
                    }
                    else
                    {
                        if (CheckNotExistXml(m_TheadIndex, m_NIndex))//检察该文件是否已经在运行
                        {
                            break;
                        }
                        else
                        {
                            m_NIndex = -1;
                        }
                    }
                }
            }
            else//如果是顺序选取用例
            {
                {
                    m_NIndex = m_TreeNodeIndex;
                    if (!CheckNotExistXml(m_TheadIndex, m_NIndex))//检察该文件是否已经在运行
                    {
                        m_NIndex = -1;
                    }
                    else
                    {
                        m_TreeNodeIndex++;//当前运行文件在列表中的索引自加一
                        if (m_TreeNodeIndex >= m_XmlFileList.Count)
                        {
                            m_TreeNodeIndex = 0;//重置为0
                            if (!ProClass.GetIsLoopRun())//如果不允许循环测试
                                m_stopTest = true;//那么停止当前的测试
                        }
                    }
                }
            }
            Monitor.Exit(oGetXmlQueue);
            return m_NIndex;
        }

        /// <summary>
        /// //创建测试工作线程
        /// </summary>
        private void CreateThread()
        {
            if (m_thread_struct == null)//根据指定的线程个数，生成相应大小的数组
                m_thread_struct = new THREAD_STRUCT[ProClass.GetThreadNum()];
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                m_thread_struct[i].m_xmlIndex = -1;
            }
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_TestThread == null)
                {
                    m_thread_struct[i].m_TestThread = new TestThread();	//创建一个测试线程对像
                    m_thread_struct[i].m_TestThread.SetForm1(this);		//把当前窗口对像的指针给定进去
                    m_thread_struct[i].m_TestThread.SetThreadIndex(i);	//设置它在线组数组中的ID				
                }
                m_thread_struct[i].m_DateTime = DateTime.Now;
                m_thread_struct[i].m_thread = new Thread(new ThreadStart(m_thread_struct[i].m_TestThread.Run));//创建一个线程委托来运行测试函数
                m_thread_struct[i].m_thread.Priority = ThreadPriority.BelowNormal;
                m_thread_struct[i].m_thread.Start();	//开始			
            }
        }
#if DM6
        /// <summary>
        /// //用来设置三个按钮(开始，停止，终止)的状态
        /// </summary>
        public void SetButtonState(bool m_start, bool m_set, bool m_fnish, bool m_fnish_all)
        {
            button_set.Enabled = m_set;
            button_Clear.Enabled = button_set.Enabled;
            button_finish.Enabled = m_fnish;
            button_finish_all.Enabled = m_fnish_all;
            button_start.Enabled = m_start;
            if (button_start.Enabled)
                button_start.Focus();
        }
#else
        public void UpdateBottonState(object o, System.EventArgs e)
        {
            int state;
            int temp;
            
            state = (int)o;
            temp = state & 0x0f;
            if (temp != 0)
            {
                if (temp == 1)
                    button_set.Enabled = true;
                else
                    button_set.Enabled = false;
                button_Clear.Enabled = button_set.Enabled;
            }
            temp = state & 0xf0;
            if (temp != 0)
            {
                temp = temp >> 4;
                if (temp == 1)
                    button_finish.Enabled = true;
                else
                    button_finish.Enabled = false;
            }
            temp = state & 0xf00;
            if (temp != 0)
            {
                temp = temp >> 8;
                if (temp == 1)
                    button_finish_all.Enabled = true;
                else
                    button_finish_all.Enabled = false;
            }
            temp = state & 0xf000;
            if (temp != 0)
            {
                temp = temp >> 12;
                if (temp == 1)
                    button_start.Enabled = true;
                else
                    button_start.Enabled = false;
                if (button_start.Enabled)
                    button_start.Focus();
            }
            LineOutFailXmlNode();
        }
        /// <summary>
        /// //用来设置三个按钮(开始，停止，终止)的状态
        /// </summary>
        public void SetButtonState(bool m_start, bool m_set, bool m_fnish, bool m_fnish_all)
        {
//             button_set.Enabled = m_set;
//             button_Clear.Enabled = button_set.Enabled;
//             button_finish.Enabled = m_fnish;
//             button_finish_all.Enabled = m_fnish_all;
//             button_start.Enabled = m_start;
//             if (button_start.Enabled)
//                 button_start.Focus();

            int state = 0;
            if (m_set)
                state |= 1;
            else
                state |= 2;

            if (m_fnish)
                state |= 1<<4;
            else
                state |= 2<<4;

            if (m_fnish_all)
                state |= 1 << 8;
            else
                state |= 2 << 8;

            if (m_start)
                state |= 1 << 12;
            else
                state |= 2 << 12;

            button_start.BeginInvoke(new System.EventHandler(UpdateBottonState), state);
        }
#endif
        private void button_Clear_Click(object sender, System.EventArgs e)
        {
            GetSelectXmlList(true);
            TestThread m_TestThread = new TestThread();
            m_TestThread.SetForm1(this);
            m_TestThread.RunClearEnvironment();
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_stopTest = true;
            ProClass.cDServer.ExitCmd();
            ProClass.ClearSaveProValue();
            ProClass.CloseProClass();
            SaveFile(Environment.CurrentDirectory + "\\测试结果\\Default.lst");
            if (m_thread_struct == null)
            {
                return;
            }
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_xmlFileName == null || m_thread_struct[i].m_xmlFileName == "" || m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Running)
                    continue;//如果线程已经自动动停止则不作处理
                else//如果线程还在运行
                {
                    AddToTextBoxSuccess("等待1秒，让线程 " + i + " 自动停止", -1, false);
                    Thread.Sleep(1000);//等1秒
                    try
                    {
                        m_thread_struct[i].m_thread.Abort();
                        m_thread_struct[i].m_thread.Join(100);
                        if (m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Stopped && m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Aborted)
                        {
                            AddToTextBoxFail("线程 " + i + " 被强行终止，该线程运行的测试已经失败，请查看消息记录", -1);
                            IntPtr m_IntPtr = OpenThread(1, false, m_thread_struct[i].m_TestThread.GetThreadID());
                            bool s = TerminateThread(m_IntPtr, 0);
                            s = false;
                        }
                    }
                    catch (Exception em)
                    {
                        string m_txt = em.Message;
                    }
                }
            }
            SaveMessageToFile();
            return;
        }

        private void treeView_xml_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if ((int)e.Node.Tag == NONE)
                e.Node.ImageIndex = 0;
            else if ((int)e.Node.Tag == XMLDIRECTORY || (int)e.Node.Tag == RTFDIRECTORY)
            {
                e.Node.ImageIndex = 2;
            }
        }

        private void treeView_xml_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m_MonuseNode = treeView_xml.GetNodeAt(e.X, e.Y);		//得到鼠标位置上的结点对像
            if (m_MonuseNode != null)
                treeView_xml.SelectedNode = m_MonuseNode;					//设置为选中对像
            else
            {
                treeView_xml.ContextMenu.MenuItems.Clear();
                treeView_xml.ContextMenu.MenuItems.Add(m_treeReflesh);	//如果在空白的地方点击的，那么给它指定刷新菜单
                return;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                treeView_xml.ContextMenu.MenuItems.Clear();
                if (treeView_xml.SelectedNode != null)
                {
                    if (treeView_xml.SelectedNode.Tag != null && (int)treeView_xml.SelectedNode.Tag != NONE)
                    {
                        m_treeRun.Enabled = button_start.Enabled;
                        treeView_xml.ContextMenu.MenuItems.Add(m_treeOpen);
                        treeView_xml.ContextMenu.MenuItems.Add(m_treeRun);
                        treeView_xml.ContextMenu.MenuItems.Add(m_treeReflesh);
                        //					treeView_xml.ContextMenu.MenuItems.Add(m_treeNewDi);
                    }
                    if (treeView_xml.SelectedNode.Tag == null || (int)treeView_xml.SelectedNode.Tag == RTFFILE || (int)treeView_xml.SelectedNode.Tag == RTFDIRECTORY)
                    {
                        treeView_xml.ContextMenu.MenuItems.Remove(m_treeRun);
						treeView_xml.ContextMenu.MenuItems.Add(m_treeImportTestResult);
						treeView_xml.ContextMenu.MenuItems.Add(m_treeClearTestResult);
                    }
                    if (treeView_xml.SelectedNode.Tag != null && (int)treeView_xml.SelectedNode.Tag == XMLFILE)
                    {
                        treeView_xml.ContextMenu.MenuItems.Add(m_treeXmlEdit);
                    }
                    if ((int)treeView_xml.SelectedNode.Tag == RTFFILE || (int)treeView_xml.SelectedNode.Tag == LOGFILE)
                        treeView_xml.ContextMenu.MenuItems.Remove(m_treeReflesh);

                    m_treeReflesh.Enabled = button_start.Enabled;
                }
            }
        }
        /// <summary>
        /// 用编厉结点的方法，用红色字体标识出测试失败的用例
        /// </summary>
        public int SearchFailXmlNode(TreeNode m_TreeNode)
        {
            int NO_RUN = 0;
            int FAIL_RUN = 1;
            int SUCCESS_RUN = 2;
            int m_haveFailNode = NO_RUN;
            int ret = NO_RUN;

            //该函数为遍历函数
            while (m_TreeNode != null)
            {
                //如果节点是文件夹，那么遍历该节点
                if ((m_TreeNode.Tag != null && ((int)m_TreeNode.Tag == XMLDIRECTORY)))
                {
                    m_haveFailNode = SearchFailXmlNode(m_TreeNode.FirstNode);
                    if (m_haveFailNode == FAIL_RUN)
                        m_TreeNode.ForeColor = Color.Red;
                    else if (m_haveFailNode == SUCCESS_RUN)
                        m_TreeNode.ForeColor = Color.Green;
                }
                else if (m_TreeNode.Tag != null && (int)m_TreeNode.Tag == XMLFILE)
                {
                    if (m_TreeNode.ImageIndex == 9 || m_TreeNode.ImageIndex == 12)
                    {
                        m_TreeNode.ForeColor = Color.Red;
                        m_haveFailNode = FAIL_RUN;
                    }
                    else if (m_TreeNode.ImageIndex == 8)
                    {
                        m_haveFailNode = SUCCESS_RUN;
                        m_TreeNode.ForeColor = Color.Green;
                    }
                }
                else
                {
                    m_TreeNode.Parent.ForeColor = m_TreeNode.ForeColor;
                }
                m_TreeNode = m_TreeNode.NextNode;
                if (m_haveFailNode == FAIL_RUN)
                {
                    ret = FAIL_RUN;
                }
                else if (m_haveFailNode == SUCCESS_RUN && ret == NO_RUN)
                {
                    ret = SUCCESS_RUN;
                }
            }
            return ret;
        }

        /// <summary>
        /// 用红色字体标识出测试失败的用例
        /// </summary>
        public void LineOutFailXmlNode()
        {
            treeView_xml.BeginUpdate();
            m_TopNode.ForeColor = treeView_xml.ForeColor;
            SearchFailXmlNode(m_TopNode.FirstNode);
            treeView_xml.EndUpdate();
            m_FailNum = 0;
            m_SuccessNum = 0;
            for (int i = 0; i < m_XmlSelectNodeList.Count; i++)
            {
                if (((TreeNode)m_XmlSelectNodeList[i]).ImageIndex == 9 || ((TreeNode)m_XmlSelectNodeList[i]).ImageIndex == 12)
                {
                    if (m_FailNum == 0)
                    {
                        bool m_tempVoice = ProClass.bValIsOutVoice;
                        ProClass.bValIsOutVoice = false;
                        AddToTextBoxFail("\n下面是统计信息：", -1);
                        ProClass.bValIsOutVoice = m_tempVoice;
                    }
                    m_FailNum++;
                    AddToTextBoxFail(((TreeNode)m_XmlSelectNodeList[i]).FullPath.Substring(("所有用例").Length) + "		执行失败！", -1);
                }
                else if (((TreeNode)m_XmlSelectNodeList[i]).ImageIndex == 8)
                {
                    m_SuccessNum++;
                }
            }
            if (m_FailNum != 0)
            {
                bool m_tempVoice = ProClass.bValIsOutVoice;
                ProClass.bValIsOutVoice = false;
                AddToTextBoxFail("总共有 " + m_FailNum + " 个测试文件执行失败！", -1);
                ProClass.bValIsOutVoice = m_tempVoice;
            }
            if (m_XmlSelectNodeList.Count > 0)
                AddToTextBoxSuccess("\n\n测试用例总数：" + m_XmlSelectNodeList.Count + "， 执行成功个数：" + m_SuccessNum + "， 未执行个数：" + (m_XmlSelectNodeList.Count - m_SuccessNum - m_FailNum) +
                    "， 执行失败个数：" + m_FailNum + "		成功率：" + ((100 * m_SuccessNum) / m_XmlSelectNodeList.Count) + "%", -1, false);

        }

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("该工具主要是用来运行包含SQL语句的XML脚本，主要是针对数据库服务器作测试。\n如果对工具有问题，或是XML模板格式有不明白的地方，请用邮件或是QQ联系：\nE_MAIL: happysunfeng@163.com, QQ: 29867706 ", "工具介绍", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void treeView_xml_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            e.Node.SelectedImageIndex = e.Node.ImageIndex;
            if (((int)(e.Node.Tag) != RTFFILE && (int)(e.Node.Tag) != NONE) || !button_start.Enabled)//2代表是运行记录文件
            {
                return;
            }
            SaveMessageToFile();
            char[] separator = new char[2];
            separator[0] = '(';
            separator[1] = ')';
            string m_path1 = e.Node.FullPath;
            string[] m_pathArry = m_path1.Split(separator);
            m_path1 = m_pathArry[0];
            for (int i = 1; i < m_pathArry.Length; i++)
            {
                if (m_pathArry[i].StartsWith("\\"))
                {
                    m_path1 += m_pathArry[i];
                }
            }
            string m_path2 = "";
            if ((int)e.Node.Tag == NONE)
            {
                if (e.Node.Parent != null && (int)e.Node.Parent.Tag == XMLFILE)
                {
                    m_path1 = e.Node.Parent.FullPath.Substring("所有用例".Length);
                    try
                    {
                        AnlyseSyntax cAnlyseSyntax = new AnlyseSyntax();
                        richTextBox_success.Rtf = cAnlyseSyntax.FormatFileText(ProClass.sValXMLFilePath + "\\" + m_path1);
                    }
                    catch (Exception me)
                    {
                        AddToTextBoxFail(me.Message, -1);
                    }
                }
            }
            else if (e.Node.Text.IndexOf("Fail.RTF") > 0)
            {
                m_path2 = m_path1.Replace("Fail", "Success");
                try
                {
                    richTextBox_fail.LoadFile(Environment.CurrentDirectory + "\\" + m_path1);
                    richTextBox_success.LoadFile(Environment.CurrentDirectory + "\\" + m_path2);
                }
                catch (Exception me)
                {
                    AddToTextBoxSuccess(me.Message, -1, true);
                }
            }
            else if (e.Node.Text.IndexOf("Success.RTF") > 0)
            {
                m_path2 = m_path1.Replace("Success", "Fail");
                try
                {
                    richTextBox_success.LoadFile(Environment.CurrentDirectory + "\\" + m_path1);
                    richTextBox_fail.LoadFile(Environment.CurrentDirectory + "\\" + m_path2);
                }
                catch (Exception me)
                {
                    AddToTextBoxSuccess(me.Message, -1, true);
                }
            }
            else
            {
                StreamReader sr = null;
                //下面分别是从保存的文件中取内容
                try
                {
                    sr = new StreamReader(ProClass.sValXMLFilePath + "\\" + m_path1, Encoding.Default);
                    if (sr != null)
                    {
                        string m_tempStr;
                        richTextBox_success.Clear();
                        m_tempStr = sr.ReadLine();
                        while (m_tempStr != null && m_tempStr != "")
                        {
                            richTextBox_success.AppendText(m_tempStr);
                            richTextBox_success.AppendText("\n");
                            m_tempStr = sr.ReadLine();
                        }
                    }
                    sr.Close();
                }
                catch (Exception me)
                {
                    AddToTextBoxSuccess(me.Message, -1, true);
                    if (sr != null)
                        sr.Close();
                }
            }
            m_IsLogText = true;
        }


        public String CreateConnectStr()
        {
			string m_connectStr;
#if DM7
            m_connectStr = "Server=" + ProClass.GetServerName(ProClass.sValServer);
            m_connectStr += ";User Id=" + ProClass.sValUserId;
            m_connectStr += ";PWD=" + ProClass.sValPassword;
            Console.Write(m_connectStr);
#else
            
			m_connectStr = "Provider=" + ProClass.sValDriveName + ";User ID=" + ProClass.sValUserId;
			m_connectStr += ";Password=" + ProClass.sValPassword;
			if (ProClass.GetIsEnableConnectPool())
			{
				m_connectStr += ";Data Source=";
			}
			else
			{
				m_connectStr += ";Data Source=";
			}
			m_connectStr += ProClass.GetServerName(ProClass.sValServer);
			if (ProClass.sValDatabase.Length > 0)
			{
				m_connectStr += ";Initial Catalog=";
				m_connectStr += ProClass.sValDatabase;
			}
			if (ProClass.GetServerPort(ProClass.sValServer).Length > 0)
			{
				m_connectStr += ";Port=";
				m_connectStr += ProClass.GetServerPort(ProClass.sValServer);
			}
#endif
            return m_connectStr;
        }
        private void treeView_xml_BeforeCheck(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {

            if (bOwnCheck)
                return;
            if ((int)(e.Node.Tag) == RTFFILE || (int)(e.Node.Tag) == NONE || ((int)(e.Node.Tag) == RTFDIRECTORY))//2代表是运行记录文件
            {
                e.Cancel = true;
                return;
            }
        }

        private void menuItem_OpenFile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog m_OpenFileDialog = new OpenFileDialog();
            m_OpenFileDialog.AddExtension = true;
            m_OpenFileDialog.CheckFileExists = true;
            m_OpenFileDialog.CheckPathExists = true;
            m_OpenFileDialog.DefaultExt = "lst";
            m_OpenFileDialog.Filter = "列表文件(*.lst)|*.lst";
            m_OpenFileDialog.RestoreDirectory = true;
            m_OpenFileDialog.Title = "载入保存的节点状态";
            m_OpenFileDialog.ShowDialog();
            OpenFile(m_OpenFileDialog.FileName);
        }
        /// <summary>
        /// 设置节点状态
        /// </summary>
        public void SetNode(TreeNode FirstNode, ArrayList m_FileList, IComparer myComparerSr)
        {
            //该函数为遍历函数
            while (FirstNode != null)
            {
                //如果节点是文件夹，那么遍历该节点
                if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLDIRECTORY)
                {
                    SetNode(FirstNode.FirstNode, m_FileList, myComparerSr);
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    //如果是该节点是文件节点，则计算出它的全路径，加到运行列表中
                    if (m_FileList.BinarySearch(FirstNode.Text, myComparerSr) >= 0)
                    {
                        FirstNode.Checked = true;
                        if (!FirstNode.Parent.IsExpanded)
                        {
                            FirstNode.Parent.Expand();
                        }
                    }
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        /// <summary>
        /// 读取保存的节点状态信息
        /// </summary>
        private void OpenFile(string m_path)
        {
            if (m_path == "")
                return;
            if (m_path.Length > 256)
            {
                AddToTextBoxFail("打开的文件名非法，或是文件路径长度超过了256个字符，请输入正确的文件名", -1);
                return;
            }
            ArrayList m_FileList = null;
            StreamReader sr = null;
            //下面分别是从保存的设置文件中，读取值来初始化上面那些变量
            try
            {
                sr = new StreamReader(m_path);
                m_FileList = new ArrayList();
                if (m_FileList == null)
                    return;
                string m_tempStr = sr.ReadLine();
                while (m_tempStr != null && m_tempStr != "")
                {
                    m_FileList.Add(m_tempStr);
                    m_tempStr = sr.ReadLine();
                }
            }
            catch (Exception e)
            {
                AddToTextBoxFail(e.Message, -1);
                return;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            IComparer myComparerSr = new mySortStringClass();
            m_FileList.Sort(myComparerSr);
            treeView_xml.BeginUpdate();
            SetNode(m_TopNode, m_FileList, myComparerSr);
            treeView_xml.EndUpdate();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }
        //用来设置该节点以及该节点的子节点为选中状态
        private void SetChildCheck(TreeNode FirstNode)
        {
            if (FirstNode == null || FirstNode.Tag == null || ((int)FirstNode.Tag != XMLDIRECTORY && (int)FirstNode.Tag != XMLFILE))
                return;
            while (FirstNode != null)
            {
                FirstNode.Checked = true;
                if (((int)FirstNode.Tag == XMLDIRECTORY))//如果是文件夹，那么子节点也要设置为选中
                {
                    SetChildCheck(FirstNode.FirstNode);
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        //用来设置该节点以及该节点的子节点为不选中状态
        private void SetChildUnCheck(TreeNode FirstNode)
        {
            if (FirstNode == null || FirstNode.Tag == null || ((int)FirstNode.Tag != XMLDIRECTORY && (int)FirstNode.Tag != XMLFILE))
                return;
            while (FirstNode != null)
            {
                FirstNode.Checked = false;
                if (((int)FirstNode.Tag == XMLDIRECTORY))//如果是文件夹，那么子节点也要设置为选中
                {
                    SetChildUnCheck(FirstNode.FirstNode);
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        //用来取消该节点的父节点选中状态
        private void CancleFatherCheck(TreeNode FirstNode)
        {
            if (FirstNode == null)
                return;
            while (FirstNode.Parent != null)
            {
                FirstNode = FirstNode.Parent;
                FirstNode.Checked = false;
            }
        }
        private void treeView_xml_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (bOwnCheck)
                return;
            if (e.Node.Tag == null || ((int)e.Node.Tag != XMLDIRECTORY && (int)e.Node.Tag != XMLFILE))
                return;
            bOwnCheck = true;
            if (e.Node.Checked)
                SetChildCheck(e.Node.FirstNode);
            else
            {
                CancleFatherCheck(e.Node);
                SetChildUnCheck(e.Node.FirstNode);
            }
            bOwnCheck = false;
        }

        private void SeparateFailXmlFile(TreeNode FirstNode, string sOldName, string sNewName)
        {
            if (sOldName.Length >= 256)
            {
                AddToTextBoxFail("某文件夹路径深度已经超过了256个字符", -1);
                return;
            }
            DirectoryInfo m_di = null;
            try
            {
                m_di = new DirectoryInfo(sNewName);
                if (m_di == null)
                    return;
                if (!m_di.Exists)
                {
                    m_di.Create();
                }
            }
            catch (Exception e)
            {
                AddToTextBoxSuccess(e.Message, -1, true);
            }
            while (FirstNode != null)
            {
                TreeNode TempNode = FirstNode.NextNode;
                //如果节点是文件夹，那么遍历该节点
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLDIRECTORY)))
                {
                    SeparateFailXmlFile(FirstNode.FirstNode, sOldName + "\\" + FirstNode.Text, sNewName + "\\" + FirstNode.Text);
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    if (FirstNode.ImageIndex == 9)
                    {
                        try
                        {
                            File.Copy(sOldName + "\\" + FirstNode.Text, sNewName + "\\" + FirstNode.Text, true);
                            CopyAppendXmlFile(sOldName + "\\", sNewName + "\\", FirstNode.Text.Substring(0, FirstNode.Text.Length - ".xml".Length));
                            File.Delete(sOldName + "\\" + FirstNode.Text);
                            FirstNode.Parent.Nodes.Remove(FirstNode);
                        }
                        catch (Exception mes)
                        {
                            AddToTextBoxSuccess(mes.Message, -1, true);
                        }
                    }
                }
                FirstNode = TempNode;
            }
        }

        private void SeparateSuccessXmlFile(TreeNode FirstNode, string sOldName, string sNewName)
        {
            if (sOldName.Length >= 256)
            {
                AddToTextBoxFail("某文件夹路径深度已经超过了256个字符", -1);
                return;
            }
            DirectoryInfo m_di = null;
            try
            {
                m_di = new DirectoryInfo(sNewName);
                if (m_di == null)
                    return;
                if (!m_di.Exists)
                {
                    m_di.Create();
                }
            }
            catch (Exception e)
            {
                AddToTextBoxSuccess(e.Message, -1, true);
            }

            while (FirstNode != null)
            {
                TreeNode TempNode = FirstNode.NextNode;
                //如果节点是文件夹，那么遍历该节点
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLDIRECTORY)))
                {
                    SeparateSuccessXmlFile(FirstNode.FirstNode, sOldName + "\\" + FirstNode.Text, sNewName + "\\" + FirstNode.Text);
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    if (FirstNode.ImageIndex == 8)
                    {
                        try
                        {
                            File.Copy(sOldName + "\\" + FirstNode.Text, sNewName + "\\" + FirstNode.Text, true);
                            CopyAppendXmlFile(sOldName + "\\", sNewName + "\\", FirstNode.Text.Substring(0, FirstNode.Text.Length - ".xml".Length));
                            File.Delete(sOldName + "\\" + FirstNode.Text);
                            FirstNode.Parent.Nodes.Remove(FirstNode);
                        }
                        catch (Exception mes)
                        {
                            AddToTextBoxSuccess(mes.Message, -1, true);
                        }
                    }
                }
                FirstNode = TempNode;
            }
        }
        //该函数用来移动跟XML文件相配的EXE和RAR文件
        private void CopyAppendXmlFile(string sOldPath, string sNewPath, string sFileName)
        {
            try
            {
                File.Copy(sOldPath + sFileName + ".exe", sNewPath + sFileName + ".exe", true);
                File.Delete(sOldPath + sFileName + ".exe");
            }
            catch (Exception mes)
            {
                AddToTextBoxFail(mes.Message, -1);
            }
            try
            {
                File.Copy(sOldPath + sFileName + ".rar", sNewPath + sFileName + ".rar", true);
                File.Delete(sOldPath + sFileName + ".rar");
            }
            catch (Exception mes)
            {
                AddToTextBoxFail(mes.Message, -1);
            }
        }
        private void cMenuItemSpSu_Click(object sender, System.EventArgs e)
        {
            DirectoryInfo m_di = null;
            try
            {
                m_di = new DirectoryInfo(ProClass.sValXMLFilePath + "\\成功测例");
                if (m_di == null)
                    return;
                if (!m_di.Exists)
                {
                    m_di.Create();
                }
            }
            catch (Exception mes)
            {
                AddToTextBoxSuccess(mes.Message, -1, true);
            }

            AddToTextBoxSuccess("开始分离运行成功的XML脚本文件", -1, false);
            TreeNode FirstNode = m_TopNode.FirstNode;
            while (FirstNode != null)
            {
                TreeNode TempNode = FirstNode.NextNode;
                //如果节点是文件夹，那么遍历该节点
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLDIRECTORY)))
                {
                    if (FirstNode.Text.CompareTo("成功测例") == 0)
                    {
                    }
                    else if (FirstNode.Text.CompareTo("失败测例") == 0)
                    {
                        SeparateSuccessXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, Environment.CurrentDirectory + "\\成功测例\\");
                    }
                    else
                    {
                        SeparateSuccessXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, Environment.CurrentDirectory + "\\成功测例\\" + FirstNode.Text);
                    }
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    if (FirstNode.ImageIndex == 9)
                    {
                        try
                        {
                            File.Copy(ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\成功测例\\" + FirstNode.Text, true);
                            CopyAppendXmlFile(ProClass.sValXMLFilePath + "\\", ProClass.sValXMLFilePath + "\\成功测例\\", FirstNode.Text.Substring(0, FirstNode.Text.Length - ".xml".Length));
                            File.Delete(ProClass.sValXMLFilePath + "\\" + FirstNode.Text);
                            FirstNode.Parent.Nodes.Remove(FirstNode);
                        }
                        catch (Exception mes)
                        {
                            AddToTextBoxSuccess(mes.Message, -1, true);
                        }
                    }
                }
                FirstNode = TempNode;
            }
            AddToTextBoxSuccess("分离结束", -1, false);

        }

        private void cMenuItemSpFa_Click(object sender, System.EventArgs e)
        {
            DirectoryInfo m_di = null;
            try
            {
                m_di = new DirectoryInfo(ProClass.sValXMLFilePath + "\\失败测例");
                if (m_di == null)
                    return;
                if (!m_di.Exists)
                {
                    m_di.Create();
                }
            }
            catch (Exception mes)
            {
                AddToTextBoxSuccess(mes.Message, -1, true);
            }

            AddToTextBoxSuccess("开始分离运行失败的XML脚本文件", -1, false);
            TreeNode FirstNode = m_TopNode.FirstNode;
            while (FirstNode != null)
            {
                TreeNode TempNode = FirstNode.NextNode;
                //如果节点是文件夹，那么遍历该节点
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLDIRECTORY)))
                {
                    if (FirstNode.Text.CompareTo("失败测例") == 0)
                    {
                    }
                    else if (FirstNode.Text.CompareTo("成功测例") == 0)
                    {
                        SeparateFailXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\失败测例\\");
                    }
                    else
                    {
                        SeparateFailXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\失败测例\\" + FirstNode.Text);
                    }
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    if (FirstNode.ImageIndex == 9)
                    {
                        try
                        {
                            File.Copy(ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\失败测例\\" + FirstNode.Text, true);
                            CopyAppendXmlFile(ProClass.sValXMLFilePath + "\\", ProClass.sValXMLFilePath + "\\失败测例\\", FirstNode.Text.Substring(0, FirstNode.Text.Length - ".xml".Length));
                            File.Delete(ProClass.sValXMLFilePath + "\\" + FirstNode.Text);
                            FirstNode.Parent.Nodes.Remove(FirstNode);
                        }
                        catch (Exception mes)
                        {
                            AddToTextBoxSuccess(mes.Message, -1, true);
                        }
                    }
                }
                FirstNode = TempNode;
            }
            AddToTextBoxSuccess("分离结束", -1, false);
        }

        private void MainForm_Closed(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void treeView_xml_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (46 == e.KeyValue)
            {
                if (treeView_xml.SelectedNode != null)
                {
                    treeView_xml.SelectedNode.Remove();
                }
            }

        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {

        }
        private void CreateNewDir(string strDir, string strDirName)//新建目录
        {
            try
            {

                DirectoryInfo myDirInfo = new DirectoryInfo(strDir);
                myDirInfo.CreateSubdirectory(strDirName);
                //MessageBox.Show("在"+strDir+"创建目录成功！！","创建目录成功");
            }
            catch
            {
                //ShowFMessage("错误的目录格式,创建目录失败！！");
                AddToTextBoxFail("错误的目录格式,创建目录失败！！", -1);

            }


        }
        /*private void CreateNewFile(string strDir, string strFilename)//新建文件
        {
            try
            {
                FileInfo myFileInfo = new FileInfo(strDir + strFilename);
                FileStream myFileStream = myFileInfo.Create();
				
                //MessageBox.Show("在"+strDir+"创建文件成功！！","创建文件chenggong");
                myFileStream.Close();
            }
            catch
            {
                ShowFMessage("错误的文件格式,创建文件失败！！");

            }
        }*/
        private void button_Start_WF_Click(object sender, System.EventArgs e)
        {
            mytestarr.Clear();//清除保存测试结果信息的mytestarr数组，保证mytestarr数组的元素都是点击“开始保存测试结果信息”之后的测试结果信息
            DateTime tm = DateTime.Now;//获得当前的时间
            string timeDate = tm.ToLongDateString();//获得当前的年、月、日时间

            string time = tm.ToLongTimeString();
            time = time.Replace(":", "_");//把“：”用“-”代替
            time = timeDate + "\\" + time + ".txt";//获得当前年、月、日、时、分、秒的字符串表示形式，为下面建立以当前的时刻为名的“.txt”的文件做准备
            string path = Environment.CurrentDirectory + "\\测试结果";//获得要要创建目录所在的完全路径
            path += "\\";
            CreateNewDir(path, timeDate);//创建一个当前的时间为名的文件夹
            //CreateNewFile(path,time);
            w_path = path + time;//把要创建文件的路径保存在此全局变量中，为下面建立此文件做准备
        }

        private void button_End_RSF_Click(object sender, System.EventArgs e)
        {
            FileInfo myfile = new FileInfo(w_path);
            StreamWriter sw = myfile.CreateText();
            DateTime tm = DateTime.Now;
            string time_end = tm.ToLongTimeString();//获得当前时间，
            for (int m = mytestarr.Count - 1; m >= 0; m--)//主要用于时间的比较，为了确保在mytestarr中的元素都是点击“结束保存测试结果信息”之前的保存的测试结果信息
            {
                if (((((RWArr)mytestarr[m]).TestDateTime).CompareTo(time_end)) > 0)//把时间大于点击“结束保存测试结果信息”时的mytestarr中的元素给删除掉
                    mytestarr.RemoveAt(m);
                else
                    break;
            }

            for (int j = 0, n = 0; j < mytestarr.Count; j = j + n)//主要用于排序，把文件名相同的文件排列在一起，主要是为了便于比较
            {
                n = 1;
                for (int k = j + 1; k < mytestarr.Count; k++)
                {
                    if ((((RWArr)mytestarr[j]).TestExName) == (((RWArr)mytestarr[k]).TestExName))
                    {
                        n++;
                        RWArr hRWArr;
                        hRWArr.TestExName = ((RWArr)mytestarr[k]).TestExName;
                        hRWArr.TestDateTime = ((RWArr)mytestarr[k]).TestDateTime;
                        hRWArr.TestResult = ((RWArr)mytestarr[k]).TestResult;
                        mytestarr.Insert(j + 1, hRWArr);
                        mytestarr.RemoveAt(k + 1);
                    }
                }
            }
            for (int i = 0; i < mytestarr.Count; i++)//把排序之后的mytestarr中的元素写到以开始点击“开始保存测试结果信息”的时间为名所建立的“.txt”文件中
            {
                sw.Write(i + "\t");//保存序号
                sw.Write(((RWArr)mytestarr[i]).TestExName + "\t");
                sw.Write(((RWArr)mytestarr[i]).TestDateTime + "\t");
                sw.WriteLine(((RWArr)mytestarr[i]).TestResult + "\t");
            }

            sw.Close();
        }

		private void richTextBox_success_TextChanged(object sender, System.EventArgs e)
		{
		
		}



        //=======

 
    }
	

	
		/// <summary>
		/// 测试线程类
		/// </summary>
		public class TestThread   //！！TestThread类 	建立测试线程 	循环执行xml文件 一个测试线程相当于一次工具的执行过程。
		{
			[DllImport("kernel32.dll")]  
			private  static  extern  UInt32  GetCurrentThreadId(); 
			private  static  Queue oQueue=new Queue();
			private string m_XmlFileName;	//存放文件名的完整路径
			private int	m_threadIndex;      //线程索引号
			private MainForm m_MainForm;
			private string m_FileName;		//存放文件名
			private UInt32 m_ThreadId;	//存放当前线程的句柄
			XmlTest m_xmlTest;			//当前测试的XML文件对像
			public bool m_AleadyStop;	//表示该对像已经结束测试

			public TestThread()
			{
				m_threadIndex = -1;
			}
			/// <summary>
			/// 设置当前窗口对像的引用名称
			/// </summary>
			public void SetForm1(MainForm m_f1)
			{
				m_MainForm = m_f1;
			}
			/// <summary>
			/// 设置当前线程在管理数组中的ID
			/// </summary>
			public void SetThreadIndex(int m_id)
			{
				m_threadIndex = m_id;
			}
			/// <summary>
			/// 把链表中的SQL语句保存成文件
			/// </summary>
			public void SaveSqlCase()
			{
				m_MainForm.SaveSqlCase();
			}
			/// <summary>
			/// 往SQL链中压入一条消息
			/// </summary>
			public void PushSqlCase(string m_sql)
			{
				m_MainForm.PushSqlCase(m_sql);
			}
			/// <summary>
			/// 输出成功消息函数
			/// </summary>
			public void ShowSMessage(string m_txt, bool m_fail)
			{
				if(m_txt == "")
				{
					return;
				}
				string m_message;
				m_message = m_FileName + ": " + m_txt;
				m_MainForm.BeginInvoke(m_MainForm.AddSuccessText, new object[] {m_message, m_threadIndex, m_fail});				
			}
			/// <summary>
			/// 输出失败消息函数
			/// </summary>
			public void ShowFMessage(string m_txt, bool iNoVoice)
			{
				if(m_txt == "")
				{
					return;
				}
				ShowSMessage(m_txt, true);
				string m_message;
				m_message = m_FileName + ": ";
				m_message += m_txt;
				if(ProClass.bValIsOutVoice && iNoVoice)
					Thread.Sleep(1000);
				m_MainForm.BeginInvoke(m_MainForm.AddFailText, new object[] {m_message, m_threadIndex, iNoVoice});
			}
			public void ShowFMessage(string m_txt)
			{
				if(m_txt == "")
				{
					return;
				}
				ShowSMessage(m_txt, true);
				string m_message;
				m_message = m_FileName + ": ";
				m_message += m_txt;
				if(ProClass.bValIsOutVoice)
					Thread.Sleep(1000);
				m_MainForm.BeginInvoke(m_MainForm.AddFailText, new object[] {m_message, m_threadIndex, true});
			}
			public void ActiveWindow()
			{
				m_MainForm.Activate();
			}
			/// <summary>
			/// 返回线程的句柄
			/// </summary>
			public UInt32 GetThreadID()
			{
				return	m_ThreadId;
			}
			/// <summary>
			/// 线程运行函数IntPtr 
			/// </summary>
			public void Run()
			{
				m_AleadyStop = false;
				Debug.Assert(m_threadIndex>=0, "线程在管理数组中的ID没有被指定", "TestThread.Run 函数");
				Debug.Assert(m_MainForm!=null, "线程中m_MainForm参数没有被给定值", "TestThread.Run 函数");
				if(m_threadIndex < 0 || m_threadIndex >= ProClass.GetThreadNum())
				{
					ShowFMessage("非法的线程ID：" + m_threadIndex);
					return;
				}
				if(m_MainForm == null)
				{
					ShowFMessage("无效的窗口指针");
					return;
				}
				m_ThreadId = GetCurrentThreadId();//得到当前线程的ID
				try
				{
					while(!m_MainForm.m_stopTest)
					{
						m_XmlFileName = m_MainForm.GetXmlFileName(m_MainForm.GetXmlFileIndex(m_threadIndex));				//获取下一个测试文件
						if(m_XmlFileName == "")
						{
							//		Debug.Assert(m_XmlFileName!="", "得到的XML文件名长度为空串", "TestThread.Run 函数");
							System.Threading.Thread.Sleep(0);
							continue;
						}
						m_FileName = GetSingleFileName();						//从文件的完整路径中得到文件名

						SetXmlFileNameToArray(m_XmlFileName);					//把当前运行的文件名设置到线程管理数组
						m_xmlTest = new XmlTest(this);							//申请一个测试文件对像
						m_xmlTest.xmlFileName = m_FileName;
             //          ShowFMessage("当前测试文件名：" + m_FileName);
                    //        m_MainForm.AddToTextBoxSuccess(, -1, false);
                        m_xmlTest.SetXmlFile(m_XmlFileName);
						if(m_MainForm.m_runLog)
							m_xmlTest.RunLog();
						else
							m_xmlTest.Run(false);										//运行测试文件对像
						if(m_xmlTest.GetSuccessInfo())
						{
#if !DM6
                            int m_index;

                            m_index = m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex;
                            m_index = (m_index << 16) + 8;
                            m_MainForm.treeView_xml.BeginInvoke(new System.EventHandler(m_MainForm.UpdateSelectNodeList), m_index);
#else
							((TreeNode)m_MainForm.m_XmlSelectNodeList[m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex]).ImageIndex = 8;
#endif
						}
						else
						{ 
#if !DM6
                            int m_index;

                            m_index = m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex;
                            m_index = (m_index << 16) + 9;
                            m_MainForm.treeView_xml.BeginInvoke(new System.EventHandler(m_MainForm.UpdateSelectNodeList), m_index);
#else
							((TreeNode)m_MainForm.m_XmlSelectNodeList[m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex]).ImageIndex = 9;
#endif
							if (ProClass.GetIsLoopRun() && !ProClass.GetIsErrRun()) 
							{
								m_xmlTest = null;
								SetXmlFileNameToArray("");
								break;
							}
						}
#if DM6
						if(((TreeNode)m_MainForm.m_XmlSelectNodeList[m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex]).IsSelected)
							((TreeNode)m_MainForm.m_XmlSelectNodeList[m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex]).SelectedImageIndex =
								((TreeNode)m_MainForm.m_XmlSelectNodeList[m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex]).ImageIndex;
#else
                        {
                            int m_index;

                            m_index = m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex;
                            m_index = (m_index << 16) + 0xffff;
                            m_MainForm.treeView_xml.BeginInvoke(new System.EventHandler(m_MainForm.UpdateSelectNodeList), m_index);
                        }
#endif
						m_xmlTest = null;
						SetXmlFileNameToArray("");								//把当前运行的文件名"空串"设置到线程管理数组
					}
				}
				catch(ThreadAbortException e)
				{
					string m_txt = e.Message;
#if !DM6
                    int m_index;

                    m_index = m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex;
                    m_index = (m_index << 16) + 12;
                    m_MainForm.treeView_xml.BeginInvoke(new System.EventHandler(m_MainForm.UpdateSelectNodeList), m_index);
#else
					((TreeNode)m_MainForm.m_XmlSelectNodeList[m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex]).ImageIndex = 12;
#endif
					ShowFMessage("线程被非正常中断，该项测试已经失败，请检察消息记录");				
				}
				finally
				{
					CheckThreadStop();
				}
			}
			/// <summary>
			/// 线程运行结束后，设置按钮的状态
			/// </summary>
			private void CheckThreadStop()
			{			
				Monitor.Enter(oQueue);			 
			{
				m_AleadyStop = true;
				bool m_EnableStartButton = true;
				for(int i=0; i<ProClass.GetThreadNum(); i++)
				{
					if(!m_MainForm.m_thread_struct[i].m_TestThread.m_AleadyStop)//如果所有线程都已经停止了运行，那么，设置开始按钮为可用的状太
					{
						m_EnableStartButton = false;
						break;
					}
				}
				if(m_EnableStartButton)
				{
					m_MainForm.SetButtonState(true, true, false, false);
#if DM6
					m_MainForm.LineOutFailXmlNode();
#endif
				}
			}
				Monitor.Exit(oQueue);//释放锁
			}
			/// <summary>
			/// 把当前线程运行的XML文件信息写到线程管理数组中
			/// </summary>
			private void SetXmlFileNameToArray(string m_str)
			{
				Monitor.Enter(m_MainForm.m_thread_struct);
				if(m_str == "")
				{
					m_MainForm.m_thread_struct[m_threadIndex].m_xmlIndex = -1;
				}
				m_MainForm.m_thread_struct[m_threadIndex].m_xmlFileName = m_str;
				Monitor.Exit(m_MainForm.m_thread_struct);
			}
			/// <summary>
			/// 从完整路径中得到文件名的名字
			/// </summary>
			private string GetSingleFileName()
			{
				int m_index = -1;
				m_index = m_XmlFileName.LastIndexOf("\\");
				if(m_index == -1)
				{
					return m_XmlFileName;
				}
				else
				{
					return m_XmlFileName.Substring(m_index+1);
				}
			}
			/// <summary>
			/// 用来强行断开测试对像的连接
			/// </summary>
			public void DisConnect()
			{
				if(m_xmlTest != null)
					m_xmlTest.DisConnect(-1);//断开测试文件的连接
				SetXmlFileNameToArray("");
			}
			/// <summary>
			/// 用来清除XML文件的运行环境
			/// </summary>
			public void RunClearEnvironment()
			{
				bool m_temp = ProClass.bValIsRandRun;
				ProClass.bValIsRandRun = false;
				do
				{
					m_XmlFileName = m_MainForm.GetXmlFileName(m_MainForm.GetXmlFileIndex(m_threadIndex));
					if(m_XmlFileName == "")
					{
						Debug.Assert(m_XmlFileName!="", "得到的XML文件名长度为空串", "TestThread.RunClearEnvironment 函数");
						break;
					}
					m_FileName = GetSingleFileName();
					m_xmlTest = new XmlTest(this);
					m_xmlTest.SetXmlFile(m_XmlFileName);
					m_xmlTest.ClearEnvironment();
				}while(m_MainForm.m_TreeNodeIndex != 0);
				ProClass.bValIsRandRun = m_temp;
			}
		}
		/// <summary>
		/// 执行语句对像，用来存放执行过的语句
		/// </summary>
		public class SQL_CASE
		{
			private string m_sql;	//存放语句
			public string sql
			{
				set
				{
					m_sql = value;
				}
				get
				{
					return m_sql;
				}
			}
			public SQL_CASE pNext;	//指向下一个本对像
		}
		class  Voice  //报警声音对像
		{  
			[DllImport("kernel32.dll")]  
			private  static  extern  int  Beep(int  dwFreq  ,int  dwDuration)  ;  
		
			public void CreateVoice()  
			{  
				int  a=0X7FF;  
				int  b=100;  
				Beep(a,b);  
			}  
		}  

		public class mySortFileInfoClass : IComparer  
		{
			// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
			int IComparer.Compare( object x, object y )  
			{
				return(String.Compare(((FileInfo)x).Name, ((FileInfo)y).Name ));
			}
		}
		public class mySortDirectoryInfoClass : IComparer  
		{
			// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
			int IComparer.Compare( object x, object y )  
			{
				return(String.Compare(((DirectoryInfo)x).Name, ((DirectoryInfo)y).Name ));
			}
		}
		public class mySortStringClass : IComparer  
		{
			// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
			int IComparer.Compare( object x, object y )  
			{
				return(String.Compare((String)x, (String)y));
			}
		}

	
}
