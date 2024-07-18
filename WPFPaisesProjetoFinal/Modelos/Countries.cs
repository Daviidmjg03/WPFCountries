using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPaisesCExecutavel.Modelos
{
    public class Countries
    {
        private CountryName _name;
        private List<string> _capital;
        private string _region;
        private string _subregion;
        private long? _population;
        private Dictionary<string, double> _gini;
        private List<string> _borders;
        private bool _independent;
        private string _status;
        private Dictionary<string, Currency> _curriencies;
        private Dictionary<string, string> _languages;

        public CountryName Name
        {
            get => _name;
            set => _name = value ?? new CountryName();
        }
        public string NameString
        {
            get => !string.IsNullOrEmpty(_name?.Common) ? _name.Common : "N/D";
        }
        public string CapitalString
        {
            get
            {
                if (_capital == null || !_capital.Any())
                {
                    return "N/D";
                }
                return _capital.FirstOrDefault();
            }
        }

        public List<string> Capital
        {
            get
            {
                if (_capital == null || _capital.Count == 0)
                {
                    return null;
                }
                return _capital;
            }
            set
            {
                _capital = value;
            }
        }
        public string RegionString
        {
            get
            {
                if (string.IsNullOrEmpty(_region))
                {
                    return "N/D";
                }
                return _region;
            }
        }

        public string Region
        {
            get
            {
                return _region;
            }
            set
            {
                _region = value;
            }
        }
        public string SubregionString
        {
            get
            {
                if (string.IsNullOrEmpty(_subregion))
                {
                    return "N/D";
                }
                return _subregion;
            }
        }

        public string Subregion
        {
            get
            {
                return _subregion;
            }
            set
            {
                _subregion = value;
            }
        }
        public string PopulationString
        {
            get
            {
                if (_population == null || _population == 0)
                {
                    return "N/D";
                }
                return _population.ToString();
            }
        }

        public long? Population
        {
            get
            {
                return _population;
            }
            set
            {
                _population = value;
            }
        }
        public string GiniString
        {
            get
            {
                if (_gini == null || !_gini.Values.Any() || _gini.Values.FirstOrDefault() == 0)
                {
                    return "N/D";
                }
                return _gini.Values.FirstOrDefault().ToString();
            }
        }
        public Dictionary<string, double> Gini
        {
            get
            {
                if (_gini == null || _gini.Count == 0)
                {
                    return null;
                }
                return _gini;
            }
            set
            {
                _gini = value;
            }
        }
        public CountryFlags Flags { get; set; }
        public List<string> Borders
        {
            get
            {
                if (_borders == null || _borders.Count == 0)
                {
                    _borders = new List<string> { "N/D" };
                }
                return _borders;
            }
            set
            {
                _borders = value;
            }
        }
        public string BordersTeste()
        {
            if (Borders.Count == 1 && Borders[0] == "N/D")
            {
                return "N/D";
            }
            else if (Borders.Count > 0 && !Borders.All(b => b == "N/D"))
            {
                return string.Join(", ", Borders.Where(border => border != "N/D"));
            }
            else
            {
                return "No borders";
            }
        }
        public string IndependentString
        {
            get
            {
                return _independent ? "Yes" : "No";
            }
        }

        public bool Independent
        {
            get
            {
                return _independent;
            }
            set
            {
                _independent = value;
            }
        }
        public string Status
        {
            get => string.IsNullOrEmpty(_status) ? "N/D" : _status;
            set => _status = value;
        }
        public string CurrienciesString
        {
            get
            {
                if (_curriencies == null || _curriencies.Count == 0 || (_curriencies.Count == 1 && _curriencies.ContainsKey("N/D")))
                {
                    return "N/D";
                }

                var firstCurrency = _curriencies.Values.FirstOrDefault(c => c.Name != "N/D");
                if (firstCurrency != null)
                {
                    return firstCurrency.Name;
                }

                return "N/D";
            }
        }

        public Dictionary<string, Currency> Currencies
        {
            get
            {
                if (_curriencies == null || _curriencies.Count == 0)
                {
                    _curriencies = new Dictionary<string, Currency> { { "N/D", new Currency { Name = "N/D", Symbol = "N/D" } } };
                }
                return _curriencies;
            }
            set { _curriencies = value; }
        }

        public string LanguagesString
        {
            get
            {
                if (_languages == null || _languages.Count == 0 || (_languages.Count == 1 && _languages.ContainsKey("N/D")))
                {
                    return "N/D";
                }
                return _languages.Values.FirstOrDefault(lang => lang != "N/D") ?? "N/D";
            }
        }

        public Dictionary<string, string> Languages
        {
            get
            {
                if (_languages == null || _languages.Count == 0)
                {
                    _languages = new Dictionary<string, string> { { "N/D", "N/D" } };
                }
                return _languages;
            }
            set { _languages = value; }
        }
    }
}
