using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace Auto_test_db
{
	/// <summary>
	/// Form_Set 的摘要说明。
	/// </summary>
	public class Form_Set : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox_S;
		private System.Windows.Forms.TextBox textBox_D;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox_U;
		private System.Windows.Forms.TextBox textBox_P;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBox_M_N;
		private System.Windows.Forms.TextBox textBox_M_T;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox checkBox_Show_Message;
		private System.Windows.Forms.CheckBox checkBox_Save_Message;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox checkBox_Random;
		private System.Windows.Forms.CheckBox checkBox_Loop;
		private System.Windows.Forms.Button button_Ok;
		private System.Windows.Forms.Button button_Cancle;
		private System.Windows.Forms.TextBox textBox_T_N;
		private System.Windows.Forms.CheckBox checkBox_AC;
		private System.Windows.Forms.Button button_defalut;
		private System.Windows.Forms.CheckBox checkBox_W;
		private System.Windows.Forms.CheckBox checkBox_Ag;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox comboBox_Prv;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox checkBox_SaveSql;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox textBox_SqlNum;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TextBox cTBCheckServerTime;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.CheckBox cCBAutoRunCheck;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.RadioButton radioButtonWin;
		private System.Windows.Forms.RadioButton radioButtonLin;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.TextBox textBoxS0;
		private System.Windows.Forms.TextBox textBoxS1;
		private System.Windows.Forms.TextBox textBoxS2;
		private System.Windows.Forms.TextBox textBoxS3;
		private System.Windows.Forms.TextBox textBoxS4;
		private System.Windows.Forms.TextBox textBoxS5;
		private System.Windows.Forms.TextBox textBoxS6;
		private System.Windows.Forms.TextBox textBoxS7;
		private System.Windows.Forms.TextBox textBoxS8;
		private System.Windows.Forms.TextBox textBoxS9;
		private System.Windows.Forms.CheckBox checkBox_interval;
		private System.Windows.Forms.CheckBox checkBox_time;
		private ProClass proVal;
		private System.Windows.Forms.CheckBox checkBox_showResult;
		private System.Windows.Forms.TextBox textBoxLevel;

		private System.Windows.Forms.Button button_level;
		//private System.Windows.Forms.Button cBTLookUp2;
		private System.Windows.Forms.TextBox cTBXMLFilePath;
		private System.Windows.Forms.Label lab123;
		private System.Windows.Forms.Button cBTLookUp2;
		private System.Windows.Forms.CheckBox checkBoxPool;
		private System.Windows.Forms.Label label5;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form_Set()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();
			this.AcceptButton = button_Ok;
			this.CancelButton = button_Cancle;
			proVal = new ProClass(null);
		}

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form_Set));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBox_P = new System.Windows.Forms.TextBox();
			this.textBox_U = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox_D = new System.Windows.Forms.TextBox();
			this.textBox_S = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.checkBox_time = new System.Windows.Forms.CheckBox();
			this.checkBox_interval = new System.Windows.Forms.CheckBox();
			this.textBox_M_T = new System.Windows.Forms.TextBox();
			this.textBox_M_N = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.checkBox_Save_Message = new System.Windows.Forms.CheckBox();
			this.checkBox_Show_Message = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.checkBox_Ag = new System.Windows.Forms.CheckBox();
			this.checkBox_W = new System.Windows.Forms.CheckBox();
			this.checkBox_AC = new System.Windows.Forms.CheckBox();
			this.checkBox_Loop = new System.Windows.Forms.CheckBox();
			this.checkBox_Random = new System.Windows.Forms.CheckBox();
			this.textBox_T_N = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.button_Ok = new System.Windows.Forms.Button();
			this.button_Cancle = new System.Windows.Forms.Button();
			this.button_defalut = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.comboBox_Prv = new System.Windows.Forms.ComboBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.textBox_SqlNum = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.checkBox_SaveSql = new System.Windows.Forms.CheckBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.cBTLookUp2 = new System.Windows.Forms.Button();
			this.lab123 = new System.Windows.Forms.Label();
			this.cTBXMLFilePath = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.cTBCheckServerTime = new System.Windows.Forms.TextBox();
			this.cCBAutoRunCheck = new System.Windows.Forms.CheckBox();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.radioButtonLin = new System.Windows.Forms.RadioButton();
			this.radioButtonWin = new System.Windows.Forms.RadioButton();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.textBoxS9 = new System.Windows.Forms.TextBox();
			this.label22 = new System.Windows.Forms.Label();
			this.textBoxS8 = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.textBoxS7 = new System.Windows.Forms.TextBox();
			this.label20 = new System.Windows.Forms.Label();
			this.textBoxS6 = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.textBoxS5 = new System.Windows.Forms.TextBox();
			this.label18 = new System.Windows.Forms.Label();
			this.textBoxS4 = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.textBoxS3 = new System.Windows.Forms.TextBox();
			this.label16 = new System.Windows.Forms.Label();
			this.textBoxS2 = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.textBoxS1 = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.textBoxS0 = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.checkBox_showResult = new System.Windows.Forms.CheckBox();
			this.button_level = new System.Windows.Forms.Button();
			this.textBoxLevel = new System.Windows.Forms.TextBox();
			this.checkBoxPool = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.textBox_P);
			this.groupBox1.Controls.Add(this.textBox_U);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.textBox_D);
			this.groupBox1.Controls.Add(this.textBox_S);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(336, 88);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "登录信息";
			// 
			// textBox_P
			// 
			this.textBox_P.Location = new System.Drawing.Point(224, 56);
			this.textBox_P.Name = "textBox_P";
			this.textBox_P.Size = new System.Drawing.Size(104, 21);
			this.textBox_P.TabIndex = 7;
			this.textBox_P.Text = "textBox1";
			// 
			// textBox_U
			// 
			this.textBox_U.Location = new System.Drawing.Point(224, 24);
			this.textBox_U.Name = "textBox_U";
			this.textBox_U.Size = new System.Drawing.Size(104, 21);
			this.textBox_U.TabIndex = 6;
			this.textBox_U.Text = "textBox1";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(176, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 24);
			this.label4.TabIndex = 5;
			this.label4.Text = "用户：";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(176, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 24);
			this.label3.TabIndex = 4;
			this.label3.Text = "口令：";
			// 
			// textBox_D
			// 
			this.textBox_D.Location = new System.Drawing.Point(64, 56);
			this.textBox_D.Name = "textBox_D";
			this.textBox_D.Size = new System.Drawing.Size(104, 21);
			this.textBox_D.TabIndex = 3;
			this.textBox_D.Text = "";
			// 
			// textBox_S
			// 
			this.textBox_S.Location = new System.Drawing.Point(64, 24);
			this.textBox_S.Name = "textBox_S";
			this.textBox_S.Size = new System.Drawing.Size(104, 21);
			this.textBox_S.TabIndex = 2;
			this.textBox_S.Text = "localhost:12345";
			this.textBox_S.Leave += new System.EventHandler(this.textBox_S_Leave);
			this.textBox_S.Enter += new System.EventHandler(this.textBox_S_Enter);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 24);
			this.label2.TabIndex = 1;
			this.label2.Text = "初始库：";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "服务器：";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.checkBox_time);
			this.groupBox2.Controls.Add(this.checkBox_interval);
			this.groupBox2.Controls.Add(this.textBox_M_T);
			this.groupBox2.Controls.Add(this.textBox_M_N);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.checkBox_Save_Message);
			this.groupBox2.Controls.Add(this.checkBox_Show_Message);
			this.groupBox2.Location = new System.Drawing.Point(8, 104);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(336, 120);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "消息设置";
			// 
			// checkBox_time
			// 
			this.checkBox_time.Location = new System.Drawing.Point(168, 88);
			this.checkBox_time.Name = "checkBox_time";
			this.checkBox_time.Size = new System.Drawing.Size(120, 24);
			this.checkBox_time.TabIndex = 7;
			this.checkBox_time.Text = "输出执行时间";
			// 
			// checkBox_interval
			// 
			this.checkBox_interval.Location = new System.Drawing.Point(8, 88);
			this.checkBox_interval.Name = "checkBox_interval";
			this.checkBox_interval.Size = new System.Drawing.Size(120, 24);
			this.checkBox_interval.TabIndex = 6;
			this.checkBox_interval.Text = "消息间隔";
			// 
			// textBox_M_T
			// 
			this.textBox_M_T.Location = new System.Drawing.Point(104, 56);
			this.textBox_M_T.Name = "textBox_M_T";
			this.textBox_M_T.Size = new System.Drawing.Size(56, 21);
			this.textBox_M_T.TabIndex = 5;
			this.textBox_M_T.Text = "textBox1";
			// 
			// textBox_M_N
			// 
			this.textBox_M_N.Location = new System.Drawing.Point(104, 24);
			this.textBox_M_N.Name = "textBox_M_N";
			this.textBox_M_N.Size = new System.Drawing.Size(56, 21);
			this.textBox_M_N.TabIndex = 4;
			this.textBox_M_N.Text = "textBox1";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 56);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(96, 24);
			this.label7.TabIndex = 3;
			this.label7.Text = "消息检察间隔：";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 24);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 24);
			this.label6.TabIndex = 2;
			this.label6.Text = "保存的消息数：";
			// 
			// checkBox_Save_Message
			// 
			this.checkBox_Save_Message.Location = new System.Drawing.Point(168, 56);
			this.checkBox_Save_Message.Name = "checkBox_Save_Message";
			this.checkBox_Save_Message.Size = new System.Drawing.Size(160, 24);
			this.checkBox_Save_Message.TabIndex = 1;
			this.checkBox_Save_Message.Text = "是否自动保存消息到文件";
			// 
			// checkBox_Show_Message
			// 
			this.checkBox_Show_Message.Location = new System.Drawing.Point(168, 24);
			this.checkBox_Show_Message.Name = "checkBox_Show_Message";
			this.checkBox_Show_Message.Size = new System.Drawing.Size(152, 24);
			this.checkBox_Show_Message.TabIndex = 0;
			this.checkBox_Show_Message.Text = "是否显示测试输出消息";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.checkBox_Ag);
			this.groupBox3.Controls.Add(this.checkBox_W);
			this.groupBox3.Controls.Add(this.checkBox_AC);
			this.groupBox3.Controls.Add(this.checkBox_Loop);
			this.groupBox3.Controls.Add(this.checkBox_Random);
			this.groupBox3.Controls.Add(this.textBox_T_N);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Location = new System.Drawing.Point(8, 232);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(336, 120);
			this.groupBox3.TabIndex = 4;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "运行设置";
			// 
			// checkBox_Ag
			// 
			this.checkBox_Ag.Location = new System.Drawing.Point(168, 88);
			this.checkBox_Ag.Name = "checkBox_Ag";
			this.checkBox_Ag.Size = new System.Drawing.Size(160, 24);
			this.checkBox_Ag.TabIndex = 6;
			this.checkBox_Ag.Text = "发生错误后继续往下执行";
			// 
			// checkBox_W
			// 
			this.checkBox_W.Location = new System.Drawing.Point(8, 88);
			this.checkBox_W.Name = "checkBox_W";
			this.checkBox_W.Size = new System.Drawing.Size(152, 24);
			this.checkBox_W.TabIndex = 5;
			this.checkBox_W.Text = "产生错误时，发声提醒";
			// 
			// checkBox_AC
			// 
			this.checkBox_AC.Location = new System.Drawing.Point(8, 56);
			this.checkBox_AC.Name = "checkBox_AC";
			this.checkBox_AC.Size = new System.Drawing.Size(112, 24);
			this.checkBox_AC.TabIndex = 4;
			this.checkBox_AC.Text = "自动清空消息区";
			// 
			// checkBox_Loop
			// 
			this.checkBox_Loop.Location = new System.Drawing.Point(168, 56);
			this.checkBox_Loop.Name = "checkBox_Loop";
			this.checkBox_Loop.Size = new System.Drawing.Size(160, 24);
			this.checkBox_Loop.TabIndex = 3;
			this.checkBox_Loop.Text = "无限循环运行测试";
			// 
			// checkBox_Random
			// 
			this.checkBox_Random.Location = new System.Drawing.Point(168, 24);
			this.checkBox_Random.Name = "checkBox_Random";
			this.checkBox_Random.Size = new System.Drawing.Size(160, 24);
			this.checkBox_Random.TabIndex = 2;
			this.checkBox_Random.Text = "随机选取测试用例";
			// 
			// textBox_T_N
			// 
			this.textBox_T_N.Location = new System.Drawing.Point(80, 24);
			this.textBox_T_N.Name = "textBox_T_N";
			this.textBox_T_N.Size = new System.Drawing.Size(80, 21);
			this.textBox_T_N.TabIndex = 1;
			this.textBox_T_N.Text = "textBox1";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 24);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(72, 24);
			this.label9.TabIndex = 0;
			this.label9.Text = "线程个数：";
			// 
			// button_Ok
			// 
			this.button_Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Ok.Location = new System.Drawing.Point(448, 472);
			this.button_Ok.Name = "button_Ok";
			this.button_Ok.Size = new System.Drawing.Size(56, 24);
			this.button_Ok.TabIndex = 5;
			this.button_Ok.Text = "确定(&O)";
			this.button_Ok.Click += new System.EventHandler(this.button_Ok_Click);
			// 
			// button_Cancle
			// 
			this.button_Cancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancle.Location = new System.Drawing.Point(520, 472);
			this.button_Cancle.Name = "button_Cancle";
			this.button_Cancle.Size = new System.Drawing.Size(56, 24);
			this.button_Cancle.TabIndex = 6;
			this.button_Cancle.Text = "取消(&C)";
			this.button_Cancle.Click += new System.EventHandler(this.button_Cancle_Click);
			// 
			// button_defalut
			// 
			this.button_defalut.Location = new System.Drawing.Point(352, 472);
			this.button_defalut.Name = "button_defalut";
			this.button_defalut.Size = new System.Drawing.Size(72, 24);
			this.button_defalut.TabIndex = 7;
			this.button_defalut.Text = "默认值(&D)";
			this.button_defalut.Click += new System.EventHandler(this.button_defalut_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(352, 360);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(64, 16);
			this.label10.TabIndex = 8;
			this.label10.Text = "OLEDB驱动";
			// 
			// comboBox_Prv
			// 
			this.comboBox_Prv.Items.AddRange(new object[] {
															  "DOLEDB",
															  "DMOLEDB.1",
															  "SQLOLEDB.1",
															  "OraOLEDB.Oracle"});
			this.comboBox_Prv.Location = new System.Drawing.Point(424, 360);
			this.comboBox_Prv.Name = "comboBox_Prv";
			this.comboBox_Prv.Size = new System.Drawing.Size(72, 20);
			this.comboBox_Prv.TabIndex = 9;
			this.comboBox_Prv.Text = "DOLEDB";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.textBox_SqlNum);
			this.groupBox4.Controls.Add(this.label11);
			this.groupBox4.Controls.Add(this.checkBox_SaveSql);
			this.groupBox4.Location = new System.Drawing.Point(8, 360);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(336, 48);
			this.groupBox4.TabIndex = 10;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "高级选项";
			// 
			// textBox_SqlNum
			// 
			this.textBox_SqlNum.Location = new System.Drawing.Point(248, 16);
			this.textBox_SqlNum.Name = "textBox_SqlNum";
			this.textBox_SqlNum.Size = new System.Drawing.Size(80, 21);
			this.textBox_SqlNum.TabIndex = 2;
			this.textBox_SqlNum.Text = "";
			this.textBox_SqlNum.TextChanged += new System.EventHandler(this.textBox_SqlNum_TextChanged);
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(192, 16);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(56, 24);
			this.label11.TabIndex = 1;
			this.label11.Text = "语句数：";
			// 
			// checkBox_SaveSql
			// 
			this.checkBox_SaveSql.Location = new System.Drawing.Point(8, 16);
			this.checkBox_SaveSql.Name = "checkBox_SaveSql";
			this.checkBox_SaveSql.Size = new System.Drawing.Size(176, 24);
			this.checkBox_SaveSql.TabIndex = 0;
			this.checkBox_SaveSql.Text = "保存出错前执行的语句";
			this.checkBox_SaveSql.Click += new System.EventHandler(this.checkBox_SaveSql_Click);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.cBTLookUp2);
			this.groupBox5.Controls.Add(this.lab123);
			this.groupBox5.Controls.Add(this.cTBXMLFilePath);
			this.groupBox5.Controls.Add(this.label12);
			this.groupBox5.Controls.Add(this.cTBCheckServerTime);
			this.groupBox5.Controls.Add(this.cCBAutoRunCheck);
			this.groupBox5.Location = new System.Drawing.Point(8, 416);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(336, 88);
			this.groupBox5.TabIndex = 11;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "重起服务器设置";
			// 
			// cBTLookUp2
			// 
			this.cBTLookUp2.Location = new System.Drawing.Point(256, 56);
			this.cBTLookUp2.Name = "cBTLookUp2";
			this.cBTLookUp2.Size = new System.Drawing.Size(72, 24);
			this.cBTLookUp2.TabIndex = 11;
			this.cBTLookUp2.Text = "浏览(&L)";
			this.cBTLookUp2.Click += new System.EventHandler(this.cBTLookUp2_Click_1);
			// 
			// lab123
			// 
			this.lab123.Location = new System.Drawing.Point(8, 56);
			this.lab123.Name = "lab123";
			this.lab123.Size = new System.Drawing.Size(88, 16);
			this.lab123.TabIndex = 10;
			this.lab123.Text = "xml文件路径";
			// 
			// cTBXMLFilePath
			// 
			this.cTBXMLFilePath.Location = new System.Drawing.Point(104, 56);
			this.cTBXMLFilePath.Name = "cTBXMLFilePath";
			this.cTBXMLFilePath.Size = new System.Drawing.Size(136, 21);
			this.cTBXMLFilePath.TabIndex = 6;
			this.cTBXMLFilePath.Text = "";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(176, 24);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(96, 21);
			this.label12.TabIndex = 4;
			this.label12.Text = "检察间隔(秒)：";
			// 
			// cTBCheckServerTime
			// 
			this.cTBCheckServerTime.Location = new System.Drawing.Point(272, 24);
			this.cTBCheckServerTime.Name = "cTBCheckServerTime";
			this.cTBCheckServerTime.Size = new System.Drawing.Size(56, 21);
			this.cTBCheckServerTime.TabIndex = 3;
			this.cTBCheckServerTime.Text = "300";
			// 
			// cCBAutoRunCheck
			// 
			this.cCBAutoRunCheck.Location = new System.Drawing.Point(8, 24);
			this.cCBAutoRunCheck.Name = "cCBAutoRunCheck";
			this.cCBAutoRunCheck.Size = new System.Drawing.Size(152, 21);
			this.cCBAutoRunCheck.TabIndex = 2;
			this.cCBAutoRunCheck.Text = "服务器断开时自动重启";
			this.cCBAutoRunCheck.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.radioButtonLin);
			this.groupBox6.Controls.Add(this.radioButtonWin);
			this.groupBox6.Location = new System.Drawing.Point(352, 416);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(136, 48);
			this.groupBox6.TabIndex = 12;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "测试服务器环境";
			// 
			// radioButtonLin
			// 
			this.radioButtonLin.Location = new System.Drawing.Point(72, 24);
			this.radioButtonLin.Name = "radioButtonLin";
			this.radioButtonLin.Size = new System.Drawing.Size(56, 16);
			this.radioButtonLin.TabIndex = 1;
			this.radioButtonLin.Text = "Linux";
			// 
			// radioButtonWin
			// 
			this.radioButtonWin.Location = new System.Drawing.Point(8, 24);
			this.radioButtonWin.Name = "radioButtonWin";
			this.radioButtonWin.Size = new System.Drawing.Size(104, 16);
			this.radioButtonWin.TabIndex = 0;
			this.radioButtonWin.Text = "Windows";
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.textBoxS9);
			this.groupBox7.Controls.Add(this.label22);
			this.groupBox7.Controls.Add(this.textBoxS8);
			this.groupBox7.Controls.Add(this.label21);
			this.groupBox7.Controls.Add(this.textBoxS7);
			this.groupBox7.Controls.Add(this.label20);
			this.groupBox7.Controls.Add(this.textBoxS6);
			this.groupBox7.Controls.Add(this.label19);
			this.groupBox7.Controls.Add(this.textBoxS5);
			this.groupBox7.Controls.Add(this.label18);
			this.groupBox7.Controls.Add(this.textBoxS4);
			this.groupBox7.Controls.Add(this.label17);
			this.groupBox7.Controls.Add(this.textBoxS3);
			this.groupBox7.Controls.Add(this.label16);
			this.groupBox7.Controls.Add(this.textBoxS2);
			this.groupBox7.Controls.Add(this.label15);
			this.groupBox7.Controls.Add(this.textBoxS1);
			this.groupBox7.Controls.Add(this.label14);
			this.groupBox7.Controls.Add(this.textBoxS0);
			this.groupBox7.Controls.Add(this.label13);
			this.groupBox7.Location = new System.Drawing.Point(352, 8);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(224, 344);
			this.groupBox7.TabIndex = 13;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "辅助服务器地址";
			// 
			// textBoxS9
			// 
			this.textBoxS9.Location = new System.Drawing.Point(72, 312);
			this.textBoxS9.Name = "textBoxS9";
			this.textBoxS9.Size = new System.Drawing.Size(144, 21);
			this.textBoxS9.TabIndex = 19;
			this.textBoxS9.Text = "textBox10";
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(8, 312);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(64, 16);
			this.label22.TabIndex = 18;
			this.label22.Text = "SERVER9：";
			// 
			// textBoxS8
			// 
			this.textBoxS8.Location = new System.Drawing.Point(72, 280);
			this.textBoxS8.Name = "textBoxS8";
			this.textBoxS8.Size = new System.Drawing.Size(144, 21);
			this.textBoxS8.TabIndex = 17;
			this.textBoxS8.Text = "textBox9";
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(8, 280);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(64, 16);
			this.label21.TabIndex = 16;
			this.label21.Text = "SERVER8：";
			// 
			// textBoxS7
			// 
			this.textBoxS7.Location = new System.Drawing.Point(72, 248);
			this.textBoxS7.Name = "textBoxS7";
			this.textBoxS7.Size = new System.Drawing.Size(144, 21);
			this.textBoxS7.TabIndex = 15;
			this.textBoxS7.Text = "textBox8";
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(8, 248);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(64, 16);
			this.label20.TabIndex = 14;
			this.label20.Text = "SERVER7：";
			// 
			// textBoxS6
			// 
			this.textBoxS6.Location = new System.Drawing.Point(72, 216);
			this.textBoxS6.Name = "textBoxS6";
			this.textBoxS6.Size = new System.Drawing.Size(144, 21);
			this.textBoxS6.TabIndex = 13;
			this.textBoxS6.Text = "textBox7";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(8, 216);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(64, 16);
			this.label19.TabIndex = 12;
			this.label19.Text = "SERVER6：";
			// 
			// textBoxS5
			// 
			this.textBoxS5.Location = new System.Drawing.Point(72, 184);
			this.textBoxS5.Name = "textBoxS5";
			this.textBoxS5.Size = new System.Drawing.Size(144, 21);
			this.textBoxS5.TabIndex = 11;
			this.textBoxS5.Text = "textBox6";
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(8, 184);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(64, 16);
			this.label18.TabIndex = 10;
			this.label18.Text = "SERVER5：";
			// 
			// textBoxS4
			// 
			this.textBoxS4.Location = new System.Drawing.Point(72, 152);
			this.textBoxS4.Name = "textBoxS4";
			this.textBoxS4.Size = new System.Drawing.Size(144, 21);
			this.textBoxS4.TabIndex = 9;
			this.textBoxS4.Text = "textBox5";
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(8, 152);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(64, 16);
			this.label17.TabIndex = 8;
			this.label17.Text = "SERVER4：";
			// 
			// textBoxS3
			// 
			this.textBoxS3.Location = new System.Drawing.Point(72, 120);
			this.textBoxS3.Name = "textBoxS3";
			this.textBoxS3.Size = new System.Drawing.Size(144, 21);
			this.textBoxS3.TabIndex = 7;
			this.textBoxS3.Text = "textBox4";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(8, 120);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(64, 16);
			this.label16.TabIndex = 6;
			this.label16.Text = "SERVER3：";
			// 
			// textBoxS2
			// 
			this.textBoxS2.Location = new System.Drawing.Point(72, 88);
			this.textBoxS2.Name = "textBoxS2";
			this.textBoxS2.Size = new System.Drawing.Size(144, 21);
			this.textBoxS2.TabIndex = 5;
			this.textBoxS2.Text = "textBox3";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(8, 88);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(64, 16);
			this.label15.TabIndex = 4;
			this.label15.Text = "SERVER2：";
			// 
			// textBoxS1
			// 
			this.textBoxS1.Location = new System.Drawing.Point(72, 56);
			this.textBoxS1.Name = "textBoxS1";
			this.textBoxS1.Size = new System.Drawing.Size(144, 21);
			this.textBoxS1.TabIndex = 3;
			this.textBoxS1.Text = "textBox2";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(8, 56);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(64, 14);
			this.label14.TabIndex = 2;
			this.label14.Text = "SERVER1：";
			// 
			// textBoxS0
			// 
			this.textBoxS0.Location = new System.Drawing.Point(72, 24);
			this.textBoxS0.Name = "textBoxS0";
			this.textBoxS0.Size = new System.Drawing.Size(144, 21);
			this.textBoxS0.TabIndex = 1;
			this.textBoxS0.Text = "textBox1";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(8, 24);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(64, 16);
			this.label13.TabIndex = 0;
			this.label13.Text = "SERVER0：";
			// 
			// checkBox_showResult
			// 
			this.checkBox_showResult.Location = new System.Drawing.Point(352, 384);
			this.checkBox_showResult.Name = "checkBox_showResult";
			this.checkBox_showResult.Size = new System.Drawing.Size(216, 24);
			this.checkBox_showResult.TabIndex = 14;
			this.checkBox_showResult.Text = "不检查结果集的情况下显示结果集";
			// 
			// button_level
			// 
			this.button_level.Location = new System.Drawing.Point(496, 430);
			this.button_level.Name = "button_level";
			this.button_level.Size = new System.Drawing.Size(80, 24);
			this.button_level.TabIndex = 15;
			this.button_level.Text = "测试等级";
			this.button_level.Click += new System.EventHandler(this.button_level_Click);
			// 
			// textBoxLevel
			// 
			this.textBoxLevel.Location = new System.Drawing.Point(0, 0);
			this.textBoxLevel.Name = "textBoxLevel";
			this.textBoxLevel.TabIndex = 0;
			this.textBoxLevel.Text = "";
			// 
			// checkBoxPool
			// 
			this.checkBoxPool.Location = new System.Drawing.Point(504, 360);
			this.checkBoxPool.Name = "checkBoxPool";
			this.checkBoxPool.Size = new System.Drawing.Size(64, 24);
			this.checkBoxPool.TabIndex = 16;
			this.checkBoxPool.Text = "连接池";
			// 
			// label5
			// 
			this.label5.ForeColor = System.Drawing.Color.Brown;
			this.label5.Location = new System.Drawing.Point(64, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(280, 16);
			this.label5.TabIndex = 8;
			this.label5.Text = "在服务器地址后加上冒号和端口号可指定新端口号";
			this.label5.Visible = false;
			// 
			// Form_Set
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(584, 509);
			this.Controls.Add(this.checkBoxPool);
			this.Controls.Add(this.button_level);
			this.Controls.Add(this.checkBox_showResult);
			this.Controls.Add(this.groupBox7);
			this.Controls.Add(this.groupBox6);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.comboBox_Prv);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.button_defalut);
			this.Controls.Add(this.button_Cancle);
			this.Controls.Add(this.button_Ok);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form_Set";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "参数设置";
			this.Load += new System.EventHandler(this.Form_Set_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
        //确定按钮  将各控件的值保存到类ProClass中的Pro为前缀的变量中
		//其中有ProClass.AddProValue(ProClass.ProXMLFilePath,cTBXMLFilePath.Text); 
		private void button_Ok_Click(object sender, System.EventArgs e)     //设置窗口确定按钮
		{
			try
			{
				DirectoryInfo m_di = new DirectoryInfo(Environment.CurrentDirectory);
				m_di.CreateSubdirectory("测试结果");
			}
			catch(Exception ee)
			{
				string m_txt = ee.Message;				
			}	
			
			int m_MNum;
			int m_MTime;
			int m_TNum;
			int m_SqlNum = 200;
			//int m_level;
			try
			{
				m_MNum = Convert.ToInt32(textBox_M_N.Text);
				m_MTime = Convert.ToInt32(textBox_M_T.Text);
				m_TNum = Convert.ToInt32(textBox_T_N.Text);
				m_SqlNum = Convert.ToInt32(textBox_SqlNum.Text);
				//m_level = Convert.ToInt32(textBoxLevel.Text);
			}
			catch(Exception ee)
			{
				MessageBox.Show("至少一个参数在转化为数字时发生异常！" + ee.Message, "输入参数不正确");
				return;
			}
			if(m_MNum<100 || m_MNum>1000000)
			{
				MessageBox.Show("保存的消息记录数非法，必需在100到1000000之间！", "输入参数不正确");
				return;
			}
			if(m_MTime<10 || m_MTime>100000)
			{
				MessageBox.Show("允许的最大消息间隔非法，必需在10到100000之间（单位为秒）！", "输入参数不正确");
				return;
			}
			if(m_TNum<1 || m_TNum>1000)
			{
				MessageBox.Show("测试线程数非法，必需在1到1000之间！");
				return;
			}
			if(m_SqlNum<2 || m_SqlNum > 10000)
			{
				MessageBox.Show("保证的SQL语句条数值非法，必需在1到10000之间！", "输入参数不正确");
				return;
			}
			//if (m_level<0 || m_level>15) {
				//MessageBox.Show("测试等级必需在0到15之间！", "输入参数不正确");
				//return;
			//}
			ProClass.AddProValue(ProClass.ProServer, textBox_S.Text);
			ProClass.AddProValue(ProClass.ProUserId, textBox_U.Text);
			ProClass.AddProValue(ProClass.ProPassword, textBox_P.Text);
			ProClass.AddProValue(ProClass.ProDatabase, textBox_D.Text);
			ProClass.AddProValue(ProClass.ProMsgNum, textBox_M_N.Text);
			ProClass.AddProValue(ProClass.ProIsOutMsg, checkBox_Show_Message.Checked.ToString());
			ProClass.AddProValue(ProClass.ProMsgCheckTime, textBox_M_T.Text);
			ProClass.AddProValue(ProClass.ProIsSaveMsg, checkBox_Save_Message.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsMsgSpan, checkBox_interval.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsOutTime, checkBox_time.Checked.ToString());
			ProClass.AddProValue(ProClass.ProThreadNum, textBox_T_N.Text);
			ProClass.AddProValue(ProClass.ProIsRandRun, checkBox_Random.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsAutoClearMsg, checkBox_AC.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsLoop, checkBox_Loop.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsOutVoice, checkBox_W.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsErrRun, checkBox_Ag.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsSaveSql, checkBox_SaveSql.Checked.ToString());
			ProClass.AddProValue(ProClass.ProSaveSqlNum, textBox_SqlNum.Text);
			ProClass.AddProValue(ProClass.ProIsAutoRunServer, cCBAutoRunCheck.Checked.ToString());
			ProClass.AddProValue(ProClass.ProServerCheckTime, cTBCheckServerTime.Text);
			//初值设置为当前进程的路径值 
			ProClass.AddProValue(ProClass.ProXMLFilePath,cTBXMLFilePath.Text); ////
			//路径初值先设为空
			//ProClass.AddProValue(ProClass.ProXMLFilePath,""); ////
			ProClass.AddProValue(ProClass.ProServer0, textBoxS0.Text);
			ProClass.AddProValue(ProClass.ProServer1, textBoxS1.Text);
			ProClass.AddProValue(ProClass.ProServer2, textBoxS2.Text);
			ProClass.AddProValue(ProClass.ProServer3, textBoxS3.Text);
			ProClass.AddProValue(ProClass.ProServer4, textBoxS4.Text);
			ProClass.AddProValue(ProClass.ProServer5, textBoxS5.Text);
			ProClass.AddProValue(ProClass.ProServer6, textBoxS6.Text);
			ProClass.AddProValue(ProClass.ProServer7, textBoxS7.Text);
			ProClass.AddProValue(ProClass.ProServer8, textBoxS8.Text);
			ProClass.AddProValue(ProClass.ProServer9, textBoxS9.Text);
			ProClass.AddProValue(ProClass.ProDriveName, comboBox_Prv.Text);
			ProClass.AddProValue(ProClass.ProConnectPool, checkBoxPool.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsWindows, radioButtonWin.Checked.ToString());
			ProClass.AddProValue(ProClass.ProIsShowResult, checkBox_showResult.Checked.ToString());
			//ProClass.AddProValue(ProClass.ProLevel, textBoxLevel.Text);

			ProClass.InitProVals();
			try
			{
				ProClass.cDServer.SetCheckServerTime(Convert.ToInt32(cTBCheckServerTime.Text));
			}
			catch(Exception err)
			{
				String msg = err.Message;
			}
			
			this.Close();
		}
        //取消按钮
		private void button_Cancle_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		 //默认按钮
		private void button_defalut_Click(object sender, System.EventArgs e)
		{
			textBox_S.Text = "LOCALHOST:12345";
			textBox_D.Text = "SYSTEM";
			textBox_U.Text = "SYSDBA";
			textBox_P.Text = "SYSDBA";
			textBox_M_N.Text = "1000";
			textBox_M_T.Text = "300";
			checkBox_Show_Message.Checked = true;
			checkBox_Save_Message.Checked = true;
			textBox_T_N.Text = "1";
			checkBox_Random.Checked = false;
			checkBox_Loop.Checked = false;
			checkBox_AC.Checked = true;
			checkBox_W.Checked = false;
			checkBox_Ag.Checked = false;
			radioButtonWin.Checked = true;
			radioButtonWin.Checked = false;
			textBoxS0.Text = "LOCALHOST";
			textBoxS1.Text = "LOCALHOST";
			textBoxS2.Text = "LOCALHOST";
			textBoxS3.Text = "LOCALHOST";
			textBoxS4.Text = "LOCALHOST";
			textBoxS5.Text = "LOCALHOST";
			textBoxS6.Text = "LOCALHOST";
			textBoxS7.Text = "LOCALHOST";
			textBoxS8.Text = "LOCALHOST";
			textBoxS9.Text = "LOCALHOST";
			checkBox_interval.Checked = true;
			checkBox_time.Checked = false;
			//textBoxLevel.Text = "15";
		}

		/// <summary>
		/// //设置SQL语句保留信息
		/// </summary>
		public void SetSaveSql(bool m_Save, int m_SqlNum)
		{
			checkBox_SaveSql.Checked = m_Save;
			if(m_Save)
			{
				textBox_SqlNum.ReadOnly = false;
			}
			else
				textBox_SqlNum.ReadOnly = true;
			textBox_SqlNum.Text = Convert.ToString(m_SqlNum);			
		}

		private void textBox_SqlNum_TextChanged(object sender, System.EventArgs e)
		{
			try
			{
				int m_SqlNum = Convert.ToInt32(textBox_SqlNum.Text);
			}
			catch(Exception ee)
			{
				MessageBox.Show("参数在转化为数字时发生异常！" + ee.Message, "输入参数不正确");
				return;
			}
		}

		private void checkBox_SaveSql_Click(object sender, System.EventArgs e)
		{
			if(checkBox_SaveSql.Checked)
			{
				textBox_SqlNum.ReadOnly = false;
			}
			else
				textBox_SqlNum.ReadOnly = true;
		}

		private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
		{
			cTBCheckServerTime.Enabled = cCBAutoRunCheck.Checked;
		}

		public void SetAutoRunInfo(bool bCk, int iTime, string sPath)
		{
			cCBAutoRunCheck.Checked = bCk;
			cTBCheckServerTime.Text = Convert.ToString(iTime);
			cTBXMLFilePath.Text=sPath;  //默认用与服务器相同路径下的测试文件

			cTBCheckServerTime.Enabled = cCBAutoRunCheck.Checked;
            cTBXMLFilePath.Enabled=cCBAutoRunCheck.Checked;  //默认什么？
            cBTLookUp2.Enabled = cCBAutoRunCheck.Checked;
		}

	    //用类ProClass的值来填充设置Form_Set窗口的控件显示内容
		private void Form_Set_Load(object sender, System.EventArgs e)
		{
			textBox_S.Text = ProClass.sValServer;
			textBox_U.Text = ProClass.sValUserId;
			textBox_P.Text = ProClass.sValPassword;
			textBox_D.Text = ProClass.sValDatabase;
			textBox_M_N.Text = ProClass.sValMsgNum;
			checkBox_Show_Message.Checked = ProClass.bValIsOutMsg;
			textBox_M_T.Text = ProClass.sValMsgCheckTime;
			checkBox_Save_Message.Checked = ProClass.GetIsSaveMsg();
			checkBox_interval.Checked = ProClass.bValIsMsgSpan;
			checkBox_time.Checked = ProClass.GetIsOutTime();
			textBox_T_N.Text = ProClass.sValThreadNum;
			checkBox_Random.Checked = ProClass.bValIsRandRun;
			checkBox_AC.Checked = ProClass.GetIsAutoClearMsg();
			checkBox_Loop.Checked = ProClass.GetIsLoopRun();
			checkBox_W.Checked = ProClass.bValIsOutVoice;
			checkBox_Ag.Checked = ProClass.GetIsErrRun();;
			checkBox_SaveSql.Checked = ProClass.bValIsSaveSql;
			textBox_SqlNum.Text = ProClass.sValSaveSqlNum;
			cCBAutoRunCheck.Checked= ProClass.GetIsAutoRunServer();
			checkBoxPool.Checked = ProClass.GetIsEnableConnectPool();
			cTBCheckServerTime.Text = ProClass.sValServerCheckTime;
			cTBXMLFilePath.Text=ProClass.sValXMLFilePath;   //从变量sValXMLFilePath中获取
			textBoxS0.Text = ProClass.sValServer0;
			textBoxS1.Text = ProClass.sValServer1;
			textBoxS2.Text = ProClass.sValServer2;
			textBoxS3.Text = ProClass.sValServer3;
			textBoxS4.Text = ProClass.sValServer4;
			textBoxS5.Text = ProClass.sValServer5;
			textBoxS6.Text = ProClass.sValServer6;
			textBoxS7.Text = ProClass.sValServer7;
			textBoxS8.Text = ProClass.sValServer8;
			textBoxS9.Text = ProClass.sValServer9;
			//textBoxLevel.Text = ProClass.sValLevel;


			comboBox_Prv.Text = ProClass.sValDriveName;
			radioButtonWin.Checked = ProClass.GetIsWindows();
			radioButtonLin.Checked = !ProClass.GetIsWindows();
			checkBox_showResult.Checked = ProClass.GetIsShowResult();
		}

       //xml路径“浏览”按钮
		private void cBTLookUp2_Click_1(object sender, System.EventArgs e)
		{
			FolderBrowserDialog m_OpenFileDialog=new FolderBrowserDialog();
			if (cTBXMLFilePath.Text != "")
			{
				m_OpenFileDialog.SelectedPath= cTBXMLFilePath.Text;
			}
			else
				m_OpenFileDialog.SelectedPath= Environment.GetEnvironmentVariable("DM_HOME");  
			m_OpenFileDialog.Description= "指定xml测试文件路径";
			if(m_OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				cTBXMLFilePath.Text = m_OpenFileDialog.SelectedPath;
			}
			//ProClass.sValOldXMLFilePath=cTBXMLFilePath.Text;
		}


		private void button_level_Click(object sender, System.EventArgs e)
		{
			Form_level_set m_levelSet =new Form_level_set ();
			m_levelSet.ShowDialog ();

			if(m_levelSet.DialogResult == DialogResult.OK)
			{
				this.ShowDialog();
			}			
		}

		private void textBox_S_Enter(object sender, System.EventArgs e)
		{
			label5.Visible = true;
		}

		private void textBox_S_Leave(object sender, System.EventArgs e)
		{
			label5.Visible = false;
		}	
	}
}
