using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using WPFPaisesCExecutavel.Modelos;

namespace WPFPaisesCExecutavel.Servicos
{
    /// <summary>
    /// Classe responsável por gerenciar os dados dos países utilizando SQLite.
    /// </summary>
    public class DataService
    {
        private readonly string dbPath = @"Data\Countries.sqlite";
        private readonly DialogService dialogueService;

        public DataService()
        {
            dialogueService = new DialogService(); 

            try
            {
                string directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                    CreateDatabase();
                }
                else
                {
                    InitializeDatabase();
                }
            }
            catch (Exception e)
            {
                dialogueService.ShowMessage("Erro", e.Message);
            }
        }

        private void CreateDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS countries (
                        Name TEXT,
                        Capital TEXT,
                        Region TEXT,
                        Subregion TEXT,
                        Population INTEGER,
                        Gini REAL,
                        Flag TEXT,
                        Borders TEXT,
                        Independent BOOLEAN,
                        Status TEXT,
                        Currencies TEXT,
                        Languages TEXT
                    )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void InitializeDatabase()
        {
            CreateDatabase(); 
        }

        private SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={dbPath};Version=3;");
        }

        /// <summary>
        /// Salva a lista de países de forma assíncrona na base de dados SQLite.
        /// </summary>
        /// <param name="countries">Lista de países a serem salvos.</param>
        /// <param name="progress">Objeto de progresso para relatar o progresso da operação.</param>
        /// <returns>Tarefa que representa a operação assíncrona.</returns>
        public async Task SaveCountriesAsync(List<Countries> countries, IProgress<int> progress)
        {
            const string sql = @"
                INSERT INTO countries 
                (Name, Capital, Region, Subregion, Population, Gini, Flag, Borders, Independent, Status, Currencies, Languages) 
                VALUES 
                (@Name, @Capital, @Region, @Subregion, @Population, @Gini, @Flag, @Borders, @Independent, @Status, @Currencies, @Languages)";

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int count = 0;
                        foreach (var country in countries)
                        {
                            using (var command = new SQLiteCommand(sql, connection))
                            {
                                var giniValue = country.Gini?.Values.FirstOrDefault() ?? 0.0;
                                var serializedBorders = JsonConvert.SerializeObject(country.Borders ?? new List<string>());
                                var serializedCurrencies = JsonConvert.SerializeObject(country.Currencies ?? new Dictionary<string, Currency>());
                                var serializedLanguages = JsonConvert.SerializeObject(country.Languages ?? new Dictionary<string, string>());

                                // Define os parâmetros do comando SQL com os dados do país atual
                                command.Parameters.AddWithValue("@Name", country.Name?.Common ?? string.Empty);
                                command.Parameters.AddWithValue("@Capital", country.Capital?.FirstOrDefault() ?? string.Empty);
                                command.Parameters.AddWithValue("@Region", country.Region ?? string.Empty);
                                command.Parameters.AddWithValue("@Subregion", country.Subregion ?? string.Empty);
                                command.Parameters.AddWithValue("@Population", country.Population ?? 0);
                                command.Parameters.AddWithValue("@Gini", giniValue);
                                command.Parameters.AddWithValue("@Flag", country.Flags?.Png ?? string.Empty);
                                command.Parameters.AddWithValue("@Borders", serializedBorders);
                                command.Parameters.AddWithValue("@Independent", country.Independent);
                                command.Parameters.AddWithValue("@Status", country.Status ?? string.Empty);
                                command.Parameters.AddWithValue("@Currencies", serializedCurrencies);
                                command.Parameters.AddWithValue("@Languages", serializedLanguages);

                                await command.ExecuteNonQueryAsync();
                            }
                            count++;
                            progress?.Report((int)((count / (double)countries.Count) * 100)); 
                        }
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback(); 
                        dialogueService.ShowMessage("Erro ao salvar países", e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Obtém de forma assíncrona a lista de países armazenados na base de dados SQLite.
        /// </summary>
        /// <returns>Tarefa que representa a operação assíncrona. O resultado da tarefa contém a lista de países.</returns>
        public async Task<List<Countries>> GetCountriesAsync()
        {
            var countries = new List<Countries>();
            const string sql = @"
                SELECT DISTINCT Name, Capital, Region, Subregion, Population, Gini, Flag, Borders, Independent, Status, Currencies, Languages 
                FROM countries 
                ORDER BY Name ASC";

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var giniValue = reader["Gini"] != DBNull.Value ? Convert.ToDouble(reader["Gini"]) : 0.0;

                                var country = new Countries
                                {
                                    Name = new CountryName { Common = reader["Name"].ToString() },
                                    Capital = DeserializeList(reader["Capital"].ToString()),
                                    Region = reader["Region"].ToString(),
                                    Subregion = reader["Subregion"].ToString(),
                                    Population = reader["Population"] != DBNull.Value ? Convert.ToInt64(reader["Population"]) : (long?)null,
                                    Gini = new Dictionary<string, double> { { "value", giniValue } },
                                    Flags = new CountryFlags { Png = reader["Flag"].ToString() },
                                    Borders = DeserializeList(reader["Borders"].ToString()),
                                    Independent = reader["Independent"] != DBNull.Value && Convert.ToBoolean(reader["Independent"]),
                                    Status = reader["Status"].ToString(),
                                    Currencies = DeserializeDictionary<Currency>(reader["Currencies"].ToString()),
                                    Languages = DeserializeDictionary<string>(reader["Languages"].ToString())
                                };

                                countries.Add(country); // Adiciona o país à lista de países obtidos
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                dialogueService.ShowMessage("Erro ao carregar países", e.Message); 
            }

            return countries;
        }

        private List<string> DeserializeList(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(jsonString); 
            }
            catch
            {
                return new List<string>(); 
            }
        }

        private Dictionary<string, T> DeserializeDictionary<T>(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, T>>(jsonString); 
            }
            catch
            {
                return new Dictionary<string, T>();
            }
        }

        /// <summary>
        /// Apaga todos os países armazenados na base de dados SQLite de forma assíncrona.
        /// </summary>
        /// <returns>Tarefa que representa a operação assíncrona.</returns>
        public async Task DeleteCountriesAsync()
        {
            const string sql = "DELETE FROM countries";

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(sql, connection))
                {
                    try
                    {
                        await command.ExecuteNonQueryAsync(); 
                    }
                    catch (Exception e)
                    {
                        dialogueService.ShowMessage("Erro ao excluir países", e.Message); 
                    }
                }
            }
        }
    }
}
