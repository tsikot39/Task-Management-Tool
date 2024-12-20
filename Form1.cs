using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private string loggedInUsername;

        // Constructor to accept the username
        public Form1(string username)
        {
            InitializeComponent();
            InitializeConnection();  // Initialize and open the connection
            LoadTasks();  // Load tasks on form load

            lblUsername.Text = "Welcome, " + username;  // Display the username
            loggedInUsername = username;  // Store the logged-in user's username

            // Initially disable the Edit and Delete buttons
            btnEditTask.Enabled = false;
            btnDeleteTask.Enabled = false;
            btnMarkCompleted.Enabled = false;

            // Subscribe to the DataGridView's SelectionChanged event
            tasksDataGridView.SelectionChanged += tasksDataGridView_SelectionChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;


            // Populate the filterComboBox with values
            filterComboBox.Items.Add("All");
            filterComboBox.Items.Add("Pending");
            filterComboBox.Items.Add("In Progress");
            filterComboBox.Items.Add("Completed");
            filterComboBox.Items.Add("On Hold");
            filterComboBox.Items.Add("Cancelled");

            filterComboBox.SelectedIndex = 0;  // Set default to "All"
            filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;

            tasksDataGridView.AllowUserToResizeRows = false;
        }

        // Initialize the connection to the database
        private void InitializeConnection()
        {
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

        private void LoadTasks(string statusFilter = "All", string titleFilter = "")
        {
            if (connection.State == System.Data.ConnectionState.Closed || connection.State == System.Data.ConnectionState.Broken)
            {
                connection.Open();
            }

            string query = "SELECT * FROM Tasks WHERE 1=1"; // Always true, used for appending filters dynamically

            if (statusFilter != "All")
            {
                query += " AND Status = @Status"; // Add filtering for task status
            }

            if (!string.IsNullOrWhiteSpace(titleFilter))
            {
                query += " AND Title LIKE @Title"; // Add filtering for title
            }

            query += " ORDER BY DueDate ASC";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                if (statusFilter != "All")
                {
                    cmd.Parameters.AddWithValue("@Status", statusFilter);
                }

                if (!string.IsNullOrWhiteSpace(titleFilter))
                {
                    cmd.Parameters.AddWithValue("@Title", $"%{titleFilter}%");
                }

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    tasksDataGridView.Rows.Clear();
                    while (reader.Read())
                    {
                        int rowIndex = tasksDataGridView.Rows.Add(reader["TaskID"], reader["Title"], reader["Description"], reader["Priority"], reader["Status"], Convert.ToDateTime(reader["DueDate"]).ToString("MM/dd/yyyy"));

                        DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                        string status = reader["Status"].ToString();

                        // Apply colors only if the task is not completed
                        if (status != "Completed")
                        {
                            if (DateTime.Today > dueDate)
                            {
                                tasksDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.Red;  // Red for overdue tasks
                            }
                            else if ((dueDate - DateTime.Today).Days <= 3 && (dueDate - DateTime.Today).Days > 0)
                            {
                                tasksDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.Orange;  // Orange for tasks due in the next 3 days
                            }
                        }
                    }
                }
            }

            tasksDataGridView.ClearSelection();
            tasksDataGridView.CurrentCell = null;
        }


        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedStatus = filterComboBox.SelectedItem.ToString();
            string titleFilter = txtSearch.Text;
            LoadTasks(selectedStatus, titleFilter);
        }


        private void tasksDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (tasksDataGridView.SelectedRows.Count > 0 && tasksDataGridView.SelectedRows[0].Cells[0].Value != null)
            {
                btnEditTask.Enabled = true;
                btnDeleteTask.Enabled = true;
                btnMarkCompleted.Enabled = true;
            }
            else
            {
                btnEditTask.Enabled = false;
                btnDeleteTask.Enabled = false;
                btnMarkCompleted.Enabled = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tasksDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            
            tasksDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            tasksDataGridView.Columns[0].Visible = false;

            tasksDataGridView.ClearSelection();
            tasksDataGridView.CurrentCell = null;

            // Set no selection after the form has loaded completely
            this.BeginInvoke((MethodInvoker)delegate
            {
                tasksDataGridView.ClearSelection();
                tasksDataGridView.CurrentCell = null;
            });

            lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;

            if (tasksDataGridView.Columns["Duedate"] != null)  // Ensure the column exists
            {
                tasksDataGridView.Columns["Duedate"].HeaderText = "Due Date";
            }

            lblUsername.AutoSize = true;
            lblSeparator.AutoSize = true;


            // Position lblUsername near the right edge
            lblUsername.Left = this.ClientSize.Width - lblUsername.Width - lblSeparator.Width - linkLogout.Width - 20;

            // Position lblSeparator right next to lblUsername
            lblSeparator.Left = lblUsername.Left + lblUsername.Width;
            lblSeparator.Top = lblUsername.Top; // Align vertically with lblUsername

            // Position linkLogout next to lblSeparator
            linkLogout.Left = lblSeparator.Left + lblSeparator.Width;
            linkLogout.Top = lblUsername.Top; // Align vertically with lblUsername
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;
            lblFilter.Left = (this.ClientSize.Width - lblFilter.Width) / 2;
            filterComboBox.Left = (this.ClientSize.Width - filterComboBox.Width) / 2;
        }

        private void linkLogout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();  // Close the current form to trigger the LoginForm to show
        }

        private void btnAddTask_Click(object sender, EventArgs e)
        {
            TaskForm taskForm = new TaskForm();
            taskForm.ShowDialog();
            LoadTasks(filterComboBox.SelectedItem.ToString());
        }

        private void btnEditTask_Click_2(object sender, EventArgs e)
        {
            if (tasksDataGridView.SelectedRows.Count > 0)
            {
                int taskId = Convert.ToInt32(tasksDataGridView.SelectedRows[0].Cells[0].Value);
                TaskForm taskForm = new TaskForm(taskId);
                taskForm.ShowDialog();
                LoadTasks(filterComboBox.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Please select a task to edit.");
            }
        }

        private void btnDeleteTask_Click_1(object sender, EventArgs e)
        {
            if (tasksDataGridView.SelectedRows.Count > 0)
            {
                int taskId = Convert.ToInt32(tasksDataGridView.SelectedRows[0].Cells[0].Value);

                DialogResult result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM Tasks WHERE TaskID = @TaskID";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@TaskID", taskId);
                    cmd.ExecuteNonQuery();
                    LoadTasks(filterComboBox.SelectedItem.ToString());
                }
            }
            else
            {
                MessageBox.Show("Please select a task to delete.");
            }
        }

        private void btnMarkCompleted_Click(object sender, EventArgs e)
        {
            if (tasksDataGridView.SelectedRows.Count > 0)
            {
                int taskId = Convert.ToInt32(tasksDataGridView.SelectedRows[0].Cells[0].Value);

                DialogResult result = MessageBox.Show("Are you sure you want to mark this task as completed?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string query = "UPDATE Tasks SET Status = 'Completed' WHERE TaskID = @TaskID";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@TaskID", taskId);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Task marked as completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadTasks(filterComboBox.SelectedItem.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating task status: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a task to mark as completed.", "No Task Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkLogout_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoginForm login = new LoginForm();
            login.Show();
            this.Close();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string selectedStatus = filterComboBox.SelectedItem.ToString();
            string titleFilter = txtSearch.Text;
            LoadTasks(selectedStatus, titleFilter);
        }


    }
}
