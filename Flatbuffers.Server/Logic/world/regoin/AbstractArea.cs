﻿using System.Security.Policy;
using Game.Logic.Utils;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic;
using Game.Logic.datatable;
using Logic.database.table;

namespace Game.Logic.World;

public abstract class AbstractArea : IArea, ITranslatableObject
	{
		protected DBArea m_dbArea = null;
		protected bool m_canBroadcast = false;
		/// <summary>
		/// Variable holding whether or not players can broadcast in this area
		/// </summary>
		public bool CanBroadcast
		{
			get { return m_canBroadcast; }
			set { m_canBroadcast = value; }
		}

		protected bool m_checkLOS = false;
		/// <summary>
		/// Variable holding whether or not to check for LOS for spells in this area
		/// </summary>
		public bool CheckLOS
		{
			get { return m_checkLOS; }
			set { m_checkLOS = value; }
		}

		protected bool m_displayMessage = true;
		/// <summary>
		/// Display entered message
		/// </summary>
		public virtual bool DisplayMessage
		{
			get { return m_displayMessage; }
			set { m_displayMessage = value; }
		}

		protected bool m_safeArea = false;
		/// <summary>
		/// Can players be attacked by other players in this area
		/// </summary>
		public virtual bool IsSafeArea
		{
			get { return m_safeArea; }
			set { m_safeArea = value; }
		}

		/// <summary>
		/// Constant holding max number of areas per zone, increase if more ares are needed,
		/// this will slightly increase memory usage on server
		/// </summary>
		public const ushort MAX_AREAS_PER_ZONE = 50;

		/// <summary>
		/// The ID of the Area eg. 15 ( == index in Region.m_areas array)
		/// </summary>
		protected ushort m_ID;

        /// <summary>
        /// Holds the translation id
        /// </summary>
        protected string m_translationId;

		/// <summary>
		/// The description of the Area eg. "Camelot Hills"
		/// </summary>
		protected string m_Description;

		/// <summary>
		/// The area sound to play on enter/leave events
		/// </summary>
		protected byte m_sound;

		/// <summary>
		/// Constructs a new AbstractArea
		/// </summary>
		/// <param name="desc"></param>
		public AbstractArea(string desc)
		{
			m_Description = desc;
		}

		public AbstractArea()
			: base()
		{
		}

		/// <summary>
		/// Returns the ID of this Area
		/// </summary>
		public ushort ID
		{
			get { return m_ID; }
			set { m_ID = value; }
		}

        public virtual LanguageDataObject.eTranslationIdentifier TranslationIdentifier
        {
            get { return LanguageDataObject.eTranslationIdentifier.eArea; }
        }

        /// <summary>
        /// Gets or sets the translation id
        /// </summary>
        public string TranslationId
        {
            get { return m_translationId; }
            set { m_translationId = (value == null ? "" : value); }
        }

		/// <summary>
		/// Return the description of this Area
		/// </summary>
		public string Description
		{
			get { return m_Description; }
		}

		/// <summary>
		/// Gets or sets the area sound
		/// </summary>
		public byte Sound
		{
			get { return m_sound; }
			set { m_sound = value; }
		}

		#region Event handling

		public void UnRegisterPlayerEnter(GameEventHandler callback)
		{
			GameEventManager.RemoveHandler(this, AreaEvent.PlayerEnter, callback);
		}

		public void UnRegisterPlayerLeave(GameEventHandler callback)
		{
			GameEventManager.RemoveHandler(this, AreaEvent.PlayerLeave, callback);
		}

		public void RegisterPlayerEnter(GameEventHandler callback)
		{
			GameEventManager.AddHandler(this, AreaEvent.PlayerEnter, callback);
		}

		public void RegisterPlayerLeave(GameEventHandler callback)
		{
			GameEventManager.AddHandler(this, AreaEvent.PlayerLeave, callback);
		}
		#endregion

		/// <summary>
		/// Checks wether area intersects with given zone
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		public abstract bool IsIntersectingZone(Zone zone);

        public abstract bool IsContaining(Coordinate spot, bool ignoreZ = false);

        [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
		public virtual bool IsContaining(IPoint3D spot)
            => IsContaining(spot.ToCoordinate(), ignoreZ: false);

        [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
		public virtual bool IsContaining(IPoint3D spot, bool checkZ)
            => IsContaining(spot.ToCoordinate(), ignoreZ: !checkZ);

        [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
		public virtual bool IsContaining(int x, int y, int z)
            => IsContaining(Coordinate.Create(x, y, z), ignoreZ: false);

        [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
		public virtual bool IsContaining(int x, int y, int z, bool checkZ)
            => IsContaining(Coordinate.Create(x, y, z), ignoreZ: !checkZ);

		/// <summary>
		/// Called whenever a player leaves the given area
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerLeave(GamePlayer player)
		{
            // if (m_displayMessage && Description != null && Description != "")
            //     player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractArea.Left", Description),
            //         eChatType.CT_System, eChatLoc.CL_SystemWindow);

			player.Notify(AreaEvent.PlayerLeave, this, new AreaEventArgs(this, player));
		}

		/// <summary>
		/// Called whenever a player enters the given area
		/// </summary>
		/// <param name="player"></param>
		public virtual void OnPlayerEnter(GamePlayer player)
		{
			// if (m_displayMessage && Description != null && Description != "")
			// {
   //              string description = Description;
   //              string screenDescription = description;
   //
   //              var translation = LanguageMgr.GetTranslation(player, this) as DBLanguageArea;
   //              if (translation != null)
   //              {
   //                  if (!Util.IsEmpty(translation.Description))
   //                      description = translation.Description;
   //
   //                  if (!Util.IsEmpty(translation.ScreenDescription))
   //                      screenDescription = translation.ScreenDescription;
   //              }
   //
   //              player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "AbstractArea.Entered", description),
   //                  eChatType.CT_System, eChatLoc.CL_SystemWindow);
			// }
			// if (Sound != 0)
			// {
			// 	player.Out.SendRegionEnterSound(Sound);
			// }
			player.Notify(AreaEvent.PlayerEnter, this, new AreaEventArgs(this, player));
		}

		public abstract void LoadFromDatabase(DBArea area);
	}