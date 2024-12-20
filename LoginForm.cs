using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private SqlConnection connection;

        public LoginForm()
        {
            InitializeComponent();
            InitializeConnection();  // Initialize the connection to the database
        }

        // Initialize the connection to the database
        private void InitializeConnection()
        {
            // Retrieve the connection string from app.config
            string connectionString = ConfigurationManager.ConnectionStrings["TaskManagementDB"].ConnectionString;
            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to the database: " + ex.Message);
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;
            lblUserLogin.Left = (this.ClientSize.Width - lblUserLogin.Width) / 2;
            btnLogin.Left = (this.ClientSize.Width - btnLogin.Width) / 2;
            linkRegister.Left = (this.ClientSize.Width - linkRegister.Width) / 2;
        }

        private void LoginForm_Resize(object sender, EventArgs e)
        {
            lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;
            lblUserLogin.Left = (this.ClientSize.Width - lblUserLogin.Width) / 2;
            btnLogin.Left = (this.ClientSize.Width - btnLogin.Width) / 2;
            linkRegister.Left = (this.ClientSize.Width - linkRegister.Width) / 2;
        }

        // Hash the password using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha256.ComputeHash(passwordBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashedBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username/Email is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string query = "SELECT Password FROM Users WHERE Username = @Username";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string storedHashedPassword = result.ToString();

                    // Verify the entered password against the hashed password in the database
                    if (BCrypt.Net.BCrypt.Verify(txtPassword.Text, storedHashedPassword))
                    {
                        // Successful login
                        Form1 mainForm = new Form1(txtUsername.Text);  // Pass username to Form1
                        this.Hide();  // Hide LoginForm when showing Form1
                        mainForm.ShowDialog();  // Show Form1 modal
                        this.Show();  // Show LoginForm again only after Form1 is closed
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during login: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ChangePassword changePasswordForm = new ChangePassword();
            changePasswordForm.Show();
            this.Hide();
        }
    }
}
