using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using MaterialSkin;
using MaterialSkin.Controls;

namespace Recording_of_requests_for_equipment_repair
{
    public partial class EmployeeForm : MaterialForm
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        private readonly int userId;
        private MaterialLabel labelTitle, labelRequests, labelStatus, labelMessage;
        private DataGridView dataGridViewRequests;
        private ComboBox comboBoxStatus;
        private MaterialFlatButton buttonChangeStatus, buttonCompleteRequest;
        private ToolTip toolTip;

        public EmployeeForm(int userId)
        {
            this.userId = userId;
            InitializeMaterialSkin();
            InitializeComponentsManually();
            LoadStatusComboBox();
            LoadEmployeeRequests();
        }

        private void InitializeMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void InitializeComponentsManually()
        {
            this.Text = "Управление заявками сотрудника";
            this.Size = new Size(800, 600);


            // Метка для заявок
            labelRequests = new MaterialLabel
            {
                Text = "Все заявки:",
                AutoSize = true,
                Location = new Point(50, 70)
            };
            this.Controls.Add(labelRequests);

            // Таблица заявок
            dataGridViewRequests = new DataGridView
            {
                Location = new Point(50, 100),
                Size = new Size(700, 300),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
            };
            this.Controls.Add(dataGridViewRequests);

            // Метка для статуса
            labelStatus = new MaterialLabel
            {
                Text = "Новый статус:",
                AutoSize = true,
                Location = new Point(50, 420)
            };
            this.Controls.Add(labelStatus);

            // Выпадающий список статусов
            comboBoxStatus = new ComboBox
            {
                Location = new Point(160, 420),
                Width = 200
            };
            comboBoxStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(comboBoxStatus);

            // Кнопка изменения статуса
            buttonChangeStatus = new MaterialFlatButton
            {
                Text = "Изменить статус",
                Location = new Point(370, 420),
                Width = 150
            };
            buttonChangeStatus.Click += new EventHandler(buttonChangeStatus_Click);
            toolTip = new ToolTip();
            toolTip.SetToolTip(buttonChangeStatus, "Изменить статус выбранной заявки");
            this.Controls.Add(buttonChangeStatus);

            // Кнопка завершения заявки
            buttonCompleteRequest = new MaterialFlatButton
            {
                Text = "Завершить заявку",
                Location = new Point(530, 420),
                Width = 150
            };
            buttonCompleteRequest.Click += new EventHandler(buttonCompleteRequest_Click);
            toolTip.SetToolTip(buttonCompleteRequest, "Завершить выбранную заявку");
            this.Controls.Add(buttonCompleteRequest);

            // Метка для сообщений
            labelMessage = new MaterialLabel
            {
                AutoSize = true,
                Location = new Point(50, 460),
                MaximumSize = new Size(700, 0),
                ForeColor = Color.Red
            };
            this.Controls.Add(labelMessage);

            // Кастомный крестик для выхода
            MaterialFlatButton btnClose = new MaterialFlatButton
            {
                Text = "X",
                Location = new Point(this.ClientSize.Width - 50, 10),
                Size = new Size(30, 30),
                ForeColor = Color.Red
            };
            btnClose.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnClose);
        }

        private void LoadStatusComboBox()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT StatusID, StatusName FROM RequestStatus";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable statusTable = new DataTable();
                        adapter.Fill(statusTable);
                        comboBoxStatus.DataSource = statusTable;
                        comboBoxStatus.DisplayMember = "StatusName";
                        comboBoxStatus.ValueMember = "StatusID";
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка загрузки статусов: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private void LoadEmployeeRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT r.RequestID, r.RequestNumber, r.RequestDate, e.[Name] AS Equipment, 
                           ft.TypeName AS FailureType, c.FirstName + ' ' + c.LastName AS Client, 
                           rs.StatusName, r.AssignedEmployeeID, em.FirstName + ' ' + em.LastName AS EmployeeName
                    FROM RepairRequests r
                    LEFT JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                    LEFT JOIN FailureTypes ft ON svalTypeID = ft.FailureTypeID
                    LEFT JOIN Clients c ON r.ClientID = c.ClientID
                    LEFT JOIN RequestStatus rs ON r.StatusID = rs.StatusID
                    LEFT JOIN Employees em ON r.AssignedEmployeeID = em.EmployeeID
                    WHERE r.AssignedEmployeeID = @EmployeeID";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@EmployeeID", userId);
                        DataTable requestsTable = new DataTable();
                        adapter.Fill(requestsTable);
                        dataGridViewRequests.DataSource = requestsTable;

                        // Настройка заголовков
                        if (dataGridViewRequests.Columns.Contains("RequestID"))
                            dataGridViewRequests.Columns["RequestID"].HeaderText = "ID заявки";
                        if (dataGridViewRequests.Columns.Contains("RequestNumber"))
                            dataGridViewRequests.Columns["RequestNumber"].HeaderText = "Номер заявки";
                        if (dataGridViewRequests.Columns.Contains("RequestDate"))
                            dataGridViewRequests.Columns["RequestDate"].HeaderText = "Дата";
                        if (dataGridViewRequests.Columns.Contains("Equipment"))
                            dataGridViewRequests.Columns["Equipment"].HeaderText = "Оборудование";
                        if (dataGridViewRequests.Columns.Contains("FailureType"))
                            dataGridViewRequests.Columns["FailureType"].HeaderText = "Тип неисправности";
                        if (dataGridViewRequests.Columns.Contains("Client"))
                            dataGridViewRequests.Columns["Client"].HeaderText = "Клиент";
                        if (dataGridViewRequests.Columns.Contains("StatusName"))
                            dataGridViewRequests.Columns["StatusName"].HeaderText = "Статус";
                        if (dataGridViewRequests.Columns.Contains("AssignedEmployeeID"))
                            dataGridViewRequests.Columns["AssignedEmployeeID"].HeaderText = "ID сотрудника";
                        if (dataGridViewRequests.Columns.Contains("EmployeeName"))
                            dataGridViewRequests.Columns["EmployeeName"].HeaderText = "Сотрудник";
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка загрузки заявок: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private void buttonChangeStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewRequests.SelectedRows.Count == 0)
                {
                    labelMessage.Text = "Ошибка: Выберите заявку.";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }

                if (comboBoxStatus.SelectedValue == null)
                {
                    labelMessage.Text = "Ошибка: Выберите статус.";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }

                string requestNumber = dataGridViewRequests.SelectedRows[0].Cells["RequestNumber"].Value.ToString();
                int newStatusId = (int)comboBoxStatus.SelectedValue;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE RepairRequests SET StatusID = @StatusID WHERE RequestNumber = @RequestNumber";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusID", newStatusId);
                        command.Parameters.AddWithValue("@RequestNumber", requestNumber);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            labelMessage.Text = "Ошибка: Заявка не найдена.";
                            labelMessage.ForeColor = Color.Red;
                            return;
                        }
                    }
                }

                labelMessage.Text = "Статус заявки успешно изменен!";
                labelMessage.ForeColor = Color.Green;
                LoadEmployeeRequests();
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private void buttonCompleteRequest_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewRequests.SelectedRows.Count == 0)
                {
                    labelMessage.Text = "Ошибка: Выберите заявку.";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }

                string requestNumber = dataGridViewRequests.SelectedRows[0].Cells["RequestNumber"].Value.ToString();
                int completedStatusId = GetStatusId("Выполнено");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE RepairRequests SET StatusID = @StatusID WHERE RequestNumber = @RequestNumber";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusID", completedStatusId);
                        command.Parameters.AddWithValue("@RequestNumber", requestNumber);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            labelMessage.Text = "Ошибка: Заявка не найдена.";
                            labelMessage.ForeColor = Color.Red;
                            return;
                        }
                    }
                }

                labelMessage.Text = "Заявка успешно завершена!";
                labelMessage.ForeColor = Color.Green;
                LoadEmployeeRequests();
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private int GetStatusId(string statusName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT StatusID FROM RequestStatus WHERE StatusName = @StatusName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusName", statusName);
                        object result = command.ExecuteScalar();
                        if (result == null)
                            throw new Exception($"Статус '{statusName}' не найден.");
                        return (int)result;
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"Ошибка получения статуса: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
                throw;
            }
        }
    }
}