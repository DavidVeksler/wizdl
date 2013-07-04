using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WebServices.UI
{
    /// <summary>
    ///     Summary description for Form1.
    /// </summary>
    public class Form1 : Form
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        private readonly PropertyGrid propertyGrid;
        private readonly PropertyGrid resultsGrid;
        private string _currentFile;

        private WebServiceList _webServiceList;
        private Button btnLoad;
        private Button btnLoadFile;
        private Button btnRefresh;
        private Button btnSaveFile;
        private Button btnTest;
        private ComboBox cbxMethods;
        private ComboBox cbxServices;
        private Panel gridPanel;
        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private SplitContainer splitContainer1;
        private TextBox txtSoap;
        private GroupBox groupResult;
        private LinkLabel linkLabel1;
        private ComboBox cbxRecentUrls;
        private TextBox txtWsdlUrl;
        private const string path = "urlcache.txt";

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            propertyGrid = new PropertyGrid();
            resultsGrid = new PropertyGrid();

            propertyGrid.HelpVisible = false;
            resultsGrid.HelpVisible = false;

            propertyGrid.ToolbarVisible = false;
            resultsGrid.ToolbarVisible = false;

            propertyGrid.Dock = DockStyle.Fill;
            resultsGrid.Dock = DockStyle.Fill;

            gridPanel.Controls.Add(propertyGrid);
            groupResult.Controls.Add(resultsGrid);

            TraceExtension.SoapTextBox = txtSoap;

            cbxServices.SelectedIndexChanged += cbxServices_SelectedIndexChanged;
            cbxMethods.SelectedIndexChanged += cbxMethods_SelectedIndexChanged;

            LoadUrlsFromCache();
        }

        /// <summary>
        ///     Clean up any resources being used.
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

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.Run(new Form1());
        }

        private void cbxServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            WebService svc = cbxServices.SelectedItem as WebService;

            if (svc != null)
            {
                cbxMethods.Items.Clear();

                foreach (WebMethod method in svc.Methods)
                {
                    cbxMethods.Items.Add(method);
                }

                if (svc.Methods.Length > 0)
                {
                    cbxMethods.SelectedIndex = 0;
                }
            }
        }

        private void cbxMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            WebMethod method = cbxMethods.SelectedItem as WebMethod;

            if (method != null)
            {
                propertyGrid.SelectedObject = method;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (txtWsdlUrl.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the WSDL URL");
                return;
            }

            AddToUrlCache(txtWsdlUrl.Text);

            try
            {
                Cursor = Cursors.WaitCursor;

                WebServiceList list = WebServiceList.LoadFromUrl(txtWsdlUrl.Text);
                Init(list, null);
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString());
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SetTitle(string file = null)
        {
            string title = "wizdl - Web Service GUI";

            if (file != null)
            {
                title += " (" + Path.GetFileName(file) + ")";
            }
            else
            {
                title += " (" + txtWsdlUrl.Text + ":" + cbxServices.Text + ":" + cbxMethods.Text + ")";
            }

            Text = title;
        }

        private void Init(WebServiceList list, string file)
        {
            _webServiceList = list;
            _currentFile = file;
            txtWsdlUrl.Text = list.Url;

            SetTitle(file);

            propertyGrid.SelectedObject = null;
            resultsGrid.SelectedObject = null;

            cbxServices.Items.Clear();
            cbxMethods.Items.Clear();

            foreach (WebService svc in _webServiceList.Services)
            {
                cbxServices.Items.Add(svc);
            }

            if (_webServiceList.Services.Length > 0)
            {
                cbxServices.SelectedIndex = 0;
            }
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = openFileDialog.FileName;
                    if (!string.IsNullOrEmpty(path))
                    {
                        WebServiceList list = WebServiceList.Deserialize(path);
                        Init(list, path);
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString());
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (_webServiceList == null)
                    return;

                string path = _currentFile;

                if (path == null && saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    path = saveFileDialog.FileName;
                }

                if (!string.IsNullOrEmpty(path))
                {
                    _webServiceList.Serialize(path);
                    _currentFile = path;
                    SetTitle(path);
                    MessageBox.Show(Path.GetFileName(path) + " saved");
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString());
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                SetTitle();

                Cursor = Cursors.WaitCursor;
                resultsGrid.CollapseAllGridItems();

                resultsGrid.SelectedObject = null;

                WebMethod method = cbxMethods.SelectedItem as WebMethod;

                if (method != null)
                {
                    resultsGrid.SelectedObject = method.Invoke();
                }

                resultsGrid.ExpandAllGridItems();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString());
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            propertyGrid.Refresh();
            resultsGrid.Refresh();
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof (Form1));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxServices = new System.Windows.Forms.ComboBox();
            this.gridPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.cbxMethods = new System.Windows.Forms.ComboBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtWsdlUrl = new System.Windows.Forms.TextBox();
            this.groupResult = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtSoap = new System.Windows.Forms.TextBox();
            this.cbxRecentUrls = new System.Windows.Forms.ComboBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Xml Files|*.xml";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "xml";
            this.saveFileDialog.Filter = "XML files|*.xml";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cbxRecentUrls);
            this.splitContainer1.Panel1.Controls.Add(this.linkLabel1);
            this.splitContainer1.Panel1.Controls.Add(this.btnSaveFile);
            this.splitContainer1.Panel1.Controls.Add(this.btnLoadFile);
            this.splitContainer1.Panel1.Controls.Add(this.btnRefresh);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.cbxServices);
            this.splitContainer1.Panel1.Controls.Add(this.gridPanel);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.btnTest);
            this.splitContainer1.Panel1.Controls.Add(this.cbxMethods);
            this.splitContainer1.Panel1.Controls.Add(this.btnLoad);
            this.splitContainer1.Panel1.Controls.Add(this.txtWsdlUrl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupResult);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(628, 658);
            this.splitContainer1.SplitterDistance = 329;
            this.splitContainer1.TabIndex = 15;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(6, 10);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(64, 13);
            this.linkLabel1.TabIndex = 29;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "WSDL URL";
            this.linkLabel1.LinkClicked +=
                new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Location = new System.Drawing.Point(534, 77);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(88, 23);
            this.btnSaveFile.TabIndex = 28;
            this.btnSaveFile.Text = "Save File";
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(534, 41);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(88, 23);
            this.btnLoadFile.TabIndex = 27;
            this.btnLoadFile.Text = "Load File";
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(438, 77);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(88, 23);
            this.btnRefresh.TabIndex = 26;
            this.btnRefresh.Text = "Refresh Grids";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 23);
            this.label2.TabIndex = 23;
            this.label2.Text = "Service";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbxServices
            // 
            this.cbxServices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxServices.Location = new System.Drawing.Point(86, 37);
            this.cbxServices.Name = "cbxServices";
            this.cbxServices.Size = new System.Drawing.Size(336, 21);
            this.cbxServices.TabIndex = 22;
            this.cbxServices.SelectedIndexChanged += new System.EventHandler(this.cbxServices_SelectedIndexChanged);
            // 
            // gridPanel
            // 
            this.gridPanel.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.gridPanel.Location = new System.Drawing.Point(6, 109);
            this.gridPanel.Name = "gridPanel";
            this.gridPanel.Size = new System.Drawing.Size(612, 208);
            this.gridPanel.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 23);
            this.label1.TabIndex = 20;
            this.label1.Text = "Method";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(438, 41);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(88, 23);
            this.btnTest.TabIndex = 19;
            this.btnTest.Text = "Test Method";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // cbxMethods
            // 
            this.cbxMethods.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMethods.Location = new System.Drawing.Point(86, 69);
            this.cbxMethods.Name = "cbxMethods";
            this.cbxMethods.Size = new System.Drawing.Size(336, 21);
            this.cbxMethods.TabIndex = 18;
            this.cbxMethods.SelectedIndexChanged += new System.EventHandler(this.cbxMethods_SelectedIndexChanged);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(438, 5);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(88, 23);
            this.btnLoad.TabIndex = 17;
            this.btnLoad.Text = "Load Url";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // txtWsdlUrl
            // 
            this.txtWsdlUrl.Location = new System.Drawing.Point(86, 5);
            this.txtWsdlUrl.Name = "txtWsdlUrl";
            this.txtWsdlUrl.Size = new System.Drawing.Size(336, 20);
            this.txtWsdlUrl.TabIndex = 15;
            this.txtWsdlUrl.Text = "http://currentodinservices.ef.com/ParentsService/";
            // 
            // groupResult
            // 
            this.groupResult.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.groupResult.Location = new System.Drawing.Point(9, 3);
            this.groupResult.Name = "groupResult";
            this.groupResult.Size = new System.Drawing.Size(607, 168);
            this.groupResult.TabIndex = 8;
            this.groupResult.TabStop = false;
            this.groupResult.Text = "Result";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtSoap);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 177);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(628, 148);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Soap Message Log";
            // 
            // txtSoap
            // 
            this.txtSoap.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSoap.Location = new System.Drawing.Point(8, 16);
            this.txtSoap.Multiline = true;
            this.txtSoap.Name = "txtSoap";
            this.txtSoap.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSoap.Size = new System.Drawing.Size(612, 120);
            this.txtSoap.TabIndex = 0;
            // 
            // cbxRecentUrls
            // 
            this.cbxRecentUrls.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                 (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                   | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRecentUrls.DropDownWidth = 336;
            this.cbxRecentUrls.FormattingEnabled = true;
            this.cbxRecentUrls.Location = new System.Drawing.Point(536, 7);
            this.cbxRecentUrls.Name = "cbxRecentUrls";
            this.cbxRecentUrls.Size = new System.Drawing.Size(82, 21);
            this.cbxRecentUrls.TabIndex = 0;
            this.cbxRecentUrls.Text = "Recent Srvcs";
            this.cbxRecentUrls.SelectedIndexChanged += new System.EventHandler(this.cbxRecentUrls_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(628, 658);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "wizdl - Web Service GUI";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://e1portal.ef.com/Odin/IT%20Technical%20Wiki/Environments.aspx");
        }

        private void cbxRecentUrls_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtWsdlUrl.Text = cbxRecentUrls.Text;
            btnLoad_Click(null, null);
        }

        private void LoadUrlsFromCache()
        {
            if (File.Exists(path))
            {
                var urls = File.ReadAllLines(path);

                foreach (string url in urls)
                {
                    cbxRecentUrls.Items.Add(url);
                }
            }
        }

        private void AddToUrlCache(string s)
        {
            if (!cbxRecentUrls.Items.Contains(txtWsdlUrl.Text))
            {
                cbxRecentUrls.Items.Add(txtWsdlUrl.Text);
            }

            string urls = cbxRecentUrls.Items.Cast<object>()
                                       .Aggregate("", (current, url) => current + (url + Environment.NewLine));

            File.WriteAllText(path, urls);
        }
    }
}