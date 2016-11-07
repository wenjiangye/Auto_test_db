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
	/// �����̵߳Ľṹ��������������߳�
	/// </summary>
	public struct THREAD_STRUCT
	{
		public Thread m_thread;
		public TestThread m_TestThread;
		public string m_xmlFileName;
		public int m_xmlIndex;			//��ѡ���ļ������е�����
		public DateTime m_DateTime;
	}
	public struct RWArr//���ڼ�¼���Խ����Ϣ��Ȼ�����¼����Ϣ���浽mytestarr������
	{
		public string TestExName;//���ڼ�¼�����������ļ���
		public string TestDateTime;//���ڼ�¼���Դ˲���������ʱ��
		public string TestResult;//���ڼ�¼�˲��������Ĳ��Խ�������ǡ��ɹ������ǡ�ʧ�ܡ�
	}
	
	/// <summary>
	/// ������
	/// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
#if DM7
        public const string sVersion = " -- �汾(1.0.8.0 Build2012.10.11(DM7))";
#else
        public const string sVersion = " -- �汾(1.0.8.0 Build2012.10.11(DM6))";
#endif
        ///Tag����ֵΪ-1,˵���ýڵ㲻����Ч�Ľڵ㣬��������ʾXML�ļ�������������������Ϣ��
        ///Tag����ֵΪ0, ˵���ýڵ��Ǵ���һ���ļ���
        ///Tag����ֵΪ1, ˵���ýڵ��Ǵ���һ��XML�����ļ�
        ///Tag����ֵΪ2, ˵���ýڵ��Ǵ���һ��RTF��¼�ļ�
        ///Tag����ֵΪ3, ˵���ýڵ��Ǵ���һ��RTF��¼�ļ���
        ///Tag����ֵΪ4, ˵���ýڵ��Ǵ���һ��XMLִ�б���ʱ��������SQL����¼�ļ���
        ///Tag����ֵΪ5, ˵���ýڵ��Ǵ���һ��XMLִ�б���ʱ��������SQL����¼�ļ�
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
        public delegate void AddTextToTextBoxSuccessDelegate(string m_text, int m_id, bool m_fail);	//����ʾ������Ϣ���������Ϣ��ί�к���
        public delegate void AddTextToTextBoxFailDelegate(string m_text, int m_id, bool iNoVoice);		//����ʾʧ����Ϣ���������Ϣ��ί�к���
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
        public  System.Windows.Forms.TreeView treeView_xml;				//�ڵ���
        private System.Windows.Forms.Button button_start;				//��ʼ��ť
        private System.Windows.Forms.RichTextBox richTextBox_fail;		//��ʾ������Ϣ���ı���
        private System.Windows.Forms.RichTextBox richTextBox_success;	//��ʾ������Ϣ���ı���
        private System.Windows.Forms.Button button_finish;				//��ɰ�ť
        private System.Windows.Forms.Button button_finish_all;			//��ֹ��ť
        private System.Windows.Forms.Button button_set;					//���ð�ť
        private MenuItem m_treeReflesh;									//��ˢ�²˵�
        private MenuItem m_treeOpen;									//���ڵ�򿪲����˵�
        private MenuItem m_treeRun;										//���ڵ����в����˵�
        private MenuItem m_treeXmlEdit;									//�ýű��༭����
		private MenuItem m_treeImportTestResult;						//�Ѳ��Խ�����뵽���ݿ���
		private MenuItem m_treeClearTestResult;							//��ղ��Խ��
        //		private MenuItem m_treeNewDi;									//���ڵ��½��ļ��в����˵�
        //		private MenuItem m_treeReName;									//���ڵ��������ļ������˵�
        //		private MenuItem m_treeDelete;									//���ڵ�ɾ���ļ������˵�
        private Queue oGetXmlQueue = new Queue();						//�ڻ�ȡ�����ļ���������ֻ֤��һ���߳�ִ���Ǹ�����
        private Queue oSqlCaseQueue = new Queue();						//�ڻ�ȡm_SqlCase��ʹ��Ȩ����ֻ֤��һ���߳�ִ��
        private TreeNode m_TopNode;										//���������ĵ�һ�����
        private TreeNode m_MonuseNode;									//�����λ���ϵĽ��
        private SQL_CASE m_SqlCase;										//ָ�����ִ�����Ľṹ��

        public AddTextToTextBoxSuccessDelegate AddSuccessText;			//��������ʾ������Ϣ���������Ϣ��ί�к���
        public AddTextToTextBoxFailDelegate AddFailText;				//��������ʾʧ����Ϣ���������Ϣ��ί�к���
        public AddNodeDelegate AddNodeDl;
        public DeleteNodeDelegate DeleteNodeDl;
        public THREAD_STRUCT[] m_thread_struct;							//��������̵߳Ľṹ
        public int m_TreeNodeIndex;									//ָʾ��ǰ���������߳�����xml�ļ���������е�����
        private ArrayList m_XmlFileList;								//������ŵ�ǰ��ѡ�е�XML�ļ��б�
        public ArrayList m_XmlSelectNodeList;	//�������ѡ�е����ڵ㡣������һ����������һһ��Ӧ��
        public static ArrayList mytestarr;      //���ڱ�����Խ������Ϣ
        public string w_path;
        public int m_message_num;										//������ʾ��ǰ��Ϣ���Ѿ���ʾ�˶�������Ϣ
        public bool m_stopTest;											//ָʾ�Ƿ�Ҫͣ�²����߳�
        public bool m_runLog;											//ָʾҪ���е�ʱ���ǲ��Բ�����SQL����¼�ļ�
        public bool m_IsLogText;										//ָʾ��ǰ��Ϣ������ʾ���Ƿ�����ʷ��Ϣ
        private bool bOwnCheck;											//ָ����ǰ��CHECK��Ϣ�ǳ����ڲ��Լ���������
        private int m_FailNum;											//ָʾ��ǰ���������ܹ��ж��ٸ�ʧ��
        private int m_SuccessNum;										//ָʾ��ǰ���������ܹ��ж��ٸ��ɹ�
        private System.Threading.Timer m_CheckMessageTimer;				//������ʱ���ʸ����߳�����������Ϣ���Ա��˽������Ƿ�����������

        private bool m_HaveNodeCheck;									//ָʾ��ǰ�����Ƿ��нڵ㱻��
        private bool bIni;												//ָʾ�����Ѿ����й�һ����

        private FileSystemWatcher watcher;								//�ļ�Ŀ¼���Ӷ���
        private System.Windows.Forms.Button button_Clear;				//�����ť
        private System.Windows.Forms.MainMenu FileMenu;					//���˵�
        private System.Windows.Forms.MenuItem menuItem_F;				//ˢ�����˵�
        private System.Windows.Forms.MenuItem menuItem_S;				//�������˵�
        private System.Windows.Forms.MenuItem menuItem_refleshTree;		//ˢ�����˵�
        private System.Windows.Forms.MenuItem menuItem_SaveFile;		//������Ϣ�˵�
        private System.Windows.Forms.MenuItem menuItem_Exit;			//�˳��˵�
        private System.Windows.Forms.MenuItem menuItem_ShowMessage;		//�����Ƿ���ʾ��Ϣ�˵�
        private System.Windows.Forms.MenuItem menuItem_Warning;			//�����Ƿ��������Ĳ˵�
        private System.Windows.Forms.MenuItem menuItem_AutoSave;		//�����Ƿ����Զ�������Ϣ�˵�
        private System.Windows.Forms.MenuItem menuItem_AutoClear;		//�����Ƿ���ÿ�ε����ʼ��ťʱ���Զ������Ϣ���Ĳ˵�
        private System.Windows.Forms.MenuItem menuItem_Loop;			//�����Ƿ�ѭ�����Բ˵�
        private System.Windows.Forms.MenuItem menuItem_Set;				//���ڵ�ͼ���б�
        private System.Windows.Forms.MenuItem menuItem1;				//���ڲ˵�
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem_OpenFile;				//�����˵�
        private System.ComponentModel.IContainer components;

        private ProClass proVal;
        public MainForm()
        {
            //8.27 �޸� ���������м���IP
			//string conn=ProClass.CommServer.ConnectSev();

            //
            // Windows ���������֧���������
            //
            InitializeComponent();
            this.Text += sVersion;//�����������
            //
            // TODO: �� InitializeComponent ���ú�����κι��캯������
            //������Ų��Խ����Ŀ¼
            try
            {
                DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory);
                m_di.CreateSubdirectory("���Խ��");

            }
            catch (Exception e)
            {
                string m_txt = e.Message;
            }
            m_stopTest = true;
            m_SqlCase = null;

            //���������������ʾ��һЩ����
            treeView_xml.HideSelection = false;		//ʧȥ����ʱ��Ҳ��ʾѡ�еĽڵ�
            treeView_xml.HotTracking = true;		//��ʾ�»���
            bOwnCheck = false;

            //����Ϊ���б����ҽ��˵�
            MenuItem myMenuItemFile;
            treeView_xml.ContextMenu = new ContextMenu();
            m_treeReflesh = new MenuItem("ˢ��(&F)", (new System.EventHandler(this.menuFlushNode_Click)), Shortcut.CtrlF);
            m_treeOpen = new MenuItem("��(&O)", (new System.EventHandler(this.menuOpenNode_Click)), Shortcut.CtrlQ);
            m_treeRun = new MenuItem("����(&R)", (new System.EventHandler(this.button_start_Click)), Shortcut.CtrlR);
            m_treeXmlEdit = new MenuItem("�༭(&E)", (new System.EventHandler(this.button_edit_Click)), Shortcut.CtrlE);
			m_treeImportTestResult = new MenuItem("�������ݿ�(&I)", (new System.EventHandler(this.button_import_Click)), Shortcut.CtrlI);
			m_treeClearTestResult = new MenuItem("���(&C)", (new System.EventHandler(this.button_clear_Click)), Shortcut.None);
            //	m_treeNewDi = new MenuItem("�½��ļ���(&N)", (new System.EventHandler(this.menuNewNode_Click)), Shortcut.CtrlN);									//���ڵ��½��ļ��в����˵�
            //	m_treeReName = new MenuItem("������(&M)", (new System.EventHandler(this.menuReName_Click)), Shortcut.CtrlM);									//���ڵ��������ļ������˵�
            //	m_treeDelete = new MenuItem("ɾ��(&D)", (new System.EventHandler(this.menuFlushNode_Click)), Shortcut.CtrlD);									//���ڵ�ɾ���ļ������˵�
           treeView_xml.ContextMenu.MenuItems.Add(m_treeReflesh);
            //ˢ����
            menuItem_refleshTree.Click += new System.EventHandler(this.menuFlushNode_Click);
            menuItem_refleshTree.Shortcut = Shortcut.CtrlF;
            //������Ϣ�˵�
            menuItem_SaveFile.Click += new System.EventHandler(this.menuSaveFile_Click);
            menuItem_SaveFile.Shortcut = Shortcut.CtrlS;
            //�˳�����
            menuItem_Exit.Click += new System.EventHandler(this.menuExit_Click);
            menuItem_Exit.Shortcut = Shortcut.CtrlX;
            //��Ϣ��ʾ�˵�
            menuItem_ShowMessage.Click += new System.EventHandler(this.menuShowMessage_Click);
            menuItem_ShowMessage.Shortcut = Shortcut.CtrlP;
            //���������˵�
            menuItem_Warning.Click += new System.EventHandler(this.menuWaring_Click);
            menuItem_Warning.Shortcut = Shortcut.CtrlW;
            //�Զ�������Ϣ�˵�
            menuItem_AutoSave.Click += new System.EventHandler(this.menuAutoSave_Click);
            menuItem_AutoSave.Shortcut = Shortcut.CtrlU;
            //�Զ������Ϣ�˵�
            menuItem_AutoClear.Click += new System.EventHandler(this.menuAutoClear_Click);
            menuItem_AutoClear.Shortcut = Shortcut.CtrlL;
            //ѭ������
            menuItem_Loop.Click += new System.EventHandler(this.menuLoop_Click);
            menuItem_Loop.Shortcut = Shortcut.CtrlK;
            //�������ò˵�
            menuItem_Set.Click += new System.EventHandler(this.button_set_Click);
            menuItem_Set.Shortcut = Shortcut.CtrlT;
            //Ϊ��ʾ�������Ϣ����Ҽ��˵�
            richTextBox_fail.ContextMenu = new ContextMenu();
            myMenuItemFile = new MenuItem("����(&Y)");
            myMenuItemFile.Shortcut = Shortcut.CtrlC;
            richTextBox_fail.ContextMenu.MenuItems.Add(myMenuItemFile);
            myMenuItemFile.Click += new System.EventHandler(this.menuClearAndCopyMessage_Click);
            myMenuItemFile = new MenuItem("���(&C)");
            richTextBox_fail.ContextMenu.MenuItems.Add(myMenuItemFile);
            myMenuItemFile.Click += new System.EventHandler(this.menuClearAndCopyMessage_Click);
            //Ϊ��ʾ������Ϣ����Ҽ��˵�
            richTextBox_success.ContextMenu = richTextBox_fail.ContextMenu;
            //��ʼ��һЩ����ʱ����
            m_message_num = 0;			//��Ϣ�������е���Ϣ��������ֵ����ÿ�α����굱ǰ��Ϣ����0
            m_TreeNodeIndex = 0;		//��ǰ���е��ļ�����ѡ�е����ڵ��ļ��б��е���������ֵ����Ҫ��ѭ������ʱ�������б����һ���ļ��������ص�һ����ʼ

            m_XmlFileList = new ArrayList();		//��������ŵ�ǰ�û�ѡ�����е������ļ�ȫ·��
            m_XmlSelectNodeList = new ArrayList();	//��������ѡ�е����ڵ�
            mytestarr = new ArrayList();
            AddSuccessText = new AddTextToTextBoxSuccessDelegate(AddToTextBoxSuccess);//��ʼ��ί�Ϻ���
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


            FillTreeView();				//������б��ѵ�ǰĿ¼�����е�XML�ļ�׷�ӽ��ڵ�
			//8.27 �޸� ���������� ������б�� ����IP  ����ǰ����

			//8.27 (2)�޸�  �Ժ��ÿһ�����Ƕ���ͨ�ŵ� ��һ��䲻Ҫ
			//string conn=ProClass.CommServer.ConnectSev();



            //		OpenFile(Environment.CurrentDirectory + "\\���Խ��\\Default.lst");
            //����һ����ʱ����ÿ��������һ�¸��̵߳�������Ϣʱ��͵�ǰʱ��ļ�����Ա��ж������Ƿ���������
            TimerCallback timerDelegate1 = new TimerCallback(CheckMessageTime);
            m_CheckMessageTimer = new System.Threading.Timer(timerDelegate1, this, 1000, ProClass.GetMsgCheckTime() * 1000);
            m_IsLogText = false;//ָʾ��ǰ��Ϣ������ʾ���Ƿ�����ʷ��Ϣ
            this.AcceptButton = button_start;
            this.CancelButton = button_finish;
            CreateWatcher();
        }    //MainForm������

        /// <summary>
        /// ������������ʹ�õ���Դ��
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

        #region Windows ������������ɵĴ���
        /// <summary>
        /// �����֧������ķ��� - ��Ҫʹ�ô���༭���޸�
        /// �˷��������ݡ�
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
            this.button1.Text = "���������ͨ��";
            // 
            // button_End_RSF
            // 
            this.button_End_RSF.ForeColor = System.Drawing.Color.Red;
            this.button_End_RSF.Location = new System.Drawing.Point(440, 8);
            this.button_End_RSF.Name = "button_End_RSF";
            this.button_End_RSF.Size = new System.Drawing.Size(136, 23);
            this.button_End_RSF.TabIndex = 7;
            this.button_End_RSF.Text = "����������Խ����Ϣ";
            this.button_End_RSF.Click += new System.EventHandler(this.button_End_RSF_Click);
            // 
            // button_Start_WF
            // 
            this.button_Start_WF.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.button_Start_WF.Location = new System.Drawing.Point(8, 8);
            this.button_Start_WF.Name = "button_Start_WF";
            this.button_Start_WF.Size = new System.Drawing.Size(136, 23);
            this.button_Start_WF.TabIndex = 6;
            this.button_Start_WF.Text = "��ʼ������Խ����Ϣ";
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
            this.button_continue.Text = "����(&T)";
            this.button_continue.Click += new System.EventHandler(this.button_continue_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.ForeColor = System.Drawing.Color.SeaGreen;
            this.button_Clear.Location = new System.Drawing.Point(656, 8);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(80, 24);
            this.button_Clear.TabIndex = 4;
            this.button_Clear.Text = "�������(&C)";
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // button_set
            // 
            this.button_set.ForeColor = System.Drawing.Color.SeaGreen;
            this.button_set.Location = new System.Drawing.Point(584, 8);
            this.button_set.Name = "button_set";
            this.button_set.Size = new System.Drawing.Size(64, 24);
            this.button_set.TabIndex = 3;
            this.button_set.Text = "����(&S)";
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
            this.button_finish_all.Text = "��ֹ(&T)";
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
            this.button_finish.Text = "ֹͣ(&B)";
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
            this.button_start.Text = "��ʼ(&R)";
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
            this.menuItem_F.Text = "�ļ�(&F)";
            // 
            // menuItem_OpenFile
            // 
            this.menuItem_OpenFile.Index = 0;
            this.menuItem_OpenFile.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuItem_OpenFile.Text = "��(&O)";
            this.menuItem_OpenFile.Click += new System.EventHandler(this.menuItem_OpenFile_Click);
            // 
            // menuItem_SaveFile
            // 
            this.menuItem_SaveFile.Index = 1;
            this.menuItem_SaveFile.Text = "����(&S)";
            // 
            // menuItem_refleshTree
            // 
            this.menuItem_refleshTree.Index = 2;
            this.menuItem_refleshTree.Text = "ˢ����(&F)";
            // 
            // menuItem_Exit
            // 
            this.menuItem_Exit.Index = 3;
            this.menuItem_Exit.Text = "�˳�(&X)";
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
            this.menuItem_S.Text = "����(&S)";
            this.menuItem_S.Popup += new System.EventHandler(this.menu_SetMenu_Click);
            // 
            // menuItem_ShowMessage
            // 
            this.menuItem_ShowMessage.Index = 0;
            this.menuItem_ShowMessage.Text = "��ʾ��Ϣ(&P)";
            // 
            // menuItem_Warning
            // 
            this.menuItem_Warning.Index = 1;
            this.menuItem_Warning.Text = "���󱨾�(&W)";
            // 
            // menuItem_AutoSave
            // 
            this.menuItem_AutoSave.Index = 2;
            this.menuItem_AutoSave.Text = "�Զ�����(&A)";
            // 
            // menuItem_AutoClear
            // 
            this.menuItem_AutoClear.Index = 3;
            this.menuItem_AutoClear.Text = "�Զ����(&C)";
            // 
            // menuItem_Loop
            // 
            this.menuItem_Loop.Index = 4;
            this.menuItem_Loop.Text = "ѭ������(&L)";
            // 
            // menuItem_Set
            // 
            this.menuItem_Set.Index = 5;
            this.menuItem_Set.Text = "��������(&S)";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.cMenuItemSpSu,
            this.cMenuItemSpFa});
            this.menuItem3.Text = "�������(&B)";
            // 
            // cMenuItemSpSu
            // 
            this.cMenuItemSpSu.Index = 0;
            this.cMenuItemSpSu.Text = "����ɹ�����";
            this.cMenuItemSpSu.Click += new System.EventHandler(this.cMenuItemSpSu_Click);
            // 
            // cMenuItemSpFa
            // 
            this.cMenuItemSpFa.Index = 1;
            this.cMenuItemSpFa.Text = "����ʧ�ܲ���";
            this.cMenuItemSpFa.Click += new System.EventHandler(this.cMenuItemSpFa_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 3;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2});
            this.menuItem1.Text = "����(&H)";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "����(&A)";
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
            this.Text = "XML�Զ����Թ���";
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
        /// Ӧ�ó��������ڵ㡣
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
                newNode.Tag = XMLDIRECTORY;	//��ʾ�ý��������һ��Ŀ¼
                newNode.ImageIndex = 2;
                tNode.Nodes.Add(newNode);
                SearchXmlFile(newNode, ProClass.sValXMLFilePath + newNode.FullPath.Substring(("���в���").Length));
                //SearchXmlFile(newNode, Environment.CurrentDirectory + newNode.FullPath.Substring(("���в���").Length));
            }
            else  //��XMLFILE�ļ�
            {
                newNode.ImageIndex = 4;    //��ʼXMLFILE�ļ���ͼ��
                newNode.Tag = XMLFILE;		//��ʾ�ý��������һ���ļ�			
                TreeNode TempNode = new TreeNode("");	//����һ����㣬����������ʾ��ӦXML�ļ�������
                TempNode.Tag = NONE;
                TempNode.ImageIndex = 5;  //��ӦXML�ļ���������� 
                newNode.Nodes.Add(TempNode);
                tNode.Nodes.Add(newNode);  //һ������tNode��
            }
        }
        // Define the event handlers.
        private void OnCreate(object source, FileSystemEventArgs e)
        {
            bool bDi = false;
            int iRes = CheckIsXmlFile(e.FullPath);
            if (iRes == NONE)
                return;
            bDi = (iRes == XMLDIRECTORY);//��ʾ�½������ļ���
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
                AddToTextBoxFail("�����б���δ�ҵ�Ҫ���������ļ���" + e.FullPath, -1);
                return;
            }
            sSpi = e.Name.Split(separator);
            FirstNode.Text = sSpi[sSpi.Length - 1];//.ToUpper()

        }
        private int CheckIsXmlFile(string sPath)
        {
            if (sPath.StartsWith("���Խ��"))
                return NONE;
            if (sPath.Length < ProClass.sValXMLFilePath.Length)
            {
                AddToTextBoxFail("�޸ĵ��ļ�·�����ڵ�ǰĿ¼���棿: " + sPath, -1);
                return NONE;
            }
            if (sPath.Length > 256)
            {
                AddToTextBoxFail("�������ļ�·��̫��: " + sPath, -1);
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
                AddToTextBoxFail("�ж��ļ��Ƿ����ļ���ʱ�������쳣��: " + e.Message, -1);
            }
            if (!bDi)
            {
                if (sPath.EndsWith(".xml") || sPath.EndsWith(".XML") || sPath.EndsWith(".Xml"))
                    return 1;//��ʾ���ļ�
                else
                    return NONE;
            }
            else
            {
                return XMLDIRECTORY;//��ʾ���ļ���
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
        //����Ŀ¼���Ӷ�����
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
        /// //�½��ļ���
        /// </summary>
        private void button_edit_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //�����ǰѡ�еĲ˵����ǿս�㣬���ҽڵ�Tag��ʶ��Ϊ�ջ�������ֵ��Ϊ-1����ִ�д򿪵Ĳ�����ע���ڵ�tag��ʶΪ-1��˵��Ϊ��ʾ�ļ�������Ϣ�Ľ��
            if ((treeView_xml.SelectedNode != null && treeView_xml.SelectedNode.Tag != null && (int)treeView_xml.SelectedNode.Tag != NONE) || treeView_xml.SelectedNode.Equals(m_TopNode))
            {
                //���ݵ�ǰ·������ڵ��������ļ���ȫ·��
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
                catch (Exception we)//���ܻᷢ���Ҳ����ļ����쳣
                {
                    AddToTextBoxFail("��ȷ��SqlXmlEdit���ߣ��Ƿ��ڸó����Ŀ¼����\n" + we.Message, -1);
                    return;
                }
            }
            else
            {
                AddToTextBoxFail("��ѡ����ȷ�Ľ�㣬�������ѡ�У�Ȼ�������Ҽ��˵���", -1);
            }
        }
		private void button_import_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.No == MessageBox.Show("ȷ��Ҫ�Ѳ��Խ�����뵽���ݿ���", "ȷ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			AddToTextBoxSuccess_low("���ڵ�����Ϣ�����ݿ⣬���������̹�����˵�����Խ�����Ѿ��ۼ��˺ܶ���Լ�¼������Ҫ���������ղ���", -1, false);
			string ret = ProClass.ImportTestResult();
			if(ret == null)
			{
				AddToTextBoxSuccess_low("�Ѿ��ɹ����뵽�����ݿ⣬����Ϊ " + ProClass.sValUserId + ".TEST_RESULT", -1, false);
			}
			else
			{
				AddToTextBoxSuccess_low("��������з�������" + ret, -1, true);
				return;
			}
			if(DialogResult.No == MessageBox.Show("�Ƿ�Ҫ�ԱȲ��Լ�¼�е����ݣ�ע�⣺���ѡ�ǣ���ô���߻��������������ɾ����ִֻ����һ�εļ�¼!", "ȷ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			try//���Է������Ƿ���Ч
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
					AddToTextBoxSuccess_low("�����ļ�		ִ��ʱ��		�Ƿ�ɹ�", 3, false);
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
			if(DialogResult.No == MessageBox.Show("ȷ��Ҫ��ղ��Լ�¼��", "ȷ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return;
			ProClass.RebuileTestResult();
			DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory + "\\" + "���Խ��");
			di.Delete(true);
			AddToTextBoxSuccess("�Ѿ����", -1, false);
		}
        /// <summary>
        /// //Ϊ���ò˵�������Ӳ˵�ǰ���
        /// </summary>
        private void menu_SetMenu_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //����Ϊ���ò˵�������Ӳ˵�ǰ���
            if (ProClass.GetIsSaveMsg())//�Զ�������Ϣ�˵�
            {
                menuItem_AutoSave.Checked = true;
            }
            else
            {
                menuItem_AutoSave.Checked = false;
            }
            if (ProClass.GetIsShowMsg())//��Ϣ��ʾ�˵�
            {
                menuItem_ShowMessage.Checked = true;
            }
            else
            {
                menuItem_ShowMessage.Checked = false;
            }
            if (ProClass.GetIsOutVoice())//���������˵�
            {
                menuItem_Warning.Checked = true;
            }
            else
            {
                menuItem_Warning.Checked = false;
            }
            if (ProClass.GetIsAutoClearMsg())//�Զ������Ϣ�˵�
            {
                menuItem_AutoClear.Checked = true;
            }
            else
            {
                menuItem_AutoClear.Checked = false;
            }
            if (ProClass.GetIsLoopRun())//ѭ������
            {
                menuItem_Loop.Checked = true;
            }
            else
            {
                menuItem_Loop.Checked = false;
            }
            if (button_start.Enabled)//���öԻ���˵�
            {
                menuItem_Set.Enabled = true;
            }
            else
            {
                menuItem_Set.Enabled = false;
            }
        }
        /// <summary>
        /// //�Զ�ѭ�����в���
        /// </summary>
        private void menuLoop_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            if (ProClass.GetIsLoopRun())//�����ǰ�����Զ�ѭ������
            {
                ((MenuItem)sender).Checked = false;
                ProClass.AddProValue(ProClass.ProIsLoop, false.ToString());
            }
            else//�������෴
            {
                ProClass.AddProValue(ProClass.ProIsLoop, true.ToString());
                ((MenuItem)sender).Checked = true;
            }
        }
        /// <summary>
        /// //�Զ������Ϣ�˵�
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
        /// //�Զ�����˵�
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
        /// //���������˵�
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
        /// //��ʾ��Ϣ
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
        /// //�˳���Ϣ�˵��¼�
        /// </summary>
        private void menuExit_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            this.Close();
            //		Application.Exit();
        }

        /// <summary>
        /// //����������ڵ���Ϣ
        /// </summary>
        private void SaveFile(string m_SaFiName)
        {
            if (m_SaFiName == "")
                return;
            if (m_SaFiName.Length > 256)
            {
                AddToTextBoxFail("�ļ�·�����ȳ�����256���ַ�����������ȷ���ļ���", -1);
                return;
            }
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(m_SaFiName);
                if (sw == null)
                {
                    AddToTextBoxFail("δ�ܴ���ָ�����ļ���" + m_SaFiName, -1);
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
                AddToTextBoxFail("����������ڵ���Ϣʱ����\n" + ee.Message, -1);
                if (sw != null)
                {
                    sw.Close();
                }
                this.Cursor = System.Windows.Forms.Cursors.Arrow;
                return;
            }
        }
        /// <summary>
        /// //����˵��¼�
        /// </summary>
        private void menuSaveFile_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            SaveFileDialog m_SaveFileDialog = new SaveFileDialog();
            m_SaveFileDialog.DefaultExt = "lst";
            m_SaveFileDialog.Filter = "�б��ļ�(*.lst)|*.lst";
            try
            {
                m_SaveFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\���Խ��";
            }
            catch (Exception we)//���ܻᷢ���Ҳ����ļ����쳣
            {
                AddToTextBoxFail(we.Message, -1);
            }
            m_SaveFileDialog.RestoreDirectory = true;
            m_SaveFileDialog.Title = "����ѡ�нڵ���Ϣ��";
            m_SaveFileDialog.ShowDialog();
            SaveFile(m_SaveFileDialog.FileName);
        }
        /// <summary>
        /// //���ڵ�򿪲˵������¼�
        /// </summary>
        private void menuOpenNode_Click(object sender, System.EventArgs e)
        {
            // Create a new OpenFileDialog and display it.
            //�����ǰѡ�еĲ˵����ǿս�㣬���ҽڵ�Tag��ʶ��Ϊ�ջ�������ֵ��Ϊ-1����ִ�д򿪵Ĳ�����ע���ڵ�tag��ʶΪ-1��˵��Ϊ��ʾ�ļ�������Ϣ�Ľ��
            if ((treeView_xml.SelectedNode != null && treeView_xml.SelectedNode.Tag != null && (int)treeView_xml.SelectedNode.Tag != NONE) || treeView_xml.SelectedNode.Equals(m_TopNode))
            {
                //���ݵ�ǰ·������ڵ��������ļ���ȫ·��
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
                catch (Exception we)//���ܻᷢ���Ҳ����ļ����쳣
                {
                    AddToTextBoxFail(we.Message, -1);
                    return;
                }
            }
            else
            {
                AddToTextBoxFail("��ѡ����ȷ�Ľ�㣬�������ѡ�У�Ȼ�������Ҽ��˵���", -1);
            }
        }
        /// <summary>
        /// //���ڵ�ˢ���¼�
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
                    // SearchXmlFile(treeView_xml.SelectedNode, Environment.CurrentDirectory + treeView_xml.SelectedNode.FullPath.Substring(("���в���").Length));
                    SearchXmlFile(treeView_xml.SelectedNode, ProClass.sValXMLFilePath + treeView_xml.SelectedNode.FullPath.Substring(("���в���").Length));
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
                    //string m_DiPath = ProClass.sValXMLFilePath + "\\" + m_path1;   00:00��� �Ƿ���ȷ��
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
        /// �˵�����¼��������滹������������Ϣ�򿽱��ַ������¼�
        /// Ϊ��ʵ�ַ��㣬��������������˵�ִ�м��ɵ���һ����������
        /// </summary>
        private void menuClearAndCopyMessage_Click(object sender, System.EventArgs e)
        {
            //�˵�����¼��������滹������������Ϣ�򿽱��ַ������¼�
            if (((MenuItem)sender).Text == "���(&C)")//�������ղ˵�����������Ϣ
            {
                SaveMessageToFile();
            }
            else
            {
                if (richTextBox_success.SelectionLength > 0 && richTextBox_success.Focused)//�������ʾ������Ϣ�򷢳��Ŀ�����Ϣ
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
        /// ��SQL���ѹ�뻺����
        /// </summary>
        public void PushSqlCase(string m_sql)
        {
            Console.Write("sql���" + m_sql);

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
        /// �ѻ�������SQL���д����Ӧ��XML�ļ�
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
            string m_path = Environment.CurrentDirectory + "\\���Խ��\\" + m_tempData + "\\ErrorLog\\Error_" + m_tempTime + ".RTF";

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
                    DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory + "\\���Խ��");
                    m_di.CreateSubdirectory(m_tempData);
                    m_di = new DirectoryInfo(Environment.CurrentDirectory + "\\���Խ��\\" + m_tempData);
                    m_di.CreateSubdirectory("ErrorLog");
                    sw = new StreamWriter(m_path, true, Encoding.Default);
                }
                catch (IOException ee)
                {
                    AddToTextBoxSuccess(ee.Message, -1, true);
                    try
                    {
                        DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory + "\\���Խ��\\" + m_tempData);
                        m_di.CreateSubdirectory("ErrorLog");
                        sw = new StreamWriter(m_path);
                    }
                    catch (Exception eee)
                    {
                        MessageBox.Show("δ���ڲ��Խ�����洴���ļ��� ErrorLog \n������ϢΪ��" + eee.Message);
                        Monitor.Exit(oSqlCaseQueue);
                        return;
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("δ���ڲ��Խ�����洴���ļ���" + m_tempData + " \n������ϢΪ��" + ee.Message);
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
                MessageBox.Show("���滺������SQL����\n" + ee.Message);
                if (sw != null)
                {
                    sw.Close();
                }
            }
            Monitor.Exit(oSqlCaseQueue);
        }
        /// <summary>
        /// //��ʼ��SQL���ṹ��
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
                    AddToTextBoxFail("û���㹻�Ŀռ���װ��ִ�������", -1);
                    return;
                }

                m_SqlCase = (SQL_CASE)m_SqlCase.pNext;
            }
            m_SqlCase.pNext = m_temp;
        }
        /// <summary>
        /// //�ѵ�ǰ�����ת��ͬ������һ�����
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
        /// //������ǰĿ¼�����е�Ŀ¼���ҳ�LOG�ļ�
        /// </summary>
        public void SearchLogFile(TreeNode FirstNode, string m_diName)
        {
            m_diName += "\\";
            if (m_diName.Length >= 256)
            {
                AddToTextBoxFail("ĳ�ļ���·������Ѿ�������256���ַ�", -1);
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
            foreach (DirectoryInfo diTemp in di)//�õ���ǰ�ڵ�Ŀ¼�����е���Ŀ¼
            {
                TreeNode newNode = new TreeNode(diTemp.Name);
                if (diTemp.Name == "Fail" || diTemp.Name == "Success")
                {
                    newNode.Tag = RTFDIRECTORY;	//��ʾ�ý��������һ�������¼�ļ���Ŀ¼
                }
                else
                {
                    newNode.Tag = LOGRTFDIRECTORY;	//Tag����ֵΪ4, ˵���ýڵ��Ǵ���һ��XMLִ�б���ʱ��������SQL����¼�ļ���
                }
                newNode.ImageIndex = 2;
                FirstNode.Nodes.Add(newNode);
                SearchLogFile(newNode, m_diName + diTemp.Name);
                if ((int)newNode.Tag == RTFDIRECTORY)
                {
                    newNode.Text = newNode.Text + "(" + newNode.Nodes.Count + ")";
                }
            }
            //	FirstNode = ReturnLastNode(FirstNode);//�ѵ�ǰ�����ת��ͬ������һ����㣬�Է���������������µĽڵ�
            FileInfo[] fi = null;
            fi = m_di.GetFiles("*.RTF");//�õ���ǰ�ڵ�Ŀ¼�������е�RTF�ļ�
            IComparer myComparerFi = new mySortFileInfoClass();
            Array.Sort(fi, myComparerFi);
            foreach (FileInfo fiTemp in fi)//��������ӵ�������б���
            {
                TreeNode newNode = new TreeNode(fiTemp.Name);
                newNode.ImageIndex = 13;
                if ((int)(FirstNode.Tag) == LOGRTFDIRECTORY)
                    newNode.Tag = LOGFILE;		//Tag����ֵΪ5, ˵���ýڵ��Ǵ���һ��XMLִ�б���ʱ��������SQL����¼�ļ�
                else
                    newNode.Tag = RTFFILE;		//��ʾ�ý��������һ����¼�ļ�
                FirstNode.Nodes.Add(newNode);
            }
        }
        /// <summary>
        /// //������ǰĿ¼�����е�Ŀ¼���ҳ�xml�ļ�
        /// </summary>
        public void SearchXmlFile(TreeNode FirstNode, string m_diName)
        {
            m_diName += "\\";
            if (m_diName.Length >= 256)
            {
                AddToTextBoxFail("ĳ�ļ���·������Ѿ�������256���ַ�", -1);
                return;
            }
            DirectoryInfo m_di = null;
            DirectoryInfo[] di = null;
            try
            {
                m_di = new DirectoryInfo(m_diName);
                if (m_di == null)
                    return;
                di = m_di.GetDirectories();//���ص�ǰĿ¼����Ŀ¼
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
            foreach (DirectoryInfo diTemp in di)//�õ���ǰ�ڵ�Ŀ¼�����е���Ŀ¼
            {
                if ((diTemp.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    if (diTemp.Name != "���Խ��")//���Ŀ¼������Ϊ���Խ������ô��Ŀ¼�µ��ļ��в��ᱻ��ӵ�����
                    {
                        TreeNode newNode = new TreeNode(diTemp.Name);
                        newNode.Tag = XMLDIRECTORY;	//��ʾ�ý��������һ��Ŀ¼
                        newNode.ImageIndex = 2;
                        FirstNode.Nodes.Add(newNode);
                        SearchXmlFile(newNode, m_diName + diTemp.Name);
                    }
                }
            }
            FileInfo[] fi = m_di.GetFiles("*.xml");//�õ���ǰ�ڵ�Ŀ¼�������е�XML�ļ�
            IComparer myComparerFi = new mySortFileInfoClass();
            Array.Sort(fi, myComparerFi);
            foreach (FileInfo fiTemp in fi)//��������ӵ�������б���
            {
                TreeNode newNode = new TreeNode(fiTemp.Name);
                newNode.ImageIndex = 4;
                newNode.Tag = XMLFILE;		//��ʾ�ý��������һ���ļ�
                FirstNode.Nodes.Add(newNode);
                TreeNode TempNode = new TreeNode("");	//����һ����㣬����������ʾ��ӦXML�ļ�������
                TempNode.Tag = NONE;
                TempNode.ImageIndex = 5;
                newNode.Nodes.Add(TempNode);
            }
        }

        /// <summary>
        /// //�ѵ�ǰĿ¼�µ��ļ�����䵽�������
        /// </summary>
        public void FillTreeView()
        {
            //////////////////////////////////////////////
            ///�ڵ�˵����
            ///ÿ���ڵ��Tag������������ָ���ڵ������
            ///Tag����ֵΪ-1,˵���ýڵ㲻����Ч�Ľڵ㣬��������ʾXML�ļ�������������������Ϣ��
            ///Tag����ֵΪ0, ˵���ýڵ��Ǵ���һ���ļ���
            ///Tag����ֵΪ1, ˵���ýڵ��Ǵ���һ��XML�����ļ�
            ///Tag����ֵΪ2, ˵���ýڵ��Ǵ���һ��RTF��¼�ļ�
            ///Tag����ֵΪ3, ˵���ýڵ��Ǵ���һ��RTF��¼�ļ���
            ///Tag����ֵΪ4, ˵���ýڵ��Ǵ���һ��XMLִ�б���ʱ��������SQL����¼�ļ���
            ///Tag����ֵΪ5, ˵���ýڵ��Ǵ���һ��XMLִ�б���ʱ��������SQL����¼�ļ�
            //////////////////////////////////////////////	
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            treeView_xml.BeginUpdate();//�����κ�����ͼ�ػ�
            treeView_xml.Nodes.Clear();//�Ӽ�����ɾ���������ڵ�
            treeView_xml.ImageList = m_imageList;//�õ��ڵ�ͼ��
            TreeNode FirstNode = new TreeNode("��������");
            FirstNode.Tag = XMLDIRECTORY;		//����һ����Ч�Ĳ��Խڵ㣬˵����Ŀ¼�ڵ�		
            FirstNode.ImageIndex = 0;//��ȡ�����õ�ǰ���ڵ㴦��δѡ��״̬ʱ����ʾͼ���ͼ���б�����ֵ
            treeView_xml.Nodes.Add(FirstNode);
            m_TopNode = FirstNode;//��ǰ���ڵ�ĵ�һ���ڵ�

            if (ProClass.sValXMLFilePath == null || ProClass.sValXMLFilePath == "")
                ProClass.sValXMLFilePath = Environment.CurrentDirectory;
            SearchXmlFile(FirstNode, ProClass.sValXMLFilePath);
            TreeNode m_result = new TreeNode("���Խ��");
            m_result.Tag = RTFDIRECTORY;		//Tag����ֵΪ3, ˵���ýڵ��Ǵ���һ��RTF��¼�ļ���
            m_result.ImageIndex = 2;   //RTF��¼�ļ���
            treeView_xml.Nodes.Add(m_result);
            SearchLogFile(m_result, Environment.CurrentDirectory + "\\���Խ��");
            FirstNode.Expand();
            treeView_xml.EndUpdate();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }

        //���ڵ��չ���¼�   ����·��ProClass.sValXMLFilePathDir��ȡ�ļ�
        private void treeView_xml_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            //�������������̬����ʾ�ڵ�ͼ��
            if ((int)e.Node.Tag == NONE)
                e.Node.ImageIndex = 1;
            else if ((int)e.Node.Tag == XMLDIRECTORY || (int)e.Node.Tag == RTFDIRECTORY)
            {
                e.Node.ImageIndex = 3;   //�ļ���
            }
            //�жϵ�ǰ�Ľڵ���ӽڵ㣬��ʶ�Ƿ�Ϊ���ַ�������ǣ���ô������������ʾXML�ļ������Ľڵ�
            if (e.Node.FirstNode != null && e.Node.FirstNode.Text == "")
            {
                XmlTextReader xmltext = null;
                XmlDocument doc = null;
                try
                {
                    //���ɵ�ǰ�ڵ�����ڵ�ǰĿ¼��ȫ·��
                    string m_path = "";
                    //Ӧ�ø������·������ȡ�ļ�
                    if (e.Node.FullPath.StartsWith("��������"))
                        //m_path = Environment.CurrentDirectory + e.Node.FullPath.Substring(("��������").Length);
                        m_path = ProClass.sValXMLFilePath + e.Node.FullPath.Substring(("��������").Length);
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
                    xmlList = doc.GetElementsByTagName("CONTENT");//��ȡ�ļ������������ֵ���Ϣ������д���ڵ�����ʾ����
                    if (xmlList != null && xmlList.Count >= 1)
                        e.Node.FirstNode.Text = xmlList[0].InnerXml;
                    else
                        e.Node.FirstNode.Text = "��������Ϣ";
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
                    e.Node.FirstNode.Text = "��������Ϣ";
                }
            }
        }

        private void CreateIniDatabase()
        {
            try//���Է������Ƿ���Ч
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
                Console.Write("Ĭ�����ݿ�" + ProClass.sValDatabase);
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
                    AddToTextBoxSuccess("���ڴ�����ʼ�����п⣬���Ժ�......", -1, false);
                //    cm.CommandText = "CREATE SCHEMA " + ProClass.sValDatabase + " DATAFILE '" + ProClass.sValDatabase + ".DBF' SIZE 256";
                       cm.CommandText = "CREATE SCHEMA " + ProClass.sValDatabase ;
                    try
                    {
                        cm.ExecuteNonQuery();
                        AddToTextBoxSuccess("��ʼ�����п�ɹ�����", -1, false);
                    }
#if DM7
                    catch (DmException mes)
#else
                    catch (OleDbException mes)
#endif
                    {
                        AddToTextBoxFail("��ʼ���ⴴ��ʧ�ܣ�" + mes.Message, -1);
                    }
                }
                cn.Close();
            }
            catch (Exception e)
            {
                AddToTextBoxFail(e.Message, -1);
            }
        }
        //��ʼ��ť�¼�
        private void button_start_Click(object sender, System.EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            if (bIni == false)//��һ������
            {
                CreateIniDatabase();
                AddToTextBoxSuccess("��һ������...", -1 ,false);
                bIni = true;
            }
            if (ProClass.GetIsAutoClearMsg())//�Ƿ���ÿ�ο�ʼ����ǰ�������Ϣ��
            {
                SaveMessageToFile();
            }
            m_IsLogText = false;           //ָʾ��ǰ��Ϣ������ʾ���Ƿ�����ʷ��Ϣ
            //����������ť��״̬
            button_start.Enabled = false;
            button_continue.Enabled = false;
            button_finish.Enabled = true;
            button_finish_all.Enabled = false;
            button_set.Enabled = false;
            button_Clear.Enabled = button_set.Enabled;
            m_stopTest = false;//��ʶ��ʼ���Ա�ʶ��   ָʾ�Ƿ�Ҫͣ�²����߳�
            //�õ����б�ѡ���Ľڵ������ļ��б�
            GetSelectXmlList(true);
          //  m_XmlFileList 		    ��������ŵ�ǰ�û�ѡ�����е������ļ�ȫ·��
          //  m_XmlSelectNodeList 	    ��������ѡ�е����ڵ�
            CreateThread();
            richTextBox_success.Focus();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }
        //������ť�¼�   
        private void button_continue_Click(object sender, System.EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            if (bIni == false)
            {
                CreateIniDatabase();
                bIni = true;
            }
            if (ProClass.GetIsAutoClearMsg())//�Ƿ���ÿ�ο�ʼ����ǰ�������Ϣ��
            {
                SaveMessageToFile();
            }
            m_IsLogText = false;
            //����������ť��״̬
            button_continue.Enabled = false;
            button_start.Enabled = false;
            button_finish.Enabled = true;
            button_finish_all.Enabled = false;
            button_set.Enabled = false;
            button_Clear.Enabled = button_set.Enabled;
            m_stopTest = false;//��ʶ��ʼ���Ա�ʶ��
            //�õ����б�ѡ���Ľڵ������ļ��б�
            GetSelectXmlList(true);

            //������õ����ļ��б�������ɾ�����Ѿ�ִ�й����ļ����ӵ�һ��û��ִ�й����ļ��п�ʼִ�С�
            //����ImageIndex ==4�ж��Ƿ�ִ�й���һ��ʼ��ImageIndex ==4   line617
            //��ImageIndex ==8ִ�гɹ�  ImageIndex ==9 ִ��ʧ��   ImageIndex ==12�쳣�жϣ�����ֹͣ
            //�����߳�ִ���б��е��ļ�  .ImageIndex = 8
            //for(int i=0;i<m_XmlSelectNodeList.Count;i++)
            //{
            while (true)
            {
                if (    //������ж��Ƿ�ִ�й��������в���  if��Ϊִ�й�
                    ((TreeNode)m_XmlSelectNodeList[0]).ImageIndex == 8 ||
                    ((TreeNode)m_XmlSelectNodeList[0]).ImageIndex == 9 ||
                    ((TreeNode)m_XmlSelectNodeList[0]).ImageIndex == 12
                    )
                    m_XmlSelectNodeList.RemoveAt(0);   //ɾ�����ý��  Ҳ�ɿ���ʹ��RemoveRrange
                else break;     //�ҵ�δִ�еĵ�һ��������ѭ�� 
            }
            CreateThread();
            richTextBox_success.Focus();
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }

        //ֹͣ��ť�¼�
        private void button_finish_Click(object sender, System.EventArgs e)
        {
            //�ı�����״̬����ʶ��Ҫֹͣ����
            m_stopTest = true;
            button_continue.Enabled = true;
            button_finish.Enabled = false;
            button_finish_all.Enabled = true;
            richTextBox_success.Focus();
        }

        //��ֹ��ť�¼�
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
            //��ֹ�����߳�
            //�������һ���ܴ�����⡣������һ���߳���Ϊִ��ĳ����䱻�����Ժ�ɱ�����Ӧ���Ǹ��̣߳��������ͷ��߳�����������ִ�С���Ϊ��ADOִ���У����Լ����½����߳���ִ�����ġ�
            button_finish_all.Enabled = false;
            if (m_thread_struct != null)
            {
                //�����Ǽ���߳������и����̵߳�����״̬��
                for (int i = 0; i < ProClass.GetThreadNum(); i++)
                {
                    if (m_thread_struct[i].m_xmlFileName == null || m_thread_struct[i].m_xmlFileName == "" || m_thread_struct[i].m_thread.ThreadState == System.Threading.ThreadState.Stopped || m_thread_struct[i].m_thread.ThreadState == System.Threading.ThreadState.Aborted)
                        continue;//����߳��Ѿ��Զ���ֹͣ��������
                    else//����̻߳�������
                    {
                        m_thread_struct[i].m_thread.Abort();		//��ͼ��ֹһ���̵߳�����
                        m_thread_struct[i].m_thread.Join(10000);		//�ȴ�����ֹ
                        //������У�����API��ɱ��.��ʵ������û�ж���ô�����������
                        if (m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Stopped && m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Aborted)
                        {
                            AddToTextBoxFail("�߳� " + i + " ��ǿ����ֹ�����߳����еĲ����Ѿ�ʧ�ܣ���鿴��Ϣ��¼", -1);
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
            //�����Ǹı䰴ť��״̬
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
            //�����öԻ�����������¶�ȡ�ļ��е�������Ϣ
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
                //8.27 �޸�  �������֮��ı��˷������˵�IP������Ҫ�رյ�ǰ���ӣ����½����µ�����
				if(ProClass.sValOldServerIP!=ProClass.sValServer)
				{
					ProClass.CommServer.CloseStream();
					string con=ProClass.CommServer.ConnectSev(); 
					//if(con=="connect success");
				}

                m_thread_struct = null;	//���߳�����ָ����գ���Ϊ���ò��������ܻ�ı��̵߳ĸ���������Ҫ���������µ�����
            }
        }
        /// <summary>
        /// //����Ϣ���е����ݱ��浽�ļ�
        /// </summary>
        public void SaveMessageToFile()
        {
            m_message_num = 0;
            if (!ProClass.GetIsSaveMsg() || m_IsLogText)//�����������Ϊ�ļ�
            {
                richTextBox_fail.Clear();
                richTextBox_success.Clear();
                return;
            }
            string m_path = Environment.CurrentDirectory + "\\" + "���Խ��";
            DirectoryInfo m_di = new DirectoryInfo(m_path);

            //�����Ǹ��ݵ�ǰʱ�䣬������Ӧ���ļ���
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
                MessageBox.Show("δ���ڲ��Խ�����洴���ļ���" + m_temp);
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
        /// //���ܵļ��һ��ÿ���̷߳���Ϣ֮��ļ��ʱ��Ϊ���٣��Ա��ж������Ƿ�����������
        /// ��ֹͣ��ť����һ��ʱ���Ժ󣬿�ʼ��ť״̬û�б��ı䣬��ô����ֹ��ť���ҳ������Ѿ������߳�
        /// </summary>
        public void CheckMessageTime(Object state)
        {
            if (m_stopTest || m_thread_struct == null)
                return;
            m_CheckMessageTimer.Change(2000000000, 2000000000);
            //����߳������У������߳��ϴ���Ϣ����ǰʱ��ļ��
            DateTime m_now = DateTime.Now;
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_xmlFileName == null || m_thread_struct[i].m_xmlFileName == "" || m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Running)
                    continue;//������߳��Ѿ�ֹͣ���о�����
                TimeSpan m_TimeSpan = m_now.Subtract(m_thread_struct[i].m_DateTime);
                if ((m_TimeSpan.Seconds + m_TimeSpan.Minutes * 60) > 300)
                {
                    AddToTextBoxFail("�߳� " + i + " �Ѿ��������û�в�������Ϣ�ˣ���ȷ��һ�����Ƿ�������", -1);
                }
            }
            m_CheckMessageTimer.Change(ProClass.GetMsgCheckTime() * 1000, ProClass.GetMsgCheckTime() * 1000);
        }

        /// <summary>
        /// //����ʾ�ɹ���Ϣ���ļ��������һ����Ϣ
        /// </summary>
        public void AddToTextBoxSuccess(string m_txt, int m_id, bool m_fail)
        {
            Debug.Assert((m_id < ProClass.GetThreadNum()) && (m_id > -2), "��������Ϣ�����߳�ID�������˹涨��ֵ", "MainForm.AddToTextBoxSuccess");
        
            if (!ProClass.bValIsOutMsg)//bValIsOutMsg�Ƿ����ִ����Ϣ
                return;
            if (m_id != -1)//-1������Ϊ�����̣߳�����ǽ����߳���Ϣ����ô������������Ϣ���ϸ���Ϣ֮��ļ��ʱ��
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
            Monitor.Enter(this);//������
            if (m_message_num++ > ProClass.iValMsgNum)//������Ϣ���ļ���
            {
                SaveMessageToFile();
            }
            Monitor.Exit(this);//�ͷ��� 

			Monitor.Enter(richTextBox_success);//������
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
            Monitor.Exit(richTextBox_success);//�ͷ��� 
        }

        /// <summary>
        /// //����ʾʧ����Ϣ���ļ��������һ����Ϣ
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
            if (ProClass.bValIsOutVoice && iNoVoice)//bValIsOutVoice�Ƿ񱨴�����ʾ
            {
                Voice m_Voice = new Voice();
                m_Voice.CreateVoice();
            }
            richTextBox_fail.AppendText(m_txt);//������
            //	richTextBox_fail.Focus();
            richTextBox_fail.ScrollToCaret();
            Monitor.Exit(richTextBox_fail);//�ͷ��� 
        }
        /// <summary>
        /// //����ʾʧ����Ϣ���ļ��������һ����Ϣ
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
            
            richTextBox_fail.AppendText(m_txt);//������
            //	richTextBox_fail.Focus();
            richTextBox_fail.ScrollToCaret();
            Monitor.Exit(richTextBox_fail);//�ͷ��� 
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
        /// //��쵱ǰѡȡ��xml�ļ��Ƿ��Ѿ�������
        /// </summary>
        public bool CheckNotExistXml(int m_threadIndex, int m_index)
        {
            if (m_threadIndex == -1)
                return true;
            Debug.Assert(m_index < m_XmlFileList.Count && m_XmlFileList.Count >= 0, "ָ�����������ڽڵ����������Խ����", "MainForm.CheckNotExistXml ����");
            //�ڵ�ǰ�����м���Ƿ��Ѿ�����ָ�����ļ�
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
        /// //��������б��еõ�һ����ѡ���Xml�ļ��б�
        /// FirstNode Ҫ���������ڵ�
        /// m_diName  �ڵ�����ȫ·��
        /// m_checked �ڵ��Ƿ�ѡ��
        /// </summary>
        public void SearchCheckNode(TreeNode FirstNode, string m_diName, bool m_checked)
        {
            //�ú���Ϊ��������
            m_diName += "\\";
            while (FirstNode != null)
            {
                if (FirstNode.Checked == true)
                {
                    m_HaveNodeCheck = true;//��ʶ����ʱ���Ƿ��нڵ�ǰ�汻���Ϲ������û�У���ô�ͻ��õ�ǰѡ�У����Ǵ�ѡ�У��Ľڵ�������
                }
                //����ڵ����ļ��У���ô�����ýڵ�
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == 0 || (int)FirstNode.Tag == 3 || (int)FirstNode.Tag == LOGRTFDIRECTORY)))
                {
                    if (FirstNode.Text != "�������ص��ļ�")
                    {
                        SearchCheckNode(FirstNode.FirstNode, m_diName + FirstNode.Text, m_checked | FirstNode.Checked);
                    }
                }
                else if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLFILE || (int)FirstNode.Tag == LOGFILE)) && (FirstNode.Checked || m_checked))
                {
                    //����Ǹýڵ����ļ��ڵ㣬����������ȫ·�����ӵ������б���
                    string m_NodeXmlName = m_diName + FirstNode.Text;
                    m_XmlFileList.Add(m_NodeXmlName);
                    m_XmlSelectNodeList.Add(FirstNode);//��ѡ�еĽڵ㣬�ӵ�������
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        /// <summary>
        /// //�������нڵ㣬����ó���ѡ�ڵ��ڵ�ǰĿ¼�µ����·��
        /// ����m_TreeNodeΪҪ��������һ�����
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
        /// //��������б��еõ�һ����ѡ���Xml�ļ��б�
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
            //�����ǰ�����б��У��ļ�����Ϊ0,��ô��ѡ�õ�ǰ����Ľڵ�����
            if (m_FindSelectNode && m_XmlFileList.Count == 0 && treeView_xml.SelectedNode != null && !m_HaveNodeCheck && treeView_xml.SelectedNode.Checked != true)
            {
                treeView_xml.SelectedNode.Checked = true;
                SearchCheckNode(FirstNode.FirstNode, ProClass.sValXMLFilePath, FirstNode.Checked);
                treeView_xml.SelectedNode.Checked = false;
            }
            if (FirstNode.NextNode != null && m_XmlFileList.Count == 0)//�������Ĳ�����δ���ֱ�ѡ�е�XML�ļ�����ô�͵�LOG��¼�ļ����в�
            {
                m_runLog = true;   //NextNode��һ��ͬ�������
                SearchCheckNode(FirstNode.NextNode, ProClass.sValXMLFilePath, false);
                //�����ǰ�����б��У��ļ�����Ϊ0,��ô��ѡ�õ�ǰ����Ľڵ�����
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
        /// //�������������б��еõ�һ��Xml�ļ���������
        /// </summary>
        public string GetXmlFileName(int m_index)
        {
            Debug.Assert(m_index < m_XmlFileList.Count && m_index >= 0, "ָ�������������ļ�����Խ����", "MainForm.GetXmlFileName ����");
            if (m_index >= m_XmlFileList.Count || m_index < 0)
                return "";
            return (string)m_XmlFileList[m_index];
        }
        /// <summary>
        /// //��������б��еõ�һ��Xml�ļ����б��е�������������
        /// </summary>
        public int GetXmlFileIndex(int m_TheadIndex)
        {

            int m_NIndex = -1;
            if (m_XmlFileList.Count < 1)
            {
                AddToTextBoxFail("���ж�����XML�ļ���Ϊ0����ѡ��Ҫ���е�XML�ļ�", -1);
                m_stopTest = true;
                return m_NIndex;
            }
            Monitor.Enter(oGetXmlQueue);

            //��������ѡȡ������
            if (ProClass.bValIsRandRun && ProClass.GetIsLoopRun())
            {
                int m_loop = 10;
                Random m_Random = new Random();
                while ((m_loop--) > 0)
                {
                    m_NIndex = m_Random.Next(0, m_XmlFileList.Count);//�õ�һ�����ļ�����֮�ڵ������
                    Debug.Assert(m_NIndex < m_XmlFileList.Count && m_NIndex >= 0, "ָ�������������ļ�����Խ����", "MainForm.GetXmlFileIndex ����");
                    if (m_NIndex >= m_XmlFileList.Count)
                    {
                        m_NIndex = -1;
                    }
                    else
                    {
                        if (CheckNotExistXml(m_TheadIndex, m_NIndex))//�����ļ��Ƿ��Ѿ�������
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
            else//�����˳��ѡȡ����
            {
                {
                    m_NIndex = m_TreeNodeIndex;
                    if (!CheckNotExistXml(m_TheadIndex, m_NIndex))//�����ļ��Ƿ��Ѿ�������
                    {
                        m_NIndex = -1;
                    }
                    else
                    {
                        m_TreeNodeIndex++;//��ǰ�����ļ����б��е������Լ�һ
                        if (m_TreeNodeIndex >= m_XmlFileList.Count)
                        {
                            m_TreeNodeIndex = 0;//����Ϊ0
                            if (!ProClass.GetIsLoopRun())//���������ѭ������
                                m_stopTest = true;//��ôֹͣ��ǰ�Ĳ���
                        }
                    }
                }
            }
            Monitor.Exit(oGetXmlQueue);
            return m_NIndex;
        }

        /// <summary>
        /// //�������Թ����߳�
        /// </summary>
        private void CreateThread()
        {
            if (m_thread_struct == null)//����ָ�����̸߳�����������Ӧ��С������
                m_thread_struct = new THREAD_STRUCT[ProClass.GetThreadNum()];
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                m_thread_struct[i].m_xmlIndex = -1;
            }
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_TestThread == null)
                {
                    m_thread_struct[i].m_TestThread = new TestThread();	//����һ�������̶߳���
                    m_thread_struct[i].m_TestThread.SetForm1(this);		//�ѵ�ǰ���ڶ����ָ�������ȥ
                    m_thread_struct[i].m_TestThread.SetThreadIndex(i);	//�����������������е�ID				
                }
                m_thread_struct[i].m_DateTime = DateTime.Now;
                m_thread_struct[i].m_thread = new Thread(new ThreadStart(m_thread_struct[i].m_TestThread.Run));//����һ���߳�ί�������в��Ժ���
                m_thread_struct[i].m_thread.Priority = ThreadPriority.BelowNormal;
                m_thread_struct[i].m_thread.Start();	//��ʼ			
            }
        }
#if DM6
        /// <summary>
        /// //��������������ť(��ʼ��ֹͣ����ֹ)��״̬
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
        /// //��������������ť(��ʼ��ֹͣ����ֹ)��״̬
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
            SaveFile(Environment.CurrentDirectory + "\\���Խ��\\Default.lst");
            if (m_thread_struct == null)
            {
                return;
            }
            for (int i = 0; i < ProClass.GetThreadNum(); i++)
            {
                if (m_thread_struct[i].m_xmlFileName == null || m_thread_struct[i].m_xmlFileName == "" || m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Running)
                    continue;//����߳��Ѿ��Զ���ֹͣ��������
                else//����̻߳�������
                {
                    AddToTextBoxSuccess("�ȴ�1�룬���߳� " + i + " �Զ�ֹͣ", -1, false);
                    Thread.Sleep(1000);//��1��
                    try
                    {
                        m_thread_struct[i].m_thread.Abort();
                        m_thread_struct[i].m_thread.Join(100);
                        if (m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Stopped && m_thread_struct[i].m_thread.ThreadState != System.Threading.ThreadState.Aborted)
                        {
                            AddToTextBoxFail("�߳� " + i + " ��ǿ����ֹ�����߳����еĲ����Ѿ�ʧ�ܣ���鿴��Ϣ��¼", -1);
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
            m_MonuseNode = treeView_xml.GetNodeAt(e.X, e.Y);		//�õ����λ���ϵĽ�����
            if (m_MonuseNode != null)
                treeView_xml.SelectedNode = m_MonuseNode;					//����Ϊѡ�ж���
            else
            {
                treeView_xml.ContextMenu.MenuItems.Clear();
                treeView_xml.ContextMenu.MenuItems.Add(m_treeReflesh);	//����ڿհ׵ĵط�����ģ���ô����ָ��ˢ�²˵�
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
        /// �ñ������ķ������ú�ɫ�����ʶ������ʧ�ܵ�����
        /// </summary>
        public int SearchFailXmlNode(TreeNode m_TreeNode)
        {
            int NO_RUN = 0;
            int FAIL_RUN = 1;
            int SUCCESS_RUN = 2;
            int m_haveFailNode = NO_RUN;
            int ret = NO_RUN;

            //�ú���Ϊ��������
            while (m_TreeNode != null)
            {
                //����ڵ����ļ��У���ô�����ýڵ�
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
        /// �ú�ɫ�����ʶ������ʧ�ܵ�����
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
                        AddToTextBoxFail("\n������ͳ����Ϣ��", -1);
                        ProClass.bValIsOutVoice = m_tempVoice;
                    }
                    m_FailNum++;
                    AddToTextBoxFail(((TreeNode)m_XmlSelectNodeList[i]).FullPath.Substring(("��������").Length) + "		ִ��ʧ�ܣ�", -1);
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
                AddToTextBoxFail("�ܹ��� " + m_FailNum + " �������ļ�ִ��ʧ�ܣ�", -1);
                ProClass.bValIsOutVoice = m_tempVoice;
            }
            if (m_XmlSelectNodeList.Count > 0)
                AddToTextBoxSuccess("\n\n��������������" + m_XmlSelectNodeList.Count + "�� ִ�гɹ�������" + m_SuccessNum + "�� δִ�и�����" + (m_XmlSelectNodeList.Count - m_SuccessNum - m_FailNum) +
                    "�� ִ��ʧ�ܸ�����" + m_FailNum + "		�ɹ��ʣ�" + ((100 * m_SuccessNum) / m_XmlSelectNodeList.Count) + "%", -1, false);

        }

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("�ù�����Ҫ���������а���SQL����XML�ű�����Ҫ��������ݿ�����������ԡ�\n����Թ��������⣬����XMLģ���ʽ�в����׵ĵط��������ʼ�����QQ��ϵ��\nE_MAIL: happysunfeng@163.com, QQ: 29867706 ", "���߽���", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void treeView_xml_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            e.Node.SelectedImageIndex = e.Node.ImageIndex;
            if (((int)(e.Node.Tag) != RTFFILE && (int)(e.Node.Tag) != NONE) || !button_start.Enabled)//2���������м�¼�ļ�
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
                    m_path1 = e.Node.Parent.FullPath.Substring("��������".Length);
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
                //����ֱ��Ǵӱ�����ļ���ȡ����
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
            if ((int)(e.Node.Tag) == RTFFILE || (int)(e.Node.Tag) == NONE || ((int)(e.Node.Tag) == RTFDIRECTORY))//2���������м�¼�ļ�
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
            m_OpenFileDialog.Filter = "�б��ļ�(*.lst)|*.lst";
            m_OpenFileDialog.RestoreDirectory = true;
            m_OpenFileDialog.Title = "���뱣��Ľڵ�״̬";
            m_OpenFileDialog.ShowDialog();
            OpenFile(m_OpenFileDialog.FileName);
        }
        /// <summary>
        /// ���ýڵ�״̬
        /// </summary>
        public void SetNode(TreeNode FirstNode, ArrayList m_FileList, IComparer myComparerSr)
        {
            //�ú���Ϊ��������
            while (FirstNode != null)
            {
                //����ڵ����ļ��У���ô�����ýڵ�
                if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLDIRECTORY)
                {
                    SetNode(FirstNode.FirstNode, m_FileList, myComparerSr);
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    //����Ǹýڵ����ļ��ڵ㣬����������ȫ·�����ӵ������б���
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
        /// ��ȡ����Ľڵ�״̬��Ϣ
        /// </summary>
        private void OpenFile(string m_path)
        {
            if (m_path == "")
                return;
            if (m_path.Length > 256)
            {
                AddToTextBoxFail("�򿪵��ļ����Ƿ��������ļ�·�����ȳ�����256���ַ�����������ȷ���ļ���", -1);
                return;
            }
            ArrayList m_FileList = null;
            StreamReader sr = null;
            //����ֱ��Ǵӱ���������ļ��У���ȡֵ����ʼ��������Щ����
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
        //�������øýڵ��Լ��ýڵ���ӽڵ�Ϊѡ��״̬
        private void SetChildCheck(TreeNode FirstNode)
        {
            if (FirstNode == null || FirstNode.Tag == null || ((int)FirstNode.Tag != XMLDIRECTORY && (int)FirstNode.Tag != XMLFILE))
                return;
            while (FirstNode != null)
            {
                FirstNode.Checked = true;
                if (((int)FirstNode.Tag == XMLDIRECTORY))//������ļ��У���ô�ӽڵ�ҲҪ����Ϊѡ��
                {
                    SetChildCheck(FirstNode.FirstNode);
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        //�������øýڵ��Լ��ýڵ���ӽڵ�Ϊ��ѡ��״̬
        private void SetChildUnCheck(TreeNode FirstNode)
        {
            if (FirstNode == null || FirstNode.Tag == null || ((int)FirstNode.Tag != XMLDIRECTORY && (int)FirstNode.Tag != XMLFILE))
                return;
            while (FirstNode != null)
            {
                FirstNode.Checked = false;
                if (((int)FirstNode.Tag == XMLDIRECTORY))//������ļ��У���ô�ӽڵ�ҲҪ����Ϊѡ��
                {
                    SetChildUnCheck(FirstNode.FirstNode);
                }
                FirstNode = FirstNode.NextNode;
            }
        }
        //����ȡ���ýڵ�ĸ��ڵ�ѡ��״̬
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
                AddToTextBoxFail("ĳ�ļ���·������Ѿ�������256���ַ�", -1);
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
                //����ڵ����ļ��У���ô�����ýڵ�
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
                AddToTextBoxFail("ĳ�ļ���·������Ѿ�������256���ַ�", -1);
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
                //����ڵ����ļ��У���ô�����ýڵ�
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
        //�ú��������ƶ���XML�ļ������EXE��RAR�ļ�
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
                m_di = new DirectoryInfo(ProClass.sValXMLFilePath + "\\�ɹ�����");
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

            AddToTextBoxSuccess("��ʼ�������гɹ���XML�ű��ļ�", -1, false);
            TreeNode FirstNode = m_TopNode.FirstNode;
            while (FirstNode != null)
            {
                TreeNode TempNode = FirstNode.NextNode;
                //����ڵ����ļ��У���ô�����ýڵ�
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLDIRECTORY)))
                {
                    if (FirstNode.Text.CompareTo("�ɹ�����") == 0)
                    {
                    }
                    else if (FirstNode.Text.CompareTo("ʧ�ܲ���") == 0)
                    {
                        SeparateSuccessXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, Environment.CurrentDirectory + "\\�ɹ�����\\");
                    }
                    else
                    {
                        SeparateSuccessXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, Environment.CurrentDirectory + "\\�ɹ�����\\" + FirstNode.Text);
                    }
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    if (FirstNode.ImageIndex == 9)
                    {
                        try
                        {
                            File.Copy(ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\�ɹ�����\\" + FirstNode.Text, true);
                            CopyAppendXmlFile(ProClass.sValXMLFilePath + "\\", ProClass.sValXMLFilePath + "\\�ɹ�����\\", FirstNode.Text.Substring(0, FirstNode.Text.Length - ".xml".Length));
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
            AddToTextBoxSuccess("�������", -1, false);

        }

        private void cMenuItemSpFa_Click(object sender, System.EventArgs e)
        {
            DirectoryInfo m_di = null;
            try
            {
                m_di = new DirectoryInfo(ProClass.sValXMLFilePath + "\\ʧ�ܲ���");
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

            AddToTextBoxSuccess("��ʼ��������ʧ�ܵ�XML�ű��ļ�", -1, false);
            TreeNode FirstNode = m_TopNode.FirstNode;
            while (FirstNode != null)
            {
                TreeNode TempNode = FirstNode.NextNode;
                //����ڵ����ļ��У���ô�����ýڵ�
                if ((FirstNode.Tag != null && ((int)FirstNode.Tag == XMLDIRECTORY)))
                {
                    if (FirstNode.Text.CompareTo("ʧ�ܲ���") == 0)
                    {
                    }
                    else if (FirstNode.Text.CompareTo("�ɹ�����") == 0)
                    {
                        SeparateFailXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\ʧ�ܲ���\\");
                    }
                    else
                    {
                        SeparateFailXmlFile(FirstNode.FirstNode, ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\ʧ�ܲ���\\" + FirstNode.Text);
                    }
                }
                else if (FirstNode.Tag != null && (int)FirstNode.Tag == XMLFILE)
                {
                    if (FirstNode.ImageIndex == 9)
                    {
                        try
                        {
                            File.Copy(ProClass.sValXMLFilePath + "\\" + FirstNode.Text, ProClass.sValXMLFilePath + "\\ʧ�ܲ���\\" + FirstNode.Text, true);
                            CopyAppendXmlFile(ProClass.sValXMLFilePath + "\\", ProClass.sValXMLFilePath + "\\ʧ�ܲ���\\", FirstNode.Text.Substring(0, FirstNode.Text.Length - ".xml".Length));
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
            AddToTextBoxSuccess("�������", -1, false);
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
        private void CreateNewDir(string strDir, string strDirName)//�½�Ŀ¼
        {
            try
            {

                DirectoryInfo myDirInfo = new DirectoryInfo(strDir);
                myDirInfo.CreateSubdirectory(strDirName);
                //MessageBox.Show("��"+strDir+"����Ŀ¼�ɹ�����","����Ŀ¼�ɹ�");
            }
            catch
            {
                //ShowFMessage("�����Ŀ¼��ʽ,����Ŀ¼ʧ�ܣ���");
                AddToTextBoxFail("�����Ŀ¼��ʽ,����Ŀ¼ʧ�ܣ���", -1);

            }


        }
        /*private void CreateNewFile(string strDir, string strFilename)//�½��ļ�
        {
            try
            {
                FileInfo myFileInfo = new FileInfo(strDir + strFilename);
                FileStream myFileStream = myFileInfo.Create();
				
                //MessageBox.Show("��"+strDir+"�����ļ��ɹ�����","�����ļ�chenggong");
                myFileStream.Close();
            }
            catch
            {
                ShowFMessage("������ļ���ʽ,�����ļ�ʧ�ܣ���");

            }
        }*/
        private void button_Start_WF_Click(object sender, System.EventArgs e)
        {
            mytestarr.Clear();//���������Խ����Ϣ��mytestarr���飬��֤mytestarr�����Ԫ�ض��ǵ������ʼ������Խ����Ϣ��֮��Ĳ��Խ����Ϣ
            DateTime tm = DateTime.Now;//��õ�ǰ��ʱ��
            string timeDate = tm.ToLongDateString();//��õ�ǰ���ꡢ�¡���ʱ��

            string time = tm.ToLongTimeString();
            time = time.Replace(":", "_");//�ѡ������á�-������
            time = timeDate + "\\" + time + ".txt";//��õ�ǰ�ꡢ�¡��ա�ʱ���֡�����ַ�����ʾ��ʽ��Ϊ���潨���Ե�ǰ��ʱ��Ϊ���ġ�.txt�����ļ���׼��
            string path = Environment.CurrentDirectory + "\\���Խ��";//���ҪҪ����Ŀ¼���ڵ���ȫ·��
            path += "\\";
            CreateNewDir(path, timeDate);//����һ����ǰ��ʱ��Ϊ�����ļ���
            //CreateNewFile(path,time);
            w_path = path + time;//��Ҫ�����ļ���·�������ڴ�ȫ�ֱ����У�Ϊ���潨�����ļ���׼��
        }

        private void button_End_RSF_Click(object sender, System.EventArgs e)
        {
            FileInfo myfile = new FileInfo(w_path);
            StreamWriter sw = myfile.CreateText();
            DateTime tm = DateTime.Now;
            string time_end = tm.ToLongTimeString();//��õ�ǰʱ�䣬
            for (int m = mytestarr.Count - 1; m >= 0; m--)//��Ҫ����ʱ��ıȽϣ�Ϊ��ȷ����mytestarr�е�Ԫ�ض��ǵ��������������Խ����Ϣ��֮ǰ�ı���Ĳ��Խ����Ϣ
            {
                if (((((RWArr)mytestarr[m]).TestDateTime).CompareTo(time_end)) > 0)//��ʱ����ڵ��������������Խ����Ϣ��ʱ��mytestarr�е�Ԫ�ظ�ɾ����
                    mytestarr.RemoveAt(m);
                else
                    break;
            }

            for (int j = 0, n = 0; j < mytestarr.Count; j = j + n)//��Ҫ�������򣬰��ļ�����ͬ���ļ�������һ����Ҫ��Ϊ�˱��ڱȽ�
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
            for (int i = 0; i < mytestarr.Count; i++)//������֮���mytestarr�е�Ԫ��д���Կ�ʼ�������ʼ������Խ����Ϣ����ʱ��Ϊ���������ġ�.txt���ļ���
            {
                sw.Write(i + "\t");//�������
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
		/// �����߳���
		/// </summary>
		public class TestThread   //����TestThread�� 	���������߳� 	ѭ��ִ��xml�ļ� һ�������߳��൱��һ�ι��ߵ�ִ�й��̡�
		{
			[DllImport("kernel32.dll")]  
			private  static  extern  UInt32  GetCurrentThreadId(); 
			private  static  Queue oQueue=new Queue();
			private string m_XmlFileName;	//����ļ���������·��
			private int	m_threadIndex;      //�߳�������
			private MainForm m_MainForm;
			private string m_FileName;		//����ļ���
			private UInt32 m_ThreadId;	//��ŵ�ǰ�̵߳ľ��
			XmlTest m_xmlTest;			//��ǰ���Ե�XML�ļ�����
			public bool m_AleadyStop;	//��ʾ�ö����Ѿ���������

			public TestThread()
			{
				m_threadIndex = -1;
			}
			/// <summary>
			/// ���õ�ǰ���ڶ������������
			/// </summary>
			public void SetForm1(MainForm m_f1)
			{
				m_MainForm = m_f1;
			}
			/// <summary>
			/// ���õ�ǰ�߳��ڹ��������е�ID
			/// </summary>
			public void SetThreadIndex(int m_id)
			{
				m_threadIndex = m_id;
			}
			/// <summary>
			/// �������е�SQL��䱣����ļ�
			/// </summary>
			public void SaveSqlCase()
			{
				m_MainForm.SaveSqlCase();
			}
			/// <summary>
			/// ��SQL����ѹ��һ����Ϣ
			/// </summary>
			public void PushSqlCase(string m_sql)
			{
				m_MainForm.PushSqlCase(m_sql);
			}
			/// <summary>
			/// ����ɹ���Ϣ����
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
			/// ���ʧ����Ϣ����
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
			/// �����̵߳ľ��
			/// </summary>
			public UInt32 GetThreadID()
			{
				return	m_ThreadId;
			}
			/// <summary>
			/// �߳����к���IntPtr 
			/// </summary>
			public void Run()
			{
				m_AleadyStop = false;
				Debug.Assert(m_threadIndex>=0, "�߳��ڹ��������е�IDû�б�ָ��", "TestThread.Run ����");
				Debug.Assert(m_MainForm!=null, "�߳���m_MainForm����û�б�����ֵ", "TestThread.Run ����");
				if(m_threadIndex < 0 || m_threadIndex >= ProClass.GetThreadNum())
				{
					ShowFMessage("�Ƿ����߳�ID��" + m_threadIndex);
					return;
				}
				if(m_MainForm == null)
				{
					ShowFMessage("��Ч�Ĵ���ָ��");
					return;
				}
				m_ThreadId = GetCurrentThreadId();//�õ���ǰ�̵߳�ID
				try
				{
					while(!m_MainForm.m_stopTest)
					{
						m_XmlFileName = m_MainForm.GetXmlFileName(m_MainForm.GetXmlFileIndex(m_threadIndex));				//��ȡ��һ�������ļ�
						if(m_XmlFileName == "")
						{
							//		Debug.Assert(m_XmlFileName!="", "�õ���XML�ļ�������Ϊ�մ�", "TestThread.Run ����");
							System.Threading.Thread.Sleep(0);
							continue;
						}
						m_FileName = GetSingleFileName();						//���ļ�������·���еõ��ļ���

						SetXmlFileNameToArray(m_XmlFileName);					//�ѵ�ǰ���е��ļ������õ��̹߳�������
						m_xmlTest = new XmlTest(this);							//����һ�������ļ�����
						m_xmlTest.xmlFileName = m_FileName;
             //          ShowFMessage("��ǰ�����ļ�����" + m_FileName);
                    //        m_MainForm.AddToTextBoxSuccess(, -1, false);
                        m_xmlTest.SetXmlFile(m_XmlFileName);
						if(m_MainForm.m_runLog)
							m_xmlTest.RunLog();
						else
							m_xmlTest.Run(false);										//���в����ļ�����
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
						SetXmlFileNameToArray("");								//�ѵ�ǰ���е��ļ���"�մ�"���õ��̹߳�������
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
					ShowFMessage("�̱߳��������жϣ���������Ѿ�ʧ�ܣ�������Ϣ��¼");				
				}
				finally
				{
					CheckThreadStop();
				}
			}
			/// <summary>
			/// �߳����н��������ð�ť��״̬
			/// </summary>
			private void CheckThreadStop()
			{			
				Monitor.Enter(oQueue);			 
			{
				m_AleadyStop = true;
				bool m_EnableStartButton = true;
				for(int i=0; i<ProClass.GetThreadNum(); i++)
				{
					if(!m_MainForm.m_thread_struct[i].m_TestThread.m_AleadyStop)//��������̶߳��Ѿ�ֹͣ�����У���ô�����ÿ�ʼ��ťΪ���õ�״̫
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
				Monitor.Exit(oQueue);//�ͷ���
			}
			/// <summary>
			/// �ѵ�ǰ�߳����е�XML�ļ���Ϣд���̹߳���������
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
			/// ������·���еõ��ļ���������
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
			/// ����ǿ�жϿ����Զ��������
			/// </summary>
			public void DisConnect()
			{
				if(m_xmlTest != null)
					m_xmlTest.DisConnect(-1);//�Ͽ������ļ�������
				SetXmlFileNameToArray("");
			}
			/// <summary>
			/// �������XML�ļ������л���
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
						Debug.Assert(m_XmlFileName!="", "�õ���XML�ļ�������Ϊ�մ�", "TestThread.RunClearEnvironment ����");
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
		/// ִ���������������ִ�й������
		/// </summary>
		public class SQL_CASE
		{
			private string m_sql;	//������
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
			public SQL_CASE pNext;	//ָ����һ��������
		}
		class  Voice  //������������
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
