﻿using CCWin;
using MSTSCLib;
using RemoteDesktop.Helper;
using RemoteDesktop.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteDesktop
{
    public partial class Form1 : Skin_Color
    {
        private List<AxMSTSCLib.AxMsRdpClient8NotSafeForScripting> rdpcArry = new List<AxMSTSCLib.AxMsRdpClient8NotSafeForScripting>();
        public Form1()
        {
            InitializeComponent();
            this.skinRollingBar1.StartRolling();
            treeView1.ExpandAll();
            InitTree();
            this.skinRollingBar1.StopRolling();
            this.skinRollingBar1.Visible = false;
            //var abcd = EncryptDecryptHelper.DESEncrypt("我哎你我哎你我哎你我哎你我哎你我哎你我哎你我哎你[];");
        }
        #region 私有方法
        /// <summary>
        /// 服务器树形结构初始化
        /// </summary>
        private void InitTree()
        {
            var typeData = DataHelper.GetNodeLevelData(Enums.LevelEnum.TypeNode);
            var serverData = DataHelper.GetNodeLevelData(Enums.LevelEnum.ServerNode);

            foreach (var item in typeData)
            {
                TreeNode tchild = new TreeNode();
                tchild.Name = item.NodeGuid;
                tchild.Text = item.NodeName;
                tchild.ImageIndex = 2;
                tchild.SelectedImageIndex = 2;
                var serverobj = serverData.Where(p => p.ParentGuid == item.NodeGuid);
                foreach (var child in serverobj)
                {
                    //分类节点添加服务器节点
                    TreeNode tchild2 = new TreeNode();
                    tchild2.Name = child.NodeGuid;
                    tchild2.Text = child.NodeName;
                    tchild.Nodes.Add(tchild2);
                }
                tchild.ExpandAll();
                //添加分类节点
                this.treeView1.Nodes[0].Nodes.Add(tchild);
            }
        }
        //----------------------------------------------------------------------------
        private void OnFatalErrorEvent1(object sender, AxMSTSCLib.IMsTscAxEvents_OnFatalErrorEvent e)
        {
            MessageBox.Show(e.errorCode.ToString() + "a", null, MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        private void OnFatalErrorEvent2(object sender, AxMSTSCLib.IMsTscAxEvents_OnLogonErrorEvent e)
        {
            //登入失败要用
            MessageBox.Show(e.lError.ToString() + "b", null, MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        private void OnDisconnectedFun(object sender, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEvent e)
        {
            //260 IP错误
            //516  远程服务器可能异常
            //MessageBox.Show(e.discReason.ToString() + "c", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            try
            {
                //切换成已断开图标
                var selectedNode = treeView1.SelectedNode;
                selectedNode.ImageIndex = 4;
                selectedNode.SelectedImageIndex = 4;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void OnFatalErrorEvent3(object sender, AxMSTSCLib.IMsTscAxEvents_OnWarningEvent e)
        {
            MessageBox.Show(e.warningCode.ToString() + "d", null, MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        private void OnFatalErrorEvent4(object sender, EventArgs e)
        {
            //顺利连接成功
            //MessageBox.Show("e", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            try
            {
                //切换成已断开图标
                var selectedNode = treeView1.SelectedNode;
                selectedNode.ImageIndex = 0;
                selectedNode.SelectedImageIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        //-------------------------------------------------------------------------------------------
        #endregion

        #region 事件处理（菜单 树形结构区域）
        /// <summary>
        /// 判断右键菜单显现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                //当前不是一个节点
                if (CurrentNode == null)
                {
                    return;
                }
                treeView1.SelectedNode = CurrentNode;
                //主节点
                if (CurrentNode.Level == 0)
                {
                    CurrentNode.ContextMenuStrip = this.contextMenuStrip2;
                    return;
                }
                //分类节点
                if (CurrentNode.Level == 1)
                {
                    CurrentNode.ContextMenuStrip = this.contextMenuStrip3;
                    return;
                }
                //服务器节点
                if (CurrentNode.Level == 2)
                {
                    CurrentNode.ContextMenuStrip = this.contextMenuStrip1;
                    var rdp = rdpcArry.Where(p => p.Name == CurrentNode.Name);
                    //右键菜单显示控制
                    foreach (ToolStripItem info in CurrentNode.ContextMenuStrip.Items)
                    {
                        switch (info.Text)
                        {
                            case "断开连接":
                            case "断开连接并关闭选项卡":
                                if (rdp.Count() > 0 && rdp.FirstOrDefault().Connected == 1)
                                {
                                    info.Enabled = true;
                                }
                                else
                                {
                                    info.Enabled = false;
                                }
                                break;
                            case "远程连接（或双击）":
                                if (rdp.Count() > 0 && rdp.FirstOrDefault().Connected == 1)
                                {
                                    info.Enabled = false;
                                }
                                else
                                {
                                    info.Enabled = true;
                                }
                                break;
                            default:
                                break;
                        }
                    }


                    return;
                }
            }
        }
        /// <summary>
        /// 新增服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 新增服务器分类ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AddTypeForm().Show(this);
        }
        /// <summary>
        /// 分类重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.skinRollingBar1.Visible = true;
            this.skinRollingBar1.StartRolling();
            var selectedNode = treeView1.SelectedNode;
            selectedNode.BeginEdit();
        }
        /// <summary>
        /// 编辑之后保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            try
            {
                var typeData = DataHelper.GetNodeLevelData(Enums.LevelEnum.TypeNode);
                var selectedGuid = treeView1.SelectedNode.Name;
                var newText = e.Label;
                if (string.IsNullOrEmpty(newText))
                {
                    e.CancelEdit = true;
                    this.skinRollingBar1.StopRolling();
                    this.skinRollingBar1.Visible = false;
                    return;
                }
                typeData.Where(p => p.NodeGuid == selectedGuid).FirstOrDefault().NodeName = newText;
                DataHelper.SaveData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.skinRollingBar1.StopRolling();
                this.skinRollingBar1.Visible = false;
            }

        }
        /// <summary>
        /// 新增服务器节点 （右键）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 新增服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedGuid = treeView1.SelectedNode.Name;
            new AddServerForm(selectedGuid).Show(this);
        }
        /// <summary>
        /// 新增服务器节点 （菜单）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new AddServerForm(null).Show(this);
        }
        /// <summary>
        /// 删除服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var selected = treeView1.SelectedNode;
                var guid = selected.Name;
                //删除数据节点
                DataHelper.CurrentData.Nodes.RemoveAll(p => p.NodeGuid == guid);
                //删除对于服务器数据
                DataHelper.CurrentData.Servers.RemoveAll(p => p.ServerGuid == guid);
                DataHelper.SaveData();
                //树形显示节点删除
                selected.Remove();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        /// <summary>
        /// 删除服务器分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除服务器分类ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("删除该分类，分类下的所有服务器连接也将删除，是否继续？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.OK)
                {
                    var selected = treeView1.SelectedNode;
                    var guid = selected.Name;
                    //删除分类节点
                    DataHelper.CurrentData.Nodes.RemoveAll(p => p.NodeGuid == guid);
                    var delobj = DataHelper.CurrentData.Nodes.Where(p => p.ParentGuid == guid);
                    //删除server数据
                    foreach (var item in delobj)
                    {
                        DataHelper.CurrentData.Servers.RemoveAll(p => p.ServerGuid == item.NodeGuid);
                    }
                    //删除改分类所有子节点
                    DataHelper.CurrentData.Nodes.RemoveAll(p => p.ParentGuid == guid);
                    DataHelper.SaveData();
                    selected.Remove();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        /// <summary>
        /// 节点双击连接远程服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Node.Level == 2)
                {
                    //远程服务器对应选项卡是否打开
                    if (this.tabControl1.Controls.Find(e.Node.Name, true).Count() > 0)
                    {
                        //远程服务器是否已经连接
                        var server = rdpcArry.Where(p => p.Name == e.Node.Name).FirstOrDefault();
                        if (server.Connected == 1)
                        {
                            MessageBox.Show("请不要重复连接", null, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        }
                        else
                        {
                            server.Connect();
                        }
                        return;
                    }
                    TabPage tp = new TabPage();
                    tp.Name = e.Node.Name;
                    tp.Text = e.Node.Text;
                    tp.UseVisualStyleBackColor = true;
                    tp.Height = this.tabControl1.Height - 20;
                    tp.Width = this.tabControl1.Width - 20;

                    this.tabControl1.Controls.Add(tp);
                    this.tabControl1.SelectedTab = tp;
                    AxMSTSCLib.AxMsRdpClient8NotSafeForScripting rdpc = new AxMSTSCLib.AxMsRdpClient8NotSafeForScripting();
                    rdpc.Dock = DockStyle.Fill;
                    tp.Controls.Add(rdpc);
                    var currentServer = DataHelper.CurrentData.Servers.Where(p => p.ServerGuid == e.Node.Name).FirstOrDefault();
                    rdpc.Server = currentServer.Ip;                                     //远程桌面的IP地址或者域名
                    rdpc.Domain = currentServer.Domain;                                 //远程服务器所在的域
                    rdpc.UserName = currentServer.UserName;                             //系统用户名
                    rdpc.AdvancedSettings2.ClearTextPassword = currentServer.Password;  //系统登录密码
                    rdpc.AdvancedSettings2.RDPPort = 3389;
                    rdpc.AdvancedSettings2.RedirectDrives = true;
                    rdpc.AdvancedSettings2.RedirectPrinters = true;
                    rdpc.ConnectingText = "正在连接.....";
                    rdpc.ColorDepth = 24;
                    rdpc.Name = e.Node.Name;
                    rdpc.OnFatalError += new AxMSTSCLib.IMsTscAxEvents_OnFatalErrorEventHandler(OnFatalErrorEvent1);
                    rdpc.OnLogonError += new AxMSTSCLib.IMsTscAxEvents_OnLogonErrorEventHandler(OnFatalErrorEvent2);
                    //断开连接事件触发
                    rdpc.OnDisconnected += new AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEventHandler(OnDisconnectedFun);
                    rdpc.OnWarning += new AxMSTSCLib.IMsTscAxEvents_OnWarningEventHandler(OnFatalErrorEvent3);
                    rdpc.OnConnected += new EventHandler(OnFatalErrorEvent4);
                    rdpc.Connect();
                    rdpcArry.Add(rdpc);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        /// <summary>
        ///  断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 断开连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeView1.SelectedNode;
            rdpcArry.Where(p => p.Name == selectedNode.Name).FirstOrDefault().Disconnect();
        }
        /// <summary>
        /// 断开连接并关闭选项卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 断开连接并关闭选项卡ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeView1.SelectedNode;
            rdpcArry.Where(p => p.Name == selectedNode.Name).FirstOrDefault().Disconnect();
            this.tabControl1.TabPages.RemoveByKey(selectedNode.Name);
        }
        #endregion

        #region 事件处理（tabpage区域）
        #endregion

    }
}
