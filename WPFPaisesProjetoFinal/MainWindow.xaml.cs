using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFPaisesCExecutavel.Modelos;
using WPFPaisesCExecutavel.Servicos;

namespace WPFPaisesProjetoFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ApiUrl = "https://restcountries.com/v3.1/all";
        private ApiService apiService;
        private DataService dataService;
        private List<Countries> countries;

        public MainWindow()
        {
            InitializeComponent();
            apiService = new ApiService();
            dataService = new DataService();
            listBoxCountries.SelectionChanged += listBoxCountries_SelectionChanged;
            // Chama o método assíncrono para carregar países
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
            LoadCountriesAsync();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCountriesAsync();
        }

        private async Task LoadCountriesAsync()
        {
            bool loadFromApi = false;

            labelResultado.Content = "A atualizar países....";
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            labelStatus.Content = "Loading...";

            try
            {
                NetworkService networkService = new NetworkService();
                var connectionResult = networkService.CheckConnection();

                if (connectionResult.IsSuccess)
                {
                    // Tenta carregar os dados da API
                    countries = await LoadApiCountries();
                    loadFromApi = countries != null && countries.Count > 0;
                }

                if (!loadFromApi)
                {
                    // Se falhar, carrega os dados do banco de dados local
                    countries = await dataService.GetCountriesAsync();
                }

                if (countries == null || countries.Count == 0)
                {
                    labelResultado.Content = "Não há ligação à Internet" + Environment.NewLine +
                        "e não foram previamente carregados os países" + Environment.NewLine +
                        "Tente mais tarde!";

                    labelStatus.Content = "Primeira inicialização deverá ter ligação à Internet";
                    return;
                }

                DisplayCountries();

                labelResultado.Content = "Países atualizados...";
                progressBar.Value = 100;

                if (loadFromApi)
                {
                    labelStatus.Content = string.Format("Países carregados da API em {0:F}", DateTime.Now);
                }
                else
                {
                    labelStatus.Content = "Países carregados da base de dados.";
                }

                if (countries.Count > 0)
                {
                    ShowCountryDetails(countries[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao carregar os países: " + ex.Message);
            }
        }

        private async Task<List<Countries>> LoadApiCountries()
        {
            var countries = await apiService.GetCountries(ApiUrl);

            if (countries != null)
            {
                // Ordena os países por nome com LINQ
                countries = countries.OrderBy(c => c.Name.Common).ToList();

                // Cria um objeto de progresso
                var progress = new Progress<int>(percent =>
                {
                    labelResultado.Content = $"Salvando países... {percent}% concluído";
                    progressBar.Value = percent;
                });

                progressBar.Visibility = Visibility.Visible;
                progressBar.Maximum = 100;

                await dataService.DeleteCountriesAsync();
                // Salva os países com relatório de progresso
                await dataService.SaveCountriesAsync(countries, progress);

                return countries;
            }
            else
            {
                MessageBox.Show("Não foi possível carregar a lista de países.");
                return null;
            }
        }

        private void DisplayCountries()
        {
            if (countries != null)
            {
                listBoxCountries.Items.Clear();
                foreach (var country in countries)
                {
                    // Cria um StackPanel para cada item da lista
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Horizontal;

                    // Adiciona o nome do país
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = country.Name.Common;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    stackPanel.Children.Add(textBlock);

                    // Adiciona a miniatura da bandeira
                    Image flagImage = new Image();
                    flagImage.Width = 20;
                    flagImage.Height = 15;
                    flagImage.Margin = new Thickness(5, 0, 0, 0); // Ajuste a margem se necessário
                    flagImage.Source = new BitmapImage(new Uri(country.Flags.Png));
                    stackPanel.Children.Insert(0, flagImage); // Insere a bandeira no início do StackPanel

                    // Adiciona o StackPanel à ListBox
                    listBoxCountries.Items.Add(stackPanel);
                }

                // Seleciona automaticamente o primeiro item da lista
                SelectFirstItem();
            }
        }

        private void SelectFirstItem()
        {
            if (listBoxCountries.Items.Count > 0)
            {
                listBoxCountries.SelectedIndex = 0;
            }
        }

        private void ShowCountryDetails(Countries country)
        {
            labelName.Text = country.NameString;
            labelCapital.Text = country.CapitalString;
            labelRegion.Text = country.RegionString;
            lblSubRegion.Text = country.SubregionString;
            labelPopulation.Text = country.PopulationString;
            labelGini.Text = country.GiniString;
            labelStatusValue.Text = country.Status;
            labelUnMember.Text = country.IndependentString;

            labelCurrencies.Text = country.CurrienciesString;
            labelLanguages.Text = country.LanguagesString;

            listBoxBorders.Items.Clear();
            listBoxBorders.Items.Add(country.BordersTeste());

            pictureBoxFlag.Source = !string.IsNullOrEmpty(country.Flags.Png) ? new BitmapImage(new Uri(country.Flags.Png)) : null;
            miniFlag.Source = !string.IsNullOrEmpty(country.Flags.Png) ? new BitmapImage(new Uri(country.Flags.Png)) : null;
        }

        private void listBoxCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = listBoxCountries.SelectedIndex;
            if (selectedIndex >= 0)
            {
                Countries selectedCountry = countries[selectedIndex];
                if (selectedCountry != null)
                {
                    ShowCountryDetails(selectedCountry);
                }
            }
        }
    }
}