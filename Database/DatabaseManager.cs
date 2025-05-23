using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace StudentCanvasApp.Database
{
    public class DatabaseManager
    {
        private string connectionString = "server=localhost;port=3306;user=student;password=1234;database=schoolmanagement;";

        public void InsertStudent(string name, string email, string password)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO student (Name, Email, Password) VALUES (@name, @email, @password)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Student inserted successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }
    }
}
