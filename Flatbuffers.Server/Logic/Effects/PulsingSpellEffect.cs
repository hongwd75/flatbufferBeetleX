using System.Text;
using Game.Logic.Effects;
using Game.Logic.Spells;

namespace Game.Logic.Effects;

public sealed class PulsingSpellEffect : IConcentrationEffect
{
	private readonly object m_LockObject = new object();
	private readonly ISpellHandler m_spellHandler = null;
	private SpellPulseAction m_spellPulseAction;

	public PulsingSpellEffect(ISpellHandler spellHandler)
	{
		if (spellHandler == null)
			throw new ArgumentNullException("spellHandler");
		m_spellHandler = spellHandler;
	}

	public override string ToString()
	{
		return new StringBuilder(64)
			.Append("Name=").Append(Name)
			.Append(", OwnerName=").Append(OwnerName)
			.Append(", Icon=").Append(Icon)
			.Append("\nSpellHandler info: ").Append(SpellHandler.ToString())
			.ToString();
	}
	
	public void Start()
	{
		lock (m_LockObject)
		{
			if (m_spellPulseAction != null)
				m_spellPulseAction.Stop();
			m_spellPulseAction = new SpellPulseAction(m_spellHandler.Caster, this);
			m_spellPulseAction.Interval = m_spellHandler.Spell.Frequency;
			m_spellPulseAction.Start(m_spellHandler.Spell.Frequency);
			m_spellHandler.Caster.ConcentrationEffects.Add(this);
		}
	}
	
	public void Cancel(bool playerCanceled)
	{
		lock (m_LockObject)
		{
			if (m_spellPulseAction != null)
			{
				m_spellPulseAction.Stop();
				m_spellPulseAction = null;
			}
			m_spellHandler.Caster.ConcentrationEffects.Remove(this);
		}
	}

	public string Name
	{
		get { return m_spellHandler.Spell.Name; }
	}

	public string OwnerName
	{
		get { return "Pulse: " + m_spellHandler.Spell.Target; }
	}

	public ushort Icon
	{
		get { return m_spellHandler.Spell.Icon; }
	}

	public byte Concentration
	{
		get { return m_spellHandler.Spell.Concentration; }
	}

	public ISpellHandler SpellHandler
	{
		get { return m_spellHandler; }
	}

	private sealed class SpellPulseAction : RegionAction
	{
		private readonly PulsingSpellEffect m_effect;

		public SpellPulseAction(GameObject actionSource, PulsingSpellEffect effect) : base(actionSource)
		{
			if (effect == null)
				throw new ArgumentNullException("effect");
			m_effect = effect;
		}

		protected override void OnTick()
		{
			PulsingSpellEffect effect = m_effect;
			effect.m_spellHandler.OnSpellPulse(effect);
		}
	}
}