namespace SOLTIUS_Scheduler_Add_On.UI
{
    partial class ManageProfile
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageProfile));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSBO = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtSAPDBPass = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSAPDBUser = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSAPDBPort = new System.Windows.Forms.TextBox();
            this.txtSAPDBServer = new System.Windows.Forms.TextBox();
            this.cboServerType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtSAPLicenseServer = new System.Windows.Forms.TextBox();
            this.txtSAPPass = new System.Windows.Forms.TextBox();
            this.txtSAPUser = new System.Windows.Forms.TextBox();
            this.txtSAPDatabase = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tabDB = new System.Windows.Forms.TabPage();
            this.label16 = new System.Windows.Forms.Label();
            this.txtWebApiUrl = new System.Windows.Forms.TextBox();
            this.txtDBPass = new System.Windows.Forms.TextBox();
            this.cboDBType = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtDBUser = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtDBServer = new System.Windows.Forms.TextBox();
            this.txtDBName = new System.Windows.Forms.TextBox();
            this.txtDBPort = new System.Windows.Forms.TextBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabSBO.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabDB.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(122, 18);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(216, 22);
            this.textBox1.TabIndex = 27;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(28, 18);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(86, 16);
            this.label25.TabIndex = 28;
            this.label25.Text = "Profile Config";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabSBO);
            this.tabControl.Controls.Add(this.tabDB);
            this.tabControl.Location = new System.Drawing.Point(16, 95);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(516, 440);
            this.tabControl.TabIndex = 29;
            // 
            // tabSBO
            // 
            this.tabSBO.Controls.Add(this.groupBox1);
            this.tabSBO.Controls.Add(this.groupBox2);
            this.tabSBO.Location = new System.Drawing.Point(4, 25);
            this.tabSBO.Margin = new System.Windows.Forms.Padding(4);
            this.tabSBO.Name = "tabSBO";
            this.tabSBO.Padding = new System.Windows.Forms.Padding(4);
            this.tabSBO.Size = new System.Drawing.Size(508, 411);
            this.tabSBO.TabIndex = 0;
            this.tabSBO.Text = "SAP Business One";
            this.tabSBO.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtSAPDBPass);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtSAPDBUser);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtSAPDBPort);
            this.groupBox1.Controls.Add(this.txtSAPDBServer);
            this.groupBox1.Controls.Add(this.cboServerType);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(8, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(487, 196);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database";
            // 
            // txtSAPDBPass
            // 
            this.txtSAPDBPass.Location = new System.Drawing.Point(152, 153);
            this.txtSAPDBPass.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPDBPass.Name = "txtSAPDBPass";
            this.txtSAPDBPass.PasswordChar = '*';
            this.txtSAPDBPass.Size = new System.Drawing.Size(325, 22);
            this.txtSAPDBPass.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 156);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Password";
            // 
            // txtSAPDBUser
            // 
            this.txtSAPDBUser.Location = new System.Drawing.Point(152, 121);
            this.txtSAPDBUser.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPDBUser.Name = "txtSAPDBUser";
            this.txtSAPDBUser.Size = new System.Drawing.Size(325, 22);
            this.txtSAPDBUser.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 124);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "User Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 92);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 60);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Server";
            // 
            // txtSAPDBPort
            // 
            this.txtSAPDBPort.Location = new System.Drawing.Point(152, 89);
            this.txtSAPDBPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPDBPort.Name = "txtSAPDBPort";
            this.txtSAPDBPort.Size = new System.Drawing.Size(325, 22);
            this.txtSAPDBPort.TabIndex = 7;
            // 
            // txtSAPDBServer
            // 
            this.txtSAPDBServer.Location = new System.Drawing.Point(152, 57);
            this.txtSAPDBServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPDBServer.Name = "txtSAPDBServer";
            this.txtSAPDBServer.Size = new System.Drawing.Size(325, 22);
            this.txtSAPDBServer.TabIndex = 6;
            // 
            // cboServerType
            // 
            this.cboServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboServerType.FormattingEnabled = true;
            this.cboServerType.Items.AddRange(new object[] {
            "SQL 2005",
            "SQL 2008",
            "SQL 2012",
            "SQL 2014",
            "SQL 2016",
            "SQL 2017",
            "SQL 2019",
            "HANA"});
            this.cboServerType.Location = new System.Drawing.Point(152, 23);
            this.cboServerType.Margin = new System.Windows.Forms.Padding(4);
            this.cboServerType.Name = "cboServerType";
            this.cboServerType.Size = new System.Drawing.Size(325, 24);
            this.cboServerType.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server Type";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtSAPLicenseServer);
            this.groupBox2.Controls.Add(this.txtSAPPass);
            this.groupBox2.Controls.Add(this.txtSAPUser);
            this.groupBox2.Controls.Add(this.txtSAPDatabase);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(8, 210);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(487, 162);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SAP Business One";
            // 
            // txtSAPLicenseServer
            // 
            this.txtSAPLicenseServer.Location = new System.Drawing.Point(152, 119);
            this.txtSAPLicenseServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPLicenseServer.Name = "txtSAPLicenseServer";
            this.txtSAPLicenseServer.Size = new System.Drawing.Size(325, 22);
            this.txtSAPLicenseServer.TabIndex = 13;
            // 
            // txtSAPPass
            // 
            this.txtSAPPass.Location = new System.Drawing.Point(152, 87);
            this.txtSAPPass.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPPass.Name = "txtSAPPass";
            this.txtSAPPass.PasswordChar = '*';
            this.txtSAPPass.Size = new System.Drawing.Size(325, 22);
            this.txtSAPPass.TabIndex = 12;
            // 
            // txtSAPUser
            // 
            this.txtSAPUser.Location = new System.Drawing.Point(152, 55);
            this.txtSAPUser.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPUser.Name = "txtSAPUser";
            this.txtSAPUser.Size = new System.Drawing.Size(325, 22);
            this.txtSAPUser.TabIndex = 11;
            // 
            // txtSAPDatabase
            // 
            this.txtSAPDatabase.Location = new System.Drawing.Point(152, 23);
            this.txtSAPDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.txtSAPDatabase.Name = "txtSAPDatabase";
            this.txtSAPDatabase.Size = new System.Drawing.Size(325, 22);
            this.txtSAPDatabase.TabIndex = 10;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 123);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(97, 16);
            this.label9.TabIndex = 3;
            this.label9.Text = "License Server";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 91);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 16);
            this.label8.TabIndex = 2;
            this.label8.Text = "Password";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 59);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 16);
            this.label7.TabIndex = 1;
            this.label7.Text = "User Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 27);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 16);
            this.label6.TabIndex = 0;
            this.label6.Text = "Database";
            // 
            // tabDB
            // 
            this.tabDB.Controls.Add(this.txtDBPass);
            this.tabDB.Controls.Add(this.label16);
            this.tabDB.Controls.Add(this.txtWebApiUrl);
            this.tabDB.Controls.Add(this.cboDBType);
            this.tabDB.Controls.Add(this.label15);
            this.tabDB.Controls.Add(this.txtDBUser);
            this.tabDB.Controls.Add(this.label14);
            this.tabDB.Controls.Add(this.label13);
            this.tabDB.Controls.Add(this.label12);
            this.tabDB.Controls.Add(this.label11);
            this.tabDB.Controls.Add(this.label10);
            this.tabDB.Controls.Add(this.txtDBServer);
            this.tabDB.Controls.Add(this.txtDBName);
            this.tabDB.Controls.Add(this.txtDBPort);
            this.tabDB.Location = new System.Drawing.Point(4, 25);
            this.tabDB.Margin = new System.Windows.Forms.Padding(4);
            this.tabDB.Name = "tabDB";
            this.tabDB.Padding = new System.Windows.Forms.Padding(4);
            this.tabDB.Size = new System.Drawing.Size(508, 411);
            this.tabDB.TabIndex = 1;
            this.tabDB.Text = "Database Staging";
            this.tabDB.UseVisualStyleBackColor = true;
            // 
            // txtDBPass
            // 
            this.txtDBPass.Location = new System.Drawing.Point(129, 169);
            this.txtDBPass.Margin = new System.Windows.Forms.Padding(4);
            this.txtDBPass.Name = "txtDBPass";
            this.txtDBPass.Size = new System.Drawing.Size(367, 22);
            this.txtDBPass.TabIndex = 26;
            this.txtDBPass.UseSystemPasswordChar = true;
            // 
            // cboDBType
            // 
            this.cboDBType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDBType.FormattingEnabled = true;
            this.cboDBType.Items.AddRange(new object[] {
            "MySQL",
            "SQLServer"});
            this.cboDBType.Location = new System.Drawing.Point(129, 7);
            this.cboDBType.Margin = new System.Windows.Forms.Padding(4);
            this.cboDBType.Name = "cboDBType";
            this.cboDBType.Size = new System.Drawing.Size(367, 24);
            this.cboDBType.TabIndex = 21;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 172);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(67, 16);
            this.label15.TabIndex = 5;
            this.label15.Text = "Password";
            // 
            // txtDBUser
            // 
            this.txtDBUser.Location = new System.Drawing.Point(129, 137);
            this.txtDBUser.Margin = new System.Windows.Forms.Padding(4);
            this.txtDBUser.Name = "txtDBUser";
            this.txtDBUser.Size = new System.Drawing.Size(367, 22);
            this.txtDBUser.TabIndex = 25;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 140);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(76, 16);
            this.label14.TabIndex = 4;
            this.label14.Text = "User Name";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 108);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(67, 16);
            this.label13.TabIndex = 3;
            this.label13.Text = "Database";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 76);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(31, 16);
            this.label12.TabIndex = 2;
            this.label12.Text = "Port";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 44);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 16);
            this.label11.TabIndex = 1;
            this.label11.Text = "Server";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 11);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 16);
            this.label10.TabIndex = 0;
            this.label10.Text = "Type";
            // 
            // txtDBServer
            // 
            this.txtDBServer.Location = new System.Drawing.Point(129, 41);
            this.txtDBServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtDBServer.Name = "txtDBServer";
            this.txtDBServer.Size = new System.Drawing.Size(367, 22);
            this.txtDBServer.TabIndex = 22;
            // 
            // txtDBName
            // 
            this.txtDBName.Location = new System.Drawing.Point(129, 105);
            this.txtDBName.Margin = new System.Windows.Forms.Padding(4);
            this.txtDBName.Name = "txtDBName";
            this.txtDBName.Size = new System.Drawing.Size(367, 22);
            this.txtDBName.TabIndex = 24;
            // 
            // txtDBPort
            // 
            this.txtDBPort.Location = new System.Drawing.Point(129, 73);
            this.txtDBPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtDBPort.Name = "txtDBPort";
            this.txtDBPort.Size = new System.Drawing.Size(367, 22);
            this.txtDBPort.TabIndex = 23;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 208);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(86, 16);
            this.label16.TabIndex = 34;
            this.label16.Text = "Web API URL";
            // 
            // txtWebApiUrl
            // 
            this.txtWebApiUrl.Location = new System.Drawing.Point(129, 205);
            this.txtWebApiUrl.Margin = new System.Windows.Forms.Padding(4);
            this.txtWebApiUrl.Name = "txtWebApiUrl";
            this.txtWebApiUrl.Size = new System.Drawing.Size(367, 22);
            this.txtWebApiUrl.TabIndex = 35;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(357, 16);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(92, 20);
            this.radioButton1.TabIndex = 30;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Production";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(357, 68);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(73, 20);
            this.radioButton3.TabIndex = 32;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Testing";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(357, 42);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(109, 20);
            this.radioButton2.TabIndex = 31;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Development";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(404, 538);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(124, 38);
            this.button1.TabIndex = 33;
            this.button1.Text = "Update";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // ManageProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 588);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManageProfile";
            this.Text = "Manage Profile ";
            this.tabControl.ResumeLayout(false);
            this.tabSBO.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabDB.ResumeLayout(false);
            this.tabDB.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSBO;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtSAPDBPass;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSAPDBUser;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSAPDBPort;
        private System.Windows.Forms.TextBox txtSAPDBServer;
        private System.Windows.Forms.ComboBox cboServerType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtSAPLicenseServer;
        private System.Windows.Forms.TextBox txtSAPPass;
        private System.Windows.Forms.TextBox txtSAPUser;
        private System.Windows.Forms.TextBox txtSAPDatabase;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabDB;
        private System.Windows.Forms.TextBox txtDBPass;
        private System.Windows.Forms.ComboBox cboDBType;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtDBUser;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtDBServer;
        private System.Windows.Forms.TextBox txtDBName;
        private System.Windows.Forms.TextBox txtDBPort;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtWebApiUrl;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button button1;
    }
}