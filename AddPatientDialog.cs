using System;
using System.Windows.Forms;
using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;

namespace PPICancerRecognitionProject
{
    public class AddPatientDialog : Form
    {
        private readonly PatientRepository _repo;
        private TextBox txtFirst;
        private TextBox txtLast;
        private DateTimePicker dtpDOB;
        private Button btnSave;

        public AddPatientDialog(PatientRepository repo)
        {
            _repo = repo;
            Text = "Add Patient";
            Size = new System.Drawing.Size(300, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            Label lbl1 = new Label { Text = "First Name:", Top = 20, Left = 20, Width = 100 };
            txtFirst = new TextBox { Top = 50, Left = 20, Width = 200 };

            Label lbl2 = new Label { Text = "Last Name:", Top = 80, Left = 20, Width = 100 };
            txtLast = new TextBox { Top = 110, Left = 20, Width = 200 };

            Label lbl3 = new Label { Text = "Date of Birth:", Top = 140, Left = 20, Width = 100 };
            dtpDOB = new DateTimePicker { Top = 170, Left = 20, Width = 200 };

            btnSave = new Button { Text = "Save", Top = 200, Left = 20, Width = 200, Height = 25};
            btnSave.Click += BtnSave_Click;

            Controls.Add(lbl1);
            Controls.Add(txtFirst);
            Controls.Add(lbl2);
            Controls.Add(txtLast);
            Controls.Add(lbl3);
            Controls.Add(dtpDOB);
            Controls.Add(btnSave);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirst.Text) || string.IsNullOrWhiteSpace(txtLast.Text))
            {
                MessageBox.Show("Please fill in first and last name.");
                return;
            }

            var p = new Patient
            {
                FirstName = txtFirst.Text,
                LastName = txtLast.Text,
                DateOfBirth = dtpDOB.Value
            };

            _repo.Add(p);
            MessageBox.Show("Patient added successfully!");
            Close();
        }
    }
}
