﻿using System.Collections;
using System.Collections.Specialized;

namespace Game.Logic.PropertyCalc;

public class MultiplicativeProperties : IMultiplicativeProperties
{
	private readonly object m_LockObject = new object();

	private sealed class PropertyEntry
	{
		public double cachedValue = 1.0;
		public HybridDictionary values;
		public void CalculateCachedValue()
		{
			if (values == null)
			{
				cachedValue = 1.0;
				return;
			}

			IDictionaryEnumerator de = values.GetEnumerator();
			double res = 1.0;
			while(de.MoveNext())
			{
				res *= (double)de.Value;
			}
			cachedValue = res;
		}
	}

	private HybridDictionary m_properties = new HybridDictionary();

	public void Set(int index, object key, double value)
	{
		lock (m_LockObject)
		{
			PropertyEntry entry = (PropertyEntry)m_properties[index];
			if (entry == null)
			{
				entry = new PropertyEntry();
				m_properties[index] = entry;
			}

			if (entry.values == null)
				entry.values = new HybridDictionary();

			entry.values[key] = value;
			entry.CalculateCachedValue();
		}
	}

	public void Remove(int index, object key)
	{
		lock (m_LockObject)
		{
			PropertyEntry entry = (PropertyEntry)m_properties[index];
			if (entry == null) return;
			if (entry.values == null) return;

			entry.values.Remove(key);

			// remove entry if it's empty
			if (entry.values.Count < 1)
			{
				m_properties.Remove(index);
				return;
			}

			entry.CalculateCachedValue();
		}
	}

	public double Get(int index)
	{
		PropertyEntry entry = (PropertyEntry)m_properties[index];
		if (entry == null) return 1.0;
		return entry.cachedValue;
	}    
}