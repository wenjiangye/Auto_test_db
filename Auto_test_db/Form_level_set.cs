using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Auto_test_db
{
	/// <summary>
	/// Form_level_set 的摘要说明。
	/// </summary>
	public class Form_level_set : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox lisan_box;
		private System.Windows.Forms.TextBox lianxu_box;
		private System.Windows.Forms.Button btn_Level_OK;
		private System.Windows.Forms.Button btn_Level_Cancel;
		private System.Windows.Forms.RadioButton lisan_Level_button;
		private System.Windows.Forms.RadioButton lianxu_Level_button;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form_level_set()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();
			this.AcceptButton = btn_Level_OK;
			this.CancelButton = btn_Level_Cancel;

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form_level_set));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lianxu_box = new System.Windows.Forms.TextBox();
			this.lianxu_Level_button = new System.Windows.Forms.RadioButton();
			this.lisan_box = new System.Windows.Forms.TextBox();
			this.lisan_Level_button = new System.Windows.Forms.RadioButton();
			this.btn_Level_OK = new System.Windows.Forms.Button();
			this.btn_Level_Cancel = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lianxu_box);
			this.groupBox1.Controls.Add(this.lianxu_Level_button);
			this.groupBox1.Controls.Add(this.lisan_box);
			this.groupBox1.Controls.Add(this.lisan_Level_button);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(256, 128);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "用例等级";
			// 
			// lianxu_box
			// 
			this.lianxu_box.Location = new System.Drawing.Point(8, 96);
			this.lianxu_box.Name = "lianxu_box";
			this.lianxu_box.Size = new System.Drawing.Size(240, 21);
			this.lianxu_box.TabIndex = 3;
			this.lianxu_box.TabStop = false;
			this.lianxu_box.Text = "";
			// 
			// lianxu_Level_button
			// 
			this.lianxu_Level_button.Checked = true;
			this.lianxu_Level_button.Location = new System.Drawing.Point(8, 72);
			this.lianxu_Level_button.Name = "lianxu_Level_button";
			this.lianxu_Level_button.Size = new System.Drawing.Size(120, 24);
			this.lianxu_Level_button.TabIndex = 2;
			this.lianxu_Level_button.TabStop = true;
			this.lianxu_Level_button.Text = "连续的测试等级";
			this.lianxu_Level_button.Click += new System.EventHandler(this.lianxu_Level_button_Click);
			this.lianxu_Level_button.CheckedChanged += new System.EventHandler(this.lianxu_Level_button_CheckedChanged);
			// 
			// lisan_box
			// 
			this.lisan_box.BackColor = System.Drawing.SystemColors.ScrollBar;
			this.lisan_box.Enabled = false;
			this.lisan_box.Location = new System.Drawing.Point(8, 40);
			this.lisan_box.Multiline = true;
			this.lisan_box.Name = "lisan_box";
			this.lisan_box.Size = new System.Drawing.Size(240, 21);
			this.lisan_box.TabIndex = 1;
			this.lisan_box.TabStop = false;
			this.lisan_box.Text = "";
			// 
			// lisan_Level_button
			// 
			this.lisan_Level_button.Location = new System.Drawing.Point(8, 16);
			this.lisan_Level_button.Name = "lisan_Level_button";
			this.lisan_Level_button.Size = new System.Drawing.Size(176, 24);
			this.lisan_Level_button.TabIndex = 0;
			this.lisan_Level_button.Text = "离散的选择测试等级";
			this.lisan_Level_button.Click += new System.EventHandler(this.lisan_Level_button_Click);
			this.lisan_Level_button.CheckedChanged += new System.EventHandler(this.lisan_Level_button_CheckedChanged);
			// 
			// btn_Level_OK
			// 
			this.btn_Level_OK.Location = new System.Drawing.Point(120, 144);
			this.btn_Level_OK.Name = "btn_Level_OK";
			this.btn_Level_OK.Size = new System.Drawing.Size(64, 23);
			this.btn_Level_OK.TabIndex = 1;
			this.btn_Level_OK.Text = "确定";
			this.btn_Level_OK.Click += new System.EventHandler(this.btn_Level_OK_Click);
			// 
			// btn_Level_Cancel
			// 
			this.btn_Level_Cancel.Location = new System.Drawing.Point(200, 144);
			this.btn_Level_Cancel.Name = "btn_Level_Cancel";
			this.btn_Level_Cancel.Size = new System.Drawing.Size(64, 23);
			this.btn_Level_Cancel.TabIndex = 2;
			this.btn_Level_Cancel.Text = "取消";
			this.btn_Level_Cancel.Click += new System.EventHandler(this.btn_Level_Cancel_Click);
			// 
			// Form_level_set
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(272, 174);
			this.Controls.Add(this.btn_Level_Cancel);
			this.Controls.Add(this.btn_Level_OK);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form_level_set";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "选择测试等级";
			this.Load += new System.EventHandler(this.Form_level_set_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		

		private void btn_Level_Cancel_Click(object sender, System.EventArgs e)
		{
			this.Close ();
		}

		private void lisan_Level_button_CheckedChanged(object sender, System.EventArgs e)
		{
			
		}

		private void lianxu_Level_button_CheckedChanged(object sender, System.EventArgs e)
		{
			
		}

		private void btn_Level_OK_Click(object sender, System.EventArgs e)
		{
			int LevelNum=15;
			int m_Level;
			string s=lisan_box.Text;
			if(ProClass.bValIsLevel ==true)//若是连续的测试等级，对输入的等级值是否合法进行判断
			{
				m_Level = Convert.ToInt32(lianxu_box.Text);
				if (m_Level<0 || m_Level>15) 
				{
					MessageBox.Show("测试等级必需在0到15之间！", "输入参数不正确");
					return;
				}
				ProClass.AddProValue(ProClass.ProLevel2, lianxu_box.Text);//如果合法，把他们的值添加到数据库中
			}
			else//对输入离散的测试等级值进行判断
			{
				char[] separator ={' ',','};
				string[] s_Level = s.Split(separator);
				if(s_Level.Length>LevelNum)
				{
					MessageBox.Show ("测试等级的数目必须在0到15之间！","输入参数不正确");
					return;
				}
				else 
				{
					for(int i=0;i<s_Level.Length;i++)
					{
						int ml=Convert.ToInt32(s_Level[i]);
						if(ml<0||ml>15)
						{
							MessageBox.Show ("第"+(i+1)+"个测试等级必须在0到15之间！","输入参数不正确");
							return;
						}
						
					}
				}
				ProClass.AddProValue(ProClass.ProLevel1, lisan_box.Text);//如果合法，把他们的值添加到数据库中
			}
			this.Close ();
			
		    
		}

		private void Form_level_set_Load(object sender, System.EventArgs e)
		{
		    lisan_box.Text =ProClass.sValLevel1 ;
			lianxu_box.Text =ProClass.sValLevel2 ;
			if(ProClass.bValIsLevel ==true)
			{
				lianxu_Level_button.Checked =true;
				lisan_Level_button.Checked = false;
			}
			else
			{
				lianxu_Level_button.Checked =false;
				lisan_Level_button.Checked =true;
			}
		}

		private void lisan_Level_button_Click(object sender, System.EventArgs e)
		{
			ProClass.bValIsLevel =false;//把变量bValIsLevel的真值设为“false”，表示当前选择的是离散型的测试等级
			lisan_box.Enabled =true;//把lisan_box的属性“Enable”的值设为“true”，可输入的状态
			lisan_box.BackColor =SystemColors.Window ;
			lianxu_Level_button.Checked =false;
			lianxu_box.Enabled =false;
			lianxu_box.BackColor =SystemColors.ScrollBar ;
		}

		private void lianxu_Level_button_Click(object sender, System.EventArgs e)
		{
			ProClass.bValIsLevel =true;//把变量bValIsLevel的真值设为“true”，表示当前选择的是连续型的测试等级
			lisan_box.Enabled =false;
			lisan_box.BackColor =SystemColors.ScrollBar;
			lisan_Level_button.Checked =false;
			lianxu_box.Enabled =true;
			lianxu_box.BackColor =SystemColors.Window;
		}
	}
}
