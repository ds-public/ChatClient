using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerHelper
{
	public enum ConnectServerResult
	{
		Unknown,
		Success,
		Failed,
		LimitOver,
		TimeOver,
		LobbyJoinFailed,
		AlreadyConnected,
	}

	public enum DisconnectServerResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyDisconnected,
	}

	public enum JoinLobbyResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyJoined,
	}

	public enum LeaveLobbyResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyLeft,
	}

	public enum CreateRoomResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyJoined,
		AlreadyCreated,
		NotLobbyJoined,
	}

	public enum JoinRoomResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyJoined,
		NotLobbyJoined,
		Closing,
		PasswordDenied,
	}

	public enum JoinOrCreateRoomResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyJoined,
		NotLobbyJoined,
		Closing,
		PasswordDenied,
	}

	public enum LeaveRoomResult
	{
		Unknown,
		Success,
		Failed,
		AlreadyLeft,
	}


}

