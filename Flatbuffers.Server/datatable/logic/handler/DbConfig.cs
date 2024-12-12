namespace Logic.database
{
    public class DbConfig
    {
		private Dictionary<string,(string, string)> nonDefaultOptions = new Dictionary<string,(string, string)>();
		private Dictionary<string, (string, string)> defaultOptions = new Dictionary<string, (string, string)>();
		private Dictionary<string, (string, string)> options => nonDefaultOptions.Union(defaultOptions).ToDictionary(pair => pair.Key, pair => pair.Value);
		private List<string> suppressedDigests = new List<string>();

		public string ConnectionString 
			=> string.Join(";", options
			.Where(k => !suppressedDigests.Contains(k.Key))
			.Select(kv => $"{kv.Value.Item1}={kv.Value.Item2}"));

		public DbConfig() { }

		public DbConfig(string connectionString)
        {
			ApplyConnectionString(connectionString);
        }

		public void ApplyConnectionString(string connectionString)
		{
			var userOptions = connectionString.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
				.Select(o => new KeyValuePair<string, string>(o.Split('=')[0], o.Split('=')[1]));

			foreach(var userOption in userOptions)
            {
				SetOption(userOption.Key, userOption.Value);
            }
		}

		public string GetValueOf(string optionName)
		{
			if (options.TryGetValue(Digest(optionName), out var optionValue))
			{
				return optionValue.Item2;
			}
			else
			{
				return "";
			}
		}

		public void SetOption(string key, string value)
        {
			if(defaultOptions.ContainsKey(Digest(key)))
            {
				defaultOptions[Digest(key)] = (defaultOptions[Digest(key)].Item1, value);
            }
			else if (nonDefaultOptions.ContainsKey(Digest(key)))
			{
				nonDefaultOptions[Digest(key)] = (nonDefaultOptions[Digest(key)].Item1, value);
			}
			else
			{
				nonDefaultOptions.Add(Digest(key), (key, value));
			}
		}

		public void AddDefaultOption(string key, string value)
        {
			if (nonDefaultOptions.ContainsKey(Digest(key)))
			{
				value = nonDefaultOptions[Digest(key)].Item2;
				nonDefaultOptions.Remove(Digest(key));
			}
			defaultOptions.Add(Digest(key), (key, value));
		}

		public void SuppressFromConnectionString(params string[] suppressedOptions)
        {
			suppressedDigests = new List<string>();
			suppressedDigests.AddRange(suppressedOptions.Select(opt => Digest(opt)));
		}

		private string Digest(string input)
        {
			return input.ToLower().Replace(" ","");
        }
    }    
}
