﻿using System.Reflection;
using Game.Logic.Utils;
using log4net;

namespace Game.Logic;

public class PropertyCollection
	{
		/// <summary>
		/// Define a logger for this class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Container of properties
		/// </summary>
		private readonly ReaderWriterDictionary<object, object> _props = new ReaderWriterDictionary<object, object>();

		/// <summary>
		/// Retrieve a property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="def">default value</param>
		/// <param name="loggued">loggued if the value is not found</param>
		/// <returns>value in properties or default value if not found</returns>
		public T getProperty<T>(object key)
		{
			return getProperty<T>(key, default(T));
		}
		public T getProperty<T>(object key, T def)
		{
			return getProperty<T>(key, def, false);
		}
		public T getProperty<T>(object key, T def, bool loggued)
		{
			object val;

			bool exists = _props.TryGetValue(key, out val);

			if (loggued)
			{
				if (!exists)
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Property '" + key + "' is required but not found, default value '" + def + "' is used.");
					
					return def;
				}
			}
			
			if (val is T)
				return (T)val;
			
			return def;
		}
		
		/// <summary>
		/// Set a property
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="val">value</param>
		public void setProperty(object key, object val)
		{
			if (val == null)
			{
				object dummy;
				_props.TryRemove(key, out dummy);
			}
			else
			{
				_props[key] = val;
			}
		}

		/// <summary>
		/// Remove a property
		/// </summary>
		/// <param name="key">key</param>
		public void removeProperty(object key)
		{
			object dummy;
			_props.TryRemove(key, out dummy);
		}

		/// <summary>
		/// List all properties
		/// </summary>
		/// <returns></returns>
		public List<string> getAllProperties()
		{
			return _props.Keys.Cast<string>().ToList();
		}

		/// <summary>
		/// Remove all properties
		/// </summary>
		public void removeAllProperties()
		{
			_props.Clear();
		}
	}