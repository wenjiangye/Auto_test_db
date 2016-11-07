# include <stdio.h>
int main(int argc, char *argv[])
{	
	bool 	m_pass = false;
	//argv[0]存放的是该程序的路径
	LPCSTR 	m_ServerName = argv[1];		//服务器名
	LPCSTR 	m_UserId = argv[2];		//ID
	LPCSTR 	m_PassWord = argv[3];		//口令
	LPCSTR 	m_DataBase = argv[4];		//初始化库
	//可以用上面的用户名创建具有不同权限的ID来执行操作
	
	//做事部分
	
	//程序的最后不要忘记把该程序创建的资源清空
	if(m_pass)
	{
		return 1;	//代表该测试成功
	}
	else
	{
		return 0;	//代表该测试失败
	}
	
}


