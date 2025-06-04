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
    public partial class ClientForm : MaterialForm
    {
        private readonly int userId;
        private MaterialLabel labelRequestNumber, labelEquipment, labelFailureType, labelProblemDescription, labelStatusLabel, labelStatus;
        private MaterialSingleLineTextField textBoxRequestNumber, textBoxProblemDescription;
        private ComboBox comboBoxEquipment, comboBoxFailureType, comboBoxStatus;
        private MaterialFlatButton buttonAddRequest;
        private DataGridView dataGridViewRequests;
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RepairRequestSystem;Integrated Security=True";

        public ClientForm(int userId)
        {
            this.userId = userId;
            InitializeMaterialSkin();
            InitializeComponentsManually();
            LoadComboBoxes();
            GenerateRequestNumber();
            LoadUserRequests();
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
            this.Text = "Управление заявками клиента";
            this.Size = new Size(800, 600);


            // Номер заявки
            labelRequestNumber = new MaterialLabel { Text = "Номер заявки:", AutoSize = true, Location = new Point(50, 70) };
            textBoxRequestNumber = new MaterialSingleLineTextField { Hint = "Номер заявки", Location = new Point(160, 70), Width = 200};
            this.Controls.Add(labelRequestNumber);
            this.Controls.Add(textBoxRequestNumber);

            // Оборудование
            labelEquipment = new MaterialLabel { Text = "Оборудование:", AutoSize = true, Location = new Point(50, 120) };
            comboBoxEquipment = new ComboBox { Location = new Point(160, 120), Width = 200 };
            comboBoxEquipment.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(labelEquipment);
            this.Controls.Add(comboBoxEquipment);

            // Тип неисправности
            labelFailureType = new MaterialLabel { Text = "Тип неисправности:", AutoSize = true, Location = new Point(50, 170) };
            comboBoxFailureType = new ComboBox { Location = new Point(160, 170), Width = 200 };
            comboBoxFailureType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(labelFailureType);
            this.Controls.Add(comboBoxFailureType);

            // Описание проблемы
            labelProblemDescription = new MaterialLabel { Text = "Описание проблемы:", AutoSize = true, Location = new Point(50, 220) };
            textBoxProblemDescription = new MaterialSingleLineTextField { Hint = "Описание проблемы", Location = new Point(160, 220), Width = 200, Height = 50 };
            this.Controls.Add(labelProblemDescription);
            this.Controls.Add(textBoxProblemDescription);

            // Статус
            labelStatusLabel = new MaterialLabel { Text = "Статус:", AutoSize = true, Location = new Point(50, 290) };
            comboBoxStatus = new ComboBox { Location = new Point(160, 290), Width = 200 };
            comboBoxStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(labelStatusLabel);
            this.Controls.Add(comboBoxStatus);

            // Кнопка добавления заявки
            buttonAddRequest = new MaterialFlatButton { Text = "Добавить заявку", Location = new Point(160, 340), Width = 150 };
            buttonAddRequest.Click += new EventHandler(buttonAddRequest_Click);
            this.Controls.Add(buttonAddRequest);

            // Метка статуса
            labelStatus = new MaterialLabel { AutoSize = true, Location = new Point(50, 380), MaximumSize = new Size(700, 0), ForeColor = Color.Red };
            this.Controls.Add(labelStatus);

            // Таблица заявок
            dataGridViewRequests = new DataGridView
            {
                Location = new Point(50, 410),
                Size = new Size(700, 150),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(dataGridViewRequests);

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

        private void LoadComboBoxes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Загрузка оборудования
                    string equipmentQuery = "SELECT EquipmentID, [Name] FROM Equipment";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(equipmentQuery, connection))
                    {
                        DataTable equipmentTable = new DataTable();
                        adapter.Fill(equipmentTable);
                        comboBoxEquipment.DataSource = equipmentTable;
                        comboBoxEquipment.DisplayMember = "[Name]";
                        comboBoxEquipment.ValueMember = "EquipmentID";
                    }

                    // Загрузка типов неисправностей
                    string failureQuery = "SELECT FailureTypeID, TypeName FROM FailureTypes";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(failureQuery, connection))
                    {
                        DataTable failureTable = new DataTable();
                        adapter.Fill(failureTable);
                        comboBoxFailureType.DataSource = failureTable;
                        comboBoxFailureType.DisplayMember = "TypeName";
                        comboBoxFailureType.ValueMember = "FailureTypeID";
                    }

                    // Загрузка статусов
                    string statusQuery = "SELECT StatusID, StatusName FROM RequestStatus";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(statusQuery, connection))
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
                labelStatus.Text = $"Ошибка загрузки данных: {ex.Message}";
                labelStatus.ForeColor = Color.Red;
            }
        }

        private void GenerateRequestNumber()
        {
            textBoxRequestNumber.Text = "REQ-" + DateTime.Now.ToString("yyyyMMdd");
        }

        private int GetClientId()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ClientID FROM Clients WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private void LoadUserRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT r.RequestNumber, r.RequestDate, e.[Name] AS EquipmentName, 
                                f.TypeName AS FailureType, r.ProblemDescription, s.StatusName
                                FROM RepairRequests r
                                JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                                JOIN FailureTypes f ON r.FailureTypeID = f.FailureTypeID
                                JOIN RequestStatus s ON r.StatusID = s.StatusID
                                JOIN Clients c ON r.ClientID = c.ClientID
                                WHERE c.UserID = @UserID";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@UserID", userId);
                        DataTable requestsTable = new DataTable();
                        adapter.Fill(requestsTable);
                        dataGridViewRequests.DataSource = requestsTable;

                        // Настройка заголовков столбцов
                        if (dataGridViewRequests.Columns.Contains("RequestNumber"))
                            dataGridViewRequests.Columns["RequestNumber"].HeaderText = "Номер заявки";
                        if (dataGridViewRequests.Columns.Contains("RequestDate"))
                            dataGridViewRequests.Columns["RequestDate"].HeaderText = "Дата";
                        if (dataGridViewRequests.Columns.Contains("EquipmentName"))
                            dataGridViewRequests.Columns["EquipmentName"].HeaderText = "Оборудование";
                        if (dataGridViewRequests.Columns.Contains("FailureType"))
                            dataGridViewRequests.Columns["FailureType"].HeaderText = "Тип неисправности";
                        if (dataGridViewRequests.Columns.Contains("ProblemDescription"))
                            dataGridViewRequests.Columns["ProblemDescription"].HeaderText = "Описание";
                        if (dataGridViewRequests.Columns.Contains("StatusName"))
                            dataGridViewRequests.Columns["StatusName"].HeaderText = "Статус";
                    }
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Ошибка загрузки заявок: {ex.Message}";
                labelStatus.ForeColor = Color.Red;
            }
        }

        private void buttonAddRequest_Click(object sender, EventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(textBoxProblemDescription.Text))
                {
                    labelStatus.Text = "Ошибка: Описание проблемы не может быть пустым.";
                    labelStatus.ForeColor = Color.Red;
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка существования RequestNumber
                    string checkQuery = "SELECT COUNT(*) FROM RepairRequests WHERE RequestNumber = @RequestNumber";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@RequestNumber", textBoxRequestNumber.Text);
                        int count = (int)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Такая заявка уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textBoxRequestNumber.Text = "";
                            GenerateRequestNumber();
                            return;
                        }
                    }

                    string query = @"INSERT INTO RepairRequests (RequestNumber, RequestDate, EquipmentID, FailureTypeID, 
                                ProblemDescription, ClientID, StatusID)
                                VALUES (@RequestNumber, @RequestDate, @EquipmentID, @FailureTypeID, 
                                @ProblemDescription, @ClientID, @StatusID)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RequestNumber", textBoxRequestNumber.Text);
                        command.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                        command.Parameters.AddWithValue("@EquipmentID", comboBoxEquipment.SelectedValue);
                        command.Parameters.AddWithValue("@FailureTypeID", comboBoxFailureType.SelectedValue);
                        command.Parameters.AddWithValue("@ProblemDescription", textBoxProblemDescription.Text);
                        command.Parameters.AddWithValue("@ClientID", GetClientId());
                        command.Parameters.AddWithValue("@StatusID", comboBoxStatus.SelectedValue);
                        command.ExecuteNonQuery();
                    }
                }

                labelStatus.Text = "Заявка успешно добавлена!";
                labelStatus.ForeColor = Color.Green;
                textBoxProblemDescription.Text = "";
                GenerateRequestNumber();
                LoadUserRequests();
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Ошибка: {ex.Message}";
                labelStatus.ForeColor = Color.Red;
            }
        }
    }
}