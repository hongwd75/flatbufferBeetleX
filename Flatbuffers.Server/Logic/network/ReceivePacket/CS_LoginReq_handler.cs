// ** CS_LoginReq 패킷 메시지

using System.Reflection;
using System.Threading.Tasks;
using BeetleX;
using Flatbuffers.Messages.Enums;
using Game.Logic;
using Game.Logic.network;
using Game.Logic.ServerProperties;
using Game.Logic.World;
using Google.FlatBuffers;
using log4net;
using Logic.database;
using Logic.database.table;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ClientPacketMessageAttribute(ClientPackets.CS_LoginReq)]
	public class CS_LoginReq_handler : IServerPacketMessage
	{
		private static readonly Dictionary<string, LockCount> m_locks = new Dictionary<string, LockCount>();
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		#pragma warning disable CS1998
		public async Task Packet(ISession session,ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			CS_LoginReq_FBS packet = CS_LoginReq.GetRootAsCS_LoginReq(byteBuffer).UnPack();

			if (GameServer.Instance.ServerStatus == eGameServerStatus.GSS_Closed)
			{
				GameServer.Instance.NetworkHandler.Disconnect(session,false);
				return;
			}

			string userName = packet.Id;
			string password = packet.Pwd;
			EnterLock(userName);
			try
			{
				OutPacket output = (OutPacket)session.SocketProcessHandler;
				bool checkAccount = false;
				var client = GameServer.Instance.Clients.GetClientByAccountName(userName, true);
				if (client != null)
				{
					bool disconnect = false;
					lock (client)
					{
						if (client.IsConnected() == false)
						{
							if (client.ClientState != GameClient.eClientState.Playing)
							{
								// 게임하고 있지 않는 상태면 접속 종료 시키고 새로 접속 시도.
								client.ClientState = GameClient.eClientState.Connecting;
							}
						}
						else
						{
							// 중복 로그인 패킷은 무시
							if (client.ClientState == GameClient.eClientState.Connecting)
							{
								return;
							} else
							if (client.ClientState == GameClient.eClientState.Playing)
							{
								disconnect = true;
								// 듀얼 접속
								output?.SendLoginDenied(eLoginError.AccountAlreadyLoggedIn);
							}
						}
					}

					if (disconnect == true)
					{
						GameServer.Instance.NetworkHandler.Disconnect(session,false);
						return;
					}
				}
				
				// 로그인 시작
				eLoginError error = eLoginError.AccountInvalid;
				Account playerAccount = GameServer.Database.FindObjectByKey<Account>(userName);
				if (playerAccount != null)
				{
					if (password.CompareTo(playerAccount.Password) == 0)
					{
						error = eLoginError.none;
						playerAccount.LastLogin = DateTime.Now;
						GameServer.Database.SaveObject(playerAccount);
					}
					else
					{
						error = eLoginError.WrongPassword;
					}
				}
				else
				{
					if (GameServer.Instance.Configuration.AutoAccountCreation && Properties.ALLOW_AUTO_ACCOUNT_CREATION)
					{
						if (string.IsNullOrEmpty(password) == false)
						{
							error = eLoginError.none;
							playerAccount = new Account();
							playerAccount.Name = userName;
							playerAccount.Password = password;
							playerAccount.Realm = 0;
							playerAccount.CreationDate = DateTime.Now;
							playerAccount.LastLogin = DateTime.Now;
							GameServer.Database.AddObject(playerAccount);
						}
					}
					else
					{
						error = eLoginError.AccountNotFound;	
					}
				}

				if (error != eLoginError.none)
				{
					output?.SendLoginDenied(error);
					GameServer.Instance.NetworkHandler.Disconnect(session,false);
				}
				else
				{
					// 로그인 완료 메시지 전송
					if (client == null)
					{
						client = GameServer.Instance.Clients.CreateAccount(playerAccount, session);
						if (client == null)
						{
							output?.SendLoginDenied(eLoginError.TooManyPlayersLoggedIn);
							GameServer.Instance.NetworkHandler.Disconnect(session,false);
							return;
						}
						else
						{
							output.Client = client;
						}
					}
					else
					{
						client.Session = session;
						client.Out.Client = client;
						client.Account = playerAccount;						
					}
					client.PingTime = DateTime.Now.Ticks;
					client.Out?.SendLoginInfo();
				}
			}
			catch (DatabaseException e)
			{
				GameServer.Instance.NetworkHandler.Disconnect(session,true);
			}			
			catch (Exception e)
			{
				GameServer.Instance.NetworkHandler.Disconnect(session,true);
			}
			finally
			{
				ExitLock(userName);
			}
		}

		private string cryptPassword(string pwd)
		{
			return pwd;
		}
		
		/// <summary>
		/// Acquires the lock on account.
		/// </summary>
		/// <param name="accountName">Name of the account.</param>
		private void EnterLock(string accountName)
		{
			// Safety check
			if (accountName == null)
			{
				accountName = string.Empty;
			}

			LockCount lockObj = null;
			lock (m_locks)
			{
				// Get/create lock object
				if (!m_locks.TryGetValue(accountName, out lockObj))
				{
					lockObj = new LockCount();
					m_locks.Add(accountName, lockObj);
				}

				if (lockObj != null)
				{
					lockObj.count++;
				}
			}

			if (lockObj != null)
			{
				Monitor.Enter(lockObj);
			}
		}

		private void ExitLock(string accountName)
		{
			// Safety check
			if (accountName == null)
			{
				accountName = string.Empty;
			}

			LockCount lockObj = null;
			lock (m_locks)
			{
				// Get lock object
				if (!m_locks.TryGetValue(accountName, out lockObj))
				{
					Log.Error("(Exit) No lock object for account: '" + accountName + "'");
				}

				// Remove lock object if no more locks on it
				if (lockObj != null)
				{
					if (--lockObj.count <= 0)
					{
						m_locks.Remove(accountName);
					}
				}
			}

			Monitor.Exit(lockObj);
		}
		
		#region ####### Lock Object #############################################################################
		private class LockCount
		{
			public int count;
		}
		#endregion
	}
}
