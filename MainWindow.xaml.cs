using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
namespace CarGarage;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public string Search_text { get => search_text; set { search_text = value; OnPropertyChanged(); } }
    private ObservableCollection<Car> cars;
    private string search_text;

    public ObservableCollection<Car> Cars { get => cars; set { cars = value; OnPropertyChanged(); } }
    private readonly IConfiguration _configuration;

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public MainWindow(IConfiguration configuration)
    {
        InitializeComponent();
        DataContext = this;
        _configuration = configuration;
        RefreshDataSource("SELECT * FROM Car_Table", null);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        using (SqlConnection connection = new(_configuration.GetConnectionString("DbConnection")))
        {
            try
            {
                connection.Open();
                SqlCommand cmd = new();
                cmd.Parameters.Add("@param", System.Data.SqlDbType.NVarChar).Value = Search_text;
                string query;
                if (MarkaRadioButton.IsChecked == true)
                    query = "SELECT * FROM Car_table WHERE Marka LIKE @param";
                else
                    query = "SELECT * FROM Car_table WHERE Model LIKE @param";
                cmd.CommandText=query;
                cmd.Connection = connection;
                RefreshDataSource(query, cmd);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddWindow();
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        window.ConnectionString = _configuration.GetConnectionString("DbConnection")!;
        window.ShowDialog();
        RefreshDataSource("SELECT * FROM Car_Table", null);
        Search_text = "";
    }

    private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var textbox = sender as TextBox;
        if (textbox?.Text.Length == 0)
            RefreshDataSource("SELECT * FROM Car_Table",null);
    }
    void RefreshDataSource(string query, SqlCommand? cmd)
    {
        using (SqlConnection connection = new(_configuration.GetConnectionString("DbConnection")))
        {
            try
            {
                connection.Open();
                cmd ??= new(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                Cars = new();
                while (reader.Read())
                    Cars.Add(new Car
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Marka = reader["Marka"].ToString(),
                        Model = reader["Model"].ToString()
                    });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}