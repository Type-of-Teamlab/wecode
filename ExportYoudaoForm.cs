﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Threading;
using System.Xml;
using System.Configuration;

namespace WeCode1._0
{
    public partial class ExportYoudaoForm : Form
    {
        Thread t;
        public OleDbConnection ExportConn;
        //每次插入数据库记录后，nodeid递增
        public int iNodeId = 1;

        public ExportYoudaoForm(OleDbConnection conn)
        {
            InitializeComponent();
            ExportConn = conn;
            label1.Text = "云笔记导出过程中请不要操作云笔记\r\n附件较大的笔记可能导出较慢，请耐心等待！";
        }

        private void ExportYoudaoForm_Shown(object sender, EventArgs e)
        {
            //新线程下载
            Control.CheckForIllegalCrossThreadCalls = false;
            t = new Thread(new ThreadStart(ExportYoudao));
            t.Start(); 
        }

        //导出笔记本
        public void ExportYoudao()
        {

            //先删除mykey以及ttree表
            string delSQL = "delete from mykeys";
            AccessAdo.ExecuteNonQuery(ExportConn, delSQL);
            delSQL = "delete from ttree";
            AccessAdo.ExecuteNonQuery(ExportConn, delSQL);
            delSQL = "delete from tcontent";
            AccessAdo.ExecuteNonQuery(ExportConn, delSQL);


            //1、先导出秘钥信息
            labelMessage.Text = "正在导出秘钥信息...";
            XmlDocument doc = new XmlDocument();
            doc.Load("TreeNodeLocal.xml");
            XmlNode preNode = doc.SelectSingleNode("//wecode[@KeyD5]");
            if (preNode != null)
            {
                //存在秘钥信息
                string keyD5 = preNode.Attributes["KeyD5"].Value;
                string keyE = preNode.Attributes["KeyE"].Value;

                //第一次设置密码，写入到数据库
                OleDbParameter p1 = new OleDbParameter("@KeyE", OleDbType.VarChar);
                p1.Value = keyE;
                OleDbParameter p2 = new OleDbParameter("@KeyD5", OleDbType.VarChar);
                p2.Value = keyD5;

                OleDbParameter[] ArrPara = new OleDbParameter[2];
                ArrPara[0] = p1;
                ArrPara[1] = p2;
                string SQL = "insert into MyKeys(KeyE,KeyD5) values(@KeyE,@KeyD5)";
                AccessAdo.ExecuteNonQuery(ExportConn, SQL, ArrPara);

            }
            labelMessage.Text = "秘钥信息导出完毕...";


            //2、生成ttree
            labelMessage.Text = "正在生成目录...";
            XmlNode wecode = doc.DocumentElement;

            //标记顺序的下标
            int iTurn = 1;
            foreach (XmlNode cNode in wecode)
            {
                //添加根节点
                if (cNode.Name == "note")
                {
                    string title = cNode.Attributes["title"].Value;
                    string Language = cNode.Attributes["Language"].Value;
                    string SysId = PubFunc.Language2Synid(Language);
                    string path = cNode.Attributes["path"].Value;
                    string createtime = cNode.Attributes["createtime"].Value;
                    string islock = cNode.Attributes["IsLock"].Value;
                    string ismark = cNode.Attributes["isMark"].Value;
                    string id = cNode.Attributes["id"].Value;
                    int nodeid = this.iNodeId;

                    OleDbParameter p1 = new OleDbParameter("@NodeId", OleDbType.Integer);
                    p1.Value = nodeid;
                    OleDbParameter p2 = new OleDbParameter("@Title", OleDbType.VarChar);
                    p2.Value = title;
                    OleDbParameter p3 = new OleDbParameter("@path", OleDbType.VarChar);
                    p3.Value = path;
                    OleDbParameter p4 = new OleDbParameter("@parentid", OleDbType.Integer);
                    p4.Value = 0;
                    OleDbParameter p5 = new OleDbParameter("@Type", OleDbType.Integer);
                    p5.Value = 1;
                    OleDbParameter p6 = new OleDbParameter("@createtime", OleDbType.Integer);
                    p6.Value = createtime;
                    OleDbParameter p7 = new OleDbParameter("@SysId", OleDbType.Integer);
                    p7.Value = SysId;
                    OleDbParameter p8 = new OleDbParameter("@turn", OleDbType.Integer);
                    p8.Value = iTurn;
                    OleDbParameter p9 = new OleDbParameter("@marktime", OleDbType.Integer);
                    p9.Value = ismark;
                    OleDbParameter p10 = new OleDbParameter("@islock", OleDbType.Integer);
                    p10.Value = islock;
                    OleDbParameter p11 = new OleDbParameter("@gid", OleDbType.VarChar);
                    p11.Value = id;

                    OleDbParameter[] ArrPara = new OleDbParameter[11];
                    ArrPara[0] = p1;
                    ArrPara[1] = p2;
                    ArrPara[2] = p3;
                    ArrPara[3] = p4;
                    ArrPara[4] = p5;
                    ArrPara[5] = p6;
                    ArrPara[6] = p7;
                    ArrPara[7] = p8;
                    ArrPara[8] = p9;
                    ArrPara[9] = p10;
                    ArrPara[10] = p11;

                    string SQL = "insert into ttree(NodeID,Title,path,ParentId,Type,CreateTime,SynId,Turn,MarkTime,IsLock,gid) " +
                                " values(@NodeId,@Title,@path,@parentid,@Type,@createtime,@SysId,@turn,@marktime,@islock,@gid) ";
                    AccessAdo.ExecuteNonQuery(ExportConn, SQL, ArrPara);

                    this.iNodeId++;
                    iTurn++;

                    addTreeNode(cNode, nodeid);

                }
                else if (cNode.Name == "book")
                {
                    string title = cNode.Attributes["title"].Value;
                    string Language = cNode.Attributes["Language"].Value;
                    string SysId = PubFunc.Language2Synid(Language);
                    string id = cNode.Attributes["id"].Value;
                    int nodeid = this.iNodeId;

                    OleDbParameter p1 = new OleDbParameter("@NodeId", OleDbType.Integer);
                    p1.Value = nodeid;
                    OleDbParameter p2 = new OleDbParameter("@Title", OleDbType.VarChar);
                    p2.Value = title;
                    OleDbParameter p3 = new OleDbParameter("@path", OleDbType.VarChar);
                    p3.Value = "";
                    OleDbParameter p4 = new OleDbParameter("@parentid", OleDbType.Integer);
                    p4.Value = 0;
                    OleDbParameter p5 = new OleDbParameter("@Type", OleDbType.Integer);
                    p5.Value = 0;
                    OleDbParameter p6 = new OleDbParameter("@createtime", OleDbType.Integer);
                    p6.Value = PubFunc.time2TotalSeconds();
                    OleDbParameter p7 = new OleDbParameter("@SysId", OleDbType.Integer);
                    p7.Value = SysId;
                    OleDbParameter p8 = new OleDbParameter("@turn", OleDbType.Integer);
                    p8.Value = iTurn;
                    OleDbParameter p9 = new OleDbParameter("@marktime", OleDbType.Integer);
                    p9.Value = 0;
                    OleDbParameter p10 = new OleDbParameter("@islock", OleDbType.Integer);
                    p10.Value = 0;
                    OleDbParameter p11 = new OleDbParameter("@gid", OleDbType.VarChar);
                    p11.Value = id;

                    OleDbParameter[] ArrPara = new OleDbParameter[11];
                    ArrPara[0] = p1;
                    ArrPara[1] = p2;
                    ArrPara[2] = p3;
                    ArrPara[3] = p4;
                    ArrPara[4] = p5;
                    ArrPara[5] = p6;
                    ArrPara[6] = p7;
                    ArrPara[7] = p8;
                    ArrPara[8] = p9;
                    ArrPara[9] = p10;
                    ArrPara[10] = p11;

                    string SQL = "insert into ttree(NodeID,Title,path,ParentId,Type,CreateTime,SynId,Turn,MarkTime,IsLock,gid) " +
                                " values(@NodeId,@Title,@path,@parentid,@Type,@createtime,@SysId,@turn,@marktime,@islock,@gid) ";
                    AccessAdo.ExecuteNonQuery(ExportConn, SQL, ArrPara);

                    this.iNodeId++;
                    iTurn++;

                    addTreeNode(cNode, nodeid);
                }

            }

            //3、将tcontent中没有的从ttree重新插入
            string sql = "insert into tcontent(nodeid,updatetime,gid,path) select nodeid,createtime,gid,path from ttree where ttree.type=1 and gid not in (select gid from tcontent)";
            AccessAdo.ExecuteNonQuery(ExportConn, sql);

            labelMessage.Text = "目录生成完毕...";


            //4、开始从云端获取文章以及附件信息
            //附件ID
            int Affid = 1;
            sql = "select * from tcontent";
            DataTable dt = AccessAdo.ExecuteDataSet(ExportConn, sql).Tables[0];
            int RowCount = dt.Rows.Count;
            progressBar1.Maximum = RowCount;
            for (int i = 0; i < RowCount; i++)
            {
                labelMessage.Text = "正在导出第"+(i+1).ToString()+"篇笔记及其附件，总共"+RowCount.ToString()+"篇...";
                string path = dt.Rows[i]["Path"].ToString();
                string gid = dt.Rows[i]["Gid"].ToString();
                string nodeid = dt.Rows[i]["NodeId"].ToString();
                string[] result = NoteAPI.GetNote(path);
                string content=result[0];
                int updatetime = Convert.ToInt32(result[1]);

                OleDbParameter p1 = new OleDbParameter("@Content", OleDbType.VarChar);
                p1.Value = content;
                OleDbParameter p2 = new OleDbParameter("@UpdateTime", OleDbType.Integer);
                p2.Value = updatetime;
                OleDbParameter p3 = new OleDbParameter("@path", OleDbType.VarChar);
                p3.Value = path;

                OleDbParameter[] ArrPara = new OleDbParameter[3];
                ArrPara[0] = p1;
                ArrPara[1] = p2;
                ArrPara[2] = p3;
                sql = "update tcontent set content=@Content,updatetime=@UpdateTime where path=@path";
                AccessAdo.ExecuteNonQuery(ExportConn, sql, ArrPara);

                //有道附件
                XmlDocument xDoc = NoteAPI.GetResourceWithXML(path);
                string fileUrl,title;
                int Datalength;
                foreach (XmlNode xNode in xDoc.DocumentElement.ChildNodes)
                {
                    fileUrl = xNode.Attributes["path"].Value;
                    title = xNode.Attributes["title"].Value;
                    Datalength = Convert.ToInt32(xNode.Attributes["filelength"].Value);
                    //下载附件数据
                    int intNewAffixid = Affid;

                    int Nodeid = Convert.ToInt32(nodeid);
                    string Title = title;
                    int timeSeconds = PubFunc.time2TotalSeconds();

                    //affixid
                    OleDbParameter para1 = new OleDbParameter("@affixid", OleDbType.Integer);
                    para1.Value = intNewAffixid;
                    //nodeid
                    OleDbParameter para2 = new OleDbParameter("@nodeid", OleDbType.Integer);
                    para2.Value = Nodeid;
                    //title
                    OleDbParameter para3 = new OleDbParameter("@title", OleDbType.VarChar);
                    para3.Value = Title;
                    //二进制数据
                    OleDbParameter para4 = new OleDbParameter("@Data", OleDbType.Binary);
                    para4.Value = DownloadFile(fileUrl);
                    //size
                    OleDbParameter para5 = new OleDbParameter("@size", OleDbType.Integer);
                    para5.Value = Datalength;
                    //time
                    OleDbParameter para6 = new OleDbParameter("@time", OleDbType.Integer);
                    para6.Value = timeSeconds;

                    OleDbParameter para7 = new OleDbParameter("@gid", OleDbType.VarChar);
                    para7.Value = gid;

                    OleDbParameter[] arrPara = new OleDbParameter[7];
                    arrPara[0] = para1;
                    arrPara[1] = para2;
                    arrPara[2] = para3;
                    arrPara[3] = para4;
                    arrPara[4] = para5;
                    arrPara[5] = para6;
                    arrPara[6] = para7;
                    string SQL = "insert into tattachment values(@affixid,@nodeid,@title,@Data,@size,@time,@gid)";
                    AccessAdo.ExecuteNonQuery(ExportConn,SQL, arrPara);
                    Affid++;
                }

                progressBar1.Value = i + 1;
                
            }


            MessageBox.Show("云笔记同步到本地完成！");
            //关闭数据库连接
            if (ExportConn.State != ConnectionState.Closed)
                ExportConn.Close();
            this.Close();

        }


        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="URL">下载文件地址</param>
        public byte[] DownloadFile(string URL)
        {
            URL += "?oauth_token=" + ConfigurationManager.AppSettings["AccessToken"];
            byte[] result;
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                System.IO.Stream st = myrp.GetResponseStream();
                memoryStream = new System.IO.MemoryStream();

                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);

                //已上传的字节数 
                long offset = 0;

                while (osize > 0)
                {
                    //System.Windows.Forms.Application.DoEvents();
                    memoryStream.Write(by, 0, osize);
                    offset += osize;
                    osize = st.Read(by, 0, (int)by.Length);
                }
                //result = memoryStream.GetBuffer();
                result = new byte[memoryStream.Length];
                memoryStream.Position = 0;
                memoryStream.Read(result, 0, (int)memoryStream.Length);
                st.Close();
                return result;
            }
            catch (System.Exception)
            {
                return null;
            }
            finally
            {
                memoryStream.Close();
            }
        }



        //递归方法
        private void addTreeNode(XmlNode xmlNode, int pid)
        {
            XmlNode xNode;
            XmlNodeList xNodeList;

            if (xmlNode.HasChildNodes) //The current node has children
            {
                xNodeList = xmlNode.ChildNodes;

                for (int x = 0; x <= xNodeList.Count - 1; x++) //Loop through the child nodes
                {
                    xNode = xmlNode.ChildNodes[x];
                    if (xNode.Name == "note")
                    {
                        string title = xNode.Attributes["title"].Value;
                        string Language = xNode.Attributes["Language"].Value;
                        string SysId = PubFunc.Language2Synid(Language);
                        string path = xNode.Attributes["path"].Value;
                        string createtime = xNode.Attributes["createtime"].Value;
                        string islock = xNode.Attributes["IsLock"].Value;
                        string ismark = xNode.Attributes["isMark"].Value;
                        string id = xNode.Attributes["id"].Value;
                        int nodeid = this.iNodeId;

                        OleDbParameter p1 = new OleDbParameter("@NodeId", OleDbType.Integer);
                        p1.Value = nodeid;
                        OleDbParameter p2 = new OleDbParameter("@Title", OleDbType.VarChar);
                        p2.Value = title;
                        OleDbParameter p3 = new OleDbParameter("@path", OleDbType.VarChar);
                        p3.Value = path;
                        OleDbParameter p4 = new OleDbParameter("@parentid", OleDbType.Integer);
                        p4.Value = pid;
                        OleDbParameter p5 = new OleDbParameter("@Type", OleDbType.Integer);
                        p5.Value = 1;
                        OleDbParameter p6 = new OleDbParameter("@createtime", OleDbType.Integer);
                        p6.Value = createtime;
                        OleDbParameter p7 = new OleDbParameter("@SysId", OleDbType.Integer);
                        p7.Value = SysId;
                        OleDbParameter p8 = new OleDbParameter("@turn", OleDbType.Integer);
                        p8.Value = x + 1;
                        OleDbParameter p9 = new OleDbParameter("@marktime", OleDbType.Integer);
                        p9.Value = ismark;
                        OleDbParameter p10 = new OleDbParameter("@islock", OleDbType.Integer);
                        p10.Value = islock;
                        OleDbParameter p11 = new OleDbParameter("@gid", OleDbType.VarChar);
                        p11.Value = id;

                        OleDbParameter[] ArrPara = new OleDbParameter[11];
                        ArrPara[0] = p1;
                        ArrPara[1] = p2;
                        ArrPara[2] = p3;
                        ArrPara[3] = p4;
                        ArrPara[4] = p5;
                        ArrPara[5] = p6;
                        ArrPara[6] = p7;
                        ArrPara[7] = p8;
                        ArrPara[8] = p9;
                        ArrPara[9] = p10;
                        ArrPara[10] = p11;

                        string SQL = "insert into ttree(NodeID,Title,path,ParentId,Type,CreateTime,SynId,Turn,MarkTime,IsLock,gid) " +
                                    " values(@NodeId,@Title,@path,@parentid,@Type,@createtime,@SysId,@turn,@marktime,@islock,@gid) ";
                        AccessAdo.ExecuteNonQuery(ExportConn, SQL, ArrPara);

                        this.iNodeId++;

                        addTreeNode(xNode, nodeid);
                    }
                    else if (xNode.Name == "book")
                    {
                        string title = xNode.Attributes["title"].Value;
                        string Language = xNode.Attributes["Language"].Value;
                        string SysId = PubFunc.Language2Synid(Language);
                        string id = xNode.Attributes["id"].Value;
                        int nodeid = this.iNodeId;

                        OleDbParameter p1 = new OleDbParameter("@NodeId", OleDbType.Integer);
                        p1.Value = nodeid;
                        OleDbParameter p2 = new OleDbParameter("@Title", OleDbType.VarChar);
                        p2.Value = title;
                        OleDbParameter p3 = new OleDbParameter("@path", OleDbType.VarChar);
                        p3.Value = "";
                        OleDbParameter p4 = new OleDbParameter("@parentid", OleDbType.Integer);
                        p4.Value = pid;
                        OleDbParameter p5 = new OleDbParameter("@Type", OleDbType.Integer);
                        p5.Value = 0;
                        OleDbParameter p6 = new OleDbParameter("@createtime", OleDbType.Integer);
                        p6.Value = PubFunc.time2TotalSeconds();
                        OleDbParameter p7 = new OleDbParameter("@SysId", OleDbType.Integer);
                        p7.Value = SysId;
                        OleDbParameter p8 = new OleDbParameter("@turn", OleDbType.Integer);
                        p8.Value = x + 1;
                        OleDbParameter p9 = new OleDbParameter("@marktime", OleDbType.Integer);
                        p9.Value = 0;
                        OleDbParameter p10 = new OleDbParameter("@islock", OleDbType.Integer);
                        p10.Value = 0;
                        OleDbParameter p11 = new OleDbParameter("@gid", OleDbType.VarChar);
                        p11.Value = id;

                        OleDbParameter[] ArrPara = new OleDbParameter[11];
                        ArrPara[0] = p1;
                        ArrPara[1] = p2;
                        ArrPara[2] = p3;
                        ArrPara[3] = p4;
                        ArrPara[4] = p5;
                        ArrPara[5] = p6;
                        ArrPara[6] = p7;
                        ArrPara[7] = p8;
                        ArrPara[8] = p9;
                        ArrPara[9] = p10;
                        ArrPara[10] = p11;

                        string SQL = "insert into ttree(NodeID,Title,path,ParentId,Type,CreateTime,SynId,Turn,MarkTime,IsLock,gid) " +
                                    " values(@NodeId,@Title,@path,@parentid,@Type,@createtime,@SysId,@turn,@marktime,@islock,@gid) ";
                        AccessAdo.ExecuteNonQuery(ExportConn, SQL, ArrPara);

                        this.iNodeId++;

                        addTreeNode(xNode, nodeid);
                    }

                }
            }
        }

        private void ExportYoudaoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            t.Abort();
            //关闭数据库连接
            if (ExportConn.State != ConnectionState.Closed)
                ExportConn.Close();

            this.Dispose();

            this.Close();
        }

    }
}
