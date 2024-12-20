using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace WindowsFormsApp1
{
    public partial class TaskForm : Form
    {
        private SqlConnection connection;
        private int? taskId = null;  // Optional, for editing an existing task

        public TaskForm(int? taskId = null)
        {
            InitializeComponent();
            this.taskId = taskId;

            // Add status options to the combobox
            comboStatus.Items.Add("Pending");
            comboStatus.Items.Add("In Progress");
            comboStatus.Items.Add("Completed");
            comboStatus.Items.Add("On Hold");
            comboStatus.Items.Add("Cancelled");

            // Retrieve the connection string from app.config
            string connectionString = ConfigurationManager.ConnectionStrings["TaskManagementDB"].ConnectionString;
            connection = new SqlConnection(connectionString);

            // Open the connection
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to the database: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (taskId.HasValue)
            {
                LoadTask(taskId.Value);  // Load task details for editing
            }
        }

        private void LoadTask(int taskId)
        {
            string query = "SELECT * FROM Tasks WHERE TaskID = @TaskID";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@TaskID", taskId);
            SqlDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtTitle.Text = reader["Title"].ToString();
                    txtDescription.Text = reader["Description"].ToString();
                    comboPriority.Text = reader["Priority"].ToString();
                    comboStatus.Text = reader["Status"].ToString();
                    dateTimeDueDate.Value = Convert.ToDateTime(reader["DueDate"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading task: " + ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
        }

        private void TaskForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnSaveTask_Click(object sender, EventArgs e)
        {
            // Validate Title
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Title is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate Description
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Description is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate Priority
            if (string.IsNullOrWhiteSpace(comboPriority.Text))
            {
                MessageBox.Show("Priority is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate Status
            if (string.IsNullOrWhiteSpace(comboStatus.Text))
            {
                MessageBox.Show("Status is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate Due Date
            if (dateTimeDueDate.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Due Date is invalid. Please select a date that is today or in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query;
            SqlCommand cmd = new SqlCommand();

            if (taskId.HasValue)  // Edit existing task
            {
                query = "UPDATE Tasks SET Title = @Title, Description = @Description, Priority = @Priority, Status = @Status, DueDate = @DueDate WHERE TaskID = @TaskID";
                cmd.Parameters.AddWithValue("@TaskID", taskId);
            }
            else  // Add new task
            {
                query = "INSERT INTO Tasks (Title, Description, Priority, Status, DueDate) VALUES (@Title, @Description, @Priority, @Status, @DueDate)";
            }

            cmd.CommandText = query;
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
            cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
            cmd.Parameters.AddWithValue("@Priority", comboPriority.Text);
            cmd.Parameters.AddWithValue("@Status", comboStatus.Text); // Include Status in the query
            cmd.Parameters.AddWithValue("@DueDate", dateTimeDueDate.Value);

            try
            {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Task saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();  // Close the form after saving
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving task: " + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void TaskForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Close the connection when the form is closed
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
