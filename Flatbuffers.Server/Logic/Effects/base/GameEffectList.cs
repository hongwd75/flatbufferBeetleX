using System.Collections;
using System.Reflection;
using Game.Logic.AI.Brain;
using Game.Logic.datatable;
using Game.Logic.Skills;
using Game.Logic.Spells;
using Game.Logic.Utils;
using log4net;
using Logic.database;
using Logic.database.table;

namespace Game.Logic.Effects;

public class GameEffectList : IEnumerable<IGameEffect>
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	private readonly object m_lockObject = new object();

	protected List<IGameEffect> m_effects;

	protected readonly GameLiving m_owner;

	protected ushort m_runningID = 1;
	
	protected volatile sbyte m_changesCount;
	
	public GameEffectList(GameLiving owner)
	{
		if (owner == null)
			throw new ArgumentNullException("owner");
		m_owner = owner;
	}
	
	public virtual bool Add(IGameEffect effect)
	{
		// dead owners don't get effects
		if (!m_owner.IsAlive || m_owner.ObjectState != GameObject.eObjectState.Active)
			return false;

		lock (m_lockObject)
		{
			if (m_effects == null)
				m_effects = new List<IGameEffect>(5);
			
			effect.InternalID = m_runningID++;
			
			if (m_runningID == 0)
				m_runningID = 1;
			
			m_effects.Add(effect);
		}

		OnEffectsChanged(effect);

		return true;
	}
	
	public virtual bool Remove(IGameEffect effect)
	{
		if (m_effects == null)
			return false;

		List<IGameEffect> changedEffects = new List<IGameEffect>();

		lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
		{
			int index = m_effects.IndexOf(effect);
			
			if (index < 0)
				return false;
			
			m_effects.RemoveAt(index);
			
			// Register remaining effects for change
			changedEffects.AddRange(m_effects.Skip(index));
		}

		BeginChanges();
		changedEffects.ForEach(OnEffectsChanged);
		CommitChanges();
		return true;
	}

	public virtual void CancelAll()
	{
		IGameEffect[] fx;

		if (m_effects == null)
			return;
		
		lock (m_lockObject)
		{
			fx = m_effects.ToArray();
			m_effects.Clear();
		}
		
		BeginChanges();
		
		foreach (var effect in fx)
		{
			effect.Cancel(false);
		}
		CommitChanges();
	}

	public virtual void RestoreAllEffects()
	{
		GamePlayer player = m_owner as GamePlayer;
		
		if (player == null || player.DBCharacter == null || GameServer.Database == null)
			return;

		var effs = GameDB<PlayerXEffect>.SelectObjects(DB.Column(nameof(PlayerXEffect.ChardID)).IsEqualTo(player.ObjectId));
		if (effs == null)
			return;

		foreach (PlayerXEffect eff in effs)
			GameServer.Database.DeleteObject(eff);

		foreach (PlayerXEffect eff in effs.GroupBy(e => e.Var1).Select(e => e.First()))
		{
			if (eff.SpellLine == GlobalSpellsLines.Reserved_Spells)
				continue;

			bool good = true;
			Spell spell = SkillBase.GetSpellByID(eff.Var1);

			if (spell == null)
				good = false;

			SpellLine line = null;

			if (!string.IsNullOrEmpty(eff.SpellLine))
			{
				line = SkillBase.GetSpellLine(eff.SpellLine, false);
				if (line == null)
				{
					good = false;
				}
			}
			else
			{
				good = false;
			}

			if (good)
			{
				ISpellHandler handler = ScriptMgr.CreateSpellHandler(player, spell, line);
				GameSpellEffect e;
				e = new GameSpellEffect(handler, eff.Duration, spell.Frequency);
				e.RestoredEffect = true;
				int[] vars = { eff.Var1, eff.Var2, eff.Var3, eff.Var4, eff.Var5, eff.Var6 };
				e.RestoreVars = vars;
				e.Start(player);
			}
		}
	}

	public virtual void SaveAllEffects()
	{
		GamePlayer player = m_owner as GamePlayer;
		
		if (player == null || m_effects == null)
			return;

		lock (m_lockObject)
		{
			if (m_effects.Count < 1)
				return;

			foreach (IGameEffect eff in m_effects)
			{
				try
				{
					var gse = eff as GameSpellEffect;
					if (gse != null)
					{
						// No concentration Effect from Other Caster
						if (gse.Concentration > 0 && gse.SpellHandler.Caster != player)
							continue;
					}
					
					PlayerXEffect effx = eff.getSavedEffect();
					
					if (effx == null)
						continue;

					if (effx.SpellLine == GlobalSpellsLines.Reserved_Spells)
						continue;

					effx.ChardID = player.ObjectId;
					
					GameServer.Database.AddObject(effx);
				}
				catch (Exception e)
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("Could not save effect ({0}) on player: {1}, {2}", eff, player, e);
				}
			}
		}
	}

	public virtual void OnEffectsChanged(IGameEffect changedEffect)
	{
		if (m_changesCount > 0)
			return;

		UpdateChangedEffects();
	}

	public void BeginChanges()
	{
		m_changesCount++;
	}

	public virtual void CommitChanges()
	{
		if (--m_changesCount < 0)
		{
			if (log.IsWarnEnabled)
				log.WarnFormat("changes count is less than zero, forgot BeginChanges()?\n{0}", Environment.StackTrace);

			m_changesCount = 0;
		}

		if (m_changesCount == 0)
			UpdateChangedEffects();
	}

	protected virtual void UpdateChangedEffects()
	{
		if (m_owner is GameNPC)
		{
			IControlledBrain npc = ((GameNPC)m_owner).Brain as IControlledBrain;
			if (npc != null)
				npc.UpdatePetWindow();
		}
	}

	public virtual T GetOfType<T>() where T : IGameEffect
	{
		if (m_effects == null)
			return default(T);

		lock (m_lockObject)
		{
			return (T)m_effects.FirstOrDefault(effect => effect.GetType().Equals(typeof(T)));
		}
	}

	public virtual ICollection<T> GetAllOfType<T>() where T : IGameEffect
	{
		if (m_effects == null)
			return Array.Empty<T>();

		lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
		{
			return m_effects.Where(effect => effect.GetType().Equals(typeof(T))).Cast<T>().ToArray();
		}
	}

	public int CountOfType<T>() where T : IGameEffect
	{
		if (m_effects == null)
			return 0;

		lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
		{
			return m_effects.Count(effect => effect.GetType().Equals(typeof(T)));
		}
	}

	public int CountOfType(params Type[] types)
	{
		if (m_effects == null)
			return 0;

		lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
		{
			return m_effects.Join(types, e => e.GetType(), t => t, (e, t) => e).Count();
		}
	}

	/// <summary>
	/// Find the first occurence of an effect with given type
	/// </summary>
	/// <param name="effectType"></param>
	/// <returns>effect or null</returns>
	public virtual IGameEffect GetOfType(Type effectType)
	{
		if (m_effects == null)
			return null;
		
		lock (m_lockObject)
		{
			return m_effects.FirstOrDefault(effect => effect.GetType().Equals(effectType));
		}
	}

	/// <summary>
	/// Find effects of specific type
	/// </summary>
	/// <param name="effectType"></param>
	/// <returns>resulting effectlist</returns>
	public virtual ICollection<IGameEffect> GetAllOfType(Type effectType)
	{
		if (m_effects == null)
			return Array.Empty<IGameEffect>();

		lock (m_lockObject) // Mannen 10:56 PM 10/30/2006 - Fixing every lock ('this')
		{
			return m_effects.Where(effect => effect.GetType().Equals(effectType)).ToArray();
		}
	}

	/// <summary>
	/// Gets count of all stored effects
	/// </summary>
	public int Count
	{
		get
		{
			if (m_effects == null)
				return 0;
			
			lock (m_lockObject)
			{
				return m_effects.Count;
			}
		}
	}

	#region IEnumerable Member

	/// <summary>
	/// Returns an enumerator for the effects
	/// </summary>
	/// <returns></returns>
	public IEnumerator<IGameEffect> GetEnumerator()
	{
		if (m_effects == null)
			return Array.Empty<IGameEffect>().AsEnumerable().GetEnumerator();
		
		lock (m_lockObject)
		{
			return m_effects.ToArray().AsEnumerable().GetEnumerator();
		}
	}

	/// <summary>
	/// Returns an enumerator for the effects
	/// </summary>
	/// <returns></returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	#endregion
	
}