namespace PPICancerRecognitionProject
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private System.Windows.Forms.ListBox lstPatients;
        private System.Windows.Forms.Button btnAddPatient;
        private System.Windows.Forms.Button btnDeletePatient;
        private System.Windows.Forms.ListBox lstScans;
        private System.Windows.Forms.PictureBox picOriginal;
        private System.Windows.Forms.PictureBox picAIResult;
        private System.Windows.Forms.Label lblAIInfo;
        private System.Windows.Forms.Button btnUploadScan;
        private System.Windows.Forms.Button btnGenerateAI;
        private System.Windows.Forms.Label lblPatients;
        private System.Windows.Forms.Label lblScans;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelLeft = new System.Windows.Forms.Panel();
            lblPatients = new System.Windows.Forms.Label();
            lstPatients = new System.Windows.Forms.ListBox();
            btnAddPatient = new System.Windows.Forms.Button();
            btnDeletePatient = new System.Windows.Forms.Button();
            panelRight = new System.Windows.Forms.Panel();
            lblScans = new System.Windows.Forms.Label();
            lstScans = new System.Windows.Forms.ListBox();
            btnUploadScan = new System.Windows.Forms.Button();
            btnGenerateAI = new System.Windows.Forms.Button();
            picOriginal = new System.Windows.Forms.PictureBox();
            picAIResult = new System.Windows.Forms.PictureBox();
            lblAIInfo = new System.Windows.Forms.Label();
            btnExportData = new System.Windows.Forms.Button();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picOriginal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAIResult).BeginInit();
            SuspendLayout();
            // 
            // panelLeft
            // 
            panelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));
            panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)((byte)240)), ((int)((byte)240)), ((int)((byte)240)));
            panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelLeft.Controls.Add(lblPatients);
            panelLeft.Controls.Add(lstPatients);
            panelLeft.Controls.Add(btnAddPatient);
            panelLeft.Controls.Add(btnDeletePatient);
            panelLeft.Location = new System.Drawing.Point(10, 10);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new System.Drawing.Size(280, 730);
            panelLeft.TabIndex = 0;
            // 
            // lblPatients
            // 
            lblPatients.AutoSize = true;
            lblPatients.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblPatients.Location = new System.Drawing.Point(10, 10);
            lblPatients.Name = "lblPatients";
            lblPatients.Size = new System.Drawing.Size(123, 28);
            lblPatients.TabIndex = 0;
            lblPatients.Text = "👩‍⚕️ Patients";
            // 
            // lstPatients
            // 
            lstPatients.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstPatients.Font = new System.Drawing.Font("Segoe UI", 10F);
            lstPatients.ItemHeight = 23;
            lstPatients.Location = new System.Drawing.Point(10, 40);
            lstPatients.Name = "lstPatients";
            lstPatients.Size = new System.Drawing.Size(255, 600);
            lstPatients.TabIndex = 1;
            lstPatients.Click += lstPatients_SelectedIndexChanged;
            // 
            // btnAddPatient
            // 
            btnAddPatient.BackColor = System.Drawing.Color.FromArgb(((int)((byte)210)), ((int)((byte)250)), ((int)((byte)210)));
            btnAddPatient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddPatient.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnAddPatient.Location = new System.Drawing.Point(10, 650);
            btnAddPatient.Name = "btnAddPatient";
            btnAddPatient.Size = new System.Drawing.Size(120, 35);
            btnAddPatient.TabIndex = 2;
            btnAddPatient.Text = "➕ Add Patient";
            btnAddPatient.UseVisualStyleBackColor = false;
            btnAddPatient.Click += btnAddPatient_Click;
            // 
            // btnDeletePatient
            // 
            btnDeletePatient.BackColor = System.Drawing.Color.FromArgb(((int)((byte)255)), ((int)((byte)220)), ((int)((byte)220)));
            btnDeletePatient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeletePatient.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnDeletePatient.Location = new System.Drawing.Point(145, 650);
            btnDeletePatient.Name = "btnDeletePatient";
            btnDeletePatient.Size = new System.Drawing.Size(120, 35);
            btnDeletePatient.TabIndex = 3;
            btnDeletePatient.Text = "🗑️ Delete";
            btnDeletePatient.UseVisualStyleBackColor = false;
            btnDeletePatient.Click += btnDeletePatient_Click;
            // 
            // panelRight
            // 
            panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            panelRight.Controls.Add(btnExportData);
            panelRight.Controls.Add(lblScans);
            panelRight.Controls.Add(lstScans);
            panelRight.Controls.Add(btnUploadScan);
            panelRight.Controls.Add(btnGenerateAI);
            panelRight.Controls.Add(picOriginal);
            panelRight.Controls.Add(picAIResult);
            panelRight.Controls.Add(lblAIInfo);
            panelRight.Location = new System.Drawing.Point(300, 10);
            panelRight.Name = "panelRight";
            panelRight.Size = new System.Drawing.Size(980, 730);
            panelRight.TabIndex = 1;
            // 
            // lblScans
            // 
            lblScans.AutoSize = true;
            lblScans.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblScans.Location = new System.Drawing.Point(0, 0);
            lblScans.Name = "lblScans";
            lblScans.Size = new System.Drawing.Size(65, 28);
            lblScans.TabIndex = 0;
            lblScans.Text = "Scans";
            // 
            // lstScans
            // 
            lstScans.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstScans.Font = new System.Drawing.Font("Segoe UI", 10F);
            lstScans.ItemHeight = 23;
            lstScans.Location = new System.Drawing.Point(0, 35);
            lstScans.Name = "lstScans";
            lstScans.Size = new System.Drawing.Size(350, 186);
            lstScans.TabIndex = 1;
            lstScans.Click += lstScans_SelectedIndexChanged;
            // 
            // btnUploadScan
            // 
            btnUploadScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnUploadScan.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnUploadScan.Location = new System.Drawing.Point(370, 35);
            btnUploadScan.Name = "btnUploadScan";
            btnUploadScan.Size = new System.Drawing.Size(140, 35);
            btnUploadScan.TabIndex = 2;
            btnUploadScan.Text = "📤 Upload Scan";
            btnUploadScan.Click += btnUploadScan_Click;
            // 
            // btnGenerateAI
            // 
            btnGenerateAI.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnGenerateAI.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnGenerateAI.Location = new System.Drawing.Point(520, 35);
            btnGenerateAI.Name = "btnGenerateAI";
            btnGenerateAI.Size = new System.Drawing.Size(180, 35);
            btnGenerateAI.TabIndex = 3;
            btnGenerateAI.Text = "⚙️ Generate AI Result";
            btnGenerateAI.Visible = false;
            btnGenerateAI.Click += btnGenerateAI_Click;
            // 
            // picOriginal
            // 
            picOriginal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picOriginal.Location = new System.Drawing.Point(0, 260);
            picOriginal.Name = "picOriginal";
            picOriginal.Size = new System.Drawing.Size(450, 400);
            picOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picOriginal.TabIndex = 4;
            picOriginal.TabStop = false;
            // 
            // picAIResult
            // 
            picAIResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picAIResult.Location = new System.Drawing.Point(500, 260);
            picAIResult.Name = "picAIResult";
            picAIResult.Size = new System.Drawing.Size(450, 400);
            picAIResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picAIResult.TabIndex = 5;
            picAIResult.TabStop = false;
            // 
            // lblAIInfo
            // 
            lblAIInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            lblAIInfo.Location = new System.Drawing.Point(0, 670);
            lblAIInfo.Name = "lblAIInfo";
            lblAIInfo.Size = new System.Drawing.Size(950, 50);
            lblAIInfo.TabIndex = 6;
            lblAIInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExportData
            // 
            btnExportData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnExportData.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnExportData.Location = new System.Drawing.Point(370, 88);
            btnExportData.Name = "btnExportData";
            btnExportData.Size = new System.Drawing.Size(180, 35);
            btnExportData.TabIndex = 7;
            btnExportData.Text = "📄Export Patient Data";
            btnExportData.UseMnemonic = false;
            btnExportData.Visible = false;
            btnExportData.Click += btnExportPdf_Click;
            // 
            // Form1
            // 
            BackColor = System.Drawing.Color.WhiteSmoke;
            ClientSize = new System.Drawing.Size(1300, 750);
            Controls.Add(panelLeft);
            Controls.Add(panelRight);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "PPI Cancer Recognition – Demo UI";
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picOriginal).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAIResult).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnExportData;

        #endregion
    }
}
